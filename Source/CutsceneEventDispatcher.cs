using System;
using System.Collections;
using System.Collections.Generic;
using MaggyHelper.Cutscenes;
using MaggyHelper.Entities;
using MaggyHelper.NPCs;
using Microsoft.Xna.Framework;
using Monocle;
using FlingBirdIntroMod = MaggyHelper.Entities.FlingBirdIntro;
using NPC = MaggyHelper.NPCs.NPC;

namespace MaggyHelper.Triggers;
internal static class CutsceneEventDispatcher
{
    internal delegate bool CutsceneRunner(string flag, Func<CutsceneEntity> factory);
    internal delegate bool ActionRunner(string flag, Func<bool> action);

    private sealed class CutsceneRegistration
    {
        public string Flag { get; }
        public Func<DispatchContext, CutsceneEntity> Factory { get; }

        public CutsceneRegistration(string flag, Func<DispatchContext, CutsceneEntity> factory)
        {
            Flag = flag;
            Factory = factory;
        }
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

    private static readonly IReadOnlyDictionary<string, CutsceneRegistration> CutsceneEvents = CreateCutsceneEvents();
    private static readonly IReadOnlyDictionary<string, Func<DispatchContext, bool>> CustomEventHandlers = CreateCustomEventHandlers();

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

        if (CutsceneEvents.TryGetValue(eventName, out var cutsceneRegistration))
        {
            return context.Trigger(cutsceneRegistration.Flag, () => cutsceneRegistration.Factory(context));
        }

        if (CustomEventHandlers.TryGetValue(eventName, out var customHandler))
        {
            return customHandler(context);
        }

        return context.Trigger($"generic_{eventName}_trigger", () => new GenericCutscene(player, eventName));
    }

