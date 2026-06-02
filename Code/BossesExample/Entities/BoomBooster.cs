// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.BoomBooster
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Reflection;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/BoomBooster"})]
[Tracked(false)]
internal class BoomBooster : Entity
{
  public static ParticleType P_Idle = new ParticleType(Bumper.P_Ambience);
  public static ParticleType P_Burst = new ParticleType(Bumper.P_FireHit);
  public static ParticleType P_Appear = new ParticleType(Booster.P_Appear)
  {
    Color = Calc.HexToColor("C0796A")
  };
  public static readonly Vector2 playerOffset = new Vector2(0.0f, -2f);
  private Sprite sprite;
  private Entity outline;
  private Wiggler wiggler;
  private BloomPoint bloom;
  private VertexLight light;
  private float respawnTimer;
  private float cannotUseTimer;
  public static FieldInfo player_boostTarget = typeof (Player).GetField("boostTarget", BindingFlags.Instance | BindingFlags.NonPublic);
  public static FieldInfo player_boostRed = typeof (Player).GetField("boostRed", BindingFlags.Instance | BindingFlags.NonPublic);
  public static FieldInfo player_LastBooster = typeof (Player).GetField("LastBooster", BindingFlags.Instance | BindingFlags.NonPublic);
  public static FieldInfo player_CurrentBooster = typeof (Player).GetField("CurrentBooster", BindingFlags.Instance | BindingFlags.NonPublic);
  public bool StartedBoosting;
  private bool hasDestroyDash;

  public bool BoostingPlayer { get; private set; }

