// SoundBank.cs — Typed FMOD audio event constants for KIRBY_CELESTE
// Generated from Audio/GUIDs.txt — all custom pusheen bank events.
//
// Usage:
//   Audio.Play(SoundBank.Char.Kirby.Jump, Position);
//   session.Audio.Music.Event = SoundBank.Music.Lvl8.Main;
//   Audio.SetAmbience(SoundBank.Env.Amb.Ch06_Lake);
//   var snap = Audio.CreateSnapshot(SoundBank.Snapshots.BossPitchSfx);
//   Audio.ReleaseSnapshot(snap);
//
// Banks loaded in: MaggyHelperModule.LoadAudioBanks()
//   Audio/Master_Bank  +  Audio/pusheen_audio_A/B/C/D

namespace Celeste.Mod.MaggyHelper
{
    /// <summary>
    /// Centralized, typed constants for every custom FMOD event and snapshot
    /// in the KIRBY_CELESTE audio banks. Mirrors Celeste's own <c>SFX</c> class pattern.
    /// </summary>
    public static class SoundBank
    {
        // ─── Music ────────────────────────────────────────────────────────────

        public static class Music
        {
            // event:/music/pusheen/lvl0/*
            public static class Lvl0
            {
                public const string Bridge      = "event:/music/pusheen/lvl0/bridge";
                public const string ChangeColor = "event:/music/pusheen/lvl0/change_color";
                public const string Creation    = "event:/music/pusheen/lvl0/creation";
                public const string Drone       = "event:/music/pusheen/lvl0/drone";
                public const string Intro       = "event:/music/pusheen/lvl0/intro";
                public const string SoulAppear  = "event:/music/pusheen/lvl0/soul_appear";
                public const string TitlePing   = "event:/music/pusheen/lvl0/title_ping";
            }

            // event:/music/pusheen/lvl1/*
            public static class Lvl1
            {
                public const string Magolor = "event:/music/pusheen/lvl1/magolor";
                public const string Main    = "event:/music/pusheen/lvl1/main";
            }

            // event:/music/pusheen/lvl2/*
            public static class Lvl2
            {
                public const string Awake               = "event:/music/pusheen/lvl2/awake";
                public const string Beginning           = "event:/music/pusheen/lvl2/beginning";
                public const string Chase               = "event:/music/pusheen/lvl2/chase";
                public const string DreamblockStingPt1  = "event:/music/pusheen/lvl2/dreamblock_sting_pt1";
                public const string DreamblockStingPt2  = "event:/music/pusheen/lvl2/dreamblock_sting_pt2";
                public const string EvilMadeline        = "event:/music/pusheen/lvl2/evil_madeline";
                public const string Mirror              = "event:/music/pusheen/lvl2/mirror";
                public const string PhoneEnd            = "event:/music/pusheen/lvl2/phone_end";
                public const string PhoneLoop           = "event:/music/pusheen/lvl2/phone_loop";
            }

            // event:/music/pusheen/lvl3/*
            public static class Lvl3
            {
                public const string Intro = "event:/music/pusheen/lvl3/intro";
                public const string Main  = "event:/music/pusheen/lvl3/main";
                public const string Theo  = "event:/music/pusheen/lvl3/theo";
            }

            // event:/music/pusheen/lvl4/*
            public static class Lvl4
            {
                public const string Awake               = "event:/music/pusheen/lvl4/awake";
                public const string Chase               = "event:/music/pusheen/lvl4/chase";
                public const string Dream               = "event:/music/pusheen/lvl4/dream";
                public const string DreamblockStingPt1  = "event:/music/pusheen/lvl4/dreamblock_sting_pt1";
                public const string DreamblockStingPt2  = "event:/music/pusheen/lvl4/dreamblock_sting_pt2";
                public const string Legend              = "event:/music/pusheen/lvl4/legend";
                public const string Mirror              = "event:/music/pusheen/lvl4/mirror";
                public const string PhoneEnd            = "event:/music/pusheen/lvl4/phone_end";
                public const string PhoneLoop           = "event:/music/pusheen/lvl4/phone_loop";
                public const string Warning             = "event:/music/pusheen/lvl4/warning";
            }

            // event:/music/pusheen/lvl5/*
            public static class Lvl5
            {
                public const string Clean       = "event:/music/pusheen/lvl5/clean";
                public const string Explore     = "event:/music/pusheen/lvl5/explore";
                public const string Intro       = "event:/music/pusheen/lvl5/intro";
                public const string OshiroChase = "event:/music/pusheen/lvl5/oshiro_chase";
                public const string OshiroTheme = "event:/music/pusheen/lvl5/oshiro_theme";
            }

            // event:/music/pusheen/lvl6/*
            public static class Lvl6
            {
                public const string HeavyWinds = "event:/music/pusheen/lvl6/heavy_winds";
                public const string Main       = "event:/music/pusheen/lvl6/main";
                public const string Minigame   = "event:/music/pusheen/lvl6/minigame";
            }

            // event:/music/pusheen/lvl7/*
            public static class Lvl7
            {
                public const string Him              = "event:/music/pusheen/lvl7/him";
                public const string MiddleTemple     = "event:/music/pusheen/lvl7/middle_temple";
                public const string Mirror           = "event:/music/pusheen/lvl7/mirror";
                public const string MirrorCutscene   = "event:/music/pusheen/lvl7/mirror_cutscene";
                public const string Normal           = "event:/music/pusheen/lvl7/normal";
                public const string SeekerDrumsLayer = "event:/music/pusheen/lvl7/seeker_drums_layer";
            }

            // event:/music/pusheen/lvl8/*
            public static class Lvl8
            {
                public const string CharaAcoustic = "event:/music/pusheen/lvl8/chara_acoustic";
                public const string CharaCore     = "event:/music/pusheen/lvl8/chara_core";
                public const string CharaFight    = "event:/music/pusheen/lvl8/chara_fight";
                public const string CharaGlitch   = "event:/music/pusheen/lvl8/chara_glitch";
                public const string Main          = "event:/music/pusheen/lvl8/main";
                public const string SecretRoom    = "event:/music/pusheen/lvl8/secret_room";
                public const string Starjump      = "event:/music/pusheen/lvl8/starjump";
                public const string TheFall       = "event:/music/pusheen/lvl8/the_fall";
                public const string Vibing        = "event:/music/pusheen/lvl8/vibing";
            }

            // event:/music/pusheen/lvl9/*
            public static class Lvl9
            {
                public const string FinalAscent = "event:/music/pusheen/lvl9/final_ascent";
                public const string Main        = "event:/music/pusheen/lvl9/main";
            }

            // event:/music/pusheen/lvl10/*
            public static class Lvl10
            {
                public const string BossBurn  = "event:/music/pusheen/lvl10/boss_burn";
                public const string BossFight = "event:/music/pusheen/lvl10/boss_fight";
                public const string Dreemurr  = "event:/music/pusheen/lvl10/dreemurr";
                public const string Fallen    = "event:/music/pusheen/lvl10/fallen";
                public const string Flowey    = "event:/music/pusheen/lvl10/flowey";
                public const string FloweyAlt = "event:/music/pusheen/lvl10/floweyalt";
                public const string Indoors   = "event:/music/pusheen/lvl10/indoors";
                public const string Intro     = "event:/music/pusheen/lvl10/intro";
                public const string Main      = "event:/music/pusheen/lvl10/main";
            }

            // event:/music/pusheen/lvl11/*
            public static class Lvl11
            {
                public const string BossBlack         = "event:/music/pusheen/lvl11/boss_black";
                public const string BossBlackEx       = "event:/music/pusheen/lvl11/boss_black_ex";
                public const string BossReb           = "event:/music/pusheen/lvl11/boss_reb";
                public const string BossRem           = "event:/music/pusheen/lvl11/boss_rem";
                public const string City              = "event:/music/pusheen/lvl11/city";
                public const string DatingStart       = "event:/music/pusheen/lvl11/dating_start";
                public const string DogAndBirdsong    = "event:/music/pusheen/lvl11/dogandbirdsong";
                public const string DogBass           = "event:/music/pusheen/lvl11/dogbass";
                public const string Main              = "event:/music/pusheen/lvl11/main";
                public const string MysteryPlace      = "event:/music/pusheen/lvl11/mysteryplace";
                public const string Papyrus           = "event:/music/pusheen/lvl11/papyrus";
                public const string Sans              = "event:/music/pusheen/lvl11/sans";
                public const string Shop              = "event:/music/pusheen/lvl11/shop";
                public const string SpaghettiChallenge = "event:/music/pusheen/lvl11/spagetti_challenge";
            }

            // event:/music/pusheen/lvl12/*
            public static class Lvl12
            {
                public const string ApexTheme           = "event:/music/pusheen/lvl12/apextheme";
                public const string BossApex            = "event:/music/pusheen/lvl12/boss_apex";
                public const string BossDummy           = "event:/music/pusheen/lvl12/boss_dummy";
                public const string BossTitan           = "event:/music/pusheen/lvl12/boss_titan";
                public const string DangerMystery       = "event:/music/pusheen/lvl12/danger_mystery";
                public const string GuardianIntro       = "event:/music/pusheen/lvl12/guardian_intro";
                public const string Main                = "event:/music/pusheen/lvl12/main";
                public const string MovingOnward        = "event:/music/pusheen/lvl12/movingonward";
                public const string PreThrowingPractices = "event:/music/pusheen/lvl12/pre_throwing_practices";
                public const string PreDummy            = "event:/music/pusheen/lvl12/predummy";
                public const string Quiet               = "event:/music/pusheen/lvl12/quiet";
                public const string Refused             = "event:/music/pusheen/lvl12/refused";
                public const string Run                 = "event:/music/pusheen/lvl12/run";
                public const string Shop                = "event:/music/pusheen/lvl12/shop";
                public const string TemVillage          = "event:/music/pusheen/lvl12/tem_village";
                public const string TemmieRoom          = "event:/music/pusheen/lvl12/temmie_room";
                public const string ThrowingPractices   = "event:/music/pusheen/lvl12/throwing_practices";
                public const string TitanTower          = "event:/music/pusheen/lvl12/titan_tower";
                public const string Vessel              = "event:/music/pusheen/lvl12/vessel";
                public const string Wish                = "event:/music/pusheen/lvl12/wish";
            }

            // event:/music/pusheen/lvl13/*
            public static class Lvl13
            {
                public const string AlphysTheme      = "event:/music/pusheen/lvl13/alphys_theme";
                public const string Angel            = "event:/music/pusheen/lvl13/angel";
                public const string Battle           = "event:/music/pusheen/lvl13/battle";
                public const string BossAxis         = "event:/music/pusheen/lvl13/boss_axis";
                public const string BossMt           = "event:/music/pusheen/lvl13/boss_mt";
                public const string BossRage         = "event:/music/pusheen/lvl13/boss_rage";
                public const string BossTenna        = "event:/music/pusheen/lvl13/boss_tenna";
                public const string Dig              = "event:/music/pusheen/lvl13/dig";
                public const string Doom             = "event:/music/pusheen/lvl13/doom";
                public const string Endless          = "event:/music/pusheen/lvl13/endless";
                public const string Explore          = "event:/music/pusheen/lvl13/explore";
                public const string Intro            = "event:/music/pusheen/lvl13/intro";
                public const string Island           = "event:/music/pusheen/lvl13/island";
                public const string Main             = "event:/music/pusheen/lvl13/main";
                public const string MtGameshowShort  = "event:/music/pusheen/lvl13/mtgameshow_short";
                public const string OffBreak         = "event:/music/pusheen/lvl13/offbreak";
                public const string OnBreak          = "event:/music/pusheen/lvl13/onbreak";
                public const string Starrod          = "event:/music/pusheen/lvl13/starrod";
                public const string SuzyTheme        = "event:/music/pusheen/lvl13/suzy_theme";
                public const string TennaFlashBack   = "event:/music/pusheen/lvl13/tenna_flash_back";
                public const string TvTime           = "event:/music/pusheen/lvl13/tvtime";
                public const string TvWorld          = "event:/music/pusheen/lvl13/tvworld";
            }

            // event:/music/pusheen/lvl14/*
            public static class Lvl14
            {
                public const string BossDddd      = "event:/music/pusheen/lvl14/boss_dddd";
                public const string BossGiga      = "event:/music/pusheen/lvl14/boss_giga";
                public const string BossGigaEx    = "event:/music/pusheen/lvl14/boss_gigaex";
                public const string Core          = "event:/music/pusheen/lvl14/core";
                public const string Cyber         = "event:/music/pusheen/lvl14/cyber";
                public const string IntroTraitor  = "event:/music/pusheen/lvl14/intro_traitor";
            }

            // event:/music/pusheen/lvl15/*
            public static class Lvl15
            {
                public const string Boss      = "event:/music/pusheen/lvl15/boss";
                public const string BossIntro = "event:/music/pusheen/lvl15/boss_intro";
                public const string Corridor  = "event:/music/pusheen/lvl15/corridor";
                public const string ElsIntro  = "event:/music/pusheen/lvl15/els_intro";
                public const string Main      = "event:/music/pusheen/lvl15/main";
                public const string Part01    = "event:/music/pusheen/lvl15/part01";
                public const string Part02    = "event:/music/pusheen/lvl15/part02";
                public const string Part03    = "event:/music/pusheen/lvl15/part03";
                public const string Shock     = "event:/music/pusheen/lvl15/shock";
                public const string Starrod   = "event:/music/pusheen/lvl15/starrod";
            }

