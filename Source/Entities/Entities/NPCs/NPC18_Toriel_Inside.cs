namespace Celeste.NPCs
{
    [Tracked(true)]
    [CustomEntity(ids: "DZ/NPC18_Toriel_Inside")]
    public class Npc18TorielInside : NPC
    {
        public const string DoorConversationAvailable = "toriel_door";
        private const string DoorConversationDone = "toriel_door_done";
        private const string CounterFlag = "toriel";

        private int conversation;
        private const int MaxConversation = 4;

        public Hahaha Hahaha;

        private global::Celeste.Player player;
        private TalkComponent talker;
        private bool talking;
        private Coroutine talkRoutine;

        private bool HasDoorConversation => Level.Session.GetFlag(DoorConversationAvailable) && !Level.Session.GetFlag(DoorConversationDone);

        private bool talkerEnabled => (conversation > 0 && conversation < MaxConversation) || HasDoorConversation;

        public Npc18TorielInside(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(Sprite = GFX.SpriteBank.Create("toriel"));
            Sprite.Play("idle");
            MoveAnim = "walk";
            Maxspeed = TorielMaxSpeed;
            Add(talker = new TalkComponent(new Rectangle(-20, -8, 40, 8), new Vector2(0f, -24f), OnTalk));
            talker.Enabled = false;
            SetupTorielSpriteSounds();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            conversation = Level.Session.GetCounter(CounterFlag);
            scene.Add(Hahaha = new Hahaha(Position + new Vector2(8f, -4f)));
            Hahaha.Enabled = false;
        }

        public override void Update()
        {
            if (!talking && conversation == 0)
            {
                player = Level.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null && Math.Abs(player.X - X) < 48f)
                {
                    OnTalk(player);
                }
            }
            talker.Enabled = talkerEnabled;
            Hahaha.Enabled = Sprite.CurrentAnimationID == "laugh";
            base.Update();
        }

        private void OnTalk(global::Celeste.Player player)
        {
            this.player = player;
            (Scene as Level).StartCutscene(EndTalking);
            Add(talkRoutine = new Coroutine(TalkRoutine(player)));
            talking = true;
        }

        private IEnumerator TalkRoutine(global::Celeste.Player player)
        {
            player.StateMachine.State = global::Celeste.Player.StDummy;
            player.Dashes = 1;
            player.ForceCameraUpdate = true;
            while (!player.OnGround())
            {
                yield return null;
            }
            yield return player.DummyWalkToExact((int)X - 16);
            player.Facing = Facings.Right;
            player.ForceCameraUpdate = false;
            Vector2 zoomPoint = new Vector2(X - 8f - Level.Camera.X, 110f);
            if (HasDoorConversation)
            {
                Sprite.Scale.X = -1f;
                yield return Level.ZoomTo(zoomPoint, 2f, 0.5f);
                yield return Textbox.Say("DZ_CH18_Toriel_Locked");
            }
            else if (conversation == 0)
            {
                yield return 0.5f;
                Sprite.Scale.X = -1f;
                yield return 0.25f;
                yield return Level.ZoomTo(zoomPoint, 2f, 0.5f);
                yield return Textbox.Say("DZ_CH18_Toriel_A", StartLaughing, StopLaughing);
            }
            else if (conversation == 1)
            {
                Sprite.Scale.X = -1f;
                yield return Level.ZoomTo(zoomPoint, 2f, 0.5f);
                yield return Textbox.Say("DZ_CH18_Toriel_B", StartLaughing, StopLaughing);
            }
            else if (conversation == 2)
            {
                Sprite.Scale.X = -1f;
                yield return Level.ZoomTo(zoomPoint, 2f, 0.5f);
                yield return Textbox.Say("DZ_CH18_Toriel_C", StartLaughing, StopLaughing);
            }
            else if (conversation == 3)
            {
                Sprite.Scale.X = -1f;
                yield return Level.ZoomTo(zoomPoint, 2f, 0.5f);
                yield return Textbox.Say("DZ_CH18_Toriel_D", StartLaughing, StopLaughing);
            }
            talker.Enabled = talkerEnabled;
            yield return Level.ZoomBack(0.5f);
            Level.EndCutscene();
            EndTalking(Level);
        }

        private IEnumerator StartLaughing()
        {
            Sprite.Play("laugh");
            yield return null;
        }

        private IEnumerator StopLaughing()
        {
            Sprite.Play("idle");
            yield return null;
        }

        private void EndTalking(Level level)
        {
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal;
                player.ForceCameraUpdate = false;
            }
            if (HasDoorConversation)
            {
                Level.Session.SetFlag(DoorConversationDone);
            }
            else
            {
                Level.Session.IncrementCounter(CounterFlag);
                conversation++;
            }
            if (talkRoutine != null)
            {
                talkRoutine.RemoveSelf();
                talkRoutine = null;
            }
            Sprite.Play("idle");
            talking = false;
        }
    }
}

