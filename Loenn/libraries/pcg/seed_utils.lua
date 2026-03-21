-- PCG Seed Utilities
-- Provides auto-generated unique seeds for PCG scripts.
-- Each call to generateSeed() returns a new unique seed, even when called
-- rapidly within the same second (uses os.clock() sub-second precision + counter).

local seedUtils = {}

-- Internal counter to guarantee uniqueness across rapid calls
local _counter = 0

--- Generate a unique random seed.
-- Combines os.time() (seconds since epoch), os.clock() (CPU time with
-- sub-second precision), and an incrementing counter to ensure every call
-- returns a distinct value.
-- @return integer  A positive integer seed suitable for math.randomseed()
function seedUtils.generateSeed()
    _counter = _counter + 1
    -- os.time() gives seconds, os.clock() gives fractional CPU seconds
    -- Multiply clock by 10000 for 0.1ms precision, add counter for uniqueness
    local timePart = os.time() % 1000000  -- keep it in a reasonable range
    local clockPart = math.floor(os.clock() * 10000) % 10000
    local seed = timePart * 10000 + clockPart + _counter
    return seed
end

--- Resolve the seed from script args.
-- If autoSeed is true or seed is 0, generates a new unique seed.
-- Otherwise returns the user-specified seed.
-- Prints the resolved seed to the Lönn console for reproducibility.
-- @param args  Table with .seed (integer) and .autoSeed (boolean) fields
-- @return integer  The resolved seed value
function seedUtils.resolveSeed(args)
    local seed = args.seed or 0
    local auto = args.autoSeed

    -- autoSeed=true overrides any seed value; seed=0 also means auto
    if auto or seed == 0 then
        seed = seedUtils.generateSeed()
    end

    print(string.format("[PCG] Seed: %d%s", seed, (auto or (args.seed or 0) == 0) and " (auto)" or ""))
    return seed
end

--- Compute the next available entity _id for the given map room list.
-- Scans all entities and triggers across all rooms and returns 1 + max found.
-- This ensures generated entities don't share IDs with existing ones, which
-- would crash Lönn when clicking the generated rooms.
-- @param rooms  Array of Lönn room tables (state.map.rooms)
-- @return integer  Next safe entity ID to use (monotonically increasing)
function seedUtils.nextEntityId(rooms)
    local maxId = 0
    for _, room in ipairs(rooms or {}) do
        for _, entity in ipairs(room.entities or {}) do
            local eid = tonumber(entity._id) or 0
            if eid > maxId then maxId = eid end
        end
        for _, trigger in ipairs(room.triggers or {}) do
            local tid = tonumber(trigger._id) or 0
            if tid > maxId then maxId = tid end
        end
    end
    return maxId + 1
end

return seedUtils