            // event:/music/pusheen/lvl16/*
            public static class Lvl16
            {
                public const string Finale          = "event:/music/pusheen/lvl16/finale";
                public const string FirstLaugh      = "event:/music/pusheen/lvl16/first_laugh";
                public const string Funni           = "event:/music/pusheen/lvl16/funni";
                public const string Glock           = "event:/music/pusheen/lvl16/glock";
                public const string Intro           = "event:/music/pusheen/lvl16/intro";
                public const string Karma           = "event:/music/pusheen/lvl16/karma";
                public const string KarmaKirby      = "event:/music/pusheen/lvl16/karma_kirby";
                public const string KarmaKirbyWithPpSpeedrunBerry  = "event:/music/pusheen/lvl16/karma_kirby_with_pp_speedrun_berry";
                public const string KarmaKirbyWithSpeedrunBerry    = "event:/music/pusheen/lvl16/karma_kirby_with_speedrun_berry";
                public const string KarmaWithSpeedrunBerry         = "event:/music/pusheen/lvl16/karma_with_speedrun_berry";
                public const string LastLaugh       = "event:/music/pusheen/lvl16/last_laugh";
                public const string Outro           = "event:/music/pusheen/lvl16/outro";
                public const string Part1           = "event:/music/pusheen/lvl16/part1";
                public const string Part2           = "event:/music/pusheen/lvl16/part2";
                public const string Part3           = "event:/music/pusheen/lvl16/part3";
                public const string Part4           = "event:/music/pusheen/lvl16/part4";
                public const string Part5           = "event:/music/pusheen/lvl16/part5";
                public const string Part6           = "event:/music/pusheen/lvl16/part6";
                public const string Part7           = "event:/music/pusheen/lvl16/part7";
                public const string Part8           = "event:/music/pusheen/lvl16/part8";
                public const string PreIntro        = "event:/music/pusheen/lvl16/pre_intro";
                public const string Saved           = "event:/music/pusheen/lvl16/saved";
                public const string Soul1           = "event:/music/pusheen/lvl16/soul1";
                public const string Soul2           = "event:/music/pusheen/lvl16/soul2";
                public const string Soul3           = "event:/music/pusheen/lvl16/soul3";
                public const string Soul4           = "event:/music/pusheen/lvl16/soul4";
                public const string Soul5           = "event:/music/pusheen/lvl16/soul5";
                public const string Soul6           = "event:/music/pusheen/lvl16/soul6";
                public const string Soul7           = "event:/music/pusheen/lvl16/soul7";
                public const string Soul8           = "event:/music/pusheen/lvl16/soul8";
                public const string Transition      = "event:/music/pusheen/lvl16/transition";
            }

            // event:/music/pusheen/lvl17/*
            public static class Lvl17
            {
                public const string Main = "event:/music/pusheen/lvl17/main";
            }

            // event:/music/pusheen/lvl18/*
            public static class Lvl18
            {
                public const string Main = "event:/music/pusheen/lvl18/main";
            }

            // event:/music/pusheen/arena/*
            public static class Arena
            {
                public const string Battle1  = "event:/music/pusheen/arena/battle_1";
                public const string Battle2  = "event:/music/pusheen/arena/battle_2";
                public const string Battle3  = "event:/music/pusheen/arena/battle_3";
                public const string Battle4  = "event:/music/pusheen/arena/battle_4";
                public const string Battle5  = "event:/music/pusheen/arena/battle_5";
                public const string Battle6  = "event:/music/pusheen/arena/battle_6";
                public const string Battle7  = "event:/music/pusheen/arena/battle_7";
                public const string Battle8  = "event:/music/pusheen/arena/battle_8";
                public const string Battle9  = "event:/music/pusheen/arena/battle_9";
                public const string Battle10 = "event:/music/pusheen/arena/battle_10";
                public const string Battle11 = "event:/music/pusheen/arena/battle_11";
                public const string Battle12 = "event:/music/pusheen/arena/battle_12";
                public const string Battle13 = "event:/music/pusheen/arena/battle_13";
            }

            // event:/music/pusheen/bside/*
            public static class BSide
            {
                public const string Metro         = "event:/music/pusheen/bside/01_metro";
                public const string Shadow        = "event:/music/pusheen/bside/02_shadow";
                public const string Arrival       = "event:/music/pusheen/bside/03_arrival";
                public const string Legend        = "event:/music/pusheen/bside/04_legend";
                public const string Fractured     = "event:/music/pusheen/bside/05_fractured";
                public const string Stronghold    = "event:/music/pusheen/bside/06_stronghold";
                public const string Inferno       = "event:/music/pusheen/bside/07_inferno";
                public const string Edge          = "event:/music/pusheen/bside/08_edge";
                public const string BeyondSummit  = "event:/music/pusheen/bside/09_beyondsummit";
                public const string Ruins         = "event:/music/pusheen/bside/10_ruins";
                public const string SnowdinCity   = "event:/music/pusheen/bside/11_snowdincity";
                public const string WaterEdgeFall = "event:/music/pusheen/bside/12_wateredgefall";
                public const string HotCliff      = "event:/music/pusheen/bside/13_hotcliff";
                public const string CyberNexus    = "event:/music/pusheen/bside/14_cybernexus";
                public const string Citadel       = "event:/music/pusheen/bside/15_citadel";
                public const string MyWorld       = "event:/music/pusheen/bside/16_myworld";
                public const string Core          = "event:/music/pusheen/bside/18_core";
            }

            // event:/music/pusheen/cassette/*
            public static class Cassette
            {
                public const string Metro         = "event:/music/pusheen/cassette/01_metro";
                public const string Shadow        = "event:/music/pusheen/cassette/02_shadow";
                public const string Arrival       = "event:/music/pusheen/cassette/03_arrival";
                public const string Legend        = "event:/music/pusheen/cassette/04_legend";
                public const string Fractured     = "event:/music/pusheen/cassette/05_fractured";
                public const string Stronghold    = "event:/music/pusheen/cassette/06_stronghold";
                public const string Inferno       = "event:/music/pusheen/cassette/07_inferno";
                public const string Edge          = "event:/music/pusheen/cassette/08_edge";
                public const string BeyondSummit  = "event:/music/pusheen/cassette/09_beyondsummit";
                public const string Ruins         = "event:/music/pusheen/cassette/10_ruins";
                public const string SnowdinCity   = "event:/music/pusheen/cassette/11_snowdincity";
                public const string WaterEdgeFall = "event:/music/pusheen/cassette/12_wateredgefall";
                public const string HotCliff      = "event:/music/pusheen/cassette/13_hotcliff";
                public const string CyberNexus    = "event:/music/pusheen/cassette/14_cybernexus";
                public const string Citadel       = "event:/music/pusheen/cassette/15_citadel";
                public const string MyWorld       = "event:/music/pusheen/cassette/16_myworld";
                public const string Core          = "event:/music/pusheen/cassette/18_core";
            }

            // event:/music/pusheen/cside/*
            public static class CSide
            {
                public const string Metro         = "event:/music/pusheen/cside/01_metro";
                public const string Shadow        = "event:/music/pusheen/cside/02_shadow";
                public const string Arrival       = "event:/music/pusheen/cside/03_arrival";
                public const string Legend        = "event:/music/pusheen/cside/04_legend";
                public const string Fractured     = "event:/music/pusheen/cside/05_fractured";
                public const string Stronghold    = "event:/music/pusheen/cside/06_stronghold";
                public const string Inferno       = "event:/music/pusheen/cside/07_inferno";
                public const string Edge          = "event:/music/pusheen/cside/08_edge";
                public const string BeyondSummit  = "event:/music/pusheen/cside/09_beyondsummit";
                public const string Ruins         = "event:/music/pusheen/cside/10_ruins";
                public const string SnowdinCity   = "event:/music/pusheen/cside/11_snowdincity";
                public const string WaterEdgeFall = "event:/music/pusheen/cside/12_wateredgefall";
                public const string HotCliff      = "event:/music/pusheen/cside/13_hotcliff";
                public const string CyberNexus    = "event:/music/pusheen/cside/14_cybernexus";
                public const string Citadel       = "event:/music/pusheen/cside/15_citadel";
                public const string MyWorld       = "event:/music/pusheen/cside/16_myworld";
                public const string Core          = "event:/music/pusheen/cside/18_core";
            }

            // event:/music/pusheen/dxside/*
            public static class DXSide
            {
                public const string Metro         = "event:/music/pusheen/dxside/01_metro";
                public const string Shadow        = "event:/music/pusheen/dxside/02_shadow";
                public const string Arrival       = "event:/music/pusheen/dxside/03_arrival";
                public const string Legend        = "event:/music/pusheen/dxside/04_legend";
                public const string Fractured     = "event:/music/pusheen/dxside/05_fractured";
                public const string Stronghold    = "event:/music/pusheen/dxside/06_stronghold";
                public const string Inferno       = "event:/music/pusheen/dxside/07_inferno";
                public const string Edge          = "event:/music/pusheen/dxside/08_edge";
                public const string BeyondSummit  = "event:/music/pusheen/dxside/09_beyondsummit";
                public const string Ruins         = "event:/music/pusheen/dxside/10_ruins";
                public const string SnowdinCity   = "event:/music/pusheen/dxside/11_snowdincity";
                public const string WaterEdgeFall = "event:/music/pusheen/dxside/12_wateredgefall";
                public const string HotCliff      = "event:/music/pusheen/dxside/13_hotcliff";
                public const string CyberNexus    = "event:/music/pusheen/dxside/14_cybernexus";
                public const string Citadel       = "event:/music/pusheen/dxside/15_citadel";
                public const string MyWorld       = "event:/music/pusheen/dxside/16_myworld";
                public const string Core18        = "event:/music/pusheen/dxside/18_core";
                public const string Core19        = "event:/music/pusheen/dxside/19_core";
            }

            // event:/music/pusheen/lobby/*
            public static class Lobby
            {
                public const string BarRestaurant = "event:/music/pusheen/lobby/Bar_Resturaunt";
                public const string Date          = "event:/music/pusheen/lobby/Date";
                public const string Grillby       = "event:/music/pusheen/lobby/Grillby";
                public const string Hotel         = "event:/music/pusheen/lobby/hotel";
            }

            // event:/music/pusheen/menu/*
            public static class Menu
            {
                public const string Cast                  = "event:/music/pusheen/menu/cast";
                public const string CompleteArea          = "event:/music/pusheen/menu/complete_area";
                public const string CompleteBside         = "event:/music/pusheen/menu/complete_bside";
                public const string CompleteBsideSummit   = "event:/music/pusheen/menu/complete_bside_summit";
                public const string CompleteCside         = "event:/music/pusheen/menu/complete_cside";
                public const string CompleteCsideSummit   = "event:/music/pusheen/menu/complete_cside_summit";
                public const string CompleteDxside        = "event:/music/pusheen/menu/complete_dxside";
                public const string CompleteSummit        = "event:/music/pusheen/menu/complete_summit";
                public const string Credits               = "event:/music/pusheen/menu/credits";
                public const string DodgeCredits          = "event:/music/pusheen/menu/dodge_credits";
                public const string Gameover              = "event:/music/pusheen/menu/gameover";
                public const string GameoverSlowdown      = "event:/music/pusheen/menu/gameover_slowdown";
            }
        }

        // ─── Characters ───────────────────────────────────────────────────────

        public static class Char
        {
            // event:/char/pusheen/kirby/*
            public static class Kirby
            {
                public const string BackpackDrop          = "event:/char/pusheen/kirby/backpack_drop";
                public const string CampfireSit          = "event:/char/pusheen/kirby/campfire_sit";
                public const string CampfireStand        = "event:/char/pusheen/kirby/campfire_stand";
                public const string ClimbLedge           = "event:/char/pusheen/kirby/climb_ledge";
                public const string CoreHairCharged      = "event:/char/pusheen/kirby/core_hair_charged";
                public const string CrystalTheoLift      = "event:/char/pusheen/kirby/crystaltheo_lift";
                public const string CrystalTheoThrow     = "event:/char/pusheen/kirby/crystaltheo_throw";
                public const string DashPinkLeft         = "event:/char/pusheen/kirby/dash_pink_left";
                public const string DashPinkRight        = "event:/char/pusheen/kirby/dash_pink_right";
                public const string DashRedLeft          = "event:/char/pusheen/kirby/dash_red_left";
                public const string DashRedRight         = "event:/char/pusheen/kirby/dash_red_right";
                public const string Death                = "event:/char/pusheen/kirby/death";
                public const string DreamblockEnter      = "event:/char/pusheen/kirby/dreamblock_enter";
                public const string DreamblockExit       = "event:/char/pusheen/kirby/dreamblock_exit";
                public const string DreamblockTravel     = "event:/char/pusheen/kirby/dreamblock_travel";
                public const string Duck                 = "event:/char/pusheen/kirby/duck";
                public const string Footstep             = "event:/char/pusheen/kirby/footstep";
                public const string Grab                 = "event:/char/pusheen/kirby/grab";
                public const string GrabLetgo            = "event:/char/pusheen/kirby/grab_letgo";
                public const string Handhold             = "event:/char/pusheen/kirby/handhold";
                public const string IdleCrackKnuckles    = "event:/char/pusheen/kirby/idle_crackknuckles";
                public const string IdleScratch          = "event:/char/pusheen/kirby/idle_scratch";
                public const string IdleSneeze           = "event:/char/pusheen/kirby/idle_sneeze";
                public const string Jump                 = "event:/char/pusheen/kirby/jump";
                public const string JumpAssisted         = "event:/char/pusheen/kirby/jump_assisted";
                public const string JumpClimbLeft        = "event:/char/pusheen/kirby/jump_climb_left";
                public const string JumpClimbRight       = "event:/char/pusheen/kirby/jump_climb_right";
                public const string JumpDreamblock       = "event:/char/pusheen/kirby/jump_dreamblock";
                public const string JumpSpecial          = "event:/char/pusheen/kirby/jump_special";
                public const string JumpSuper            = "event:/char/pusheen/kirby/jump_super";
                public const string JumpSuperslide       = "event:/char/pusheen/kirby/jump_superslide";
                public const string JumpSuperwall        = "event:/char/pusheen/kirby/jump_superwall";
                public const string JumpWallLeft         = "event:/char/pusheen/kirby/jump_wall_left";
                public const string JumpWallRight        = "event:/char/pusheen/kirby/jump_wall_right";
                public const string Landing              = "event:/char/pusheen/kirby/landing";
                public const string MirrorTempleBigLanding = "event:/char/pusheen/kirby/mirrortemple_big_landing";
                public const string Predeath             = "event:/char/pusheen/kirby/predeath";
                public const string Revive               = "event:/char/pusheen/kirby/revive";
                public const string Stand                = "event:/char/pusheen/kirby/stand";
                public const string SummitAreaStart      = "event:/char/pusheen/kirby/summit_areastart";
                public const string SummitFlyToNext      = "event:/char/pusheen/kirby/summit_flytonext";
                public const string SummitSit            = "event:/char/pusheen/kirby/summit_sit";
                public const string TheoCollapse         = "event:/char/pusheen/kirby/theo_collapse";
                public const string Wallslide            = "event:/char/pusheen/kirby/wallslide";
                public const string WaterDashGen         = "event:/char/pusheen/kirby/water_dash_gen";
                public const string WaterDashIn          = "event:/char/pusheen/kirby/water_dash_in";
                public const string WaterDashOut         = "event:/char/pusheen/kirby/water_dash_out";
                public const string WaterIn              = "event:/char/pusheen/kirby/water_in";
                public const string WaterMoveGeneral     = "event:/char/pusheen/kirby/water_move_general";
                public const string WaterMoveShallow     = "event:/char/pusheen/kirby/water_move_shallow";
                public const string WaterOut             = "event:/char/pusheen/kirby/water_out";

