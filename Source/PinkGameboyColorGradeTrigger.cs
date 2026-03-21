namespace MaggyHelper.Triggers
{
    /// <summary>
    /// Trigger that applies a pink Game Boy-style color grade and sets a flag when activated
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/PinkGameboyColorGradeTrigger")]
    [Tracked]
    public class PinkGameboyColorGradeTrigger : Trigger
    {
        private string flagToSet;
        private string colorGradeName;
        private bool triggerOnce;
        private bool hasTriggered;
        private float transitionDuration;
        private bool playSound;

        public PinkGameboyColorGradeTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            flagToSet = data.Attr(nameof(flagToSet), "pink_gameboy_activated");
            colorGradeName = data.Attr(nameof(colorGradeName), "pinkgameboy");
            triggerOnce = data.Bool(nameof(triggerOnce), true);
            transitionDuration = data.Float(nameof(transitionDuration), 0.5f);
            playSound = data.Bool(nameof(playSound), true);
            hasTriggered = false;
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (!triggerOnce || !hasTriggered)
            {
                applyColorGrade();
                hasTriggered = true;
            }
        }

        private void applyColorGrade()
        {
            var level = Scene as Level;
            if (level != null)
            {
                // Apply the color grade transition
                level.NextColorGrade(colorGradeName, transitionDuration);

                // Set the session flag
                level.Session.SetFlag(flagToSet, true);

                // Play a subtle sound effect if enabled
                if (playSound)
                {
                    Audio.Play("event:/game/general/cassette_bubblereturn", Position);
                }

                // Optional: Add a subtle flash effect to emphasize the transition
                level.Flash(Color.Pink * 0.2f);
            }
        }
    }
}