    private static Dictionary<string, CutsceneRegistration> CreateCutsceneEvents()
    {
        var handlers = new Dictionary<string, CutsceneRegistration>(StringComparer.Ordinal);

        AddCutscene(handlers, "ch0_theo", "ch0_theo_trigger", ctx => new Cs00Theo(ctx.Player));
        AddCutscene(handlers, "ch0_ending", "ch0_ending_trigger", ctx => new CS00_EndingMod(ctx.Player));

        AddCutscene(handlers, new[] { "mod_city_end_1", "ch1_mod_city_end" }, "ch1_mod_end_trigger", ctx => new Cs01ModEnding(ctx.Player));

        AddCutscene(handlers, "ch2_chara_mirror", "ch2_chara_mirror_trigger", ctx => {
            var charaMirror = FindEntity<CharaMirror>(ctx, "CharaMirror");
            return charaMirror != null ? new CS02_CharaMirror(ctx.Player, charaMirror) : null;
        });
        AddCutscene(handlers, new[] { "chara_trap", "ch2_chara_trap" }, "chara_trap_trigger", ctx => new Cs02CharaTrap(ctx.Player));
        AddCutscene(handlers, new[] { "call_kirby", "ch2_call_kirby" }, "call_kirby_trigger", ctx => new Cs02CallKirby(ctx.Player));
        AddCutscene(handlers, "ch2_journal", "ch2_journal_trigger", ctx => new Cs02JournalMod(ctx.Player));

        AddCutscene(handlers, "ch3_meetup", "ch3_meetup_trigger", ctx => {
            var magolor = FindEntity<Npc03Maggy>(ctx, "Npc03Maggy");
            return magolor != null ? new Cs03Meetup(magolor, ctx.Player, null, 0) : null;
        });
        AddCutscene(handlers, "ch3_first_step", "ch3_first_step_trigger", ctx => new Cs03FirstStep(ctx.Player));
        AddCutscene(handlers, new[] { "mod_city_end_2", "ch3_mod_city_end" }, "ch3_2nd_mod_end_trigger", ctx => new Cs03ModEnding(ctx.Player));

        AddCutscene(handlers, "ch4_escape", "ch4_escape_trigger", ctx => new Cs04Escape(ctx.Player));
        AddCutscene(handlers, "ch4_call_mom", "ch4_call_mom_trigger", ctx => new Cs04CallMom(ctx.Player));
        AddCutscene(handlers, "ch4_mirror", "ch4_mirror_trigger", ctx => {
            var dreamMirror = ctx.Level.Entities.FindFirst<DreamMirror>();
            if (dreamMirror != null)
            {
                return new Cs04Mirror(ctx.Player, dreamMirror);
            }

            var ralseiMirror = ctx.Level.Entities.FindFirst<RalseiMirror>();
            if (ralseiMirror != null)
            {
                return new Cs04Mirror(ctx.Player, ralseiMirror);
            }

            Logger.Log(LogLevel.Warn, nameof(CutsceneEventDispatcher), $"{ctx.EventName}: no mirror entity found");
            return null;
        });
        AddCutscene(handlers, "ch4_chara_warning", "ch4_chara_warning_trigger", ctx => {
            var charaChaser = FindEntity<CharaChaser2>(ctx, "CharaChaser2");
            return charaChaser != null ? new CS04_CharaWarning(charaChaser) : null;
        });
        AddCutscene(handlers, "ch4_ending", "ch4_ending_trigger", ctx => new Cs04Ending(ctx.Player));

        AddCutscene(handlers, "ch5_see_maddy", "ch5_see_maddy_trigger", ctx => new Cs05SeeMaddy(ctx.Player, 0));
        AddCutscene(handlers, "ch5_diary", "ch5_diary_trigger", ctx => new Cs05Diary(false, false, ctx.Player));
        AddCutscene(handlers, "ch5_guestbook", "ch5_guestbook_trigger", ctx => new Cs05Guestbook(ctx.Player));
        AddCutscene(handlers, "ch5_memo", "ch5_memo_trigger", ctx => new Cs05Memo(ctx.Player));
        AddCutscene(handlers, "ch5_maddy_phone", "ch5_maddy_phone_trigger", ctx => new Cs05MaddyPhone(ctx.Player, 0f));
        AddCutscene(handlers, "ch5_ending", "ch5_ending_trigger", ctx => {
            var ending = FindEntity<global::MaggyHelper.Entities.ResortRoofEnding>(ctx, "ResortRoofEnding");
            return ending != null ? new CS05_Ending(ending, ctx.Player) : null;
        });
        AddCutscene(handlers, "ch5_magorlor_escape", "ch5_magolor_escape_trigger", ctx => {
            var magolor = FindEntity<NPC05_Magolor_Escaping>(ctx, "NPC05_Magolor_Escaping");
            return magolor != null ? new CS05_MagolorEscape(magolor, ctx.Player) : null;
        });
        AddCutscene(handlers, "ch5_oshiro_lobby", "ch5_oshiro_lobby_trigger", ctx => {
            var oshiro = FindEntity<NPC>(ctx, "NPC");
            return oshiro != null ? new CS05_OshiroLobby(ctx.Player, oshiro) : null;
        });
        AddCutscene(handlers, "ch5_oshiro_clutter", "ch5_oshiro_clutter_trigger", ctx => {
            var oshiro = FindEntity<NPC05_Oshiro_Cluttter>(ctx, "NPC05_Oshiro_Cluttter");
            return oshiro != null ? new CS05_OshiroClutter(ctx.Player, oshiro, 0) : null;
        });

        AddCutscene(handlers, "ch6_intro", "ch6_intro_trigger", ctx => {
            var gondola = FindEntity<GondolaMaggy>(ctx, "GondolaMaggy");
            var theo = FindEntity<NPC>(ctx, "NPC");
            return gondola != null && theo != null ? new CS06_Gondola(theo, gondola, ctx.Player) : null;
        });
        AddCutscene(handlers, "ch6_stronghold", "ch6_stronghold_trigger", ctx => new Cs06Stronghold(ctx.Player));
        AddCutscene(handlers, "ch6_end", "ch6_end_trigger", ctx => new Cs06End(ctx.Player));

        AddCutscene(handlers, "ch7_enter", "ch7_enter_trigger", ctx => new Cs07Enter(ctx.Player));
        AddCutscene(handlers, "ch7_intro", "ch7_intro_trigger", ctx => new Cs07Intro(ctx.Player));
        AddCutscene(handlers, "ch7_see_maddy_mirror", "ch7_see_maddy_mirror_trigger", ctx => new Cs07MaddyMirror(ctx.Player));
        AddCutscene(handlers, "ch7_pre_ingeste", "ch7_pre_ingeste_trigger", ctx => new Cs07PreIngeste(ctx.Player));
        AddCutscene(handlers, "ch7_see_maddy", "ch7_see_maddy_trigger", ctx => new Cs05SeeMaddy(ctx.Player, 0));
        AddCutscene(handlers, "ch7_found_maddy", "found_maddy_trigger", ctx => new Cs07SaveMaddy(ctx.Player));
        AddCutscene(handlers, "ch7_genocide_wakeup", "ch7_genocide_wakeup_trigger", ctx => new CS07_GenocideWakeup(ctx.Player));
        AddCutscene(handlers, "ch7_darker", "ch7_darker_trigger", ctx => new CS07_Darker(ctx.Player));

        AddCutscene(handlers, "ch8_plat", "npc08_maddy_plat_trigger", ctx => {
            var madelineNpc = FindEntity<Npc08MadelinePlateau>(ctx, "Npc08MadelinePlateau");
            var madeline = FindEntity<CelesteNPC>(ctx, "Celeste.NPC");
            return madelineNpc != null && madeline != null ? new Cs08Campfire(madelineNpc, ctx.Player, madeline) : null;
        });
        AddCutscene(handlers, new[] { "ch8_charaboss_intro", "ch8_intro_chara_boss" }, "ch8_charaboss_intro", ctx => {
            var charaBoss = FindEntity<CharaBoss>(ctx, "CharaBoss");
            return charaBoss != null ? new Cs08CharaBossIntro(ctx.Player.X, ctx.Player, charaBoss) : null;
        });
        AddCutscene(handlers, "ch8_chara_boss_mid", "ch8_chara_boss_mid_trigger", ctx => new CS08_BossMid());
        AddCutscene(handlers, "ch8_chara_boss_center", "ch8_chara_boss_center_trigger", ctx => new Cs08CharaBossCenter(ctx.Player));
        AddCutscene(handlers, "ch8_chara_boss_end", "ch8_chara_boss_end_trigger", ctx => {
            var chara = FindEntity<Npc08CharaCrying>(ctx, "Npc08CharaCrying");
            return chara != null ? new Cs08CharaBossEnd(ctx.Player, chara) : null;
        });
        AddCutscene(handlers, "ch8_end", "ch8_end_trigger", ctx => {
            var theoAndMaddy = FindEntity<Npc08MaddyAndTheoEnding>(ctx, "Npc08MaddyAndTheoEnding");
            var maggy = FindEntity<Npc08MaggyEnding>(ctx, "Npc08MaggyEnding");
            return theoAndMaddy != null && maggy != null ? new Cs08End(ctx.Player, theoAndMaddy, maggy) : null;
        });
        AddCutscene(handlers, "ch8_theo", "ch8_theo_trigger", ctx => new Cs08Theo(ctx.Player));
        AddCutscene(handlers, "ch8_reflectionmod", "ch8_reflectionmod_trigger", ctx => new Cs08Reflection(ctx.Player, targetX: 0f));
        AddCutscene(handlers, "ch8_star_jump_end", "ch8_star_jump_end_trigger", ctx => {
            var npc = FindEntity<CelesteNPC>(ctx, "Celeste.NPC");
            return npc != null ? new CS08_StarJumpEnd(npc, ctx.Player, ctx.Player.Position, ctx.Level.Camera.Position) : null;
        });

        AddCutscene(handlers, "ch9_arrivial", "ch9_arrival_trigger", ctx => new CS09_Ascend(0, "ch9_arrivial", false));
        AddCutscene(handlers, "ch9_fake_saved", "ch9_fake_saved_trigger", ctx => new CS09_FakeSavePoint(ctx.Player, CS09_FakeSavePoint.GetCurrentStage(ctx.Level)));
        AddCutscene(handlers, "ch9_credits", "ch9_credits_trigger", ctx => new CS09_Credits(ctx.Player));
        AddCutscene(handlers, "ch9_message_end", "ch9_message_end_trigger", ctx => new CS09_MessageEnd(ctx.Player));
        AddCutscene(handlers, "ch9_end", "ch9_end_trigger", ctx => new Cs09End(ctx.Player));

        AddCutscene(handlers, "CH10_flowey_intro", "ch10_flowey_intro_trigger", ctx => new Cs10FloweyIntro(ctx.Player));
        AddCutscene(handlers, "CH10_flowey_intro_scene", "ch10_flowey_intro_complete", ctx => new FloweyIntroScene(ctx.Player));
        AddCutscene(handlers, "ch10_house", "ch10_house_trigger", ctx => new Cs10House(ctx.Player));
        AddCutscene(handlers, "ch10_house_indoor", "ch10_house_indoor_trigger", ctx => new Cs10HouseIndoor(ctx.Player));
        AddCutscene(handlers, "ch10_house_outdoor", "ch10_house_outdoor_trigger", ctx => new Cs10HouseOutdoor(ctx.Player));
        AddCutscene(handlers, "ch10_pre_boss", "ch10_pre_boss_trigger", ctx => new Cs10PreBoss(ctx.Player));
        AddCutscene(handlers, "ch10_post_boss", "ch10_post_boss_trigger", ctx => new Cs10PostBoss(ctx.Player));
        AddCutscene(handlers, "ch10_piano_start", "ch10_piano_start_trigger", ctx => new CS10_PianoStart(ctx.Player));
        AddCutscene(handlers, "ch10_roxus_start", "ch10_roxus_start_trigger", ctx => new CS10_RoxusStart(ctx.Player));
        AddCutscene(handlers, "ch10_sans_restaurant", "ch10_sans_restaurant_trigger", ctx => new CS10_SansRestaurant(ctx.Player));
        AddCutscene(handlers, "ch10_titan_tower_approach", "ch10_titan_tower_approach_trigger", ctx => new CS10_TitanTowerApproach(ctx.Player));
        AddCutscene(handlers, "ch10_titan_boss_battle", "ch10_titan_boss_battle_trigger", ctx => new CS10_TitanBossBattle(ctx.Player));
        AddCutscene(handlers, "ch10_maddy_baddy_chara_intro", "ch10_maddy_baddy_chara_intro_trigger", ctx => new CS10_MaddyBaddyCharaIntro(ctx.Player, ctx.Level.Session.Level));

        AddCutscene(handlers, "ch11_intro", "ch11_intro_trigger", ctx => new CS11_Intro_Marlet(ctx.Player));
        AddCutscene(handlers, "ch11_town", "ch11_town_trigger", ctx => new Cs11Town(ctx.Player));
        AddCutscene(handlers, "ch11_marlet_pre_boss", "ch11_marlet_pre_boss_trigger", ctx => new Cs11MarletPreBoss(ctx.Player));
        AddCutscene(handlers, "ch11_marlet_boss_end", "ch11_marlet_boss_end_trigger", ctx => new Cs11MarletBossEnd(ctx.Player));
        AddCutscene(handlers, "ch11_bar_arrival", "ch11_bar_arrival_trigger", ctx => new CS11_BarArrival(ctx.Player));
        AddCutscene(handlers, "ch11_cowboy_bar_intro", "ch11_cowboy_bar_intro_trigger", ctx => new CS11_CowboyBarIntro(ctx.Player));
        AddCutscene(handlers, "ch11_cinematic_bar", "ch11_cinematic_bar_trigger", ctx => new CS11_CinematicBar(ctx.Player));
        AddCutscene(handlers, "ch11_maggy", "ch11_maggy_trigger", ctx => new CS11_Maggy(ctx.Player));
        AddCutscene(handlers, "ch11_maggy_end", "ch11_maggy_end_trigger", ctx => new CS11_MaggyEnd(ctx.Player));
        AddCutscene(handlers, "ch11_collecting_mini_heart_enough", "ch11_collecting_mini_heart_enough_trigger", ctx => new CS11_CollectingMiniHeartEnough(ctx.Player));
        AddCutscene(handlers, "ch11_boss_intro", "ch11_boss_intro_trigger", ctx => new CS11_BossIntro(ctx.Player));
        AddCutscene(handlers, "ch11_boss_mid", "ch11_boss_mid_trigger", ctx => new CS11_BossMid(ctx.Player));
        AddCutscene(handlers, "ch11_boss_outro", "ch11_boss_outro_trigger", ctx => new CS11_BossOutro(ctx.Player));
        AddCutscene(handlers, "ch11_starlo_and_marlet", "ch11_starlo_and_marlet_trigger", ctx => new CS11_StarloAndMarlet(ctx.Player));

        AddCutscene(handlers, "ch12_intro", "ch12_intro_trigger", ctx => new Cs12Intro(ctx.Player));
        AddCutscene(handlers, "ch12_titan_tower", "ch12_titan_tower_trigger", ctx => new Cs12TitanTower(ctx.Player));
        AddCutscene(handlers, "ch12_titan_pre_boss", "ch12_titan_pre_boss_trigger", ctx => new Cs12TitanPreBoss(ctx.Player));
        AddCutscene(handlers, "ch12_undyne_refused_to_died", "ch12_undyne_refused_to_died_trigger", ctx => new Cs12UndyneRefusedToDied(ctx.Player));
        AddCutscene(handlers, "ch12_titan_post_boss", "ch12_titan_post_boss_trigger", ctx => new Cs12TitanPostBoss(ctx.Player));
        AddCutscene(handlers, "ch12_end", "ch12_end_trigger", ctx => new Cs12End(ctx.Player));

        AddCutscene(handlers, "ch13_hot_lava", "ch13_hot_lava_trigger", ctx => new Cs13HotLava(ctx.Player));
        AddCutscene(handlers, "ch13_axis_intro", "ch13_axis_intro_trigger", ctx => new Cs13AxisIntro(ctx.Player));
        AddCutscene(handlers, "ch13_well_prepared", "ch13_well_prepared_trigger", ctx => new Cs13WellPrepared(ctx.Player));
        AddCutscene(handlers, "ch13_axis_pre_boss", "ch13_axis_pre_boss_trigger", ctx => new Cs13AxisPreBoss(ctx.Player));
        AddCutscene(handlers, "ch13_axis_post_boss", "ch13_axis_post_boss_trigger", ctx => new Cs13AxisPostBoss(ctx.Player));
        AddCutscene(handlers, "ch13_end", "ch13_end_trigger", ctx => new Cs13End(ctx.Player));
        AddCutscene(handlers, "ch13_intro", "ch13_intro_trigger", ctx => new CS13_Intro(ctx.Player));
        AddCutscene(handlers, "ch13_meta_knight_encounter", "ch13_meta_knight_encounter_trigger", ctx => new CS13_MetaKnightEncounter(ctx.Player));
        AddCutscene(handlers, "ch13_axis_boss_battle", "ch13_axis_boss_battle_trigger", ctx => new CS13_AxisBossBattle(ctx.Player));

        AddCutscene(handlers, "ch14_intro_core", "ch14_intro_core_trigger", ctx => new Cs14IntroCore(ctx.Player));
        AddCutscene(handlers, "ch14_giga_axis_pre_boss", "ch14_giga_axis_pre_boss_trigger", ctx => new Cs14GigaAxisPreBoss(ctx.Player));
        AddCutscene(handlers, "ch14_giga_axis_post_boss", "ch14_giga_axis_post_boss_trigger", ctx => new Cs14GigaAxisPostBoss(ctx.Player));
        AddCutscene(handlers, "ch14_enter_last_elevator", "ch14_enter_last_elevator_trigger", ctx => new Cs14EnterLastElevator(ctx.Player));
        AddCutscene(handlers, "ch14_intro", "ch14_intro_trigger", ctx => new CS14_Intro(ctx.Player));
        AddCutscene(handlers, "ch14_hollow_programmer", "ch14_hollow_programmer_trigger", ctx => new CS14_HollowProgrammer(ctx.Player));
        AddCutscene(handlers, "ch14_giant_axis_battle", "ch14_giant_axis_battle_trigger", ctx => new CS14_GiantAxisBattle(ctx.Player));

        AddCutscene(handlers, "ch15_exit_last_elevator", "ch15_exit_last_elevator_trigger", ctx => new Cs15ExitLastElevator(ctx.Player));
        AddCutscene(handlers, "ch15_zantas_1", "ch15_zantas_1_trigger", ctx => new Cs15Zantas1(ctx.Player));
        AddCutscene(handlers, "ch15_zantas_2", "ch15_zantas_2_trigger", ctx => new Cs15Zantas2(ctx.Player));
        AddCutscene(handlers, "ch15_judgement", "ch15_judgement_trigger", ctx => new Cs15Judgement(ctx.Player));
        AddCutscene(handlers, "ch15_intro_roaring_titan", "ch15_intro_roaring_titan_trigger", ctx => new Cs15IntroRoaringTitan(ctx.Player));
        AddCutscene(handlers, "ch15_barrier", "ch15_barrier_trigger", ctx => new Cs15Barrier(ctx.Player));
        AddCutscene(handlers, "ch15_roaring_titan_pre_boss", "ch15_roaring_titan_pre_boss_trigger", ctx => new Cs15RoaringTitanPreBoss(ctx.Player));
        AddCutscene(handlers, "ch15_roaring_titan_post_boss", "ch15_roaring_titan_post_boss_trigger", ctx => new Cs15RoaringTitanPostBoss(ctx.Player));
        AddCutscene(handlers, "ch15_flowey_transformation", "ch15_flowey_transformation_trigger", ctx => new CS15_FloweyTransformation(ctx.Player));
        AddCutscene(handlers, "ch15_mountain_peak_arrival", "ch15_mountain_peak_arrival_trigger", ctx => new CS15_MountainPeakArrival(ctx.Player));
        AddCutscene(handlers, "ch15_roaring_titan_king_battle", "ch15_roaring_titan_king_battle_trigger", ctx => new CS15_RoaringTitanKingBattle(ctx.Player));
        AddCutscene(handlers, "ch15_titan_council_judgment", "ch15_titan_council_judgment_trigger", ctx => new CS15_TitanCouncilJudgment(ctx.Player));

        AddCutscene(handlers, "ch16_els_intro", "ch16_els_intro_trigger", ctx => new CS16_ElsIntro(ctx.Player));
        AddCutscene(handlers, "ch16_els_finale", "ch16_els_finale_trigger", ctx => new CS16_ElsFinale(ctx.Player));
        AddCutscene(handlers, "ch16_els_outro", "ch16_els_outro_trigger", ctx => new CS16_ElsOutro(ctx.Player));
        AddCutscene(handlers, "ch16_exited", "ch16_exited_trigger", ctx => new Cs16Exited(ctx.Player));
        AddCutscene(handlers, "ch16_end", "ch16_end_trigger", ctx => new Cs16End(ctx.Player));
        AddCutscene(handlers, "ch16_barrier_breaks", "ch16_barrier_breaks_trigger", ctx => new CS16_BarrierBreaks(ctx.Player));
        AddCutscene(handlers, "ch16_corrupted_reality_intro", "ch16_corrupted_reality_intro_trigger", ctx => new CS16_CorruptedRealityIntro(ctx.Player));
        AddCutscene(handlers, "ch16_lost_souls_unite", "ch16_lost_souls_unite_trigger", ctx => new CS16_LostSoulsUnite(ctx.Player));
        AddCutscene(handlers, "ch16_save_file_battle", "ch16_save_file_battle_trigger", ctx => new CS16_SaveFileBattle(ctx.Player));

        AddCutscene(handlers, "ch17_welcomehome", "ch17_welcomehome_trigger", ctx => new CS17_WelcomeHome(ctx.Player, targetX: 0f));
        AddCutscene(handlers, "ch17_epilouge", "ch17_epilogue_trigger", ctx => new Cs17Epilogue(ctx.Player));
        AddCutscene(handlers, "ch17_credits", "ch17_credits_trigger", ctx => new CS17_Credits());
        AddCutscene(handlers, "ch17_ending_mod", "ch17_ending_mod_trigger", ctx => new CS17_EndingMod());

        AddCutscene(handlers, "ch18_outro", "ch18_outro_trigger", ctx => new CS18_Outro(ctx.Player));

        AddCutscene(handlers, "ch19_another_dimension_intro", "ch19_another_dimension_intro_trigger", ctx => new Cs19AnotherDimensionIntro(ctx.Player));
        AddCutscene(handlers, "ch19_big_final_room", "ch19_big_final_room_trigger", ctx => new Cs19BigFinalRoom(ctx.Player, first: true));
        AddCutscene(handlers, new[] { "ch19_hub_second_intro", "ch19_hub_second_intro_trigger" }, "ch19_hub_second_intro_trigger", ctx => new CS19_HubSecondIntro(ctx.Level, ctx.Player));
        AddCutscene(handlers, "ch19_loop", "ch19_loop_trigger", ctx => {
            var charaDummy = FindEntity<CharaDummy>(ctx, "CharaDummy");
            return charaDummy != null ? new Cs19TrapinLoop(ctx.Player, charaDummy) : null;
        });
        AddCutscene(handlers, "ch19_chara_help", "ch19_chara_help_trigger", ctx => new CS19_CharaHelps(ctx.Player));
        AddCutscene(handlers, "ch19_free_bird", "ch19_free_bird_trigger", ctx => new CS19_FreeBird());
        AddCutscene(handlers, "ch19_kill_the_bird", "ch19_kill_the_bird_trigger", ctx => {
            var bird = FindEntity<FlingBirdIntroMod>(ctx, "FlingBirdIntro");
            return bird != null ? new CS19_KillTheBird(ctx.Player, bird) : null;
        });
        AddCutscene(handlers, "ch19_miss_the_bird", "ch19_miss_the_bird_trigger", ctx => {
            var bird = FindEntity<FlingBirdIntroMod>(ctx, "FlingBirdIntro");
            return bird != null ? new CS19_MissTheBird(ctx.Player, bird) : null;
        });
        AddCutscene(handlers, "ch19_gravestone", "ch19_gravestone_trigger", ctx => {
            var gravestone = FindEntity<NPC19_Gravestone>(ctx, "NPC19_Gravestone");
            return gravestone != null ? new CS19_Gravestone(ctx.Player, gravestone, Vector2.Zero) : null;
        });
        AddCutscene(handlers, "ch19_final_launch", "ch19_final_launch_trigger", ctx => {
            var boost = FindEntity<CustomCharaBoost>(ctx, "CustomCharaBoost");
            return boost != null ? new CS19_FinalLaunch(ctx.Player, boost, true) : null;
        });
        AddCutscene(handlers, "ch19_souls_discarded", "ch19_souls_discarded_trigger", ctx => new CS19_SoulsDiscarded(ctx.Player));
        AddCutscene(handlers, "ch19_beyond_the_void", "ch19_beyond_the_void_trigger", ctx => new CS19_BeyondTheVoid(ctx.Player));
        AddCutscene(handlers, "ch19_memories_of_the_past", "ch19_memories_of_the_past_trigger", ctx => new CS19_MemoriesOfThePast(ctx.Player));
        AddCutscene(handlers, "ch19_els_breaks_free", "ch19_els_breaks_free_trigger", ctx => new CS19_ElsBreaksFree(ctx.Player));
        AddCutscene(handlers, "ch19_broken_star_warrior", "ch19_broken_star_warrior_trigger", ctx => new CS19_BrokenStarWarrior(ctx.Player));
        AddCutscene(handlers, "ch19_edge_of_universe", "ch19_edge_of_universe_trigger", ctx => new CS19_EdgeOfUniverse(ctx.Player));

        AddCutscene(handlers, "ch20_bird_guidance_intro", "ch20_bird_guidance_intro_trigger", ctx => new Cs20BirdGuidanceIntro(ctx.Player));
        AddCutscene(handlers, "ch20_intro", "ch20_intro_trigger", ctx => new Cs20Intro(ctx.Player));
        AddCutscene(handlers, "ch20_fake_madeline_and_badeline", CS20_FakeMadelineAndBadeline.Flag, ctx => new CS20_FakeMadelineAndBadeline(ctx.Player));
        AddCutscene(handlers, "ch20_tess_fake_pre_boss", "ch20_tess_fake_pre_boss_trigger", ctx => new Cs20TessFakePreBoss(ctx.Player));
        AddCutscene(handlers, "ch20_tess_fake_post_boss", "ch20_tess_fake_post_boss_trigger", ctx => new Cs20TessFakePostBoss(ctx.Player));
        AddCutscene(handlers, "ch20_nothiness", "ch20_nothingness_trigger", ctx => new Cs20Nothingness(ctx.Player));
        AddCutscene(handlers, "ch20_tess_pre_boss_for_real", "ch20_tess_pre_boss_for_real_trigger", ctx => new Cs20TessPreBossForReal(ctx.Player));
        AddCutscene(handlers, "ch20_saved", "ch20_saved_trigger", ctx => new CS20_Saved(ctx.Player));
        AddCutscene(handlers, "ch20_ending", "ch20_true_end_trigger", ctx => new CS20_Ending(ctx.Player));
        AddCutscene(handlers, new[] { "ch20_asriel_god_boss_identity_reveal", "ch20_asriel_god_boss_identity_reveal_trigger" }, "ch20_asriel_god_boss_identity_reveal_trigger", ctx => {
            var asrielBoss = FindEntity<AsrielGodBoss>(ctx, "AsrielGodBoss");
            return asrielBoss != null ? new CS20_AsrielRevealIdentity(ctx.Player, asrielBoss) : null;
        });
        AddCutscene(handlers, "ch20_asriel_angel_of_death_boss_intro", "ch20_asriel_angel_of_death_boss_intro_trigger", ctx => new CS20_AsrielAngelOfDeathBossIntro(ctx.Level.Session.Level));
        AddCutscene(handlers, "ch20_boss_mid", "ch20_boss_mid_trigger", ctx => new CS20_BossMid());
        AddCutscene(handlers, "ch20_boss_end", "ch20_boss_end_trigger", ctx => new CS20_BossEnd());
        AddCutscene(handlers, "ch20_asriel_boss_end", CS20_AsrielBossEnd.Flag, ctx => new CS20_AsrielBossEnd(ctx.Player));
        AddCutscene(handlers, "ch20_final_boss_defeat", "ch20_final_boss_defeat_trigger", ctx => new CS20_FinalBossDefeat(ctx.Player));
        AddCutscene(handlers, "ch20_rainbow_blossom_tree", "ch20_rainbow_blossom_tree_trigger", ctx => new CS20_RainbowBlossomTree(ctx.Player));
        AddCutscene(handlers, "ch20_restoration_and_farewell", "ch20_restoration_and_farewell_trigger", ctx => new CS20_RestorationAndFarewell(ctx.Player));
        AddCutscene(handlers, "ch20_end_later", "ch20_end_later_trigger", ctx => new CS20_Later(ctx.Player));
        AddCutscene(handlers, "ch20_end_cinematic", "ch20_end_cinematic_trigger", ctx => new CS20_Ending(ctx.Player));
        AddCutscene(handlers, "ch20_white_cymbal_fade_teleport_video", "ch20_white_cymbal_fade_teleport_video_trigger", ctx => new CS20_WhiteCymbalFadeTeleportVideo(ctx.Player));

        AddCutscene(handlers, "ch21_beaches", "ch21_beaches_trigger", ctx => new Cs21Beaches(ctx.Player));
        AddCutscene(handlers, "ch21_special_thanks", "ch21_special_thanks_trigger", ctx => new Cs21SpecialThanks(ctx.Player));
        AddCutscene(handlers, "ch21_epilogue_credits", "ch21_epilogue_credits_trigger", ctx => new CS21_EpilogueCredits(ctx.Player));
        AddCutscene(handlers, "ch21_two_worlds_unite", "ch21_two_worlds_unite_trigger", ctx => new CS21_TwoWorldsUnite(ctx.Player));

        return handlers;
    }