  public BoomBooster(EntityData data, Vector2 offset)
    : base(((data.Position) + (offset)))
  {
    this.Depth = -8500;
    this.Collider = (Collider) new Circle(10f, 0.0f, 2f);
    this.Add((Component) (this.sprite = global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.SpriteBank.Create("boomBooster")));
    this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer), (Collider) null, (Collider) null));
    this.Add((Component) (this.light = new VertexLight(Color.White, 1f, 16 /*0x10*/, 32 /*0x20*/)));
    this.Add((Component) (this.bloom = new BloomPoint(0.1f, 16f)));
    this.Add((Component) (this.wiggler = Wiggler.Create(0.5f, 4f, (Action<float>) (f => ((GraphicsComponent) this.sprite).Scale = ((Vector2.One) * ((float) (1.0 + (double) f * 0.25)))), false, false)));
    this.Add((Component) new MirrorReflection());
  }

  public override void Added(Scene scene)
  {
    base.Added(scene);
    Image image = new Image(GFX.Game["objects/booster/outline"]);
    image.CenterOrigin();
    ((GraphicsComponent) image).Color = ((Color.White) * (0.75f));
    this.outline = new Entity(this.Position);
    this.outline.Depth = 8999;
    this.outline.Visible = false;
    this.outline.Add((Component) image);
    this.outline.Add((Component) new MirrorReflection());
    scene.Add(this.outline);
  }

  private void AppearParticles()
  {
    ParticleSystem particlesBg = this.SceneAs<Level>().ParticlesBG;
    for (int index = 0; index < 360; index += 30)
      particlesBg.Emit(BoomBooster.P_Appear, 1, this.Center, ((Vector2.One) * (2f)), (float) index * ((float) Math.PI / 180f));
  }

  private void OnPlayer(Player player)
  {
    if ((double) this.respawnTimer > 0.0 || (double) this.cannotUseTimer > 0.0)
      return;
    this.cannotUseTimer = 0.45f;
    this.hasDestroyDash = global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.hasDestroyDash;
    player.StateMachine.State = Player.StPickup;
    player.Speed = Vector2.Zero;
    new DynData<Player>(player).Set<Vector2>("boostTarget", this.Center);
    this.StartedBoosting = true;
    global::Celeste.Audio.Play("event:/game/04_cliffside/greenbooster_enter", this.Position);
    this.wiggler.Start();
    this.sprite.Play("inside", false, false);
    ((GraphicsComponent) this.sprite).FlipX = player.Facing == Facings.Left;
  }

  public void PlayerBoosted(Player player, Vector2 direction)
  {
    this.StartedBoosting = false;
    Vector2 vector2 = player.ExplodeLaunch(((((Entity) player).Center) - (direction)), false, false);
    new DynData<Player>(player).Set<float>("dashCooldownTimer", 0.0f);
    this.SceneAs<Level>().DirectionalShake(vector2, 0.15f);
    this.SceneAs<Level>().Displacement.AddBurst(this.Center, 0.4f, 12f, 36f, 0.5f, (Ease.Easer) null, (Ease.Easer) null);
    this.SceneAs<Level>().Displacement.AddBurst(this.Center, 0.4f, 24f, 48f, 0.5f, (Ease.Easer) null, (Ease.Easer) null);
    this.SceneAs<Level>().Displacement.AddBurst(this.Center, 0.4f, 36f, 60f, 0.5f, (Ease.Easer) null, (Ease.Easer) null);
    this.SceneAs<Level>().Particles.Emit(Seeker.P_Regen, 12, ((this.Center) + (((vector2) * (12f)))), ((Vector2.One) * (3f)), Calc.Angle(vector2));
    this.SceneAs<Level>().Particles.Emit(BoomBooster.P_Burst, 12, ((this.Center) + (((vector2) * (12f)))), ((Vector2.One) * (3f)), Calc.Angle(vector2));
    this.sprite.Play("burst", false, false);
    global::Celeste.Audio.Play("event:/new_content/game/10_farewell/puffer_splode", this.Position);
    this.outline.Visible = true;
    ((Component) this.sprite).Visible = false;
    this.cannotUseTimer = 0.0f;
    this.respawnTimer = 1f;
    this.wiggler.Stop();
    global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.hasDestroyDash = this.hasDestroyDash;
    global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.destroyDashActive = false;
  }

  public override void Render()
  {
    Vector2 position = ((GraphicsComponent) this.sprite).Position;
    ((GraphicsComponent) this.sprite).Position = Calc.Floor(position);
    if (this.sprite.CurrentAnimationID != "burst" && ((Component) this.sprite).Visible)
      ((GraphicsComponent) this.sprite).DrawOutline(1);
    base.Render();
    ((GraphicsComponent) this.sprite).Position = position;
  }

  public void Respawn()
  {
    global::Celeste.Audio.Play("event:/game/04_cliffside/greenbooster_reappear", this.Position);
    ((GraphicsComponent) this.sprite).Position = Vector2.Zero;
    this.sprite.Play("idle", true, false);
    this.wiggler.Start();
    ((Component) this.sprite).Visible = true;
    this.outline.Visible = false;
    this.AppearParticles();
  }

  public override void Update()
  {
    base.Update();
    if ((double) this.cannotUseTimer > 0.0)
      this.cannotUseTimer -= Engine.DeltaTime;
    if ((double) this.respawnTimer > 0.0)
    {
      this.respawnTimer -= Engine.DeltaTime;
      if ((double) this.respawnTimer <= 0.0)
        this.Respawn();
    }
    if ((double) this.respawnTimer <= 0.0)
    {
      Vector2 vector2 = Vector2.Zero;
      Player entity = this.Scene.Tracker.GetEntity<Player>();
      if (entity != null && this.CollideCheck((Entity) entity))
        vector2 = ((((((Entity) entity).Center) + (BoomBooster.playerOffset))) - (this.Position));
      ((GraphicsComponent) this.sprite).Position = Calc.Approach(((GraphicsComponent) this.sprite).Position, vector2, 80f * Engine.DeltaTime);
      if (this.Scene.OnInterval(0.05f))
      {
        float num = Calc.NextAngle(Calc.Random);
        this.SceneAs<Level>().Particles.Emit(BoomBooster.P_Idle, 1, ((((this.Center) + (((GraphicsComponent) this.sprite).Position))) + (Calc.AngleToVector(num, 8f))), ((Vector2.One) * (2f)), num);
      }
    }
    if (!(this.sprite.CurrentAnimationID == "inside") || this.CollideCheck<Player>())
      return;
    this.sprite.Play("idle", false, false);
  }
}
