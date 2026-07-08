// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.DoubleDashTrigger
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

#nullable disable
namespace Celeste.Mod.DZ.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/DoubleDashTrigger"})]
[Tracked(false)]
internal class DoubleDashTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
  private Level level;

  public override void Added(Scene scene)
  {
    ((Entity) this).Added(scene);
    this.level = ((Entity) this).SceneAs<Level>();
  }

  public override void OnEnter(Player player) => this.level.Session.Inventory.Dashes = 2;
}
