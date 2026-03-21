local npcDialogKeys = require("ui.forms.fields.npc_dialog_keys")
local npcSessionFlags = require("ui.forms.fields.npc_session_flags")
local utils = require("utils")
local drawing = require("utils.drawing")

-- Configuration constants to eliminate magic numbers
local NPCConfig = {
    DEFAULT_TRIGGER_DISTANCE = 32.0,
    WIDE_TRIGGER_DISTANCE = 48.0,
    LARGE_TRIGGER_DISTANCE = 64.0,
    CLOSE_TRIGGER_DISTANCE = 40.0,
    
    MIN_TRIGGER_DISTANCE = 8.0,
    MAX_TRIGGER_DISTANCE = 200.0,
    
    DEFAULT_DEPTH = 1000,
    DEFAULT_JUSTIFICATION = {0.5, 1.0}
}

-- Sprite configuration to eliminate hardcoded paths
local SpriteConfig = {
    THEO = "characters/theo/",
    GRANNY = "characters/granny/",
    MADELINE = "characters/madeline/",
    BADELINE = "characters/badeline/",
    OSHIRO = "characters/oshiro/",
    KIRBY = "characters/kirby/",
    CHARA = "characters/chara/",
    RALSEI = "characters/ralsei/",
    MAGALOR = "characters/magolor/",
    SANS = "characters/sans/",
    PAPYRUS = "characters/papyrus/",
    TORIEL = "characters/toriel/",
    ASGORE = "characters/asgore/",
    UNDYNE = "characters/undyne/",
    GASTER = "characters/gaster/",
    ASRIEL = "characters/asriel/",
    FLOWEY = "characters/flowey/",
    PLAYER = "characters/player/",
    BIRD = "characters/bird/",
    -- New characters
    RICK = "characters/rick/",
    KINE = "characters/kine/",
    COO = "characters/coo/",
    BANDANA_WADDLE_DEE = "characters/bandana_waddle_dee/",
    KING_DDD = "characters/king_ddd/",
    META_KNIGHT = "characters/meta_knight/",
    ADELINE = "characters/adeline/",
    CLOVER = "characters/clover/",
    MELODY = "characters/melody/",
    BATTY = "characters/batty/",
    EMILY = "characters/emily/",
    CODY = "characters/cody/",
    ODIN = "characters/odin/",
    CHARLO = "characters/charlo/",
    FRISK = "characters/frisk/",
    MAGOLOR_NEW = "characters/magolor_new/",
    SUSIE_HALTMANN = "characters/susie_haltmann/",
    NESS = "characters/ness/",
    TARANZA = "characters/taranza/",
    GOOEY = "characters/gooey/",
    SQUEAKER = "characters/squeaker/",
    DARK_META_KNIGHT = "characters/dark_meta_knight/",
    MARX = "characters/marx/",
    FRAN_ZALEA = "characters/fran_zalea/",
    FLAMBERGE_ZALEA = "characters/flamberge_zalea/",
    HYNES_ZALEA = "characters/hynes_zalea/",
    ASRIEL_NEW = "characters/asriel_new/",
    KIRBY_CLASSIC = "characters/kirby_classic/"
}

-- Dialog configuration to centralize dialog keys
local DialogConfig = {
    THEO_GREETING = "CH0_THEO_A",
    GRANNY_WISDOM = "CH20_MADANDBADFINALLYMEETGRANNY",
    MADELINE_INTRO = "CH8_MADELINE_INTRO",
    BADELINE_GRIEF = "CH2_BADELINE_GRIEFA",
    OSHIRO_FRONT_DESK = "CH5_OSHIRO_FRONT_DESK",
    KIRBY_INTRO = "CH1_KIRBY_INTRO",
    CHARA_INTRO = "CH2_CHARA_INTRO",
    RALSEI_INTRO = "RALSEI_INTRO",
    MAGALOR_GREETING = "CH1_MAGGY_A",
    VOICE_MESSAGE = "CH9_TORIEL_AND_SANS_VOICEPHONECALLMESSAGE",
    TORIEL_GUIDANCE = "CH16_TORIEL_A",
    ASGORE_MONSTER = "CH19_MONSTER_CAME_B",
    UNDYNE_MONSTER = "CH19_MONSTER_CAME_E",
    GASTER_VOICE = "CH6_GASTERVOICE",
    ASRIEL_RUINS = "CH10_RUINS_INTRO",
    FLOWEY_INTRO = "CH10_FLOWEY_INTRO"
}

