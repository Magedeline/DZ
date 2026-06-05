// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.ConquerorBoss
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

#pragma warning disable CS0618 // Engine.TimeRate is obsolete - decompiled third-party code

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/ConquerorBoss"})]
[Tracked(false)]
public class ConquerorBoss : Actor
{
  public Sprite Sprite;
  public Sprite shockwave;
  public Sprite shockwaveLeft;
  public Sprite lightning;
  private bool lightningVisible;
  public Sprite lightningV;
  private bool lightningVVisible;
  public int Phase;
  public bool dead;
  private StateMachine state;
  private float cameraXOffset;
  private float cameraXLeftOffset;
  private float cameraYOffset;
  private Level level;
  private float yApproachSpeed = 300f;
  private float xApproachSpeed = 400f;
  private bool easeBackFromRightEdge;
  private bool easeBackFromBottomEdge;
  private float attackSpeed;
  private bool collidedVertically;
  private int currPhase;
  private bool stopChasing;
  private bool xFlipped;
  private float damageCooldown;
  private float playerDamageCooldown;
  private Color currRed;
  private float health;
  private Hitbox DashCollider;
  private Hitbox BounceCollider;
  private Hitbox BounceCollider2;
  private bool bossAttacking;
  private bool yTop;
  private float beamXLoc;
  private List<float> beamXList;
  private float shakeTimer;
  private HealthBar hp;
  private Sprite chargeWave;
  private int playerHealth;
  private int beamOffsetIndex = 0;
  private int randomSeed;
  private List<float> beamOffset;
  private List<int> phase4Moves;
  private SoundSource laserSfx1;
  private Collision onCollideV;
  private bool lightningRemoved;
  private float timeToTeleportLightning;
  private bool fromCutscene;
  private bool HardMode;
  private bool beginFollowup;
  private bool followupGrounded;
  private bool sawBadelineDialogue;
  private ConquerorHealth chpGUI;
  private float previousSeed = 1f;
  private long startTime;
  public static ParticleType P_Dissipate = new ParticleType()
  {
    Color = Color.White,
    Size = 1f,
    FadeMode = (ParticleType.FadeModes) 2,
    SpeedMin = 30f,
    SpeedMax = 60f,
    DirectionRange = 1.04719758f,
    LifeMin = 0.3f,
    LifeMax = 0.6f
  };

  private float TargetX
  {
    get
    {
      Player entity = ((Scene) this.level).Tracker.GetEntity<Player>();
      if (entity == null)
        return ((Entity) this).X;
      double centerX = (double) ((Entity) entity).CenterX;
      Rectangle bounds1 = this.level.Bounds;
      double num1 = (double) (bounds1.Left + 8);
      Rectangle bounds2 = this.level.Bounds;
      double num2 = (double) (bounds2.Right - 8);
      return MathHelper.Clamp((float) centerX, (float) num1, (float) num2);
    }
  }

  private float TargetY
  {
    get
    {
      Player entity = ((Scene) this.level).Tracker.GetEntity<Player>();
      if (entity == null)
        return ((Entity) this).Y;
      double centerY = (double) ((Entity) entity).CenterY;
      Rectangle bounds1 = this.level.Bounds;
      double num1 = (double) (bounds1.Top + 8);
      Rectangle bounds2 = this.level.Bounds;
      double num2 = (double) (bounds2.Bottom - 8);
      return MathHelper.Clamp((float) centerY, (float) num1, (float) num2);
    }
  }

