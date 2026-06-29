using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Celeste.Cutscenes;
using Celeste.Entities;
using Celeste.Mod;
using Celeste.NPCs;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using FlingBirdIntro = Celeste.Entities.FlingBirdIntro;
using NPC = Celeste.NPCs.NPC;

namespace DZ
{
    [CustomEntity("DZ/EventTrigger")]
    [Tracked]
    public class MaggyEventTrigger : Trigger
    {
        public delegate Entity CutsceneLoader(MaggyEventTrigger trigger, Player player, string eventID);

        public string Event;

        public bool OnSpawnHack;

        public bool OnlyOnce;

        private bool triggered;

        private string onceFlag;

        private EventInstance snapshot;

        private static HashSet<string> _LoadStrings = new HashSet<string>
        {
            "end_city", "end_oldsite_dream", "end_oldsite_awake", "ch5_see_theo", "ch5_found_theo", "ch5_mirror_reflection", "cancel_ch5_see_theo", "ch6_boss_intro", "ch6_reflect", "ch7_summit",
            "ch8_door", "ch9_goto_the_future", "ch9_goto_the_past", "ch9_moon_intro", "ch9_hub_intro", "ch9_hub_transition_out", "ch9_badeline_helps", "ch9_farewell", "ch9_ending", "ch9_end_golden",
            "ch9_final_room", "ch9_ding_ding_ding", "ch9_golden_snapshot", "seeTheoInCrystal", "foundTheoInCrystal", "reflection", "it_ch5_see_theo", "it_ch5_see_theo_b", "ignore_darkness_", "boss_intro",
            "reflection", "moon_intro", "hub_intro", "badeline_helps", "final_room_deaths", "final_room_deaths", "event:/new_content/game/10_farewell/pico8_flag", "decals/10-farewell/finalflag", "snapshot:/game_10_golden_room_flavour", "golden",
            "cs03_first_step", "cs03_meetup", "cs03_mod_ending",
            "Event '", "' does not exist!"
        };

        public static readonly Dictionary<string, CutsceneLoader> CutsceneLoaders = new Dictionary<string, CutsceneLoader>();

        // Static constructor ensures CutsceneEventDispatcher is initialized to populate CutsceneLoaders
        static MaggyEventTrigger()
        {
            RuntimeHelpers.RunClassConstructor(typeof(global::Celeste.Triggers.CutsceneEventDispatcher).TypeHandle);
        }

