// Decompiled with JetBrains decompiler
// Type: Celeste.Mod.ricky06ModPack.Entities.TeleporterDeco
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\AppData\Local\Temp\Mihavec\86958f101c\Code\ricky06ModPack.dll

#nullable disable
namespace MaggyHelper.Entities
{
  [CustomEntity(new string[] { "MaggyHelper/TeleporterDeco" })]
  internal class TeleporterDeco : Entity
  {
    private Level level;
    public static ParticleType PDissipate = new ParticleType()
    {
      Color = Calc.HexToColor("272b4a"),
      Size = 1f,
      FadeMode = ParticleType.FadeModes.Late,
      SpeedMin = 5f,
      SpeedMax = 10f,
      DirectionRange = 1.04719758f,
      LifeMin = 1f,
      LifeMax = 1.5f
    };
    private string colorHex;
    public EntityID Eid;

    public TeleporterDeco(EntityData data, Vector2 offset, EntityID id)
      : base(data.Position + offset)
    {
      this.Add((Component) new VertexLight(Color.White, 1f, 16, 32));
      this.Add((Component) new BloomPoint(0.1f, 16f));
      this.Visible = false;
      this.colorHex = data.Attr("color");
      this.Eid = id;
    }

    public override void Added(Scene scene)
    {
      base.Added(scene);
      this.level = this.SceneAs<Level>();
      if (this.level.Session.GetFlag("DoNotLoad" + this.Eid.ToString()))
        this.RemoveSelf();
      this.Visible = true;
      TeleporterDeco.PDissipate.Color = Calc.HexToColor(this.colorHex);
    }

    public override void Update()
    {
      base.Update();
      if (this.Scene.OnInterval(1f))
        this.level.Displacement.AddBurst(this.Center, 1f, 0.0f, 40f);
      if (!this.Scene.OnInterval(0.05f))
        return;
      float direction = Calc.Random.NextAngle();
      this.SceneAs<Level>().Particles.Emit(TeleporterDeco.PDissipate, 5, this.Center, Vector2.One * 2f, direction);
    }
  }
}