  public ConquerorBoss(Vector2 position, int phase, bool fromCutscene, bool HardMode)
    : base(position)
  {
    this.fromCutscene = fromCutscene;
    this.HardMode = HardMode;
    this.Phase = phase;
    ((Entity) this).Add((Component) (this.Sprite = global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.SpriteBank.Create("conqueror_boss")));
    ((Entity) this).Add((Component) (this.chargeWave = global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.SpriteBank.Create("beamCharge")));
    ((Entity) this).Add((Component) (this.lightning = global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.SpriteBank.Create("boss_lightning_charge")));
    ((Component) this.lightning).Visible = false;
    this.lightning.OnFinish = (Action<string>) (_param1 => this.lightningVisible = false);
    ((Entity) this).Add((Component) (this.lightningV = global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.SpriteBank.Create("boss_lightningv_charge")));
    ((Component) this.lightningV).Visible = false;
    this.lightningV.OnFinish = (Action<string>) (_param1 => this.lightningVVisible = false);
    ((Entity) this).Collider = (Collider) new Hitbox(30f, 24f, -15f, -12f);
    this.DashCollider = new Hitbox(30f, 24f, -15f, -12f);
    this.BounceCollider = new Hitbox(50f, 5f, -25f, -16f);
    this.BounceCollider2 = new Hitbox(50f, 5f, -25f, 11f);
    this.beamOffset = new List<float>()
    {
      0.0f,
      0.2f,
      0.4f,
      0.6f
    };
    this.phase4Moves = new List<int>()
    {
      1,
      2,
      3,
      4,
      5,
      6,
      7
    };
    this.health = HardMode ? 150f : 80f;
    this.playerHealth = 3;
    this.lightningRemoved = true;
    // ISSUE: method pointer
    this.onCollideV = new Collision(this.OnCollideV);
    this.state = new StateMachine(15);
    this.state.SetCallbacks(0, new Func<int>(this.ChaseXUpdate), new Func<IEnumerator>(this.ChaseCoroutine), new Action(this.ChaseBegin), (Action) null);
    this.state.SetCallbacks(1, new Func<int>(this.WaitingUpdate), (Func<IEnumerator>) null, (Action) null, (Action) null);
    this.state.SetCallbacks(2, new Func<int>(this.AttackXUpdate), (Func<IEnumerator>) null, new Action(this.AttackXBegin), (Action) null);
    this.state.SetCallbacks(3, new Func<int>(this.ChaseYUpdate), new Func<IEnumerator>(this.ChaseCoroutine), new Action(this.ChaseBegin), (Action) null);
    this.state.SetCallbacks(4, new Func<int>(this.AttackYUpdate), (Func<IEnumerator>) null, new Action(this.AttackYBegin), (Action) null);
    this.state.SetCallbacks(5, new Func<int>(this.ShockwaveUpdate), new Func<IEnumerator>(this.ShockwaveCoroutine), new Action(this.ShockwaveBegin), (Action) null);
    this.state.SetCallbacks(6, new Func<int>(this.BeamUpdate), new Func<IEnumerator>(this.BeamCoroutine), new Action(this.BeamBegin), (Action) null);
    this.state.SetCallbacks(7, new Func<int>(this.BossUpdate), new Func<IEnumerator>(this.BossCoroutine), new Action(this.BossBegin), (Action) null);
    this.state.SetCallbacks(8, new Func<int>(this.ChaseXLeftUpdate), new Func<IEnumerator>(this.ChaseCoroutine), new Action(this.ChaseBegin), (Action) null);
    this.state.SetCallbacks(9, new Func<int>(this.AttackXLeftUpdate), (Func<IEnumerator>) null, new Action(this.AttackXBegin), (Action) null);
    this.state.SetCallbacks(10, new Func<int>(this.DummyUpdate), (Func<IEnumerator>) null, new Action(this.DummyBegin), (Action) null);
    this.state.SetCallbacks(11, new Func<int>(this.TeleportXUpdate), new Func<IEnumerator>(this.TeleportXCoroutine), (Action) null, (Action) null);
    this.state.SetCallbacks(12, new Func<int>(this.LightningUpdate), new Func<IEnumerator>(this.LightningCoroutine), new Action(this.LightningBegin), (Action) null);
    this.state.SetCallbacks(13, new Func<int>(this.XFollowupUpdate), new Func<IEnumerator>(this.XFollowupCoroutine), new Action(this.XFollowupBegin), (Action) null);
    ((Entity) this).Depth = this.Phase == 3 || this.Phase == 4 ? -12500 : -12500;
    ((Entity) this).Add((Component) (this.laserSfx1 = new SoundSource()));
    ((Entity) this).Add((Component) this.state);
    ((Entity) this).Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer), ((Entity) this).Collider, (Collider) null));
    ((Entity) this).Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayerDash), (Collider) this.DashCollider, (Collider) null));
    ((Entity) this).Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayerDown), (Collider) this.BounceCollider, (Collider) null));
    ((Entity) this).Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayerUp), (Collider) this.BounceCollider2, (Collider) null));
    ((Entity) this).Add((Component) new TransitionListener()
    {
      OnOutBegin = (Action) (() =>
      {
        if (this.Phase == 1)
        {
          double x = (double) ((Entity) this).X;
          Rectangle bounds = this.level.Bounds;
          double num = (double) bounds.Left + (double) ((Image) this.Sprite).Width / 2.0;
          if (x > num)
            ((Entity) this).Visible = false;
          else
            this.easeBackFromRightEdge = true;
        }
        else
        {
          if (this.Phase != 2)
            return;
          double y = (double) ((Entity) this).Y;
          Rectangle bounds = this.level.Bounds;
          double num = (double) bounds.Left + (double) ((Image) this.Sprite).Height / 2.0;
          if (y > num)
            ((Entity) this).Visible = false;
          else
            this.easeBackFromBottomEdge = true;
        }
      }),
      OnOut = (Action<float>) (_param1 =>
      {
        if (this.Phase == 1)
        {
          ((Component) this.lightning).Update();
          if (!this.easeBackFromRightEdge)
            return;
          ((Entity) this).X = ((Entity) this).X - 128f * Engine.RawDeltaTime;
        }
        else
        {
          if (this.Phase != 2)
            return;
          ((Component) this.lightningV).Update();
          if (this.easeBackFromBottomEdge)
            ((Entity) this).Y = ((Entity) this).Y - 128f * Engine.RawDeltaTime;
        }
      })
    });
  }

  public ConquerorBoss(EntityData data, Vector2 offset)
    : this(((data.Position) + (offset)), data.Int(nameof (Phase), 0), false, data.Bool("hard", false))
  {
  }

  public override void Awake(Scene scene)
  {
    base.Awake(scene);
    this.startTime = ((Entity) this).SceneAs<Level>().Session.Time;
  }

  private void checkShake()
  {
    if (!((Scene) this.level).OnRawInterval(0.04f))
      return;
    PropertyInfo property = this.level.GetType().BaseType.GetProperty("ShakeVector");
    int num = (int) Math.Ceiling((double) this.shakeTimer * 10.0);
    property.SetValue((object) this.level, (object) new Vector2((float) (-num + Calc.Random.Next(num * 2 + 1)), (float) (-num + Calc.Random.Next(num * 2 + 1))));
    this.shakeTimer -= Engine.DeltaTime;
  }

  public override void Update()
  {
    base.Update();
    Engine.TimeRate = 1f;
    if ((double) this.damageCooldown > 0.0)
      this.damageCooldown -= Engine.DeltaTime;
    if ((double) this.playerDamageCooldown > 0.0)
      this.playerDamageCooldown -= Engine.DeltaTime;
    if (((((GraphicsComponent) this.Sprite).Color) != (Color.White)))
      ((Image) this.Sprite).SetColor(this.currRed = Color.Lerp(this.currRed, Color.White, 0.2f));
    if ((double) ((GraphicsComponent) this.Sprite).Scale.Y == 1.0)
      return;
    ((GraphicsComponent) this.Sprite).Scale.X = MathHelper.Lerp(((GraphicsComponent) this.Sprite).Scale.X, (float) (1.0 * (this.xFlipped ? 1.0 : -1.0)), 0.1f);
    ((GraphicsComponent) this.Sprite).Scale.Y = MathHelper.Lerp(((GraphicsComponent) this.Sprite).Scale.Y, 1f, 0.1f);
  }

  public override void Added(Scene scene)
  {
    base.Added(scene);
    this.level = ((Entity) this).SceneAs<Level>();
    if (this.Phase == 1)
      ((Entity) this).Y = this.TargetY;
    else if (this.Phase == 2)
      ((Entity) this).X = this.TargetX;
    this.cameraXOffset = -48f;
    this.cameraXLeftOffset = 48f;
    this.cameraYOffset = -48f;
    this.beamXList = new List<float>() { 40f, 160f, 280f };
    if (this.Phase == 4)
    {
      global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.InitPlayerHealth(3);
      this.hp = new HealthBar(this.playerHealth);
      this.chpGUI = new ConquerorHealth(this.HardMode);
      ((Scene) this.level).Add((Entity) this.hp);
      ((Scene) this.level).Add((Entity) this.chpGUI);
    }
    if (!this.fromCutscene)
      this.state.State = 1;
    else
      this.state.State = 0;
  }

  private int ChaseXUpdate()
  {
    this.cameraXOffset = this.Phase == 4 ? 20f : Calc.Approach(this.cameraXOffset, 20f, 80f * Engine.DeltaTime);
    ((Entity) this).X = this.level.Camera.Left + this.cameraXOffset;
    if (!this.stopChasing && ((Scene) this.level).Tracker.GetEntity<Player>() != null && (this.Phase != 4 || !this.OnGround(1)))
      ((Entity) this).CenterY = Calc.Approach(((Entity) this).CenterY, this.TargetY, this.yApproachSpeed * Engine.DeltaTime);
    return 0;
  }

  private int ChaseXLeftUpdate()
  {
    this.cameraXLeftOffset = this.Phase == 4 ? -20f : Calc.Approach(this.cameraXLeftOffset, -20f, 80f * Engine.DeltaTime);
    ((Entity) this).X = this.level.Camera.Right + this.cameraXLeftOffset;
    if (!this.stopChasing && ((Scene) this.level).Tracker.GetEntity<Player>() != null && (this.Phase != 4 || !this.OnGround(1)))
      ((Entity) this).CenterY = Calc.Approach(((Entity) this).CenterY, this.TargetY, this.yApproachSpeed * Engine.DeltaTime);
    return 8;
  }

  private void ChaseBegin()
  {
    ((Entity) this).Visible = false;
    this.stopChasing = false;
  }

  private IEnumerator ChaseCoroutine()
  {
    if (!this.HardMode)
      yield return (object) 0.45f;
    yield return (object) 0.05f;
    if (this.Phase != 4)
      yield return (object) 0.7f;
    if (this.Phase == 4)
    {
      this.Appear();
      this.stopChasing = true;
      if (!this.HardMode)
        yield return (object) 0.08f;
      yield return (object) 0.1f;
      this.Sprite.Play("ycharge", false, false);
      global::Celeste.Audio.Play("event:/ricky06/FightSFX/cb-vertical-charge");
      if (!this.HardMode)
        yield return (object) 0.07f;
      yield return (object) 0.32f;
    }
    else
    {
      yield return (object) 0.43f;
      this.Appear();
      this.stopChasing = true;
      yield return (object) 0.18f;
      if (this.Phase == 1)
      {
        this.Sprite.Play("xcharge", false, false);
        global::Celeste.Audio.Play("event:/ricky06/FightSFX/cb-horizontal-charge");
        this.lightningVisible = true;
        this.lightning.Play("once", true, false);
        ((GraphicsComponent) this.Sprite).Scale.X *= -1f;
      }
      else
      {
        this.Sprite.Play("ycharge", false, false);
        global::Celeste.Audio.Play("event:/ricky06/FightSFX/cb-vertical-charge");
      }
      yield return (object) 0.39f;
    }
    if (((Entity) this).Scene.Tracker.GetEntity<Player>() != null)
    {
      if (this.Phase == 1 || this.Phase == 4 && (this.currPhase == 1 || this.currPhase == 5))
        this.state.State = !this.xFlipped ? 2 : 9;
      else if (this.Phase == 2 || this.Phase == 4 && (this.currPhase == 2 || this.currPhase == 6))
        this.state.State = 4;
    }
    else
      this.Sprite.Play("idle", false, false);
  }

  private int TeleportXUpdate() => 11;

  private IEnumerator TeleportXCoroutine()
  {
    yield return (object) 0.5f;
    ((Entity) this).X = this.xFlipped ? this.level.Camera.Right - 20f : this.level.Camera.Left + 20f;
    ((Entity) this).Y = this.yTop ? this.level.Camera.Top + 80f : this.level.Camera.Bottom - 40f;
    this.Appear();
    if (!this.HardMode)
      yield return (object) 0.08f;
    yield return (object) 0.1f;
    this.Sprite.Play("xcharge", false, false);
    global::Celeste.Audio.Play("event:/ricky06/FightSFX/cb-horizontal-charge");
    this.lightningVisible = true;
    this.lightning.Play("once", true, false);
    if (this.xFlipped)
    {
      if (!this.HardMode)
        yield return (object) 0.1f;
      yield return (object) 0.29f;
      this.state.State = 9;
    }
    else
    {
      ((GraphicsComponent) this.Sprite).Scale.X *= -1f;
      if (!this.HardMode)
        yield return (object) 0.1f;
      yield return (object) 0.29f;
      this.state.State = 2;
    }
  }

  private int ChaseYUpdate()
  {
    if (!this.stopChasing)
    {
      this.cameraYOffset = this.Phase == 4 ? 28f : Calc.Approach(this.cameraYOffset, 20f, 80f * Engine.DeltaTime);
      ((Entity) this).Y = this.level.Camera.Top + this.cameraYOffset;
      if (((Scene) this.level).Tracker.GetEntity<Player>() != null)
        ((Entity) this).CenterX = this.Phase == 4 ? Calc.Clamp(this.TargetX, this.level.Camera.Left + 60f, this.level.Camera.Right - 60f) : Calc.Approach(((Entity) this).CenterX, this.TargetX, this.xApproachSpeed * Engine.DeltaTime);
    }
    return 3;
  }

  private int AttackYUpdate()
  {
    this.MoveV(this.attackSpeed * Engine.DeltaTime, this.onCollideV, (Solid) null);
    this.attackSpeed = Calc.Approach(this.attackSpeed, 1600f, 8000f * Engine.DeltaTime);
    if (this.collidedVertically)
    {
      global::Celeste.Audio.Play("event:/ricky06/FightSFX/cb-vertical-impact");
      return 5;
    }
    if (((Entity) this).Scene.OnInterval(0.015f))
      TrailManager.Add((Entity) this, ((Color.Purple) * (0.8f)), 1f, false, false);
    if ((double) ((Entity) this).Y < (double) this.level.Camera.Bottom + 48.0)
      return 4;
    this.level.Shake(0.3f);
    Input.Rumble((RumbleStrength) 2, (RumbleLength) 2);
    ((Entity) this).Y = this.level.Camera.Top - 48f;
    this.cameraYOffset = -48f;
    ((Entity) this).Visible = false;
    return this.Phase == 4 ? 7 : 3;
  }

  private int AttackXUpdate()
  {
    ((Entity) this).X = ((Entity) this).X + this.attackSpeed * Engine.DeltaTime;
    this.attackSpeed = this.Phase == 4 ? Calc.Approach(this.attackSpeed, 500f, 3000f * Engine.DeltaTime) : Calc.Approach(this.attackSpeed, 1000f, 4000f * Engine.DeltaTime);
    if ((double) ((Entity) this).X >= (double) this.level.Camera.Right - 100.0 && this.HardMode && this.lightningRemoved)
      return 13;
    if ((double) ((Entity) this).X >= (double) this.level.Camera.Right + 48.0)
    {
      ((Entity) this).X = this.level.Camera.Left - 48f;
      this.cameraXOffset = -48f;
      ((Entity) this).Visible = false;
      ((GraphicsComponent) this.Sprite).Scale.X *= -1f;
      if (this.Phase != 4)
        return 0;
      this.bossAttacking = false;
      return 7;
    }
    if (((Entity) this).Scene.OnInterval(0.05f))
      TrailManager.Add((Entity) this, ((Color.Purple) * (0.8f)), 1f, false, false);
    return 2;
  }

  private int AttackXLeftUpdate()
  {
    ((Entity) this).X = ((Entity) this).X - this.attackSpeed * Engine.DeltaTime;
    this.attackSpeed = this.Phase == 4 ? Calc.Approach(this.attackSpeed, 500f, 3000f * Engine.DeltaTime) : Calc.Approach(this.attackSpeed, 1000f, 4000f * Engine.DeltaTime);
    if ((double) ((Entity) this).X <= (double) this.level.Camera.Left + 100.0 && this.HardMode && this.lightningRemoved)
      return 13;
    if ((double) ((Entity) this).X <= (double) this.level.Camera.Left - 48.0)
    {
      ((Entity) this).X = this.level.Camera.Right + 48f;
      this.cameraXLeftOffset = 48f;
      ((Entity) this).Visible = false;
      if (this.Phase != 4)
        return 8;
      this.bossAttacking = false;
      return 7;
    }
    if (((Entity) this).Scene.OnInterval(0.05f))
      TrailManager.Add((Entity) this, ((Color.Purple) * (0.8f)), 1f, false, false);
    return 9;
  }

  private void AttackXBegin()
  {
    this.attackSpeed = 0.0f;
    this.Sprite.Play("xattack", false, false);
    if (this.Phase != 4)
      global::Celeste.Audio.Play("event:/ricky06/FightSFX/cb-horizontal-dash-fast");
    else
      global::Celeste.Audio.Play("event:/ricky06/FightSFX/cb-horizontal-dash-slow");
  }

  private void AttackYBegin()
  {
    this.attackSpeed = 0.0f;
    this.Sprite.Play("yattack", false, false);
  }

  private void XFollowupBegin()
  {
    this.followupGrounded = false;
    this.beginFollowup = false;
  }

  private int XFollowupUpdate()
  {
    if (this.beginFollowup)
    {
      if (this.yTop)
      {
        if ((double) ((Entity) this).Y >= (double) this.level.Camera.Bottom - 40.0)
          return 5;
        ((Entity) this).Y = Calc.Approach(((Entity) this).Y, this.level.Camera.Bottom - 40f, 10f);
      }
      else if (this.followupGrounded)
        return 7;
    }
    return 13;
  }

  private IEnumerator XFollowupCoroutine()
  {
    if (this.yTop)
    {
      this.Sprite.Play("ycharge", false, false);
      global::Celeste.Audio.Play("event:/ricky06/FightSFX/cb-vertical-charge");
      yield return (object) 0.2f;
      this.beginFollowup = true;
      this.Sprite.Play("yattack", false, false);
      global::Celeste.Audio.Play("event:/ricky06/FightSFX/cb-vertical-impact");
    }
    else
    {
      this.bossAttacking = false;
      this.Sprite.Play("chargeup", false, false);
      yield return (object) 0.1f;
      this.beginFollowup = true;
      this.Sprite.Play("chargestrike", false, false);
      this.level.Displacement.AddBurst(((Entity) this).Center, 0.3f, 0.0f, 60f, 1f, (Ease.Easer) null, (Ease.Easer) null);
      ((Entity) this).Add((Component) new Coroutine(this.CreateLightningCoroutine(true), true));
      yield return (object) 0.1f;
      this.Disappear();
      yield return (object) 0.28f;
      this.followupGrounded = true;
    }
  }

  private IEnumerator onSlamFlash()
  {
    this.level.Flash(((Color.White) * (0.4f)), false);
    this.level.Displacement.AddBurst(((Entity) this).Position, 0.5f, 0.0f, 150f, 1f, (Ease.Easer) null, (Ease.Easer) null);
    yield return (object) 0.1f;
    this.level.Displacement.AddBurst(((Entity) this).Position, 0.5f, 0.0f, 150f, 1f, (Ease.Easer) null, (Ease.Easer) null);
  }

  private int ShockwaveUpdate() => 5;

  private IEnumerator ShockwaveCoroutine()
  {
    Input.Rumble((RumbleStrength) 2, (RumbleLength) 2);
    this.level.Shake(0.3f);
    this.Sprite.Play("yattackend", false, false);
    this.lightningVVisible = true;
    this.lightningV.Play("once", true, false);
    yield return (object) 0.1f;
    if (this.Phase == 4)
    {
      this.bossAttacking = false;
      yield return (object) 0.4f;
      this.Disappear();
      yield return (object) 0.4f;
    }
    ((Entity) this).Y = this.level.Camera.Top - 48f;
    this.cameraYOffset = -48f;
    ((Entity) this).Visible = false;
    this.collidedVertically = false;
    this.state.State = this.Phase != 4 ? 3 : 7;
  }

  private void ShockwaveBegin()
  {
    Player entity = ((Scene) this.level).Tracker.GetEntity<Player>();
    if (entity != null)
    {
      ((Scene) this.level).Add((Entity) Engine.Pooler.Create<BossDebris>().Init(new Vector2(((Entity) this).Left + 16f, ((Entity) this).Top - 45f)));
      ((Scene) this.level).Add((Entity) Engine.Pooler.Create<Shockwave>().Init(this, entity, false));
      ((Scene) this.level).Add((Entity) Engine.Pooler.Create<Shockwave>().Init(this, entity, true));
    }
    ((Entity) this).Add((Component) new Coroutine(this.onSlamFlash(), true));
  }

  private void LightningBegin()
  {
    this.lightningRemoved = false;
    ((Entity) this).X = 160f + this.level.Camera.Left;
    this.Appear();
    this.timeToTeleportLightning = 1.5f;
  }

  private IEnumerator LightningCoroutine()
  {
    this.Sprite.Play("chargeup", false, false);
    yield return (object) 0.45f;
    this.Sprite.Play("chargestrike", false, false);
    this.level.Displacement.AddBurst(((Entity) this).Center, 0.3f, 0.0f, 60f, 1f, (Ease.Easer) null, (Ease.Easer) null);
    yield return (object) 0.65f;
    ((Entity) this).Add((Component) new Coroutine(this.CreateLightningCoroutine(false), true));
    this.Disappear();
  }

  private IEnumerator CreateLightningCoroutine(bool followup)
  {
    if (followup)
    {
      float randomVal = this.randomNextDouble() ? 0.0f : 30f;
      for (int i = 0; i < 3; ++i)
        ((Scene) this.level).Add((Entity) Engine.Pooler.Create<ConquerorLightningStrike>().Init(new Vector2(this.level.Camera.Left + ((float) ((double) i * 100.0 + 20.0) + randomVal), this.level.Camera.Top), this));
    }
    else if (this.xFlipped)
    {
      for (int i = 4; i >= 0; --i)
      {
        ((Scene) this.level).Add((Entity) Engine.Pooler.Create<ConquerorLightningStrike>().Init(new Vector2(this.level.Camera.Left + (float) (i * 64 /*0x40*/), this.level.Camera.Top), this));
        yield return (object) 1f;
      }
    }
    else
    {
      for (int i = 0; i < 5; ++i)
      {
        ((Scene) this.level).Add((Entity) Engine.Pooler.Create<ConquerorLightningStrike>().Init(new Vector2(this.level.Camera.Left + (float) (i * 64 /*0x40*/), this.level.Camera.Top), this));
        yield return (object) 1f;
      }
    }
    this.lightningRemoved = true;
  }

  private int LightningUpdate()
  {
    this.timeToTeleportLightning -= Engine.DeltaTime;
    return (double) this.timeToTeleportLightning <= 0.0 ? 7 : 12;
  }

  private int BeamUpdate() => 6;

  private void BeamBegin()
  {
    if (this.Phase == 4)
      ((Entity) this).X = this.beamXLoc + this.level.Camera.Left;
    this.Appear();
  }

  private IEnumerator BeamCoroutine()
  {
    while (true)
    {
      if (!this.HardMode)
        yield return (object) 0.125f;
      this.Sprite.Play("beamcharge", false, false);
      if (!this.HardMode)
        yield return (object) 0.05f;
      yield return (object) 0.05f;
      yield return (object) this.Beam();
      if (this.Phase != 4)
      {
        yield return (object) 0.8f;
      }
      else
      {
        if (!this.HardMode)
          yield return (object) 0.1f;
        yield return (object) 0.1f;
        this.Disappear();
        if (!this.HardMode)
          yield return (object) 0.12f;
        yield return (object) 0.28f;
        this.state.State = 7;
      }
    }
  }

  private bool randomNextDouble()
  {
    Player entity = ((Entity) this).Scene.Tracker.GetEntity<Player>();
    if (entity != null)
      this.previousSeed = (float) (((double) ((Entity) entity).X % 2.0 * (double) this.previousSeed + (double) ((Entity) entity).Y % 2.0 + (double) ((this.level.Session.Time - this.startTime) / 10000000L % 2L)) % 2.0);
    return (double) this.previousSeed != 0.0;
  }

  private T randomChoose<T>(List<T> iter)
  {
    Player entity = ((Entity) this).Scene.Tracker.GetEntity<Player>();
    if (entity != null)
      this.previousSeed = ((float) ((double) ((Entity) entity).X % (double) iter.Count * (double) this.previousSeed + (double) ((Entity) entity).Y % (double) iter.Count) + (float) ((int) ((this.level.Session.Time - this.startTime) / 10000000L) % iter.Count)) % (float) iter.Count;
    return iter[Math.Abs((int) this.previousSeed)];
  }

  private void BossBegin()
  {
    this.Sprite.Play("idle", false, false);
    this.currPhase = 0;
    ((Entity) this).Visible = false;
    ((Entity) this).CenterX = this.level.Camera.Left + 160f;
    ((Entity) this).CenterY = this.level.Camera.Top + 70f;
    this.stopChasing = false;
    this.xFlipped = this.randomNextDouble();
    this.yTop = this.randomNextDouble();
    this.currPhase = this.randomChoose<int>(this.phase4Moves);
    if (this.currPhase == 4 && !this.lightningRemoved)
    {
      this.currPhase = 2;
      this.previousSeed = 2f;
    }
    this.beamXLoc = this.randomChoose<float>(this.beamXList);
  }

  private int BossUpdate()
  {
    if (this.currPhase == 1 || this.currPhase == 5)
    {
      this.bossAttacking = true;
      return 11;
    }
    if (this.currPhase == 2 || this.currPhase == 6)
    {
      this.bossAttacking = true;
      return 3;
    }
    if (this.currPhase == 3 || this.currPhase == 7)
    {
      ((Entity) this).Visible = true;
      return 6;
    }
    if (this.currPhase != 4)
      return 7;
    ((Entity) this).Visible = true;
    return 12;
  }

  private IEnumerator BossCoroutine()
  {
    yield return (object) null;
  }

  private void Appear()
  {
    this.Sprite.Play("appear", false, false);
    ((GraphicsComponent) this.Sprite).Scale = new Vector2(1f, 1f);
    ((Entity) this).Visible = true;
  }

  private void Disappear()
  {
    this.Sprite.Play(nameof (Disappear), false, false);
    global::Celeste.Audio.Play("event:/ricky06/FightSFX/cb-disappear");
    ((Entity) this).Add((Component) new Coroutine(this.EmitParticles(), true));
  }

  private IEnumerator EmitParticles()
  {
    Vector2 temp = ((Entity) this).Position;
    for (int i = 0; i < 20; ++i)
    {
      float dir = Calc.NextAngle(Calc.Random);
      ((Entity) this).SceneAs<Level>().Particles.Emit(ConquerorBoss.P_Dissipate, 5, ((temp) + (Calc.AngleToVector(dir, 8f))), ((Vector2.One) * (10f)), dir);
      yield return (object) null;
    }
  }

  private IEnumerator Beam()
  {
    this.laserSfx1.Play("event:/char/badeline/boss_laser_charge", (string) null, 0.0f);
    this.chargeWave.Play("chargeBeam", false, false);
    yield return (object) 0.1f;
    Player entity = ((Scene) this.level).Tracker.GetEntity<Player>();
    this.beamOffsetIndex %= this.beamOffset.Count;
    if (entity != null)
    {
      for (int i = 0; i < 8; ++i)
        ((Scene) this.level).Add((Entity) Engine.Pooler.Create<ConquerorBeam>().Init(this, ((((Entity) this).Center) + (new Vector2((float) Math.Cos(Math.PI / 4.0 * (double) i + (double) this.beamOffset[this.beamOffsetIndex]), (float) Math.Sin(Math.PI / 4.0 * (double) i + (double) this.beamOffset[this.beamOffsetIndex]))))));
    }
    ++this.beamOffsetIndex;
    yield return (object) 0.5f;
    this.Sprite.Play("beamstrike", false, false);
    yield return (object) 0.5f;
    this.laserSfx1.Stop(true);
    global::Celeste.Audio.Play("event:/char/badeline/boss_laser_fire", ((Entity) this).Position);
    this.chargeWave.Play("invisible", false, false);
  }

  private int DummyUpdate() => 10;

  private void DummyBegin() => ((Entity) this).Visible = false;

  private void OnPlayerDash(Player player)
  {
    if (!((Entity) this).Visible || this.Phase != 4 || !player.DashAttacking || !((player.Speed) != (Vector2.Zero)))
      return;
    this.BossTakeDamage(player);
  }

  private void OnPlayerDown(Player player)
  {
    if (!((Entity) this).Visible || this.Phase != 4 || !player.DashAttacking || (double) player.Speed.Y <= 0.0)
      return;
    player.Bounce(((Entity) this).Top);
    this.BossTakeDamage(player, true);
  }

  private void OnPlayerUp(Player player)
  {
    if (!((Entity) this).Visible || this.Phase != 4 || !player.DashAttacking || (double) player.Speed.Y >= 0.0)
      return;
    player.Speed.Y *= -1f;
    player.StateMachine.State = Player.StNormal;
    this.BossTakeDamage(player, true);
  }

  private void OnPlayer(Player player)
  {
    if (this.Phase != 4 && ((Entity) this).Visible)
    {
      player.Die(Calc.SafeNormalize(((((Entity) player).Center) - (((Entity) this).Center))), false, true);
    }
    else
    {
      if (!((Entity) this).Visible || !this.bossAttacking)
        return;
      this.PlayerTakeDamage(player);
    }
  }

  public void PlayerTakeDamage(Player player)
  {
    if (this.Phase != 4)
    {
      player.Die(Calc.SafeNormalize(((((Entity) player).Center) - (((Entity) this).Center))), false, true);
    }
    else
    {
      if (global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.destroyDashActive || (double) this.playerDamageCooldown > 0.0 || SaveData.Instance.Assists.Invincible)
        return;
      this.playerDamageCooldown = 1f;
      bool flag = global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.DecreasePlayerHealth(player, this);
      this.level.Shake(0.3f);
      Input.Rumble((RumbleStrength) 2, (RumbleLength) 2);
      this.level.Flash(((Color.Red) * (0.3f)), false);
      if (!flag)
      {
        ((Entity) this).Add((Component) new Coroutine(this.PlayerStaggerCoroutine(player, ((((Entity) player).Center) - (((Entity) this).Center))), true));
        ((Entity) this).Add((Component) new Coroutine(this.PlayerInvulnerableCoroutine(player), true));
      }
      else
        player.Die(Calc.SafeNormalize(((((Entity) player).Center) - (((Entity) this).Center))), false, true);
      if (this.hp != null)
        this.hp.DecreaseHealth();
      else
        Logger.Log("conquerorboss", "HP not initialized");
    }
  }

  private IEnumerator PlayerStaggerCoroutine(Player player, Vector2 bounce)
  {
    if (((bounce) != (Vector2.Zero)))
    {
      global::Celeste.Celeste.Freeze(0.05f);
      yield return (object) null;
      Vector2 from = ((Entity) player).Position;
      Vector2 to = new Vector2(from.X + (float) (((double) bounce.X < 0.0 ? -1.0 : 1.0) * 30.0), from.Y - 5f);
      Tween tween = Tween.Create((Tween.TweenMode) 1, Ease.CubeOut, 0.2f, true);
      ((Entity) this).Add((Component) tween);
      tween.OnUpdate = (Action<Tween>) (t =>
      {
        if (((Entity) player).CollideCheck<Solid>(((((Entity) this).Position) + (new Vector2(-5f, 0.0f)))))
          { }
        Vector2 vector2 = ((from) + (((((to) - (from))) * (t.Eased))));
        ((Actor) player).MoveToX(vector2.X, (Collision) null);
        ((Actor) player).MoveToY(vector2.Y, (Collision) null);
        ((GraphicsComponent) player.Sprite).Rotation = (float) (Math.Floor((double) t.Eased * 4.0) * 6.2831854820251465);
      });
      yield return (object) tween.Duration;
      tween.Stop();
      tween = (Tween) null;
    }
  }

  private IEnumerator PlayerInvulnerableCoroutine(Player player)
  {
    int times = 2;
    Tween tween = Tween.Create((Tween.TweenMode) 1, Ease.CubeOut, this.playerDamageCooldown, true);
    ((Entity) this).Add((Component) tween);
    tween.OnUpdate = (Action<Tween>) (t =>
    {
      if (!((Entity) this).Scene.OnInterval(0.02f))
        return;
      if (times <= 0)
      {
        ((Component) player.Sprite).Visible = false;
        ((Component) player.Hair).Visible = false;
        times = 2;
      }
      else
      {
        ((Component) player.Sprite).Visible = true;
        ((Component) player.Hair).Visible = true;
        --times;
      }
    });
    yield return (object) tween.Duration;
    tween.Stop();
    ((Component) player.Sprite).Visible = true;
    ((Component) player.Hair).Visible = true;
  }

  private void BossTakeDamage(Player player, bool ignore = false)
  {
    if (!this.sawBadelineDialogue && !this.level.Session.GetFlag("cp_boss_badeline_dialogue") && !this.HardMode)
    {
      ((Entity) this).Scene.Add((Entity) new MiniTextbox("CP_BOSS_BADELINE_DIALOGUE"));
      this.sawBadelineDialogue = true;
      this.level.Session.SetFlag("cp_boss_badeline_dialogue", true);
    }
    if ((double) this.damageCooldown <= 0.0 | ignore)
    {
      this.damageCooldown = 0.2f;
      global::Celeste.Audio.Play("event:/ricky06/FightSFX/cb-boss-hit");
      this.chpGUI.removeHealth();
      player.UseRefill(false);
      this.health -= 2f;
      ((Image) this.Sprite).SetColor(Color.Red);
      ((GraphicsComponent) this.Sprite).Scale = new Vector2((float) (1.3999999761581421 * (this.xFlipped ? 1.0 : -1.0)), 0.6f);
      this.currRed = Color.Red;
    }
    if ((double) this.health > 0.0)
      return;
    this.Die();
  }

  private int WaitingUpdate()
  {
    Player entity = ((Entity) this).Scene.Tracker.GetEntity<Player>();
    if (entity != null && this.Phase == 4)
      return 7;
    int num1;
    if (entity != null && ((entity.Speed) != (Vector2.Zero)))
    {
      double x = (double) ((Entity) entity).X;
      Rectangle bounds = this.level.Bounds;
      double num2 = (double) (bounds.Left + 48 /*0x30*/);
      num1 = x > num2 ? 1 : 0;
    }
    else
      num1 = 0;
    if (num1 != 0)
    {
      switch (this.Phase)
      {
        case 1:
          return 0;
        case 2:
          return 3;
        case 3:
          return 6;
      }
    }
    return 1;
  }

  private void OnCollideV(CollisionData data) => this.collidedVertically = true;

  private void Die()
  {
    this.dead = true;
    this.state.State = 10;
    this.level.Flash(Color.White, false);
    foreach (NPC_Boss entity in ((Entity) this).Scene.Tracker.GetEntities<NPC_Boss>())
    {
      if (this.HardMode)
        entity.invokeDefeatFinal();
      else
        entity.invokeDefeat();
    }
    this.FixThings();
  }

  private void FixThings()
  {
    ((Scene) this.level).Remove((Entity) this.hp);
    ((Scene) this.level).Remove((Entity) this.chpGUI);
    Player entity = Engine.Scene.Tracker.GetEntity<Player>();
    if (entity != null)
    {
      ((Component) entity.Sprite).Visible = true;
      ((Component) entity.Hair).Visible = true;
    }
    ((Scene) this.level).Remove((Entity) this);
  }

  public override void Render()
  {
    if (this.lightningVisible)
    {
      ((GraphicsComponent) this.lightning).RenderPosition = new Vector2(this.level.Camera.Left, ((Entity) this).Top + 16f);
      ((Component) this.lightning).Render();
    }
    if (this.lightningVVisible)
    {
      ((GraphicsComponent) this.lightningV).RenderPosition = new Vector2(((Entity) this).Left + 16f, ((Entity) this).Bottom - 185f);
      ((Component) this.lightningV).Render();
    }
    ((Entity) this).Render();
  }

  public override void Removed(Scene scene)
  {
    base.Removed(scene);
    Glitch.Value = 0.0f;
    this.FixThings();
  }
}