                // event:/new_content/char/pusheen/kirby/* (Chapter 19 additions)
                public const string BounceBoost             = "event:/new_content/char/pusheen/kirby/bounce_boost";
                public const string DeathGolden             = "event:/new_content/char/pusheen/kirby/death_golden";
                public const string FlyFinal                = "event:/new_content/char/pusheen/kirby/flyfinal";
                public const string FlyNext                 = "event:/new_content/char/pusheen/kirby/flynext";
                public const string GliderDrop              = "event:/new_content/char/pusheen/kirby/glider_drop";
                public const string HiccupDucking           = "event:/new_content/char/pusheen/kirby/hiccup_ducking";
                public const string HiccupStanding          = "event:/new_content/char/pusheen/kirby/hiccup_standing";
                public const string ScreenEntryGolden       = "event:/new_content/char/pusheen/kirby/screenentry_golden";
                public const string ScreenEntryGran         = "event:/new_content/char/pusheen/kirby/screenentry_gran";
                public const string ScreenEntryGranLanding  = "event:/new_content/char/pusheen/kirby/screenentry_gran_landing";
                public const string ScreenEntryLowGrav      = "event:/new_content/char/pusheen/kirby/screenentry_lowgrav";
                public const string ScreenEntryStuborn      = "event:/new_content/char/pusheen/kirby/screenentry_stubborn";
            }

            // event:/char/pusheen/chara/*
            public static class Chara
            {
                public const string Appear           = "event:/char/pusheen/chara/appear";
                public const string BoosterBegin     = "event:/char/pusheen/chara/booster_begin";
                public const string BoosterFinal     = "event:/char/pusheen/chara/booster_final";
                public const string BoosterReappear  = "event:/char/pusheen/chara/booster_reappear";
                public const string BoosterRelocate  = "event:/char/pusheen/chara/booster_relocate";
                public const string BoosterThrow     = "event:/char/pusheen/chara/booster_throw";
                public const string BossBullet       = "event:/char/pusheen/chara/boss_bullet";
                public const string BossHug          = "event:/char/pusheen/chara/boss_hug";
                public const string BossIdleAir      = "event:/char/pusheen/chara/boss_idle_air";
                public const string BossLaserCharge  = "event:/char/pusheen/chara/boss_laser_charge";
                public const string BossLaserFire    = "event:/char/pusheen/chara/boss_laser_fire";
                public const string BossPrefightGetup = "event:/char/pusheen/chara/boss_prefight_getup";
                public const string CharaJoin        = "event:/char/pusheen/chara/chara_join";
                public const string CharaSplit       = "event:/char/pusheen/chara/chara_split";
                public const string ClimbLedge       = "event:/char/pusheen/chara/climb_ledge";
                public const string DashRedLeft      = "event:/char/pusheen/chara/dash_red_left";
                public const string DashRedRight     = "event:/char/pusheen/chara/dash_red_right";
                public const string Disappear        = "event:/char/pusheen/chara/disappear";
                public const string DreamblockEnter  = "event:/char/pusheen/chara/dreamblock_enter";
                public const string DreamblockExit   = "event:/char/pusheen/chara/dreamblock_exit";
                public const string DreamblockTravel = "event:/char/pusheen/chara/dreamblock_travel";
                public const string Duck             = "event:/char/pusheen/chara/duck";
                public const string Footstep         = "event:/char/pusheen/chara/footstep";
                public const string Grab             = "event:/char/pusheen/chara/grab";
                public const string GrabLetgo        = "event:/char/pusheen/chara/grab_letgo";
                public const string Handhold         = "event:/char/pusheen/chara/handhold";
                public const string Jump             = "event:/char/pusheen/chara/jump";
                public const string JumpAssisted     = "event:/char/pusheen/chara/jump_assisted";
                public const string JumpClimbLeft    = "event:/char/pusheen/chara/jump_climb_left";
                public const string JumpClimbRight   = "event:/char/pusheen/chara/jump_climb_right";
                public const string JumpDreamblock   = "event:/char/pusheen/chara/jump_dreamblock";
                public const string JumpSpecial      = "event:/char/pusheen/chara/jump_special";
                public const string JumpSuper        = "event:/char/pusheen/chara/jump_super";
                public const string JumpSuperslide   = "event:/char/pusheen/chara/jump_superslide";
                public const string JumpSuperwall    = "event:/char/pusheen/chara/jump_superwall";
                public const string JumpWallLeft     = "event:/char/pusheen/chara/jump_wall_left";
                public const string JumpWallRight    = "event:/char/pusheen/chara/jump_wall_right";
                public const string Landing          = "event:/char/pusheen/chara/landing";
                public const string LevelEntry       = "event:/char/pusheen/chara/level_entry";
                public const string Stand            = "event:/char/pusheen/chara/stand";
                public const string TempleMove       = "event:/char/pusheen/chara/temple_move_chats";
                public const string TempleMoveFirst  = "event:/char/pusheen/chara/temple_move_first";
                public const string Wallslide        = "event:/char/pusheen/chara/wallslide";
            }

            // event:/char/pusheen/granny/*
            public static class Granny
            {
                public const string CaneTap          = "event:/char/pusheen/granny/cane_tap";
                public const string LaughFirstPhrase = "event:/char/pusheen/granny/laugh_firstphrase";
                public const string LaughOneHa       = "event:/char/pusheen/granny/laugh_oneha";
            }

            // event:/char/pusheen/oshiro/*
            public static class Oshiro
            {
                public const string BossCharge         = "event:/char/pusheen/oshiro/boss_charge";
                public const string BossEnterScreen    = "event:/char/pusheen/oshiro/boss_enter_screen";
                public const string BossPrecharge      = "event:/char/pusheen/oshiro/boss_precharge";
                public const string BossReform         = "event:/char/pusheen/oshiro/boss_reform";
                public const string BossSlamFinal      = "event:/char/pusheen/oshiro/boss_slam_final";
                public const string BossSlamFirst      = "event:/char/pusheen/oshiro/boss_slam_first";
                public const string BossTransformBegin = "event:/char/pusheen/oshiro/boss_transform_begin";
                public const string BossTransformBurst = "event:/char/pusheen/oshiro/boss_transform_burst";
                public const string ChatCollapse       = "event:/char/pusheen/oshiro/chat_collapse";
                public const string ChatGetUp          = "event:/char/pusheen/oshiro/chat_get_up";
                public const string ChatTurnLeft       = "event:/char/pusheen/oshiro/chat_turn_left";
                public const string ChatTurnRight      = "event:/char/pusheen/oshiro/chat_turn_right";
                public const string Move01_0xa_exit    = "event:/char/pusheen/oshiro/move_01_0xa_exit";
                public const string Move02_03a_exit    = "event:/char/pusheen/oshiro/move_02_03a_exit";
                public const string Move03_08a_exit    = "event:/char/pusheen/oshiro/move_03_08a_exit";
                public const string Move04_PaceLeft    = "event:/char/pusheen/oshiro/move_04_pace_left";
                public const string Move04_PaceRight   = "event:/char/pusheen/oshiro/move_04_pace_right";
                public const string Move05_09b_exit    = "event:/char/pusheen/oshiro/move_05_09b_exit";
                public const string Move06_04d_exit    = "event:/char/pusheen/oshiro/move_06_04d_exit";
                public const string Move07_Roof00Enter = "event:/char/pusheen/oshiro/move_07_roof00_enter";
                public const string Move08_Roof07Exit  = "event:/char/pusheen/oshiro/move_08_roof07_exit";
            }

            // event:/char/pusheen/theo/*
            public static class Theo
            {
                public const string PhoneTapsLoop         = "event:/char/pusheen/theo/phone_taps_loop";
                public const string ResortCeilingventHey  = "event:/char/pusheen/theo/resort_ceilingvent_hey";
                public const string ResortCeilingventPopOff = "event:/char/pusheen/theo/resort_ceilingvent_popoff";
                public const string ResortCeilingventSeeya = "event:/char/pusheen/theo/resort_ceilingvent_seeya";
                public const string ResortCeilingventShake = "event:/char/pusheen/theo/resort_ceilingvent_shake";
                public const string ResortCrawl           = "event:/char/pusheen/theo/resort_crawl";
                public const string ResortStandToCrawl    = "event:/char/pusheen/theo/resort_standtocrawl";
                public const string ResortVentGrab        = "event:/char/pusheen/theo/resort_vent_grab";
                public const string ResortVentRip         = "event:/char/pusheen/theo/resort_vent_rip";
                public const string ResortVentTug         = "event:/char/pusheen/theo/resort_vent_tug";
                public const string ResortVentTumble      = "event:/char/pusheen/theo/resort_vent_tumble";
                public const string YoloFist              = "event:/char/pusheen/theo/yolo_fist";
            }

            // event:/char/dialogue/pusheen/*
            public static class Dialogue
            {
                public const string ReadMe       = "event:/char/dialogue/pusheen/!!!_README";
                public const string Alphy        = "event:/char/dialogue/pusheen/alphy";
                public const string Asgore       = "event:/char/dialogue/pusheen/asgore";
                public const string Asriel       = "event:/char/dialogue/pusheen/asriel";
                public const string AsrielGod    = "event:/char/dialogue/pusheen/asriel_god";
                public const string Chara        = "event:/char/dialogue/pusheen/chara";
                public const string Defender     = "event:/char/dialogue/pusheen/defender";
                public const string Els          = "event:/char/dialogue/pusheen/els";
                public const string Flowey       = "event:/char/dialogue/pusheen/flowey";
                public const string King         = "event:/char/dialogue/pusheen/king";
                public const string Kirby        = "event:/char/dialogue/pusheen/kirby";
                public const string KirbyMirror  = "event:/char/dialogue/pusheen/kirby_mirror";
                public const string KirbyWebcam  = "event:/char/dialogue/pusheen/kirby_webcam";
                public const string Magolor      = "event:/char/dialogue/pusheen/magolor";
                public const string Papyrus      = "event:/char/dialogue/pusheen/papyrus";
                public const string Priest       = "event:/char/dialogue/pusheen/preist";
                public const string Queen        = "event:/char/dialogue/pusheen/queen";
                public const string Sans         = "event:/char/dialogue/pusheen/sans";
                public const string Spamton      = "event:/char/dialogue/pusheen/spamton";
                public const string Suzy         = "event:/char/dialogue/pusheen/suzy";
                public const string Tenna        = "event:/char/dialogue/pusheen/tenna";
                public const string Toriel       = "event:/char/dialogue/pusheen/toriel";
                public const string Undyne       = "event:/char/dialogue/pusheen/undyne";
                public const string UndyneHyper  = "event:/char/dialogue/pusheen/undyne_hyper";
                public const string WaddleDee    = "event:/char/dialogue/pusheen/waddledee";
                public const string PhoneStaticAsriel = "event:/char/dialogue/pusheen/sfx_support/phone_static_asriel";
                public const string PhoneStaticGaster = "event:/char/dialogue/pusheen/sfx_support/phone_static_gaster";
            }
        }

        // ─── Game (in-level SFX) ──────────────────────────────────────────────

