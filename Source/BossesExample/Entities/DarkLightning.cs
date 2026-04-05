// Decompiled with JetBrains decompiler
// Type: Celeste.Mod.ricky06ModPack.Entities.DarkLightning
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/DarkLightning"})]
[Tracked(false)]
public class DarkLightning : Entity
{
  public static ParticleType P_Shatter = new ParticleType(Lightning.P_Shatter);
  public const string Flag = "disable_lightning";
  public float Fade;
  private bool disappearing;
  private float toggleOffset;
  public int VisualWidth;
  public int VisualHeight;
  private ConquerorBoss boss;
  private bool sineMove;

  public DarkLightning(
    Vector2 position,
    int width,
    int height,
    Vector2? node,
    float moveTime,
    ConquerorBoss bossfight = null,
    bool sineMove = true)
    : base(position)
  {
    this.VisualWidth = width;
    this.VisualHeight = height;
    this.Collider = (Collider) new Hitbox((float) (width - 2), (float) (height - 2), 1f, 1f);
    this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer), (Collider) null, (Collider) null));
    if (node.HasValue)
      this.Add((Component) new Coroutine(this.MoveRoutine(position, node.Value, moveTime), true));
    this.toggleOffset = Calc.NextFloat(Calc.Random);
    this.boss = bossfight;
    this.sineMove = sineMove;
  }

  public DarkLightning(EntityData data, Vector2 levelOffset)
    : this(((data.Position) + (levelOffset)), data.Width, data.Height, data.FirstNodeNullable(new Vector2?(levelOffset)), data.Float("moveTime", 0.0f))
  {
  }

  public virtual void Added(Scene scene)
  {
    base.Added(scene);
    scene.Tracker.GetEntity<DarkLightningRenderer>()?.Track(this);
  }

  public virtual void Removed(Scene scene)
  {
    base.Removed(scene);
    scene.Tracker.GetEntity<DarkLightningRenderer>().Untrack(this);
  }

  public virtual void Update()
  {
    if (this.Collidable && this.Scene.OnInterval(0.25f, this.toggleOffset))
      this.ToggleCheck();
    if (!this.Collidable && this.Scene.OnInterval(0.05f, this.toggleOffset))
      this.ToggleCheck();
    base.Update();
  }

  public void ToggleCheck() => this.Collidable = this.Visible = this.InView();

  private bool InView()
  {
    Camera camera = (this.Scene as Level).Camera;
    return (double) this.X + (double) this.Width > (double) camera.X - 16.0 && (double) this.Y + (double) this.Height > (double) camera.Y - 16.0 && (double) this.X < (double) camera.X + 320.0 + 16.0 && (double) this.Y < (double) camera.Y + 180.0 + 16.0;
  }

  private void OnPlayer(Player player)
  {
    if (this.disappearing || SaveData.Instance.Assists.Invincible)
      return;
    if (global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.destroyDashActive)
    {
      Vector2 vector2 = new Vector2(Math.Abs(((GraphicsComponent) player.Sprite).Scale.X) * (float) player.Facing, ((GraphicsComponent) player.Sprite).Scale.Y);
      TrailManager.Add((Entity) player, vector2, Color.Black, 1f);
    }
    else
    {
      int num = Math.Sign(((Entity) player).X - this.X);
      if (num == 0)
        num = -1;
      if (this.boss != null)
        this.boss.PlayerTakeDamage(player);
      else
        player.Die(((Vector2.UnitX) * ((float) num)), false, true);
    }
  }

  private IEnumerator MoveRoutine(Vector2 start, Vector2 end, float moveTime)
  {
    while (true)
    {
      yield return (object) this.Move(start, end, moveTime);
      yield return (object) this.Move(end, start, moveTime);
    }
  }

  private IEnumerator Move(Vector2 start, Vector2 end, float moveTime)
  {
    float at = 0.0f;
    while (true)
    {
      if (this.sineMove)
        this.Position = Vector2.Lerp(start, end, Ease.SineInOut.Invoke(at));
      else
        this.Position = Vector2.Lerp(start, end, Ease.Linear.Invoke(at));
      if ((double) at < 1.0)
      {
        yield return (object) null;
        at = MathHelper.Clamp(at + Engine.DeltaTime / moveTime, 0.0f, 1f);
      }
      else
        break;
    }
  }

  private void Shatter()
  {
    if (this.Scene == null)
      return;
    for (int index1 = 4; (double) index1 < (double) this.Width; index1 += 8)
    {
      for (int index2 = 4; (double) index2 < (double) this.Height; index2 += 8)
        this.SceneAs<Level>().ParticlesFG.Emit(DarkLightning.P_Shatter, 1, ((this.TopLeft) + (new Vector2((float) index1, (float) index2))), ((Vector2.One) * (3f)));
    }
  }

  public static IEnumerator PulseRoutine(Level level)
  {
    for (float t2 = 0.0f; (double) t2 < 1.0; t2 += Engine.DeltaTime * 8f)
    {
      DarkLightning.SetPulseValue(level, t2);
      yield return (object) null;
    }
    for (float t2 = 1f; (double) t2 > 0.0; t2 -= Engine.DeltaTime * 8f)
    {
      DarkLightning.SetPulseValue(level, t2);
      yield return (object) null;
    }
    DarkLightning.SetPulseValue(level, 0.0f);
  }

  private static void SetPulseValue(Level level, float t)
  {
    BloomRenderer bloom = level.Bloom;
    DarkLightningRenderer entity = ((Scene) level).Tracker.GetEntity<DarkLightningRenderer>();
    Glitch.Value = MathHelper.Lerp(0.0f, 0.075f, t);
    bloom.Strength = MathHelper.Lerp(1f, 1.2f, t);
    entity.Fade = t * 0.2f;
  }

  private static void SetBreakValue(Level level, float t)
  {
    BloomRenderer bloom = level.Bloom;
    DarkLightningRenderer entity = ((Scene) level).Tracker.GetEntity<DarkLightningRenderer>();
    Glitch.Value = MathHelper.Lerp(0.0f, 0.15f, t);
    bloom.Strength = MathHelper.Lerp(1f, 1.5f, t);
    entity.Fade = t * 0.6f;
  }

  public static IEnumerator RemoveRoutine(Level level, Action onComplete = null)
  {
    List<DarkLightning> blocks = ((Scene) level).Entities.FindAll<DarkLightning>();
    foreach (DarkLightning item in new List<DarkLightning>((IEnumerable<DarkLightning>) blocks))
    {
      item.disappearing = true;
      if ((double) item.Right < (double) level.Camera.Left || (double) item.Bottom < (double) level.Camera.Top || (double) item.Left > (double) level.Camera.Right || (double) item.Top > (double) level.Camera.Bottom)
      {
        blocks.Remove(item);
        item.RemoveSelf();
      }
    }
    DarkLightningRenderer entity = ((Scene) level).Tracker.GetEntity<DarkLightningRenderer>();
    entity.StopAmbience();
    entity.UpdateSeeds = false;
    for (float t2 = 0.0f; (double) t2 < 1.0; t2 += Engine.DeltaTime * 4f)
    {
      DarkLightning.SetBreakValue(level, t2);
      yield return (object) null;
    }
    DarkLightning.SetBreakValue(level, 1f);
    level.Shake(0.3f);
    for (int num = blocks.Count - 1; num >= 0; --num)
      blocks[num].Shatter();
    for (float t2 = 0.0f; (double) t2 < 1.0; t2 += Engine.DeltaTime * 8f)
    {
      DarkLightning.SetBreakValue(level, 1f - t2);
      yield return (object) null;
    }
    DarkLightning.SetBreakValue(level, 0.0f);
    foreach (DarkLightning item2 in blocks)
      item2.RemoveSelf();
    FlingBird flingBird = ((Scene) level).Entities.FindFirst<FlingBird>();
    if (flingBird != null)
      flingBird.LightningRemoved = true;
    Action action = onComplete;
    if (action != null)
      action();
  }
}
