namespace Celeste.Entities;
[CustomEntity(ids: "MaggyHelper/HeartDoorMod")]
internal class HeartGemDoor : Entity
{
    private MTexture temp = new MTexture();
    private Particle[] particles = new Particle[50];
    private const string opened_flag = "opened_heartgem_mod_door_";
    private int requires;
    private int size;
    private readonly float openDistance;
    private float openPercent;
    private Solid topSolid;
    private Solid botSolid;
    private float offset;
    private Vector2 mist;
    private List<MTexture> icon;

    private int heartGems
    {
        get
        {
            if (SaveData.Instance.CheatMode)
                return this.requires;
            return SaveData.Instance.TotalHeartGems;
        }
    }

    private float counter { get; set; }

    private bool opened { get; set; }

    private float openAmount => this.openPercent * this.openDistance;

    public HeartGemDoor(EntityData data, Vector2 offset)
      : base(data.Position + offset)
    {
        this.requires = data.Int(nameof(requires), 0);
        this.Add((Component)new CustomBloom(new Action(this.renderBloom)));
        this.size = data.Width;
        this.openDistance = 32f;
        Vector2? nullable = data.FirstNodeNullable(new Vector2?(offset));
        if (nullable.HasValue)
            this.openDistance = Math.Abs(nullable.Value.Y - this.Y);
        this.icon = GFX.Game.GetAtlasSubtextures("objects/heart_spear_door_mod/icon");
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Level level1 = scene as Level;
        for (int index = 0; index < this.particles.Length; ++index)
        {
            this.particles[index].Position = new Vector2(Calc.Random.NextFloat((float)this.size), Calc.Random.NextFloat((float)level1.Bounds.Height));
            this.particles[index].Speed = (float)Calc.Random.Range(4, 12);
            this.particles[index].Color = Color.White * Calc.Random.Range(0.2f, 0.6f);
        }
        Level level2 = level1;
        double x = (double)this.X;
        Rectangle bounds = level1.Bounds;
        double topY = (double)(bounds.Top - 32);
        Vector2 position1 = new Vector2((float)x, (float)topY);
        double size1 = (double)this.size;
        double y = (double)this.Y;
        bounds = level1.Bounds;
        double top = (double)bounds.Top;
        double topSolidHeight = y - top + 32.0;
        Solid solid1 = this.topSolid = new Solid(position1, (float)size1, (float)topSolidHeight, true);
        level2.Add((Entity)solid1);
        this.topSolid.SurfaceSoundIndex = 32;
        Level level3 = level1;
        Vector2 position2 = new Vector2(this.X, this.Y);
        double size2 = (double)this.size;
        bounds = level1.Bounds;
        double botSolidHeight = (double)bounds.Bottom - (double)this.Y + 32.0;
        Solid solid2 = this.botSolid = new Solid(position2, (float)size2, (float)botSolidHeight, true);
        level3.Add((Entity)solid2);
        this.botSolid.SurfaceSoundIndex = 32;
        if ((this.Scene as Level).Session.GetFlag("opened_heartgem_mod_door_" + (object)this.requires))
        {
            this.opened = true;
            this.openPercent = 1f;
            this.counter = (float)this.requires;
            this.topSolid.Y -= this.openDistance;
            this.botSolid.Y += this.openDistance;
        }
        else
            this.Add(new Coroutine(this.Routine(), true));
    }

