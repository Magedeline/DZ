namespace Celeste.Cutscenes
{
    public class CutsceneManager : Entity
    {
        private static Dictionary<string, Func<IEnumerator>> cutsceneRegistry = new();
        
        public static void RegisterCutscene(string id, Func<IEnumerator> cutscene)
        {
            cutsceneRegistry[id] = cutscene;
        }
        
        public static IEnumerator PlayCutscene(string id, Level level)
        {
            if (cutsceneRegistry.TryGetValue(id, out var cutscene))
            {
                yield return cutscene();
            }
            else
            {
                Logger.Log(LogLevel.Warn, "DZ", $"Cutscene '{id}' not found!");
            }
        }

        /// <summary>
        /// Resets player input and movement to prevent stuck movement after cutscenes.
        /// Call this in cutscene OnEnd methods to ensure proper cleanup.
        /// </summary>
        public static void ResetPlayerState(global::Celeste.Player player)
        {
            if (player == null) return;

            // Reset input values
            Input.MoveX.Value = 0;
            Input.MoveY.Value = 0;
            
            // Reset player speed and movement state
            player.Speed = Vector2.Zero;
            player.OverrideDashDirection = null;
            
            // Reset dummy state flags if applicable
            player.DummyAutoAnimate = true;
            player.DummyGravity = true;
            player.DummyFriction = true;
        }

        internal static void Initialize()
        {
            cutsceneRegistry = new Dictionary<string, Func<IEnumerator>>();
            Logger.Log(LogLevel.Info, "DZ", "CutsceneManager initialized");
        }

        internal static void Cleanup() 
        {
            cutsceneRegistry?.Clear();
            Logger.Log(LogLevel.Info, "DZ", "CutsceneManager cleaned up");
        }
    }
    
    public class CutsceneTrigger : Entity
    {
        private global::Celeste.Player player;
        private string cutsceneId;
        private Action onComplete;

        public CutsceneTrigger(string cutsceneId, global::Celeste.Player player, Action onComplete = null)
        {
            this.cutsceneId = cutsceneId;
            this.player = player;
            this.onComplete = onComplete;
            Tag = Tags.Global;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(runCutscene()));
        }

        private IEnumerator runCutscene()
        {
            Level level = Scene as Level;
            level.PauseLock = true;
            player.StateMachine.State = global::Celeste.Player.StDummy;

            yield return CutsceneManager.PlayCutscene(cutsceneId, level);

            // Reset player state to prevent stuck movement
            CutsceneManager.ResetPlayerState(player);
            
            level.PauseLock = false;
            player.StateMachine.State = global::Celeste.Player.StNormal;
            onComplete?.Invoke();
            RemoveSelf();
        }
    }
}




