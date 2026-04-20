using Celeste.Cutscenes;

namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger that starts the Chapter 18 outro cutscene with game closing effect
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/CH18OutroTrigger")]
    public class CH18OutroTrigger : Trigger
    {
        private bool hasTriggered;
        private bool triggerOnce;

        public CH18OutroTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            triggerOnce = data.Bool("triggerOnce", true);
            hasTriggered = false;
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (!hasTriggered || !triggerOnce)
            {
                startOutroCutscene(player);
            }
        }

        private void startOutroCutscene(global::Celeste.Player player)
        {
            if (Scene is Level level && player != null)
            {
                hasTriggered = true;

                // Start the cutscene using the correct signature
                var cutscene = new CS18_Outro(player);
                level.Add(cutscene);
            }
        }
    }
}



