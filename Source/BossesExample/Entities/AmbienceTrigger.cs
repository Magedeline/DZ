// Decompiled with JetBrains decompiler
// Type: Celeste.Mod.ricky06ModPack.Entities.AmbienceTrigger
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/AmbienceTrigger"})]
[Tracked(false)]
internal class AmbienceTrigger : Trigger
{
  public string ambience = "event:/new_content/env/10_rain";

  public AmbienceTrigger(EntityData data, Vector2 offset)
    : base(data, offset)
  {
    this.ambience = data.Attr(nameof (ambience), "");
  }

  public virtual void OnEnter(Player player)
  {
    base.OnEnter(player);
    Session session = ((Entity) this).SceneAs<Level>().Session;
    session.Audio.Ambience.Event = SFX.EventnameByHandle(this.ambience);
    session.Audio.Apply();
  }
}
