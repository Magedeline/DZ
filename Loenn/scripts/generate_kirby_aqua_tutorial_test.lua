--[[
    Kirby + Aqua Tutorial Bird Test Map Generator
    ============================================
    Generates a compact test map focused on the new tutorial bird flow:
      - MaggyHelper/KirbyTutorialBird
      - MaggyHelper/KirbyTutorialBirdTrigger
      - Aqua condition-function gating via AquaTutorialCompat

    Run from the Tools/ directory:
      lua generate_kirby_aqua_tutorial_test.lua

    Output:
      Maps/Maggy/PCG/kirby_aqua_tutorial_test.bin
      SID: Maggy/PCG/kirby_aqua_tutorial_test
]]

local scriptDir = arg and arg[0] and arg[0]:match("(.-)[^/\\]*$") or "./"

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

    function mt:set(x, y, v)
        if x >= 1 and y >= 1 and x <= w and y <= h then
            data[y][x] = v
        end
    end

    function mt:size()
        return w, h
    end

    return setmetatable({}, mt)
end

local binEncoder = dofile(scriptDir .. "../Loenn/libraries/pcg/bin_encoder.lua")
local mapBuilder = dofile(scriptDir .. "../Loenn/libraries/pcg/map_builder.lua")

local ROOM_W = 60
local ROOM_H = 34
local ROOM_PX_W = ROOM_W * 8
local ROOM_PX_H = ROOM_H * 8

local nextEntityId = 1
local function eid()
    local id = nextEntityId
    nextEntityId = nextEntityId + 1
    return id
end

local function makeRoomMatrix(w, h)
    local m = matrixLib.filled("0", w, h)

    -- Outer walls
    for x = 1, w do
        m:set(x, 1, "1")
        m:set(x, 2, "1")
        m:set(x, h, "1")
        m:set(x, h - 1, "1")
    end
    for y = 1, h do
        m:set(1, y, "1")
        m:set(2, y, "1")
        m:set(w, y, "1")
        m:set(w - 1, y, "1")
    end

    -- Main floor
    for x = 3, w - 2 do
        m:set(x, h - 3, "1")
        m:set(x, h - 2, "1")
    end

    -- Small swing platform strip
    for x = 18, 28 do
        m:set(x, h - 10, "1")
    end

    return m
end

local function buildRoom(index, name, entities, triggers)
    local gridX = (index - 1) % 2
    local gridY = math.floor((index - 1) / 2)

    return {
        matrix = makeRoomMatrix(ROOM_W, ROOM_H),
        entities = entities or {},
        triggers = triggers or {},
        width = ROOM_W,
        height = ROOM_H,
        x = gridX * ROOM_PX_W,
        y = gridY * ROOM_PX_H,
        name = name,
        roomStyle = "normal",
    }
end

-- Inject triggers into room XML output.
local origRoomToLevel = mapBuilder.roomToLevel
mapBuilder.roomToLevel = function(roomData, index)
    local roomElement = origRoomToLevel(roomData, index)
    if roomData.triggers and #roomData.triggers > 0 then
        for _, child in ipairs(roomElement.__children) do
            if child.__name == "triggers" then
                child.__children = child.__children or {}
                for _, trig in ipairs(roomData.triggers) do
                    table.insert(child.__children, mapBuilder.entityToElement(trig))
                end
            end
        end
    end
    return roomElement
end

local rooms = {}

-- Room 1: Intro tutorial bird + fixed-hook progression trigger.
do
    local entities = {
        { _name = "player", id = eid(), x = 40, y = 200 },
        {
            _name = "MaggyHelper/KirbyTutorialBird",
            id = eid(),
            x = 120,
            y = 200,
            birdId = "kirby_aqua_intro",
            dialogs = "tutorial_dash;tutorial_dreamjump",
            controls = "mod:Aqua/ThrowHook,PLUS,UpRight;HOLD,Grab,PLUS,tinyarrow,Jump",
            startupIndex = 0,
            triggerOnce = true,
            faceLeft = true,
            caw = true,
            onlyOnce = false,
        },
        {
            _name = "MaggyHelper/Kirby_Mode_Toggle_Trigger",
            id = eid(),
            x = 300,
            y = 160,
            width = 32,
            height = 32,
            activationMode = "OnEnter",
            transformEffect = "Instant",
            triggerState = "Enable",
            oneUse = false,
            respectSettings = true,
            silentMode = false,
            initialPower = "None",
            effectDuration = 0.2,
            particleColor = "FFC0CB",
            particleCount = 0,
            screenShake = false,
            shakeIntensity = 0.0,
            transformSound = "event:/desolozantas/char/kirby/transform",
            playSound = false,
            flagRequired = "",
            flagToSet = "",
        }
    }

    local triggers = {
        {
            _name = "MaggyHelper/KirbyTutorialBirdTrigger",
            id = eid(),
            x = 220,
            y = 144,
            width = 80,
            height = 80,
            birdId = "kirby_aqua_intro",
            tutorialIndex = 1,
            conditionFunction = "mod:MaggyHelper.Extensions.Kirby.ModCompat.AquaTutorialCompat.IsAquaHookFixed",
        }
    }

    table.insert(rooms, buildRoom(1, "aqua_tutorial_intro", entities, triggers))
end

-- Room 2: Swing tutorial + close trigger on attracted state.
do
    local entities = {
        { _name = "player", id = eid(), x = 40, y = 200 },
        {
            _name = "MaggyHelper/KirbyTutorialBird",
            id = eid(),
            x = 160,
            y = 200,
            birdId = "kirby_aqua_swing",
            dialogs = "tutorial_climb;tutorial_dreamjump",
            controls = "HOLD,Grab,PLUS,Left;HOLD,Grab,PLUS,tinyarrow,Jump",
            startupIndex = -1,
            triggerOnce = true,
            faceLeft = false,
            caw = true,
            onlyOnce = false,
        }
    }

    local triggers = {
        {
            _name = "MaggyHelper/KirbyTutorialBirdTrigger",
            id = eid(),
            x = 150,
            y = 96,
            width = 120,
            height = 144,
            birdId = "kirby_aqua_swing",
            tutorialIndex = 1,
            conditionFunction = "mod:MaggyHelper.Extensions.Kirby.ModCompat.AquaTutorialCompat.IsKirbyAquaSwinging",
        },
        {
            _name = "MaggyHelper/KirbyTutorialBirdTrigger",
            id = eid(),
            x = 300,
            y = 96,
            width = 120,
            height = 144,
            birdId = "kirby_aqua_swing",
            tutorialIndex = -1,
            conditionFunction = "mod:MaggyHelper.Extensions.Kirby.ModCompat.AquaTutorialCompat.IsKirbyAquaAttracted",
        }
    }

    table.insert(rooms, buildRoom(2, "aqua_tutorial_swing", entities, triggers))
end

print("===============================================")
print("  Kirby + Aqua Tutorial Bird Test Generator")
print("===============================================")
print("  Rooms: " .. tostring(#rooms))

local levelData = { rooms = rooms }
local packageName = "Maggy/PCG/kirby_aqua_tutorial_test"
local mapData = mapBuilder.buildMap(levelData, packageName)

local outputDir = scriptDir .. "../Maps/Maggy/PCG"
os.execute('mkdir "' .. outputDir .. '" 2>nul')

local outputPath = outputDir .. "/kirby_aqua_tutorial_test.bin"
local ok, err = binEncoder.encodeFile(outputPath, mapData)

if not ok then
    print("ERROR: " .. tostring(err))
    os.exit(1)
end

print("  Written: " .. outputPath)
print("  SID: " .. packageName)
print("===============================================")
