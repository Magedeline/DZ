// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.ConquerorKeys
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

#nullable disable
namespace Celeste.Mod.DZ.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/ConquerorKeys"})]
[Tracked(false)]
public class ConquerorKeys : Entity
{
  protected static readonly int[] animationFrames = new int[23]
  {
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    1,
    2,
    3,
    4,
    5,
    6,
    7,
    8,
    9,
    10,
    11,
    12,
    13
  };
  protected Sprite sprite;
  private string spriteName;
  private bool inCollectAnimation;
  protected Wiggler scaleWiggler;
  private Vector2 moveWiggleDir;
  private Wiggler moveWiggler;
  private float bounceSfxDelay;
  protected VertexLight light;
  protected BloomPoint bloom;
  private ParticleType shineParticle;
  private HoldableCollider holdableCollider;
  private SoundEmitter sfx;
  private string roomID;
  private float spawnX;
  private float spawnY;
  private Level level;

  public ConquerorKeys(EntityData data, Vector2 position)
    : base(((data.Position) + (position)))
  {
    this.spriteName = data.Attr(nameof (sprite), "");
    this.roomID = data.Attr(nameof (roomID), "");
    this.spawnX = data.Float(nameof (spawnX), 0.0f);
    this.spawnY = data.Float(nameof (spawnY), 0.0f);
    this.Collider = (Collider) new Hitbox(12f, 12f, -6f, -6f);
    this.Add((Component) (this.scaleWiggler = Wiggler.Create(0.5f, 4f, (Action<float>) (f => ((GraphicsComponent) this.sprite).Scale = ((Vector2.One) * ((float) (1.0 + (double) f * 0.30000001192092896)))), false, false)));
    this.moveWiggler = Wiggler.Create(0.8f, 2f, (Action<float>) null, false, false);
    this.moveWiggler.StartZero = true;
    this.Add((Component) this.moveWiggler);
    this.Add((Component) new PlayerCollider(new Action<Player>(this.onPlayer), (Collider) null, (Collider) null));
    this.Add((Component) (this.holdableCollider = new HoldableCollider(new Action<Holdable>(this.onHoldable), (Collider) null)));
  }

  public override void Awake(Scene scene)
  {
    base.Awake(scene);
    this.level = this.SceneAs<Level>();
    AreaKey area = (scene as Level).Session.Area;
    string spriteName = this.spriteName;
    this.Add((Component) (this.sprite = global::Celeste.Mod.DZ.BossesExample.BossesExampleModule.SpriteBank.Create(spriteName)));
    if (this.KeyIsActive())
      this.sprite.Play("ghost", false, false);
    else
      this.sprite.Play("idle", false, false);
    this.sprite.OnLoop = (Action<string>) (_param1 =>
    {
      if (!this.Visible)
        return;
      global::Celeste.Audio.Play("event:/game/general/crystalheart_pulse", this.Position);
      this.scaleWiggler.Start();
      (this.Scene as Level).Displacement.AddBurst(((this.Position) + (((GraphicsComponent) this.sprite).Position)), 0.35f, 4f, 24f, 0.25f, (Ease.Easer) null, (Ease.Easer) null);
    });
    Color white = Color.White;
    this.shineParticle = new ParticleType(HeartGem.P_BlueShine);
    this.Add((Component) (this.light = new VertexLight(Color.Lerp(white, Color.White, 0.5f), 1f, 32 /*0x20*/, 64 /*0x40*/)));
    this.Add((Component) (this.bloom = new BloomPoint(0.75f, 16f)));
  }

  public override void Update()
  {
    base.Update();
    this.bounceSfxDelay -= Engine.DeltaTime;
    ((GraphicsComponent) this.sprite).Position = ((((this.moveWiggleDir) * (this.moveWiggler.Value))) * (-8f));
    if (this.Visible && this.Scene.OnInterval(0.1f))
      this.SceneAs<Level>().Particles.Emit(this.shineParticle, 1, ((this.Center) + (((GraphicsComponent) this.sprite).Position)), ((Vector2.One) * (4f)));
    int num;
    if (this.inCollectAnimation)
    {
      Player entity = this.Scene.Tracker.GetEntity<Player>();
      num = entity != null ? (entity.Dead ? 1 : 0) : 1;
    }
    else
      num = 0;
    if (num == 0)
      return;
    this.interruptCollection();
  }

