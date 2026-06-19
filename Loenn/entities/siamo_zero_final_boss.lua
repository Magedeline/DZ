-- Siamo Zero Final Boss
-- Public-facing final boss presets focused on the Siamo Zero forms.
-- Includes the base Siamo Zero encounter plus Siamo Zero Delta and
-- Celestial Zero variants for extra and remix encounters.

local siamoZeroFinalBoss = {}

siamoZeroFinalBoss.name = "DZ/SiamoZeroFinalBoss"
siamoZeroFinalBoss.depth = 0
siamoZeroFinalBoss.texture = "characters/els_true_final_boss/boss00"
siamoZeroFinalBoss.justification = {0.5, 1.0}
siamoZeroFinalBoss.nodeVisibility = "always"

-- Supports unlimited nodes for movement patterns (BadelineBoss style)
siamoZeroFinalBoss.nodeLimits = {0, -1}
siamoZeroFinalBoss.nodeLineRenderType = "line"

siamoZeroFinalBoss.placements = {
    {
        name = "main",
        data = {
            patternIndex = 4,
            siamoTier = "soulBlack",
            siamoVariant = "zero",
            dialog = true,
            startHit = false,
            attackSequence = ""
        }
    },
    {
        name = "siamo_zero_aeon",
        data = {
            patternIndex = 2,
            siamoTier = "soulBlack",
            siamoVariant = "zero",
            dialog = false,
            startHit = true,
            attackSequence = "CrescentBeamShot,EnergySwordCombo,TornadoSlash,RevolutionSword,RisingSpine,DownThrust,DrillStab,EnergyShower"
        }
    },
    {
        name = "siamo_zero_morpho",
        data = {
            patternIndex = 3,
            siamoTier = "soulBlack",
            siamoVariant = "zero",
            dialog = false,
            startHit = true,
            attackSequence = "VortexStrike,DoubleSideSlash,MorphoEmerge,TimeborderCollapse"
        }
    },
    {
        name = "siamo_zero_pink",
        data = {
            patternIndex = 4,
            siamoTier = "pink",
            siamoVariant = "zero",
            dialog = false,
            startHit = true,
            attackSequence = ""
        }
    },
    {
        name = "siamo_zero_soul_black",
        data = {
            patternIndex = 4,
            siamoTier = "soulBlack",
            siamoVariant = "zero",
            dialog = false,
            startHit = true,
            attackSequence = ""
        }
    },
    {
        name = "siamo_zero_stellarruss",
        data = {
            patternIndex = 4,
            siamoTier = "stellarruss",
            siamoVariant = "zero",
            dialog = false,
            startHit = true,
            attackSequence = ""
        }
    },
    {
        name = "siamo_zero_delta_extra",
        data = {
            patternIndex = 4,
            siamoTier = "soulBlack",
            siamoVariant = "delta",
            dialog = false,
            startHit = true,
            attackSequence = ""
        }
    },
    {
        name = "celestial_zero_remix",
        data = {
            patternIndex = 4,
            siamoTier = "stellarruss",
            siamoVariant = "celestial",
            dialog = false,
            startHit = true,
            attackSequence = ""
        }
    },
    {
        name = "siamo_zero_ultimate_chain",
        data = {
            patternIndex = 4,
            siamoTier = "stellarruss",
            siamoVariant = "zero",
            dialog = false,
            startHit = true,
            attackSequence = "EnergyShower,VortexStrike,TimeborderCollapse"
        }
    }
}

-- Attack pattern options exposed for Siamo Zero encounters
local attackPatterns = {
    -- Siamo Zero - Aeon Hero Fake attacks (sword/beam)
    "CrescentBeamShot",
    "EnergySwordCombo",
    "TornadoSlash",
    "RevolutionSword",
    "RisingSpine",
    "DownThrust",
    "DrillStab",
    "EnergyShower",
    -- Siamo Zero - Morpho Knight Fake attacks (vortex/slash)
    "VortexStrike",
    "DoubleSideSlash",
    "MorphoEmerge",
    "TimeborderCollapse",
    "ConqueredPeakCascade",
    -- Utility / clone summons
    "SummonGalactaKnightClone",
    "SummonMorphoKnightClone",
    "ClearKnightClones"
}

