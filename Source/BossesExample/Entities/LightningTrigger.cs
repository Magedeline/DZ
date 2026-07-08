// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.LightningTrigger
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

#nullable disable
namespace Celeste.Mod.DZ.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/LightningTrigger"})]
[Tracked(false)]
internal class LightningTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
  private bool triggered = false;
  private Level level;

  public override void Added(Scene scene)
  {
    ((Entity) this).Added(scene);
    this.level = ((Entity) this).SceneAs<Level>();
    if (!this.level.Session.GetFlag("cp_lightning_trigger_1"))
      return;
    ((Entity) this).RemoveSelf();
  }

  public override void OnEnter(Player player)
  {
    if (this.triggered)
      return;
    base.OnEnter(player);
    global::Celeste.Audio.Play("event:/new_content/game/10_farewell/lightning_strike");
    this.level.Flash(Color.White, false);
    this.level.Shake(0.3f);
    Level level1 = this.level;
    double num1 = (double) ((Entity) player).X + 60.0;
    Rectangle bounds1 = this.level.Bounds;
    double num2 = (double) (bounds1.Bottom - 180);
    LightningStrike lightningStrike1 = new LightningStrike(new Vector2((float) num1, (float) num2), 10, 200f, 0.0f);
    ((Scene) level1).Add((Entity) lightningStrike1);
    Level level2 = this.level;
    double num3 = (double) ((Entity) player).X + 220.0;
    Rectangle bounds2 = this.level.Bounds;
    double num4 = (double) (bounds2.Bottom - 180);
    LightningStrike lightningStrike2 = new LightningStrike(new Vector2((float) num3, (float) num4), 40, 200f, 0.25f);
    ((Scene) level2).Add((Entity) lightningStrike2);
    this.triggered = true;
    this.level.Session.SetFlag("cp_lightning_trigger_1", true);
  }
}