  private void onPlayer(Player player)
  {
    Level scene = this.Scene as Level;
    if (player.DashAttacking)
    {
      this.heartBroken(player, (Holdable) null, scene);
    }
    else
    {
      player.PointBounce(this.Center);
      this.moveWiggler.Start();
      this.scaleWiggler.Start();
      this.moveWiggleDir = Calc.SafeNormalize(((this.Center) - (((Entity) player).Center)), Vector2.UnitY);
      Input.Rumble((RumbleStrength) 1, (RumbleLength) 1);
      if ((double) this.bounceSfxDelay > 0.0)
        return;
      global::Celeste.Audio.Play("event:/game/general/crystalheart_bounce", this.Position);
      this.bounceSfxDelay = 0.1f;
    }
  }

  public void onHoldable(Holdable holdable)
  {
    Player entity = this.Scene.Tracker.GetEntity<Player>();
    if (entity == null || !holdable.Dangerous(this.holdableCollider))
      return;
    this.heartBroken(entity, holdable, this.SceneAs<Level>());
  }

  private void heartBroken(Player player, Holdable holdable, Level level)
  {
    this.Add((Component) new Coroutine(this.SmashRoutine(player, level), true));
  }

  private IEnumerator SmashRoutine(Player player, Level level)
  {
    level.CanRetry = false;
    this.inCollectAnimation = true;
    this.Collidable = false;
    this.sfx = SoundEmitter.Play("event:/game/07_summit/gem_get", (Entity) this, new Vector2?());
    this.Depth = -2000000;
    yield return (object) null;
    global::Celeste.Celeste.Freeze(0.2f);
    yield return (object) null;
    Engine.TimeRate = 0.5f;
    int original_depth = ((Entity) player).Depth;
    ((Entity) player).Depth = -2000000;
    for (int i = 0; i < 10; ++i)
      this.Scene.Add((Entity) new AbsorbOrb(this.Position, (Entity) null, new Vector2?()));
    level.Shake(0.3f);
    Input.Rumble((RumbleStrength) 2, (RumbleLength) 1);
    level.Flash(Color.White, false);
    this.light.Alpha = this.bloom.Alpha = 0.0f;
    level.FormationBackdrop.Display = true;
    level.FormationBackdrop.Alpha = 1f;
    this.Visible = false;
    for (float time = 0.0f; (double) time < 2.0; time += Engine.RawDeltaTime)
    {
      Engine.TimeRate = Calc.Approach(Engine.TimeRate, 0.0f, Engine.RawDeltaTime * 0.25f);
      yield return (object) null;
    }
    yield return (object) null;
    if (player.Dead)
      yield return (object) 100f;
    Engine.TimeRate = 1f;
    ((Entity) player).Depth = original_depth;
    this.GetKey();
  }

  private void GetKey()
  {
    this.interruptCollection();
    this.TeleportLevel();
    this.FixAndAddToSession();
    this.RemoveSelf();
  }

  private void FixAndAddToSession()
  {
    Level scene = this.Scene as Level;
    scene.NextColorGrade("none", 0.0f);
    scene.Bloom.Base = 0.0f;
    this.RegisterActivated();
  }

  private bool KeyIsActive()
  {
    return global::Celeste.Mod.DZ.BossesExample.BossesExampleModule.Settings.ResetKeysForSession ? this.level.Session.GetFlag("CP-ConquerorKeys-" + this.spriteName) : global::Celeste.Mod.DZ.BossesExample.BossesExampleModule.SaveData.StoneFlags.Contains(this.spriteName);
  }

