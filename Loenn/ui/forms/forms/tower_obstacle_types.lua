-- Tower Obstacle Types Form Field for Lönn
-- Provides autocomplete for 3D tower obstacle types and configurations

local towerObstacleTypes = {}

towerObstacleTypes.fieldType = "dropdown"
towerObstacleTypes.allowEmpty = false
towerObstacleTypes.editable = false

-- Obstacle type categories
towerObstacleTypes.options = {
    -- Basic Obstacles
    "static_block",
    "moving_block",
    "rotating_block",
    "swinging_block",
    "falling_block",
    "crumbling_block",
    "ice_block",
    "fire_block",
    "electric_block",
    "crystal_block",
    
    -- Spike Obstacles
    "static_spikes",
    "retracting_spikes",
    "rotating_spikes",
    "moving_spikes",
    "fire_spikes",
    "ice_spikes",
    "electric_spikes",
    "crystal_spikes",
    "hidden_spikes",
    "pressure_spikes",
    
    -- Platform Obstacles
    "disappearing_platform",
    "tilting_platform",
    "moving_platform",
    "rotating_platform",
    "spring_platform",
    "conveyor_platform",
    "magnetic_platform",
    "phase_platform",
    "gravity_platform",
    "teleport_platform",
    
    -- Energy Obstacles
    "laser_beam",
    "energy_wall",
    "force_field",
    "electric_arc",
    "plasma_bolt",
    "energy_orb",
    "power_conduit",
    "static_discharge",
    "electromagnetic_pulse",
    "energy_vortex",
    
    -- Projectile Obstacles
    "cannon",
    "arrow_trap",
    "fireball_launcher",
    "ice_shard_thrower",
    "rock_catapult",
    "energy_blaster",
    "crystal_shooter",
    "missile_launcher",
    "grenade_thrower",
    "magic_projectile",
    
    -- Environmental Obstacles
    "lava_flow",
    "water_jet",
    "steam_vent",
    "wind_current",
    "ice_slide",
    "quicksand",
    "acid_pool",
    "poison_gas",
    "fog_cloud",
    "blizzard",
    
    -- Mechanical Obstacles
    "grinding_gears",
    "spinning_blade",
    "crushing_piston",
    "hydraulic_press",
    "pendulum_blade",
    "buzz_saw",
    "drill",
    "conveyor_belt",
    "elevator_trap",
    "mechanical_arm",
    
    -- Magic Obstacles
    "magic_barrier",
    "teleport_trap",
    "illusion_wall",
    "gravity_well",
    "time_distortion",
    "dimension_rift",
    "curse_zone",
    "healing_aura",
    "mana_drain",
    "spell_reflection",
    
    -- Boss Obstacles
    "boss_attack_zone",
    "boss_projectile",
    "boss_slam_area",
    "boss_charge_path",
    "boss_aoe_attack",
    "boss_shield",
    "boss_minion_spawn",
    "boss_transformation",
    "boss_rage_mode",
    "boss_vulnerability",
    
    -- Interactive Obstacles
    "lever_gate",
    "pressure_plate",
    "timed_switch",
    "color_coded_door",
    "key_lock",
    "puzzle_block",
    "memory_sequence",
    "riddle_gate",
    "musical_lock",
    "pattern_match"
}

-- Difficulty categories
towerObstacleTypes.difficulty = {
    beginner = {
        "static_block", "moving_block", "disappearing_platform", 
        "static_spikes", "moving_platform", "lever_gate"
    },
    intermediate = {
        "rotating_block", "retracting_spikes", "tilting_platform",
        "laser_beam", "cannon", "grinding_gears", "pressure_plate"
    },
    advanced = {
        "swinging_block", "rotating_spikes", "energy_wall",
        "fireball_launcher", "spinning_blade", "magic_barrier"
    },
    expert = {
        "falling_block", "moving_spikes", "force_field",
        "missile_launcher", "crushing_piston", "gravity_well"
    },
    master = {
        "electric_block", "teleport_platform", "energy_vortex",
        "boss_attack_zone", "time_distortion", "dimension_rift"
    }
}

-- Movement patterns
towerObstacleTypes.movementPatterns = {
    static = {"static_block", "static_spikes", "lever_gate"},
    linear = {"moving_block", "moving_platform", "conveyor_belt"},
    circular = {"rotating_block", "spinning_blade", "pendulum_blade"},
    oscillating = {"swinging_block", "retracting_spikes", "hydraulic_press"},
    random = {"disappearing_platform", "teleport_trap", "chaos_zone"},
    timed = {"falling_block", "timed_switch", "pressure_spikes"},
    player_triggered = {"pressure_plate", "proximity_sensor", "motion_detector"},
    scripted = {"boss_attack_zone", "cutscene_obstacle", "story_barrier"}
}

-- Damage types
towerObstacleTypes.damageTypes = {
    none = {"lever_gate", "moving_platform", "teleport_platform"},
    instant = {"static_spikes", "laser_beam", "crushing_piston"},
    continuous = {"fire_block", "lava_flow", "poison_gas"},
    knockback = {"explosion", "wind_current", "force_field"},
    special = {"mana_drain", "curse_zone", "time_distortion"}
}

-- Visual effects
towerObstacleTypes.visualEffects = {
    particles = {"fire_block", "ice_block", "electric_block", "magic_barrier"},
    glow = {"energy_wall", "crystal_block", "laser_beam", "power_conduit"},
    animation = {"grinding_gears", "spinning_blade", "pendulum_blade"},
    transparency = {"phase_platform", "illusion_wall", "ghost_block"},
    color_change = {"color_coded_door", "pattern_match", "mood_lighting"}
}

-- Audio cues
towerObstacleTypes.audioCues = {
    mechanical = {"grinding_gears", "hydraulic_press", "buzz_saw"},
    magical = {"magic_barrier", "teleport_trap", "dimension_rift"},
    elemental = {"fire_block", "ice_slide", "wind_current"},
    warning = {"falling_block", "missile_launcher", "boss_attack_zone"},
    ambient = {"lava_flow", "steam_vent", "crystal_resonance"}
}

return towerObstacleTypes