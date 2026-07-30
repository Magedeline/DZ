namespace Celeste.Entities.Chapters
{
    /// <summary>
    /// FMOD event paths for the chapter 10-16 entities, kept in one place so a bank
    /// rebuild only has to be reconciled against this file rather than hunted for
    /// across twenty entities.
    ///
    /// Every path here exists in GUIDs.txt. If FMOD renames an event, change it once
    /// here; if an event is deleted, the compiler will not catch it, so re-run the
    /// GUIDs cross-check after a bank rebuild.
    /// </summary>
    public static class ChapterSfx
    {
        #region Chapter 10 - Ruins
        public const string RuinsPillarBreak = "event:/DZ/game/10_ruins/pillar_break";
        public const string RuinsBigPillar = "event:/DZ/game/10_ruins/big_pillar";
        public const string RuinsTouchSwitchAny = "event:/DZ/game/10_ruins/touchswitch_any";
        public const string RuinsTouchSwitchLast = "event:/DZ/game/10_ruins/touchswitch_last";
        public const string RuinsTouchSwitchLastOneshot = "event:/DZ/game/10_ruins/touchswitch_last_oneshot";
        public const string RuinsTouchSwitchLastCutoff = "event:/DZ/game/10_ruins/touchswitch_last_cutoff";
        public const string RuinsGateOpen = "event:/DZ/game/10_ruins/touchswitch_gate_open";
        public const string RuinsGateFinish = "event:/DZ/game/10_ruins/touchswitch_gate_finish";
        public const string RuinsMouse = "event:/DZ/game/10_ruins/mouse";
        #endregion

        #region Chapter 11 - Snowding City
        public const string SnowIceIdle = "event:/DZ/game/11_snowdin_city/ice_idle";
        public const string SnowSwing = "event:/DZ/game/11_snowdin_city/swing";
        public const string SnowBoneBreak = "event:/DZ/game/11_snowdin_city/bone_break";
        public const string SnowHit = "event:/DZ/game/11_snowdin_city/hit";
        public const string SnowSmash = "event:/DZ/game/11_snowdin_city/smash";
        public const string SnowRolling = "event:/DZ/game/11_snowdin_city/rolling";
        public const string SnowTimesUp = "event:/DZ/game/11_snowdin_city/timesup";
        #endregion

        #region Chapter 12 - Wateredgefall
        public const string WaterSplash = "event:/DZ/game/12_wateredgefall/water_splash";
        public const string WaterSpearAppear = "event:/DZ/game/12_wateredgefall/spear_appear";
        public const string WaterSpearThrow = "event:/DZ/game/12_wateredgefall/spear_throw";
        public const string WaterWingShut = "event:/DZ/game/12_wateredgefall/wingshut";
        public const string WaterTitanBeam = "event:/DZ/game/12_wateredgefall/titan_beam";
        #endregion

        #region Chapter 13 - Hotcliff
        public const string HotTreadmill = "event:/DZ/game/13_hotcliff/treadmill";
        public const string HotTurretCharge = "event:/DZ/game/13_hotcliff/turret_charge";
        public const string HotTurretFire = "event:/DZ/game/13_hotcliff/turret_fire";
        public const string HotLaserFire = "event:/DZ/game/13_hotcliff/laser_fire";
        #endregion

        #region Chapter 14 - Cyber Nexus
        public const string NexusEnter = "event:/DZ/game/14_cybernexus/enter";
        public const string NexusSwing = "event:/DZ/game/14_cybernexus/swing";
        public const string NexusHit = "event:/DZ/game/14_cybernexus/hit";
        #endregion

        #region Chapter 15 - Citadel
        public const string CitadelTarget = "event:/DZ/game/15_citadel/target";
        public const string CitadelFire = "event:/DZ/game/15_citadel/fire";
        #endregion

        #region Chapter 16 - MyWorld
        public const string MyWorldNukeDrop = "event:/DZ/game/16_myworld/nuke_drop";
        public const string MyWorldNukeExplosion = "event:/DZ/game/16_myworld/nuke_explosion";
        public const string MyWorldBigNukeDrop = "event:/DZ/game/16_myworld/big_nuke_drop";
        public const string MyWorldBigNukeExplosion = "event:/DZ/game/16_myworld/big_nuke_explosion";
        public const string MyWorldFreaksOut1 = "event:/DZ/game/16_myworld/freaksout_01";
        public const string MyWorldFreaksOut2 = "event:/DZ/game/16_myworld/freaksout_02";
        public const string MyWorldFreaksOut3 = "event:/DZ/game/16_myworld/freaksout_03";
        public const string MyWorldFreaksOut4 = "event:/DZ/game/16_myworld/freaksout_04";
        public const string MyWorldFreaksOut5 = "event:/DZ/game/16_myworld/freaksout_05";
        #endregion
    }
}
