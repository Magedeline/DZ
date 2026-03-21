using MaggyHelper.Cutscenes;

namespace MaggyHelper.NPCs;
    [CustomEntity(ids: "MaggyHelper/NPC05_Magolor_Escaping")]
    public class NPC05_Magolor_Escaping : NPC
{
    // Stub for SetupTheoSpriteSounds - sets up sprite sounds for Theo-like characters
    protected new void SetupTheoSpriteSounds()
    {
        // Sprite sound setup - can be empty stub or add actual sound setup
    }
    
    public class Grate : Entity
    {
        public Image Sprite;

        private Vector2 speed;

        private bool falling;

        private float alpha = 1f;

        public Grate(Vector2 position)
            : base(position)
        {
            Add(Sprite = new Image(GFX.Game["scenery/grate"]));
            Sprite.JustifyOrigin(0.5f, 0f);
            Sprite.Rotation = (float)Math.PI / 2f;
        }

        public void Fall()
        {
            Audio.Play("event:/char/theo/resort_vent_tumble", Position);
            falling = true;
            speed = new Vector2(-120f, -120f);
            base.Collider = new Hitbox(2f, 2f, -2f, -1f);
        }
        public override void Update()
        {
            if (falling)
            {
                speed.X = Calc.Approach(speed.X, 0f, Engine.DeltaTime * 120f);
                speed.Y += 400f * Engine.DeltaTime;
                Position += speed * Engine.DeltaTime;
                if (CollideCheck<Solid>(Position + new Vector2(0f, 2f)) && speed.Y > 0f)
                {
                    speed.Y = (0f - speed.Y) * 0.25f;
                }
                alpha -= Engine.DeltaTime;
                Sprite.Rotation += Engine.DeltaTime * speed.Length() * 0.05f;
                Sprite.Color = Color.White * alpha;
                if (alpha <= 0f)
                {
                    RemoveSelf();
                }
            }
            base.Update();
        }
    }

    private bool talked;

    private VertexLight light;

    public Grate grate;

    public NPC05_Magolor_Escaping(Vector2 position)
        : base(position)
    {
        Add(Sprite = GFX.SpriteBank.Create("theo"));
        Sprite.Play("idle");
        Sprite.X = -4f;
        SetupTheoSpriteSounds();
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Add(light = new VertexLight(base.Center - Position, Color.White, 1f, 32, 64));
        while (!CollideCheck<Solid>(Position + new Vector2(1f, 0f)))
        {
            base.X++;
        }
        grate = new Grate(Position + new Vector2(base.Width / 2f, -8f));
        base.Scene.Add(grate);
        Sprite.Play("goToVent");
    }

    public override void Update()
    {
        base.Update();
        global::Celeste.Player player = base.Scene.Entities.FindFirst<global::Celeste.Player>();
        if (player != null && !talked && player.X > base.X - 100f)
        {
            Talk(player);
        }
        if (Sprite.CurrentAnimationID == "pullVent" && Sprite.CurrentAnimationFrame > 0)
        {
            grate.Sprite.X = 0f;
        }
        else
        {
            grate.Sprite.X = 1f;
        }
    }
    private void Talk(global::Celeste.Player player)
    {
        talked = true;
        base.Scene.Add(new CS05_MagolorEscape(this, player));
    }
    public void CrawlUntilOut()
    {
        Sprite.Scale.X = 1f;
        Sprite.Play("crawl");
        Add(new Coroutine(CrawlUntilOutRoutine()));
    }

    private IEnumerator CrawlUntilOutRoutine()
    {
        AddTag(Tags.Global);
        int target = (Scene as Level).Bounds.Right + 280;
        while (X != (float)target)
        {
            X = Calc.Approach(X, target, 20f * Engine.DeltaTime);
            yield return null;
        }
        Scene.Remove(this);
    }
}




