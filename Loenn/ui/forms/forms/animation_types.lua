-- Animation Types Form Field for Lönn  
-- Provides autocomplete for character and entity animation types

local animationTypes = {}

animationTypes.fieldType = "dropdown"
animationTypes.allowEmpty = true
animationTypes.editable = false

-- Character animation categories
animationTypes.options = {
    -- Basic Animations
    "idle",
    "walk",
    "run",
    "jump",
    "fall", 
    "land",
    "crouch",
    "crawl",
    "climb",
    "hang",
    
    -- Emotional Animations
    "happy",
    "sad",
    "angry",
    "surprised",
    "worried",
    "confused",
    "excited",
    "bored",
    "scared",
    "relieved",
    "determined",
    "thoughtful",
    "disappointed",
    "proud",
    "embarrassed",
    
    -- Interaction Animations
    "talk",
    "listen",
    "nod",
    "shake_head",
    "point",
    "wave",
    "beckon",
    "gesture",
    "bow",
    "curtsy",
    "handshake",
    "hug",
    "high_five",
    "salute",
    "clap",
    
    -- Action Animations
    "pickup",
    "throw",
    "push",
    "pull",
    "carry",
    "use_item",
    "examine",
    "search",
    "open_door",
    "close_door",
    "knock",
    "ring_bell",
    "flip_switch",
    "press_button",
    
    -- Combat Animations
    "attack",
    "defend",
    "dodge",
    "block",
    "parry",
    "charge",
    "retreat",
    "victory",
    "defeat",
    "wounded",
    "heal",
    "cast_spell",
    "aim",
    "shoot",
    "reload",
    
    -- Special Animations
    "transform",
    "teleport",
    "invisible",
    "float",
    "fly",
    "swim",
    "dig",
    "phase",
    "grow",
    "shrink",
    "glow",
    "sparkle",
    "dissolve",
    "materialize",
    "explode",
    
    -- Kirby Specific
    "inhale",
    "exhale",
    "copy_ability",
    "hover",
    "puff_up",
    "star_spit",
    "power_up",
    "ability_pose",
    "float_idle",
    "bounce",
    
    -- Theo Specific
    "panic",
    "nervous",
    "reassure",
    "photograph",
    "backpack_adjust",
    "mountain_point",
    "selfie",
    "journal_write",
    "compass_check",
    "binoculars",
    
    -- Madeline Specific
    "dash",
    "wall_slide",
    "grab_wall",
    "breathing_exercise",
    "mirror_touch",
    "strawberry_collect",
    "feather_use",
    "crystal_heart",
    "summit_flag",
    "anxiety_attack",
    
    -- Granny Specific
    "knit",
    "tea_sip",
    "wisdom_share",
    "cane_tap",
    "rocking_chair",
    "cookie_offer",
    "story_tell",
    "memory_recall",
    "gentle_pat",
    "warm_smile",
    
    -- Chara Specific
    "menacing",
    "knife_brandish",
    "evil_grin",
    "soul_drain",
    "reality_tear",
    "nightmare_form",
    "puppet_control",
    "time_stop",
    "fourth_wall_break",
    "reset_world",
    
    -- Environmental Animations
    "door_open",
    "door_close",
    "platform_move",
    "switch_flip",
    "gear_turn",
    "crystal_shine",
    "fire_flicker",
    "water_flow",
    "wind_blow",
    "lightning_strike",
    "earthquake",
    "snow_fall",
    
    -- UI Animations
    "button_press",
    "menu_open",
    "menu_close",
    "text_appear",
    "text_disappear",
    "icon_bounce",
    "progress_fill",
    "loading_spin",
    "notification_pop",
    "alert_flash"
}

-- Animation categories for filtering
animationTypes.categories = {
    movement = {"idle", "walk", "run", "jump", "fall", "climb", "float", "fly"},
    emotion = {"happy", "sad", "angry", "surprised", "worried", "excited"},
    interaction = {"talk", "wave", "point", "nod", "bow", "gesture"},
    combat = {"attack", "defend", "dodge", "charge", "victory", "defeat"},
    special = {"transform", "teleport", "glow", "sparkle", "dissolve"},
    character_specific = {"inhale", "dash", "menacing", "knit", "panic"}
}

-- Animation durations (in seconds)
animationTypes.durations = {
    instant = {"teleport", "flash", "appear", "disappear"},
    short = {"nod", "blink", "button_press", "wave"},
    medium = {"walk", "talk", "gesture", "attack", "pickup"},
    long = {"transform", "cast_spell", "story_tell", "breathing_exercise"},
    looping = {"idle", "walk", "run", "fly", "glow", "flicker"}
}

-- Animation priorities
animationTypes.priorities = {
    low = {"idle", "ambient", "background"},
    normal = {"walk", "talk", "gesture", "interact"},
    high = {"attack", "defend", "special", "cutscene"},
    critical = {"death", "transform", "boss_special", "story_moment"}
}

return animationTypes