  private void RegisterActivated()
  {
    this.level.Session.SetFlag("CP-ConquerorKeys-" + this.spriteName, true);
    if (this.KeyIsActive())
      return;
    global::Celeste.Mod.DZ.BossesExample.BossesExampleModule.SaveData.StoneFlags.Add(this.spriteName);
  }

  private void interruptCollection()
  {
    Level scene = this.Scene as Level;
    scene.CanRetry = true;
    scene.FormationBackdrop.Display = false;
    Engine.TimeRate = 1f;
    global::Celeste.Audio.SetMusicParam("lowpass", 0.0f);
    if (this.sfx == null)
      return;
    this.sfx.Source.Param("end", 1f);
  }

  private void TeleportLevel()
  {
    if (this.roomID.Length == 0)
      return;
    Player player = this.Scene.Tracker.GetEntity<Player>();
    global::Celeste.Audio.Play("event:/new_content/game/10_farewell/glitch_short");
    Level level = this.SceneAs<Level>();
    LevelData leveldata = level.Session.LevelData;
    Session session = (this.Scene as Level).Session;
    Tween tween1 = Tween.Create((Tween.TweenMode) 1, (Ease.Easer) null, 0.1f, true);
    tween1.OnUpdate = (Action<Tween>) (t => Glitch.Value = 0.7f * t.Eased);
    tween1.OnComplete = (Action<Tween>) (_param1 => ((Scene) level).OnEndOfFrame += (Action) (() =>
    {
      Vector2 position = ((Entity) player).Position;
      Player player1 = player;
      ((Entity) player1).Position = ((((Entity) player1).Position) - (leveldata.Position));
      Camera camera = level.Camera;
      camera.Position = ((camera.Position) - (leveldata.Position));
      int dashes = player.Dashes;
      float stamina = player.Stamina;
      Facings facing = player.Facing;
      level.Session.Level = this.roomID;
      Leader leader = ((Entity) player).Get<Leader>();
      foreach (Follower follower in leader.Followers)
      {
        if (((Component) follower).Entity != null)
        {
          ((Component) follower).Entity.AddTag(((int) (Tags.Global)));
          level.Session.DoNotLoad.Add(follower.ParentEntityID);
        }
      }
      ((Scene) level).Remove((Entity) player);
      level.UnloadLevel();
      ((Scene) level).Add((Entity) player);
      level.LoadLevel((Player.IntroTypes) 0, false);
      leveldata = level.Session.LevelData;
      ((Entity) player).Position = Calc.ClosestTo(level.Session.LevelData.Spawns, new Vector2(this.spawnX, this.spawnY));
      player.Dashes = dashes;
      player.Stamina = stamina;
      player.Facing = facing;
      level.Camera.Position = level.GetFullCameraTargetAt(player, ((Entity) player).Position);
      level.Session.RespawnPoint = new Vector2?(Calc.ClosestTo(level.Session.LevelData.Spawns, ((Entity) player).Position));
      foreach (Follower follower in leader.Followers)
      {
        if (((Component) follower).Entity != null)
        {
          Entity entity = ((Component) follower).Entity;
          entity.Position = ((entity.Position) + (((((Entity) player).Position) - (position))));
          ((Component) follower).Entity.RemoveTag(((int) (Tags.Global)));
          level.Session.DoNotLoad.Remove(follower.ParentEntityID);
        }
      }
      for (int index1 = 0; index1 < leader.PastPoints.Count; ++index1)
      {
        List<Vector2> pastPoints = leader.PastPoints;
        int index2 = index1;
        pastPoints[index2] = ((pastPoints[index2]) + (((((Entity) player).Position) - (position))));
      }
      leader.TransferFollowers();
      Tween tween2 = Tween.Create((Tween.TweenMode) 1, (Ease.Easer) null, 0.3f, true);
      tween2.OnUpdate = (Action<Tween>) (t => Glitch.Value = (float) (0.699999988079071 * (1.0 - (double) t.Eased)));
      ((Entity) player).Add((Component) tween2);
    }));
    ((Entity) player).Add((Component) tween1);
  }
}

