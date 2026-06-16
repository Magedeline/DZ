// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.TeleporterDeco
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

#nullable disable
namespace Celeste.Mod.DZ.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/TeleporterDeco"})]
[Tracked(false)]
internal class TeleporterDeco : Entity
{
  private Level level;
  public static ParticleType P_Dissipate = new ParticleType()
  {
    Color = Calc.HexToColor("272b4a"),
    Size = 1f,
    FadeMode = (ParticleType.FadeModes) 2,
    SpeedMin = 5f,
    SpeedMax = 10f,
    DirectionRange = 1.04719758f,
    LifeMin = 1f,
    LifeMax = 1.5f
  };
  private string colorHex;
  public EntityID eid;

  public TeleporterDeco(EntityData data, Vector2 offset, EntityID id)
    : base(((data.Position) + (offset)))
  {
    this.Add((Component) new VertexLight(Color.White, 1f, 16 /*0x10*/, 32 /*0x20*/));
    this.Add((Component) new BloomPoint(0.1f, 16f));
    this.Visible = false;
    this.colorHex = data.Attr("color", "");
    this.eid = id;
  }

  public override void Added(Scene scene)
  {
    base.Added(scene);
    this.level = this.SceneAs<Level>();
    if (this.level.Session.GetFlag("DoNotLoad" + this.eid.ToString()))
      this.RemoveSelf();
    this.Visible = true;
    TeleporterDeco.P_Dissipate.Color = Calc.HexToColor(this.colorHex);
  }

  public override void Update()
  {
    base.Update();
    if (this.Scene.OnInterval(1f))
      this.level.Displacement.AddBurst(this.Center, 1f, 0.0f, 40f, 1f, (Ease.Easer) null, (Ease.Easer) null);
    if (!this.Scene.OnInterval(0.05f))
      return;
    float num = Calc.NextAngle(Calc.Random);
    this.SceneAs<Level>().Particles.Emit(TeleporterDeco.P_Dissipate, 5, this.Center, ((Vector2.One) * (2f)), num);
  }
}
