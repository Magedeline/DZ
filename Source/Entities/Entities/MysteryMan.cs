using Celeste.Cutscenes;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// The mystery man (Gaster) — an interactable NPC that tells his backstory
    /// as a youth from another reality who once served as the Royal Scientist.
    /// After the dialog concludes, a sound plays and he instantly vanishes.
    /// </summary>
    [CustomEntity("DZ/MysteryMan")]
    [Tracked]
    public class MysteryMan : Entity
    {
        private Sprite sprite;
        private TalkComponent talker;
        private VertexLight light;

        private string dialogKey;
        private string audioEvent;
        private string flagName;
        private bool onlyOnce;
        private bool hasInteracted;

        private static bool sessionInteracted;

        public MysteryMan(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            dialogKey = data.Attr("dialogKey", "DZ_MYSTERYMAN_GASTER");
            audioEvent = data.Attr("audioEvent", "event:/DZ/game/08_edge/mysterygo");
            flagName = data.Attr("flagName", "darkeryetdarker");
            onlyOnce = data.Bool("onlyOnce", true);

            Depth = 100;

            try
            {
                Add(sprite = GFX.SpriteBank.Create("mysteryman"));
                sprite.Play("idle");
            }
            catch
            {
                Logger.Log(LogLevel.Warn, "DZ", "Failed to load 'mysteryman' sprite, using fallback");
            }

            Add(talker = new TalkComponent(
                new Rectangle(-24, -24, 48, 48),
                new Vector2(0f, -24f),
                OnTalk
            ));
            talker.PlayerMustBeFacing = false;

            Add(light = new VertexLight(Color.White, 1f, 16, 32));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (onlyOnce && Scene is Level level)
            {
                bool alreadyDone = sessionInteracted;
                if (!alreadyDone && !string.IsNullOrEmpty(flagName))
                    alreadyDone = level.Session.GetFlag(flagName);

                if (alreadyDone)
                {
                    hasInteracted = true;
                    sessionInteracted = true;
                    Visible = false;
                    Collidable = false;
                    talker.Enabled = false;
                }
            }
        }

        private void OnTalk(global::Celeste.Player player)
        {
            if (player == null || Scene == null || hasInteracted)
                return;

            hasInteracted = true;
            sessionInteracted = true;
            talker.Enabled = false;

            Level level = SceneAs<Level>();
            level.StartCutscene(OnCutsceneEnd);
            Add(new Coroutine(InteractionRoutine(level, player)));
        }

        private IEnumerator InteractionRoutine(Level level, global::Celeste.Player player)
        {
            player.StateMachine.State = global::Celeste.Player.StDummy;

            yield return Textbox.Say(dialogKey);

            Audio.Play(audioEvent, Position);

            sprite?.Play("vanish");

            yield return 0.15f;

            Visible = false;
            Collidable = false;

            OnCutsceneEnd(level);
        }

        private void OnCutsceneEnd(Level level)
        {
            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                player.StateMachine.State = global::Celeste.Player.StNormal;
            }

            level.EndCutscene();

            if (!string.IsNullOrEmpty(flagName))
            {
                level.Session.SetFlag(flagName, true);
            }
        }
    }
}
