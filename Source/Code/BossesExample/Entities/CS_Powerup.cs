// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.CS_Powerup
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

#pragma warning disable CS0618 // Engine.TimeRate is obsolete - decompiled third-party code

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

public class CS_Powerup : CutsceneEntity
{
  private NPC_Boss boss;
  private Player player;
  private BadelineDummy badeline;
  private Level level;

  public CS_Powerup(Player player, NPC cbboss)
    : base(true, false)
  {
    this.boss = (NPC_Boss) cbboss;
    this.player = player;
  }

  public override void Added(Scene scene)
  {
    base.Added(scene);
    this.level = ((Entity) this).SceneAs<Level>();
  }

  public override void OnBegin(Level level)
  {
    ((Entity) this).Add((Component) new Coroutine(this.Cutscene(level), true));
  }

  public IEnumerator Cutscene(Level level)
  {
    while (this.player == null)
    {
      this.player = ((Entity) this).Scene.Tracker.GetEntity<Player>();
      yield return (object) null;
    }
    this.player.StateMachine.State = Player.StDummy;
    this.player.StateMachine.Locked = true;
    this.player.ForceCameraUpdate = true;
    ((Entity) this.player).Position = Calc.Floor(((Entity) this.player).Position);
    this.player.DummyGravity = true;
    this.player.DummyAutoAnimate = false;
    this.player.DummyFriction = true;
    while (!((Actor) this.player).OnGround(1) || (double) this.player.Speed.Y < 0.0)
      yield return (object) null;
    ((Entity) this).Add((Component) new Coroutine(this.LevelZoom(new Vector2(150f, 104f), 1.5f, 1.5f), true));
    Player player = this.player;
    Rectangle bounds = level.Bounds;
    double num = (double) bounds.Left + 160.0;
    yield return (object) player.DummyRunTo((float) num, true);
    ((Sprite) this.player.Sprite).Play("tired", false, false);
    yield return (object) 2f;
    ((Scene) level).Add((Entity) (this.badeline = new BadelineDummy(((Entity) this.player).Position)));
    level.Displacement.AddBurst(((Entity) this.player).Position, 0.5f, 8f, 48f, 0.5f, (Ease.Easer) null, (Ease.Easer) null);
    Input.Rumble((RumbleStrength) 0, (RumbleLength) 1);
    this.player.CreateSplitParticles();
    global::Celeste.Audio.Play("event:/char/badeline/maddy_split");
    Vector2 start2 = ((Entity) this.player).Position;
    ((GraphicsComponent) this.badeline.Sprite).Scale.X = 1f;
    this.badeline.FloatSpeed = 480f;
    this.badeline.FloatAccel = 960f;
    yield return (object) this.badeline.FloatTo(((start2) + (new Vector2(-20f, -5f))), new int?(), false, false, false);
    yield return (object) Textbox.Say("CP_BOSS_POWERUP_1", Array.Empty<Func<IEnumerator>>());
    yield return (object) level.ZoomAcross(new Vector2(135f, 100f), 1.2f, 0.1f);
    yield return (object) 0.2f;
    Engine.TimeRate = 0.5f;
    ((Entity) this).Add((Component) new Coroutine(this.BossAttack(), true));
    ((Entity) this).Add((Component) new Coroutine(this.CameraPan(), true));
    yield return (object) 0.1f;
    ((Entity) this).Add((Component) new Coroutine(this.BadelineSlam(), true));
    yield return (object) 0.01f;
    level.Shake(0.3f);
    this.player.Speed.X = 500f;
    ((Sprite) this.player.Sprite).Play("roll", false, false);
    this.player.DummyFriction = false;
    while ((double) this.player.Speed.X != 0.0)
    {
      this.player.Speed.X = Calc.Approach(this.player.Speed.X, 0.0f, 120f * Engine.DeltaTime);
      if (((Entity) this).Scene.OnInterval(0.1f))
        Dust.BurstFG(((Entity) this.player).Position, -1.57079637f, 2, 4f, (ParticleType) null);
      yield return (object) null;
    }
    while ((double) Engine.TimeRate < 1.0)
    {
      Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, 4f * Engine.DeltaTime);
      yield return (object) null;
    }
    this.player.DummyFriction = true;
    yield return (object) 2f;
    ((Sprite) this.player.Sprite).Play("rollGetUp", false, false);
    this.badeline.FloatSpeed = 120f;
    this.badeline.FloatAccel = 240f;
    yield return (object) this.badeline.FloatTo(((((Entity) this.player).Position) + (new Vector2(20f, -16f))), new int?(), true, false, false);
    yield return (object) 0.2f;
    yield return (object) Textbox.Say("CP_BOSS_POWERUP_2", Array.Empty<Func<IEnumerator>>());
    yield return (object) 0.8f;
    ((Sprite) this.player.Sprite).Play("idle", false, false);
    yield return (object) Textbox.Say("CP_BOSS_POWERUP_3", new Func<IEnumerator>[1]
    {
      new Func<IEnumerator>(this.sigh_trigger_0)
    });
    global::Celeste.Audio.Play("event:/new_content/char/badeline/maddy_join_quick", ((Entity) this.badeline).Position);
    Vector2 from = ((Entity) this.badeline).Position;
    for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime / 0.25f)
    {
      ((Entity) this.badeline).Position = Vector2.Lerp(from, ((Entity) this.player).Position, Ease.CubeIn.Invoke(p));
      yield return (object) null;
    }
    this.Level.Displacement.AddBurst(((Entity) this.player).Center, 0.5f, 8f, 32f, 0.5f, (Ease.Easer) null, (Ease.Easer) null);
    this.Level.Session.Inventory.Dashes = 2;
    this.player.Dashes = 2;
    ((Entity) this.badeline).RemoveSelf();
    yield return (object) level.ZoomBack(1f);
    this.EndCutscene(level, true);
  }

  private IEnumerator CameraPan()
  {
    yield return (object) 0.19f;
    yield return (object) this.level.ZoomAcross(new Vector2(184f, 100f), 1.2f, 0.15f);
  }

  private IEnumerator LevelZoom(Vector2 focus, float zoom, float duration)
  {
    yield return (object) this.level.ZoomTo(focus, zoom, duration);
  }

  private IEnumerator sigh_trigger_0()
  {
    yield return (object) 1f;
  }

  private IEnumerator BossAttack()
  {
    bool flag = true;
    ((GraphicsComponent) this.boss.Sprite).Scale.X *= -1f;
    ((Entity) this.boss).Visible = true;
    this.boss.Sprite.Play("xcharge", false, false);
    global::Celeste.Audio.Play("event:/ricky06/FightSFX/cb-horizontal-charge");
    yield return (object) 0.19f;
    this.boss.Sprite.Play("xattack", false, false);
    global::Celeste.Audio.Play("event:/ricky06/FightSFX/cb-horizontal-dash-fast");
    this.boss.lightningVisible = true;
    this.boss.lightning.Play("once", true, false);
    for (float p = 0.0f; (double) p < 0.5; p += Engine.DeltaTime)
    {
      Rectangle bounds;
      int num1;
      if (flag)
      {
        double x = (double) ((Entity) this.boss).Position.X;
        bounds = this.level.Bounds;
        double num2 = (double) bounds.Right - 76.0;
        num1 = x >= num2 ? 1 : 0;
      }
      else
        num1 = 0;
      if (num1 != 0)
      {
        this.level.Flash(((Color.White) * (0.8f)), false);
        this.level.Shake(0.3f);
        foreach (DashBlock db in ((Entity) this).Scene.Tracker.GetEntities<DashBlock>())
          db.Break(((Entity) this.boss).Position, new Vector2(((Entity) this.boss).Position.X + 10f, ((Entity) this.boss).Position.Y), true, true);
        flag = false;
        this.level.Session.Audio.Music.Event = "event:/ricky06/cpostsilence";
        this.level.Session.Audio.Apply(false);
      }
      ref Vector2 local = ref ((Entity) this.boss).Position;
      double x1 = (double) ((Entity) this.boss).Position.X;
      bounds = this.level.Bounds;
      double num3 = (double) bounds.Left + 400.0;
      double num4 = (double) Calc.Approach((float) x1, (float) num3, 15f);
      local.X = (float) num4;
      yield return (object) null;
    }
  }

  private IEnumerator BadelineSlam()
  {
    for (float p = 0.0f; (double) p < 0.10000000149011612; p += Engine.DeltaTime)
    {
      if (((Entity) this).Scene.OnInterval(0.01f))
        TrailManager.Add((Entity) this.badeline, Color.Purple, 1f, false, false);
      if ((double) p > 0.0099999997764825821)
      {
        ref Vector2 local = ref ((Entity) this.badeline).Position;
        double y = (double) ((Entity) this.badeline).Position.Y;
        Rectangle bounds = this.level.Bounds;
        double num1 = (double) bounds.Top + 50.0;
        double num2 = (double) Calc.Approach((float) y, (float) num1, 5f);
        local.Y = (float) num2;
      }
      ref Vector2 local1 = ref ((Entity) this.badeline).Position;
      double x = (double) ((Entity) this.badeline).Position.X;
      Rectangle bounds1 = this.level.Bounds;
      double num3 = (double) bounds1.Left + 260.0;
      double num4 = (double) Calc.Approach((float) x, (float) num3, 10f);
      local1.X = (float) num4;
      yield return (object) null;
    }
  }

  public override void OnEnd(Level level)
  {
    foreach (DashBlock entity in ((Entity) this).Scene.Tracker.GetEntities<DashBlock>())
    {
      entity.Break(((Entity) this).Position, ((Entity) this).Position, false, false);
      ((Entity) entity).RemoveSelf();
    }
    if (this.player == null)
      this.player = ((Entity) this).Scene.Tracker.GetEntity<Player>();
    this.player.StateMachine.Locked = false;
    this.player.StateMachine.State = Player.StNormal;
    if (this.WasSkipped)
      ((Entity) this.player).Position = level.Session.RespawnPoint.Value;
    this.player.Speed.Y = 0.0f;
    while (((Entity) this.player).CollideCheck<Solid>())
    {
      Player player = this.player;
      ((Entity) player).Y = ((Entity) player).Y - 1f;
    }
    level.Camera.Position = this.player.CameraTarget;
    this.Level.Session.Inventory.Dashes = 2;
    this.player.Dashes = 2;
    if (this.badeline != null)
      ((Entity) this.badeline).RemoveSelf();
    level.Session.Audio.Music.Event = "event:/ricky06/cpostsilence";
    level.Session.Audio.Apply(false);
    ((Entity) this.boss).RemoveSelf();
    level.ResetZoom();
    level.Session.SetFlag("boss_powerup_cutscene", true);
  }

  public override void Update() => base.Update();
}

