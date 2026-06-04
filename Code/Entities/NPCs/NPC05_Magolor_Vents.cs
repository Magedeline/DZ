namespace Celeste.NPCs
{
    [CustomEntity(ids: "MaggyHelper/NPC05_Magolor_Vents")]
    public class NPC05_Magolor_Vents : NPC
    {
        protected new TalkComponent Talker;
        
        private class Grate : Entity
        {
            private Image sprite;

            private float shake;

            private Vector2 speed;

            private bool falling;

            private float alpha = 1f;

            public Grate(Vector2 position)
                : base(position)
            {
                Add(sprite = new Image(GFX.Game["scenery/grate"]));
                sprite.JustifyOrigin(0.5f, 0f);
            }

            public void Shake()
            {
                if (!falling)
                {
                    Audio.Play("guid://{6fb99d9c-74ca-412d-b3b0-247b39a382df}", Position);
                    shake = 0.5f;
                }
            }

            public void Fall()
            {
                Audio.Play("guid://{5220fbf8-61c9-4a15-8490-8571a7cfa54f}", Position);
                falling = true;
                speed = new Vector2(40f, 200f);
                base.Collider = new Hitbox(2f, 2f, -1f);
            }

            public override void Update()
            {
                if (shake > 0f)
                {
                    shake -= Engine.DeltaTime;
                    if (base.Scene.OnInterval(0.05f))
                    {
                        sprite.X = 1f - sprite.X;
                    }
                }
                if (falling)
                {
                    speed.X = Calc.Approach(speed.X, 0f, Engine.DeltaTime * 80f);
                    speed.Y += 200f * Engine.DeltaTime;
                    Position += speed * Engine.DeltaTime;
                    if (CollideCheck<Solid>(Position + new Vector2(0f, 2f)) && speed.Y > 0f)
                    {
                        speed.Y = (0f - speed.Y) * 0.25f;
                    }
                    alpha -= Engine.DeltaTime;
                    sprite.Rotation += Engine.DeltaTime;
                    sprite.Color = Color.White * alpha;
                    if (alpha <= 0f)
                    {
                        RemoveSelf();
                    }
                }
                base.Update();
            }
        }

        private const string AppeardFlag = "magolorVentsAppeared";

        private const string TalkedFlag = "magolorVentsTalked";

        private const int SpriteAppearY = -8;

        private float particleDelay;

        private bool appeared;

        private Grate grate;

        public NPC05_Magolor_Vents(Vector2 position)
            : base(position)
        {
            base.Tag = Tags.TransitionUpdate;
            Add(Sprite = GFX.SpriteBank.Create("magolor"));
            Sprite.Scale.Y = -1f;
            Sprite.Scale.X = -1f;
            Visible = false;
            Maxspeed = 48f;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (base.Session.GetFlag("magolorVentsTalked"))
            {
                RemoveSelf();
            }
            else
            {
                Add(new Coroutine(Appear()));
            }
        }

        public override void Update()
        {
            base.Update();
            if (appeared)
            {
                return;
            }
            particleDelay -= Engine.DeltaTime;
            if (particleDelay <= 0f)
            {
                Level.ParticlesFG.Emit(ParticleTypes.VentDust, 8, Position, new Vector2(6f, 0f));
                if (grate != null)
                {
                    grate.Shake();
                }
                particleDelay = Calc.Random.Choose(1f, 2f, 3f);
            }
        }

        private IEnumerator Appear()
        {
            if (!Session.GetFlag("magolorVentsAppeared"))
            {
                grate = new Grate(Position);
                Scene.Add(grate);
                global::Celeste.Player entity;
                do
                {
                    yield return null;
                    entity = Scene.Tracker.GetEntity<global::Celeste.Player>();
                }
                while (entity == null || !(entity.X > X - 32f));
                Audio.Play("guid://{f37cd8d3-4249-47ca-8243-28ae26cf7c5c}", Position);
                Level.ParticlesFG.Emit(ParticleTypes.VentDust, 24, Position, new Vector2(6f, 0f));
                grate.Fall();
                int from = -24;
                for (float p = 0f; p < 1f; p += Engine.DeltaTime * 2f)
                {
                    yield return null;
                    Visible = true;
                    Sprite.Y = (float)from + (float)(-8 - from) * Ease.CubeOut(p);
                }
                Session.SetFlag("magolorVentsAppeared");
            }
            appeared = true;
            Sprite.Y = -8f;
            Visible = true;
            Add(Talker = new TalkComponent(new Rectangle(-16, 0, 32, 100), new Vector2(0f, -8f), OnTalk));
        }

        private void OnTalk(global::Celeste.Player player)
        {
            Level.StartCutscene(OnTalkEnd);
            Add(new Coroutine(Talk(player)));
        }

        private IEnumerator Talk(global::Celeste.Player player)
        {
            yield return PlayerApproach(player, turnToFace: true, 10f, -1);
            player.DummyAutoAnimate = false;
            player.Sprite.Play("lookUp");
            yield return CutsceneEntity.CameraTo(new Vector2(Level.Bounds.Right - 320, Level.Bounds.Top), 0.5f);
            yield return Level.ZoomTo(new Vector2(240f, 70f), 2f, 0.5f);
            yield return Textbox.Say("MAGGYHELPER_CH5_MAGGY_VENTS");
            yield return Disappear();
            yield return 0.25f;
            yield return Level.ZoomBack(0.5f);
            Level.EndCutscene();
            OnTalkEnd(Level);
        }

        private void OnTalkEnd(Level level)
        {
            global::Celeste.Player entity = base.Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
            {
                entity.DummyAutoAnimate = true;
                entity.StateMachine.Locked = false;
                entity.StateMachine.State = 0;
            }
            base.Session.SetFlag("magolorVentsTalked");
            RemoveSelf();
        }

        private IEnumerator Disappear()
        {
            Audio.Play("guid://{d3ade5b6-5418-4d18-8ac3-b31f1f5a9581}", Position);
            int to = -24;
            float from = Sprite.Y;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 2f)
            {
                yield return null;
                Level.ParticlesFG.Emit(ParticleTypes.VentDust, 1, Position, new Vector2(6f, 0f));
                Sprite.Y = from + ((float)to - from) * Ease.BackIn(p);
            }
        }
    }
}