        public static class Game
        {
            // event:/game/pusheen/general/*
            public static class General
            {
                public const string AssistDashAim          = "event:/game/pusheen/general/assist_dash_aim";
                public const string AssistDreamblockBounce = "event:/game/pusheen/general/assist_dreamblockbounce";
                public const string AssistNonsolidIn       = "event:/game/pusheen/general/assist_nonsolid_in";
                public const string AssistNonsolidOut      = "event:/game/pusheen/general/assist_nonsolid_out";
                public const string AssistScreenbottom     = "event:/game/pusheen/general/assist_screenbottom";
                public const string BirdIn                 = "event:/game/pusheen/general/bird_in";
                public const string BirdLandDirt           = "event:/game/pusheen/general/bird_land_dirt";
                public const string BirdPeck               = "event:/game/pusheen/general/bird_peck";
                public const string BirdSquawk             = "event:/game/pusheen/general/bird_squawk";
                public const string BirdStartle            = "event:/game/pusheen/general/bird_startle";
                public const string BirdbabyFlyaway        = "event:/game/pusheen/general/birdbaby_flyaway";
                public const string BirdbabyHop            = "event:/game/pusheen/general/birdbaby_hop";
                public const string BirdbabyTweetLoop      = "event:/game/pusheen/general/birdbaby_tweet_loop";
                public const string CassetteBlockSwitch1   = "event:/game/pusheen/general/cassette_block_switch_1";
                public const string CassetteBlockSwitch2   = "event:/game/pusheen/general/cassette_block_switch_2";
                public const string CassetteBubbleReturn   = "event:/game/pusheen/general/cassette_bubblereturn";
                public const string CrystalheartAstralGet  = "event:/game/pusheen/general/crystalheart_astral_get";
                public const string CrystalheartAstralPreview = "event:/game/pusheen/general/crystalheart_astral_preview";
                public const string CrystalheartBlueGet    = "event:/game/pusheen/general/crystalheart_blue_get";
                public const string CrystalheartBounce     = "event:/game/pusheen/general/crystalheart_bounce";
                public const string CrystalheartGoldGet    = "event:/game/pusheen/general/crystalheart_gold_get";
                public const string CrystalheartPinkGet    = "event:/game/pusheen/general/crystalheart_pink_get";
                public const string CrystalheartPinkPreview = "event:/game/pusheen/general/crystalheart_pink_preview";
                public const string CrystalheartPulse      = "event:/game/pusheen/general/crystalheart_pulse";
                public const string CrystalheartRainbowGet = "event:/game/pusheen/general/crystalheart_rainbow_get";
                public const string CrystalheartRedGet     = "event:/game/pusheen/general/crystalheart_red_get";
                public const string DebrisDirt             = "event:/game/pusheen/general/debris_dirt";
                public const string DebrisStone            = "event:/game/pusheen/general/debris_stone";
                public const string DebrisWood             = "event:/game/pusheen/general/debris_wood";
                public const string DiamondReturn          = "event:/game/pusheen/general/diamond_return";
                public const string DiamondTouch           = "event:/game/pusheen/general/diamond_touch";
                public const string FallblockImpact        = "event:/game/pusheen/general/fallblock_impact";
                public const string FallblockShake         = "event:/game/pusheen/general/fallblock_shake";
                public const string KeyGet                 = "event:/game/pusheen/general/key_get";
                public const string LookoutMove            = "event:/game/pusheen/general/lookout_move";
                public const string LookoutUse             = "event:/game/pusheen/general/lookout_use";
                public const string PassageClosedBehind    = "event:/game/pusheen/general/passage_closed_behind";
                public const string PlatformDisintegrate   = "event:/game/pusheen/general/platform_disintegrate";
                public const string PlatformReturn         = "event:/game/pusheen/general/platform_return";
                public const string SecretRevealed         = "event:/game/pusheen/general/secret_revealed";
                public const string SeedCompleteBerry      = "event:/game/pusheen/general/seed_complete_berry";
                public const string SeedCompleteMain       = "event:/game/pusheen/general/seed_complete_main";
                public const string SeedPoof               = "event:/game/pusheen/general/seed_poof";
                public const string SeedPulse              = "event:/game/pusheen/general/seed_pulse";
                public const string SeedReappear           = "event:/game/pusheen/general/seed_reappear";
                public const string SeedTouch              = "event:/game/pusheen/general/seed_touch";
                public const string SpotlightIntro         = "event:/game/pusheen/general/spotlight_intro";
                public const string SpotlightOutro         = "event:/game/pusheen/general/spotlight_outro";
                public const string Spring                 = "event:/game/pusheen/general/spring";
                public const string StrawberryBluePulse    = "event:/game/pusheen/general/strawberry_blue_pulse";
                public const string StrawberryBlueTouch    = "event:/game/pusheen/general/strawberry_blue_touch";
                public const string StrawberryFlyaway      = "event:/game/pusheen/general/strawberry_flyaway";
                public const string StrawberryGet          = "event:/game/pusheen/general/strawberry_get";
                public const string StrawberryLaugh        = "event:/game/pusheen/general/strawberry_laugh";
                public const string StrawberryPulse        = "event:/game/pusheen/general/strawberry_pulse";
                public const string StrawberryTouch        = "event:/game/pusheen/general/strawberry_touch";
                public const string StrawberryWingflap     = "event:/game/pusheen/general/strawberry_wingflap";
                public const string TapeGet                = "event:/game/pusheen/general/tape_get";
                public const string TapePreview            = "event:/game/pusheen/general/tape_preview";
                public const string ThingBooped           = "event:/game/pusheen/general/thing_booped";
                public const string TouchswitchAny        = "event:/game/pusheen/general/touchswitch_any";
                public const string TouchswitchGateFinish = "event:/game/pusheen/general/touchswitch_gate_finish";
                public const string TouchswitchGateOpen   = "event:/game/pusheen/general/touchswitch_gate_open";
                public const string TouchswitchLast       = "event:/game/pusheen/general/touchswitch_last";
                public const string TouchswitchLastCutoff = "event:/game/pusheen/general/touchswitch_last_cutoff";
                public const string TouchswitchLastOneshot = "event:/game/pusheen/general/touchswitch_last_oneshot";
                public const string WallBreakDirt         = "event:/game/pusheen/general/wall_break_dirt";
                public const string WallBreakIce          = "event:/game/pusheen/general/wall_break_ice";
                public const string WallBreakStone        = "event:/game/pusheen/general/wall_break_stone";
                public const string WallBreakWood         = "event:/game/pusheen/general/wall_break_wood";
            }

            // event:/game/pusheen/00_prologue/*
            public static class Ch00_Prologue
            {
                public const string BridgeRumbleLoop      = "event:/game/pusheen/00_prologue/bridge_rumble_loop";
                public const string BridgeSupportBreak    = "event:/game/pusheen/00_prologue/bridge_support_break";
                public const string CarDown               = "event:/game/pusheen/00_prologue/car_down";
                public const string CarUp                 = "event:/game/pusheen/00_prologue/car_up";
                public const string FallblockFirstImpact  = "event:/game/pusheen/00_prologue/fallblock_first_impact";
                public const string FallblockFirstShake   = "event:/game/pusheen/00_prologue/fallblock_first_shake";
                public const string IntroVignette         = "event:/game/pusheen/00_prologue/intro_vignette";
            }

            // event:/game/pusheen/01_metro/*
            public static class Ch01_Metro
            {
                public const string BirdsisFinish      = "event:/game/pusheen/01_metro/birdsis_finish";
                public const string BirdsisThrust      = "event:/game/pusheen/01_metro/birdsis_thrust";
                public const string ConsoleBlue        = "event:/game/pusheen/01_metro/console_blue";
                public const string ConsolePurple      = "event:/game/pusheen/01_metro/console_purple";
                public const string ConsoleRed         = "event:/game/pusheen/01_metro/console_red";
                public const string ConsoleStaticLong  = "event:/game/pusheen/01_metro/console_static_long";
                public const string ConsoleStaticLoop  = "event:/game/pusheen/01_metro/console_static_loop";
                public const string ConsoleStaticShort = "event:/game/pusheen/01_metro/console_static_short";
                public const string ConsoleWhite       = "event:/game/pusheen/01_metro/console_white";
                public const string ConsoleYellow      = "event:/game/pusheen/01_metro/console_yellow";
            }

            // event:/game/pusheen/02_shadow/*
            public static class Ch02_Shadow
            {
                public const string SequenceCharaIntro      = "event:/game/pusheen/02_shadow/sequence_chara_intro";
                public const string SequenceMirror          = "event:/game/pusheen/02_shadow/sequence_mirror";
                public const string SequencePhoneVortex     = "event:/game/pusheen/02_shadow/sequence_phone_vortex";
                public const string SequencePhoneVortexClosed = "event:/game/pusheen/02_shadow/sequence_phone_vortex_closed";
            }

            // event:/game/pusheen/03_arrival/*
            public static class Ch03_Arrival
            {
                public const string BirdkidFinish      = "event:/game/pusheen/03_arrival/birdkid_finish";
                public const string BirdkidThrust      = "event:/game/pusheen/03_arrival/birdkid_thrust";
                public const string ConsoleBlue        = "event:/game/pusheen/03_arrival/console_blue";
                public const string ConsolePurple      = "event:/game/pusheen/03_arrival/console_purple";
                public const string ConsoleRed         = "event:/game/pusheen/03_arrival/console_red";
                public const string ConsoleStaticLong  = "event:/game/pusheen/03_arrival/console_static_long";
                public const string ConsoleStaticLoop  = "event:/game/pusheen/03_arrival/console_static_loop";
                public const string ConsoleStaticShort = "event:/game/pusheen/03_arrival/console_static_short";
                public const string ConsoleWhite       = "event:/game/pusheen/03_arrival/console_white";
                public const string ConsoleYellow      = "event:/game/pusheen/03_arrival/console_yellow";
                public const string IntroVignette      = "event:/game/pusheen/03_arrival/intro_vignette";
            }

            // event:/game/pusheen/04_legend/*
            public static class Ch04_Legend
            {
                public const string LanternHit             = "event:/game/pusheen/04_legend/lantern_hit";
                public const string SequenceCharaIntro     = "event:/game/pusheen/04_legend/sequence_chara_intro";
                public const string SequenceMirror         = "event:/game/pusheen/04_legend/sequence_mirror";
                public const string SequencePhonePickup    = "event:/game/pusheen/04_legend/sequence_phone_pickup";
                public const string SequencePhoneRingLoop  = "event:/game/pusheen/04_legend/sequence_phone_ring_loop";
                public const string SequencePhoneRingtoneLoop = "event:/game/pusheen/04_legend/sequence_phone_ringtone_loop";
                public const string SequencePhoneTransform = "event:/game/pusheen/04_legend/sequence_phone_transform";
                public const string TheoSelfieFoley        = "event:/game/pusheen/04_legend/theoselfie_foley";
                public const string TheoSelfiePhotoFilter  = "event:/game/pusheen/04_legend/theoselfie_photo_filter";
                public const string TheoSelfiePhotoIn      = "event:/game/pusheen/04_legend/theoselfie_photo_in";
                public const string TheoSelfiePhotoOut     = "event:/game/pusheen/04_legend/theoselfie_photo_out";
            }

            // event:/game/pusheen/05_fractured/*
            public static class Ch05_Fractured
            {
                public const string ClutterswitchBooks     = "event:/game/pusheen/05_fractured/clutterswitch_books";
                public const string ClutterswitchBoxes     = "event:/game/pusheen/05_fractured/clutterswitch_boxes";
                public const string ClutterswitchFinish    = "event:/game/pusheen/05_fractured/clutterswitch_finish";
                public const string ClutterswitchLinens    = "event:/game/pusheen/05_fractured/clutterswitch_linens";
                public const string ClutterswitchReturn    = "event:/game/pusheen/05_fractured/clutterswitch_return";
                public const string ClutterswitchSquish    = "event:/game/pusheen/05_fractured/clutterswitch_squish";
                public const string DeskbellAgain          = "event:/game/pusheen/05_fractured/deskbell_again";
                public const string DoorMetalClose         = "event:/game/pusheen/05_fractured/door_metal_close";
                public const string DoorMetalOpen          = "event:/game/pusheen/05_fractured/door_metal_open";
                public const string DoorWoodClose          = "event:/game/pusheen/05_fractured/door_wood_close";
                public const string DoorWoodOpen           = "event:/game/pusheen/05_fractured/door_wood_open";
                public const string FallblockWoodImpact    = "event:/game/pusheen/05_fractured/fallblock_wood_impact";
                public const string FallblockWoodShake     = "event:/game/pusheen/05_fractured/fallblock_wood_shake";
                public const string FallblockWoodDistantImpact = "event:/game/pusheen/05_fractured/fallblock_wooddistant_impact";
                public const string FluffTendrilEmerge     = "event:/game/pusheen/05_fractured/fluff_tendril_emerge";
                public const string FluffTendrilRecede     = "event:/game/pusheen/05_fractured/fluff_tendril_recede";
                public const string FluffTendrilTouch      = "event:/game/pusheen/05_fractured/fluff_tendril_touch";
                public const string ForcefieldBump         = "event:/game/pusheen/05_fractured/forcefield_bump";
                public const string ForcefieldIdleLoop     = "event:/game/pusheen/05_fractured/forcefield_idle_loop";
                public const string ForcefieldVanish       = "event:/game/pusheen/05_fractured/forcefield_vanish";
                public const string KeyUnlock              = "event:/game/pusheen/05_fractured/key_unlock";
                public const string LanternBump            = "event:/game/pusheen/05_fractured/lantern_bump";
                public const string MemoIn                 = "event:/game/pusheen/05_fractured/memo_in";
                public const string MemoOut                = "event:/game/pusheen/05_fractured/memo_out";
                public const string PlatformHorizLeft      = "event:/game/pusheen/05_fractured/platform_horiz_left";
                public const string PlatformHorizRight     = "event:/game/pusheen/05_fractured/platform_horiz_right";
                public const string PlatformVertDownLoop   = "event:/game/pusheen/05_fractured/platform_vert_down_loop";
                public const string PlatformVertEnd        = "event:/game/pusheen/05_fractured/platform_vert_end";
                public const string PlatformVertStart      = "event:/game/pusheen/05_fractured/platform_vert_start";
                public const string PlatformVertUpLoop     = "event:/game/pusheen/05_fractured/platform_vert_up_loop";
                public const string SequenceOshiroIntro    = "event:/game/pusheen/05_fractured/sequence_oshiro_intro";
                public const string SequenceOshiroFluffPt1 = "event:/game/pusheen/05_fractured/sequence_oshirofluff_pt1";
                public const string SequenceOshiroFluffPt2 = "event:/game/pusheen/05_fractured/sequence_oshirofluff_pt2";
                public const string SuiteCharaCeilingBreak = "event:/game/pusheen/05_fractured/suite_chara_ceilingbreak";
                public const string SuiteCharaExitTop      = "event:/game/pusheen/05_fractured/suite_chara_exittop";
                public const string SuiteCharaIntro        = "event:/game/pusheen/05_fractured/suite_chara_intro";
                public const string SuiteCharaMirrorBreak  = "event:/game/pusheen/05_fractured/suite_chara_mirrorbreak";
                public const string SuiteCharaMoveRoof     = "event:/game/pusheen/05_fractured/suite_chara_moveroof";
                public const string SuiteCharaMoveStageLeft = "event:/game/pusheen/05_fractured/suite_chara_movestageleft";
                public const string TrapdoorFromBottom     = "event:/game/pusheen/05_fractured/trapdoor_frombottom";
                public const string TrapdoorFromTop        = "event:/game/pusheen/05_fractured/trapdoor_fromtop";
            }

            // event:/game/pusheen/06_stronghold/*
            public static class Ch06_Stronghold
            {
                public const string ArrowblockActivate    = "event:/game/pusheen/06_stronghold/arrowblock_activate";
                public const string ArrowblockBreak       = "event:/game/pusheen/06_stronghold/arrowblock_break";
                public const string ArrowblockDebris      = "event:/game/pusheen/06_stronghold/arrowblock_debris";
                public const string ArrowblockMove        = "event:/game/pusheen/06_stronghold/arrowblock_move";
                public const string ArrowblockMoveChild   = "event:/game/pusheen/06_stronghold/arrowblock_move_child";
                public const string ArrowblockReappear    = "event:/game/pusheen/06_stronghold/arrowblock_reappear";
                public const string ArrowblockReformBegin = "event:/game/pusheen/06_stronghold/arrowblock_reform_begin";
                public const string ArrowblockSideDepress = "event:/game/pusheen/06_stronghold/arrowblock_side_depress";
                public const string ArrowblockSideRelease = "event:/game/pusheen/06_stronghold/arrowblock_side_release";
                public const string CloudBlueBoost        = "event:/game/pusheen/06_stronghold/cloud_blue_boost";
                public const string CloudPinkBoost        = "event:/game/pusheen/06_stronghold/cloud_pink_boost";
                public const string CloudPinkReappear     = "event:/game/pusheen/06_stronghold/cloud_pink_reappear";
                public const string GondolaCliffStart     = "event:/game/pusheen/06_stronghold/gondola_cliffmechanism_start";
                public const string GondolaFinish         = "event:/game/pusheen/06_stronghold/gondola_finish";
                public const string GondolaHaltedLoop     = "event:/game/pusheen/06_stronghold/gondola_halted_loop";
                public const string GondolaMovementLoop   = "event:/game/pusheen/06_stronghold/gondola_movement_loop";
                public const string GondolaRestart        = "event:/game/pusheen/06_stronghold/gondola_restart";
                public const string GondolaScaryHands01   = "event:/game/pusheen/06_stronghold/gondola_scaryhands_01";
                public const string GondolaScaryHands02   = "event:/game/pusheen/06_stronghold/gondola_scaryhands_02";
                public const string GondolaScaryHands03   = "event:/game/pusheen/06_stronghold/gondola_scaryhands_03";
                public const string GondolaTheoFall       = "event:/game/pusheen/06_stronghold/gondola_theo_fall";
                public const string GondolaTheoLeverFail  = "event:/game/pusheen/06_stronghold/gondola_theo_lever_fail";
                public const string GondolaTheoLeverStart = "event:/game/pusheen/06_stronghold/gondola_theo_lever_start";
                public const string GondolaTheoRecover    = "event:/game/pusheen/06_stronghold/gondola_theo_recover";
                public const string GondolaTheoSelfieHalt = "event:/game/pusheen/06_stronghold/gondola_theoselfie_halt";
                public const string GreenBoosterDash      = "event:/game/pusheen/06_stronghold/greenbooster_dash";
                public const string GreenBoosterEnd       = "event:/game/pusheen/06_stronghold/greenbooster_end";
                public const string GreenBoosterEnter     = "event:/game/pusheen/06_stronghold/greenbooster_enter";
                public const string GreenBoosterReappear  = "event:/game/pusheen/06_stronghold/greenbooster_reappear";
                public const string SnowballImpact        = "event:/game/pusheen/06_stronghold/snowball_impact";
                public const string SnowballSpawn         = "event:/game/pusheen/06_stronghold/snowball_spawn";
                public const string StoneBlockade         = "event:/game/pusheen/06_stronghold/stone_blockade";
                public const string WhiteblockFallThru    = "event:/game/pusheen/06_stronghold/whiteblock_fallthru";
            }

