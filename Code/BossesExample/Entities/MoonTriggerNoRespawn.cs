// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.MoonTriggerNoRespawn
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/MoonTriggerNoRespawn"})]
[Tracked(false)]
internal class MoonTriggerNoRespawn : MoonGlitchBackgroundTrigger
{
  public EntityID id;

  public MoonTriggerNoRespawn(EntityData data, Vector2 offset, EntityID entId)
    : base(data, offset)
  {
    this.id = entId;
  }

  public override void OnEnter(Player player)
  {
    EntityID id = this.id;
    Session session = (((Entity) this).Scene as Level).Session;
    if (session.GetFlag("DoNotLoad" + id.ToString()))
      return;
    base.OnEnter(player);
    session.SetFlag("DoNotLoad" + id.ToString(), true);
  }
}
