#!/usr/bin/env lua
--[[
================================================================================
  MaggyHelper PCG Pipeline — Command-Line Interface
================================================================================

  End-to-end Procedural Content Generation pipeline for Celeste levels.

  The pipeline performs:
    1. Loads a training dataset (map .bin or built-in rooms)
    2. Builds a Dictionary of Probability Transitions (DPT) via n-gram extraction
    3. Generates a level skeleton (room layout & connectivity)
    4. Populates rooms using a Multidimensional Markov Chain (MdMC) model
    5. Applies post-processing for playability (Celeste-A* pathfinder)
    6. Resets problematic skeletons when --reset-skeleton is enabled
    7. Encodes the level into a playable .bin file via the Room Encoder

  Usage:
    lua pcg_cli.lua [OPTIONS]

  Required:
    (none — the CLI uses sensible defaults for everything)

  Options:
    -c,   --config STR          MdMC configuration matrix (default "000011012")
    -td,  --training-dataset STR  Path to a Celeste .bin map for training data,
                                  or "builtin" (default "builtin")
    -nr,  --nb-rooms INT        Number of rooms to generate (default 8)
    -p,   --proba FLOAT         Probability-p2 for labyrinth-style levels.
                                  0 = pathway, 1 = random room order (default 0.3)
    -rs,  --room-size W H       Room dimensions width height in tiles (default 40 23)
    -btd, --bt-depth INT        Max backtracking depth for MdMC generation (default 3)
    -tl,  --tries-limit INT     Max attempts for room generation (default 15)
    -r,   --reset-skeleton      Regenerate skeleton if room generation fails
    -s,   --seed INT            Random seed (0 = auto) (default 0)
    -o,   --output STR          Output .bin path (default Maps/Maggy/PCG/pcg_generated.bin)
          --preset STR          Use a named preset (overrides config/room-size/bt-depth)
          --list-presets         List available presets and exit
          --dry-run              Run pipeline but do not write .bin file
    -v,   --verbose             Print detailed progress
    -q,   --quiet               Suppress output except errors
    -h,   --help                Show this help message and exit

  Examples:
    lua pcg_cli.lua
    lua pcg_cli.lua --nb-rooms 12 --seed 42
    lua pcg_cli.lua --config 000111012 --room-size 48 27 --bt-depth 4
    lua pcg_cli.lua --training-dataset ../Maps/Maggy/ASide/01_Ruins.bin
    lua pcg_cli.lua --preset resort --nb-rooms 10 --proba 0.5
    lua pcg_cli.lua --list-presets
    lua pcg_cli.lua --reset-skeleton --tries-limit 30 --verbose

================================================================================
]]

--------------------------------------------------------------------------------
-- 0. Resolve script directory & module paths
--------------------------------------------------------------------------------

local scriptDir = arg and arg[0] and arg[0]:match("(.-)[^/\\]*$") or "./"
if scriptDir == "" then scriptDir = "./" end

-- Add paths so require() finds the PCG library modules
package.path = scriptDir .. "../Loenn/?.lua;"
             .. scriptDir .. "../Loenn/?/init.lua;"
             .. scriptDir .. "?.lua;"
             .. package.path

--------------------------------------------------------------------------------
-- 1. Argument Parser
--------------------------------------------------------------------------------

local function printHelp()
    print([[
MaggyHelper PCG Pipeline CLI
=============================

Usage:  lua pcg_cli.lua [OPTIONS]

Options:
  -c,   --config STR            MdMC configuration matrix (default "000011012")
                                  3x3 neighbor context as 9-char string.
                                  0=ignore, 1=context, 2=target tile.

  -td,  --training-dataset STR  Training data source (default "builtin")
                                  "builtin"  — use hand-crafted sample rooms
                                  <path.bin> — extract rooms from a Celeste map

  -nr,  --nb-rooms INT          Number of rooms to generate (default 8)

  -p,   --proba FLOAT           Branching probability / labyrinth factor (default 0.3)
                                  0.0 = linear pathway
                                  1.0 = fully random room order

  -rs,  --room-size W H         Room dimensions in tiles (default 40 23)

  -btd, --bt-depth INT          Max MdMC backtracking depth (default 3)

  -tl,  --tries-limit INT       Max generation attempts per room (default 15)

  -r,   --reset-skeleton        Regenerate skeleton on room failure

  -s,   --seed INT              Random seed, 0 = auto (default 0)

  -o,   --output STR            Output .bin file path
                                  (default ../Maps/Maggy/PCG/pcg_generated.bin)

        --preset STR            Use a named preset (overrides defaults for
                                  config, room-size, bt-depth, entity density)

        --list-presets           List all available presets and exit

        --dry-run               Run pipeline without writing output file

  -v,   --verbose               Print detailed progress for each room

  -q,   --quiet                 Only print errors

  -h,   --help                  Show this help and exit
]])
end