            // event:/game/pusheen/07_inferno/*
            public static class Ch07_Inferno
            {
                public const string BladespinnerSpin         = "event:/game/pusheen/07_inferno/bladespinner_spin";
                public const string ButtonActivate           = "event:/game/pusheen/07_inferno/button_activate";
                public const string ButtonDepress            = "event:/game/pusheen/07_inferno/button_depress";
                public const string ButtonReturn             = "event:/game/pusheen/07_inferno/button_return";
                public const string CrackedwallVanish        = "event:/game/pusheen/07_inferno/crackedwall_vanish";
                public const string CrystalMaddyBreakFree    = "event:/game/pusheen/07_inferno/crystalmaddy_break_free";
                public const string CrystalMaddyHitGround    = "event:/game/pusheen/07_inferno/crystalmaddy_hit_ground";
                public const string CrystalMaddyHitSide      = "event:/game/pusheen/07_inferno/crystalmaddy_hit_side";
                public const string EyePulse                 = "event:/game/pusheen/07_inferno/eye_pulse";
                public const string EyebroEyemove            = "event:/game/pusheen/07_inferno/eyebro_eyemove";
                public const string EyewallBounce            = "event:/game/pusheen/07_inferno/eyewall_bounce";
                public const string EyewallDestroy           = "event:/game/pusheen/07_inferno/eyewall_destroy";
                public const string GateMaddyClose           = "event:/game/pusheen/07_inferno/gate_maddy_close";
                public const string GateMaddyOpen            = "event:/game/pusheen/07_inferno/gate_maddy_open";
                public const string GateMainClose            = "event:/game/pusheen/07_inferno/gate_main_close";
                public const string GateMainOpen             = "event:/game/pusheen/07_inferno/gate_main_open";
                public const string KeyUnlockDark            = "event:/game/pusheen/07_inferno/key_unlock_dark";
                public const string KeyUnlockLight           = "event:/game/pusheen/07_inferno/key_unlock_light";
                public const string MainmirrorReveal         = "event:/game/pusheen/07_inferno/mainmirror_reveal";
                public const string MainmirrorTorchLit1      = "event:/game/pusheen/07_inferno/mainmirror_torch_lit_1";
                public const string MainmirrorTorchLit2      = "event:/game/pusheen/07_inferno/mainmirror_torch_lit_2";
                public const string MainmirrorTorchLoop      = "event:/game/pusheen/07_inferno/mainmirror_torch_loop";
                public const string RedBoosterDash           = "event:/game/pusheen/07_inferno/redbooster_dash";
                public const string RedBoosterEnd            = "event:/game/pusheen/07_inferno/redbooster_end";
                public const string RedBoosterEnter          = "event:/game/pusheen/07_inferno/redbooster_enter";
                public const string RedBoosterMove           = "event:/game/pusheen/07_inferno/redbooster_move";
                public const string RedBoosterReappear       = "event:/game/pusheen/07_inferno/redbooster_reappear";
                public const string RoomLightlevelDown       = "event:/game/pusheen/07_inferno/room_lightlevel_down";
                public const string RoomLightlevelUp         = "event:/game/pusheen/07_inferno/room_lightlevel_up";
                public const string SeekerAggro             = "event:/game/pusheen/07_inferno/seeker_aggro";
                public const string SeekerBooped            = "event:/game/pusheen/07_inferno/seeker_booped";
                public const string SeekerDash              = "event:/game/pusheen/07_inferno/seeker_dash";
                public const string SeekerDashTurn          = "event:/game/pusheen/07_inferno/seeker_dash_turn";
                public const string SeekerDeath             = "event:/game/pusheen/07_inferno/seeker_death";
                public const string SeekerHitLightwall      = "event:/game/pusheen/07_inferno/seeker_hit_lightwall";
                public const string SeekerHitNormal         = "event:/game/pusheen/07_inferno/seeker_hit_normal";
                public const string SeekerPlayerControlStart = "event:/game/pusheen/07_inferno/seeker_playercontrolstart";
                public const string SeekerRevive            = "event:/game/pusheen/07_inferno/seeker_revive";
                public const string SeekerStatueBreak       = "event:/game/pusheen/07_inferno/seeker_statue_break";
                public const string SwapblockMove           = "event:/game/pusheen/07_inferno/swapblock_move";
                public const string SwapblockMoveEnd        = "event:/game/pusheen/07_inferno/swapblock_move_end";
                public const string SwapblockReturn         = "event:/game/pusheen/07_inferno/swapblock_return";
                public const string SwapblockReturnEnd      = "event:/game/pusheen/07_inferno/swapblock_return_end";
                public const string TorchActivate           = "event:/game/pusheen/07_inferno/torch_activate";
            }

            // event:/game/pusheen/08_edge/*
            public static class Ch08_Edge
            {
                public const string BossSpikesBurst        = "event:/game/pusheen/08_edge/boss_spikes_burst";
                public const string CharaFeatherSlice      = "event:/game/pusheen/08_edge/chara_feather_slice";
                public const string CharaFreakout1         = "event:/game/pusheen/08_edge/chara_freakout_1";
                public const string CharaFreakout2         = "event:/game/pusheen/08_edge/chara_freakout_2";
                public const string CharaFreakout3         = "event:/game/pusheen/08_edge/chara_freakout_3";
                public const string CharaFreakout4         = "event:/game/pusheen/08_edge/chara_freakout_4";
                public const string CharaFreakout5         = "event:/game/pusheen/08_edge/chara_freakout_5";
                public const string CharaFreakout6         = "event:/game/pusheen/08_edge/chara_freakout_6";
                public const string CharaPullCliffbreak    = "event:/game/pusheen/08_edge/chara_pull_cliffbreak";
                public const string CharaPullImpact        = "event:/game/pusheen/08_edge/chara_pull_impact";
                public const string CharaPullRumbleLoop    = "event:/game/pusheen/08_edge/chara_pull_rumble_loop";
                public const string CharaPullWhooshDown    = "event:/game/pusheen/08_edge/chara_pull_whooshdown";
                public const string CrushblockActivate     = "event:/game/pusheen/08_edge/crushblock_activate";
                public const string CrushblockImpact       = "event:/game/pusheen/08_edge/crushblock_impact";
                public const string CrushblockMoveLoop     = "event:/game/pusheen/08_edge/crushblock_move_loop";
                public const string CrushblockMoveLoopCovert = "event:/game/pusheen/08_edge/crushblock_move_loop_covert";
                public const string CrushblockRest         = "event:/game/pusheen/08_edge/crushblock_rest";
                public const string CrushblockRestWaypoint = "event:/game/pusheen/08_edge/crushblock_rest_waypoint";
                public const string CrushblockReturnLoop   = "event:/game/pusheen/08_edge/crushblock_return_loop";
                public const string FallSpikeSmash         = "event:/game/pusheen/08_edge/fall_spike_smash";
                public const string FallblockBossImpact    = "event:/game/pusheen/08_edge/fallblock_boss_impact";
                public const string FallblockBossShake     = "event:/game/pusheen/08_edge/fallblock_boss_shake";
                public const string FeatherBubbleBounce    = "event:/game/pusheen/08_edge/feather_bubble_bounce";
                public const string FeatherBubbleGet       = "event:/game/pusheen/08_edge/feather_bubble_get";
                public const string FeatherBubbleRenew     = "event:/game/pusheen/08_edge/feather_bubble_renew";
                public const string FeatherGet             = "event:/game/pusheen/08_edge/feather_get";
                public const string FeatherReappear        = "event:/game/pusheen/08_edge/feather_reappear";
                public const string FeatherRenew           = "event:/game/pusheen/08_edge/feather_renew";
                public const string FeatherStateBump       = "event:/game/pusheen/08_edge/feather_state_bump";
                public const string FeatherStateEnd        = "event:/game/pusheen/08_edge/feather_state_end";
                public const string FeatherStateLoop       = "event:/game/pusheen/08_edge/feather_state_loop";
                public const string FeatherStateWarning    = "event:/game/pusheen/08_edge/feather_state_warning";
                public const string HugCharaGlow           = "event:/game/pusheen/08_edge/hug_chara_glow";
                public const string HugImage1              = "event:/game/pusheen/08_edge/hug_image_1";
                public const string HugImage2              = "event:/game/pusheen/08_edge/hug_image_2";
                public const string HugImage3              = "event:/game/pusheen/08_edge/hug_image_3";
                public const string HugLevelupTextIn       = "event:/game/pusheen/08_edge/hug_levelup_text_in";
                public const string HugLevelupTextOut      = "event:/game/pusheen/08_edge/hug_levelup_text_out";
            }

            // event:/game/pusheen/09_beyondsummit/*
            public static class Ch09_BeyondSummit
            {
                public const string AltitudeCount     = "event:/game/pusheen/09_beyondsummit/altitude_count";
                public const string CheckpointConfetti = "event:/game/pusheen/09_beyondsummit/checkpoint_confetti";
                public const string GemGet            = "event:/game/pusheen/09_beyondsummit/gem_get";
                public const string GemUnlock1        = "event:/game/pusheen/09_beyondsummit/gem_unlock_1";
                public const string GemUnlock2        = "event:/game/pusheen/09_beyondsummit/gem_unlock_2";
                public const string GemUnlock3        = "event:/game/pusheen/09_beyondsummit/gem_unlock_3";
                public const string GemUnlock4        = "event:/game/pusheen/09_beyondsummit/gem_unlock_4";
                public const string GemUnlock5        = "event:/game/pusheen/09_beyondsummit/gem_unlock_5";
                public const string GemUnlock6        = "event:/game/pusheen/09_beyondsummit/gem_unlock_6";
                public const string GemUnlockComplete = "event:/game/pusheen/09_beyondsummit/gem_unlock_complete";
            }

            // event:/game/pusheen/13_hotcliff/*
            public static class Ch13_HotCliff
            {
                public const string Pew = "event:/game/pusheen/13_hotcliff/pew";
            }

            // event:/game/pusheen/14_cybernexus/*
            public static class Ch14_CyberNexus
            {
                public const string Crunch              = "event:/game/pusheen/14_cybernexus/crunch";
                public const string PortalOpenExit      = "event:/game/pusheen/14_cybernexus/game_14_portal_openexit_leaving_hotcliff";
            }

            // event:/game/pusheen/15_citadel/*
            public static class Ch15_Citadel
            {
                public const string CinematicTrident = "event:/game/pusheen/15_citadel/cinematic_trident";
                public const string FireFlame        = "event:/game/pusheen/15_citadel/fire_flame";
                public const string LeavingCyber     = "event:/game/pusheen/15_citadel/leaving_cyber";
            }

            // event:/game/pusheen/16_myworld/*
            public static class Ch16_MyWorld
            {
                public const string DestroyedA   = "event:/game/pusheen/16_myworld/destroyed_a";
                public const string DestroyedB   = "event:/game/pusheen/16_myworld/destroyed_b";
                public const string DestroyedC   = "event:/game/pusheen/16_myworld/destroyed_c";
                public const string GameoverStuck = "event:/game/pusheen/16_myworld/gameover_stuck";
                public const string LastHit       = "event:/game/pusheen/16_myworld/last_hit";
                public const string StoryStuck    = "event:/game/pusheen/16_myworld/story_stuck";
                public const string Woosh         = "event:/game/pusheen/16_myworld/woosh";
            }

            // event:/game/pusheen/18_core/*
            public static class Ch18_Core
            {
                public const string BounceblockBreak    = "event:/game/pusheen/18_core/bounceblock_break";
                public const string BounceblockReappear = "event:/game/pusheen/18_core/bounceblock_reappear";
                public const string BounceblockTouch    = "event:/game/pusheen/18_core/bounceblock_touch";
                public const string ConveyorActivate    = "event:/game/pusheen/18_core/conveyor_activate";
                public const string FinalHeartGet       = "event:/game/pusheen/18_core/final_heart_get";
                public const string FrontdoorHeartfill  = "event:/game/pusheen/18_core/frontdoor_heartfill";
                public const string FrontdoorUnlock     = "event:/game/pusheen/18_core/frontdoor_unlock";
                public const string HotPinballActivate  = "event:/game/pusheen/18_core/hotpinball_activate";
                public const string IceballBreak        = "event:/game/pusheen/18_core/iceball_break";
                public const string IceblockReappear    = "event:/game/pusheen/18_core/iceblock_reappear";
                public const string IceblockTouch       = "event:/game/pusheen/18_core/iceblock_touch";
                public const string PinballBumperHit    = "event:/game/pusheen/18_core/pinballbumper_hit";
                public const string RisingThreat        = "event:/game/pusheen/18_core/rising_threat";
                public const string SidewayThreats      = "event:/game/pusheen/18_core/sideway_threats";
                public const string SwitchToCold        = "event:/game/pusheen/18_core/switch_to_cold";
                public const string SwitchToHot         = "event:/game/pusheen/18_core/switch_to_hot";
            }
        }

