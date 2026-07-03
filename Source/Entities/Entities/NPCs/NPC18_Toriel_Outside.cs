namespace Celeste.NPCs
{
    [Tracked(true)]
    [CustomEntity(ids: "DZ/NPC18_Toriel_Outside")]
    public class Npc18TorielOutside : NPC
    {
        public const string Flag = "toriel_outside";

        public Hahaha Hahaha;

        private bool talking;
        private global::Celeste.Player player;
        private bool leaving;

        public Npc18TorielOutside(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(Sprite = DZModule.SpriteBank.Create("toriel"));
            Sprite.Play("idle");
            MoveAnim = "walk";
            IdleAnim = "idle";
            Maxspeed = TorielMaxSpeed;
            SetupTorielSpriteSounds();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if ((scene as Level).Session.GetFlag(Flag))
            {
                RemoveSelf();
            }
            scene.Add(Hahaha = new Hahaha(Position + new Vector2(8f, -4f)));
            Hahaha.Enabled = false;
        }

        public override void Update()
        {
            if (!talking)
            {
                player = Level.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null && player.X > X - 48f)
                {
                    (Scene as Level).StartCutscene(EndTalking);
                    Add(new Coroutine(TalkRoutine(player)));
                    talking = true;
                }
            }
            Hahaha.Enabled = Sprite.CurrentAnimationID == "laugh";
            base.Update();
        }

        private IEnumerator TalkRoutine(global::Celeste.Player player)
        {
            player.StateMachine.State = global::Celeste.Player.StDummy;
            while (!player.OnGround())
            {
                yield return null;
            }
            Sprite.Scale.X = -1f;
            yield return player.DummyWalkToExact((int)X - 16);
            yield return 0.5f;
            yield return Level.ZoomTo(new Vector2(200f, 110f), 2f, 0.5f);
            yield return Textbox.Say("DZ_CH18_Toriel_Outside", MoveRight, ExitRight);
            yield return Level.ZoomBack(0.5f);
            Sprite.Scale.X = 1f;
            if (!leaving)
            {
                yield return ExitRight();
            }
            while (X < (float)(Level.Bounds.Right + 8))
            {
                yield return null;
            }
            Level.EndCutscene();
            EndTalking(Level);
        }

        private IEnumerator MoveRight()
        {
            yield return MoveTo(X + 8f);
        }

        private IEnumerator ExitRight()
        {
            leaving = true;
            Add(new Coroutine(MoveTo(Level.Bounds.Right + 16)));
            yield return null;
        }

        private void EndTalking(Level level)
        {
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal;
            }
            Level.Session.SetFlag(Flag);
            RemoveSelf();
        }
    }
}

