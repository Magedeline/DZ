-- Overworld Models Form Field for L�nn
-- Provides autocomplete for 3D overworld model types and configurations

local overworldModels = {}

overworldModels.fieldType = "dropdown"
overworldModels.allowEmpty = false
overworldModels.editable = false

-- 3D Model categories
overworldModels.options = {
    -- Architectural Models
    "tower",
    "building", 
    "castle",
    "fortress",
    "lighthouse",
    "windmill",
    "bridge",
    "gate",
    "archway",
    "pillar",
    "obelisk",
    "monument",
    "statue",
    "fountain",
    "well",
    "altar",
    "throne",
    "podium",
    
    -- Natural Models
    "mountain",
    "hill",
    "cliff",
    "rock",
    "boulder",
    "tree",
    "pine_tree",
    "oak_tree",
    "dead_tree",
    "bush",
    "flower",
    "grass_patch",
    "mushroom",
    "crystal_formation",
    "geode",
    "stalactite",
    "stalagmite",
    "cave_entrance",
    
    -- Platform Models
    "platform",
    "floating_platform",
    "moving_platform",
    "rotating_platform",
    "spring_platform",
    "ice_platform",
    "lava_platform",
    "crystal_platform",
    "metal_platform",
    "stone_platform",
    "wood_platform",
    "glass_platform",
    
    -- Crystal Models
    "crystal",
    "power_crystal",
    "heart_crystal",
    "energy_crystal",
    "ice_crystal",
    "fire_crystal",
    "wind_crystal",
    "earth_crystal",
    "light_crystal",
    "dark_crystal",
    "rainbow_crystal",
    "shard",
    
    -- Mechanical Models
    "gear",
    "lever",
    "switch",
    "button",
    "panel",
    "console",
    "antenna",
    "satellite",
    "radar",
    "turbine",
    "engine",
    "generator",
    "conduit",
    "pipe",
    
    -- Decorative Models
    "banner",
    "flag",
    "sign",
    "post",
    "fence",
    "railing",
    "lamp",
    "lantern",
    "torch",
    "candle",
    "campfire",
    "brazier",
    "cauldron",
    "chest",
    "barrel",
    "crate",
    "vase",
    "pot",
    
    -- Transportation Models
    "cart",
    "wagon",
    "boat",
    "ship",
    "balloon",
    "airship",
    "portal",
    "teleporter",
    "elevator",
    "escalator",
    "conveyor",
    "rail",
    
    -- Weather/Environmental
    "cloud",
    "storm_cloud",
    "rainbow",
    "lightning",
    "tornado",
    "whirlpool",
    "waterfall",
    "geyser",
    "volcano",
    "crater",
    "fissure",
    "chasm"
}

-- Model size categories
overworldModels.sizeCategories = {
    tiny = {"shard", "candle", "flower", "mushroom"},
    small = {"crystal", "torch", "post", "bush", "rock"},
    medium = {"tree", "platform", "statue", "chest", "gear"},
    large = {"building", "tower", "bridge", "boulder", "cave_entrance"},
    huge = {"mountain", "castle", "fortress", "volcano", "chasm"},
    massive = {"mountain_range", "floating_island", "massive_crystal"}
}

-- Material types for models
overworldModels.materials = {
    stone = {"tower", "building", "castle", "mountain", "rock", "statue"},
    metal = {"gear", "antenna", "engine", "platform", "railing"},
    crystal = {"crystal", "power_crystal", "geode", "shard"},
    wood = {"tree", "post", "cart", "fence", "chest"},
    ice = {"ice_crystal", "ice_platform"},
    fire = {"torch", "campfire", "brazier", "volcano"},
    glass = {"glass_platform", "vase"},
    energy = {"portal", "teleporter", "energy_crystal"}
}

-- Animation types for models
overworldModels.animations = {
    static = {"mountain", "rock", "building", "statue"},
    rotate = {"gear", "turbine", "crystal", "windmill"},
    float = {"floating_platform", "cloud", "balloon"},
    pulse = {"power_crystal", "energy_crystal", "portal"},
    sway = {"tree", "banner", "flag"},
    flicker = {"torch", "candle", "campfire"},
    flow = {"waterfall", "geyser", "lightning"},
    spin = {"tornado", "whirlpool"}
}

-- Lighting categories
overworldModels.lighting = {
    none = {"rock", "mountain", "dead_tree"},
    ambient = {"tree", "building", "platform"},
    bright = {"crystal", "torch", "lighthouse"},
    colored = {"rainbow", "rainbow_crystal", "portal"},
    dynamic = {"lightning", "campfire", "volcano"},
    magical = {"power_crystal", "teleporter", "altar"}
}

-- Collision types
overworldModels.collision = {
    none = {"cloud", "rainbow", "lightning"},
    solid = {"rock", "building", "platform", "tree"},
    platform = {"floating_platform", "ice_platform"},
    interactive = {"switch", "lever", "portal", "chest"},
    climbable = {"tower", "cliff", "tree", "ladder"},
    damaging = {"lava_platform", "volcano", "lightning"}
}

return overworldModels