        // ─── Environment / Ambience ───────────────────────────────────────────

        public static class Env
        {
            // event:/env/pusheen/amb/*
            public static class Amb
            {
                public const string Ch00_Prologue      = "event:/env/pusheen/amb/00_prologue";
                public const string Ch01_Main          = "event:/env/pusheen/amb/01_main";
                public const string Ch02_Awake         = "event:/env/pusheen/amb/02_awake";
                public const string Ch02_Dream         = "event:/env/pusheen/amb/02_dream";
                public const string Ch03_Exterior      = "event:/env/pusheen/amb/03_exterior";
                public const string Ch03_Interior      = "event:/env/pusheen/amb/03_interior";
                public const string Ch03_Pico8Closeup  = "event:/env/pusheen/amb/03_pico8_closeup";
                public const string Ch04_Main          = "event:/env/pusheen/amb/04_main";
                public const string Ch05_InteriorDark  = "event:/env/pusheen/amb/05_interior_dark";
                public const string Ch05_InteriorMain  = "event:/env/pusheen/amb/05_interior_main";
                public const string Ch05_MirrorSequence = "event:/env/pusheen/amb/05_mirror_sequence";
                public const string Ch06_Lake          = "event:/env/pusheen/amb/06_lake";
                public const string Ch06_Main          = "event:/env/pusheen/amb/06_main";
                public const string Ch09_Main          = "event:/env/pusheen/amb/09_main";
                public const string Worldmap           = "event:/env/pusheen/amb/worldmap";
            }

            // event:/env/pusheen/local/*
            public static class Local
            {
                public const string PhoneLamp              = "event:/env/pusheen/local/02_shadow/phone_lamp";
                public const string BrokenWindowLarge      = "event:/env/pusheen/local/05_fractured/broken_window_large";
                public const string BrokenWindowSmall      = "event:/env/pusheen/local/05_fractured/broken_window_small";
                public const string Pico8Machine           = "event:/env/pusheen/local/05_fractured/pico8_machine";
                public const string BossIdleGround        = "event:/env/pusheen/local/08_edge/boss_idle_ground";
                public const string FlagFlap               = "event:/env/pusheen/local/09_beyond_summit/flag_flap";
                public const string ConveyorIdle           = "event:/env/pusheen/local/18_core/conveyor_idle";
                public const string FireballsIdle          = "event:/env/pusheen/local/18_core/fireballs_idle";
                public const string LavagateIdle           = "event:/env/pusheen/local/18_core/lavagate_idle";
                public const string CampfireLoop           = "event:/env/pusheen/local/campfire_loop";
                public const string CampfireStart          = "event:/env/pusheen/local/campfire_start";
                public const string WaterfallBigIn         = "event:/env/pusheen/local/waterfall_big_in";
                public const string WaterfallBigMain       = "event:/env/pusheen/local/waterfall_big_main";
                public const string WaterfallSmallInDeep   = "event:/env/pusheen/local/waterfall_small_in_deep";
                public const string WaterfallSmallInShallow = "event:/env/pusheen/local/waterfall_small_in_shallow";
                public const string WaterfallSmallMain     = "event:/env/pusheen/local/waterfall_small_main";
            }
        }

        // ─── UI ───────────────────────────────────────────────────────────────

        public static class UI
        {
            // event:/ui/pusheen/game/*
            public static class Game
            {
                public const string ChatoptionsAppear      = "event:/ui/pusheen/game/chatoptions_appear";
                public const string ChatoptionsRollDown    = "event:/ui/pusheen/game/chatoptions_roll_down";
                public const string ChatoptionsRollUp      = "event:/ui/pusheen/game/chatoptions_roll_up";
                public const string ChatoptionsSelect      = "event:/ui/pusheen/game/chatoptions_select";
                public const string GeneralTextLoop        = "event:/ui/pusheen/game/general_text_loop";
                public const string HotspotMainIn          = "event:/ui/pusheen/game/hotspot_main_in";
                public const string HotspotMainOut         = "event:/ui/pusheen/game/hotspot_main_out";
                public const string HotspotNoteIn          = "event:/ui/pusheen/game/hotspot_note_in";
                public const string HotspotNoteOut         = "event:/ui/pusheen/game/hotspot_note_out";
                public const string IncrementDashcount     = "event:/ui/pusheen/game/increment_dashcount";
                public const string IncrementStrawberry    = "event:/ui/pusheen/game/increment_strawberry";
                public const string LookoutOff             = "event:/ui/pusheen/game/lookout_off";
                public const string LookoutOn              = "event:/ui/pusheen/game/lookout_on";
                public const string MemorialDreamLoop      = "event:/ui/pusheen/game/memorial_dream_loop";
                public const string MemorialDreamTextIn    = "event:/ui/pusheen/game/memorial_dream_text_in";
                public const string MemorialDreamTextLoop  = "event:/ui/pusheen/game/memorial_dream_text_loop";
                public const string MemorialDreamTextOut   = "event:/ui/pusheen/game/memorial_dream_text_out";
                public const string MemorialTextIn         = "event:/ui/pusheen/game/memorial_text_in";
                public const string MemorialTextLoop       = "event:/ui/pusheen/game/memorial_text_loop";
                public const string MemorialTextOut        = "event:/ui/pusheen/game/memorial_text_out";
                public const string Pause                  = "event:/ui/pusheen/game/pause";
                public const string TextAdvanceMadeline    = "event:/ui/pusheen/game/textadvance_madeline";
                public const string TextAdvanceOther       = "event:/ui/pusheen/game/textadvance_other";
                public const string TextboxMadelineIn      = "event:/ui/pusheen/game/textbox_madeline_in";
                public const string TextboxMadelineOut     = "event:/ui/pusheen/game/textbox_madeline_out";
                public const string TextboxOtherIn         = "event:/ui/pusheen/game/textbox_other_in";
                public const string TextboxOtherOut        = "event:/ui/pusheen/game/textbox_other_out";
                public const string TutorialNoteFlipBack   = "event:/ui/pusheen/game/tutorial_note_flip_back";
                public const string TutorialNoteFlipFront  = "event:/ui/pusheen/game/tutorial_note_flip_front";
                public const string Unpause                = "event:/ui/pusheen/game/unpause";
            }

            // event:/ui/pusheen/main/*
            public static class Main
            {
                public const string AssistButtonInfo       = "event:/ui/pusheen/main/assist_button_info";
                public const string AssistButtonNo         = "event:/ui/pusheen/main/assist_button_no";
                public const string AssistButtonYes        = "event:/ui/pusheen/main/assist_button_yes";
                public const string AssistInfoWhistle      = "event:/ui/pusheen/main/assist_info_whistle";
                public const string BsideIntroText         = "event:/ui/pusheen/main/bside_intro_text";
                public const string ButtonBack             = "event:/ui/pusheen/main/button_back";
                public const string ButtonClimb            = "event:/ui/pusheen/main/button_climb";
                public const string ButtonInvalid          = "event:/ui/pusheen/main/button_invalid";
                public const string ButtonLowkey           = "event:/ui/pusheen/main/button_lowkey";
                public const string ButtonSelect           = "event:/ui/pusheen/main/button_select";
                public const string ButtonToggleOff        = "event:/ui/pusheen/main/button_toggle_off";
                public const string ButtonToggleOn         = "event:/ui/pusheen/main/button_toggle_on";
                public const string MessageConfirm         = "event:/ui/pusheen/main/message_confirm";
                public const string RenameEntryAccept      = "event:/ui/pusheen/main/rename_entry_accept";
                public const string RenameEntryBackspace   = "event:/ui/pusheen/main/rename_entry_backspace";
                public const string RenameEntryChar        = "event:/ui/pusheen/main/rename_entry_char";
                public const string RenameEntryRollover    = "event:/ui/pusheen/main/rename_entry_rollover";
                public const string RenameEntrySpace       = "event:/ui/pusheen/main/rename_entry_space";
                public const string RolloverDown           = "event:/ui/pusheen/main/rollover_down";
                public const string RolloverUp             = "event:/ui/pusheen/main/rollover_up";
                public const string SavefileBegin          = "event:/ui/pusheen/main/savefile_begin";
                public const string SavefileDelete         = "event:/ui/pusheen/main/savefile_delete";
                public const string SavefileRenameStart    = "event:/ui/pusheen/main/savefile_rename_start";
                public const string SavefileRolloverDown   = "event:/ui/pusheen/main/savefile_rollover_down";
                public const string SavefileRolloverFirst  = "event:/ui/pusheen/main/savefile_rollover_first";
                public const string SavefileRolloverUp     = "event:/ui/pusheen/main/savefile_rollover_up";
                public const string TitleFirstInput        = "event:/ui/pusheen/main/title_firstinput";
                public const string WhooshLargeIn          = "event:/ui/pusheen/main/whoosh_large_in";
                public const string WhooshLargeOut         = "event:/ui/pusheen/main/whoosh_large_out";
                public const string WhooshListIn           = "event:/ui/pusheen/main/whoosh_list_in";
                public const string WhooshListOut          = "event:/ui/pusheen/main/whoosh_list_out";
                public const string WhooshSavefileIn       = "event:/ui/pusheen/main/whoosh_savefile_in";
                public const string WhooshSavefileOut      = "event:/ui/pusheen/main/whoosh_savefile_out";
                // Postcard events — In/Out per chapter
                public const string PostcardCh1In          = "event:/ui/pusheen/main/postcard_ch1_in";
                public const string PostcardCh1Out         = "event:/ui/pusheen/main/postcard_ch1_out";
                public const string PostcardCh2In          = "event:/ui/pusheen/main/postcard_ch2_in";
                public const string PostcardCh2Out         = "event:/ui/pusheen/main/postcard_ch2_out";
                public const string PostcardCh3In          = "event:/ui/pusheen/main/postcard_ch3_in";
                public const string PostcardCh3Out         = "event:/ui/pusheen/main/postcard_ch3_out";
                public const string PostcardCh4In          = "event:/ui/pusheen/main/postcard_ch4_in";
                public const string PostcardCh4Out         = "event:/ui/pusheen/main/postcard_ch4_out";
                public const string PostcardCh5In          = "event:/ui/pusheen/main/postcard_ch5_in";
                public const string PostcardCh5Out         = "event:/ui/pusheen/main/postcard_ch5_out";
                public const string PostcardCh6In          = "event:/ui/pusheen/main/postcard_ch6_in";
                public const string PostcardCh6Out         = "event:/ui/pusheen/main/postcard_ch6_out";
                public const string PostcardCh7In          = "event:/ui/pusheen/main/postcard_ch7_in";
                public const string PostcardCh7Out         = "event:/ui/pusheen/main/postcard_ch7_out";
                public const string PostcardCh8In          = "event:/ui/pusheen/main/postcard_ch8_in";
                public const string PostcardCh8Out         = "event:/ui/pusheen/main/postcard_ch8_out";
                public const string PostcardCh9In          = "event:/ui/pusheen/main/postcard_ch9_in";
                public const string PostcardCh9Out         = "event:/ui/pusheen/main/postcard_ch9_out";
                public const string PostcardCh10In         = "event:/ui/pusheen/main/postcard_ch10_in";
                public const string PostcardCh10Out        = "event:/ui/pusheen/main/postcard_ch10_out";
                public const string PostcardCh11In         = "event:/ui/pusheen/main/postcard_ch11_in";
                public const string PostcardCh11Out        = "event:/ui/pusheen/main/postcard_ch11_out";
                public const string PostcardCh12In         = "event:/ui/pusheen/main/postcard_ch12_in";
                public const string PostcardCh12Out        = "event:/ui/pusheen/main/postcard_ch12_out";
                public const string PostcardCh13In         = "event:/ui/pusheen/main/postcard_ch13_in";
                public const string PostcardCh13Out        = "event:/ui/pusheen/main/postcard_ch13_out";
                public const string PostcardCh14In         = "event:/ui/pusheen/main/postcard_ch14_in";
                public const string PostcardCh14Out        = "event:/ui/pusheen/main/postcard_ch14_out";
                public const string PostcardCh15In         = "event:/ui/pusheen/main/postcard_ch15_in";
                public const string PostcardCh15Out        = "event:/ui/pusheen/main/postcard_ch15_out";
                public const string PostcardCh16In         = "event:/ui/pusheen/main/postcard_ch16_in";
                public const string PostcardCh16Out        = "event:/ui/pusheen/main/postcard_ch16_out";
                public const string PostcardDsidesIn       = "event:/ui/pusheen/main/postcard_dsides_in";
                public const string PostcardDsidesOut      = "event:/ui/pusheen/main/postcard_dsides_out";
            }

            // event:/ui/pusheen/postgame/*
            public static class PostGame
            {
                public const string CrystalHeart        = "event:/ui/pusheen/postgame/crystal_heart";
                public const string DeathAppear         = "event:/ui/pusheen/postgame/death_appear";
                public const string DeathCount          = "event:/ui/pusheen/postgame/death_count";
                public const string DeathFinal          = "event:/ui/pusheen/postgame/death_final";
                public const string GoldberryCount      = "event:/ui/pusheen/postgame/goldberry_count";
                public const string StrawberryCount     = "event:/ui/pusheen/postgame/strawberry_count";
                public const string StrawberryTotal     = "event:/ui/pusheen/postgame/strawberry_total";
                public const string StrawberryTotalAll  = "event:/ui/pusheen/postgame/strawberry_total_all";
                public const string UnlockBside         = "event:/ui/pusheen/postgame/unlock_bside";
                public const string UnlockNewChapter    = "event:/ui/pusheen/postgame/unlock_newchapter";
                public const string UnlockNewChapterIcon = "event:/ui/pusheen/postgame/unlock_newchapter_icon";
            }

