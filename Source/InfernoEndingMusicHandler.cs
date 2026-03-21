using System.Text.RegularExpressions;

namespace MaggyHelper.Entities
{
    /// <summary>
    /// Handles cross-fading music layers during the ending sequence of
    /// Chapter 7 (Infernal Reflections). As the player progresses through
    /// matching rooms, layer 1 fades out and layer 5 fades in.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/InfernoEndingMusicHandler")]
    [Tracked]
    public class InfernoEndingMusicHandler : Entity
    {
        private HashSet<string> levels = new HashSet<string>();
        private float startX;
        private float endX;
        private string startLevel;
        private string endLevel;
        private string roomPattern;
        private string musicEvent;
        private int fadeOutLayer;
        private int fadeInLayer;

        public InfernoEndingMusicHandler(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            startLevel = data.Attr("startLevel", "e-01");
            endLevel = data.Attr("endLevel", "e-09");
            roomPattern = data.Attr("roomPattern", "e-*");
            musicEvent = data.Attr("music", "event:/music/lvl5/mirror");
            fadeOutLayer = data.Int("fadeOutLayer", 1);
            fadeInLayer = data.Int("fadeInLayer", 5);

            Tag = (int)Tags.TransitionUpdate | (int)Tags.Global;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            Regex regex = new Regex(
                Regex.Escape(roomPattern).Replace("\\*", ".*") + "$");

            Level level = scene as Level;

            foreach (LevelData levelData in level.Session.MapData.Levels)
            {
                if (levelData.Name.Equals(startLevel))
                    startX = levelData.Bounds.Left;
                else if (levelData.Name.Equals(endLevel))
                    endX = levelData.Bounds.Right;

                if (regex.IsMatch(levelData.Name))
                    levels.Add(levelData.Name);
            }
        }

        public override void Update()
        {
            base.Update();

            Level level = Scene as Level;
            CelestePlayer player = Scene.Tracker.GetEntity<CelestePlayer>();

            if (player != null && levels.Contains(level.Session.Level)
                && Audio.CurrentMusic == musicEvent)
            {
                float progress = Calc.Clamp(
                    (player.X - startX) / (endX - startX), 0f, 1f);

                level.Session.Audio.Music.Layer(fadeOutLayer, 1f - progress);
                level.Session.Audio.Music.Layer(fadeInLayer, progress);
                level.Session.Audio.Apply(forceSixteenthNoteHack: false);
            }
        }
    }
}
