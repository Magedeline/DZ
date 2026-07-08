// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.Shockwave
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

#nullable disable
namespace Celeste.Mod.DZ.BossesExample.Entities;

internal class Shockwave : Entity
{
  private Sprite shockwave;
  private float speed;
  private bool flipped;
  private Level level;
  private float maintainHeightTime;
  private ConquerorBoss boss;
  private bool disappeared;
  private bool disappearing;

  public Shockwave()
    : base(Vector2.Zero)
  {
    this.Add((Component) (this.shockwave = global::Celeste.Mod.DZ.BossesExample.BossesExampleModule.SpriteBank.Create(nameof (shockwave))));
    this.Collider = (Collider) new Hitbox(10f, 40f, -10f, -20f);
    this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer), (Collider) null, (Collider) null));
    this.Depth = -12600;
    this.maintainHeightTime = 1f;
  }

  public Shockwave Init(ConquerorBoss boss, Player player, bool flipped)
  {
    this.speed = 0.0f;
    this.boss = boss;
    this.Position = ((Entity) boss).Center;
    this.Y += 10f;
    if (this.flipped = flipped)
    {
      this.Collider.Position.X += 8f;
      ((GraphicsComponent) this.shockwave).Scale.X *= -1f;
    }
    this.shockwave.Play("shockwave", false, false);
    return this;
  }

  public override void Added(Scene scene)
  {
    base.Added(scene);
    this.level = this.SceneAs<Level>();
  }

  public override void Removed(Scene scene)
  {
    base.Removed(scene);
    this.level = (Level) null;
  }

  public override void Update()
  {
    base.Update();
    this.X += (float) ((double) this.speed * (double) Engine.DeltaTime * (this.flipped ? -1.0 : 1.0));
    this.speed = Calc.Approach(this.speed, 200f, 285f * Engine.DeltaTime);
    if ((double) this.maintainHeightTime > 0.0)
      this.maintainHeightTime -= Engine.DeltaTime;
    else
      this.Add((Component) new Coroutine(this.disappearCoroutine(), true));
    if ((double) this.X < (double) this.level.Camera.Right + 40.0 && (double) this.X > (double) this.level.Camera.Left - 40.0 && !this.disappeared && !this.boss.dead)
      return;
    this.RemoveSelf();
  }

  public IEnumerator disappearCoroutine()
  {
    this.shockwave.Play("disappear", false, false);
    this.disappearing = true;
    yield return (object) 0.1f;
    this.disappeared = true;
  }

  private void OnPlayer(Player player)
  {
    if (this.disappearing)
      return;
    this.boss.PlayerTakeDamage(player);
  }
}