            // event:/ui/pusheen/world_map/*
            public static class WorldMap
            {
                public const string ChapterBack              = "event:/ui/pusheen/world_map/chapter/back";
                public const string ChapterCheckpointBack    = "event:/ui/pusheen/world_map/chapter/checkpoint_back";
                public const string ChapterCheckpointPhotoAdd    = "event:/ui/pusheen/world_map/chapter/checkpoint_photo_add";
                public const string ChapterCheckpointPhotoRemove = "event:/ui/pusheen/world_map/chapter/checkpoint_photo_remove";
                public const string ChapterCheckpointStart   = "event:/ui/pusheen/world_map/chapter/checkpoint_start";
                public const string ChapterLevelSelect       = "event:/ui/pusheen/world_map/chapter/level_select";
                public const string ChapterPaneContract      = "event:/ui/pusheen/world_map/chapter/pane_contract";
                public const string ChapterPaneExpand        = "event:/ui/pusheen/world_map/chapter/pane_expand";
                public const string ChapterTabRollLeft       = "event:/ui/pusheen/world_map/chapter/tab_roll_left";
                public const string ChapterTabRollRight      = "event:/ui/pusheen/world_map/chapter/tab_roll_right";
                public const string IconAssistSkip           = "event:/ui/pusheen/world_map/icon/assist_skip";
                public const string IconFlipLeft             = "event:/ui/pusheen/world_map/icon/flip_left";
                public const string IconFlipRight            = "event:/ui/pusheen/world_map/icon/flip_right";
                public const string IconRollLeft             = "event:/ui/pusheen/world_map/icon/roll_left";
                public const string IconRollRight            = "event:/ui/pusheen/world_map/icon/roll_right";
                public const string IconSelect               = "event:/ui/pusheen/world_map/icon/select";
                public const string JournalBack              = "event:/ui/pusheen/world_map/journal/back";
                public const string JournalHeartGrab         = "event:/ui/pusheen/world_map/journal/heart_grab";
                public const string JournalHeartRelease      = "event:/ui/pusheen/world_map/journal/heart_release";
                public const string JournalHeartRoll         = "event:/ui/pusheen/world_map/journal/heart_roll";
                public const string JournalHeartShiftDown    = "event:/ui/pusheen/world_map/journal/heart_shift_down";
                public const string JournalHeartShiftUp      = "event:/ui/pusheen/world_map/journal/heart_shift_up";
                public const string JournalPageCoverBack     = "event:/ui/pusheen/world_map/journal/page_cover_back";
                public const string JournalPageCoverForward  = "event:/ui/pusheen/world_map/journal/page_cover_forward";
                public const string JournalPageMainBack      = "event:/ui/pusheen/world_map/journal/page_main_back";
                public const string JournalPageMainForward   = "event:/ui/pusheen/world_map/journal/page_main_forward";
                public const string JournalSelect            = "event:/ui/pusheen/world_map/journal/select";
                public const string Whoosh1000msBack         = "event:/ui/pusheen/world_map/whoosh/1000ms_back";
                public const string Whoosh1000msForward      = "event:/ui/pusheen/world_map/whoosh/1000ms_forward";
                public const string Whoosh400msBack          = "event:/ui/pusheen/world_map/whoosh/400ms_back";
                public const string Whoosh400msForward       = "event:/ui/pusheen/world_map/whoosh/400ms_forward";
                public const string Whoosh600msBack          = "event:/ui/pusheen/world_map/whoosh/600ms_back";
                public const string Whoosh600msForward       = "event:/ui/pusheen/world_map/whoosh/600ms_forward";
                public const string Whoosh700msBack          = "event:/ui/pusheen/world_map/whoosh/700ms_back";
                public const string Whoosh700msForward       = "event:/ui/pusheen/world_map/whoosh/700ms_forward";
                public const string Whoosh900msBack          = "event:/ui/pusheen/world_map/whoosh/900ms_back";
                public const string Whoosh900msForward       = "event:/ui/pusheen/world_map/whoosh/900ms_forward";
            }
        }

        // ─── New Content (Chapter 19 / Spaces) ───────────────────────────────

        public static class NewContent
        {
            // event:/new_content/game/pusheen/19_spaces/*
            public static class Ch19_Spaces
            {
                public const string BirdCameraPanUp          = "event:/new_content/game/pusheen/19_spaces/bird_camera_pan_up";
                public const string BirdCrashsceneLeave      = "event:/new_content/game/pusheen/19_spaces/bird_crashscene_leave";
                public const string BirdCrashsceneRecover    = "event:/new_content/game/pusheen/19_spaces/bird_crashscene_recover";
                public const string BirdCrashsceneRelocate   = "event:/new_content/game/pusheen/19_spaces/bird_crashscene_relocate";
                public const string BirdCrashsceneStart      = "event:/new_content/game/pusheen/19_spaces/bird_crashscene_start";
                public const string BirdCrashsceneTwitch1    = "event:/new_content/game/pusheen/19_spaces/bird_crashscene_twitch_1";
                public const string BirdCrashsceneTwitch2    = "event:/new_content/game/pusheen/19_spaces/bird_crashscene_twitch_2";
                public const string BirdCrashsceneTwitch3    = "event:/new_content/game/pusheen/19_spaces/bird_crashscene_twitch_3";
                public const string BirdFlappyscene          = "event:/new_content/game/pusheen/19_spaces/bird_flappyscene";
                public const string BirdFlappysceneEntry     = "event:/new_content/game/pusheen/19_spaces/bird_flappyscene_entry";
                public const string BirdFlyUpToNext          = "event:/new_content/game/pusheen/19_spaces/bird_fly_uptonext";
                public const string BirdFlyUpRoll            = "event:/new_content/game/pusheen/19_spaces/bird_flyuproll";
                public const string BirdRelocate             = "event:/new_content/game/pusheen/19_spaces/bird_relocate";
                public const string BirdStartle              = "event:/new_content/game/pusheen/19_spaces/bird_startle";
                public const string BirdThrow                = "event:/new_content/game/pusheen/19_spaces/bird_throw";
                public const string BirdWingflap             = "event:/new_content/game/pusheen/19_spaces/bird_wingflap";
                public const string CafeComputerOff          = "event:/new_content/game/pusheen/19_spaces/cafe_computer_off";
                public const string CafeComputerOn           = "event:/new_content/game/pusheen/19_spaces/cafe_computer_on";
                public const string CafeComputerOnOld        = "event:/new_content/game/pusheen/19_spaces/cafe_computer_on_old";
                public const string CafeComputerStartupSfx   = "event:/new_content/game/pusheen/19_spaces/cafe_computer_startupsfx";
                public const string EndsceneAttachmentClick  = "event:/new_content/game/pusheen/19_spaces/endscene_attachment_click";
                public const string EndsceneAttachmentNotify = "event:/new_content/game/pusheen/19_spaces/endscene_attachment_notify";
                public const string EndsceneDialTheo         = "event:/new_content/game/pusheen/19_spaces/endscene_dial_theo";
                public const string EndsceneFinalInput       = "event:/new_content/game/pusheen/19_spaces/endscene_final_input";
                public const string EndscenePhotoZoom        = "event:/new_content/game/pusheen/19_spaces/endscene_photo_zoom";
                public const string FakeheartBounce          = "event:/new_content/game/pusheen/19_spaces/fakeheart_bounce";
                public const string FakeheartGet             = "event:/new_content/game/pusheen/19_spaces/fakeheart_get";
                public const string FakeheartPulse           = "event:/new_content/game/pusheen/19_spaces/fakeheart_pulse";
                public const string FuseboxHit1              = "event:/new_content/game/pusheen/19_spaces/fusebox_hit_1";
                public const string FuseboxHit2              = "event:/new_content/game/pusheen/19_spaces/fusebox_hit_2";
                public const string GliderEmancipate         = "event:/new_content/game/pusheen/19_spaces/glider_emancipate";
                public const string GliderEngage             = "event:/new_content/game/pusheen/19_spaces/glider_engage";
                public const string GliderLand               = "event:/new_content/game/pusheen/19_spaces/glider_land";
                public const string GliderMovement           = "event:/new_content/game/pusheen/19_spaces/glider_movement";
                public const string GliderPlatformDissipate  = "event:/new_content/game/pusheen/19_spaces/glider_platform_dissipate";
                public const string GliderWallbounceLeft     = "event:/new_content/game/pusheen/19_spaces/glider_wallbounce_left";
                public const string GliderWallbounceRight    = "event:/new_content/game/pusheen/19_spaces/glider_wallbounce_right";
                public const string GlitchLong               = "event:/new_content/game/pusheen/19_spaces/glitch_long";
                public const string GlitchMedium             = "event:/new_content/game/pusheen/19_spaces/glitch_medium";
                public const string GlitchShort              = "event:/new_content/game/pusheen/19_spaces/glitch_short";
                public const string HeartDoor                = "event:/new_content/game/pusheen/19_spaces/heart_door";
                public const string KeyUnlock1               = "event:/new_content/game/pusheen/19_spaces/key_unlock_1";
                public const string KeyUnlock2               = "event:/new_content/game/pusheen/19_spaces/key_unlock_2";
                public const string KeyUnlock3               = "event:/new_content/game/pusheen/19_spaces/key_unlock_3";
                public const string KeyUnlock4               = "event:/new_content/game/pusheen/19_spaces/key_unlock_4";
                public const string KeyUnlock5               = "event:/new_content/game/pusheen/19_spaces/key_unlock_5";
                public const string LightningStrike          = "event:/new_content/game/pusheen/19_spaces/lightning_strike";
                public const string LockedDoorAppear1        = "event:/new_content/game/pusheen/19_spaces/locked_door_appear_1";
                public const string LockedDoorAppear2        = "event:/new_content/game/pusheen/19_spaces/locked_door_appear_2";
                public const string LockedDoorAppear3        = "event:/new_content/game/pusheen/19_spaces/locked_door_appear_3";
                public const string LockedDoorAppear4        = "event:/new_content/game/pusheen/19_spaces/locked_door_appear_4";
                public const string LockedDoorAppear5        = "event:/new_content/game/pusheen/19_spaces/locked_door_appear_5";
                public const string Pico8Flag                = "event:/new_content/game/pusheen/19_spaces/pico8_flag";
                public const string PinkDiamondReturn        = "event:/new_content/game/pusheen/19_spaces/pinkdiamond_return";
                public const string PinkDiamondTouch         = "event:/new_content/game/pusheen/19_spaces/pinkdiamond_touch";
                public const string PptCubeTransition        = "event:/new_content/game/pusheen/19_spaces/ppt_cube_transition";
                public const string PptDissolveTransition    = "event:/new_content/game/pusheen/19_spaces/ppt_dissolve_transition";
                public const string PptDoubleclick           = "event:/new_content/game/pusheen/19_spaces/ppt_doubleclick";
                public const string PptHappyWavedashing      = "event:/new_content/game/pusheen/19_spaces/ppt_happy_wavedashing";
                public const string PptImpossible            = "event:/new_content/game/pusheen/19_spaces/ppt_impossible";
                public const string PptItsEasy               = "event:/new_content/game/pusheen/19_spaces/ppt_its_easy";
                public const string PptMouseclick            = "event:/new_content/game/pusheen/19_spaces/ppt_mouseclick";
                public const string PptSpinningTransition    = "event:/new_content/game/pusheen/19_spaces/ppt_spinning_transition";
                public const string PptWavedashWhoosh        = "event:/new_content/game/pusheen/19_spaces/ppt_wavedash_whoosh";
                public const string PufferBoop               = "event:/new_content/game/pusheen/19_spaces/puffer_boop";
                public const string PufferExpand             = "event:/new_content/game/pusheen/19_spaces/puffer_expand";
                public const string PufferReform             = "event:/new_content/game/pusheen/19_spaces/puffer_reform";
                public const string PufferReturn             = "event:/new_content/game/pusheen/19_spaces/puffer_return";
                public const string PufferShrink             = "event:/new_content/game/pusheen/19_spaces/puffer_shrink";
                public const string PufferSplode             = "event:/new_content/game/pusheen/19_spaces/puffer_splode";
                public const string QuakeOnset               = "event:/new_content/game/pusheen/19_spaces/quake_onset";
                public const string QuakeRockbreak           = "event:/new_content/game/pusheen/19_spaces/quake_rockbreak";
                public const string StrawberryGoldDetach     = "event:/new_content/game/pusheen/19_spaces/strawberry_gold_detach";
                public const string ZipMover                 = "event:/new_content/game/pusheen/19_spaces/zip_mover";
            }
        }

        // ─── Bosses (New Content) ─────────────────────────────────────────────

        public static class Bosses
        {
            // event:/new_content/char/pusheen/bosses/asriel/*
            public static class Asriel
            {
                public const string BigBulletFire         = "event:/new_content/char/pusheen/bosses/asriel/big_bullet_fire";
                public const string BigLaser              = "event:/new_content/char/pusheen/bosses/asriel/big_laser";
                public const string BigStar               = "event:/new_content/char/pusheen/bosses/asriel/big_star";
                public const string BiggerGunMechanized   = "event:/new_content/char/pusheen/bosses/asriel/biggergunmechanized";
                public const string ChargeIntro           = "event:/new_content/char/pusheen/bosses/asriel/charge_intro";
                public const string Cinematiccut          = "event:/new_content/char/pusheen/bosses/asriel/cinematiccut";
                public const string Cinematiccut2d        = "event:/new_content/char/pusheen/bosses/asriel/cinematiccut_2d";
                public const string Cinematiccut3d        = "event:/new_content/char/pusheen/bosses/asriel/cinematiccut_3d";
                public const string CinematictcutFlame2d  = "event:/new_content/char/pusheen/bosses/asriel/cinematiccut_Flame_2d";
                public const string CrashBubbleBurst      = "event:/new_content/char/pusheen/bosses/asriel/crashbubbleburst";
                public const string CrashBubbleCharge     = "event:/new_content/char/pusheen/bosses/asriel/crashvubblecharge";
                public const string Create                = "event:/new_content/char/pusheen/bosses/asriel/create";
                public const string FinalBeam             = "event:/new_content/char/pusheen/bosses/asriel/finalbeam";
                public const string GetHit                = "event:/new_content/char/pusheen/bosses/asriel/get_hit";
                public const string Grab                  = "event:/new_content/char/pusheen/bosses/asriel/grab";
                public const string Gunshot               = "event:/new_content/char/pusheen/bosses/asriel/gunshot";
                public const string HypergonorCharge      = "event:/new_content/char/pusheen/bosses/asriel/hypergoner_charge";
                public const string Laser                 = "event:/new_content/char/pusheen/bosses/asriel/laser";
                public const string LaserBeamStrikeImpact = "event:/new_content/char/pusheen/bosses/asriel/laser_Beam_strike_impact";
                public const string LaserIntro            = "event:/new_content/char/pusheen/bosses/asriel/laser_intro";
                public const string LightningHit1         = "event:/new_content/char/pusheen/bosses/asriel/lightning_hit1";
                public const string LightningHit2         = "event:/new_content/char/pusheen/bosses/asriel/lightning_hit2";
                public const string PhaserBlast           = "event:/new_content/char/pusheen/bosses/asriel/phaser_blast";
                public const string Roar                  = "event:/new_content/char/pusheen/bosses/asriel/roar";
                public const string Sparkles              = "event:/new_content/char/pusheen/bosses/asriel/sparkles";
                public const string Spellcast             = "event:/new_content/char/pusheen/bosses/asriel/spellcast";
                public const string SpellcastGlitch       = "event:/new_content/char/pusheen/bosses/asriel/spellcast_glitch";
                public const string Star                  = "event:/new_content/char/pusheen/bosses/asriel/star";
            }

