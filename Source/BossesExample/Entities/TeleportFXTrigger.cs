// Decompiled with JetBrains decompiler
// Type: Celeste.Mod.ricky06ModPack.Entities.TeleportFXTrigger
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

#pragma warning disable CS0618 // Engine.TimeRate is obsolete - decompiled third-party code

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/TeleportFXTrigger"})]
[Tracked(false)]
internal class TeleportFXTrigger : Trigger
{
  private string color;
  private float speed;
  private float slowest;
  private Trigger.PositionModes PositionMode;

  public TeleportFXTrigger(EntityData data, Vector2 offset)
    : base(data, offset)
  {
    this.color = data.Attr(nameof (color), "");
    this.speed = data.Float(nameof (speed), 1f);
    this.slowest = data.Float(nameof (slowest), 1f);
    this.PositionMode = data.Enum<Trigger.PositionModes>("positionMode", (Trigger.PositionModes) 0);
  }

  public override void OnEnter(Player player)
  {
    base.OnEnter(player);
    if (this.color.Length == 0)
      this.color = (string) null;
    this.SceneAs<Level>().NextColorGrade(this.color, this.speed);
  }

  public override void OnStay(Player player)
  {
    base.OnStay(player);
    Engine.TimeRate = MathHelper.Lerp(1f, this.slowest, this.GetPositionLerp(player, this.PositionMode));
  }

  public override void OnLeave(Player player)
  {
    base.OnLeave(player);
    this.Remove();
  }

  public void Remove() => this.RemoveSelf();
}