local function parseArgs(rawArgs)
    local opts = {
        config           = "000011012",
        trainingDataset  = "builtin",
        nbRooms          = 8,
        proba            = 0.3,
        roomWidth        = 40,
        roomHeight       = 23,
        btDepth          = 3,
        triesLimit       = 15,
        resetSkeleton    = false,
        seed             = 0,
        output           = nil,  -- resolved later
        preset           = nil,
        listPresets      = false,
        dryRun           = false,
        verbose          = false,
        quiet            = false,
        help             = false,
    }

    local i = 1
    while i <= #rawArgs do
        local a = rawArgs[i]

        if a == "-h" or a == "--help" then
            opts.help = true

        elseif a == "--list-presets" then
            opts.listPresets = true

        elseif a == "--dry-run" then
            opts.dryRun = true

        elseif a == "-r" or a == "--reset-skeleton" then
            opts.resetSkeleton = true

        elseif a == "-v" or a == "--verbose" then
            opts.verbose = true

        elseif a == "-q" or a == "--quiet" then
            opts.quiet = true

        elseif a == "-c" or a == "--config" then
            i = i + 1
            opts.config = rawArgs[i]
            if not opts.config or #opts.config ~= 9 then
                io.stderr:write("Error: --config requires a 9-character string (e.g. 000011012)\n")
                os.exit(1)
            end

        elseif a == "-td" or a == "--training-dataset" then
            i = i + 1
            opts.trainingDataset = rawArgs[i]
            if not opts.trainingDataset then
                io.stderr:write("Error: --training-dataset requires a path or 'builtin'\n")
                os.exit(1)
            end

        elseif a == "-nr" or a == "--nb-rooms" then
            i = i + 1
            opts.nbRooms = tonumber(rawArgs[i])
            if not opts.nbRooms or opts.nbRooms < 1 then
                io.stderr:write("Error: --nb-rooms requires a positive integer\n")
                os.exit(1)
            end

        elseif a == "-p" or a == "--proba" then
            i = i + 1
            opts.proba = tonumber(rawArgs[i])
            if not opts.proba or opts.proba < 0 or opts.proba > 1 then
                io.stderr:write("Error: --proba requires a float in [0.0, 1.0]\n")
                os.exit(1)
            end

        elseif a == "-rs" or a == "--room-size" then
            i = i + 1
            opts.roomWidth = tonumber(rawArgs[i])
            i = i + 1
            opts.roomHeight = tonumber(rawArgs[i])
            if not opts.roomWidth or not opts.roomHeight
               or opts.roomWidth < 8 or opts.roomHeight < 8 then
                io.stderr:write("Error: --room-size requires two integers >= 8 (width height)\n")
                os.exit(1)
            end

        elseif a == "-btd" or a == "--bt-depth" then
            i = i + 1
            opts.btDepth = tonumber(rawArgs[i])
            if not opts.btDepth or opts.btDepth < 0 then
                io.stderr:write("Error: --bt-depth requires an integer >= 0\n")
                os.exit(1)
            end

        elseif a == "-tl" or a == "--tries-limit" then
            i = i + 1
            opts.triesLimit = tonumber(rawArgs[i])
            if not opts.triesLimit or opts.triesLimit < 1 then
                io.stderr:write("Error: --tries-limit requires a positive integer\n")
                os.exit(1)
            end

        elseif a == "-s" or a == "--seed" then
            i = i + 1
            opts.seed = tonumber(rawArgs[i])
            if not opts.seed then
                io.stderr:write("Error: --seed requires an integer\n")
                os.exit(1)
            end

        elseif a == "-o" or a == "--output" then
            i = i + 1
            opts.output = rawArgs[i]
            if not opts.output then
                io.stderr:write("Error: --output requires a file path\n")
                os.exit(1)
            end

        elseif a == "--preset" then
            i = i + 1
            opts.preset = rawArgs[i]
            if not opts.preset then
                io.stderr:write("Error: --preset requires a name\n")
                os.exit(1)
            end

        else
            io.stderr:write("Unknown option: " .. tostring(a) .. "\n")
            io.stderr:write("Run with --help for usage.\n")
            os.exit(1)
        end

        i = i + 1
    end

    -- Default output path relative to script location
    if not opts.output then
        opts.output = scriptDir .. "../Maps/Maggy/PCG/pcg_generated.bin"
    end

    return opts
end

