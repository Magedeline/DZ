-- Celeste map.bin Binary Encoder
-- Serializes a Lua element tree into the Celeste binary map format.
-- Based on the format used by Lönn/Maple/Everest.
--
-- Usage:
--   local binEncoder = require("libraries.pcg.bin_encoder")
--   binEncoder.encodeFile("path/to/map.bin", mapData)

local binEncoder = {}

--------------------------------------------------------------------------------
-- Binary Writer
--------------------------------------------------------------------------------

local function createWriter()
    local parts = {}
    local writer = {}

    function writer:writeByte(v)
        v = math.floor(v) % 256
        table.insert(parts, string.char(v))
    end

    function writer:writeBool(v)
        self:writeByte(v and 1 or 0)
    end

    function writer:writeUShort(v)
        -- uint16 little-endian
        v = math.floor(v) % 65536
        table.insert(parts, string.char(v % 256, math.floor(v / 256) % 256))
    end

    function writer:writeSignedShort(v)
        -- int16 little-endian (two's complement)
        v = math.floor(v)
        if v < 0 then v = v + 65536 end
        self:writeUShort(v)
    end

    function writer:writeSignedLong(v)
        -- int32 little-endian (two's complement)
        v = math.floor(v)
        if v < 0 then v = v + 4294967296 end
        local b0 = v % 256
        local b1 = math.floor(v / 256) % 256
        local b2 = math.floor(v / 65536) % 256
        local b3 = math.floor(v / 16777216) % 256
        table.insert(parts, string.char(b0, b1, b2, b3))
    end

    function writer:writeFloat(v)
        -- IEEE 754 single-precision float, little-endian
        -- Using string.pack if available (Lua 5.3+), otherwise manual
        if string.pack then
            table.insert(parts, string.pack("<f", v))
        else
            -- Manual IEEE 754 encoding
            local sign = 0
            if v < 0 then sign = 1; v = -v end
            local mantissa, exponent
            if v == 0 then
                mantissa = 0
                exponent = 0
            elseif v == math.huge then
                mantissa = 0
                exponent = 255
            elseif v ~= v then -- NaN
                mantissa = 1
                exponent = 255
            else
                exponent = math.floor(math.log(v) / math.log(2))
                mantissa = (v / (2 ^ exponent) - 1) * (2 ^ 23)
                exponent = exponent + 127
                if exponent <= 0 then
                    mantissa = math.floor(v / (2 ^ (-126)) * (2 ^ 23))
                    exponent = 0
                elseif exponent >= 255 then
                    mantissa = 0
                    exponent = 255
                else
                    mantissa = math.floor(mantissa + 0.5)
                end
            end
            mantissa = math.floor(mantissa)
            local b0 = mantissa % 256
            local b1 = math.floor(mantissa / 256) % 256
            local b2 = math.floor(mantissa / 65536) % 128 + (exponent % 2) * 128
            local b3 = math.floor(exponent / 2) + sign * 128
            table.insert(parts, string.char(b0, b1, b2, b3))
        end
    end

    function writer:writeVariableLength(length)
        length = math.floor(length)
        while length > 127 do
            self:writeByte(length % 128 + 128)
            length = math.floor(length / 128)
        end
        self:writeByte(length)
    end

    function writer:writeString(s)
        self:writeVariableLength(#s)
        table.insert(parts, s)
    end

    function writer:writeRaw(s)
        table.insert(parts, s)
    end

    function writer:getString()
        return table.concat(parts)
    end

    return writer
end

--------------------------------------------------------------------------------
-- Run-Length Encoding
--------------------------------------------------------------------------------

function binEncoder.encodeRunLength(str)
    if #str == 0 then return "" end
    local res = {}
    local count = 1
    local current = str:sub(1, 1)
    for i = 2, #str do
        local ch = str:sub(i, i)
        if ch ~= current or count == 255 then
            table.insert(res, string.char(count))
            table.insert(res, current)
            count = 1
            current = ch
        else
            count = count + 1
        end
    end
    table.insert(res, string.char(count))
    table.insert(res, current)
    return table.concat(res)
end

--------------------------------------------------------------------------------
-- String Lookup Collection
--------------------------------------------------------------------------------

local function countStrings(data, seen)
    seen = seen or {}
    local name = data.__name or ""
    seen[name] = (seen[name] or 0) + 1

    for k, v in pairs(data) do
        if type(k) == "string" and k ~= "__name" and k ~= "__children" then
            seen[k] = (seen[k] or 0) + 1
            -- String values go into lookup (except innerText values)
            if type(v) == "string" and k ~= "innerText" then
                seen[v] = (seen[v] or 0) + 1
            end
        end
    end

    if data.__children then
        for _, child in ipairs(data.__children) do
            countStrings(child, seen)
        end
    end

    return seen
end

--------------------------------------------------------------------------------
-- Value Encoding
--------------------------------------------------------------------------------

local function encodeValue(writer, key, value, lookup)
    if type(value) == "boolean" then
        writer:writeByte(0)
        writer:writeBool(value)

    elseif type(value) == "number" then
        local isFloat = (value ~= math.floor(value)) or (math.abs(value) == math.huge) or (value ~= value)
        if isFloat then
            writer:writeByte(4)  -- float32
            writer:writeFloat(value)
        elseif value >= 0 and value <= 255 then
            writer:writeByte(1)  -- uint8
            writer:writeByte(value)
        elseif value >= -32768 and value <= 32767 then
            writer:writeByte(2)  -- int16
            writer:writeSignedShort(value)
        elseif value >= -2147483648 and value <= 2147483647 then
            writer:writeByte(3)  -- int32
            writer:writeSignedLong(value)
        else
            writer:writeByte(4)  -- float fallback
            writer:writeFloat(value)
        end

    elseif type(value) == "string" then
        if key == "innerText" then
            -- innerText is NOT in lookup — use raw or RLE
            local rle = binEncoder.encodeRunLength(value)
            if #rle < #value and #rle < 32767 then
                writer:writeByte(7)  -- RLE string
                writer:writeSignedShort(#rle)
                writer:writeRaw(rle)
            else
                writer:writeByte(6)  -- raw string
                writer:writeString(value)
            end
        else
            local idx = lookup[value]
            if idx then
                writer:writeByte(5)  -- lookup ref
                writer:writeSignedShort(idx - 1)  -- 0-based
            else
                writer:writeByte(6)  -- raw string
                writer:writeString(value)
            end
        end
    else
        -- Unknown type → convert to string
        writer:writeByte(6)
        writer:writeString(tostring(value))
    end
end

--------------------------------------------------------------------------------
-- Element Encoding
--------------------------------------------------------------------------------

local function encodeElement(writer, data, lookup)
    -- Write element name (lookup index, 0-based)
    local nameIdx = lookup[data.__name or ""] or 1
    writer:writeUShort(nameIdx - 1)

    -- Collect attributes (excluding __name, __children)
    local attrs = {}
    for k, v in pairs(data) do
        if type(k) == "string" and k ~= "__name" and k ~= "__children" then
            table.insert(attrs, { key = k, value = v })
        end
    end

    -- Write attribute count
    writer:writeByte(#attrs)

    -- Write each attribute
    for _, attr in ipairs(attrs) do
        local keyIdx = lookup[attr.key] or 1
        writer:writeUShort(keyIdx - 1)  -- 0-based
        encodeValue(writer, attr.key, attr.value, lookup)
    end

    -- Write children
    local children = data.__children or {}
    writer:writeUShort(#children)
    for _, child in ipairs(children) do
        encodeElement(writer, child, lookup)
    end
end

--------------------------------------------------------------------------------
-- Public API
--------------------------------------------------------------------------------

--- Encode a map data table into binary and save to file.
-- @param path     Output file path (.bin)
-- @param data     Map element tree
-- @param header   Header string (default "CELESTE MAP")
function binEncoder.encodeFile(path, data, header)
    header = header or "CELESTE MAP"

    local writer = createWriter()

    -- 1. Collect lookup strings
    local seen = countStrings(data)
    local lookupStrings = {}
    local lookup = {}
    for s, _ in pairs(seen) do
        table.insert(lookupStrings, s)
    end
    table.sort(lookupStrings)  -- deterministic order

    -- 2. Write header
    writer:writeString(header)

    -- 3. Write package name
    writer:writeString(data._package or "")

    -- 4. Write lookup table
    writer:writeUShort(#lookupStrings)
    for i, s in ipairs(lookupStrings) do
        writer:writeString(s)
        lookup[s] = i  -- 1-based
    end

    -- 5. Encode root element tree
    encodeElement(writer, data, lookup)

    -- 6. Write to file
    local fh = io.open(path, "wb")
    if not fh then
        return false, "Could not open file for writing: " .. path
    end
    fh:write(writer:getString())
    fh:close()

    return true
end

--- Encode to a binary string (instead of file).
-- @param data    Map element tree
-- @param header  Header string (default "CELESTE MAP")
-- @return string Binary data
function binEncoder.encodeToString(data, header)
    header = header or "CELESTE MAP"

    local writer = createWriter()

    local seen = countStrings(data)
    local lookupStrings = {}
    local lookup = {}
    for s, _ in pairs(seen) do
        table.insert(lookupStrings, s)
    end
    table.sort(lookupStrings)

    writer:writeString(header)
    writer:writeString(data._package or "")

    writer:writeUShort(#lookupStrings)
    for i, s in ipairs(lookupStrings) do
        writer:writeString(s)
        lookup[s] = i
    end

    encodeElement(writer, data, lookup)

    return writer:getString()
end

return binEncoder
