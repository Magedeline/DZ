namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger for Chapter 17 cutscenes - descent and credits sequences
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/Chapter17CutsceneTrigger")]
    public class Chapter17CutsceneTrigger : Trigger
    {
        public string CutsceneId { get; private set; }
        public bool TriggerOnce { get; private set; }

        private bool triggered = false;

        public Chapter17CutsceneTrigger(EntityData data, Vector2 offset) 
            : base(data, offset)
        {
            CutsceneId = data.Attr("cutsceneId", "ch17_default");
            TriggerOnce = data.Bool("triggerOnce", true);
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (TriggerOnce && triggered)
                return;

            triggered = true;
            var level = SceneAs<Level>();
            if (level != null)
            {
                TriggerCutscene(level, player);
            }
        }

        private void TriggerCutscene(Level level, global::Celeste.Player player)
        {
            // Set session flag for the cutscene
            level.Session.SetFlag($"ch17_cutscene_{CutsceneId}", true);

            // Handle specific cutscene types
            switch (CutsceneId)
            {
                case "ch17_descent_begins":
                    StartDescentCutscene(level, player);
                    break;

                case "ch17_leaving_zantas":
                    StartLeavingZantasCutscene(level, player);
                    break;

                case "ch17_credits_introduction":
                    StartCreditsIntro(level);
                    break;

                case "ch17_thanking_companions":
                    StartThankingCompanions(level);
                    break;

                case "ch17_final_descent":
                    StartFinalDescent(level, player);
                    break;

                case "ch17_epilogue":
                    StartEpilogue(level);
                    break;

                default:
                    // Generic cutscene handling via Lua
                    if (LuaCutsceneManager.IsInitialized)
                    {
                        // Lua cutscene system will handle it
                        LuaCutsceneManager.CallLuaFunction($"triggerCutscene(\"{CutsceneId}\")");
                    }
                    break;
            }
        }

        private void StartDescentCutscene(Level level, global::Celeste.Player player)
        {
            // Disable player control during descent
            player.StateMachine.State = global::Celeste.Player.StDummy;
            
            // Set flags for descent sequence
            level.Session.SetFlag("ch17_descent_active", true);
            
            // Trigger any dialog
            level.Add(new MiniTextbox("CH17_DESCENT_BEGINS"));
        }

        private void StartLeavingZantasCutscene(Level level, global::Celeste.Player player)
        {
            level.Session.SetFlag("ch17_leaving_zantas", true);
            level.Add(new MiniTextbox("CH17_LEAVING_ZANTAS"));
        }

        private void StartCreditsIntro(Level level)
        {
            level.Session.SetFlag("ch17_credits_intro", true);
            
            // Set flag to trigger credits part 1
            KIRBY_CELESTEModule.LaunchPart1Credits = true;
        }

        private void StartThankingCompanions(Level level)
        {
            level.Session.SetFlag("ch17_thanking_companions", true);
            level.Add(new MiniTextbox("CH17_THANKING_COMPANIONS"));
        }

        private void StartFinalDescent(Level level, global::Celeste.Player player)
        {
            level.Session.SetFlag("ch17_final_descent", true);
            player.StateMachine.State = global::Celeste.Player.StDummy;
        }

        private void StartEpilogue(Level level)
        {
            level.Session.SetFlag("ch17_epilogue", true);
            
            // Set flag to trigger credits part 2
            KIRBY_CELESTEModule.LaunchPart2Credits = true;
        }
    }
}
