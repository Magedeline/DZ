namespace Celeste.Entities
{
    /// <summary>
    /// Payphone entity that allows player to talk to Chara instead of Badeline
    /// Based on vanilla Celeste payphone but modified for Ingeste
    /// Replaces all Badeline references with Chara for story consistency
    /// </summary>
    [Tracked]
    [CustomEntity(ids: "MaggyHelper/Payphone")]
    public class Payphone : Entity
    {
        private const string ACTIVATED_FLAG = "payphone_activated";

        private Sprite sprite;
        private TalkComponent talk;
        private SoundSource loopSfx;
        private bool activated;
        private string dialogId;
        private string flagToSet;
        private bool onlyOnce;
        
        // Public properties for cutscene access
        public Sprite Sprite => sprite;
        public bool Broken { get; set; }
        public Vector2 Blink = Vector2.Zero;

        public Payphone(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            dialogId = data.Attr("dialogId", "CH2_DREAM_PHONECALL_TRAP");
            flagToSet = data.Attr("flagToSet", "");
            onlyOnce = data.Bool("onlyOnce", true);

            Depth = 8999;

            setupSprite();
            setupComponents();
        }

        private void setupSprite()
        {
            Add(sprite = new Sprite(GFX.Game, "cutscenes/payphone/"));
            sprite.AddLoop("idle", "phone", 0.1f, 0);
            sprite.AddLoop("ringing", "phone", 0.1f, 0);
            sprite.Add("pickUp", "phone", 0.08f, "idle", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
            sprite.AddLoop("talkPhone", "phone", 0.08f, 11);
            sprite.Add("hangUp", "phone", 0.08f, "idle", 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0);
            sprite.Add("jumpBack", "phone", 0.08f, "idle", 12, 13, 14, 15, 16, 17);
            sprite.Add("scare", "phone", 0.08f, "idle", 18, 19, 20, 21);
            sprite.Add("transform", "phone", 0.08f, "idle", 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45);
            sprite.Add("eat", "phone", 0.08f, "monsterIdle", 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 83, 83, 84, 84, 85, 85, 86, 86, 87, 87, 74, 75, 76, 77, 78, 79, 80, 81, 82);
            sprite.AddLoop("monsterIdle", "phone", 0.2f, 83, 84, 85, 86, 87);
            sprite.Play("idle");
        }

        private void setupComponents()
        {
            Add(talk = new TalkComponent(
                new Rectangle(-16, -8, 32, 16),
                new Vector2(0f, -24f),
                onTalk
            ));

            Add(loopSfx = new SoundSource());
        }

        private void onTalk(global::Celeste.Player player)
        {
            if (Scene is Level level)
            {
                if (onlyOnce && level.Session.GetFlag(ACTIVATED_FLAG))
                {
                    return;
                }

                level.StartCutscene(onTalkEnd);
                Add(new Coroutine(talkCoroutine(player)));
            }
        }

        private IEnumerator talkCoroutine(global::Celeste.Player player)
        {
            player.StateMachine.State = global::Celeste.Player.StDummy;

            // Answer the phone
            sprite.Play("pickUp");
            yield return 0.5f;

            // Play dialog
            yield return Textbox.Say(dialogId);

            // Hang up
            sprite.Play("hangUp");
            yield return 0.3f;

            endCutscene();
        }

        private void endCutscene()
        {
            if (Scene is Level level)
            {
                level.EndCutscene();
                onTalkEnd(level);
            }
        }

        private void onTalkEnd(Level level)
        {
            if (!string.IsNullOrEmpty(flagToSet))
            {
                level.Session.SetFlag(flagToSet, true);
            }

            if (onlyOnce)
            {
                level.Session.SetFlag(ACTIVATED_FLAG, true);
                talk.Enabled = false;
            }

            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                player.StateMachine.State = global::Celeste.Player.StNormal;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (Scene is Level level && onlyOnce && level.Session.GetFlag(ACTIVATED_FLAG))
            {
                talk.Enabled = false;
            }
        }

        public override void Update()
        {
            base.Update();

            if (!activated && sprite.CurrentAnimationID == "idle")
            {
                // Phone ringing logic could go here
            }
        }

        public override void Removed(Scene scene)
        {
            loopSfx?.Stop();
            base.Removed(scene);
        }
    }
}



