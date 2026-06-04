namespace Celeste.Entities;

    public class MeteoriteBlock : Entity
{
    public Vector2 Shake;
    public Vector2 Start;
    public Vector2 End;
    public TileGrid Tilegrid;
    public SoundSource ShakingSfx;
    public string LevelFlags;
    private readonly int surfaceSoundIndex;

    public MeteoriteBlock(Vector2 position, int width, int height, Vector2 node)
    {
        this.Start = position;
        this.End = node;
        this.Depth = -10501;
        this.surfaceSoundIndex = 4;
        this.Add((Component)(this.Tilegrid = GFX.FGAutotiler.GenerateBox('3', width / 8, height / 8).TileGrid));
        this.Add((Component)(this.ShakingSfx = new SoundSource()));
    }

    public MeteoriteBlock(EntityData data, Vector2 offset)
      : this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset)
    {
        this.LevelFlags = data.Attr("flags");
        string str = data.Attr("tiletype");
        if (!string.IsNullOrEmpty(str))
        {
            this.surfaceSoundIndex = SurfaceIndex.TileToIndex[str[0]];
            this.Remove((Component)this.Tilegrid);
            this.Add((Component)(this.Tilegrid = GFX.FGAutotiler.GenerateBox(str[0], data.Width / 8, data.Height / 8).TileGrid));
        }
        this.Add((Component)new TileInterceptor(this.Tilegrid, false));
    }

    public override void Added(Scene scene)
    {
        Level level = scene as Level;
        if (level.Session.Area.GetLevelSet() == nameof(Celeste) || string.IsNullOrEmpty(this.LevelFlags))
        {
            this.orig_Added(scene);
        }
        else
        {
            base.Added(scene);
            if (this.LevelFlags.Split(',').Any(flag => level.Session.GetLevelFlag(flag)))
                this.Position = this.End;
            else
                this.Add((Component)new Coroutine(this.sequence()));
        }
    }
    
    public override void Update()
    {
        this.Tilegrid.Position = this.Shake;
        base.Update();
    }

    private IEnumerator sequence()
    {
        global::Celeste.Player entity1;
        do
        {
            yield return null;
            entity1 = this.Scene.Tracker.GetEntity<global::Celeste.Player>();
        }
        while (entity1 == null || entity1.X < this.X + 30.0 || entity1.X > this.Right + 8.0);
        this.ShakingSfx.Play("guid://{388260ad-8996-4220-a041-4165704072d4}");
        float time = 1.2f;
        Shaker shaker = new Shaker(time, true, v => this.Shake = v);
        this.Add((Component)shaker);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        for (; time > 0.0; time -= Engine.DeltaTime)
        {
            global::Celeste.Player entity2 = this.Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (entity2 != null && (entity2.X >= this.X + this.Width - 8.0 || entity2.X < this.X + 28.0))
            {
                shaker.RemoveSelf();
                break;
            }
            yield return null;
        }
        shaker = null;
        for (int index = 2; index < this.Width; index += 4)
        {
            this.SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(this.X + index, this.Y), Vector2.One * 4f, 1.57079637f);
            this.SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(this.X + index, this.Y), Vector2.One * 4f);
        }
        this.ShakingSfx.Param("release", 1f);
        time = 0.0f;
        do
            if (true)
            {
                {
                    yield return null;
                    time = Calc.Approach(time, 1f, 2f * Engine.DeltaTime);
                    this.moveTo(Vector2.Lerp(this.Start, this.End, Ease.CubeIn(time)));
                }
            }
        while (time < 1.0);
        for (int index = 0; index <= this.Width; index += 4)
        {
            this.SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(this.X + index, this.Bottom), Vector2.One * 4f, -1.57079637f);
            float direction = index >= this.Width / 2.0 ? 0.0f : 3.14159274f;
            this.SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(this.X + index, this.Bottom), Vector2.One * 4f, direction);
        }
        this.ShakingSfx.Stop();
        Audio.Play("guid://{0c8ca90b-c14e-41ca-8233-ec0f74a6aab5}", this.Position);
        this.SceneAs<Level>().Shake();
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        this.Add((Component)new Shaker(0.25f, true, v => this.Shake = v));
    }

    private void moveTo(Vector2 lerp)
    {
        var movement = lerp - Position;
        Position = lerp;
        Tilegrid.Position += movement;
    }

    private void orig_Added(Scene scene)
    {
        base.Added(scene);
        if (this.SceneAs<Level>().Session.GetLevelFlag("1") || this.SceneAs<Level>().Session.GetLevelFlag("0b"))
            this.Position = this.End;
        else
            this.Add((Component)new Coroutine(this.sequence()));
    }
}

