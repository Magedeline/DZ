// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.DestroySpinnerRefill
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/DestroySpinnerRefill"})]
[Tracked(false)]
internal class DestroySpinnerRefill : Entity
{
  public EntityID? eid;
  public static ParticleType P_Shatter = new ParticleType(Refill.P_Shatter);
  public static ParticleType P_Regen = new ParticleType(Refill.P_Regen);
  public static ParticleType P_Glow = new ParticleType(Refill.P_Glow);
  public static ParticleType P_ShatterTwo = new ParticleType(Refill.P_ShatterTwo);
  public static ParticleType P_RegenTwo = new ParticleType(Refill.P_RegenTwo);
  public static ParticleType P_GlowTwo = new ParticleType(Refill.P_GlowTwo);
  private Sprite sprite;
  private Sprite flash;
  private Image outline;
  private Wiggler wiggler;
  private BloomPoint bloom;
  private VertexLight light;
  private Level level;
  private SineWave sine;
  private bool twoDashes;
  public bool oneUse;
  private ParticleType p_shatter;
  private ParticleType p_regen;
  private ParticleType p_glow;
  private float respawnTimer;

  public DestroySpinnerRefill(Vector2 position, bool twoDashes, bool oneUse, EntityID? id = null)
    : base(position)
  {
    if (id.HasValue)
      this.eid = id;
    this.Collider = (Collider) new Hitbox(16f, 16f, -8f, -8f);
    this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer), (Collider) null, (Collider) null));
    this.twoDashes = twoDashes;
    this.oneUse = oneUse;
    string str;
    if (twoDashes)
    {
      str = "objects/refillTwo/";
      this.p_shatter = DestroySpinnerRefill.P_ShatterTwo;
      this.p_regen = DestroySpinnerRefill.P_RegenTwo;
      this.p_glow = DestroySpinnerRefill.P_GlowTwo;
    }
    else
    {
      str = "objects/destroyRefill/";
      this.p_shatter = DestroySpinnerRefill.P_Shatter;
      this.p_regen = DestroySpinnerRefill.P_Regen;
      this.p_glow = DestroySpinnerRefill.P_Glow;
    }
    this.Add((Component) (this.outline = new Image(GFX.Game[str + nameof (outline)])));
    this.outline.CenterOrigin();
    ((Component) this.outline).Visible = false;
    this.Add((Component) (this.sprite = global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.SpriteBank.Create("destroy_refill")));
    this.sprite.Play("idle", false, false);
    this.Add((Component) (this.flash = new Sprite(GFX.Game, str + nameof (flash))));
    this.flash.Add(nameof (flash), "", 0.05f);
    this.flash.OnFinish = (Action<string>) (_param1 => ((Component) this.flash).Visible = false);
    ((Image) this.flash).CenterOrigin();
    this.Add((Component) (this.wiggler = Wiggler.Create(1f, 4f, (Action<float>) (v => ((GraphicsComponent) this.sprite).Scale = ((GraphicsComponent) this.flash).Scale = ((Vector2.One) * ((float) (1.0 + (double) v * 0.20000000298023224)))), false, false)));
    this.Add((Component) new MirrorReflection());
    this.Add((Component) (this.bloom = new BloomPoint(0.8f, 16f)));
    this.Add((Component) (this.light = new VertexLight(Color.White, 1f, 16 /*0x10*/, 48 /*0x30*/)));
    this.Add((Component) (this.sine = new SineWave(0.6f, 0.0f)));
    this.sine.Randomize();
    this.UpdateY();
    this.Depth = -100;
  }

  public DestroySpinnerRefill(EntityData data, Vector2 offset, EntityID id)
    : this(((data.Position) + (offset)), data.Bool("twoDash", false), data.Bool(nameof (oneUse), false), new EntityID?(id))
  {
  }

  public virtual void Awake(Scene scene)
  {
    base.Awake(scene);
    this.level = this.SceneAs<Level>();
    this.Add((Component) new Coroutine(this.spriteFlashAppear(), true));
  }

  public IEnumerator spriteFlashAppear()
  {
    this.sprite.Play("appear", false, false);
    this.bloom.Alpha = 1f;
    this.bloom.Radius = 41f;
    for (int i = 0; i < 12; ++i)
    {
      this.bloom.Alpha -= 0.01f;
      this.bloom.Radius -= 2f;
      yield return (object) 0.01f;
    }
    this.bloom.Alpha = 0.8f;
    this.bloom.Radius = 16f;
    this.sprite.Play("idle", false, false);
  }

  public virtual void Update()
  {
    base.Update();
    if ((double) this.respawnTimer > 0.0)
    {
      this.respawnTimer -= Engine.DeltaTime;
      if ((double) this.respawnTimer <= 0.0)
        this.Respawn();
    }
    else if (this.Scene.OnInterval(0.1f))
      this.level.ParticlesFG.Emit(this.p_glow, 1, this.Position, ((Vector2.One) * (5f)));
    this.UpdateY();
    this.light.Alpha = Calc.Approach(this.light.Alpha, ((Component) this.sprite).Visible ? 1f : 0.0f, 4f * Engine.DeltaTime);
    this.bloom.Alpha = this.light.Alpha * 0.8f;
    if (!this.Scene.OnInterval(2f) || !((Component) this.sprite).Visible)
      return;
    this.flash.Play("flash", true, false);
    ((Component) this.flash).Visible = true;
  }

  private void Respawn()
  {
    if (this.Collidable)
      return;
    this.Collidable = true;
    ((Component) this.sprite).Visible = true;
    ((Component) this.outline).Visible = false;
    this.Depth = -100;
    this.wiggler.Start();
    Audio.Play(this.twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_return" : "event:/game/general/diamond_return", this.Position);
    this.level.ParticlesFG.Emit(this.p_regen, 16 /*0x10*/, this.Position, ((Vector2.One) * (2f)));
  }

  private void UpdateY()
  {
    float num = ((GraphicsComponent) this.flash).Y = ((GraphicsComponent) this.sprite).Y = this.bloom.Y = this.sine.Value * 2f;
  }

  public virtual void Render()
  {
    if (((Component) this.sprite).Visible)
      ((GraphicsComponent) this.sprite).DrawOutline(1);
    base.Render();
  }

  private void OnPlayer(Player player)
  {
    if (!player.UseRefill(this.twoDashes) && global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.hasDestroyDash)
      return;
    Audio.Play(this.twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", this.Position);
    Input.Rumble((RumbleStrength) 1, (RumbleLength) 1);
    this.Collidable = false;
    this.Add((Component) new Coroutine(this.RefillRoutine(player), true));
    this.respawnTimer = 2.5f;
    global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.hasDestroyDash = true;
    if (global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.dw == null)
    {
      global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.dw = new DashWave(player);
      ((Scene) this.level).Add((Entity) global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.dw);
    }
  }

  private IEnumerator RefillRoutine(Player player)
  {
    global::Celeste.Celeste.Freeze(0.05f);
    yield return (object) null;
    this.level.Shake(0.3f);
    ((Component) this.sprite).Visible = ((Component) this.flash).Visible = false;
    if (!this.oneUse)
      ((Component) this.outline).Visible = true;
    this.Depth = 8999;
    yield return (object) 0.05f;
    float num = Calc.Angle(player.Speed);
    this.level.ParticlesFG.Emit(this.p_shatter, 5, this.Position, ((Vector2.One) * (4f)), num - 1.57079637f);
    this.level.ParticlesFG.Emit(this.p_shatter, 5, this.Position, ((Vector2.One) * (4f)), num + 1.57079637f);
    SlashFx.Burst(this.Position, num);
    if (this.oneUse)
      this.RemoveSelf();
  }
}