        public float Time { get; private set; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public MaggyEventTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Event = data.Attr("event");
            OnSpawnHack = data.Bool("onSpawn");
            OnlyOnce = data.Bool("onlyOnce", true);
            onceFlag = $"event_trigger_once_{data.Level.Name}_{data.ID}";
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (OnSpawnHack)
            {
                Player player = CollideFirst<Player>();
                if (player != null)
                {
                    OnEnter(player);
                }
            }
            if (Event == "ch9_badeline_helps")
            {
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                if (entity != null && entity.Left > base.Right)
                {
                    RemoveSelf();
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void OnEnter(Player player)
        {
            Player player2 = player;
            if (triggered)
            {
                return;
            }
            Level level = base.Scene as Level;
            if (OnlyOnce && level.Session.GetFlag(onceFlag))
            {
                return;
            }
            triggered = true;
            if (OnlyOnce)
            {
                level.Session.SetFlag(onceFlag, true);
            }
            if (TriggerCustomEvent(this, player, Event))
            {
                return;
            }
            switch (Event)
            {
                case "end_city":
                    base.Scene.Add(new CS01_Ending(player2));
                    break;
                case "end_oldsite_dream":
                    base.Scene.Add(new CS02_DreamingPhonecall(player2));
                    break;
                case "end_oldsite_awake":
                    base.Scene.Add(new CS02_Ending(player2));
                    break;
                case "ch5_see_theo":
                    if (!(base.Scene as Level).Session.GetFlag("seeTheoInCrystal"))
                    {
                        base.Scene.Add(new CS05_SeeTheo(player2, 0));
                    }
                    break;
                case "ch5_found_theo":
                    if (!level.Session.GetFlag("foundTheoInCrystal"))
                    {
                        base.Scene.Add(new CS05_SaveTheo(player2));
                    }
                    break;
                case "ch5_mirror_reflection":
                    if (!level.Session.GetFlag("reflection"))
                    {
                        base.Scene.Add(new CS05_Reflection1(player2));
                    }
                    break;
                case "cancel_ch5_see_theo":
                    level.Session.SetFlag("it_ch5_see_theo");
                    level.Session.SetFlag("it_ch5_see_theo_b");
                    level.Session.SetFlag("ignore_darkness_" + level.Session.Level);
                    Add(new Coroutine(Brighten()));
                    break;
                case "ch6_boss_intro":
                    if (!level.Session.GetFlag("boss_intro"))
                    {
                        level.Add(new CS06_BossIntro(base.Center.X, player2, level.Entities.FindFirst<FinalBoss>()));
                    }
                    break;
                case "ch6_reflect":
                    if (!level.Session.GetFlag("reflection"))
                    {
                        base.Scene.Add(new CS06_Reflection(player2, base.Center.X - 5f));
                    }
                    break;
                case "ch7_summit":
                    base.Scene.Add(new CS07_Ending(player2, new Vector2(base.Center.X, base.Bottom)));
                    break;
                case "ch8_door":
                    base.Scene.Add(new CS08_EnterDoor(player2, base.Left));
                    break;
                case "ch9_goto_the_future":
                case "ch9_goto_the_past":
                    level.OnEndOfFrame += [MethodImpl(MethodImplOptions.NoInlining)] () =>
                    {
                        new Vector2(level.LevelOffset.X + (float)level.Bounds.Width - player2.X, player2.Y - level.LevelOffset.Y);
                        Vector2 levelOffset = level.LevelOffset;
                        Vector2 vector = player2.Position - level.LevelOffset;
                        Vector2 vector2 = level.Camera.Position - level.LevelOffset;
                        Facings facing = player2.Facing;
                        level.Remove(player2);
                        level.UnloadLevel();
                        level.Session.Dreaming = true;
                        level.Session.Level = ((Event == "ch9_goto_the_future") ? "intro-01-future" : "intro-00-past");
                        level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
                        level.Session.FirstLevel = false;
                        level.LoadLevel(Player.IntroTypes.Transition);
                        level.Camera.Position = level.LevelOffset + vector2;
                        level.Session.Inventory.Dashes = 1;
                        player2.Dashes = Math.Min(player2.Dashes, 1);
                        level.Add(player2);
                        player2.Position = level.LevelOffset + vector;
                        player2.Facing = facing;
                        player2.Hair.MoveHairBy(level.LevelOffset - levelOffset);
                        if (level.Wipe != null)
                        {
                            level.Wipe.Cancel();
                        }
                        level.Flash(Color.White);
                        level.Shake();
                        level.Add(new LightningStrike(new Vector2(player2.X + 60f, level.Bounds.Bottom - 180), 10, 200f));
                        level.Add(new LightningStrike(new Vector2(player2.X + 220f, level.Bounds.Bottom - 180), 40, 200f, 0.25f));
                        Audio.Play("event:/new_content/game/10_farewell/lightning_strike");
                    };
                    break;
                case "ch9_moon_intro":
                    if (!level.Session.GetFlag("moon_intro") && player2.StateMachine.State == 13)
                    {
                        base.Scene.Add(new CS10_MoonIntro(player2));
                        break;
                    }
                    level.Entities.FindFirst<Celeste.Entities.BirdNPC>()?.RemoveSelf();
                    level.Session.Inventory.Dashes = 1;
                    player2.Dashes = 1;
                    break;
                case "ch9_hub_intro":
                    if (!level.Session.GetFlag("hub_intro"))
                    {
                        base.Scene.Add(new CS10_HubIntro(base.Scene, player2));
                    }
                    break;
                case "ch9_hub_transition_out":
                    Add(new Coroutine(Ch9HubTransitionBackgroundToBright(player2)));
                    break;
                case "ch9_badeline_helps":
                    if (!level.Session.GetFlag("badeline_helps"))
                    {
                        base.Scene.Add(new CS10_BadelineHelps(player2));
                    }
                    break;
                case "ch9_farewell":
                    base.Scene.Add(new CS10_Farewell(player2));
                    break;
                case "ch9_ending":
                    base.Scene.Add(new CS10_Ending(player2));
                    break;
                case "ch9_end_golden":
                    ScreenWipe.WipeColor = Color.White;
                    new FadeWipe(level, wipeIn: false, [MethodImpl(MethodImplOptions.NoInlining)] () =>
                    {
                        level.OnEndOfFrame += [MethodImpl(MethodImplOptions.NoInlining)] () =>
                        {
                            level.TeleportTo(player2, "end-granny", Player.IntroTypes.Transition);
                            player2.Speed = Vector2.Zero;
                        };
                    }).Duration = 1f;
                    break;
                case "ch9_final_room":
                {
                    Session session = (base.Scene as Level).Session;
                    switch (session.GetCounter("final_room_deaths"))
                    {
                        case 0:
                            base.Scene.Add(new CS10_FinalRoom(player2, first: true));
                            break;
                        case 50:
                            base.Scene.Add(new CS10_FinalRoom(player2, first: false));
                            break;
                    }
                    session.IncrementCounter("final_room_deaths");
                    break;
                }
                case "ch9_ding_ding_ding":
                {
                    Audio.Play("event:/new_content/game/10_farewell/pico8_flag", base.Center);
                    Decal decal = null;
                    foreach (Decal item in base.Scene.Entities.FindAll<Decal>())
                    {
                        if (item.Name.Equals("decals/10-farewell/finalflag", StringComparison.OrdinalIgnoreCase))
                        {
                            decal = item;
                            break;
                        }
                    }
                    decal?.FinalFlagTrigger();
                    break;
                }
                case "ch9_golden_snapshot":
                    snapshot = Audio.CreateSnapshot("snapshot:/game_10_golden_room_flavour");
                    (base.Scene as Level).SnapColorGrade("golden");
                    break;
                default:
                    throw new Exception("Event '" + Event + "' does not exist!");
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Audio.ReleaseSnapshot(snapshot);
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Audio.ReleaseSnapshot(snapshot);
        }

        private IEnumerator Brighten()
        {
            Level level = Scene as Level;
            float darkness = AreaData.Get(level).DarknessAlpha;
            while (level.Lighting.Alpha != darkness)
            {
                level.Lighting.Alpha = Calc.Approach(level.Lighting.Alpha, darkness, Engine.DeltaTime * 4f);
                yield return null;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator Ch9HubTransitionBackgroundToBright(Player player)
        {
            Level level = Scene as Level;
            float start = Bottom;
            float end = Top;
            while (true)
            {
                float fadeAlphaMultiplier = Calc.ClampedMap(player.Y, start, end);
                foreach (Backdrop item in level.Background.GetEach<Backdrop>("bright"))
                {
                    item.ForceVisible = true;
                    item.FadeAlphaMultiplier = fadeAlphaMultiplier;
                }
                yield return null;
            }
        }

        public static bool TriggerCustomEvent(MaggyEventTrigger trigger, Player player, string eventID)
        {
            if (CutsceneLoaders.TryGetValue(eventID, out var value))
            {
                Entity entity = value(trigger, player, eventID);
                if (entity != null)
                {
                    trigger.Scene.Add(entity);
                    return true;
                }
            }
            if (!_LoadStrings.Contains(eventID))
            {
                string text = "EventTrigger";
                bool shouldLog;
                Logger.LogInterpolatedStringHandler<LogLevelConstTypes.Warn> str = new Logger.LogInterpolatedStringHandler<LogLevelConstTypes.Warn>(24, 1, text, out shouldLog);
                if (shouldLog)
                {
                    str.AppendLiteral("Event '");
                    str.AppendFormatted(eventID);
                    str.AppendLiteral("' does not exist!");
                }
                Logger.Warn(text, str);
                return true;
            }
            return false;
        }
    }
}

namespace Celeste.Triggers
{
    internal static class CutsceneEventDispatcher
    {
        internal delegate bool CutsceneRunner(string flag, Func<CutsceneEntity> factory);
        internal delegate bool ActionRunner(string flag, Func<bool> action);

        static CutsceneEventDispatcher()
        {
            RegisterCutscenes();
        }

        private static void RegisterCutscenes()
        {
            // Register only verified cutscenes with compatible signatures
            // Chapter 1
            Register("cs01_mod_ending", (trigger, player, eventId) => new global::Celeste.Cutscenes.Cs01ModEnding(player));
            
            // Chapter 2
            Register("cs02_chara_intro", (trigger, player, eventId) => {
                var chara = (trigger.Scene as Level).Entities.FindFirst<global::Celeste.Entities.CharaChaser>();
                return chara != null ? new global::Celeste.Cutscenes.CS02_CharaIntro(chara) : null;
            });

            // Chapter 3
            Register("cs03_first_step", (trigger, player, eventId) => {
                var kPlayer = GetKPlayer(trigger.Scene);
                var actualPlayer = kPlayer != null ? Unsafe.As<global::Celeste.Entities.K_Player, global::Celeste.Player>(ref kPlayer) : player;
                return new global::Celeste.Cutscenes.Cs03FirstStep(actualPlayer);
            });
            Register("cs03_meetup", (trigger, player, eventId) => {
                var level = trigger.Scene as Level;
                var maggy = level?.Entities.FindFirst<global::Celeste.NPCs.Npc03Maggy>();
                if (maggy == null) return null;
                var zoomCoroutine = new Coroutine(level.ZoomTo(maggy.Position + new Vector2(0f, -16f), 1.5f, 2f));
                int conv = 0;
                if (global::Celeste.SaveData.Instance?.HasFlag("WassupMagolor") == true &&
                    global::Celeste.SaveData.Instance?.HasFlag("BadelineJoinKirby") == true)
                {
                    if (!level.Session.GetFlag("maggy_03_Meetup_conv1")) conv = 1;
                    else if (!level.Session.GetFlag("maggy_03_Meetup_conv2")) conv = 2;
                    else if (!level.Session.GetFlag("maggy_03_Meetup_conv3")) conv = 3;
                    else if (!level.Session.GetFlag("maggy_03_Meetup_conv4")) conv = 4;
                }
                var kPlayer = GetKPlayer(trigger.Scene);
                var actualPlayer = kPlayer != null ? Unsafe.As<global::Celeste.Entities.K_Player, global::Celeste.Player>(ref kPlayer) : player;
                return new global::Celeste.Cutscenes.Cs03Meetup(maggy, actualPlayer, zoomCoroutine, conv);
            });
            Register("cs03_mod_ending", (trigger, player, eventId) => {
                var kPlayer = GetKPlayer(trigger.Scene);
                var actualPlayer = kPlayer != null ? Unsafe.As<global::Celeste.Entities.K_Player, global::Celeste.Player>(ref kPlayer) : player;
                return new global::Celeste.Cutscenes.Cs03ModEnding(actualPlayer);
            });
            
            // Chapter 7
            Register("cs07_darker", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS07_Darker(player));
            Register("cs07_genocide_vision_finale", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS07_GenocideVisionFinale(player));
            Register("cs07_genocide_vision_intro", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS07_GenocideVisionIntro(player));
            Register("cs07_genocide_wakeup", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS07_GenocideWakeup(player));
            
            // Chapter 9
            Register("cs09_area_complete", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS09_AreaComplete(player));
            Register("cs09_credits", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS09_Credits(player));
            Register("cs09_golden_flower", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS09_GoldenFlower(player));
            Register("cs09_message_end", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS09_MessageEnd(player));
            Register("ch9_arrivial", (trigger, player, eventId) => new DZ.CS09_Arrivial(player));

            // Chapter 10 - Flowey intro (full sequence: Flowey emerges, threatens Madeline,
            // Kirby & friends arrive). Delegates to FloweyIntroScene which handles spawning,
            // animations, camera, audio, and the Normal/Returning/Assist dialog variants.
            Register("ch10_flowey_intro", (trigger, player, eventId) => new global::Celeste.Cutscenes.FloweyIntroScene(player));

            // Chapter 15
            Register("ch15_zantas_1", (trigger, player, eventId) => new global::Celeste.Cutscenes.Cs15Zantas1(player));
            Register("ch15_zantas_2", (trigger, player, eventId) => new global::Celeste.Cutscenes.Cs15Zantas2(player));
            
            // Chapter 12
            Register("cs12_titan_boss_intro", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS12_TitanBossIntro(player));
            Register("cs12_titan_boss_outro", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS12_TitanBossOutro(player));

            // Chapter 13
            Register("ch13_tenna_pre_intro_video", (trigger, player, eventId) => {
                Engine.Scene = new global::Celeste.Cutscenes.Cs13TennaVideoVignette(player.SceneAs<Level>().Session, "DZ_CH13_TENNA_PRE_INTRO", "vignettes/ch13_tenna_pre_intro");
                return null;
            });
            Register("ch13_tenna_earth_video", (trigger, player, eventId) => {
                Engine.Scene = new global::Celeste.Cutscenes.Cs13TennaVideoVignette(player.SceneAs<Level>().Session, "DZ_CH13_TENNA_EARTH_VIDEO_destructions", "vignettes/ch13_tenna_earth_video");
                return null;
            });
            // Chapter 13 cassette tape vignettes (individual tapes)
            Register("ch13_tape_00", (trigger, player, eventId) => {
                Engine.Scene = new global::Celeste.Cutscenes.Cs13TapeVignette(player.SceneAs<Level>().Session, global::Celeste.Cutscenes.Cs13TapeVignette.TapeKeys.Tape00);
                return null;
            });
            Register("ch13_tape_01", (trigger, player, eventId) => {
                Engine.Scene = new global::Celeste.Cutscenes.Cs13TapeVignette(player.SceneAs<Level>().Session, global::Celeste.Cutscenes.Cs13TapeVignette.TapeKeys.Tape01);
                return null;
            });
            Register("ch13_tape_02", (trigger, player, eventId) => {
                Engine.Scene = new global::Celeste.Cutscenes.Cs13TapeVignette(player.SceneAs<Level>().Session, global::Celeste.Cutscenes.Cs13TapeVignette.TapeKeys.Tape02);
                return null;
            });
            Register("ch13_tape_03", (trigger, player, eventId) => {
                Engine.Scene = new global::Celeste.Cutscenes.Cs13TapeVignette(player.SceneAs<Level>().Session, global::Celeste.Cutscenes.Cs13TapeVignette.TapeKeys.Tape03);
                return null;
            });
            Register("ch13_tape_04", (trigger, player, eventId) => {
                Engine.Scene = new global::Celeste.Cutscenes.Cs13TapeVignette(player.SceneAs<Level>().Session, global::Celeste.Cutscenes.Cs13TapeVignette.TapeKeys.Tape04);
                return null;
            });
            Register("ch13_tape_05", (trigger, player, eventId) => {
                Engine.Scene = new global::Celeste.Cutscenes.Cs13TapeVignette(player.SceneAs<Level>().Session, global::Celeste.Cutscenes.Cs13TapeVignette.TapeKeys.Tape05);
                return null;
            });
            Register("ch13_tape_final", (trigger, player, eventId) => {
                Engine.Scene = new global::Celeste.Cutscenes.Cs13TapeVignette(player.SceneAs<Level>().Session, global::Celeste.Cutscenes.Cs13TapeVignette.TapeKeys.TapeFinal);
                return null;
            });
            // Chapter 13 full tape sequence vignette (plays all tapes in order)
            Register("ch13_tape_vignette", (trigger, player, eventId) => {
                Engine.Scene = new global::Celeste.Cutscenes.Cs13TapeVignette(player.SceneAs<Level>().Session);
                return null;
            });

            // Chapter 15
            Register("cs15_titan_king_boss", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS15_TitanKingBoss(player));

            // Chapter 16
            Register("cs16_barrier_breaks", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS16_BarrierBreaks(player));
            Register("cs16_corrupted_reality_intro", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS16_CorruptedRealityIntro(player));
            Register("cs16_els_finale", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS16_ElsFinale(player));
            Register("cs16_els_intro", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS16_ElsIntro(player));
            Register("cs16_els_outro", (trigger, player, eventId) => {
                Engine.Scene = new global::Celeste.Cutscenes.CS16_ElsOutro(player.SceneAs<Level>().Session);
                return null; // Return null since we're replacing the scene
            });
            Register("cs16_lost_souls_unite", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS16_LostSoulsUnite(player));
            Register("cs16_save_file_battle", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS16_SaveFileBattle(player));

            // Chapter 18 - Outro vignette (phone call ending that restarts to chapter 19).
            // CS18_Outro is a DesoloZantasVignette (a Scene), so we replace Engine.Scene
            // and return null, matching the cs16_els_outro registration pattern.
            Register("ch18_outro", (trigger, player, eventId) => {
                Engine.Scene = new global::Celeste.Cutscenes.CS18_Outro(player.SceneAs<Level>().Session);
                return null; // Return null since we're replacing the scene
            });

            // Chapter 19
            Register("cs19_another_dimension_intro", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS19_AnotherDimensionIntro(player));
            Register("cs19_gravestone", (trigger, player, eventId) => new CS19_Gravestone(player, null, Vector2.Zero));
            Register("cs19_beyond_the_void", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS19_BeyondTheVoid(player));
            Register("cs19_chara_helps", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS19_CharaHelps(player));
            Register("cs19_edge_of_universe", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS19_EdgeOfUniverse(player));
            Register("cs19_hub_second_intro", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS19_HubSecondIntro(player));
            Register("cs19_trapin_loop", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS19_TrapinLoop(player));

            // Chapter 21
            Register("cs21_els_termina_intro", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS21_ElsTerminaBoss(player, false));
            Register("cs21_els_termina_end", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS21_ElsTerminaBoss(player, true));
            Register("cs21_cast", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS21_Cast(player));
            Register("cs21_epilogue_credits", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS21_EpilogueCredits(player));
            Register("cs21_fake_the_end", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS21_FakeTheEnd(player));
            Register("cs21_final_cutscenes", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS21_FinalCutscenes(player));
            Register("cs21_final_titan_summit", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS21_FinalTitanSummit(player));
            Register("cs21_special_thanks_dodge_credits", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS21_SpecialThanksDodgeCredits(player));
            Register("cs21_two_worlds_unite", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS21_TwoWorldsUnite(player));
            Register("cs21_saved", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS21_Saved(player));
            Register("cs21_farewell", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS21_RestorationAndFarewell(player));
            Register("cs21_ending", (trigger, player, eventId) => new global::Celeste.Cutscenes.CS21_Ending(player));

        }

        private static void Register(string eventId, DZ.MaggyEventTrigger.CutsceneLoader factory)
        {
            DZ.MaggyEventTrigger.CutsceneLoaders[eventId] = factory;
        }

        /// <summary>
        /// Gets the K_Player from the scene if it exists, for K_Player-aware cutscene support.
        /// Returns null if no K_Player is present (vanilla Player mode).
        /// </summary>
        private static global::Celeste.Entities.K_Player GetKPlayer(Scene scene)
        {
            return scene?.Tracker.GetEntity<global::Celeste.Entities.K_Player>();
        }

        private sealed class DispatchContext
        {
            public Level Level { get; }
            public global::Celeste.Player Player { get; }
            public string EventName { get; }
            public CutsceneRunner TriggerCutscene { get; }
            public ActionRunner RunAction { get; }
            public Action DeferCh2CharaIntro { get; }

            public DispatchContext(
                Level level,
                global::Celeste.Player player,
                string eventName,
                CutsceneRunner triggerCutscene,
                ActionRunner runAction,
                Action deferCh2CharaIntro)
            {
                Level = level;
                Player = player;
                EventName = eventName;
                TriggerCutscene = triggerCutscene;
                RunAction = runAction;
                DeferCh2CharaIntro = deferCh2CharaIntro;
            }

            public bool Trigger(string flag, Func<CutsceneEntity> factory)
            {
                return TriggerCutscene(flag, factory);
            }

            public bool Run(string flag, Func<bool> action)
            {
                return RunAction(flag, action);
            }
        }

        public static bool TryDispatch(
            Level level,
            global::Celeste.Player player,
            string eventName,
            CutsceneRunner triggerCutscene,
            ActionRunner runAction,
            Action deferCh2CharaIntro = null)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                return false;
            }

            var context = new DispatchContext(
                level,
                player,
                eventName,
                triggerCutscene,
                runAction,
                deferCh2CharaIntro ?? (() => { }));

            return context.Trigger($"generic_{eventName}_trigger", () => new GenericCutscene(player, eventName));
        }

        private static T FindEntity<T>(DispatchContext context, string description) where T : Entity
        {
            T entity = context.Level.Entities.FindFirst<T>();
            if (entity == null)
            {
                Logger.Log(LogLevel.Warn, nameof(CutsceneEventDispatcher), $"{context.EventName}: {description} not found");
            }

            return entity;
        }

        private sealed class GenericCutscene : CutsceneEntity
        {
            private readonly global::Celeste.Player player;
            private readonly string eventName;

            public GenericCutscene(global::Celeste.Player player, string eventName)
            {
                this.player = player;
                this.eventName = eventName;
            }

            public override void OnBegin(Level level)
            {
                Add(new Coroutine(Cutscene(level)));
            }

            private IEnumerator Cutscene(Level level)
            {
                player.StateMachine.State = Player.StDummy;
                yield return 0.5f;
                yield return Textbox.Say(eventName.ToUpperInvariant());
                yield return 0.5f;
                EndCutscene(level);
            }

            public override void OnEnd(Level level)
            {
                if (player != null)
                {
                    player.StateMachine.State = Player.StNormal;
                }
            }
        }
    }
}

