using MaggyHelper.Cutscenes;
using MaggyHelper.Entities;
using MaggyHelper.NPCs;
// Type alias for FlingBirdIntroMod (same as FlingBirdIntro)
using FlingBirdIntroMod = MaggyHelper.Entities.FlingBirdIntro;
using NPC = MaggyHelper.NPCs.NPC;

namespace MaggyHelper.Triggers;

[CustomEntity(ids: "MaggyHelper/EventTrigger")]
[Tracked]
public class IngesteEventTrigger : Trigger
{
    private readonly string eventName;
    private bool hasTriggered = false;
    private bool pendingCh2CharaIntro;

    public IngesteEventTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        eventName = data.Attr("event", "");
        Logger.Log(LogLevel.Info, "IngesteEventTrigger", $"Created trigger for event: {eventName}");
    }

    internal static void Load()
    {
        On.Celeste.Level.LoadLevel += Level_LoadLevel;
        Logger.Log(LogLevel.Info, "IngesteEventTrigger", "Hooks registered");
    }

    internal static void Unload()
    {
        On.Celeste.Level.LoadLevel -= Level_LoadLevel;
        Logger.Log(LogLevel.Info, "IngesteEventTrigger", "Hooks unregistered");
    }

    private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, global::Celeste.Player.IntroTypes playerIntro, bool isFromLoader)
    {
        orig(self, playerIntro, isFromLoader);
        Logger.Log(LogLevel.Debug, "IngesteEventTrigger", "Level loaded, triggers should be active");
    }

    private void TriggerOnce(Level level, string flag, Func<CutsceneEntity> cutsceneFactory)
    {
        Logger.Log(LogLevel.Info, "IngesteEventTrigger", $"TriggerOnce called for flag: {flag}");
        
        if (level.Session.GetFlag(flag))
        {
            Logger.Log(LogLevel.Info, "IngesteEventTrigger", $"Flag {flag} already set, skipping trigger");
            return;
        }

        level.Session.SetFlag(flag);
        
        try
        {
            var cs = cutsceneFactory();
            if (cs != null)
            {
                Logger.Log(LogLevel.Info, "IngesteEventTrigger", $"Adding cutscene {cs.GetType().Name} to scene");
                Scene.Add(cs);
            }
            else
            {
                Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "Cutscene factory returned null");
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "IngesteEventTrigger", $"Error creating cutscene: {ex}");
        }
        
        RemoveSelf();
    }

    public override void OnEnter(global::Celeste.Player player)
    {
        base.OnEnter(player);
        
        if (hasTriggered)
        {
            Logger.Log(LogLevel.Debug, "IngesteEventTrigger", "Trigger already fired, ignoring");
            return;
        }

        Logger.Log(LogLevel.Info, "IngesteEventTrigger", $"Player entered trigger with event: {eventName}");
        
        if (Scene is not Level level)
        {
            Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "Scene is not a Level, cannot trigger cutscene");
            return;
        }

        hasTriggered = true;

        // Handle all event types
        ProcessEvent(level, player, eventName);
    }

    public override void Update()
    {
        base.Update();

        if (!pendingCh2CharaIntro || Scene is not Level level)
        {
            return;
        }

        // Stop retrying once the intro has already played or the trigger flag is set.
        if (level.Session.GetFlag("evil_chara_intro") || level.Session.GetFlag("ch2_chara_intro_trigger"))
        {
            pendingCh2CharaIntro = false;
            RemoveSelf();
            return;
        }

        var charaChaser = Scene.Entities.FindFirst<CharaChaser>();
        if (charaChaser == null)
        {
            return;
        }

        pendingCh2CharaIntro = false;
        TriggerOnce(level, "ch2_chara_intro_trigger", () => new CS02_CharaIntro(charaChaser));
    }

     private void ProcessEvent(Level level, global::Celeste.Player player, string eventName)
    {
        Logger.Log(LogLevel.Info, "IngesteEventTrigger", $"Processing event: {eventName}");

        switch (eventName)
        {
            // ==================== Chapter 0 ====================
            case "ch0_theo":
                TriggerOnce(level, "ch0_theo_trigger", () => new Cs00Theo(player));
                break;
                
            case "ch0_ending":
                TriggerOnce(level, "ch0_ending_trigger", () => new CS00_EndingMod(player));
                break;

            // ==================== Chapter 1 ====================
            case "mod_city_end_1":
            case "ch1_mod_city_end":
                TriggerOnce(level, "ch1_mod_end_trigger", () => new Cs01ModEnding(player));
                break;

            // ==================== Chapter 2 ====================
            case "ch2_chara_intro":
                // Requires CharaChaser entity in the scene
                var charaChaser = Scene.Entities.FindFirst<CharaChaser>();
                if (charaChaser != null)
                {
                    TriggerOnce(level, "ch2_chara_intro_trigger", () => new CS02_CharaIntro(charaChaser));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "ch2_chara_intro: CharaChaser not found yet, deferring trigger");
                    pendingCh2CharaIntro = true;
                    hasTriggered = false;
                }
                break;
                
            case "ch2_chara_mirror":
                // Requires CharaMirror entity in the scene
                var charaMirror = Scene.Entities.FindFirst<CharaMirror>();
                if (charaMirror != null)
                {
                    TriggerOnce(level, "ch2_chara_mirror_trigger", () => new CS02_CharaMirror(player, charaMirror));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "ch2_chara_mirror: CharaMirror not found");
                    RemoveSelf();
                }
                break;
                
            case "chara_trap":
            case "ch2_chara_trap":
                TriggerOnce(level, "chara_trap_trigger", () => new Cs02CharaTrap(player));
                break;
                
            case "call_kirby":
            case "ch2_call_kirby":
                TriggerOnce(level, "call_kirby_trigger", () => new Cs02CallKirby(player));
                break;
                
            case "ch2_journal":
                TriggerOnce(level, "ch2_journal_trigger", () => new Cs02JournalMod(player));
                break;

            // ==================== Chapter 3 ====================
            case "ch3_meetup":
                var magolor = Scene.Entities.FindFirst<Npc03Maggy>();
                TriggerOnce(level, "ch3_meetup_trigger", () => new Cs03Meetup(magolor, player, null, 0));
                break;
                
            case "ch3_first_step":
                TriggerOnce(level, "ch3_first_step_trigger", () => new Cs03FirstStep(player));
                break;
                
            case "mod_city_end_2":
            case "ch3_mod_city_end":
                TriggerOnce(level, "ch3_2nd_mod_end_trigger", () => new Cs03ModEnding(player));
                break;

            // ==================== Chapter 4 ====================
            case "ch4_escape":
                TriggerOnce(level, "ch4_escape_trigger", () => new Cs04Escape(player));
                break;
                
            case "ch4_call_mom":
                TriggerOnce(level, "ch4_call_mom_trigger", () => new Cs04CallMom(player));
                break;
                
            case "ch4_mirror":
                // Requires DreamMirror or RalseiMirror entity
                var dreamMirror = Scene.Entities.FindFirst<DreamMirror>();
                var ralseiMirror = Scene.Entities.FindFirst<RalseiMirror>();
                if (dreamMirror != null)
                {
                    TriggerOnce(level, "ch4_mirror_trigger", () => new Cs04Mirror(player, dreamMirror));
                }
                else if (ralseiMirror != null)
                {
                    TriggerOnce(level, "ch4_mirror_trigger", () => new Cs04Mirror(player, ralseiMirror));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "ch4_mirror: No mirror entity found");
                    RemoveSelf();
                }
                break;
                
            case "ch4_chara_warning":
                // Requires CharaChaser2 entity
                var charaChaser2 = Scene.Entities.FindFirst<CharaChaser2>();
                if (charaChaser2 != null)
                {
                    TriggerOnce(level, "ch4_chara_warning_trigger", () => new CS04_CharaWarning(charaChaser2));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "ch4_chara_warning: CharaChaser2 not found");
                    RemoveSelf();
                }
                break;
                
            case "ch4_ending":
                TriggerOnce(level, "ch4_ending_trigger", () => new Cs04Ending(player));
                break;

            // ==================== Chapter 5 ====================
            case "ch5_see_maddy":
                TriggerOnce(level, "ch5_see_maddy_trigger", () => new Cs05SeeMaddy(player, 0));
                break;
                
            case "ch5_diary":
                TriggerOnce(level, "ch5_diary_trigger", () => new Cs05Diary(false, false, player));
                break;
                
            case "ch5_guestbook":
                TriggerOnce(level, "ch5_guestbook_trigger", () => new Cs05Guestbook(player));
                break;
                
            case "ch5_memo":
                TriggerOnce(level, "ch5_memo_trigger", () => new Cs05Memo(player));
                break;
                
            case "ch5_maddy_phone":
                TriggerOnce(level, "ch5_maddy_phone_trigger", () => new Cs05MaddyPhone(player, 0f));
                break;

            case "ch5_ending":
                // Requires ResortRoofEnding entity
                var resortRoofEnding = Scene.Entities.FindFirst<global::MaggyHelper.Entities.ResortRoofEnding>();
                if (resortRoofEnding != null)
                {
                    TriggerOnce(level, "ch5_ending_trigger", () => new CS05_Ending(resortRoofEnding, player));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "ch5_ending: ResortRoofEnding not found");
                    RemoveSelf();
                }
                break;
                
            case "ch5_magorlor_escape":
                // Requires NPC05_Magolor_Escaping entity
                var magolorEscaping = Scene.Entities.FindFirst<NPC05_Magolor_Escaping>();
                if (magolorEscaping != null)
                {
                    TriggerOnce(level, "ch5_magolor_escape_trigger", () => new CS05_MagolorEscape(magolorEscaping, player));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "ch5_magolor_escape: NPC05_Magolor_Escaping not found");
                    RemoveSelf();
                }
                break;
                
            case "ch5_oshiro_lobby":
                // Requires NPC oshiro entity (NPC)
                var oshiroLobby = Scene.Entities.FindFirst<NPC>();
                if (oshiroLobby != null)
                {
                    TriggerOnce(level, "ch5_oshiro_lobby_trigger", () => new CS05_OshiroLobby(player, oshiroLobby));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "ch5_oshiro_lobby: NPC not found");
                    RemoveSelf();
                }
                break;
                
            case "ch5_oshiro_clutter":
                // Requires NPC05_Oshiro_Cluttter entity
                var oshiroClutter = Scene.Entities.FindFirst<NPC05_Oshiro_Cluttter>();
                if (oshiroClutter != null)
                {
                    TriggerOnce(level, "ch5_oshiro_clutter_trigger", () => new CS05_OshiroClutter(player, oshiroClutter, 0));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "ch5_oshiro_clutter: NPC05_Oshiro_Cluttter not found");
                    RemoveSelf();
                }
                break;

            // ==================== Chapter 6 ====================
            case "ch6_intro":
                var gondola = Scene.Entities.FindFirst<GondolaMaggy>();
                var theoNPC = Scene.Entities.FindFirst<NPC>();
                if (gondola != null && theoNPC != null)
                {
                    TriggerOnce(level, "ch6_intro_trigger", () => new CS06_Gondola(theoNPC, gondola, player));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "ch6_intro: Required entities not found");
                    RemoveSelf();
                }
                break;
                
            case "ch6_stronghold":
                TriggerOnce(level, "ch6_stronghold_trigger", () => new Cs06Stronghold(player));
                break;
                
            case "ch6_end":
                TriggerOnce(level, "ch6_end_trigger", () => new Cs06End(player));
                break;

            // ==================== Chapter 7 ====================
            case "ch7_enter":
                TriggerOnce(level, "ch7_enter_trigger", () => new Cs07Enter(player));
                break;
                
            case "ch7_intro":
                TriggerOnce(level, "ch7_intro_trigger", () => new Cs07Intro(player));
                break;
                
            case "ch7_see_maddy_mirror":
                TriggerOnce(level, "ch7_see_maddy_mirror_trigger", () => new Cs07MaddyMirror(player));
                break;
                
            case "ch7_pre_ingeste":
                TriggerOnce(level, "ch7_pre_ingeste_trigger", () => new Cs07PreIngeste(player));
                break;
                
            case "ch7_see_maddy":
                TriggerOnce(level, "ch7_see_maddy_trigger", () => new Cs05SeeMaddy(player, 0));
                break;
                
            case "ch7_found_maddy":
                TriggerOnce(level, "found_maddy_trigger", () => new Cs07SaveMaddy(player));
                break;
                
            case "ch7_mirror_portal":
                // Requires TesseractMirrorGateway entity
                var tesseractMirror = Scene.Entities.FindFirst<TesseractMirrorGateway>();
                if (tesseractMirror != null)
                {
                    TriggerOnce(level, "ch7_mirror_portal_trigger", () => new CS07_MirrorPortal(player, tesseractMirror));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "ch7_mirror_portal: TesseractMirrorGateway not found");
                    RemoveSelf();
                }
                break;
                
            case "ch7_darker":
                TriggerOnce(level, "ch7_darker_trigger", () => new CS07_Darker(player));
                break;

            // ==================== Chapter 8 ====================
            case "ch8_plat":
                var npc08MaddyPlat = Scene.Entities.FindFirst<Npc08MadelinePlateau>();
                var celesteNpcMadeline = Scene.Entities.FindFirst<CelesteNPC>();
                TriggerOnce(level, "npc08_maddy_plat_trigger", () => new Cs08Campfire(npc08MaddyPlat, player, celesteNpcMadeline));
                break;

            case "ch8_charaboss_intro":
            case "ch8_intro_chara_boss":
                var CharaBoss = Scene.Entities.FindFirst<CharaBoss>();
                if (CharaBoss != null)
                {
                    TriggerOnce(level, "ch8_charaboss_intro", () => new Cs08CharaBossIntro(player.X, player, CharaBoss));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "ch8_charaboss_intro: CharaBoss not found");
                    RemoveSelf();
                }
                break;
                
            case "ch8_chara_boss_mid":
                // CS08_BossMid has no constructor parameters - it finds player internally
                TriggerOnce(level, "ch8_chara_boss_mid_trigger", () => new CS08_BossMid());
                break;
                
            case "ch8_chara_boss_center":
                TriggerOnce(level, "ch8_chara_boss_center_trigger", () => new Cs08CharaBossCenter(player));
                break;
                
            case "ch8_chara_boss_end":
                var charaCrying = Scene.Entities.FindFirst<Npc08CharaCrying>();
                TriggerOnce(level, "ch8_chara_boss_end_trigger", () => new Cs08CharaBossEnd(player, charaCrying));
                break;
                
            case "ch8_end":
                var badelineDummy = Scene.Entities.FindFirst<global::MaggyHelper.Entities.BadelineDummy>();
                var charaDummy = Scene.Entities.FindFirst<CharaDummy>();
                var Npc08MaddyAndTheoEnding = Scene.Entities.FindFirst<Npc08MaddyAndTheoEnding>();
                var Npc08MaggyEnding = Scene.Entities.FindFirst<Npc08MaggyEnding>();
                TriggerOnce(level, "ch8_end_trigger", () => new Cs08End(player, Npc08MaddyAndTheoEnding, Npc08MaggyEnding));
                break;
                
            case "ch8_theo":
                TriggerOnce(level, "ch8_theo_trigger", () => new Cs08Theo(player));
                break;

            case "ch8_reflectionmod":
                TriggerOnce(level, "ch8_reflectionmod_trigger", () => new Cs08Reflection(player, targetX: 0f));
                break;
                
            case "ch8_star_jump_end":
                // Requires NPC star jump controller (Celeste.NPC)
                var starJumpController = Scene.Entities.FindFirst<CelesteNPC>();
                if (starJumpController != null)
                {
                    TriggerOnce(level, "ch8_star_jump_end_trigger", () => new CS08_StarJumpEnd(starJumpController, player, player.Position, level.Camera.Position));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "ch8_star_jump_end: NPC controller not found");
                    RemoveSelf();
                }
                break;

            // ==================== Chapter 9 ====================
            case "ch9_arrivial":
                TriggerOnce(level, "ch9_arrival_trigger", () => new CS09_Ascend(0, "ch9_arrivial", false));
                break;

            case "ch9_fake_saved":
                TriggerOnce(level, "ch9_fake_saved_trigger", () => new CS09_FakeSavePoint(player, CS09_FakeSavePoint.GetCurrentStage(level)));
                break;
                
            case "ch9_credits":
                TriggerOnce(level, "ch9_credits_trigger", () => new CS09_Credits(player));
                break;
                
            case "ch9_message_end":
                TriggerOnce(level, "ch9_message_end_trigger", () => new CS09_MessageEnd(player));
                break;
                
            case "ch9_end":
                TriggerOnce(level, "ch9_end_trigger", () => new Cs09End(player));
                break;

            // ==================== Chapter 10 ====================
            case "ch10_intro":
                if (!level.Session.GetFlag("ch10_intro_trigger"))
                {
                    level.Session.SetFlag("ch10_intro_trigger");
                    Logger.Log(LogLevel.Info, "IngesteEventTrigger", "Changing scene to Cs10IntroVignetteAlt");
                    Engine.Scene = new Cs10IntroVignetteAlt(level.Session);
                    RemoveSelf();
                }
                break;
                
            case "CH10_flowey_intro":
                TriggerOnce(level, "ch10_flowey_intro_trigger", () => new Cs10FloweyIntro(player));
                break;

            case "CH10_flowey_intro_scene":
                TriggerOnce(level, "ch10_flowey_intro_complete", () => new FloweyIntroScene(player));
                break;
                
            case "ch10_house":
                TriggerOnce(level, "ch10_house_trigger", () => new Cs10House(player));
                break;
                
            case "ch10_house_indoor":
                TriggerOnce(level, "ch10_house_indoor_trigger", () => new Cs10HouseIndoor(player));
                break;
                
            case "ch10_house_outdoor":
                TriggerOnce(level, "ch10_house_outdoor_trigger", () => new Cs10HouseOutdoor(player));
                break;
                
            case "ch10_pre_boss":
                TriggerOnce(level, "ch10_pre_boss_trigger", () => new Cs10PreBoss(player));
                break;
                
            case "ch10_post_boss":
                TriggerOnce(level, "ch10_post_boss_trigger", () => new Cs10PostBoss(player));
                break;
                
            case "ch10_piano_start":
                TriggerOnce(level, "ch10_piano_start_trigger", () => new CS10_PianoStart(player));
                break;
                
            case "ch10_roxus_start":
                TriggerOnce(level, "ch10_roxus_start_trigger", () => new CS10_RoxusStart(player));
                break;
                
            case "ch10_sans_restaurant":
                TriggerOnce(level, "ch10_sans_restaurant_trigger", () => new CS10_SansRestaurant(player));
                break;
                
            case "ch10_titan_tower_approach":
                TriggerOnce(level, "ch10_titan_tower_approach_trigger", () => new CS10_TitanTowerApproach(player));
                break;
                
            case "ch10_titan_boss_battle":
                TriggerOnce(level, "ch10_titan_boss_battle_trigger", () => new CS10_TitanBossBattle(player));
                break;
                
            case "ch10_maddy_baddy_chara_intro":
                TriggerOnce(level, "ch10_maddy_baddy_chara_intro_trigger", () => new CS10_MaddyBaddyCharaIntro(player, level.Session.Level));
                break;

            // ==================== Chapter 11 ====================
            case "ch11_intro":
                TriggerOnce(level, "ch11_intro_trigger", () => new CS11_Intro_Marlet(player));
                break;
                
            case "ch11_town":
                TriggerOnce(level, "ch11_town_trigger", () => new Cs11Town(player));
                break;
                
            case "ch11_marlet_pre_boss":
                TriggerOnce(level, "ch11_marlet_pre_boss_trigger", () => new Cs11MarletPreBoss(player));
                break;
                
            case "ch11_marlet_boss_end":
                TriggerOnce(level, "ch11_marlet_boss_end_trigger", () => new Cs11MarletBossEnd(player));
                break;
                
            case "ch11_bar_arrival":
                TriggerOnce(level, "ch11_bar_arrival_trigger", () => new CS11_BarArrival(player));
                break;
                
            case "ch11_cowboy_bar_intro":
                TriggerOnce(level, "ch11_cowboy_bar_intro_trigger", () => new CS11_CowboyBarIntro(player));
                break;
                
            case "ch11_cinematic_bar":
                TriggerOnce(level, "ch11_cinematic_bar_trigger", () => new CS11_CinematicBar(player));
                break;
                
            case "ch11_maggy":
                TriggerOnce(level, "ch11_maggy_trigger", () => new CS11_Maggy(player));
                break;
                
            case "ch11_maggy_end":
                TriggerOnce(level, "ch11_maggy_end_trigger", () => new CS11_MaggyEnd(player));
                break;
                
            case "ch11_collecting_mini_heart_enough":
                TriggerOnce(level, "ch11_collecting_mini_heart_enough_trigger", () => new CS11_CollectingMiniHeartEnough(player));
                break;
                
            case "ch11_boss_intro":
                TriggerOnce(level, "ch11_boss_intro_trigger", () => new CS11_BossIntro(player));
                break;
                
            case "ch11_boss_mid":
                TriggerOnce(level, "ch11_boss_mid_trigger", () => new CS11_BossMid(player));
                break;
                
            case "ch11_boss_outro":
                TriggerOnce(level, "ch11_boss_outro_trigger", () => new CS11_BossOutro(player));
                break;
                
            case "ch11_starlo_and_marlet":
                TriggerOnce(level, "ch11_starlo_and_marlet_trigger", () => new CS11_StarloAndMarlet(player));
                break;

            // ==================== Chapter 12 ====================
            case "ch12_intro":
                TriggerOnce(level, "ch12_intro_trigger", () => new Cs12Intro(player));
                break;
                
            case "ch12_titan_tower":
                TriggerOnce(level, "ch12_titan_tower_trigger", () => new Cs12TitanTower(player));
                break;
                
            case "ch12_titan_pre_boss":
                TriggerOnce(level, "ch12_titan_pre_boss_trigger", () => new Cs12TitanPreBoss(player));
                break;
                
            case "ch12_undyne_refused_to_died":
                TriggerOnce(level, "ch12_undyne_refused_to_died_trigger", () => new Cs12UndyneRefusedToDied(player));
                break;
                
            case "ch12_titan_post_boss":
                TriggerOnce(level, "ch12_titan_post_boss_trigger", () => new Cs12TitanPostBoss(player));
                break;
                
            case "ch12_end":
                TriggerOnce(level, "ch12_end_trigger", () => new Cs12End(player));
                break;
                
            // Note: Cs12TowerFountain requires EntityData and offset - not suitable for trigger use
            // Removing ch12_tower_fountain from EventTrigger as it requires map entity data

            // ==================== Chapter 13 ====================
            case "ch13_hot_lava":
                TriggerOnce(level, "ch13_hot_lava_trigger", () => new Cs13HotLava(player));
                break;
                
            case "ch13_axis_intro":
                TriggerOnce(level, "ch13_axis_intro_trigger", () => new Cs13AxisIntro(player));
                break;
                
            case "ch13_well_prepared":
                TriggerOnce(level, "ch13_well_prepared_trigger", () => new Cs13WellPrepared(player));
                break;
                
            case "ch13_axis_pre_boss":
                TriggerOnce(level, "ch13_axis_pre_boss_trigger", () => new Cs13AxisPreBoss(player));
                break;
                
            case "ch13_axis_post_boss":
                TriggerOnce(level, "ch13_axis_post_boss_trigger", () => new Cs13AxisPostBoss(player));
                break;
                
            case "ch13_end":
                TriggerOnce(level, "ch13_end_trigger", () => new Cs13End(player));
                break;
                
            case "ch13_intro":
                TriggerOnce(level, "ch13_intro_trigger", () => new CS13_Intro(player));
                break;
                
            case "ch13_meta_knight_encounter":
                TriggerOnce(level, "ch13_meta_knight_encounter_trigger", () => new CS13_MetaKnightEncounter(player));
                break;
                
            case "ch13_axis_boss_battle":
                TriggerOnce(level, "ch13_axis_boss_battle_trigger", () => new CS13_AxisBossBattle(player));
                break;

            // ==================== Chapter 14 ====================
            case "ch14_intro_core":
                TriggerOnce(level, "ch14_intro_core_trigger", () => new Cs14IntroCore(player));
                break;
                
            case "ch14_giga_axis_pre_boss":
                TriggerOnce(level, "ch14_giga_axis_pre_boss_trigger", () => new Cs14GigaAxisPreBoss(player));
                break;
                
            case "ch14_giga_axis_post_boss":
                TriggerOnce(level, "ch14_giga_axis_post_boss_trigger", () => new Cs14GigaAxisPostBoss(player));
                break;
                
            case "ch14_enter_last_elevator":
                TriggerOnce(level, "ch14_enter_last_elevator_trigger", () => new Cs14EnterLastElevator(player));
                break;
                
            case "ch14_intro":
                TriggerOnce(level, "ch14_intro_trigger", () => new CS14_Intro(player));
                break;
                
            case "ch14_hollow_programmer":
                TriggerOnce(level, "ch14_hollow_programmer_trigger", () => new CS14_HollowProgrammer(player));
                break;
                
            case "ch14_giant_axis_battle":
                TriggerOnce(level, "ch14_giant_axis_battle_trigger", () => new CS14_GiantAxisBattle(player));
                break;

            // ==================== Chapter 15 ====================
            case "ch15_exit_last_elevator":
                TriggerOnce(level, "ch15_exit_last_elevator_trigger", () => new Cs15ExitLastElevator(player));
                break;
                
            case "ch15_zantas_1":
                TriggerOnce(level, "ch15_zantas_1_trigger", () => new Cs15Zantas1(player));
                break;
                
            case "ch15_zantas_2":
                TriggerOnce(level, "ch15_zantas_2_trigger", () => new Cs15Zantas2(player));
                break;
                
            case "ch15_judgement":
                TriggerOnce(level, "ch15_judgement_trigger", () => new Cs15Judgement(player));
                break;
                
            case "ch15_intro_roaring_titan":
                TriggerOnce(level, "ch15_intro_roaring_titan_trigger", () => new Cs15IntroRoaringTitan(player));
                break;
                
            case "ch15_barrier":
                TriggerOnce(level, "ch15_barrier_trigger", () => new Cs15Barrier(player));
                break;
                
            case "ch15_roaring_titan_pre_boss":
                TriggerOnce(level, "ch15_roaring_titan_pre_boss_trigger", () => new Cs15RoaringTitanPreBoss(player));
                break;
                
            case "ch15_roaring_titan_post_boss":
                TriggerOnce(level, "ch15_roaring_titan_post_boss_trigger", () => new Cs15RoaringTitanPostBoss(player));
                break;
                
            case "ch15_flowey_transformation":
                TriggerOnce(level, "ch15_flowey_transformation_trigger", () => new CS15_FloweyTransformation(player));
                break;
                
            case "ch15_mountain_peak_arrival":
                TriggerOnce(level, "ch15_mountain_peak_arrival_trigger", () => new CS15_MountainPeakArrival(player));
                break;
                
            case "ch15_roaring_titan_king_battle":
                TriggerOnce(level, "ch15_roaring_titan_king_battle_trigger", () => new CS15_RoaringTitanKingBattle(player));
                break;
                
            case "ch15_titan_council_judgment":
                TriggerOnce(level, "ch15_titan_council_judgment_trigger", () => new CS15_TitanCouncilJudgment(player));
                break;

            // ==================== Chapter 16 ====================
            case "ch16_els_intro":
                TriggerOnce(level, "ch16_els_intro_trigger", () => new CS16_ElsIntro(player));
                break;
                
            case "ch16_els_finale":
                TriggerOnce(level, "ch16_els_finale_trigger", () => new CS16_ElsFinale(player));
                break;
                
            case "ch16_els_outro":
                TriggerOnce(level, "ch16_els_outro_trigger", () => new CS16_ElsOutro(player));
                break;
                
            case "ch16_exited":
                TriggerOnce(level, "ch16_exited_trigger", () => new Cs16Exited(player));
                break;
                
            case "ch16_end":
                TriggerOnce(level, "ch16_end_trigger", () => new Cs16End(player));
                break;
                
            case "ch16_Epilouge":
                TriggerOnce(level, "ch16_Epilouge_trigger", () => new Cs16WelcomeHome(player, targetX: 0f));
                break;
                
            case "ch16_barrier_breaks":
                TriggerOnce(level, "ch16_barrier_breaks_trigger", () => new CS16_BarrierBreaks(player));
                break;
                
            case "ch16_corrupted_reality_intro":
                TriggerOnce(level, "ch16_corrupted_reality_intro_trigger", () => new CS16_CorruptedRealityIntro(player));
                break;
                
            case "ch16_lost_souls_unite":
                TriggerOnce(level, "ch16_lost_souls_unite_trigger", () => new CS16_LostSoulsUnite(player));
                break;
                
            case "ch16_save_file_battle":
                TriggerOnce(level, "ch16_save_file_battle_trigger", () => new CS16_SaveFileBattle(player));
                break;

            // ==================== Chapter 17 ====================
            case "ch17_epilouge":
                TriggerOnce(level, "ch17_epilogue_trigger", () => new Cs17Epilogue(player));
                break;
                
            case "ch17_credits":
                // CS17_Credits has no parameters
                TriggerOnce(level, "ch17_credits_trigger", () => new CS17_Credits());
                break;
                
            case "ch17_ending_mod":
                // CS17_EndingMod has no parameters
                TriggerOnce(level, "ch17_ending_mod_trigger", () => new CS17_EndingMod());
                break;

            // ==================== Chapter 18 ====================
            case "ch18_outro":
                TriggerOnce(level, "ch18_outro_trigger", () => new CS18_Outro(player));
                break;

            // ==================== Chapter 19 ====================
            case "ch19_another_dimension_intro":
                TriggerOnce(level, "ch19_another_dimension_intro_trigger", () => new Cs19AnotherDimensionIntro(player));
                break;
                
            case "ch19_big_final_room":
                TriggerOnce(level, "ch19_big_final_room_trigger", () => new Cs19BigFinalRoom(player, first: true));
                break;
                
            case "ch19_hub_second_intro":
            case "ch19_hub_second_intro_trigger":
                TriggerOnce(level, "ch19_hub_second_intro_trigger", () => new CS19_HubSecondIntro(Scene, player));
                break;

            case "ch19_loop":
                var charaDummyLoop = Scene.Entities.FindFirst<CharaDummy>();
                var magolorLoop = Scene.Entities.FindFirst<Npc19MaggyLoop>();
                var powergen = Scene.Entities.FindFirst<PowerGenerator>();
                var flingbird = Scene.Entities.FindFirst<FlingBird>();
                var CustomCharaBoost = Scene.Entities.FindFirst<CustomCharaBoost>();
                TriggerOnce(level, "ch19_loop_trigger", () => new Cs19TrapinLoop(player, charaDummyLoop));
                break;
                
            case "ch19_chara_help":
                TriggerOnce(level, "ch19_chara_help_trigger", () => new CS19_CharaHelps(player));
                var charaDummyHelpingHand = Scene.Entities.FindFirst<CharaDummy>();
                break;
                
            case "ch19_free_bird":
                // CS19_FreeBird has no constructor parameters
                TriggerOnce(level, "ch19_free_bird_trigger", () => new CS19_FreeBird());
                break;
                
            case "ch19_kill_the_bird":
                // Requires FlingBirdIntroMod entity
                var flingBirdKill = Scene.Entities.FindFirst<FlingBirdIntroMod>();
                if (flingBirdKill != null)
                {
                    TriggerOnce(level, "ch19_kill_the_bird_trigger", () => new CS19_KillTheBird(player, flingBirdKill));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "ch19_kill_the_bird: FlingBirdIntroMod not found");
                    RemoveSelf();
                }
                break;
                
            case "ch19_miss_the_bird":
                // Requires FlingBirdIntroMod entity
                var flingBirdMiss = Scene.Entities.FindFirst<FlingBirdIntroMod>();
                if (flingBirdMiss != null)
                {
                    TriggerOnce(level, "ch19_miss_the_bird_trigger", () => new CS19_MissTheBird(player, flingBirdMiss));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "ch19_miss_the_bird: FlingBirdIntroMod not found");
                    RemoveSelf();
                }
                break;
                
            case "ch19_gravestone":
                // Requires NPC19_Gravestone entity
                var gravestone = Scene.Entities.FindFirst<NPC19_Gravestone>();
                if (gravestone != null)
                {
                    TriggerOnce(level, "ch19_gravestone_trigger", () => new CS19_Gravestone(player, gravestone, Vector2.Zero));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "ch19_gravestone: NPC19_Gravestone not found");
                    RemoveSelf();
                }
                break;
                
            case "ch19_final_launch":
                // Requires CustomCharaBoost entity
                var charaBoost = Scene.Entities.FindFirst<CustomCharaBoost>();
                if (charaBoost != null)
                {
                    TriggerOnce(level, "ch19_final_launch_trigger", () => new CS19_FinalLaunch(player, charaBoost, true));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "IngesteEventTrigger", "ch19_final_launch: CustomCharaBoost not found");
                    RemoveSelf();
                }
                break;
                
            case "ch19_souls_discarded":
                TriggerOnce(level, "ch19_souls_discarded_trigger", () => new CS19_SoulsDiscarded(player));
                break;
                
            case "ch19_beyond_the_void":
                TriggerOnce(level, "ch19_beyond_the_void_trigger", () => new CS19_BeyondTheVoid(player));
                break;
                
            case "ch19_memories_of_the_past":
                TriggerOnce(level, "ch19_memories_of_the_past_trigger", () => new CS19_MemoriesOfThePast(player));
                break;
                
            case "ch19_els_breaks_free":
                TriggerOnce(level, "ch19_els_breaks_free_trigger", () => new CS19_ElsBreaksFree(player));
                break;
                
            case "ch19_broken_star_warrior":
                TriggerOnce(level, "ch19_broken_star_warrior_trigger", () => new CS19_BrokenStarWarrior(player));
                break;
                
            case "ch19_edge_of_universe":
                TriggerOnce(level, "ch19_edge_of_universe_trigger", () => new CS19_EdgeOfUniverse(player));
                break;
                
            case "ch19_goto_the_future":
            case "ch19_goto_the_past":
                level.OnEndOfFrame += () =>
                {
                    new Vector2(level.LevelOffset.X + (float)level.Bounds.Width - player.X, player.Y - level.LevelOffset.Y);
                    Vector2 levelOffset = level.LevelOffset;
                    Vector2 vector = player.Position - level.LevelOffset;
                    Vector2 vector2 = level.Camera.Position - level.LevelOffset;
                    Facings facing = player.Facing;
                    level.Remove(player);
                    level.UnloadLevel();
                    level.Session.Dreaming = true;
                    level.Session.Level = ((eventName == "ch19_goto_the_future") ? "intro-01-future-maggy" : "intro-00-past-maggy");
                    level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
                    level.Session.FirstLevel = false;
                    level.LoadLevel(global::Celeste.Player.IntroTypes.Transition);
                    level.Camera.Position = level.LevelOffset + vector2;
                    level.Session.Inventory.Dashes = 1;
                    player.Dashes = Math.Min(player.Dashes, 1);
                    level.Add(player);
                    player.Position = level.LevelOffset + vector;
                    player.Facing = facing;
                    player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
                    if (level.Wipe != null)
                    {
                        level.Wipe.Cancel();
                    }
                    level.Flash(Color.White);
                    level.Shake();
                    level.Add(new LightningStrike(new Vector2(player.X + 60f, level.Bounds.Bottom - 180), 10, 200f));
                    level.Add(new LightningStrike(new Vector2(player.X + 220f, level.Bounds.Bottom - 180), 40, 200f, 0.25f));
                    Audio.Play("event:/new_content/game/10_farewell/lightning_strike");
                };
                RemoveSelf();
                break;

            // ==================== Chapter 20 ====================
            case "ch20_bird_guidance_intro":
                TriggerOnce(level, "ch20_bird_guidance_intro_trigger", () => new Cs20BirdGuidanceIntro(player));
                break;
                
            case "ch20_intro":
                TriggerOnce(level, "ch20_intro_trigger", () => new Cs20Intro(player));
                break;

            case "ch20_fake_madeline_and_badeline":
                TriggerOnce(level, CS20_FakeMadelineAndBadeline.Flag, () => new CS20_FakeMadelineAndBadeline(player));
                break;
                
            case "ch20_tess_fake_pre_boss":
                TriggerOnce(level, "ch20_tess_fake_pre_boss_trigger", () => new Cs20TessFakePreBoss(player));
                break;
                
            case "ch20_tess_fake_post_boss":
                TriggerOnce(level, "ch20_tess_fake_post_boss_trigger", () => new Cs20TessFakePostBoss(player));
                break;
                
            case "ch20_nothiness":
                TriggerOnce(level, "ch20_nothingness_trigger", () => new Cs20Nothingness(player));
                break;
                
            case "ch20_tess_pre_boss_for_real":
                TriggerOnce(level, "ch20_tess_pre_boss_for_real_trigger", () => new Cs20TessPreBossForReal(player));
                break;
                
            case "ch20_saved":
                TriggerOnce(level, "ch20_saved_trigger", () => new CS20_Saved(player));
                var Npc20_MadelineSaved = Scene.Entities.FindFirst<Npc20_Madeline>();
                var badelineDummySaved = Scene.Entities.FindFirst<global::MaggyHelper.Entities.BadelineDummy>();
                var Npc20_AsrielSaved = Scene.Entities.FindFirst<Npc20_Asriel>();
                var Npc20_Granny = Scene.Entities.FindFirst<Npc20_Granny>();
                break;
                
            case "ch20_ending":
                TriggerOnce(level, "ch20_true_end_trigger", () => new CS20_Ending(player));
                break;

            case "ch20_asriel_god_boss_identity_reveal":
            case "ch20_asriel_god_boss_identity_reveal_trigger":
                var asrielBoss = Scene.Entities.FindFirst<AsrielGodBoss>();
                TriggerOnce(level, "ch20_asriel_god_boss_identity_reveal_trigger", () => new CS20_AsrielRevealIdentity(player, asrielBoss));
                break;
                
            case "ch20_asriel_angel_of_death_boss_intro":
                // CS20_AsrielAngelOfDeathBossIntro takes optional roomId string
                TriggerOnce(level, "ch20_asriel_angel_of_death_boss_intro_trigger", () => new CS20_AsrielAngelOfDeathBossIntro(level.Session.Level));
                break;
                
            case "ch20_boss_mid":
                // CS20_BossMid has no constructor parameters - finds player internally
                TriggerOnce(level, "ch20_boss_mid_trigger", () => new CS20_BossMid());
                break;
                
            case "ch20_boss_end":
                // CS20_BossEnd has no constructor parameters
                TriggerOnce(level, "ch20_boss_end_trigger", () => new CS20_BossEnd());
                break;

            case "ch20_asriel_boss_end":
                // Post Angel-of-Death fight: Asriel breaks down, Els reveals itself
                TriggerOnce(level, CS20_AsrielBossEnd.Flag, () => new CS20_AsrielBossEnd(player));
                break;
                
            case "ch20_final_boss_defeat":
                TriggerOnce(level, "ch20_final_boss_defeat_trigger", () => new CS20_FinalBossDefeat(player));
                break;
                
            case "ch20_rainbow_blossom_tree":
                TriggerOnce(level, "ch20_rainbow_blossom_tree_trigger", () => new CS20_RainbowBlossomTree(player));
                break;
                
            case "ch20_restoration_and_farewell":
                TriggerOnce(level, "ch20_restoration_and_farewell_trigger", () => new CS20_RestorationAndFarewell(player));
                break;
                
            case "ch20_end_later":
                TriggerOnce(level, "ch20_end_later_trigger", () => new CS20_Later(player));
                break;

            case "ch20_end_cinematic":
                TriggerOnce(level, "ch20_end_cinematic_trigger", () => new CS20_Ending(player));
                break;
                
            case "ch20_white_cymbal_fade_teleport_video":
                TriggerOnce(level, "ch20_white_cymbal_fade_teleport_video_trigger", () => new CS20_WhiteCymbalFadeTeleportVideo(player));
                break;

            // ==================== Chapter 21 ====================
            case "ch21_beaches":
                TriggerOnce(level, "ch21_beaches_trigger", () => new Cs21Beaches(player));
                break;
                
            case "ch21_special_thanks":
                TriggerOnce(level, "ch21_special_thanks_trigger", () => new Cs21SpecialThanks(player));
                break;
                
            case "ch21_epilogue_credits":
                TriggerOnce(level, "ch21_epilogue_credits_trigger", () => new CS21_EpilogueCredits(player));
                break;
                
            case "ch21_two_worlds_unite":
                TriggerOnce(level, "ch21_two_worlds_unite_trigger", () => new CS21_TwoWorldsUnite(player));
                break;
                
            default:
                Logger.Log(LogLevel.Warn, "IngesteEventTrigger", $"Unknown event: {eventName}");
                TriggerOnce(level, $"generic_{eventName}_trigger", () => CreateGenericCutscene(player, eventName));
                break;
        }
    }

    private CutsceneEntity CreateGenericCutscene(global::Celeste.Player player, string eventName)
    {
        return new GenericCutscene(player, eventName);
    }

    private class GenericCutscene : CutsceneEntity
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
            Add(new Coroutine(cutscene(level)));
        }

        private IEnumerator cutscene(Level level)
        {
            player.StateMachine.State = 11;
            yield return 0.5f;
            
            string dialogKey = $"{eventName.ToUpper()}";
            yield return Textbox.Say(dialogKey);
            
            yield return 0.5f;
            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            if (player != null)
                player.StateMachine.State = 0;
        }
    }
}
