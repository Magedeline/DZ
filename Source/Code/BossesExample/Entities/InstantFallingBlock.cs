// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.InstantFallingBlock
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/InstantFallingBlock"})]
internal class InstantFallingBlock : FallingBlock
{
  private EntityID entityID;
  private bool manualTrigger;

  public InstantFallingBlock(EntityData data, Vector2 offset, EntityID entId)
    : base(data, offset)
  {
    this.entityID = entId;
    this.manualTrigger = data.Bool(nameof (manualTrigger), false);
  }

  public override void Added(Scene scene)
  {
    ((Entity) this).Added(scene);
    if (this.manualTrigger)
    {
      if (!((Entity) this).SceneAs<Level>().Session.GetFlag("c-08-fallen"))
        return;
      this.Triggered = true;
    }
    else if ((scene as Level).Session.GetFlag("DoNotLoad" + this.entityID.ToString()))
      this.Triggered = true;
  }

  public override void OnShake(Vector2 amount)
  {
    base.OnShake(amount);
    if (this.manualTrigger)
      return;
    (((Entity) this).Scene as Level).Session.SetFlag("DoNotLoad" + this.entityID.ToString(), true);
  }
}