-- Pattern index for different behavior modes
local patternOptions = {0, 1, 2, 3, 4}
local siamoTierOptions = {"pink", "soulBlack", "stellarruss"}
local siamoVariantOptions = {"zero", "delta", "celestial"}

siamoZeroFinalBoss.fieldInformation = {
    patternIndex = {
        fieldType = "integer",
        options = patternOptions,
        editable = true,
        minimumValue = 0,
        maximumValue = 4
    },
    siamoTier = {
        fieldType = "string",
        options = siamoTierOptions,
        editable = true
    },
    siamoVariant = {
        fieldType = "string",
        options = siamoVariantOptions,
        editable = true
    },
    dialog = {
        fieldType = "boolean",
        editable = true
    },
    startHit = {
        fieldType = "boolean",
        editable = true
    },
    attackSequence = {
        fieldType = "string",
        editable = true,
        options = attackPatterns
    }
}

siamoZeroFinalBoss.fieldOrder = {
    "x", "y",
    "patternIndex",
    "siamoTier",
    "siamoVariant",
    "dialog",
    "startHit",
    "attackSequence"
}

-- Tooltip information
siamoZeroFinalBoss.tooltips = {
        patternIndex = "Attack pattern index (0-4) for different Siamo Zero behavior presets and aggression levels.",
    siamoTier = "Siamo Zero palette and aggression preset. Pink is lightest, soulBlack is the default, stellarruss is the rainbow cosmic-void variant with the hardest timings.",
    siamoVariant = "Identity preset for the boss. zero uses the Siamo Zero name, delta becomes the extra boss Siamo Zero Delta, and celestial becomes the remix boss Celestial Zero with the most dramatic music progression.",
        dialog = "Show the base Siamo Zero intro before the fight begins.",
        startHit = "Boss takes an initial hit to begin combat immediately, similar to BadelineBoss.",
    attackSequence = [[Custom attack sequence (comma-separated attack names)
Leave empty for default AI behavior.
Optional per-step delay syntax: AttackName:1.25

SIAMO ZERO (Fallen Path / Corrupted Nightmare):
siamoTier scales this phase: Pink is slower and lighter, soulBlack is the default loadout, and stellarruss is faster with denser projectile patterns.
  Sub-Phase A: Aeon Hero Fake (Sword/Beam):
• CrescentBeamShot - 3 crescent projectiles in fan pattern
• EnergySwordCombo - 6-hit teleporting sword combo + shockwave
• TornadoSlash - Spiraling tornado with trailing blade projectiles
• RevolutionSword - 3 waves of expanding blade rings
• RisingSpine - 8 spine pillars rising from ground below Kglobal::Player
• DownThrust - Rise + dive thrust with ground shockwave (10 blades)
• DrillStab - Rapid forward drill with bilateral blade trail
• EnergyShower - 30 energy projectiles raining from above
• ConqueredPeakCascade - Celeste-style diagonal rush chain used by Delta/Celestial variants

  Sub-Phase B: Morpho Knight Fake (Vortex/Slash):
• VortexStrike - Vortex summon + pull-in + explosive blade release
• DoubleSideSlash - Two sweeping crescent fans from left/right
• MorphoEmerge - Vanish + emerge from below with upward pillar strike
• TimeborderCollapse - 120-frame reality distortion + 3 projectile waves + final 24-projectile burst

UTILITY / CLONES:
• SummonGalactaKnightClone - Spawns a roaming Galacta Knight clone on Els' left
• SummonMorphoKnightClone - Spawns a roaming Morpho Knight clone on Els' right
• ClearKnightClones - Removes any currently active knight clones]]
}