--------------------------------------------------------------------------------
-- 2. Matrix Shim (replaces Lönn's built-in matrix library)
--------------------------------------------------------------------------------
-- The PCG modules call matrix:get(x,y,default), matrix:set(x,y,val),
-- matrix:size() and the top-level matrixLib.filled(val, w, h).

local matrixLib = {}

function matrixLib.filled(val, w, h)
    local data = {}
    for y = 1, h do
        data[y] = {}
        for x = 1, w do
            data[y][x] = val
        end
    end

    local mt = {}
    mt.__index = mt

    function mt:get(x, y, default)
        if x < 1 or y < 1 or x > w or y > h then
            return default or "0"
        end
        return data[y][x] or default or "0"
    end

    function mt:set(x, y, val)
        if x >= 1 and y >= 1 and x <= w and y <= h then
            data[y][x] = val
        end
    end

    function mt:size()
        return w, h
    end

    function mt:getSlice(x1, y1, x2, y2, default)
        local slice = {}
        for y = y1, y2 do
            slice[y - y1 + 1] = {}
            for x = x1, x2 do
                slice[y - y1 + 1][x - x1 + 1] = self:get(x, y, default)
            end
        end
        return slice
    end

    return setmetatable({}, mt)
end

function matrixLib.fromFunction(fn, w, h)
    local m = matrixLib.filled("0", w, h)
    for y = 1, h do
        for x = 1, w do
            m:set(x, y, fn(x, y))
        end
    end
    return m
end

--------------------------------------------------------------------------------
-- 3. Built-in Training Data
--------------------------------------------------------------------------------
-- Hand-crafted sample rooms for when no external dataset is provided.

local function roomFromStrings(lines)
    local h = #lines
    local w = #lines[1]
    local m = matrixLib.filled("0", w, h)
    for y = 1, h do
        for x = 1, w do
            if lines[y]:sub(x, x) == "1" then
                m:set(x, y, "1")
            end
        end
    end
    return m
end

local function createBuiltinTrainingRooms()
    local rooms = {}

    -- Platforming room with staggered ledges
    table.insert(rooms, roomFromStrings({
        "11111111111111111111111111111111111111111",
        "1.......................................1",
        "1.......................................1",
        "1..1111....1111....1111....1111.........1",
        "1.......................................1",
        "1.......1111.......1111.......1111......1",
        "1.......................................1",
        "1....1111....1111....1111....1111.......1",
        "1.......................................1",
        "1..........1111........1111.............1",
        "1...1111...........1111.................1",
        "1.......................................1",
        "1.......................................1",
        "1.1111...1111..111...1111...1111..111...1",
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
    }))

    -- Staircase descent
    table.insert(rooms, roomFromStrings({
        "11111111111111111111111111111111111111111",
        "1.......................................1",
        "1...................................111.1",
        "1..................................1111.1",
        "1.............................111.......1",
        "1............................1111.......1",
        "1.........................111...........1",
        "1........................1111...........1",
        "1.....................111................1",
        "1....................1111................1",
        "1.................111...................1",
        "1................1111...................1",
        "1.............111......................1",
        "1............1111......................1",
        "1.........111..........................1",
        "1........1111..........................1",
        "1.....111..............................1",
        "1....1111..............................1",
        "1..111.................................1",
        "1.1111.................................1",
        "11111..................................1",
        "1.......................................1",
        "11111111111111111111111111111111111111111",
    }))

    -- Cavern arches
    table.insert(rooms, roomFromStrings({
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
        "111...1111111111...11111111...1111111111",
        "11.....11111111.....1111111.....11111111",
        "1.......111111.......111111.......111111",
        "1................1..........1..........1",
        "1.......11.......111.......111.........1",
        "11......111.....11111....111111....11111",
        "111....11111...1111111...1111111..111111",
        "11......111.....11111.....1111......1111",
        "1........1.......111.......11.........11",
        "1.................1....................11",
        "11...............11...................111",
        "1111...1111...111111...1111...1111111111",
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
        "11111111111111111111111111111111111111111",
    }))

    -- Grid / maze-like room
    table.insert(rooms, roomFromStrings({
        "11111111111111111111111111111111111111111",
        "1.........1.........1.........1........1",
        "1.........1.........1.........1........1",
        "1.........1.........1.........1........1",
        "1...111...1..111....1...111...1..111...1",
        "1...111........1....1...111........1...1",
        "1..........1...1....1.........1...1....1",
        "1..........1........1.........1........1",
        "1..111.....1.111....1..111....1..111...1",
        "1..111.....1.111....1..111....1..111...1",
        "1..........1........1.........1........1",
        "1..........1........1.........1........1",
        "1......111.1........1.....111.1........1",
        "1......111..........1.....111..........1",
        "1.......................................1",
        "1...111...1..111....1...111...1..111...1",
        "1...111........1....1...111........1...1",
        "1..........1...1....1.........1...1....1",
        "1..........1........1.........1........1",
        "1..111.....1.111....1..111....1..111...1",
        "1..111.....1.111....1..111....1..111...1",
        "1..........1........1.........1........1",
        "11111111111111111111111111111111111111111",
    }))

    -- Column room with vertical gaps
    table.insert(rooms, roomFromStrings({
        "11111111111111111111111111111111111111111",
        "1.1..1..1..1..1..1..1..1..1..1..1..1..1",
        "1.1..1..1..1..1..1..1..1..1..1..1..1..1",
        "1.1..1..1..1..1..1..1..1..1..1..1..1..1",
        "1.1.....1.....1.....1.....1.....1.....1",
        "1.1.....1.....1.....1.....1.....1.....1",
        "1.......................................1",
        "1.......................................1",
        "1..1.....1.....1.....1.....1.....1.....1",
        "1..1..1..1..1..1..1..1..1..1..1..1..1..1",
        "1..1..1..1..1..1..1..1..1..1..1..1..1..1",
        "1.....1.....1.....1.....1.....1.....1..1",
        "1.....1.....1.....1.....1.....1.....1..1",
        "1.......................................1",
        "1..1.....1.....1.....1.....1.....1.....1",
        "1..1..1..1..1..1..1..1..1..1..1..1..1..1",
        "1..1..1..1..1..1..1..1..1..1..1..1..1..1",
        "1.....1.....1.....1.....1.....1.....1..1",
        "1.....1.....1.....1.....1.....1.....1..1",
        "1.......................................1",
        "1.......................................1",
        "1.......................................1",
        "11111111111111111111111111111111111111111",
    }))

    -- Open chamber with central mound
    table.insert(rooms, roomFromStrings({
        "11111111111111111111111111111111111111111",
        "1.......................................1",
        "1.......................................1",
        "11.......11111111111111.......1111111111",
        "111......11111111111111......11111111111",
        "1111.....11111111111111.....111111111111",
        "1111.....11111111111111.....111111111111",
        "1111..........111111.........11111111111",
        "1111..........111111..........1111111111",
        "11111..........1111...........1111111111",
        "11111..........1111...........1111111111",
        "111111..........11............1111111111",
        "1111111.........11.........111111111111",
        "11111111...................1111111111111",
        "1111111111.................11111111111",
        "1.......................................1",
        "1.......................................1",
        "1.......................................1",
        "1.......................................1",
        "1.......................................1",
        "1.......................................1",
        "1.......................................1",
        "11111111111111111111111111111111111111111",
    }))

    return rooms
end

--------------------------------------------------------------------------------
-- 4. Load PCG Modules (standalone, via dofile)
--------------------------------------------------------------------------------

local markov      = dofile(scriptDir .. "../Loenn/libraries/pcg/markov.lua")
local generator   = dofile(scriptDir .. "../Loenn/libraries/pcg/generator.lua")
local skeleton    = dofile(scriptDir .. "../Loenn/libraries/pcg/skeleton.lua")
local pathfinder  = dofile(scriptDir .. "../Loenn/libraries/pcg/pathfinder.lua")
local binEncoder  = dofile(scriptDir .. "../Loenn/libraries/pcg/bin_encoder.lua")
local mapBuilder  = dofile(scriptDir .. "../Loenn/libraries/pcg/map_builder.lua")

--------------------------------------------------------------------------------
-- 5. Load Presets from init module (safe extraction)
--------------------------------------------------------------------------------

-- We load presets from init.lua by extracting just the PRESETS table.
-- If init.lua cannot be loaded standalone (e.g. Lönn dependencies), we define
-- a minimal fallback preset set here.

local PRESETS
do
    -- Try loading the full init module
    local ok, pcgInit = pcall(dofile, scriptDir .. "../Loenn/libraries/pcg/init.lua")
    if ok and pcgInit and pcgInit.PRESETS then
        PRESETS = pcgInit.PRESETS
    else
        -- Fallback: minimal presets matching the paper's configurations
        PRESETS = {
            default = {
                config = "000011012", roomWidth = 40, roomHeight = 23,
                exitSize = 5, borderMaterial = "1", backtrackDepth = 3,
                entityDensity = 0.15, cleanupPasses = 2, branchProb = 0.3,
                playabilityCheck = true, maxRetries = 10, roomStyle = "normal",
            },
            open = {
                config = "000011012", roomWidth = 48, roomHeight = 27,
                exitSize = 6, borderMaterial = "1", backtrackDepth = 4,
                entityDensity = 0.2, cleanupPasses = 3, branchProb = 0.2,
                playabilityCheck = true, maxRetries = 15, roomStyle = "normal",
            },
            tight = {
                config = "000011012", roomWidth = 32, roomHeight = 18,
                exitSize = 4, borderMaterial = "1", backtrackDepth = 2,
                entityDensity = 0.1, cleanupPasses = 1, branchProb = 0.4,
                playabilityCheck = true, maxRetries = 8, roomStyle = "normal",
            },
        }
    end
end

--------------------------------------------------------------------------------
-- 6. Logging Utilities
--------------------------------------------------------------------------------

local LOG_QUIET   = 0
local LOG_NORMAL  = 1
local LOG_VERBOSE = 2

local logLevel = LOG_NORMAL

local function log(level, fmt, ...)
    if level <= logLevel then
        print(string.format(fmt, ...))
    end
end

local function logn(fmt, ...)   log(LOG_NORMAL,  fmt, ...) end
local function logv(fmt, ...)   log(LOG_VERBOSE, fmt, ...) end
local function loge(fmt, ...)
    io.stderr:write(string.format("ERROR: " .. fmt .. "\n", ...))
end

--------------------------------------------------------------------------------
-- 7. ASCII Visualization
--------------------------------------------------------------------------------

local function renderMatrix(matrix, title)
    local w, h = matrix:size()
    local lines = {}
    table.insert(lines, string.format("  === %s (%dx%d) ===", title or "Room", w, h))
    for y = 1, h do
        local row = "  "
        for x = 1, w do
            local t = matrix:get(x, y, "0")
            row = row .. (t == "0" and "." or t)
        end
        table.insert(lines, row)
    end
    return table.concat(lines, "\n")
end

--------------------------------------------------------------------------------
-- 8. Training Data Loader
--------------------------------------------------------------------------------
-- Supports three training data sources:
--   "builtin"   — hand-crafted sample rooms (default)
--   *.json      — JSON file produced by celeste_map_to_json.py (rooms[].solids)
--   *.txt       — Plain-text tile grids separated by blank lines
--   *.bin       — Celeste binary map: auto-converts via celeste_map_to_json.py
--
-- For .bin files, the loader attempts to invoke celeste_map_to_json.py
-- (located in Tools/) to produce a temporary JSON, then parses that.

--- Minimal JSON parser (handles arrays, objects, strings, numbers, bools, null)
--- Does NOT handle all edge cases but works for celeste_map_to_json.py output.
local function jsonDecode(str)
    local pos = 1

    local function skipWhitespace()
        pos = str:find("[^ \t\r\n]", pos) or (#str + 1)
    end

    local function peek()
        skipWhitespace()
        return str:sub(pos, pos)
    end

    local parseValue  -- forward declaration

    local function parseString()
        assert(str:sub(pos, pos) == '"', "Expected '\"' at position " .. pos)
        pos = pos + 1
        local start = pos
        local parts = {}
        while pos <= #str do
            local ch = str:sub(pos, pos)
            if ch == '"' then
                table.insert(parts, str:sub(start, pos - 1))
                pos = pos + 1
                return table.concat(parts)
            elseif ch == '\\' then
                table.insert(parts, str:sub(start, pos - 1))
                pos = pos + 1
                local esc = str:sub(pos, pos)
                if esc == '"' or esc == '\\' or esc == '/' then
                    table.insert(parts, esc)
                elseif esc == 'n' then table.insert(parts, '\n')
                elseif esc == 'r' then table.insert(parts, '\r')
                elseif esc == 't' then table.insert(parts, '\t')
                else table.insert(parts, esc)
                end
                pos = pos + 1
                start = pos
            else
                pos = pos + 1
            end
        end
        error("Unterminated string at position " .. start)
    end

    local function parseNumber()
        local start = pos
        if str:sub(pos, pos) == '-' then pos = pos + 1 end
        while pos <= #str and str:sub(pos, pos):match("[0-9]") do pos = pos + 1 end
        if pos <= #str and str:sub(pos, pos) == '.' then
            pos = pos + 1
            while pos <= #str and str:sub(pos, pos):match("[0-9]") do pos = pos + 1 end
        end
        if pos <= #str and str:sub(pos, pos):lower() == 'e' then
            pos = pos + 1
            if str:sub(pos, pos) == '+' or str:sub(pos, pos) == '-' then pos = pos + 1 end
            while pos <= #str and str:sub(pos, pos):match("[0-9]") do pos = pos + 1 end
        end
        return tonumber(str:sub(start, pos - 1))
    end

    local function parseArray()
        assert(str:sub(pos, pos) == '[')
        pos = pos + 1
        local arr = {}
        if peek() == ']' then pos = pos + 1; return arr end
        while true do
            table.insert(arr, parseValue())
            skipWhitespace()
            if str:sub(pos, pos) == ']' then pos = pos + 1; return arr end
            assert(str:sub(pos, pos) == ',', "Expected ',' in array at " .. pos)
            pos = pos + 1
        end
    end

    local function parseObject()
        assert(str:sub(pos, pos) == '{')
        pos = pos + 1
        local obj = {}
        if peek() == '}' then pos = pos + 1; return obj end
        while true do
            skipWhitespace()
            local key = parseString()
            skipWhitespace()
            assert(str:sub(pos, pos) == ':', "Expected ':' after key at " .. pos)
            pos = pos + 1
            obj[key] = parseValue()
            skipWhitespace()
            if str:sub(pos, pos) == '}' then pos = pos + 1; return obj end
            assert(str:sub(pos, pos) == ',', "Expected ',' in object at " .. pos)
            pos = pos + 1
        end
    end

    parseValue = function()
        skipWhitespace()
        local ch = str:sub(pos, pos)
        if ch == '"' then return parseString()
        elseif ch == '{' then return parseObject()
        elseif ch == '[' then return parseArray()
        elseif ch == 't' then
            assert(str:sub(pos, pos + 3) == "true")
            pos = pos + 4; return true
        elseif ch == 'f' then
            assert(str:sub(pos, pos + 4) == "false")
            pos = pos + 5; return false
        elseif ch == 'n' then
            assert(str:sub(pos, pos + 3) == "null")
            pos = pos + 4; return nil
        else
            return parseNumber()
        end
    end

    return parseValue()
end

--- Parse tile grid lines (newline-separated tile characters) into a matrix.
local function tileStringToMatrix(tileStr)
    if not tileStr or tileStr == "" then return nil end
    local rows = {}
    for line in tileStr:gmatch("[^\r\n]+") do
        if #line > 0 then
            table.insert(rows, line)
        end
    end
    if #rows < 2 then return nil end
    local w = #rows[1]
    if w < 4 then return nil end
    local m = matrixLib.filled("0", w, #rows)
    for y, line in ipairs(rows) do
        for x = 1, math.min(w, #line) do
            local ch = line:sub(x, x)
            if ch ~= "0" then
                m:set(x, y, ch)
            end
        end
    end
    return m
end

--- Extract rooms from a JSON map structure produced by celeste_map_to_json.py.
--- The JSON format has: { rooms: [ { tiles: { width, height, tiles: [flat array] }, ... } ] }
local function extractRoomsFromJson(data)
    local rooms = {}

    -- Primary format: data.rooms[].tiles.tiles (flat array)
    if data.rooms and type(data.rooms) == "table" then
        for _, room in ipairs(data.rooms) do
            if room.tiles and room.tiles.tiles then
                local tw = room.tiles.width or room.tileWidth or 40
                local th = room.tiles.height or room.tileHeight or 23
                local flat = room.tiles.tiles

                if type(flat) == "table" and #flat >= tw * th then
                    local m = matrixLib.filled("0", tw, th)
                    for y = 1, th do
                        for x = 1, tw do
                            local idx = (y - 1) * tw + x
                            local ch = flat[idx]
                            if ch and ch ~= "0" then
                                m:set(x, y, tostring(ch))
                            end
                        end
                    end
                    -- Only use rooms with some solid tiles
                    local solidCount = 0
                    for y = 1, th do
                        for x = 1, tw do
                            if m:get(x, y, "0") ~= "0" then
                                solidCount = solidCount + 1
                            end
                        end
                    end
                    if solidCount > 0 and tw >= 8 and th >= 8 then
                        table.insert(rooms, m)
                    end
                end
            end

            -- Also check for solids as a tile string (newline-separated grid)
            if room.solids and type(room.solids) == "string" then
                local m = tileStringToMatrix(room.solids)
                if m then
                    local mw, mh = m:size()
                    if mw >= 8 and mh >= 8 then
                        table.insert(rooms, m)
                    end
                end
            end
        end
    end

    -- Fallback: recursive walk for nested element trees
    if #rooms == 0 then
        local function walk(node)
            if type(node) ~= "table" then return end

            local solids = nil
            if node.innerText and type(node.innerText) == "string" then
                if node.__name == "solids" or node.name == "solids" then
                    solids = node.innerText
                end
            end
            if node.__children then
                for _, child in ipairs(node.__children) do
                    if type(child) == "table" and
                       (child.__name == "solids" or child.name == "solids") and
                       child.innerText then
                        solids = child.innerText
                    end
                end
            end

            if solids then
                local m = tileStringToMatrix(solids)
                if m then
                    local mw, mh = m:size()
                    if mw >= 8 and mh >= 8 then
                        table.insert(rooms, m)
                    end
                end
            end

            -- Recurse
            if node.__children then
                for _, child in ipairs(node.__children) do walk(child) end
            end
            if node.children then
                for _, child in ipairs(node.children) do walk(child) end
            end
            for i = 1, #node do
                if type(node[i]) == "table" then walk(node[i]) end
            end
        end
        walk(data)
    end

    return rooms
end

local function loadTrainingData(source)
    if source == "builtin" then
        logn("  Source: built-in hand-crafted rooms")
        return createBuiltinTrainingRooms()
    end

    logn("  Source: %s", source)

    -- Resolve path: try as-is, then relative to script dir
    local resolvedPath = source
    local fh = io.open(resolvedPath, "rb")
    if not fh then
        resolvedPath = scriptDir .. source
        fh = io.open(resolvedPath, "rb")
    end
    if not fh then
        return nil, "Cannot open training dataset file: " .. source
    end
    fh:close()

    local ext = resolvedPath:match("%.([^%.]+)$")
    ext = ext and ext:lower() or ""

    ---------------------------------------------------------------------------
    -- .txt files: plain-text tile grids separated by blank lines
    ---------------------------------------------------------------------------
    if ext == "txt" then
        local fh2 = io.open(resolvedPath, "r")
        local content = fh2:read("*a")
        fh2:close()

        local rooms = {}
        -- Split on double-newlines
        for block in content:gmatch("[^\n]+(\n[^\n]+)*") do
            local m = tileStringToMatrix(block)
            if m then
                local mw, mh = m:size()
                if mw >= 8 and mh >= 8 then
                    table.insert(rooms, m)
                end
            end
        end

        if #rooms == 0 then
            return nil, "No valid tile grids found in: " .. source
        end
        return rooms
    end

    ---------------------------------------------------------------------------
    -- .json files: JSON map data (e.g. from celeste_map_to_json.py)
    ---------------------------------------------------------------------------
    if ext == "json" then
        local fh2 = io.open(resolvedPath, "r")
        local content = fh2:read("*a")
        fh2:close()

        local ok, data = pcall(jsonDecode, content)
        if not ok then
            return nil, "Failed to parse JSON from: " .. source .. "\n  " .. tostring(data)
        end

        local rooms = extractRoomsFromJson(data)
        if #rooms == 0 then
            return nil, "No rooms with tile data found in JSON: " .. source
        end
        return rooms
    end

    ---------------------------------------------------------------------------
    -- .bin files: Celeste binary maps — auto-convert via celeste_map_to_json.py
    ---------------------------------------------------------------------------
    if ext == "bin" then
        -- Check for celeste_map_to_json.py in the same directory
        local converterPath = scriptDir .. "celeste_map_to_json.py"
        local converterExists = io.open(converterPath, "r")
        if not converterExists then
            return nil, string.format(
                "Cannot auto-convert .bin file. Place celeste_map_to_json.py in %s\n" ..
                "  Or convert manually: python celeste_map_to_json.py %s output.json\n" ..
                "  Then run: lua pcg_cli.lua --training-dataset output.json",
                scriptDir, resolvedPath)
        end
        converterExists:close()

        -- Create temp JSON output path
        local tmpJson = resolvedPath:gsub("%.bin$", "") .. "_pcgtmp.json"
        local cmd = string.format('python "%s" "%s" "%s"', converterPath, resolvedPath, tmpJson)
        logn("  Converting .bin → JSON via celeste_map_to_json.py ...")
        logv("  Command: %s", cmd)

        local exitCode = os.execute(cmd)
        if exitCode ~= 0 and exitCode ~= true then
            return nil, "celeste_map_to_json.py failed. Convert manually and use --training-dataset with a .json file."
        end

        -- Parse the JSON
        local fh2 = io.open(tmpJson, "r")
        if not fh2 then
            return nil, "Converter ran but JSON output not found: " .. tmpJson
        end
        local content = fh2:read("*a")
        fh2:close()

        -- Clean up temp file
        os.remove(tmpJson)

        local ok, data = pcall(jsonDecode, content)
        if not ok then
            return nil, "Failed to parse converted JSON: " .. tostring(data)
        end

        local rooms = extractRoomsFromJson(data)
        if #rooms == 0 then
            return nil, "No rooms with tile data found after converting: " .. source
        end
        return rooms
    end

    return nil, string.format(
        "Unsupported training dataset format: .%s\n" ..
        "  Supported: .json, .txt, .bin (requires celeste_map_to_json.py), or 'builtin'",
        ext)
end

--------------------------------------------------------------------------------
-- 9. Main Pipeline
--------------------------------------------------------------------------------

local function main()
    local opts = parseArgs(arg or {})

    -- Handle immediate exit commands
    if opts.help then
        printHelp()
        os.exit(0)
    end

    if opts.listPresets then
        local names = {}
        for name, _ in pairs(PRESETS) do
            table.insert(names, name)
        end
        table.sort(names)
        print("Available PCG presets:")
        print(string.rep("-", 60))
        for _, name in ipairs(names) do
            local p = PRESETS[name]
            print(string.format("  %-20s  %dx%d  config=%s  bt=%d  style=%s",
                name,
                p.roomWidth or 40, p.roomHeight or 23,
                p.config or "000011012",
                p.backtrackDepth or 3,
                p.roomStyle or "normal"))
        end
        print(string.rep("-", 60))
        print(string.format("Total: %d presets", #names))
        os.exit(0)
    end

    -- Set log level
    if opts.quiet then
        logLevel = LOG_QUIET
    elseif opts.verbose then
        logLevel = LOG_VERBOSE
    end

    -- Apply preset overrides
    local preset = nil
    if opts.preset then
        preset = PRESETS[opts.preset]
        if not preset then
            loge("Unknown preset: %s", opts.preset)
            loge("Run with --list-presets to see available presets.")
            os.exit(1)
        end
        -- Preset provides defaults; CLI flags still override
        -- We only apply preset values if the user did NOT explicitly set them
        -- (We detect this by checking if the value equals the default from parseArgs)
        if opts.config == "000011012" and preset.config then
            opts.config = preset.config
        end
        if opts.roomWidth == 40 and preset.roomWidth then
            opts.roomWidth = preset.roomWidth
        end
        if opts.roomHeight == 23 and preset.roomHeight then
            opts.roomHeight = preset.roomHeight
        end
        if opts.btDepth == 3 and preset.backtrackDepth then
            opts.btDepth = preset.backtrackDepth
        end
        if opts.proba == 0.3 and preset.branchProb then
            opts.proba = preset.branchProb
        end
        if opts.triesLimit == 15 and preset.maxRetries then
            opts.triesLimit = preset.maxRetries
        end
    end

    -- Resolve seed
    local seed = opts.seed
    if seed == 0 then
        seed = os.time()
    end
    math.randomseed(seed)

    -- Get auxiliary settings from preset or defaults
    local exitSize       = (preset and preset.exitSize)       or 5
    local borderMaterial = (preset and preset.borderMaterial)  or "1"
    local entityDensity  = (preset and preset.entityDensity)  or 0.15
    local cleanupPasses  = (preset and preset.cleanupPasses)  or 2
    local checkPlayable  = (preset and preset.playabilityCheck ~= nil) and preset.playabilityCheck or true
    local roomStyle      = (preset and preset.roomStyle)      or "normal"

    -- Banner
    logn("=" .. string.rep("=", 68))
    logn("  MaggyHelper PCG Pipeline CLI")
    logn("=" .. string.rep("=", 68))
    logn("")
    logn("  Configuration:")
    logn("    MdMC config:       %s", opts.config)
    logn("    Rooms:             %d", opts.nbRooms)
    logn("    Room size:         %dx%d tiles (%dx%d px)",
        opts.roomWidth, opts.roomHeight,
        opts.roomWidth * 8, opts.roomHeight * 8)
    logn("    Branch probability: %.2f", opts.proba)
    logn("    Backtrack depth:   %d", opts.btDepth)
    logn("    Tries limit:       %d", opts.triesLimit)
    logn("    Reset skeleton:    %s", opts.resetSkeleton and "yes" or "no")
    logn("    Seed:              %d%s", seed, opts.seed == 0 and " (auto)" or "")
    if opts.preset then
        logn("    Preset:            %s", opts.preset)
    end
    logn("    Output:            %s", opts.output)
    logn("")

    ---------------------------------------------------------------------------
    -- STEP 1: Load training dataset & build DPT
    ---------------------------------------------------------------------------
    logn("[1/5] Loading training data...")

    local trainingRooms, loadErr = loadTrainingData(opts.trainingDataset)
    if not trainingRooms then
        loge("Failed to load training data: %s", loadErr or "unknown error")
        os.exit(1)
    end

    logn("  Loaded %d training rooms", #trainingRooms)

    logn("[1/5] Building Dictionary of Probability Transitions (DPT)...")
    local offsets = markov.parseConfig(opts.config)
    local dpt = markov.train(trainingRooms, offsets)

    local ngramCount = 0
    for _ in pairs(dpt) do ngramCount = ngramCount + 1 end
    logn("  DPT: %d unique n-gram states from %d rooms", ngramCount, #trainingRooms)
    logn("")

    ---------------------------------------------------------------------------
    -- STEP 2: Generate level skeleton
    ---------------------------------------------------------------------------
    local maxSkeletonAttempts = opts.resetSkeleton and 5 or 1
    local skel = nil
    local skeletonAttempt = 0

    local function generateAndPopulateSkeleton()
        skeletonAttempt = skeletonAttempt + 1
        logn("[2/5] Generating level skeleton%s...",
            skeletonAttempt > 1 and string.format(" (attempt %d)", skeletonAttempt) or "")

        local currentSeed = seed + (skeletonAttempt - 1) * 7919
        skel = skeleton.generate(opts.nbRooms, opts.roomWidth, opts.roomHeight, opts.proba, currentSeed)

        logn("  Created %d-room skeleton", #skel.rooms)
        logv("  Start room: %d, End room: %d", skel.startRoom, skel.endRoom)
        logn("")
        logn(skeleton.visualize(skel))
        logn("")

        -----------------------------------------------------------------------
        -- STEP 3: Generate room content via MdMC
        -----------------------------------------------------------------------
        logn("[3/5] Generating room content via MdMC...")
        logn("")

        local levelRooms = {}
        local playableCount = 0
        local failedRooms = {}

        for ri, skelRoom in ipairs(skel.rooms) do
            local bestMatrix   = nil
            local bestEntities = nil
            local bestPlayable = false
            local bestAttempt  = 0

            for attempt = 1, opts.triesLimit do
                local roomSeed = currentSeed + skelRoom.id * 100 + attempt

                local m = markov.generate(dpt, offsets, opts.roomWidth, opts.roomHeight,
                                          opts.btDepth, roomSeed, matrixLib)
                if m then
                    generator.placeBorders(m, borderMaterial, skelRoom.exits, exitSize)
                    generator.placeExits(m, skelRoom.exits, exitSize)
                    generator.cleanupTiles(m, cleanupPasses)

                    local playable = true
                    if checkPlayable then
                        playable = pathfinder.evaluatePlayability(m, skelRoom.exits, exitSize)
                    end

                    if playable or attempt == opts.triesLimit then
                        math.randomseed(roomSeed)
                        local entities = generator.generateEntities(
                            m, skelRoom.exits, skelRoom.x, skelRoom.y, entityDensity
                        )

                        bestMatrix   = m
                        bestEntities = entities
                        bestPlayable = playable
                        bestAttempt  = attempt

                        if playable then break end
                    end
                end
            end

            if not bestMatrix then
                table.insert(failedRooms, skelRoom.id)
                loge("  Room %d: FAILED after %d attempts", skelRoom.id, opts.triesLimit)
            else
                if bestPlayable then playableCount = playableCount + 1 end

                local roomName = string.format("pcg_%02d", skelRoom.id)
                local roomData = {
                    matrix   = bestMatrix,
                    entities = bestEntities,
                    width    = opts.roomWidth,
                    height   = opts.roomHeight,
                    exits    = skelRoom.exits,
                    x        = skelRoom.x,
                    y        = skelRoom.y,
                    name     = roomName,
                }
                table.insert(levelRooms, roomData)

                -- Build exit display string
                local exitStr = ""
                if skelRoom.exits.left   then exitStr = exitStr .. "L" end
                if skelRoom.exits.right  then exitStr = exitStr .. "R" end
                if skelRoom.exits.top    then exitStr = exitStr .. "T" end
                if skelRoom.exits.bottom then exitStr = exitStr .. "B" end

                local label = string.format("Room %d [%s]%s%s",
                    skelRoom.id, exitStr,
                    skelRoom.isStart and " (START)" or "",
                    skelRoom.isEnd   and " (END)"   or "")

                logv("%s", renderMatrix(bestMatrix, label))
                logv("  Playable: %s | Entities: %d | Attempt: %d/%d",
                    bestPlayable and "YES" or "NO",
                    #bestEntities, bestAttempt, opts.triesLimit)
                logv("")

                logn("  Room %2d/%d  [%s]%s  playable=%-3s  entities=%-3d  attempts=%d",
                    ri, #skel.rooms, exitStr,
                    skelRoom.isStart and " START" or (skelRoom.isEnd and " END  " or "      "),
                    bestPlayable and "yes" or "no",
                    #bestEntities, bestAttempt)
            end
        end

        logn("")
        logn("  Playability: %d/%d rooms (%d%%)",
            playableCount, #skel.rooms,
            #skel.rooms > 0 and math.floor(playableCount / #skel.rooms * 100) or 0)

        -- Check if we need to reset skeleton
        if #failedRooms > 0 and opts.resetSkeleton and skeletonAttempt < maxSkeletonAttempts then
            logn("")
            logn("  WARNING: %d room(s) failed. Resetting skeleton...", #failedRooms)
            return nil, nil, failedRooms
        end

        return levelRooms, playableCount, failedRooms
    end

    -- Run with optional skeleton reset loop
    local levelRooms, playableCount, failedRooms

    for _ = 1, maxSkeletonAttempts do
        levelRooms, playableCount, failedRooms = generateAndPopulateSkeleton()
        if levelRooms then break end
    end

    if not levelRooms or #levelRooms == 0 then
        loge("Pipeline failed: no rooms were generated after %d skeleton attempts.", skeletonAttempt)
        os.exit(1)
    end

    ---------------------------------------------------------------------------
    -- STEP 4: Build map data structure
    ---------------------------------------------------------------------------
    logn("")
    logn("[4/5] Building Celeste map structure...")

    local levelData = { rooms = levelRooms, skeleton = skel }
    local packageName = "Maggy/PCG/pcg_generated"
    local mapData = mapBuilder.buildMap(levelData, packageName)

    logn("  Package SID:  %s", packageName)
    if mapData.__children and mapData.__children[2] and mapData.__children[2].__children then
        logn("  Levels:       %d", #mapData.__children[2].__children)
    end

    ---------------------------------------------------------------------------
    -- STEP 5: Encode & write .bin file
    ---------------------------------------------------------------------------
    if opts.dryRun then
        logn("")
        logn("[5/5] Dry run — skipping .bin output")
        logn("")
        logn("=" .. string.rep("=", 68))
        logn("  DRY RUN COMPLETE")
        logn("  %d rooms generated, %d playable",
            #levelRooms, playableCount or 0)
        logn("=" .. string.rep("=", 68))
        os.exit(0)
    end

    logn("")
    logn("[5/5] Encoding Room Encoder → .bin output...")

    -- Ensure output directory exists
    local outputDir = opts.output:match("(.-)[^/\\]+$") or "./"
    if outputDir ~= "" then
        -- Cross-platform directory creation
        local isWindows = package.config:sub(1, 1) == "\\"
        if isWindows then
            os.execute('mkdir "' .. outputDir .. '" 2>nul')
        else
            os.execute('mkdir -p "' .. outputDir .. '" 2>/dev/null')
        end
    end

    local ok, err = binEncoder.encodeFile(opts.output, mapData)

    if ok then
        -- Report file size
        local fh = io.open(opts.output, "rb")
        local size = 0
        if fh then
            size = fh:seek("end")
            fh:close()
        end

        logn("  Written: %s", opts.output)
        logn("  Size:    %d bytes (%.1f KB)", size, size / 1024)
        logn("")
        logn("=" .. string.rep("=", 68))
        logn("  SUCCESS — Level generated!")
        logn("")
        logn("  Output:  %s", opts.output)
        logn("  Rooms:   %d generated, %d playable",
            #levelRooms, playableCount or 0)
        logn("  Seed:    %d", seed)
        logn("")
        logn("  To play in Celeste:")
        logn("    SID: %s", packageName)
        logn("    Open in Loenn or load via Everest debug menu")
        logn("=" .. string.rep("=", 68))
    else
        loge("Failed to write .bin file: %s", tostring(err))
        os.exit(1)
    end
end

--------------------------------------------------------------------------------
-- Entry point
--------------------------------------------------------------------------------
main()
