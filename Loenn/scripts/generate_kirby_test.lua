--[[
    Kirby Test Map Generator — PCG Showcase
    =========================================
    Generates a comprehensive test map (#kirby_test.bin) that places
    EVERY MaggyHelper entity and trigger in themed showcase rooms,
    each with an explanatory sign NPC.

    Run from the Tools/ directory:
        lua generate_kirby_test.lua

    Output:
        Maps/Maggy/PCG/kirby_test.bin   (SID: Maggy/PCG/kirby_test)
]]

local scriptDir = arg and arg[0] and arg[0]:match("(.-)[^/\\]*$") or "./"

--------------------------------------------------------------------------------
-- Matrix Shim
--------------------------------------------------------------------------------
local matrixLib = {}
function matrixLib.filled(val, w, h)
    local data = {}
    for y = 1, h do
        data[y] = {}
        for x = 1, w do data[y][x] = val end
    end
    local mt = {}; mt.__index = mt
    function mt:get(x, y, default)
        if x < 1 or y < 1 or x > w or y > h then return default or "0" end
        return data[y][x] or default or "0"
    end
    function mt:set(x, y, v)
        if x >= 1 and y >= 1 and x <= w and y <= h then data[y][x] = v end
    end
    function mt:size() return w, h end
    return setmetatable({}, mt)
end

--------------------------------------------------------------------------------
-- Load PCG libraries
--------------------------------------------------------------------------------
local binEncoder = dofile(scriptDir .. "../Loenn/libraries/pcg/bin_encoder.lua")
local mapBuilder = dofile(scriptDir .. "../Loenn/libraries/pcg/map_builder.lua")

--------------------------------------------------------------------------------
-- Room Builder Helpers
--------------------------------------------------------------------------------
local ROOM_W = 60   -- tiles
local ROOM_H = 34   -- tiles
local ROOM_PX_W = ROOM_W * 8  -- 480
local ROOM_PX_H = ROOM_H * 8  -- 272

local nextEntityId = 1
local function eid()
    local id = nextEntityId
    nextEntityId = nextEntityId + 1
    return id
end

--- Build a showcase room matrix: walls on edges, flat floor at bottom, open interior.
--- Includes small platforms for entity placement.
local function makeShowcaseMatrix(tilesW, tilesH)
    local m = matrixLib.filled("0", tilesW, tilesH)
    -- Top & bottom walls
    for x = 1, tilesW do
        m:set(x, 1, "1")
        m:set(x, 2, "1")
        m:set(x, tilesH, "1")
        m:set(x, tilesH - 1, "1")
    end
    -- Left & right walls
    for y = 1, tilesH do
        m:set(1, y, "1")
        m:set(2, y, "1")
        m:set(tilesW, y, "1")
        m:set(tilesW - 1, y, "1")
    end
    -- Floor platform at y = tilesH - 4
    for x = 3, tilesW - 2 do
        m:set(x, tilesH - 3, "1")
        m:set(x, tilesH - 2, "1")
    end
    return m
end

--- Build a room with entities and triggers.
local function buildRoom(index, name, entities, triggers, style)
    local gridX = (index - 1) % 4
    local gridY = math.floor((index - 1) / 4)
    local m = makeShowcaseMatrix(ROOM_W, ROOM_H)
    local roomData = {
        matrix = m,
        entities = entities or {},
        width = ROOM_W,
        height = ROOM_H,
        x = gridX * ROOM_PX_W,
        y = gridY * ROOM_PX_H,
        name = name,
        roomStyle = style or "normal",
    }
    return roomData
end

--- Create a sign NPC entity at position to show the room description.
local function makeSign(x, y, dialogId)
    return {
        _name = "MaggyHelper/InteractiveSign",
        id = eid(), x = x, y = y,
        width = 16, height = 16,
        dialogId = dialogId,
        signTexture = "objects/MaggyHelper/interactive_sign/sign",
    }
end

--- Creates a vanilla player spawn entity.
local function makePlayer(x, y)
    return { _name = "player", id = eid(), x = x, y = y, width = 0, height = 0 }
end

--------------------------------------------------------------------------------
-- ENTITY DEFINITIONS — Every entity grouped by theme
--------------------------------------------------------------------------------
local rooms = {}

-- ============================================================================
-- ROOM 1: Spawn & Basics
-- ============================================================================
do
    local e = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_01"))
    -- KirbySpawnPoint
    table.insert(e, { _name = "MaggyHelper/KirbySpawnPoint", id = eid(),
        x = 100, y = 200, spawnAsKirby = true, startingAbility = "None" })
    -- KirbyPlayer
    table.insert(e, { _name = "MaggyHelper/KirbyPlayer", id = eid(),
        x = 140, y = 200, facing = 1 })
    -- SampleEntity
    table.insert(e, { _name = "MaggyHelper/SampleEntity", id = eid(),
        x = 180, y = 200, sampleProperty = 0 })
    table.insert(rooms, buildRoom(1, "01_spawn_basics", e, {}))
end

-- ============================================================================
-- ROOM 2: Kirby Enemies (Small) — 13 variants
-- ============================================================================
do
    local e = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_02"))
    local variants = {
        {name="Waddle Dee",   variant=0,  powerType=0,  hp=1},
        {name="Waddle Doo",   variant=1,  powerType=1,  hp=2},
        {name="Hot Head",     variant=2,  powerType=2,  hp=2},
        {name="Chilly",       variant=3,  powerType=3,  hp=2},
        {name="Sparky",       variant=4,  powerType=4,  hp=2},
        {name="Rocky",        variant=5,  powerType=5,  hp=2},
        {name="Sir Kibble",   variant=6,  powerType=6,  hp=2},
        {name="Poppy",        variant=7,  powerType=7,  hp=2},
        {name="Wheelie",      variant=8,  powerType=8,  hp=2},
        {name="Gordo",        variant=9,  powerType=0,  hp=99},
        {name="Biospark",     variant=10, powerType=9,  hp=2},
        {name="Bonkers",      variant=11, powerType=10, hp=3},
        {name="Mirror",       variant=12, powerType=11, hp=2},
    }
    for i, v in ipairs(variants) do
        local px = 80 + ((i - 1) % 7) * 50
        local py = 160 + math.floor((i - 1) / 7) * 40
        table.insert(e, { _name = "MaggyHelper/KirbySmallEnemy", id = eid(),
            x = px, y = py,
            variant = v.variant, powerType = v.powerType, maxHealth = v.hp,
            moveSpeed = 30.0, canBeInhaled = (v.variant ~= 9),
            detectionRange = 80.0, attackRange = 40.0,
            facingRight = true, patrolDistance = 48.0 })
    end
    table.insert(rooms, buildRoom(2, "02_small_enemies", e, {}))
end

-- ============================================================================
-- ROOM 3: Advanced Enemies
-- ============================================================================
do
    local e = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_03"))
    local startX = 100
    local spacing = 40
    -- WaddleDee standalone
    table.insert(e, { _name = "MaggyHelper/WaddleDee", id = eid(),
        x = startX, y = 200, health = 1, moveSpeed = 30.0,
        patrolDistance = 50.0, canBeInhaled = true })
    -- WaddleDoo standalone
    table.insert(e, { _name = "MaggyHelper/WaddleDoo", id = eid(),
        x = startX + spacing, y = 200, health = 2, moveSpeed = 25.0,
        attackCooldown = 3.0, canBeInhaled = true })
    -- Gordo standalone
    table.insert(e, { _name = "MaggyHelper/Gordo", id = eid(),
        x = startX + spacing*2, y = 160,
        movementType = "Stationary", moveDistance = 48.0,
        moveSpeed = 40.0, pauseDuration = 0.5 })
    -- ScarfyEnemy
    table.insert(e, { _name = "MaggyHelper/ScarfyEnemy", id = eid(),
        x = startX + spacing*3, y = 200, health = 2,
        moveSpeed = 20.0, chaseSpeed = 100.0, canBeInhaled = false })
    -- BombWaddleDee
    table.insert(e, { _name = "MaggyHelper/BombWaddleDee", id = eid(),
        x = startX + spacing*4, y = 200, health = 1,
        throwInterval = 3.0, throwRange = 80.0, bombSpeed = 120.0 })
    -- DarkMatterMinion
    table.insert(e, { _name = "MaggyHelper/DarkMatterMinion", id = eid(),
        x = startX + spacing*5, y = 200, health = 2,
        fireInterval = 2.0, beamLength = 60.0 })
    -- ShieldEnemy
    table.insert(e, { _name = "MaggyHelper/ShieldEnemy", id = eid(),
        x = startX + spacing*6, y = 200, health = 2,
        speed = 30.0, shieldHealth = 3 })
    -- MirrorEnemy
    table.insert(e, { _name = "MaggyHelper/MirrorEnemy", id = eid(),
        x = 100, y = 140, health = 2, reflectRadius = 40.0 })
    -- SplittingEnemy
    table.insert(e, { _name = "MaggyHelper/SplittingEnemy", id = eid(),
        x = 140, y = 140, health = 3, splitCount = 2,
        isSmall = false, speed = 30.0 })
    -- BurrowingEnemy
    table.insert(e, { _name = "MaggyHelper/BurrowingEnemy", id = eid(),
        x = 180, y = 200, health = 1, detectionRange = 80.0,
        surfaceTime = 2.0, burrowTime = 3.0 })
    -- GhostEnemy
    table.insert(e, { _name = "MaggyHelper/GhostEnemy", id = eid(),
        x = 220, y = 140, health = 2, chaseSpeed = 40.0 })
    -- CloneEnemy
    table.insert(e, { _name = "MaggyHelper/CloneEnemy", id = eid(),
        x = 260, y = 140, health = 1, delaySeconds = 2.0,
        color = "FF4444" })
    -- SwarmEnemy
    table.insert(e, { _name = "MaggyHelper/SwarmEnemy", id = eid(),
        x = 300, y = 140, count = 5, chaseRange = 80.0 })
    -- ElectricEnemy
    table.insert(e, { _name = "MaggyHelper/ElectricEnemy", id = eid(),
        x = 340, y = 200, health = 2, chargeTime = 1.5,
        shockRadius = 40.0, shockSpeed = 60.0 })
    -- MagnetEnemy
    table.insert(e, { _name = "MaggyHelper/MagnetEnemy", id = eid(),
        x = 380, y = 200, health = 2, pullStrength = 30.0,
        pullRange = 60.0 })
    -- HealerEnemy
    table.insert(e, { _name = "MaggyHelper/HealerEnemy", id = eid(),
        x = 340, y = 140, health = 1, healRange = 80.0, healRate = 1.0 })
    -- PhantomKnight
    table.insert(e, { _name = "MaggyHelper/PhantomKnight", id = eid(),
        x = 380, y = 140, health = 3, hiddenTime = 2.0,
        attackTime = 1.0, slashRange = 30.0 })
    table.insert(rooms, buildRoom(3, "03_advanced_enemies", e, {}))
end

-- ============================================================================
-- ROOM 4: Kirby Bosses
-- ============================================================================
do
    local e = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_04"))
    -- KirbyBoss
    table.insert(e, { _name = "MaggyHelper/KirbyBoss", id = eid(),
        x = 120, y = 160, health = 15, attackCooldown = 2.0,
        bossMusic = "event:/desolozantas/music/miniboss/main0" })
    -- MetaKnightBoss
    table.insert(e, { _name = "MaggyHelper/MetaKnightBoss", id = eid(),
        x = 200, y = 160, health = 20, attackCooldown = 0.8,
        bossMusic = "event:/desolozantas/music/lvl13/metarminator_kight" })
    -- WhispyWoodsBoss
    table.insert(e, { _name = "MaggyHelper/WhispyWoodsBoss", id = eid(),
        x = 280, y = 160, patternIndex = 1, cameraPastY = 120.0,
        dialog = false, startHit = false, cameraLockY = true,
        attackSequence = "",
        nodes = { {x=280, y=100} } })
    -- DededeBoss
    table.insert(e, { _name = "MaggyHelper/DededeBoss", id = eid(),
        x = 360, y = 160, health = 25, attackCooldown = 1.5,
        bossMusic = "event:/music/lvl9/main" })
    -- KirbyMidBoss — Mr. Frosty variant
    table.insert(e, { _name = "MaggyHelper/KirbyMidBoss", id = eid(),
        x = 120, y = 100, bossType = 2, maxHealth = 10,
        arenaWidth = 160, arenaHeight = 120, powerType = 3,
        canBeInhaled = true })
    -- KirbyMidBoss — Bonkers variant
    table.insert(e, { _name = "MaggyHelper/KirbyMidBoss", id = eid(),
        x = 280, y = 100, bossType = 3, maxHealth = 12,
        arenaWidth = 160, arenaHeight = 120, powerType = 10,
        canBeInhaled = true })
    table.insert(rooms, buildRoom(4, "04_bosses", e, {}))
end

-- ============================================================================
-- ROOM 5: Kirby Food & Items
-- ============================================================================
do
    local e = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_05"))
    local foodTypes = {
        {ft=0, name="Apple",         heal=1},
        {ft=1, name="Max Tomato",    heal=99},
        {ft=2, name="Meat",          heal=3},
        {ft=3, name="Cake",          heal=4},
        {ft=4, name="Cherry Bunch",  heal=2},
        {ft=5, name="Invincibility Star", heal=0},
        {ft=6, name="1-Up",          heal=0},
        {ft=10, name="Pep Brew",     heal=5},
        {ft=15, name="Lollipop",     heal=0},
        {ft=20, name="Point Star",   heal=0},
    }
    for i, f in ipairs(foodTypes) do
        local px = 100 + ((i - 1) % 5) * 60
        local py = 160 + math.floor((i - 1) / 5) * 40
        table.insert(e, { _name = "MaggyHelper/KirbyFood", id = eid(),
            x = px, y = py, foodType = f.ft, healAmount = f.heal,
            isDamaging = false, despawnTime = 30.0, isFalling = false })
    end
    -- PopstarBerry
    table.insert(e, { _name = "MaggyHelper/PopstarBerry", id = eid(),
        x = 380, y = 160, collectSound = "Original",
        customCollectSound = "", levelSet = "Maggy/Main/ZFinalChapter",
        maps = "", requires = "" })
    table.insert(rooms, buildRoom(5, "05_food_items", e, {}))
end

-- ============================================================================
-- ROOM 6: Ability Stars (All 10 main types)
-- ============================================================================
do
    local e = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_06"))
    local abilities = {"Fire", "Ice", "Sword", "Beam", "Spark", "Stone",
                       "Bomb", "Hammer", "Ninja", "Cutter", "Wheel",
                       "Fighter", "Mirror", "Sleep", "Parasol"}
    for i, ab in ipairs(abilities) do
        local px = 80 + ((i - 1) % 5) * 70
        local py = 120 + math.floor((i - 1) / 5) * 40
        table.insert(e, { _name = "MaggyHelper/AbilityStar", id = eid(),
            x = px, y = py, ability = ab })
    end
    table.insert(rooms, buildRoom(6, "06_ability_stars", e, {}))
end

-- ============================================================================
-- ROOM 7: Movement Entities (WarpStar, StarBubble, StarBlock, etc.)
-- ============================================================================
do
    local e = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_07"))
    -- WarpStar
    table.insert(e, { _name = "MaggyHelper/WarpStar", id = eid(),
        x = 100, y = 160, oneUse = false, shielded = false,
        isKirbyWarpStar = true })
    -- WarpStar (one-use)
    table.insert(e, { _name = "MaggyHelper/WarpStar", id = eid(),
        x = 140, y = 160, oneUse = true, shielded = false,
        isKirbyWarpStar = true })
    -- StarBubble
    table.insert(e, { _name = "DesoloZantas/StarBubble", id = eid(),
        x = 180, y = 140, duration = 5.0, floatSpeed = 80.0,
        immuneToHazards = true, respawnTime = 3.0 })
    -- StarBlock
    table.insert(e, { _name = "MaggyHelper/StarBlock", id = eid(),
        x = 230, y = 200, width = 16, height = 16 })
    -- StarJumpBlock
    table.insert(e, { _name = "MaggyHelper/StarJumpBlock", id = eid(),
        x = 260, y = 200, width = 24, height = 8, sinks = true })
    -- GravityFlipPlatform
    table.insert(e, { _name = "MaggyHelper/GravityFlipPlatform", id = eid(),
        x = 300, y = 200, width = 32, height = 8,
        cooldown = 2.0, togglable = true })
    -- SpringCloud
    table.insert(e, { _name = "MaggyHelper/SpringCloud", id = eid(),
        x = 350, y = 200, width = 24, respawnTime = 3.0,
        extraHeight = 0.0 })
    -- LaunchCannon
    table.insert(e, { _name = "MaggyHelper/LaunchCannon", id = eid(),
        x = 400, y = 200, launchSpeed = 400.0,
        autoFire = false, autoAngle = 0.0 })
    table.insert(rooms, buildRoom(7, "07_movement_stars", e, {}))
end

-- ============================================================================
-- ROOM 8: Wind & Flow Entities
-- ============================================================================
do
    local e = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_08"))
    -- WindTunnel (up)
    table.insert(e, { _name = "MaggyHelper/WindTunnel", id = eid(),
        x = 100, y = 100, width = 32, height = 96,
        direction = "Up", strength = 200.0, affectsKirbyMore = true })
    -- WindTunnel (right)
    table.insert(e, { _name = "MaggyHelper/WindTunnel", id = eid(),
        x = 160, y = 120, width = 80, height = 32,
        direction = "Right", strength = 150.0, affectsKirbyMore = true })
    -- ConveyorBelt (right)
    table.insert(e, { _name = "MaggyHelper/ConveyorBelt", id = eid(),
        x = 260, y = 200, width = 64, speed = 60.0, moveRight = true })
    -- ConveyorBelt (left, fast)
    table.insert(e, { _name = "MaggyHelper/ConveyorBelt", id = eid(),
        x = 340, y = 200, width = 64, speed = 120.0, moveRight = false })
    -- RainbowBridge
    table.insert(e, { _name = "MaggyHelper/RainbowBridge", id = eid(),
        x = 160, y = 200, width = 80, height = 8,
        speedThreshold = 20.0 })
    -- BubbleRaft
    table.insert(e, { _name = "MaggyHelper/BubbleRaft", id = eid(),
        x = 420, y = 140, duration = 8.0, floatSpeed = 20.0 })
    table.insert(rooms, buildRoom(8, "08_wind_flow", e, {}))
end

-- ============================================================================
-- ROOM 9: Puzzle Entities
-- ============================================================================
do
    local e = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_09"))
    -- PortalDoor A
    table.insert(e, { _name = "MaggyHelper/PortalDoor", id = eid(),
        x = 100, y = 180, portalId = "portal_A", color = "4488FF",
        nodes = { {x=300, y=180} } })
    -- PortalDoor B
    table.insert(e, { _name = "MaggyHelper/PortalDoor", id = eid(),
        x = 300, y = 180, portalId = "portal_B", color = "FF8844",
        nodes = { {x=100, y=180} } })
    -- StickyWall
    table.insert(e, { _name = "MaggyHelper/StickyWall", id = eid(),
        x = 160, y = 140, width = 8, height = 48,
        stickDuration = 5.0, infiniteStick = false })
    -- IcePlatform
    table.insert(e, { _name = "MaggyHelper/IcePlatform", id = eid(),
        x = 200, y = 200, width = 48, friction = 0.95, canMelt = true })
    -- MagnetRail
    table.insert(e, { _name = "MaggyHelper/MagnetRail", id = eid(),
        x = 260, y = 120, speed = 80.0, color = "FFFF00",
        nodes = { {x=380, y=120} } })
    -- PhaseBlock
    table.insert(e, { _name = "MaggyHelper/PhaseBlock", id = eid(),
        x = 350, y = 200, width = 24, height = 24,
        phaseSpeed = 1.5, phaseOffset = 0.0 })
    -- WeightSwitch
    table.insert(e, { _name = "MaggyHelper/WeightSwitch", id = eid(),
        x = 400, y = 200, width = 16,
        requiredWeight = 1.0, flag = "weight_switch_test",
        persistent = false })
    -- ColorLens (Red)
    table.insert(e, { _name = "MaggyHelper/ColorLens", id = eid(),
        x = 100, y = 100, width = 24, height = 24, color = "FF0000" })
    -- ColorFilterBlock (Blue)
    table.insert(e, { _name = "MaggyHelper/ColorFilterBlock", id = eid(),
        x = 140, y = 100, width = 24, height = 24, color = "0000FF" })
    -- TimePlatform (past)
    table.insert(e, { _name = "MaggyHelper/TimePlatform", id = eid(),
        x = 200, y = 120, width = 32, height = 8,
        timeEra = "past", flagName = "time_state_future" })
    -- TimePlatform (future)
    table.insert(e, { _name = "MaggyHelper/TimePlatform", id = eid(),
        x = 250, y = 120, width = 32, height = 8,
        timeEra = "future", flagName = "time_state_future" })
    table.insert(rooms, buildRoom(9, "09_puzzle_entities", e, {}))
end

-- ============================================================================
-- ROOM 10: Kirby NPCs
-- ============================================================================
do
    local e = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_10"))
    local npcs = {
        {char=0, beh=0, name="Bandana Waddle Dee (Stationary)"},
        {char=1, beh=0, name="King Dedede (Friendly)"},
        {char=2, beh=0, name="Meta Knight (Friendly)"},
        {char=3, beh=0, name="Magolor"},
        {char=4, beh=4, name="Shop Keeper"},
        {char=5, beh=2, name="Companion (Follow)"},
        {char=0, beh=3, name="Patrol NPC"},
    }
    for i, npc in ipairs(npcs) do
        local px = 80 + (i - 1) * 55
        table.insert(e, { _name = "MaggyHelper/KirbyNPC", id = eid(),
            x = px, y = 200, character = npc.char, behavior = npc.beh,
            dialogId = "KIRBY_NPC_TEST_" .. i,
            canGiveItem = false, giveItemId = "",
            followDistance = 48, moveSpeed = 40.0 })
    end
    table.insert(rooms, buildRoom(10, "10_kirby_npcs", e, {}))
end

-- ============================================================================
-- ROOM 11: Gordo Movement Patterns
-- ============================================================================
do
    local e = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_11"))
    local patterns = {"Stationary","Horizontal","Vertical","Diagonal","Circular"}
    for i, pat in ipairs(patterns) do
        table.insert(e, { _name = "MaggyHelper/Gordo", id = eid(),
            x = 80 + (i - 1) * 70, y = 140,
            movementType = pat, moveDistance = 48.0,
            moveSpeed = 40.0, pauseDuration = 0.5 })
    end
    table.insert(rooms, buildRoom(11, "11_gordo_patterns", e, {}))
end

-- ============================================================================
-- ROOM 12: KirbyMidBoss Variants
-- ============================================================================
do
    local e = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_12"))
    local bosses = {
        {bt=0, name="Whispy Woods", pow=0},
        {bt=1, name="Kracko", pow=4},
        {bt=2, name="Mr. Frosty", pow=3},
        {bt=3, name="Bonkers", pow=10},
        {bt=4, name="Bugzzy", pow=0},
        {bt=5, name="Fire Lion", pow=2},
        {bt=6, name="Iron Mam", pow=0},
        {bt=7, name="Grand Wheely", pow=8},
        {bt=8, name="Box Boxer", pow=0},
        {bt=9, name="Master Hand", pow=0},
    }
    for i, b in ipairs(bosses) do
        local px = 60 + ((i - 1) % 5) * 80
        local py = 120 + math.floor((i - 1) / 5) * 80
        table.insert(e, { _name = "MaggyHelper/KirbyMidBoss", id = eid(),
            x = px, y = py, bossType = b.bt, maxHealth = 10,
            arenaWidth = 160, arenaHeight = 120,
            powerType = b.pow, canBeInhaled = true })
    end
    table.insert(rooms, buildRoom(12, "12_midboss_all", e, {}))
end

-- ============================================================================
-- ROOMS 13–15: PLAYER STATE TRIGGERS
-- ============================================================================

-- ROOM 13: Kirby Ability & Player Triggers
do
    local e = {}
    local t = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_13"))
    -- KirbyAbilityTrigger — give fire
    table.insert(t, { _name = "MaggyHelper/KirbyAbilityTrigger", id = eid(),
        x = 100, y = 160, width = 32, height = 32,
        action = "Give", ability = "Fire", onlyOnce = false })
    -- KirbyAbilityTrigger — give ice
    table.insert(t, { _name = "MaggyHelper/KirbyAbilityTrigger", id = eid(),
        x = 150, y = 160, width = 32, height = 32,
        action = "Give", ability = "Ice", onlyOnce = false })
    -- KirbyAbilityTrigger — remove ability
    table.insert(t, { _name = "MaggyHelper/KirbyAbilityTrigger", id = eid(),
        x = 200, y = 160, width = 32, height = 32,
        action = "Remove", ability = "None", onlyOnce = false })
    -- KirbyAbilityTrigger — toggle float
    table.insert(t, { _name = "MaggyHelper/KirbyAbilityTrigger", id = eid(),
        x = 250, y = 160, width = 32, height = 32,
        action = "ToggleFloat", ability = "None", onlyOnce = false })
    -- Kirby_Player_Trigger — on enter
    table.insert(t, { _name = "MaggyHelper/Kirby_Player_Trigger", id = eid(),
        x = 310, y = 160, width = 32, height = 32,
        activationType = "OnEnter", transformationType = "Instant",
        oneUse = false, transformAnimation = true,
        transformDuration = 0.5, preserveVelocity = true,
        requiredFlag = "", playSound = true, initialPower = "Sword" })
    -- Kirby_Mode_Toggle_Trigger
    table.insert(t, { _name = "MaggyHelper/Kirby_Mode_Toggle_Trigger", id = eid(),
        x = 370, y = 160, width = 32, height = 32,
        activationMode = "OnEnter", transformEffect = "Sparkle",
        triggerState = "Toggle", oneUse = false,
        respectSettings = true, silentMode = false, flagRequired = "" })
    -- AbilitySwapTrigger
    table.insert(t, { _name = "MaggyHelper/AbilitySwapTrigger", id = eid(),
        x = 100, y = 100, width = 32, height = 32,
        abilityName = "Sword", onlyOnce = false })
    table.insert(rooms, buildRoom(13, "13_ability_triggers", e, t))
end

-- ROOM 14: Player Modifier Triggers
do
    local e = {}
    local t = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_14"))
    -- GravityZoneTrigger — low gravity
    table.insert(t, { _name = "MaggyHelper/GravityZoneTrigger", id = eid(),
        x = 80, y = 120, width = 48, height = 80,
        gravityDirection = "Up", gravityStrength = 100.0 })
    -- SpeedModifierTrigger — slow
    table.insert(t, { _name = "MaggyHelper/SpeedModifierTrigger", id = eid(),
        x = 140, y = 160, width = 48, height = 48,
        speedMultiplier = 0.5, affectsX = true, affectsY = true })
    -- SpeedModifierTrigger — fast
    table.insert(t, { _name = "MaggyHelper/SpeedModifierTrigger", id = eid(),
        x = 200, y = 160, width = 48, height = 48,
        speedMultiplier = 2.0, affectsX = true, affectsY = true })
    -- InvincibilityTrigger
    table.insert(t, { _name = "MaggyHelper/InvincibilityTrigger", id = eid(),
        x = 260, y = 160, width = 32, height = 48,
        duration = 5.0, flashColor = "FFD700" })
    -- SizeChangeTrigger — shrink
    table.insert(t, { _name = "MaggyHelper/SizeChangeTrigger", id = eid(),
        x = 310, y = 160, width = 32, height = 48,
        scaleFactor = 0.5 })
    -- SizeChangeTrigger — grow
    table.insert(t, { _name = "MaggyHelper/SizeChangeTrigger", id = eid(),
        x = 360, y = 160, width = 32, height = 48,
        scaleFactor = 2.0 })
    -- DisableAbilityTrigger — no dash
    table.insert(t, { _name = "MaggyHelper/DisableAbilityTrigger", id = eid(),
        x = 80, y = 80, width = 48, height = 48,
        disableDash = true, disableGrab = false, disableJump = false })
    -- StaminaModTrigger — infinite
    table.insert(t, { _name = "MaggyHelper/StaminaModTrigger", id = eid(),
        x = 140, y = 80, width = 48, height = 48,
        staminaMultiplier = 999.0, regenMultiplier = 1.0 })
    -- MirrorModeTrigger — horizontal
    table.insert(t, { _name = "MaggyHelper/MirrorModeTrigger", id = eid(),
        x = 200, y = 80, width = 48, height = 48,
        mirrorX = true, mirrorY = false })
    -- OneHitTrigger
    table.insert(t, { _name = "MaggyHelper/OneHitTrigger", id = eid(),
        x = 260, y = 80, width = 48, height = 48,
        flag = "one_hit_mode" })
    -- DashRefreshTrigger
    table.insert(t, { _name = "MaggyHelper/DashRefreshTrigger", id = eid(),
        x = 320, y = 80, width = 32, height = 48,
        dashCount = 3, onlyOnce = false })
    table.insert(rooms, buildRoom(14, "14_player_modifiers", e, t))
end

-- ============================================================================
-- ROOM 15: Camera & Visual Triggers
-- ============================================================================
do
    local e = {}
    local t = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_15"))
    -- CameraShakeTrigger — light
    table.insert(t, { _name = "MaggyHelper/CameraShakeTrigger", id = eid(),
        x = 80, y = 160, width = 32, height = 32,
        intensity = 0.3, duration = 0.5, direction = "Both" })
    -- CameraShakeTrigger — heavy
    table.insert(t, { _name = "MaggyHelper/CameraShakeTrigger", id = eid(),
        x = 120, y = 160, width = 32, height = 32,
        intensity = 1.0, duration = 1.0, direction = "Vertical" })
    -- ZoomTrigger — in
    table.insert(t, { _name = "MaggyHelper/ZoomTrigger", id = eid(),
        x = 170, y = 160, width = 32, height = 32,
        targetZoom = 2.0, zoomDuration = 0.5 })
    -- ZoomTrigger — out
    table.insert(t, { _name = "MaggyHelper/ZoomTrigger", id = eid(),
        x = 210, y = 160, width = 32, height = 32,
        targetZoom = 0.5, zoomDuration = 0.5 })
    -- ScreenFlashTrigger — white
    table.insert(t, { _name = "MaggyHelper/ScreenFlashTrigger", id = eid(),
        x = 260, y = 160, width = 32, height = 32,
        color = "FFFFFF", duration = 0.3, onlyOnce = false })
    -- ScreenFlashTrigger — red
    table.insert(t, { _name = "MaggyHelper/ScreenFlashTrigger", id = eid(),
        x = 300, y = 160, width = 32, height = 32,
        color = "FF0000", duration = 0.5, onlyOnce = false })
    -- PixelationTrigger — retro
    table.insert(t, { _name = "MaggyHelper/PixelationTrigger", id = eid(),
        x = 350, y = 160, width = 32, height = 32,
        pixelSize = 4, transitionDuration = 0.3 })
    -- VignetteTrigger
    table.insert(t, { _name = "MaggyHelper/VignetteTrigger", id = eid(),
        x = 400, y = 160, width = 32, height = 32,
        vignetteStrength = 0.7, vignetteColor = "000000" })
    -- ColorShiftTrigger — sepia
    table.insert(t, { _name = "MaggyHelper/ColorShiftTrigger", id = eid(),
        x = 80, y = 100, width = 48, height = 32,
        targetColor = "C8A060", blendStrength = 0.5,
        transitionDuration = 0.5 })
    -- ParallaxShiftTrigger
    table.insert(t, { _name = "MaggyHelper/ParallaxShiftTrigger", id = eid(),
        x = 140, y = 100, width = 48, height = 32,
        parallaxX = 0.5, parallaxY = 0.5, duration = 1.0 })
    -- SplitScreenTrigger — horizontal
    table.insert(t, { _name = "MaggyHelper/SplitScreenTrigger", id = eid(),
        x = 200, y = 100, width = 48, height = 32,
        splitDirection = "Horizontal", splitRatio = 0.5 })
    -- WeatherChangeTrigger — rain
    table.insert(t, { _name = "MaggyHelper/WeatherChangeTrigger", id = eid(),
        x = 260, y = 100, width = 48, height = 32,
        weatherType = "Rain", intensity = 0.7,
        transitionDuration = 1.0 })
    -- WeatherChangeTrigger — snow
    table.insert(t, { _name = "MaggyHelper/WeatherChangeTrigger", id = eid(),
        x = 320, y = 100, width = 48, height = 32,
        weatherType = "Snow", intensity = 0.5,
        transitionDuration = 1.0 })
    table.insert(rooms, buildRoom(15, "15_camera_visual", e, t))
end

-- ============================================================================
-- ROOM 16: Combat & Spawn Triggers
-- ============================================================================
do
    local e = {}
    local t = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_16"))
    -- EnemySpawnTrigger — waddle dee
    table.insert(t, { _name = "MaggyHelper/EnemySpawnTrigger", id = eid(),
        x = 100, y = 160, width = 40, height = 40,
        enemyType = "WaddleDee", count = 3, spawnDelay = 0.0,
        respawn = false })
    -- EnemySpawnTrigger — gordo
    table.insert(t, { _name = "MaggyHelper/EnemySpawnTrigger", id = eid(),
        x = 160, y = 160, width = 40, height = 40,
        enemyType = "Gordo", count = 1, spawnDelay = 1.0,
        respawn = false })
    -- EnemyWaveTrigger
    table.insert(t, { _name = "MaggyHelper/EnemyWaveTrigger", id = eid(),
        x = 220, y = 140, width = 64, height = 64,
        waveCount = 3, enemiesPerWave = 3,
        spawnDelay = 2.0, flag = "" })
    -- AmbushTrigger
    table.insert(t, { _name = "MaggyHelper/AmbushTrigger", id = eid(),
        x = 310, y = 140, width = 64, height = 64,
        enemyCount = 4, lockCamera = true, flag = "" })
    -- BossIntroTrigger
    table.insert(t, { _name = "MaggyHelper/BossIntroTrigger", id = eid(),
        x = 400, y = 160, width = 40, height = 40,
        bossName = "Test Boss", dialogId = "KIRBY_TEST_BOSS_INTRO",
        musicEvent = "", flag = "" })
    table.insert(rooms, buildRoom(16, "16_combat_spawn", e, t))
end

-- ============================================================================
-- ROOM 17: Health & Damage Triggers
-- ============================================================================
do
    local e = {}
    local t = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_17"))
    -- HealthSystemTrigger
    table.insert(t, { _name = "MaggyHelper/HealthSystemTrigger", id = eid(),
        x = 80, y = 100, width = 360, height = 120,
        maxHP = 6, kirbyMode = true, showUI = true,
        persistent = true, displayMode = 0, trackBosses = true,
        healOnEnter = false, healAmount = 0 })
    -- HealTrigger
    table.insert(t, { _name = "MaggyHelper/HealTrigger", id = eid(),
        x = 100, y = 160, width = 40, height = 40,
        healAmount = 2, fullHeal = false,
        removeAfterUse = true, onlyOnce = true })
    -- HealTrigger — full heal
    table.insert(t, { _name = "MaggyHelper/HealTrigger", id = eid(),
        x = 160, y = 160, width = 40, height = 40,
        healAmount = 0, fullHeal = true,
        removeAfterUse = true, onlyOnce = true })
    -- DamageTrigger
    table.insert(t, { _name = "MaggyHelper/DamageTrigger", id = eid(),
        x = 240, y = 160, width = 40, height = 40,
        damage = 1, cooldown = 1.0, removeAfterHit = false })
    -- DamageTrigger — one-time
    table.insert(t, { _name = "MaggyHelper/DamageTrigger", id = eid(),
        x = 300, y = 160, width = 40, height = 40,
        damage = 2, cooldown = 0.0, removeAfterHit = true })
    table.insert(rooms, buildRoom(17, "17_health_damage", e, t))
end

-- ============================================================================
-- ROOM 18: Timer, Checkpoint & Teleport Triggers
-- ============================================================================
do
    local e = {}
    local t = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_18"))
    -- TimerStartTrigger
    table.insert(t, { _name = "MaggyHelper/TimerStartTrigger", id = eid(),
        x = 100, y = 160, width = 32, height = 32,
        timerId = "timer_test", duration = 60.0,
        showDisplay = true })
    -- TimerStopTrigger
    table.insert(t, { _name = "MaggyHelper/TimerStopTrigger", id = eid(),
        x = 160, y = 160, width = 32, height = 32,
        timerId = "timer_test" })
    -- CheckpointTrigger
    table.insert(t, { _name = "MaggyHelper/CheckpointTrigger", id = eid(),
        x = 220, y = 160, width = 32, height = 32,
        checkpointId = "cp_test_1", showEffect = true })
    -- TeleportTrigger
    table.insert(t, { _name = "MaggyHelper/TeleportTrigger", id = eid(),
        x = 290, y = 160, width = 32, height = 32,
        targetRoom = "lvl_01_spawn_basics", targetX = 40,
        targetY = 200, showEffect = true })
    -- CountdownEscapeTrigger
    table.insert(t, { _name = "MaggyHelper/CountdownEscapeTrigger", id = eid(),
        x = 360, y = 140, width = 48, height = 64,
        countdown = 30.0, targetRoom = "lvl_01_spawn_basics",
        warningTime = 10.0, onlyOnce = true })
    table.insert(rooms, buildRoom(18, "18_timer_checkpoint", e, t))
end

-- ============================================================================
-- ROOM 19: Narrative & Audio Triggers
-- ============================================================================
do
    local e = {}
    local t = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_19"))
    -- MusicLayerTrigger
    table.insert(t, { _name = "MaggyHelper/MusicLayerTrigger", id = eid(),
        x = 100, y = 160, width = 32, height = 32,
        layerIndex = 1, enabled = true, fade = true })
    -- NarratorTrigger
    table.insert(t, { _name = "MaggyHelper/NarratorTrigger", id = eid(),
        x = 160, y = 140, width = 48, height = 48,
        dialogId = "KIRBY_TEST_NARRATOR", duration = 3.0,
        position = "Top", onlyOnce = true })
    -- FlashbackTrigger
    table.insert(t, { _name = "MaggyHelper/FlashbackTrigger", id = eid(),
        x = 230, y = 140, width = 48, height = 48,
        targetRoom = "lvl_01_spawn_basics",
        flashbackDuration = 5.0,
        dialogId = "KIRBY_TEST_FLASHBACK", onlyOnce = true })
    -- SecretRevealTrigger
    table.insert(t, { _name = "MaggyHelper/SecretRevealTrigger", id = eid(),
        x = 300, y = 160, width = 40, height = 40,
        flag = "secret_found_test",
        revealSound = "", cameraTarget = "" })
    -- CharacterSwapTrigger
    table.insert(t, { _name = "MaggyHelper/CharacterSwapTrigger", id = eid(),
        x = 370, y = 160, width = 32, height = 32,
        targetCharacter = "Kirby", onlyOnce = false })
    table.insert(rooms, buildRoom(19, "19_narrative_audio", e, t))
end

-- ============================================================================
-- ROOM 20: Sample / Misc Triggers
-- ============================================================================
do
    local e = {}
    local t = {}
    table.insert(e, makePlayer(40, 200))
    table.insert(e, makeSign(64, 200, "KIRBY_TEST_ROOM_20"))
    -- SampleTrigger
    table.insert(t, { _name = "MaggyHelper/SessionFlagTrigger", id = eid(),
        x = 100, y = 180, width = 32, height = 32,
        sessionFlag = "kirby_test_room_20_trigger", flagAction = "Toggle" })
    table.insert(rooms, buildRoom(20, "20_misc_sample", e, t))
end

--------------------------------------------------------------------------------
-- PATCH: inject triggers into room __children
--------------------------------------------------------------------------------
-- The mapBuilder.roomToLevel creates an empty triggers element.
-- We override it to include our triggers.
local origRoomToLevel = mapBuilder.roomToLevel
mapBuilder.roomToLevel = function(roomData, index)
    local level = origRoomToLevel(roomData, index)
    -- Find triggers child and populate it
    if roomData.triggers and #roomData.triggers > 0 then
        for _, child in ipairs(level.__children) do
            if child.__name == "triggers" then
                for _, trig in ipairs(roomData.triggers) do
                    table.insert(child.__children, mapBuilder.entityToElement(trig))
                end
                break
            end
        end
    end
    return level
end

--- Attach triggers to roomData before building
local allRoomData = {}
-- Room buildRoom returns roomData without triggers. Let's rebuild with triggers.
-- Actually we stored triggers in the build calls above.
-- We need to re-associate. Let's restructure:

-- The rooms table already has all room data from buildRoom(). We need to
-- attach the trigger lists. Let me refactor the approach: store triggers
-- on the roomData object directly.

-- Retroactive trigger injection:
-- Rooms 1–12 have no triggers. Rooms 13–20 have triggers passed to buildRoom.
-- But buildRoom ignores the trigger argument — let me fix that.

-- Actually, looking at the buildRoom function above, it takes (index, name, entities, triggers, style)
-- but never stores triggers. Let me patch that.

-- Re-associate triggers with rooms that have them:
local roomTriggerSets = {}

-- Build triggers lists matching room indices
-- Rooms 13-20 were created with trigger tables
-- We need to re-capture them. Since we already built rooms, let's re-attach.

-- Actually, looking more carefully at the code above, buildRoom doesn't receive triggers as an arg
-- in its body. Let me fix the buildRoom function and re-run.
-- SOLUTION: Just attach .triggers directly since we override roomToLevel.

-- The rooms were built via buildRoom() which returned roomData without triggers.
-- But the trigger tables were passed to buildRoom as the 4th arg, which was ignored.
-- We need to re-build. Instead, let's just attach triggers after the fact.
-- The buildRoom function call DID receive triggers, but the function ignores them.
-- Let's patch buildRoom to store triggers:

-- Since Lua is dynamic, we can just add the triggers field after creation:
-- Rooms 1-12: no triggers
-- Room 13: ability triggers
-- etc.

-- But we already created the rooms without storing the triggers separately.
-- Let me just re-create the problematic rooms.

-- SIMPLEST FIX: Override buildRoom to accept and store triggers
local function buildRoomWithTriggers(index, name, entities, triggers, style)
    local room = buildRoom(index, name, entities, {}, style)
    room.triggers = triggers or {}
    return room
end

-- Now re-build rooms 13-20 which have triggers
-- Actually, the rooms table already has the data. We just need to add triggers.
-- Since Lua tables are references, I can't easily recover the trigger tables
-- that were passed to buildRoom since the function didn't store them.

-- REAL FIX: Let me rebuild rooms 13-20 by simply setting .triggers on them
-- after initial construction. The trigger data IS in the room already or not.
-- Since buildRoom(13, ..., e, t) was called but t was not stored,
-- we need to reconstruct. But the trigger tables were local variables.

-- I'll use a different approach: just re-add the triggers to the rooms directly.
-- The problem is we lost the trigger references. Let me re-create them.

-- Actually wait — looking at the code more carefully, rooms 13-20 DO have
-- the triggers already. The issue is that `buildRoom` ignores the triggers
-- BUT we still have the `rooms` table. The triggers were passed as arg 4
-- which was unused. So the trigger data is LOST.

-- The cleanest fix is to recreate rooms 13-20. But that's a lot of repetition.
-- Instead, let me re-index and rebuild only the trigger data.

-- Let me take a different approach: rebuild ALL trigger data for rooms 13-20
-- and attach them to the rooms[] entries.

-- ROOM 13 triggers
rooms[13].triggers = {
    { _name = "MaggyHelper/KirbyAbilityTrigger", id = eid(),
        x = 100, y = 160, width = 32, height = 32,
        action = "Give", ability = "Fire", onlyOnce = false },
    { _name = "MaggyHelper/KirbyAbilityTrigger", id = eid(),
        x = 150, y = 160, width = 32, height = 32,
        action = "Give", ability = "Ice", onlyOnce = false },
    { _name = "MaggyHelper/KirbyAbilityTrigger", id = eid(),
        x = 200, y = 160, width = 32, height = 32,
        action = "Remove", ability = "None", onlyOnce = false },
    { _name = "MaggyHelper/KirbyAbilityTrigger", id = eid(),
        x = 250, y = 160, width = 32, height = 32,
        action = "ToggleFloat", ability = "None", onlyOnce = false },
    { _name = "MaggyHelper/Kirby_Player_Trigger", id = eid(),
        x = 310, y = 160, width = 32, height = 32,
        activationType = "OnEnter", transformationType = "Instant",
        oneUse = false, transformAnimation = true,
        transformDuration = 0.5, preserveVelocity = true,
        requiredFlag = "", playSound = true, initialPower = "Sword" },
    { _name = "MaggyHelper/Kirby_Mode_Toggle_Trigger", id = eid(),
        x = 370, y = 160, width = 32, height = 32,
        activationMode = "OnEnter", transformEffect = "Sparkle",
        triggerState = "Toggle", oneUse = false,
        respectSettings = true, silentMode = false, flagRequired = "" },
    { _name = "MaggyHelper/AbilitySwapTrigger", id = eid(),
        x = 100, y = 100, width = 32, height = 32,
        abilityName = "Sword", onlyOnce = false },
}

-- ROOM 14 triggers
rooms[14].triggers = {
    { _name = "MaggyHelper/GravityZoneTrigger", id = eid(),
        x = 80, y = 120, width = 48, height = 80,
        gravityDirection = "Up", gravityStrength = 100.0 },
    { _name = "MaggyHelper/SpeedModifierTrigger", id = eid(),
        x = 140, y = 160, width = 48, height = 48,
        speedMultiplier = 0.5, affectsX = true, affectsY = true },
    { _name = "MaggyHelper/SpeedModifierTrigger", id = eid(),
        x = 200, y = 160, width = 48, height = 48,
        speedMultiplier = 2.0, affectsX = true, affectsY = true },
    { _name = "MaggyHelper/InvincibilityTrigger", id = eid(),
        x = 260, y = 160, width = 32, height = 48,
        duration = 5.0, flashColor = "FFD700" },
    { _name = "MaggyHelper/SizeChangeTrigger", id = eid(),
        x = 310, y = 160, width = 32, height = 48,
        scaleFactor = 0.5 },
    { _name = "MaggyHelper/SizeChangeTrigger", id = eid(),
        x = 360, y = 160, width = 32, height = 48,
        scaleFactor = 2.0 },
    { _name = "MaggyHelper/DisableAbilityTrigger", id = eid(),
        x = 80, y = 80, width = 48, height = 48,
        disableDash = true, disableGrab = false, disableJump = false },
    { _name = "MaggyHelper/StaminaModTrigger", id = eid(),
        x = 140, y = 80, width = 48, height = 48,
        staminaMultiplier = 999.0, regenMultiplier = 1.0 },
    { _name = "MaggyHelper/MirrorModeTrigger", id = eid(),
        x = 200, y = 80, width = 48, height = 48,
        mirrorX = true, mirrorY = false },
    { _name = "MaggyHelper/OneHitTrigger", id = eid(),
        x = 260, y = 80, width = 48, height = 48,
        flag = "one_hit_mode" },
    { _name = "MaggyHelper/DashRefreshTrigger", id = eid(),
        x = 320, y = 80, width = 32, height = 48,
        dashCount = 3, onlyOnce = false },
}

-- ROOM 15 triggers
rooms[15].triggers = {
    { _name = "MaggyHelper/CameraShakeTrigger", id = eid(),
        x = 80, y = 160, width = 32, height = 32,
        intensity = 0.3, duration = 0.5, direction = "Both" },
    { _name = "MaggyHelper/CameraShakeTrigger", id = eid(),
        x = 120, y = 160, width = 32, height = 32,
        intensity = 1.0, duration = 1.0, direction = "Vertical" },
    { _name = "MaggyHelper/ZoomTrigger", id = eid(),
        x = 170, y = 160, width = 32, height = 32,
        targetZoom = 2.0, zoomDuration = 0.5 },
    { _name = "MaggyHelper/ZoomTrigger", id = eid(),
        x = 210, y = 160, width = 32, height = 32,
        targetZoom = 0.5, zoomDuration = 0.5 },
    { _name = "MaggyHelper/ScreenFlashTrigger", id = eid(),
        x = 260, y = 160, width = 32, height = 32,
        color = "FFFFFF", duration = 0.3, onlyOnce = false },
    { _name = "MaggyHelper/ScreenFlashTrigger", id = eid(),
        x = 300, y = 160, width = 32, height = 32,
        color = "FF0000", duration = 0.5, onlyOnce = false },
    { _name = "MaggyHelper/PixelationTrigger", id = eid(),
        x = 350, y = 160, width = 32, height = 32,
        pixelSize = 4, transitionDuration = 0.3 },
    { _name = "MaggyHelper/VignetteTrigger", id = eid(),
        x = 400, y = 160, width = 32, height = 32,
        vignetteStrength = 0.7, vignetteColor = "000000" },
    { _name = "MaggyHelper/ColorShiftTrigger", id = eid(),
        x = 80, y = 100, width = 48, height = 32,
        targetColor = "C8A060", blendStrength = 0.5,
        transitionDuration = 0.5 },
    { _name = "MaggyHelper/ParallaxShiftTrigger", id = eid(),
        x = 140, y = 100, width = 48, height = 32,
        parallaxX = 0.5, parallaxY = 0.5, duration = 1.0 },
    { _name = "MaggyHelper/SplitScreenTrigger", id = eid(),
        x = 200, y = 100, width = 48, height = 32,
        splitDirection = "Horizontal", splitRatio = 0.5 },
    { _name = "MaggyHelper/WeatherChangeTrigger", id = eid(),
        x = 260, y = 100, width = 48, height = 32,
        weatherType = "Rain", intensity = 0.7,
        transitionDuration = 1.0 },
    { _name = "MaggyHelper/WeatherChangeTrigger", id = eid(),
        x = 320, y = 100, width = 48, height = 32,
        weatherType = "Snow", intensity = 0.5,
        transitionDuration = 1.0 },
}

-- ROOM 16 triggers
rooms[16].triggers = {
    { _name = "MaggyHelper/EnemySpawnTrigger", id = eid(),
        x = 100, y = 160, width = 40, height = 40,
        enemyType = "WaddleDee", count = 3, spawnDelay = 0.0,
        respawn = false },
    { _name = "MaggyHelper/EnemySpawnTrigger", id = eid(),
        x = 160, y = 160, width = 40, height = 40,
        enemyType = "Gordo", count = 1, spawnDelay = 1.0,
        respawn = false },
    { _name = "MaggyHelper/EnemyWaveTrigger", id = eid(),
        x = 220, y = 140, width = 64, height = 64,
        waveCount = 3, enemiesPerWave = 3,
        spawnDelay = 2.0, flag = "" },
    { _name = "MaggyHelper/AmbushTrigger", id = eid(),
        x = 310, y = 140, width = 64, height = 64,
        enemyCount = 4, lockCamera = true, flag = "" },
    { _name = "MaggyHelper/BossIntroTrigger", id = eid(),
        x = 400, y = 160, width = 40, height = 40,
        bossName = "Test Boss", dialogId = "KIRBY_TEST_BOSS_INTRO",
        musicEvent = "", flag = "" },
}

-- ROOM 17 triggers
rooms[17].triggers = {
    { _name = "MaggyHelper/HealthSystemTrigger", id = eid(),
        x = 80, y = 100, width = 360, height = 120,
        maxHP = 6, kirbyMode = true, showUI = true,
        persistent = true, displayMode = 0, trackBosses = true,
        healOnEnter = false, healAmount = 0 },
    { _name = "MaggyHelper/HealTrigger", id = eid(),
        x = 100, y = 160, width = 40, height = 40,
        healAmount = 2, fullHeal = false,
        removeAfterUse = true, onlyOnce = true },
    { _name = "MaggyHelper/HealTrigger", id = eid(),
        x = 160, y = 160, width = 40, height = 40,
        healAmount = 0, fullHeal = true,
        removeAfterUse = true, onlyOnce = true },
    { _name = "MaggyHelper/DamageTrigger", id = eid(),
        x = 240, y = 160, width = 40, height = 40,
        damage = 1, cooldown = 1.0, removeAfterHit = false },
    { _name = "MaggyHelper/DamageTrigger", id = eid(),
        x = 300, y = 160, width = 40, height = 40,
        damage = 2, cooldown = 0.0, removeAfterHit = true },
}

-- ROOM 18 triggers
rooms[18].triggers = {
    { _name = "MaggyHelper/TimerStartTrigger", id = eid(),
        x = 100, y = 160, width = 32, height = 32,
        timerId = "timer_test", duration = 60.0,
        showDisplay = true },
    { _name = "MaggyHelper/TimerStopTrigger", id = eid(),
        x = 160, y = 160, width = 32, height = 32,
        timerId = "timer_test" },
    { _name = "MaggyHelper/CheckpointTrigger", id = eid(),
        x = 220, y = 160, width = 32, height = 32,
        checkpointId = "cp_test_1", showEffect = true },
    { _name = "MaggyHelper/TeleportTrigger", id = eid(),
        x = 290, y = 160, width = 32, height = 32,
        targetRoom = "lvl_01_spawn_basics", targetX = 40,
        targetY = 200, showEffect = true },
    { _name = "MaggyHelper/CountdownEscapeTrigger", id = eid(),
        x = 360, y = 140, width = 48, height = 64,
        countdown = 30.0, targetRoom = "lvl_01_spawn_basics",
        warningTime = 10.0, onlyOnce = true },
}

-- ROOM 19 triggers
rooms[19].triggers = {
    { _name = "MaggyHelper/MusicLayerTrigger", id = eid(),
        x = 100, y = 160, width = 32, height = 32,
        layerIndex = 1, enabled = true, fade = true },
    { _name = "MaggyHelper/NarratorTrigger", id = eid(),
        x = 160, y = 140, width = 48, height = 48,
        dialogId = "KIRBY_TEST_NARRATOR", duration = 3.0,
        position = "Top", onlyOnce = true },
    { _name = "MaggyHelper/FlashbackTrigger", id = eid(),
        x = 230, y = 140, width = 48, height = 48,
        targetRoom = "lvl_01_spawn_basics",
        flashbackDuration = 5.0,
        dialogId = "KIRBY_TEST_FLASHBACK", onlyOnce = true },
    { _name = "MaggyHelper/SecretRevealTrigger", id = eid(),
        x = 300, y = 160, width = 40, height = 40,
        flag = "secret_found_test",
        revealSound = "", cameraTarget = "" },
    { _name = "MaggyHelper/CharacterSwapTrigger", id = eid(),
        x = 370, y = 160, width = 32, height = 32,
        targetCharacter = "Kirby", onlyOnce = false },
}

-- ROOM 20 triggers
rooms[20].triggers = {
    { _name = "MaggyHelper/SessionFlagTrigger", id = eid(),
        x = 100, y = 180, width = 32, height = 32,
        sessionFlag = "kirby_test_room_20_trigger", flagAction = "Toggle" },
}

--------------------------------------------------------------------------------
-- BUILD & ENCODE
--------------------------------------------------------------------------------
print("=" .. string.rep("=", 60))
print("  MaggyHelper Kirby Test Map Generator (PCG)")
print("=" .. string.rep("=", 60))
print()
print(string.format("  Rooms: %d", #rooms))
print(string.format("  Room size: %dx%d tiles (%dx%d px)", ROOM_W, ROOM_H, ROOM_PX_W, ROOM_PX_H))
print()

-- Count entities and triggers
local totalEntities = 0
local totalTriggers = 0
for _, room in ipairs(rooms) do
    totalEntities = totalEntities + #(room.entities or {})
    totalTriggers = totalTriggers + #(room.triggers or {})
end
print(string.format("  Total entities: %d", totalEntities))
print(string.format("  Total triggers: %d", totalTriggers))
print()

print("[1/2] Building map structure...")
local levelData = { rooms = rooms }
local packageName = "Maggy/PCG/kirby_test"
local mapData = mapBuilder.buildMap(levelData, packageName)
print(string.format("  Package: %s", packageName))

print("[2/2] Encoding binary map...")
local outputDir = scriptDir .. "../Maps/Maggy/PCG"
os.execute('mkdir "' .. outputDir .. '" 2>nul')

local outputPath = outputDir .. "/kirby_test.bin"
local ok, err = binEncoder.encodeFile(outputPath, mapData)

if ok then
    local fh = io.open(outputPath, "rb")
    local size = 0
    if fh then size = fh:seek("end"); fh:close() end
    print(string.format("  Written: %s", outputPath))
    print(string.format("  Size: %d bytes (%.1f KB)", size, size / 1024))
    print()
    print("=" .. string.rep("=", 60))
    print("  SUCCESS! Kirby Test Map saved to:")
    print("  " .. outputPath)
    print()
    print("  SID: " .. packageName)
    print("  20 showcase rooms with every entity & trigger")
    print("=" .. string.rep("=", 60))
else
    print("  ERROR: " .. tostring(err))
    os.exit(1)
end
