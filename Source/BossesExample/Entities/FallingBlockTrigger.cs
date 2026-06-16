// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.FallingBlockTrigger
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

#nullable disable
namespace Celeste.Mod.DZ.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/FallingBlockTrigger"})]
[Tracked(false)]
internal class FallingBlockTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
  public override void OnEnter(Player player)
  {
    Level level = ((Entity) this).SceneAs<Level>();
    if (level.Session.GetFlag("c-08-fallen"))
      return;
    level.Session.SetFlag("c-08-fallen", true);
  }
}