-- Factory function to create NPC placement data
local function createNPCPlacement(config)
    return {
        name = config.name,
        data = {
            spriteId = config.spriteId,
            triggerDistance = config.triggerDistance or NPCConfig.DEFAULT_TRIGGER_DISTANCE,
            canTriggerEvents = config.canTriggerEvents ~= false,
            canInteract = config.canInteract ~= false,
            interactionDialog = config.interactionDialog or "",
            triggerFlag = config.triggerFlag or "",
            removeAfterInteraction = config.removeAfterInteraction or false,
            centerPosition = config.centerPosition or false
        }
    }
end

-- NPC placement configurations
local npcPlacements = {
    {name = "theo_npc", spriteId = SpriteConfig.THEO, interactionDialog = DialogConfig.THEO_GREETING, triggerFlag = "met_theo"},
    {name = "granny_npc", spriteId = SpriteConfig.GRANNY, triggerDistance = NPCConfig.WIDE_TRIGGER_DISTANCE, interactionDialog = DialogConfig.GRANNY_WISDOM, triggerFlag = "met_granny"},
    {name = "madeline_npc", spriteId = SpriteConfig.MADELINE, interactionDialog = DialogConfig.MADELINE_INTRO, triggerFlag = "met_madeline"},
    {name = "badeline_npc", spriteId = SpriteConfig.BADELINE, interactionDialog = DialogConfig.BADELINE_GRIEF, triggerFlag = "met_badeline"},
    {name = "oshiro_npc", spriteId = SpriteConfig.OSHIRO, triggerDistance = NPCConfig.CLOSE_TRIGGER_DISTANCE, interactionDialog = DialogConfig.OSHIRO_FRONT_DESK, triggerFlag = "met_oshiro"},
    {name = "kirby_npc", spriteId = SpriteConfig.KIRBY, interactionDialog = DialogConfig.KIRBY_INTRO, triggerFlag = "met_kirby"},
    {name = "chara_npc", spriteId = SpriteConfig.CHARA, interactionDialog = DialogConfig.CHARA_INTRO, triggerFlag = "met_chara"},
    {name = "ralsei_npc", spriteId = SpriteConfig.RALSEI, interactionDialog = DialogConfig.RALSEI_INTRO, triggerFlag = "met_ralsei"},
    {name = "magalor_npc", spriteId = SpriteConfig.MAGALOR, interactionDialog = DialogConfig.MAGALOR_GREETING, triggerFlag = "met_magalor"},
    {name = "sans_npc", spriteId = SpriteConfig.SANS, interactionDialog = DialogConfig.VOICE_MESSAGE, triggerFlag = "met_sans"},
    {name = "papyrus_npc", spriteId = SpriteConfig.PAPYRUS, interactionDialog = DialogConfig.VOICE_MESSAGE, triggerFlag = "met_papyrus"},
    {name = "toriel_npc", spriteId = SpriteConfig.TORIEL, interactionDialog = DialogConfig.TORIEL_GUIDANCE, triggerFlag = "met_toriel"},
    {name = "asgore_npc", spriteId = SpriteConfig.ASGORE, interactionDialog = DialogConfig.ASGORE_MONSTER, triggerFlag = "met_asgore"},
    {name = "undyne_npc", spriteId = SpriteConfig.UNDYNE, interactionDialog = DialogConfig.UNDYNE_MONSTER, triggerFlag = "met_undyne"},
    {name = "gaster_npc", spriteId = SpriteConfig.GASTER, interactionDialog = DialogConfig.GASTER_VOICE, triggerFlag = "met_gaster"},
    {name = "asriel_npc", spriteId = SpriteConfig.ASRIEL, interactionDialog = DialogConfig.ASRIEL_RUINS, triggerFlag = "met_asriel"},
    {name = "flowey_npc", spriteId = SpriteConfig.FLOWEY, interactionDialog = DialogConfig.FLOWEY_INTRO, triggerFlag = "met_flowey"},
    
    -- Special behavior NPCs
    {name = "proximity_trigger_only", spriteId = SpriteConfig.THEO, triggerDistance = NPCConfig.LARGE_TRIGGER_DISTANCE, canInteract = false, triggerFlag = "player_detected"},
    {name = "interaction_only", spriteId = SpriteConfig.GRANNY, canTriggerEvents = false, interactionDialog = "HELLO_PLAYER"},
    {name = "one_time_npc", spriteId = SpriteConfig.MADELINE, interactionDialog = "FAREWELL_MESSAGE", triggerFlag = "final_encounter", removeAfterInteraction = true},
    {name = "centered_npc", spriteId = SpriteConfig.THEO, interactionDialog = "CH18_GETTING_TOGETHER", triggerFlag = "center_met", centerPosition = true},
    {name = "chapter_intro_npc", spriteId = SpriteConfig.KIRBY, triggerDistance = NPCConfig.WIDE_TRIGGER_DISTANCE, interactionDialog = "CH0_MODINTRO", triggerFlag = "chapter_started"},
    {name = "boss_encounter_npc", spriteId = SpriteConfig.CHARA, triggerDistance = NPCConfig.LARGE_TRIGGER_DISTANCE, interactionDialog = "CH8_CHARA_BOSS_START", triggerFlag = "boss_encounter", centerPosition = true},
    {name = "tutorial_npc", spriteId = SpriteConfig.BADELINE, triggerDistance = NPCConfig.CLOSE_TRIGGER_DISTANCE, interactionDialog = "CH1_KIRBY_TUTORIAL", triggerFlag = "tutorial_given"},
    {name = "phone_call_npc", spriteId = SpriteConfig.SANS, interactionDialog = "CH2_DREAM_PHONECALL_TRAP", triggerFlag = "phone_call_received"},
    {name = "mirror_npc", spriteId = SpriteConfig.MADELINE, interactionDialog = "CH7_MADELINE_MIRROR", triggerFlag = "mirror_encounter"},
    {name = "epilogue_npc", spriteId = SpriteConfig.THEO, interactionDialog = "EPMOD_CABIN", triggerFlag = "epilogue_started"}
}