            // event:/new_content/char/pusheen/bosses/els/*
            public static class Els
            {
                public const string Beam               = "event:/new_content/char/pusheen/bosses/els/beam";
                public const string BigHit             = "event:/new_content/char/pusheen/bosses/els/big_hit";
                public const string BiggerStar         = "event:/new_content/char/pusheen/bosses/els/bigger_star";
                public const string BiggerStarGlitch   = "event:/new_content/char/pusheen/bosses/els/bigger_star_glitch";
                public const string ConsumeCrystalHeart = "event:/new_content/char/pusheen/bosses/els/consume_crystal_heart";
                public const string Create             = "event:/new_content/char/pusheen/bosses/els/create";
                public const string CryingDeath        = "event:/new_content/char/pusheen/bosses/els/crying_death";
                public const string DarkMatterSpawn    = "event:/new_content/char/pusheen/bosses/els/dark_matter_spawn";
                public const string GetHit             = "event:/new_content/char/pusheen/bosses/els/get_hit";
                public const string GetHitExtra        = "event:/new_content/char/pusheen/bosses/els/get_hit_extra";
                public const string Idle               = "event:/new_content/char/pusheen/bosses/els/idle";
                public const string Impact             = "event:/new_content/char/pusheen/bosses/els/impact";
                public const string Knockout           = "event:/new_content/char/pusheen/bosses/els/knockout";
                public const string Laugh              = "event:/new_content/char/pusheen/bosses/els/laugh";
                public const string Laughing           = "event:/new_content/char/pusheen/bosses/els/laughing";
                public const string ManipulatedTime    = "event:/new_content/char/pusheen/bosses/els/manipultaed_time";
                public const string PreCreate          = "event:/new_content/char/pusheen/bosses/els/pre_create";
                public const string PreImpact          = "event:/new_content/char/pusheen/bosses/els/pre_impact";
                public const string Revival            = "event:/new_content/char/pusheen/bosses/els/revival";
                public const string RiftBullet         = "event:/new_content/char/pusheen/bosses/els/rift_bullet";
                public const string RiftOpen           = "event:/new_content/char/pusheen/bosses/els/rift_open";
                public const string ShellCrack         = "event:/new_content/char/pusheen/bosses/els/shell_crack";
                public const string Slash              = "event:/new_content/char/pusheen/bosses/els/slash";
                public const string Spawn              = "event:/new_content/char/pusheen/bosses/els/spawn";
                public const string Teleport           = "event:/new_content/char/pusheen/bosses/els/teleport";
                public const string Woosh              = "event:/new_content/char/pusheen/bosses/els/woosh";
            }
        }

        // ─── Classic / Pico-8 ─────────────────────────────────────────────────

        public static class Classic
        {
            public const string Pico8Boot    = "event:/classic/pusheen/pico8_boot";
            public const string Pico8Mus00   = "event:/classic/pusheen/pico8_mus_00";
            public const string Pico8Mus01   = "event:/classic/pusheen/pico8_mus_01";
            public const string Pico8Mus02   = "event:/classic/pusheen/pico8_mus_02";
            public const string Pico8Mus03   = "event:/classic/pusheen/pico8_mus_03";
            public const string Sfx0         = "event:/classic/pusheen/sfx0";
            public const string Sfx1         = "event:/classic/pusheen/sfx1";
            public const string Sfx2         = "event:/classic/pusheen/sfx2";
            public const string Sfx3         = "event:/classic/pusheen/sfx3";
            public const string Sfx4         = "event:/classic/pusheen/sfx4";
            public const string Sfx5         = "event:/classic/pusheen/sfx5";
            public const string Sfx6         = "event:/classic/pusheen/sfx6";
            public const string Sfx7         = "event:/classic/pusheen/sfx7";
            public const string Sfx8         = "event:/classic/pusheen/sfx8";
            public const string Sfx9         = "event:/classic/pusheen/sfx9";
            public const string Sfx13        = "event:/classic/pusheen/sfx13";
            public const string Sfx14        = "event:/classic/pusheen/sfx14";
            public const string Sfx15        = "event:/classic/pusheen/sfx15";
            public const string Sfx16        = "event:/classic/pusheen/sfx16";
            public const string Sfx23        = "event:/classic/pusheen/sfx23";
            public const string Sfx35        = "event:/classic/pusheen/sfx35";
            public const string Sfx37        = "event:/classic/pusheen/sfx37";
            public const string Sfx38        = "event:/classic/pusheen/sfx38";
            public const string Sfx51        = "event:/classic/pusheen/sfx51";
            public const string Sfx54        = "event:/classic/pusheen/sfx54";
            public const string Sfx55        = "event:/classic/pusheen/sfx55";
            public const string Sfx61        = "event:/classic/pusheen/sfx61";
            public const string Sfx62        = "event:/classic/pusheen/sfx62";
        }

        // ─── Snapshots ────────────────────────────────────────────────────────

        /// <summary>
        /// FMOD audio mixer snapshots. Use with <c>Audio.CreateSnapshot()</c> and
        /// <c>Audio.ReleaseSnapshot()</c> to activate/deactivate audio mix states.
        /// </summary>
        public static class Snapshots
        {
            // Assist/speed variants
            public const string AssistSpeed50          = "snapshot:/assist_game_speed/assist_speed_50";
            public const string AssistSpeed60          = "snapshot:/assist_game_speed/assist_speed_60";
            public const string AssistSpeed70          = "snapshot:/assist_game_speed/assist_speed_70";
            public const string AssistSpeed80          = "snapshot:/assist_game_speed/assist_speed_80";
            public const string AssistSpeed90          = "snapshot:/assist_game_speed/assist_speed_90";
            public const string VariantSpeed120        = "snapshot:/variant_speed/variant_speed_120";
            public const string VariantSpeed140        = "snapshot:/variant_speed/variant_speed_140";
            public const string VariantSpeed160        = "snapshot:/variant_speed/variant_speed_160";

            // Berry cooperation
            public const string BerryCooperation1000s = "snapshot:/berry_cooperation/1000s_down";
            public const string BerryCooperation2000s = "snapshot:/berry_cooperation/2000s_down";
            public const string BerryCooperation3000s = "snapshot:/berry_cooperation/3000s_down";
            public const string BerryCooperation4000s = "snapshot:/berry_cooperation/4000s_down";
            public const string BerryCooperation5000s = "snapshot:/berry_cooperation/5000s_down";

            // General
            public const string BossPitchSfx           = "snapshot:/boss_pitch_sfx";
            public const string GenBossPitchSfx        = "snapshot:/gen_boss_pitch_sfx";
            public const string CharGrannyLaughsDown   = "snapshot:/char_granny_laughs_down";
            public const string DialogueInProgress     = "snapshot:/dialogue_in_progress";
            public const string EnvAllAmbDown          = "snapshot:/env_allamb_down";
            public const string EnvNewWorldmapDown     = "snapshot:/env_new_worldmap_down";
            public const string EnvWorldmapDown        = "snapshot:/env_worldmap_down";
            public const string MusicAllMute           = "snapshot:/music_all_mute";
            public const string MusicMainsMute         = "snapshot:/music_mains_mute";
            public const string MusicEdgeSecret        = "snapshot:/music_edge_secret";
            public const string MusicKirbySecretRevealed = "snapshot:/music_kirby_secretrevealed";
            public const string MusicReflectionSecret  = "snapshot:/music_reflection_secret";
            public const string MusicSecretRevealed    = "snapshot:/music_secretrevealed";
            public const string MusCassetteAmbDown     = "snapshot:/mus_cassette_amb_down";
            public const string MusTapeAmbDown         = "snapshot:/mus_tape_amb_down";
            public const string MsLvl1VerbTransition   = "snapshot:/mus_lvl1_verbtransition";
            public const string MsLvl3VerbTransition   = "snapshot:/mus_lvl3_verbtransition";
            public const string PauseMenu              = "snapshot:/pause_menu";
            public const string Underwater             = "snapshot:/underwater";

            // Chapter 00
            public const string Game00PrologueAmbDown  = "snapshot:/game_00_prologue_amb_down";
            public const string Game00PrologueAmbOff   = "snapshot:/game_00_prologue_amb_off";
            public const string Game00Verb             = "snapshot:/game_00_verb";

            // Chapter 01
            public const string Game01BirdbrosFinish   = "snapshot:/game_01_birdbros_finish";
            public const string Game01BirdsisFinish    = "snapshot:/game_01_birdsis_finish";

            // Chapter 02
            public const string Game02DreammemorialFade = "snapshot:/game_02_dreammemorial_fade";
            public const string Game02ShadowFade        = "snapshot:/game_02_shadow_fade";

            // Chapter 03
            public const string Game03BirdkidFinish     = "snapshot:/game_03_birdkid_finish";
            public const string Game03ClutterswitchMoment = "snapshot:/game_03_clutterswitch_moment";
            public const string Game03OshiroFreakout    = "snapshot:/game_03_oshirofreakout";
            public const string Game03Pico8Room         = "snapshot:/game_03_pico8room";

            // Chapter 04
            public const string Game04GondolaFeatherMain = "snapshot:/game_04_gondolafeather_main";
            public const string Game04GondolaFeatherVerb = "snapshot:/game_04_gondolafeather_verb";
            public const string Game04LegendFade         = "snapshot:/game_04_legend_fade";

            // Chapter 05
            public const string Game05ClutterswitchMoment = "snapshot:/game_05_clutterswitch_moment";
            public const string Game05Eyedeath           = "snapshot:/game_05_eyedeath";
            public const string Game05Eyedistance        = "snapshot:/game_05_eyedistance";
            public const string Game05MusPulseController = "snapshot:/game_05_mus_pulse_controller";
            public const string Game05OshiroFreakout     = "snapshot:/game_05_oshirofreakout";
            public const string Game05PicoboyRoom        = "snapshot:/game_05_picoboyroom";
            public const string Game05TorchArp           = "snapshot:/game_05_torch_arp";

            // Chapter 06
            public const string Game06GondolaFeatherMain = "snapshot:/game_06_gondolafeather_main";
            public const string Game06GondolaFeatherVerb = "snapshot:/game_06_gondolafeather_verb";

            // Chapter 07
            public const string Game07BiggerEyedeath     = "snapshot:/game_07_bigger_eyedeath";
            public const string Game07BiggerEyedistance  = "snapshot:/game_07_bigger_eyedistance";
            public const string Game07InfernoArp         = "snapshot:/game_07_inferno_arp";
            public const string Game07MusPulseController = "snapshot:/game_07_mus_pulse_controller";

            // Chapter 10
            public const string Game10AmbVoidspiral      = "snapshot:/game_10_amb_voidspiral_active";
            public const string Game10BirMusicPart01     = "snapshot:/game_10_BIR_music_part01";
            public const string Game10BirMusicPart02     = "snapshot:/game_10_BIR_music_part02";
            public const string Game10BirSfx             = "snapshot:/game_10_BIR_sfx";
            public const string Game10BirdWingsSilenced  = "snapshot:/game_10_bird_wings_silenced";
            public const string Game10CafeComputerActive = "snapshot:/game_10_cafe_computer_active";
            public const string Game10FinalBoost         = "snapshot:/game_10_final_boost";
            public const string Game10GlitchActive       = "snapshot:/game_10_glitch_active";
            public const string Game10GoldenRoomFlavour  = "snapshot:/game_10_golden_room_flavour";
            public const string Game10GoldenroomDeathFix = "snapshot:/game_10_goldenroom_death_fix";
            public const string Game10GrannyCloudsDlg    = "snapshot:/game_10_granny_clouds_dialogue";
            public const string Game10InSpace            = "snapshot:/game_10_in_space";
            public const string Game10InsideCafe         = "snapshot:/game_10_inside_cafe";
            public const string Game10KevinPcSendControl = "snapshot:/game_10_kevinpc_sendcontrol";
            public const string Game10KevinPcVerbTransition = "snapshot:/game_10_kevinpc_verbtransition";

            // Chapter 19
            public const string Game19AmbVoidspiral      = "snapshot:/game_19_amb_voidspiral_active";
            public const string Game19BirMusicPart01     = "snapshot:/game_19_BIR_music_part01";
            public const string Game19BirMusicPart02     = "snapshot:/game_19_BIR_music_part02";
            public const string Game19BirSfx             = "snapshot:/game_19_BIR_sfx";
            public const string Game19BirdWingsSilenced  = "snapshot:/game_19_bird_wings_silenced";
            public const string Game19CafeComputerActive = "snapshot:/game_19_cafe_computer_active";
            public const string Game19GlitchActive       = "snapshot:/game_19_glitch_active";
            public const string Game19GoldenroomDeathFix = "snapshot:/game_19_goldenroom_death_fix";
            public const string Game19GreenGreensRoomFlavour = "snapshot:/game_19_greengreens_room_flavour";
            public const string Game19GreenGreensroomDeathFix = "snapshot:/game_19_greengreensroom_death_fix";
            public const string Game19InVoid             = "snapshot:/game_19_in_void";
            public const string Game19InsideCafe         = "snapshot:/game_19_inside_cafe";
            public const string Game19PusheenPcSendControl = "snapshot:/game_19_pusheenpc_sendcontrol";
            public const string Game19PusheenPcVerbTransition = "snapshot:/game_19_pusheenpc_verbtransition";

            // Chapter 21
            public const string Game21HeavenCloudsDlg    = "snapshot:/game_21_heaven_clouds_dialogue";

            // Game general
            public const string GameGenCrystalheart      = "snapshot:/game_gen_crystalheart";
            public const string GameGenDashAssistActive  = "snapshot:/game_gen_dash_assist_active";
            public const string GameGenFinalBoost        = "snapshot:/game_gen_final_boost";
            public const string GameGenLargeBerryGet     = "snapshot:/game_gen_large_berry_get";
        }
    }
}
