-- lua_pcg: Pure-Lua PCG32 (XSH-RR) implementation for Lönn
-- Port of https://github.com/luau-project/lua-pcg
-- Implements PCG-XSH-RR (32-bit output, 64-bit state) from https://www.pcg-random.org/
--
-- No FFI / no C dependencies — uses 16-bit limb arithmetic for 64-bit math
-- so it works in any Lua 5.1 / LuaJIT environment including Lönn's sandbox.
--
-- API matches lua-pcg's pcg32 class:
--   local pcg = require("mods").requireFromPlugin("libraries.lua_pcg")
--   local rng = pcg.pcg32.new([initstate_lo, initstate_hi, initseq_lo, initseq_hi])
--   local n   = rng:next([a [, b]])
--   rng:nextFloat()      -- [0, 1)
--   rng:nextBytes()      -- {b1, b2, b3, b4}
--   rng:close()

local bit  = require("bit")
local band, bor, bxor, rshift, lshift, ror =
    bit.band, bit.bor, bit.bxor, bit.rshift, bit.lshift, bit.ror

--------------------------------------------------------------------------------
-- 64-bit unsigned integer as {lo, hi} (each 0 .. 0xFFFFFFFF)
-- All arithmetic mod 2^64.
--------------------------------------------------------------------------------
local M = 0x100000000   -- 2^32
local H = 0x10000       -- 2^16

local function u32(n)
    n = n % M
    if n < 0 then n = n + M end
    return n
end

local function ZERO() return {0, 0} end
local function ONE()  return {1, 0} end
local function U64(lo, hi) return {u32(lo), u32(hi)} end

local function add64(a, b)
    local lo = a[1] + b[1]
    local carry = 0
    if lo >= M then lo = lo - M; carry = 1 end
    local hi = a[2] + b[2] + carry
    if hi >= M then hi = hi - M end
    return {lo, hi}
end

local function xor64(a, b)
    return {u32(bxor(a[1], b[1])), u32(bxor(a[2], b[2]))}
end

local function rshift64(a, n)
    if n == 0  then return {a[1], a[2]} end
    if n >= 64 then return ZERO() end
    if n >= 32 then
        return {u32(rshift(a[2], n - 32)), 0}
    end
    local lo = u32(bor(rshift(a[1], n), lshift(a[2], 32 - n)))
    local hi = u32(rshift(a[2], n))
    return {lo, hi}
end

local function mul64(a, b)
    local a0 = a[1] % H
    local a1 = math.floor(a[1] / H) % H
    local a2 = a[2] % H
    local a3 = math.floor(a[2] / H) % H

    local b0 = b[1] % H
    local b1 = math.floor(b[1] / H) % H
    local b2 = b[2] % H
    local b3 = math.floor(b[2] / H) % H

    local t0 = a0 * b0
    local t1 = a0 * b1 + a1 * b0
    local t2 = a0 * b2 + a1 * b1 + a2 * b0
    local t3 = a0 * b3 + a1 * b2 + a2 * b1 + a3 * b0

    local r0 = t0 % H
    t1 = t1 + math.floor(t0 / H)
    local r1 = t1 % H
    t2 = t2 + math.floor(t1 / H)
    local r2 = t2 % H
    t3 = t3 + math.floor(t2 / H)
    local r3 = t3 % H

    return {r0 + r1 * H, r2 + r3 * H}
end

local function ror32(v, r)
    r = r % 32
    if r == 0 then return v end
    return u32(ror(v, r))
end

--------------------------------------------------------------------------------
-- PCG32 multiplier: 6364136223846793005 = 0x5851F42D 4C957F2D
--------------------------------------------------------------------------------
local PCG_MULT = U64(0x4C957F2D, 0x5851F42D)

--------------------------------------------------------------------------------
-- pcg32 class
--------------------------------------------------------------------------------
local pcg32 = {}
pcg32.__index = pcg32

function pcg32.new(initstate_lo, initstate_hi, initseq_lo, initseq_hi)
    local self = setmetatable({}, pcg32)
    self._state = ZERO()
    self._inc   = ZERO()
    self:seed(initstate_lo, initstate_hi, initseq_lo, initseq_hi)
    return self
end

function pcg32:seed(initstate_lo, initstate_hi, initseq_lo, initseq_hi)
    local initstate, initseq

    if initstate_lo == nil then
        local t = os.time()
        local c = math.floor(os.clock() * 1e6)
        local addr = tonumber(tostring({}):match("0x(%x+)"), 16) or 0
        initstate = U64(u32(t * 1000000 + c), u32(addr))
    else
        initstate = U64(initstate_lo or 0, initstate_hi or 0)
    end

    if initseq_lo == nil and initstate_hi == nil then
        initseq = U64(54, 0)
    else
        initseq = U64(initseq_lo or 54, initseq_hi or 0)
    end

    self._state = ZERO()
    self._inc   = add64(mul64(initseq, U64(2, 0)), ONE())
    self:_step()
    self._state = add64(self._state, initstate)
    self:_step()
end

function pcg32:_step()
    self._state = add64(mul64(self._state, PCG_MULT), self._inc)
end

function pcg32:next(a, b)
    local old = {self._state[1], self._state[2]}
    self:_step()

    local shifted18 = rshift64(old, 18)
    local xored     = xor64(shifted18, old)
    local shifted27 = rshift64(xored, 27)
    local xorshifted = shifted27[1]

    local rot_n = rshift64(old, 59)
    local rot_v = rot_n[1]

    local result = ror32(xorshifted, rot_v)

    if a and b then
        local range = b - a
        if range <= 0 then error("pcg32:next(a, b) requires a < b") end
        return a + (result % range)
    elseif a then
        if a <= 0 then error("pcg32:next(a) requires a > 0") end
        return result % a
    end
    return result
end

function pcg32:nextFloat()
    return self:next() / 4294967296
end

function pcg32:nextBytes()
    local n = self:next()
    return {
        n % 256,
        math.floor(n / 256) % 256,
        math.floor(n / 65536) % 256,
        math.floor(n / 16777216) % 256,
    }
end

function pcg32:advance(delta_lo, delta_hi)
    local delta = U64(delta_lo or 0, delta_hi or 0)
    local cur_mult = {PCG_MULT[1], PCG_MULT[2]}
    local cur_plus = {self._inc[1], self._inc[2]}
    local acc_mult = ONE()
    local acc_plus = ZERO()

    while delta[1] ~= 0 or delta[2] ~= 0 do
        if delta[1] % 2 == 1 then
            acc_mult = mul64(acc_mult, cur_mult)
            acc_plus = add64(mul64(acc_plus, cur_mult), cur_plus)
        end
        cur_plus = mul64(add64(cur_mult, ONE()), cur_plus)
        cur_mult = mul64(cur_mult, cur_mult)
        delta = rshift64(delta, 1)
    end

    self._state = add64(mul64(acc_mult, self._state), acc_plus)
end

function pcg32:close()
    self._state = ZERO()
    self._inc   = ZERO()
end

--------------------------------------------------------------------------------
-- Module table
--------------------------------------------------------------------------------
local lua_pcg = {
    version          = "0.1.0-pure-lua",
    emulation64bit   = true,
    emulation128bit  = true,
    has32bitinteger  = true,
    has64bitinteger  = false,
    pcg32            = pcg32,
    }

return lua_pcg