    public IEnumerator Routine()
    {
        Level level = this.Scene as Level;
        while (!this.opened && this.counter < this.requires)
        {
            global::Celeste.Player player = this.Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && Math.Abs(player.X - this.Center.X) < 80.0)
            {                if (this.counter == 0.0f && this.heartGems > 0)
                    Audio.Play("event:/pusheen/game/18_heart/frontdoor_heartfill", this.Position);
                if (this.heartGems < this.requires)
                    level.Session.SetFlag("toriel_door", false);
                int was = (int)this.counter;
                int target = Math.Min(this.heartGems, this.requires);
                this.counter = Calc.Approach(this.counter, target, Engine.DeltaTime * this.requires * 0.8f);
                if (was != (int)this.counter)
                {
                    yield return 0.1f;                    if (this.counter < target)
                        Audio.Play("event:/pusheen/game/18_heart/frontdoor_heartfill", this.Position);
                }
            }
            else
                this.counter = Calc.Approach(this.counter, 0.0f, Engine.DeltaTime * this.requires * 4.0f);
            yield return null;
        }
        yield return 0.5f;
        this.Scene.Add(new WhiteLine(this.Position, this.size));
        level.Shake(0.3f);
        level.Flash(Color.White * 0.5f, false);
        Audio.Play("event:/pusheen/game/18_heart/frontdoor_unlock", this.Position);
        this.opened = true;
        level.Session.SetFlag("opened_heartgem_mod_door_" + this.requires, true);
        this.offset = 0.0f;
        yield return 0.6f;
        float topFrom = this.topSolid.Y;
        float topTo = this.topSolid.Y - this.openDistance;
        float botFrom = this.botSolid.Y;
        float botTo = this.botSolid.Y + this.openDistance;
        for (float p = 0.0f; p < 1.0f; p += Engine.DeltaTime)
        {
            level.Shake(0.3f);
            this.openPercent = Ease.CubeIn(p);
            this.topSolid.MoveToY(MathHelper.Lerp(topFrom, topTo, this.openPercent));
            this.botSolid.MoveToY(MathHelper.Lerp(botFrom, botTo, this.openPercent));
            if (p >= 0.4f && level.OnInterval(0.1f))
            {
            }
            yield return null;
        }
        this.topSolid.MoveToY(topTo);
        this.botSolid.MoveToY(botTo);
        this.openPercent = 1f;
    }

    public override void Update()
    {
        base.Update();
        if (this.opened)
            return;
        this.offset += 12f * Engine.DeltaTime;
        this.mist.X -= 4f * Engine.DeltaTime;
        this.mist.Y -= 24f * Engine.DeltaTime;
        for (int index = 0; index < this.particles.Length; ++index)
            this.particles[index].Position.Y += this.particles[index].Speed * Engine.DeltaTime;
    }

    private void renderBloom()
    {
        if (this.opened)
            return;
        this.drawBloom(new Rectangle((int)this.topSolid.X, (int)this.topSolid.Y, this.size, (int)((double)this.topSolid.Height + (double)this.botSolid.Height)));
    }

    private void drawBloom(Rectangle bounds)
    {
        Draw.Rect((float)(bounds.Left - 4), (float)bounds.Top, 2f, (float)bounds.Height, Color.White * 0.25f);
        Draw.Rect((float)(bounds.Left - 2), (float)bounds.Top, 2f, (float)bounds.Height, Color.White * 0.5f);
        Draw.Rect(bounds, Color.White * 0.75f);
        Draw.Rect((float)bounds.Right, (float)bounds.Top, 2f, (float)bounds.Height, Color.White * 0.5f);
        Draw.Rect((float)(bounds.Right + 2), (float)bounds.Top, 2f, (float)bounds.Height, Color.White * 0.25f);
    }

    private void drawMist(Rectangle bounds, Vector2 mist)
    {
        Color color = Color.White * 0.6f;
        MTexture mtexture = GFX.Game["objects/heart_spear_door_mod/iso"];
        int val11 = mtexture.Width / 2;
        int val12 = mtexture.Height / 2;
        for (int index1 = 0; index1 < bounds.Width; index1 += val11)
        {
            for (int index2 = 0; index2 < bounds.Height; index2 += val12)
            {
                mtexture.GetSubtexture((int)this.mod(mist.X, (float)val11), (int)this.mod(mist.Y, (float)val12), Math.Min(val11, bounds.Width - index1), Math.Min(val12, bounds.Height - index2), this.temp);
                this.temp.Draw(new Vector2((float)(bounds.X + index1), (float)(bounds.Y + index2)), Vector2.Zero, color);
            }
        }
    }

    private void drawInterior(Rectangle bounds)
    {
        Draw.Rect(bounds, Calc.HexToColor("18668f"));
        this.drawMist(bounds, this.mist);
        this.drawMist(bounds, new Vector2(this.mist.Y, this.mist.X) * 1.5f);
        Vector2 vector21 = (this.Scene as Level).Camera.Position;
        if (this.opened)
            vector21 = Vector2.Zero;
        for (int index = 0; index < this.particles.Length; ++index)
        {
            Vector2 vector22 = this.particles[index].Position + vector21 * 0.2f;
            vector22.X = this.mod(vector22.X, (float)bounds.Width);
            vector22.Y = this.mod(vector22.Y, (float)bounds.Height);
            Draw.Pixel.Draw(new Vector2((float)bounds.X, (float)bounds.Y) + vector22, Vector2.Zero, this.particles[index].Color);
        }
    }

    private void drawEdges(Rectangle bounds, Color color)
    {
        MTexture mtexture1 = GFX.Game["objects/heartdoor/edge"];
        MTexture mtexture2 = GFX.Game["objects/heartdoor/top"];
        int height = (int)((double)this.offset % 8.0);
        if (height > 0)
        {
            mtexture1.GetSubtexture(0, 8 - height, 7, height, this.temp);
            this.temp.DrawJustified(new Vector2((float)(bounds.Left + 4), (float)bounds.Top), new Vector2(0.5f, 0.0f), color, new Vector2(-1f, 1f));
            this.temp.DrawJustified(new Vector2((float)(bounds.Right - 4), (float)bounds.Top), new Vector2(0.5f, 0.0f), color, new Vector2(1f, 1f));
        }
        for (int index = 0; index < bounds.Height; index += 8)
        {
            mtexture1.GetSubtexture(0, 0, 8, Math.Min(8, bounds.Height - index), this.temp);
            this.temp.DrawJustified(new Vector2((float)(bounds.Left + 4), (float)(bounds.Top + index + height)), new Vector2(0.5f, 0.0f), color, new Vector2(-1f, 1f));
            this.temp.DrawJustified(new Vector2((float)(bounds.Right - 4), (float)(bounds.Top + index + height)), new Vector2(0.5f, 0.0f), color, new Vector2(1f, 1f));
        }
        if (!this.opened)
            return;
        for (int index = 0; index < bounds.Width; index += 8)
        {
            mtexture2.DrawCentered(new Vector2((float)(bounds.Left + 4 + index), (float)(bounds.Top + 4)), color);
            mtexture2.DrawCentered(new Vector2((float)(bounds.Left + 4 + index), (float)(bounds.Bottom - 4)), color, new Vector2(1f, -1f));
        }
    }

    public override void Render()
    {
        Color color = this.opened ? Color.White * 0.25f : Color.White;
        if (!this.opened)
        {
            Rectangle bounds = new Rectangle((int)this.topSolid.X, (int)this.topSolid.Y, this.size, (int)((double)this.topSolid.Height + (double)this.botSolid.Height));
            this.drawInterior(bounds);
            this.drawEdges(bounds, color);
        }
        else
        {
            Rectangle bounds1 = new Rectangle((int)this.topSolid.X, (int)this.topSolid.Y, this.size, (int)this.topSolid.Height);
            this.drawInterior(bounds1);
            this.drawEdges(bounds1, color);
            Rectangle bounds2 = new Rectangle((int)this.botSolid.X, (int)this.botSolid.Y, this.size, (int)this.botSolid.Height);
            this.drawInterior(bounds2);
            this.drawEdges(bounds2, color);
        }
        float iconSpacing = 12f;
        int iconsPerRow = (int)((double)(this.size - 8) / (double)iconSpacing);
        int rowCount = (int)Math.Ceiling((double)this.requires / (double)iconsPerRow);
        for (int index1 = 0; index1 < rowCount; ++index1)
        {
            int iconsThisRow = (index1 + 1) * iconsPerRow < this.requires ? iconsPerRow : this.requires - index1 * iconsPerRow;
            Vector2 vector2 = new Vector2(this.X + (float)this.size * 0.5f, this.Y) + new Vector2((float)((double)-iconsThisRow / 2.0 + 0.5), (float)((double)-rowCount / 2.0 + (double)index1 + 0.5)) * iconSpacing;
            if (this.opened)
            {
                if (index1 < rowCount / 2)
                    vector2.Y -= this.openAmount + 8f;
                else
                    vector2.Y += this.openAmount + 8f;
            }
            for (int index2 = 0; index2 < iconsThisRow; ++index2)
            {
                int iconIndex = index1 * iconsPerRow + index2;
                this.icon[(int)((double)Ease.CubeIn(Calc.ClampedMap(this.counter, (float)iconIndex, (float)iconIndex + 1f, 0.0f, 1f)) * (double)(this.icon.Count - 1))].DrawCentered(vector2 + new Vector2((float)index2 * iconSpacing, 0.0f), color);
            }
        }
    }

    private float mod(float x, float m)
    {
        return (x % m + m) % m;
    }

    private struct Particle
    {
        public Vector2 Position;
        public float Speed;
        public Color Color;
    }

    private class WhiteLine : Entity
    {
        private float fade = 1f;
        private int blockSize;

        public WhiteLine(Vector2 origin, int blockSize)
          : base(origin)
        {
            this.Depth = -1000000;
            this.blockSize = blockSize;
        }

        public new void Update()
        {
            base.Update();
            fade = Calc.Approach(fade, 0.0f, Engine.DeltaTime);
            if (fade <= 0.0f)
            {
                RemoveSelf();
                Level level = SceneAs<Level>();
                for (float left = level.Camera.Left; left < level.Camera.Right; left++)
                {
                    if (left < X || left >= X + blockSize)
                    {
                    }
                }
            }
        }

        public override void Render()
        {
            Vector2 position = (this.Scene as Level).Camera.Position;
            float height = Math.Max(1f, 4f * this.fade);
            Draw.Rect(position.X - 10f, this.Y - height / 2f, 340f, height, Color.White);
        }
    }
}




