// Decompiled with JetBrains decompiler
// Type: Celeste.Mod.ricky06ModPack.Entities.ConquerorLightningStrike
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

internal class ConquerorLightningStrike : Entity
{
  private Sprite strike;
  private ConquerorBoss boss;
  private Level level;
  public static ParticleType P_Dissipate = new ParticleType(FinalBossBeam.P_Dissipate);

  public ConquerorLightningStrike()
  {
    this.Add((Component) (this.strike = global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.SpriteBank.Create("boss_lightning_strike")));
    this.Collider = (Collider) new Hitbox(20f, 0.0f, 20f, 0.0f);
    this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer), (Collider) null, (Collider) null));
    ((GraphicsComponent) this.strike).Scale.Y *= 1.05f;
  }

  public virtual void Added(Scene scene)
  {
    base.Added(scene);
    this.level = this.SceneAs<Level>();
  }

  public ConquerorLightningStrike Init(Vector2 pos, ConquerorBoss b)
  {
    this.boss = b;
    this.Position = pos;
    this.Add((Component) new Coroutine(this.StrikeCoroutine(), true));
    return this;
  }

  public virtual void Update()
  {
    base.Update();
    if (!this.boss.dead)
      return;
    this.RemoveSelf();
  }

  private IEnumerator StrikeCoroutine()
  {
    this.strike.Play("aim", false, false);
    yield return (object) 0.6f;
    this.strike.Play("strike", false, false);
    Audio.Play("event:/ricky06/FightSFX/cb-lightning");
    for (int i = 0; i < 4; ++i)
    {
      this.Collider.Height += 40f;
      yield return (object) 0.02f;
    }
    this.level.Shake(0.3f);
    this.Collider.Height = 0.0f;
    yield return (object) 0.1f;
    this.RemoveSelf();
  }

  private void OnPlayer(Player player) => this.boss.PlayerTakeDamage(player);
}
