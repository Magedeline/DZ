using Celeste.Cutscenes;

namespace Celeste.Entities;
[CustomEntity(ids: "MaggyHelper/RoofTopEnding")]
[Tracked]
public class ResortRoofEnding : Solid
    {
        private MTexture[] roofCenters = new MTexture[4]
        {
            GFX.Game["decals/3-resort/roofCenter"],
            GFX.Game["decals/3-resort/roofCenter_b"],
            GFX.Game["decals/3-resort/roofCenter_c"],
            GFX.Game["decals/3-resort/roofCenter_d"]
        };
        private List<Image> images = new List<Image>();
        private List<Coroutine> wobbleRoutines = new List<Coroutine>();
        public bool BeginFalling;

        public ResortRoofEnding(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, 2f, true)
        {
            EnableAssistModeChecks = false;
            Image image1 = new Image(GFX.Game["decals/3-resort/roofEdge_d"]);
            image1.CenterOrigin();
            image1.X = 8f;
            image1.Y = 4f;
            Add(image1);
            int num;
            for (num = 0; num < (double) Width; num += 16)
            {
                Image image2 = new Image(Calc.Random.Choose(roofCenters));
                image2.CenterOrigin();
                image2.X = num + 8;
                image2.Y = 4f;
                Add(image2);
                images.Add(image2);
            }
            Image image3 = new Image(GFX.Game["decals/3-resort/roofEdge"]);
            image3.CenterOrigin();
            image3.X = num + 8;
            image3.Y = 4f;
            Add(image3);
            images.Add(image3);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if ((Scene as Level).Session.GetFlag("oshiroEndingalt"))
                return;
            global::Celeste.Player entity = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (entity == null)
                return;
            Scene.Add(new CS05_Ending(this, entity));
        }

        public override void Render()
        {
            Position += Shake;
            base.Render();
            Position -= Shake;
        }

        public void Wobble(AngyOshiro ghost, bool fall = false)
        {
            foreach (Component wobbleRoutine in wobbleRoutines)
                wobbleRoutine.RemoveSelf();
            wobbleRoutines.Clear();
            global::Celeste.Player entity = Scene.Tracker.GetEntity<global::Celeste.Player>();
            foreach (Image image in images)
            {
                // Use ghost.Position.X instead of ghost.X
                float ghostDist = ghost != null ? Math.Abs(X + image.X - ghost.Position.X) * (1f / 1000f) : 0f;
                Coroutine coroutine = new Coroutine(wobbleImage(image, ghostDist, entity, fall));
                Add(coroutine);
                wobbleRoutines.Add(coroutine);
            }
        }

        private IEnumerator wobbleImage(Image img, float delay, global::Celeste.Player player, bool fall)
        {
            ResortRoofEnding resortRoofEnding = this;
            float orig = img.Y;
            yield return delay;
            for (int index = 0; index < 2; ++index)
                resortRoofEnding.Scene.Add(Engine.Pooler.Create<Debris>().Init(resortRoofEnding.Position + img.Position + new Vector2(index * 8 - 4, Calc.Random.Range(0, 8)), '9'));
            float p1;
            float amount;
            if (!fall)
            {
                p1 = 0.0f;
                amount = 5f;
                while (true)
                {
                    p1 += Engine.DeltaTime * 16f;
                    amount = Calc.Approach(amount, 1f, Engine.DeltaTime * 5f);
                    float num = (float) Math.Sin(p1) * amount;
                    img.Y = orig + num;
                    if (player != null && Math.Abs(resortRoofEnding.X + img.X - player.X) < 16.0)
                        player.Sprite.Y = num;
                    yield return null;
                }
            }
            if (fall)
            {
                while (!resortRoofEnding.BeginFalling)
                {
                    int num = Calc.Random.Range(0, 2);
                    img.Y = orig + num;
                    if (player != null && Math.Abs(resortRoofEnding.X + img.X - player.X) < 16.0)
                        player.Sprite.Y = num;
                    yield return 0.01f;
                }
                img.Texture = GFX.Game["decals/3-resort/roofCenter_snapped_" + Calc.Random.Choose("a", "b", "c")];
                resortRoofEnding.Collidable = false;
                amount = Calc.Random.NextFloat();
                p1 = Calc.Random.NextFloat(48f) - 24f;
                float speedY = (float) -(80.0 + Calc.Random.NextFloat(80f));
                float up = new Vector2(0.0f, -1f).Angle();
                float off = Calc.Random.NextFloat();
                for (float p2 = 0.0f; p2 < 4.0; p2 += Engine.DeltaTime)
                {
                    Image image = img;
                    image.Position += new Vector2(p1, speedY) * Engine.DeltaTime;
                    img.Rotation += amount * Ease.CubeIn(p2);
                    p1 = Calc.Approach(p1, 0.0f, Engine.DeltaTime * 200f);
                    speedY += 600f * Engine.DeltaTime;
                if (resortRoofEnding.Scene.OnInterval(0.1f, off))
                    Dust.Burst(resortRoofEnding.Position + img.Position, up, 1);
            }
        }
        player.Sprite.Y = 0.0f;
    }
            }




