namespace Celeste.Triggers
{
    [CustomEntity("musicTrigger")]
    public class MusicTrigger : Trigger
    {
        private string track;
        private bool resetOnLeave;
        private int progress;

        public MusicTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            track = data.Attr(nameof(track), "");
            resetOnLeave = data.Bool(nameof(resetOnLeave), true);
            progress = data.Int(nameof(progress), 0);
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);
            
            if (!string.IsNullOrEmpty(track))
            {
                var level = Scene as Level;
                if (level != null)
                {
                    if (track == "none" || track == "null")
                    {
                        Audio.SetMusic(null);
                    }
                    else
                    {
                        Audio.SetMusic(track, true, true);
                        if (progress > 0)
                        {
                            Audio.SetMusicParam(nameof(progress), progress);
                        }
                    }
                }
            }
        }

        public override void OnLeave(global::Celeste.Player player)
        {
            base.OnLeave(player);
            
            if (resetOnLeave)
            {
                var level = Scene as Level;
                if (level != null)
                {
                    // Reset to level's default music - simplified
                    var session = level.Session;
                    if (session != null)
                    {
                        var areaData = AreaData.Get(session.Area);
                        if (areaData != null)
                        {
                            // Simplified - just set to null to reset
                            Audio.SetMusic(null);
                        }
                    }
                }
            }
        }
    }
}