-- Attack descriptions for reference (based on C# implementation)
siamoZeroFinalBoss.attackDescriptions = {
    -- Phase 1 (Doppia Elillca) - Duality/Mirror themed attacks
    DoppiaCloneAssault = [[Spawns 4 shadow clones in cardinal directions (150px radius)
SFX: Els_Create, Els_Spawn, Els_Activate
Effects: Burst particles at spawn positions, displacement burst]],

    DualityWave = [[Charges for 0.5s then releases expanding energy wave
SFX: Els_Charge (charge), Els_Impact (release)
Effects: Screen shake (1.5), displacement burst (256px radius), 360° particle wave]],

    ShadowBlast = [[Builds energy for 0.6s then fires 12 shadow projectiles in all directions
SFX: Els_Build (buildup), Els_Slice, Els_Rift_Bullet (per projectile)
Effects: Displacement burst, screen shake (1.0)]],

    MirrorDimension = [[Creates 6 dimensional rifts in circular pattern (200px radius)
SFX: Els_Rift, Els_Teleport
Effects: Purple flash, displacement burst (192px radius), 20 particles per rift]],

    DimensionalDefense = [[Activates defensive shield for 5 seconds with 75% damage reduction
SFX: Els_Bubble, Els_Activate
Effects: Cyan flash, protective barrier particles (360° ring), displacement burst
Cooldown: Active duration only]],

    DualityHeal = [[Heals 15% of max HP with green energy absorption
SFX: Els_Revival, Els_Charge
Cooldown: 15 seconds
Effects: Light green flash, displacement burst, 30 green healing particles]],

    RiftStrikeCombo = [[5-hit teleporting combo attack using dimension rifts
Cost: 30 rift power
SFX: Els_BeamSlash, Els_Rift, Els_Teleport, Els_Impact, Els_Slice
Effects: Teleports to random positions, 8 projectiles per strike, 0.2s between hits]],

    quickDashAttack = [[Quick dash attack with afterimage effect
SFX: Els_Teleport
Effects: White flash, displacement burst, dash particles]],

    energyOrbShot = [[Fires a slow-moving energy orb projectile
SFX: Els_Rift_Bullet
Effects: Displacement burst, shoot particles in firing direction]],

    burstHeal = [[Quick heal restoring 5% of max HP
SFX: Els_Revival
Cooldown: 10 seconds (half of DualityHeal)
Effects: Light green flash, displacement burst, 15 healing particles]],
    
    -- Phase 2 (Penumbra Phastasm) - Void/Darkness themed attacks
    PenumbraVoidStorm = [[Spawns 20 void projectiles across 500x500 area
SFX: Els_Darkmatter_Spawn, Els_Build, Els_Spawn
Effects: Screen shake (2.0), dark flash, 10 burst particles per projectile]],

    PhantasmBarrage = [[Rapid-fire projectiles from all 5 phantasm light positions simultaneously
SFX: Els_Shell_Screamer, Els_Precreate, Els_Rift_Bullet
Effects: 5 projectiles from each light position, dash particles]],

    VoidCollapse = [[Implosion pull-in effect followed by massive explosion after 1.2s
SFX: Els_PreImpact (implosion), Els_BigHit, Els_Impact (explosion)
Effects: Inward displacement (-1.5), purple flash, screen shake (2.0), 50 explosion particles]],

    DimensionalTear = [[Creates 8 reality tears across 600x600 area with spatial distortion
SFX: Els_Shellcrack, Els_Rift
Effects: Black flash, screen shake (1.5), displacement burst per tear, 15 particles each]],

    UltimateAnnihilation = [[Void mode only: 2 second charge then massive 50-projectile explosion
Requirements: Void mode active (HP ≤ 20%)
SFX: Els_Final_Cry, Els_Predeath (charge), Els_BigHit, Els_StarDeath (release)
Effects: Red flash, screen shake (4.0), massive displacement burst (512px radius)]],

    VoidShield = [[Enhanced defensive shield for 6 seconds with dark void energy
SFX: Els_Bubble, Els_Darkmatter_Spawn
Effects: Screen shake (1.0), displacement burst (192px radius), dark shield particles (360° ring)
Special: All phantasm lights intensify to 1.5 alpha during shield]],

    PenumbraRegeneration = [[Heals 20% of max HP and restores 20 rift power through void absorption
SFX: Els_Revival, Els_Charge
Cooldown: 15 seconds
Effects: Displacement burst (192px radius), 50 purple healing particles across 256px area]],

    DimensionalCataclysm = [[Creates 8 rifts in circular pattern (250px radius), each firing 12 projectiles
Cost: 40 rift power
SFX: Els_Time_Manipulator_Start, Els_Precreate, Els_Rift, Els_Spawn
Effects: Purple flash, displacement burst per rift, 0.3s delay between rifts, final explosion]],

    RiftMaelstrom = [[4 second spiral vortex of expanding rifts, 6 projectiles per rift
Cost: 50 rift power
SFX: Els_Time_Manipulator_Start, Els_Rift, Els_Teleport, Els_Rift_Bullet
Effects: Screen shake (1.5), purple flash, spiral pattern expanding from 100px to 220px radius]],

    ApocalypticRiftBlast = [[Ultimate dimension rift attack consuming all 100 rift power
Cost: 100 rift power (full charge)
Duration: 2s charge + multi-wave release
SFX: Els_Knockout, Els_Final_Cry, Els_Build, Els_Charge (charge phase)
     Els_BigHit, Els_Impact, Els_Time_Manipulator_End, Els_BeamSlash, Els_Slice, 
     Els_Rift_Bullet, Els_Shellcrack, Els_StarDeath (release phase)
Effects: 
  • Charge: Red flash, screen shake (2.0), pull-in particles
  • Release: White flash, screen shake (4.0), massive displacement (512px radius)
  • 3 waves of 24 projectiles each (0.4s between waves)
  • 15 reality tears across 800px area (0.1s between tears)
  • Final shockwave with screen shake (3.0) and displacement (384px radius)]],

    -- Phase 3 (Siamo Zero) - Aeon Hero Fake attacks
    CrescentBeamShot = [[Fires 3 crescent projectiles in a fan pattern (~17° spread)
Sprite: crescent_beam_shot (aeon_hero_fake)
SFX: Els_Charge (charge 0.4s), Els_BeamSlash (fire)
Effects: Displacement burst (192px), screen shake (1.5), 5 shoot particles per projectile
Projectile: SiamoZeroCrescentProjectile (speed 280, cyan color)]],

    EnergySwordCombo = [[6-hit teleporting sword combo with final shockwave
Sprite: energy_sword (6 sub-anims a-f)
SFX: Els_Teleport (per teleport), Els_Slice (per slash), Els_BigHit (finale)
Effects: 0.18s between hits, random offset teleports near Kglobal::global::Celeste.Player(80x60px)
  Each hit: displacement burst (128px), 8 burst particles, energy blade projectile
  Finale: screen shake (1.5), displacement (256px), 16-directional particle burst]],

    TornadoSlash = [[2.5s spiraling tornado attack with trailing slash projectiles
Sprite: tornado_attack, tornado_slash
SFX: Els_Shell_Screamer (start), Els_Slice (per trail), Els_BigHit (landing)
Effects: Spiral path toward Kglobal::global::Celeste.Player(200px→20px radius, 3 rotations)
  Trail: energy blade every 0.15s, 4 burst particles, constant displacement
  Landing: screen shake (2.0), displacement (384px), 8 crescent projectiles in ring]],

    RevolutionSword = [[3 waves of expanding blade rings (5/7/9 blades)
Sprite: revolution_sword (5 sub-anims a-e)
SFX: Els_Slice (per wave)
Effects: 0.5s between waves, displacement per wave (expanding radius 60/100/140px)
  3 shoot particles per blade, increasing range per wave]],

    RisingSpine = [[8 spine pillars rising from ground in a line below Kglobal::Player
Sprite: rising_spine (13 sub-anims a-m)
SFX: Els_Rift (start), Els_Rift (per pillar, 0.12s apart)
Effects: Displacement burst per pillar (64px), 6 burst particles each
  Pillars: 40px spacing, rise from ground at Kglobal::global::Celeste.PlayerY position
  Final: screen shake (1.0)]],

    DownThrust = [[Rise up 120px, pause, then dive thrust to Kglobal::global::Celeste.Playerwith ground shockwave
Sprite: down_thrust (2 sub-anims a-b)
SFX: Els_Build (rise), Els_BigHit (dive + impact)
Effects:
  Rise: 0.4s cubic ease-out, then 0.2s pause
  Dive: 0.25s cubic ease-in, shoot particles during descent
  Impact: screen shake (2.5), displacement (256px), gold flash
  Shockwave: 10 energy blades radiating outward in all directions]],

    DrillStab = [[Rapid forward drill attack (300px distance) leaving bilateral blade trail
Sprite: drill_stab (3 sub-anims a-c)
SFX: Els_Build (start), Els_BigHit (end)
Effects: 0.35s cubic ease-in toward Kglobal::global::Celeste.Playerdirection
  Trail: perpendicular blade pairs every 0.05s, cyan color
  3 burst particles per pair
  End: screen shake (1.5), displacement (192px)]],

    EnergyShower = [[0.6s charge then rain of 30 energy crescent projectiles from above
Sprite: thirty_energy_shower (5 sub-anims a-e)
SFX: Els_Charge (charge), Els_BeamSlash (fire), Els_Rift_Bullet (every 5th projectile)
Effects:
  Charge: gold flash, displacement (256px)
  Rain: 30 projectiles over 1.5s (0.05s apart)
  Spawn: ±200px of Kglobal::global::Celeste.PlayerX, above camera top, varied angles and speeds
  Final: screen shake (1.0)]],

        ConqueredPeakCascade = [[Celeste FinalBoss-inspired diagonal rush chain.
Teleports to alternating corners around the Kglobal::Player, telegraphs with dash markers, then slices through the arena with crossing sword dives.
Best suited for Delta and Celestial variants where the form is larger and more aggressive.]],

    -- Phase 3 (Siamo Zero) - Morpho Knight Fake attacks
    VortexStrike = [[Vortex summon (0.6s) → pull-in phase (1.5s) → explosive strike
Sprite: vortex_summon, vortex_pull, vortex_strike
SFX: Els_Time_Manipulator_Start (summon), Els_Time_Manipulator_Start (pull), Els_BigHit (strike)
Effects:
  Summon: displacement (384px), purple flash
  Pull: inward displacement (-0.8), 4 inward particles/frame for 1.5s
  Strike: screen shake (3.0), magenta flash, displacement (512px)
  Explosion: 12 energy blades radiating outward (magenta, speed 300)]],

    DoubleSideSlash = [[Two sweeping crescent slash fans from left and right sides
Sprite: double_side_slash
SFX: Els_Slice (per slash)
Effects:
  Left slash: 6 crescent projectiles in 180° arc from left (purple), displacement
  0.35s delay
  Right slash: 6 crescent projectiles in 180° arc from right (magenta), displacement
  Screen shake (1.0) per slash]],

    MorphoEmerge = [[Vanish (0.8s) then emerge from below Kglobal::global::Celeste.Playerwith massive upward pillar strike
Sprite: emerge, c_emerge
SFX: Els_Teleport (vanish), Els_Darkmatter_Spawn (emerge), Els_BigHit (impact)
Effects:
  Vanish: boss turns invisible, displacement (128px)
  Emerge: teleport below Kglobal::global::Celeste.Player(+80px Y), full visibility restore
  Impact: screen shake (2.5), magenta flash, displacement (384px)
  Pillar: 6 spine pillars rising upward (48px spacing), 4 burst particles each
  Rise: cubic ease-out rise 140px over 0.4s]],

    TimeborderCollapse = [[Activates 120-frame timeborder overlay + 3 projectile waves + final burst
Sprite: siamo_zero_timeborders/timeborders (120 frames, 0.06s per frame)
SFX: Els_Final_Cry, Els_Time_Manipulator_Start (start)
     Els_Shellcrack (per wave), Els_Time_Manipulator_End (finale)
Effects:
  Start: red flash, screen shake (3.0), displacement (768px)
  Timeborder: pulsing 35±15% alpha overlay for duration
  3 Waves (0.8s apart): projectiles from screen left/right (6/8/10 per side)
  Finale: white flash, screen shake (4.0), displacement (1024px)
  Final burst: 24 crescent projectiles radiating from center (red, varied speed)
    Total duration: ~5s including 2s fade-out]],

        SummonGalactaKnightClone = [[Spawns a roaming Galacta Knight clone at Els' left flank.
Clone behavior: orbiting duelist with crescent volleys, dash slashes, warp strikes, and radial sword bursts.
    Sequence use: can be inserted into attackSequence or called with a custom delay using SummonGalactaKnightClone:1.5]],

        SummonMorphoKnightClone = [[Spawns a roaming Morpho Knight clone at Els' right flank.
Clone behavior: orbiting duelist with warp strikes, crescent volleys, dash slashes, and radial blade bursts.
Sequence use: can be inserted into attackSequence or called with a custom delay using SummonMorphoKnightClone:1.5]],

        ClearKnightClones = [[Despawns any active knight clones previously summoned by the Els attack sequence.
Useful for transition beats, cleanup before cutscenes, or phase handoff scripting.]]
}

function siamoZeroFinalBoss.ignoredFields(entity)
    return {}
end

return siamoZeroFinalBoss