local NPC_Event = {}

NPC_Event.name = "MaggyHelper/NPC_Event"
NPC_Event.depth = NPCConfig.DEFAULT_DEPTH
NPC_Event.justification = NPCConfig.DEFAULT_JUSTIFICATION
NPC_Event.texture = SpriteConfig.THEO .. "idle00"

-- Generate placements from configuration
NPC_Event.placements = {}
for _, config in ipairs(npcPlacements) do
    table.insert(NPC_Event.placements, createNPCPlacement(config))
end

-- Texture mapping using lookup table instead of chain of if-elseif
local textureMap = {
    [SpriteConfig.THEO] = "characters/theo/idle00",
    [SpriteConfig.GRANNY] = "characters/granny/idle00",
    [SpriteConfig.MADELINE] = "characters/madeline/idle00",
    [SpriteConfig.BADELINE] = "characters/badeline/idle00",
    [SpriteConfig.OSHIRO] = "characters/oshiro/oshiro00",
    [SpriteConfig.KIRBY] = "characters/kirby/idle00",
    [SpriteConfig.CHARA] = "characters/chara/idle00",
    [SpriteConfig.RALSEI] = "characters/ralsei/idle00",
    [SpriteConfig.MAGALOR] = "characters/magolor/idle00",
    [SpriteConfig.SANS] = "characters/sans/idle00",
    [SpriteConfig.PAPYRUS] = "characters/papyrus/idle00",
    [SpriteConfig.TORIEL] = "characters/toriel/idle00",
    [SpriteConfig.ASGORE] = "characters/asgore/idle00",
    [SpriteConfig.UNDYNE] = "characters/undyne/idle00",
    [SpriteConfig.GASTER] = "characters/gaster/idle00",
    [SpriteConfig.ASRIEL] = "characters/asriel/idle00",
    [SpriteConfig.FLOWEY] = "characters/flowey/idle00",
    -- New characters
    [SpriteConfig.RICK] = "characters/rick/idle00",
    [SpriteConfig.KINE] = "characters/kine/idle00",
    [SpriteConfig.COO] = "characters/coo/idle00",
    [SpriteConfig.BANDANA_WADDLE_DEE] = "characters/bandana_waddle_dee/idle00",
    [SpriteConfig.KING_DDD] = "characters/king_ddd/idle00",
    [SpriteConfig.META_KNIGHT] = "characters/meta_knight/idle00",
    [SpriteConfig.ADELINE] = "characters/adeline/idle00",
    [SpriteConfig.CLOVER] = "characters/clover/idle00",
    [SpriteConfig.MELODY] = "characters/melody/idle00",
    [SpriteConfig.BATTY] = "characters/batty/idle00",
    [SpriteConfig.EMILY] = "characters/emily/idle00",
    [SpriteConfig.CODY] = "characters/cody/idle00",
    [SpriteConfig.ODIN] = "characters/odin/idle00",
    [SpriteConfig.CHARLO] = "characters/charlo/idle00",
    [SpriteConfig.FRISK] = "characters/frisk/idle00",
    [SpriteConfig.MAGOLOR_NEW] = "characters/magolor_new/idle00",
    [SpriteConfig.SUSIE_HALTMANN] = "characters/susie_haltmann/idle00",
    [SpriteConfig.NESS] = "characters/ness/idle00",
    [SpriteConfig.TARANZA] = "characters/taranza/idle00",
    [SpriteConfig.GOOEY] = "characters/gooey/idle00",
    [SpriteConfig.SQUEAKER] = "characters/squeaker/idle00",
    [SpriteConfig.DARK_META_KNIGHT] = "characters/dark_meta_knight/idle00",
    [SpriteConfig.MARX] = "characters/marx/idle00",
    [SpriteConfig.FRAN_ZALEA] = "characters/fran_zalea/idle00",
    [SpriteConfig.FLAMBERGE_ZALEA] = "characters/flamberge_zalea/idle00",
    [SpriteConfig.HYNES_ZALEA] = "characters/hynes_zalea/idle00",
    [SpriteConfig.ASRIEL_NEW] = "characters/asriel_new/idle00",
    [SpriteConfig.KIRBY_CLASSIC] = "characters/kirby_classic/idle00"
}

