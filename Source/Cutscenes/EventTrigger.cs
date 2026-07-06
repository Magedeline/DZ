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
    public class DZEventTrigger : Trigger
    {
        public string Event;

        public bool OnSpawnHack;

        public bool OnlyOnce;

        private bool triggered;

        private string onceFlag;

        private EventInstance snapshot;

        private static global::Celeste.Entities.K_Player GetKPlayer(Scene scene)
        {
            return scene?.Tracker.GetEntity<global::Celeste.Entities.K_Player>();
        }

        public float Time { get; private set; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public DZEventTrigger(EntityData data, Vector2 offset)
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
            
            // Freeze player and start cutscene context for vanilla events
            player2.StateMachine.State = Player.StDummy;
            level.StartCutscene(OnCutsceneEnd);
            
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
                case "cancelDZ_CH5_see_theo":
                    level.Session.SetFlag("itDZ_CH5_see_theo");
                    level.Session.SetFlag("itDZ_CH5_see_theo_b");
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
                    level.OnEndOfFrame += () =>
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
                    new FadeWipe(level, wipeIn: false, () =>
                    {
                        level.OnEndOfFrame += () =>
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
                case "cs01_mod_ending":
                    base.Scene.Add(new global::Celeste.Cutscenes.Cs01ModEnding(player2));
                    break;
                case "cs02_chara_intro":
                case "cs02DZ_CHara_intro":
                {
                    var chara = level.Entities.FindFirst<global::Celeste.Entities.CharaChaser>();
                    if (chara != null)
                        base.Scene.Add(new global::Celeste.Cutscenes.CS02DZ_CHaraIntro(chara));
                    break;
                }
                case "cs02_dreaming_phonecall_portal":
                {
                    var payphone = level.Entities.FindFirst<global::Celeste.Entities.Payphone>();
                    if (payphone != null)
                        base.Scene.Add(new global::Celeste.Cutscenes.Cs02DreamingPhonecallPortal(player2));
                    break;
                }
                case "cs02_awake_phonecall_ending":
                {
                    var payphone = level.Entities.FindFirst<global::Celeste.Entities.Payphone>();
                    if (payphone != null)
                        base.Scene.Add(new global::Celeste.Cutscenes.Cs02CallKirby(player2));
                    break;
                }
                case "cs03_first_step":
                {
                    var kPlayer = GetKPlayer(level);
                    var actualPlayer = kPlayer != null ? Unsafe.As<global::Celeste.Entities.K_Player, global::Celeste.Player>(ref kPlayer) : player2;
                    base.Scene.Add(new global::Celeste.Cutscenes.Cs03FirstStep(actualPlayer));
                    break;
                }
                case "cs03_meetup":
                {
                    var DZ = level.Entities.FindFirst<global::Celeste.NPCs.Npc03DZ>();
                    if (DZ != null)
                    {
                        var zoomCoroutine = new Coroutine(level.ZoomTo(DZ.Position + new Vector2(0f, -16f), 1.5f, 2f));
                        int conv = 0;
                        if (global::Celeste.SaveData.Instance?.HasFlag("WassupMagolor") == true &&
                            global::Celeste.SaveData.Instance?.HasFlag("BadelineJoinKirby") == true)
                        {
                            if (!level.Session.GetFlag("DZ_03_Meetup_conv1")) conv = 1;
                            else if (!level.Session.GetFlag("DZ_03_Meetup_conv2")) conv = 2;
                            else if (!level.Session.GetFlag("DZ_03_Meetup_conv3")) conv = 3;
                            else if (!level.Session.GetFlag("DZ_03_Meetup_conv4")) conv = 4;
                        }
                        var kPlayer = GetKPlayer(level);
                        var actualPlayer = kPlayer != null ? Unsafe.As<global::Celeste.Entities.K_Player, global::Celeste.Player>(ref kPlayer) : player2;
                        base.Scene.Add(new global::Celeste.Cutscenes.Cs03Meetup(DZ, actualPlayer, zoomCoroutine, conv));
                        
                        // Mark the conversation as completed
                        if (conv >= 1 && conv <= 4)
                        {
                            level.Session.SetFlag($"DZ_03_Meetup_conv{conv}");
                        }
                    }
                    break;
                }
                case "cs03_mod_ending":
                {
                    var kPlayer = GetKPlayer(level);
                    var actualPlayer = kPlayer != null ? Unsafe.As<global::Celeste.Entities.K_Player, global::Celeste.Player>(ref kPlayer) : player2;
                    base.Scene.Add(new global::Celeste.Cutscenes.Cs03ModEnding(actualPlayer));
                    break;
                }
                case "cs07_darker":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS07_Darker(player2));
                    break;
                case "cs07_genocide_vision_finale":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS07_GenocideVisionFinale(player2));
                    break;
                case "cs07_genocide_vision_intro":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS07_GenocideVisionIntro(player2));
                    break;
                case "cs07_genocide_wakeup":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS07_GenocideWakeup(player2));
                    break;
                case "cs08_charaboss_intro":
                    base.Scene.Add(new global::Celeste.Cutscenes.Cs08CharaBossIntro(player2));
                    break;
                case "cs09_area_complete":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS09_AreaComplete(player2));
                    break;
                case "cs09_credits":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS09_Credits(player2));
                    break;
                case "cs09_golden_flower":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS09_GoldenFlower(player2));
                    break;
                case "cs09_message_end":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS09_MessageEnd(player2));
                    break;
                case "ch9_arrivial":
                    base.Scene.Add(new DZ.CS09_Arrivial(player2));
                    break;
                case "ch10_flowey_intro":
                    base.Scene.Add(new global::Celeste.Cutscenes.FloweyIntroScene(player2));
                    break;
                case "ch15_zantas_1":
                    base.Scene.Add(new global::Celeste.Cutscenes.Cs15Zantas1(player2));
                    break;
                case "ch15_zantas_2":
                    base.Scene.Add(new global::Celeste.Cutscenes.Cs15Zantas2(player2));
                    break;
                case "cs12_titan_boss_intro":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS12_TitanBossIntro(player2));
                    break;
                case "cs12_titan_boss_outro":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS12_TitanBossOutro(player2));
                    break;
                case "ch13_tenna_pre_intro_video":
                    Engine.Scene = new global::Celeste.Cutscenes.Cs13TennaVideoVignette(level.Session, "DZ_CH13_TENNA_PRE_INTRO", "vignettes/ch13_tenna_pre_intro");
                    break;
                case "ch13_tenna_earth_video":
                    Engine.Scene = new global::Celeste.Cutscenes.Cs13TennaVideoVignette(level.Session, "DZ_CH13_TENNA_EARTH_VIDEO_destructions", "vignettes/ch13_tenna_earth_video");
                    break;
                case "ch13_tape_00":
                    Engine.Scene = new global::Celeste.Cutscenes.Cs13TapeVignette(level.Session, global::Celeste.Cutscenes.Cs13TapeVignette.TapeKeys.Tape00);
                    break;
                case "ch13_tape_01":
                    Engine.Scene = new global::Celeste.Cutscenes.Cs13TapeVignette(level.Session, global::Celeste.Cutscenes.Cs13TapeVignette.TapeKeys.Tape01);
                    break;
                case "ch13_tape_02":
                    Engine.Scene = new global::Celeste.Cutscenes.Cs13TapeVignette(level.Session, global::Celeste.Cutscenes.Cs13TapeVignette.TapeKeys.Tape02);
                    break;
                case "ch13_tape_03":
                    Engine.Scene = new global::Celeste.Cutscenes.Cs13TapeVignette(level.Session, global::Celeste.Cutscenes.Cs13TapeVignette.TapeKeys.Tape03);
                    break;
                case "ch13_tape_04":
                    Engine.Scene = new global::Celeste.Cutscenes.Cs13TapeVignette(level.Session, global::Celeste.Cutscenes.Cs13TapeVignette.TapeKeys.Tape04);
                    break;
                case "ch13_tape_05":
                    Engine.Scene = new global::Celeste.Cutscenes.Cs13TapeVignette(level.Session, global::Celeste.Cutscenes.Cs13TapeVignette.TapeKeys.Tape05);
                    break;
                case "ch13_tape_final":
                    Engine.Scene = new global::Celeste.Cutscenes.Cs13TapeVignette(level.Session, global::Celeste.Cutscenes.Cs13TapeVignette.TapeKeys.TapeFinal);
                    break;
                case "ch13_tape_vignette":
                    Engine.Scene = new global::Celeste.Cutscenes.Cs13TapeVignette(level.Session);
                    break;
                case "payphone_eat":
                    base.Scene.Add(new global::Celeste.Cutscenes.Cs02DreamingPhonecallPortal(player2));
                    break;
                case "cs15_titan_king_boss":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS15_TitanKingBoss(player2));
                    break;
                case "cs16_barrier_breaks":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS16_BarrierBreaks(player2));
                    break;
                case "cs16_corrupted_reality_intro":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS16_CorruptedRealityIntro(player2));
                    break;
                case "cs16_els_finale":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS16_ElsFinale(player2));
                    break;
                case "cs16_els_intro":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS16_ElsIntro(player2));
                    break;
                case "cs16_els_outro":
                    Engine.Scene = new global::Celeste.Cutscenes.CS16_ElsOutro(level.Session);
                    break;
                case "cs16_lost_souls_unite":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS16_LostSoulsUnite(player2));
                    break;
                case "cs16_save_file_battle":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS16_SaveFileBattle(player2));
                    break;
                case "ch18_outro":
                    Engine.Scene = new global::Celeste.Cutscenes.CS18_Outro(level.Session);
                    break;
                case "cs19_another_dimension_intro":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS19_AnotherDimensionIntro(player2));
                    break;
                case "cs19_gravestone":
                    base.Scene.Add(new CS19_Gravestone(player2, null, Vector2.Zero));
                    break;
                case "cs19_beyond_the_void":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS19_BeyondTheVoid(player2));
                    break;
                case "cs19DZ_CHara_helps":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS19DZ_CHaraHelps(player2));
                    break;
                case "cs19_edge_of_universe":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS19_EdgeOfUniverse(player2));
                    break;
                case "cs19_hub_second_intro":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS19_HubSecondIntro(player2));
                    break;
                case "cs19_trapin_loop":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS19_TrapinLoop(player2));
                    break;
                case "cs21_els_termina_intro":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS21_ElsTerminaBoss(player2, false));
                    break;
                case "cs21_els_termina_end":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS21_ElsTerminaBoss(player2, true));
                    break;
                case "cs21_cast":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS21_Cast(player2));
                    break;
                case "cs21_epilogue_credits":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS21_EpilogueCredits(player2));
                    break;
                case "cs21_fake_the_end":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS21_FakeTheEnd(player2));
                    break;
                case "cs21_final_cutscenes":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS21_FinalCutscenes(player2));
                    break;
                case "cs21_final_titan_summit":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS21_FinalTitanSummit(player2));
                    break;
                case "cs21_special_thanks_dodge_credits":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS21_SpecialThanksDodgeCredits(player2));
                    break;
                case "cs21_two_worlds_unite":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS21_TwoWorldsUnite(player2));
                    break;
                case "cs21_saved":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS21_Saved(player2));
                    break;
                case "cs21_farewell":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS21_RestorationAndFarewell(player2));
                    break;
                case "cs21_ending":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS21_Ending(player2));
                    break;
                case "ch8_chara_boss_center":
                case "ch8DZ_CHara_boss_center":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS08_BossCenter());
                    break;
                case "ch8_end":
                    base.Scene.Add(new global::Celeste.Cutscenes.Cs08End(player2));
                    break;
                case "ch20_asriel_true_reveal":
                {
                    var asrielBoss = level.Entities.FindFirst<AsrielGodBoss>();
                    if (asrielBoss != null)
                    {
                        level.Add(new CS20_AsrielRevealIdentity(player2, asrielBoss));
                    }
                    break;
                }
                case "ch20_end_later":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS20_Later(player2));
                    break;
                case "ch20_end_cinematic":
                    base.Scene.Add(new global::Celeste.Cutscenes.CS21_Ending(player2));
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

        private void OnCutsceneEnd(Level level)
        {
            Player player = level.Tracker.GetEntity<Player>();
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal;
            }
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
                float fadeAlphaMultiplier = Calc.ClampedMap(player.Y, start, end, 0f, 1f);
                foreach (Backdrop item in level.Background.GetEach<Backdrop>("bright"))
                {
                    item.ForceVisible = true;
                    item.FadeAlphaMultiplier = fadeAlphaMultiplier;
                }
                yield return null;
            }
        }

    }
}