    private static Dictionary<string, Func<DispatchContext, bool>> CreateCustomEventHandlers()
    {
        var handlers = new Dictionary<string, Func<DispatchContext, bool>>(StringComparer.Ordinal);

        AddHandler(handlers, "ch2_chara_intro", ctx => {
            var charaChaser = ctx.Level.Entities.FindFirst<CharaChaser>();
            if (charaChaser != null)
            {
                return ctx.Trigger("ch2_chara_intro_trigger", () => new CS02_CharaIntro(charaChaser));
            }

            Logger.Log(LogLevel.Warn, nameof(CutsceneEventDispatcher), "ch2_chara_intro: CharaChaser not found yet, deferring trigger");
            ctx.DeferCh2CharaIntro();
            return false;
        });

        AddHandler(handlers, "ch7_mirror_portal", ctx => {
            ctx.Level.Session.SetFlag(CH7GenocideMirrorState.EnabledFlag, false);
            return ctx.Trigger("ch7_mirror_portal_trigger", () => {
                var mirror = FindEntity<TesseractMirrorGateway>(ctx, "TesseractMirrorGateway");
                return mirror != null ? new CS07_MirrorPortal(ctx.Player, mirror) : null;
            });
        });

        AddHandler(handlers, "ch7_genocide_mirror_portal", ctx => {
            ctx.Level.Session.SetFlag(CH7GenocideMirrorState.EnabledFlag, true);
            return ctx.Trigger("ch7_genocide_mirror_portal_trigger", () => {
                var mirror = FindEntity<TesseractMirrorGateway>(ctx, "TesseractMirrorGateway");
                return mirror != null ? new CS07_MirrorPortal(ctx.Player, mirror) : null;
            });
        });

        AddHandler(handlers, "ch7_genocide_vision_intro", ctx => {
            if (ctx.Level.Session.GetFlag(CH7GenocideMirrorState.StartedFlag))
            {
                return ctx.Run(string.Empty, () => true);
            }

            return ctx.Trigger("ch7_genocide_vision_intro_trigger", () => new CS07_GenocideVisionIntro(ctx.Player));
        });

        AddHandler(handlers, "ch7_genocide_vision_finale", ctx => {
            if (ctx.Level.Session.GetFlag(CH7GenocideMirrorState.StartedFlag))
            {
                return ctx.Run(string.Empty, () => true);
            }

            return ctx.Trigger("ch7_genocide_vision_finale_trigger", () => new CS07_GenocideVisionFinale(ctx.Player));
        });

        AddHandler(handlers, "ch10_intro", ctx => ctx.Run("ch10_intro_trigger", () => {
            Engine.Scene = new Cs10IntroVignetteAlt(ctx.Level.Session);
            return true;
        }));

        AddHandler(handlers, "ch19_goto_the_future", ctx => RunFuturePastWarp(ctx, "intro-01-future-maggy"));
        AddHandler(handlers, "ch19_goto_the_past", ctx => RunFuturePastWarp(ctx, "intro-00-past-maggy"));

        return handlers;
    }