-- Convert sprite configs to options array
local spriteOptions = {}
for _, spritePath in pairs(SpriteConfig) do
    table.insert(spriteOptions, spritePath)
end

NPC_Event.fieldInformation = {
    spriteId = {
        options = spriteOptions,
        editable = true,
        description = "Path to the sprite graphics"
    },
    triggerDistance = {
        fieldType = "number",
        minimumValue = NPCConfig.MIN_TRIGGER_DISTANCE,
        maximumValue = NPCConfig.MAX_TRIGGER_DISTANCE,
        description = "Distance at which the NPC triggers events"
    },
    canTriggerEvents = {
        fieldType = "boolean",
        description = "Whether the NPC can trigger proximity events"
    },
    canInteract = {
        fieldType = "boolean",
        description = "Whether the player can talk to this NPC"
    },
    interactionDialog = npcDialogKeys,
    triggerFlag = npcSessionFlags,
    removeAfterInteraction = {
        fieldType = "boolean",
        description = "Whether to remove the NPC after first interaction"
    },
    centerPosition = {
        fieldType = "boolean",
        description = "Whether to center the NPC in the level bounds"
    }
}

-- Function to get the texture based on sprite ID using lookup table
function NPC_Event.texture(room, entity)
    local spriteId = entity.spriteId or SpriteConfig.THEO
    
    -- Direct lookup instead of chain of conditionals
    local texture = textureMap[spriteId]
    if texture then
        return texture
    end
    
    -- Fallback: try partial matching for custom sprites
    for configSprite, configTexture in pairs(textureMap) do
        if spriteId:find(configSprite:gsub("characters/", ""):gsub("/", "")) then
            return configTexture
        end
    end
    
    -- Default fallback
    return textureMap[SpriteConfig.THEO]
end

-- Visual selection rectangle for the entity
function NPC_Event.selection(room, entity)
    local triggerDistance = entity.triggerDistance or NPCConfig.DEFAULT_TRIGGER_DISTANCE
    return utils.rectangle(
        entity.x - triggerDistance/2, 
        entity.y - triggerDistance/2, 
        triggerDistance, 
        triggerDistance
    )
end

-- Draw the trigger radius in the editor
function NPC_Event.draw(room, entity, viewport)
    local x, y = entity.x or 0, entity.y or 0
    local triggerDistance = entity.triggerDistance or NPCConfig.DEFAULT_TRIGGER_DISTANCE
    
    -- Visual styling constants
    local FILL_COLOR = {0.3, 0.8, 1.0, 0.3}
    local OUTLINE_COLOR = {0.3, 0.8, 1.0, 0.8}
    local DEFAULT_COLOR = {1.0, 1.0, 1.0, 1.0}
    
    -- Draw trigger radius circle
    drawing.setColor(FILL_COLOR)
    drawing.circle("fill", x, y, triggerDistance)
    
    -- Draw trigger radius outline
    drawing.setColor(OUTLINE_COLOR)
    drawing.circle("line", x, y, triggerDistance)
    
    -- Reset color
    drawing.setColor(DEFAULT_COLOR)
end

return NPC_Event