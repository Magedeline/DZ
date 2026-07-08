// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.BossFightTrigger
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

#nullable disable
namespace Celeste.Mod.DZ.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/BossFightTrigger"})]
[Tracked(false)]
internal class BossFightTrigger : Trigger
{
  private Level level;
  private bool firstTime;
  private bool hardMode;

  public BossFightTrigger(EntityData data, Vector2 offset)
    : base(data, offset)
  {
    this.hardMode = data.Bool("hard", false);
  }

  public override void Added(Scene scene)
  {
    base.Added(scene);
    this.level = ((Entity) this).SceneAs<Level>();
  }

  public override void OnEnter(Player player)
  {
    base.OnEnter(player);
    this.level.Session.Inventory.Dashes = 2;
  }

  public override void OnLeave(Player player)
  {
    base.OnLeave(player);
    if (this.firstTime)
      return;
    ((Entity) this).Add((Component) new Coroutine(this.AddBoss(), true));
    Scene scene = ((Entity) this).Scene;
    Rectangle bounds1 = this.level.Bounds;
    double num = (double) bounds1.Left + 296.0;
    Rectangle bounds2 = this.level.Bounds;
    double top = (double) bounds2.Top;
    InvisibleBarrier invisibleBarrier = new InvisibleBarrier(new Vector2((float) num, (float) top), 24f, 180f);
    scene.Add((Entity) invisibleBarrier);
    this.firstTime = true;
  }

  private IEnumerator AddBoss()
  {
    if (!this.level.Session.GetFlag("cb_screech_intro"))
    {
      foreach (NPC_Boss nb in ((Entity) this).Scene.Tracker.GetEntities<NPC_Boss>())
        nb.invokeIntroScreech(this.hardMode);
    }
    else
    {
      Scene scene = ((Entity) this).Scene;
      Rectangle bounds = this.level.Bounds;
      double left = (double) bounds.Left;
      bounds = this.level.Bounds;
      double top = (double) bounds.Top;
      ConquerorBoss conquerorBoss = new ConquerorBoss(new Vector2((float) left, (float) top), 4, false, this.hardMode);
      scene.Add((Entity) conquerorBoss);
      global::Celeste.Audio.SetMusic("event:/ricky06/CP-OST6", true, true);
    }
    yield return (object) null;
  }
}