    private static bool RunFuturePastWarp(DispatchContext ctx, string nextLevel)
    {
        return ctx.Run(string.Empty, () => {
            ctx.Level.OnEndOfFrame += () => {
                Vector2 levelOffset = ctx.Level.LevelOffset;
                Vector2 playerOffset = ctx.Player.Position - ctx.Level.LevelOffset;
                Vector2 cameraOffset = ctx.Level.Camera.Position - ctx.Level.LevelOffset;
                Facings facing = ctx.Player.Facing;
                ctx.Level.Remove(ctx.Player);
                ctx.Level.UnloadLevel();
                ctx.Level.Session.Dreaming = true;
                ctx.Level.Session.Level = nextLevel;
                ctx.Level.Session.RespawnPoint = ctx.Level.GetSpawnPoint(new Vector2(ctx.Level.Bounds.Left, ctx.Level.Bounds.Top));
                ctx.Level.Session.FirstLevel = false;
                ctx.Level.LoadLevel(global::Celeste.Player.IntroTypes.Transition);
                ctx.Level.Camera.Position = ctx.Level.LevelOffset + cameraOffset;
                ctx.Level.Session.Inventory.Dashes = 1;
                ctx.Player.Dashes = Math.Min(ctx.Player.Dashes, 1);
                ctx.Level.Add(ctx.Player);
                ctx.Player.Position = ctx.Level.LevelOffset + playerOffset;
                ctx.Player.Facing = facing;
                ctx.Player.Hair.MoveHairBy(ctx.Level.LevelOffset - levelOffset);
                if (ctx.Level.Wipe != null)
                {
                    ctx.Level.Wipe.Cancel();
                }
                ctx.Level.Flash(Color.White);
                ctx.Level.Shake();
                ctx.Level.Add(new LightningStrike(new Vector2(ctx.Player.X + 60f, ctx.Level.Bounds.Bottom - 180), 10, 200f));
                ctx.Level.Add(new LightningStrike(new Vector2(ctx.Player.X + 220f, ctx.Level.Bounds.Bottom - 180), 40, 200f, 0.25f));
                Audio.Play("event:/new_content/game/10_farewell/lightning_strike");
            };

            return true;
        });
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

    private static void AddCutscene(
        IDictionary<string, CutsceneRegistration> handlers,
        string eventName,
        string flag,
        Func<DispatchContext, CutsceneEntity> factory)
    {
        handlers[eventName] = new CutsceneRegistration(flag, factory);
    }

    private static void AddCutscene(
        IDictionary<string, CutsceneRegistration> handlers,
        IEnumerable<string> eventNames,
        string flag,
        Func<DispatchContext, CutsceneEntity> factory)
    {
        foreach (string eventName in eventNames)
        {
            AddCutscene(handlers, eventName, flag, factory);
        }
    }

    private static void AddHandler(
        IDictionary<string, Func<DispatchContext, bool>> handlers,
        string eventName,
        Func<DispatchContext, bool> handler)
    {
        handlers[eventName] = handler;
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
            player.StateMachine.State = 11;
            yield return 0.5f;
            yield return Textbox.Say(eventName.ToUpperInvariant());
            yield return 0.5f;
            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            if (player != null)
            {
                player.StateMachine.State = 0;
            }
        }
    }
}
