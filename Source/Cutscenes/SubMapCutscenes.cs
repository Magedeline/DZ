namespace Celeste.Cutscenes
{
    /// <summary>
    /// Cutscene for introducing submaps in chapters 10-14
    /// </summary>
    public class CsSubMapIntro : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CsSubMapIntro(global::Celeste.Player player, int chapter) : base()
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            // Implement the required abstract method
            // Add logic for what should happen when the cutscene begins
        }

        public override void OnEnd(Level level)
        {
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal; // Use 0 for normal state instead of Player.StNormal
            }
        }
    }

    /// <summary>
    /// Cutscene for Chapter 10 submap completion
    /// </summary>
    public class Cs10SubMapComplete : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public Cs10SubMapComplete(global::Celeste.Player player, int submap) : base()
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            // Implement the required abstract method
            // Add logic for what should happen when the cutscene begins
        }

        public override void OnEnd(Level level)
        {
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal; // Use 0 for normal state instead of Player.StNormal
            }
        }
    }

    /// <summary>
    /// Cutscene for Chapter 11 submap completion
    /// </summary>
    public class Cs11SubMapComplete : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public Cs11SubMapComplete(global::Celeste.Player player, int submap) : base()
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            // Implement the required abstract method
            // Add logic for what should happen when the cutscene begins
        }

        public override void OnEnd(Level level)
        {
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal; // Use 0 for normal state instead of Player.StNormal
            }
        }
    }

    /// <summary>  
    /// Cutscene for Chapter 12 submap completion  
    /// </summary>  
    public class Cs12SubMapComplete : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public Cs12SubMapComplete(global::Celeste.Player player, int submap) : base()
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            // Implement the required abstract method
            // Add logic for what should happen when the cutscene begins
        }

        public override void OnEnd(Level level)
        {
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal; // Use 0 for normal state instead of Player.StNormal
            }
        }
    }

    /// <summary>
    /// Generic submap completion cutscene for chapters 13-14
    /// </summary>
    public class CsGenericSubMapComplete : CutsceneEntity
    {
        private readonly global::Celeste.Player player;

        public CsGenericSubMapComplete(global::Celeste.Player player, int chapter, int submap) : base()
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            // Implement the required abstract method
            // Add logic for what should happen when the cutscene begins
        }

        public override void OnEnd(Level level)
        {
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal; // Use 0 for normal state instead of Player.StNormal
            }
        }
    }
}



