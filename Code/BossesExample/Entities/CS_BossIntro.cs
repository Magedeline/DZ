// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.CS_BossIntro
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

public class CS_BossIntro : CutsceneEntity
{
  private NPC boss;
  private Player player;
  private float anxiety;
  private float anxietyFlicker;
  private SoundSource sfx;
  private SoundSource events;

  public CS_BossIntro(NPC boss)
    : base(true, false)
  {
    this.boss = boss;
    ((Entity) this).Add((Component) (this.sfx = new SoundSource()));
    ((Entity) this).Add((Component) (this.events = new SoundSource()));
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
      if (this.player == null)
        yield return (object) null;
      else
        break;
    }
    this.player.StateMachine.State = Player.StDummy;
    this.player.StateMachine.Locked = true;
    this.player.ForceCameraUpdate = true;
    while (!((Actor) this.player).OnGround(1) || (double) this.player.Speed.Y < 0.0)
      yield return (object) null;
    this.sfx.Play("event:/ricky06/CutsceneSFX/buildup", (string) null, 0.0f);
    yield return (object) level.ZoomTo(new Vector2(120f, 130f), 2f, 1.5f);
    Player player = this.player;
    Rectangle bounds = level.Bounds;
    double num = (double) bounds.Left + 150.0;
    yield return (object) player.DummyWalkTo((float) num, false, 1f, false);
    Input.Rumble((RumbleStrength) 1, (RumbleLength) 1);
    this.events.Play("event:/ricky06/CutsceneSFX/earthquake", (string) null, 0.0f);
    yield return (object) this.shakeLevel(level, 0.5f, 0.5f);
    yield return (object) this.shakeLevel(level, 0.5f, 0.2f);
    this.player.Facing = Facings.Left;
    yield return (object) this.shakeLevel(level, 0.1f, 0.1f);
    yield return (object) 0.5f;
    yield return (object) level.ZoomAcross(new Vector2(110f, 70f), 1.5f, 1f);
    yield return (object) 0.5f;
    ((Entity) this.boss).Visible = true;
    this.boss.Sprite.Play("spark1", false, false);
    this.events.Play("event:/ricky06/CutsceneSFX/sparks1", (string) null, 0.0f);
    yield return (object) 2f;
    this.boss.Sprite.Play("spark2", false, false);
    this.events.Play("event:/ricky06/CutsceneSFX/sparks2", (string) null, 0.0f);
    yield return (object) 1f;
    this.boss.Sprite.Play("appear", false, false);
    this.events.Play("event:/ricky06/CutsceneSFX/suction", (string) null, 0.0f);
    level.Displacement.AddBurst(((Entity) this.boss).Position, 3f, 0.0f, 100f, 0.4f, (Ease.Easer) null, (Ease.Easer) null);
    yield return (object) 1.4f;
    this.events.Play("event:/ricky06/FightSFX/cb-disappear", (string) null, 0.0f);
    yield return (object) 1.6f;
    this.boss.Sprite.Play("screech", false, false);
    Input.Rumble((RumbleStrength) 1, (RumbleLength) 4);
    yield return (object) 1.5f;
    global::Celeste.Audio.Play("event:/ricky06/CutsceneSFX/screech");
    for (int i = 0; i < 10; ++i)
    {
      level.Displacement.AddBurst(((Entity) this.boss).Position, 0.5f, 20f, 80f, 0.5f, (Ease.Easer) null, (Ease.Easer) null);
      yield return (object) this.shakeLevel(level, 0.1f, (float) (1 - i / 10));
      this.anxiety = Calc.Approach(this.anxiety, 0.5f, Engine.DeltaTime * 0.5f);
    }
    level.Session.Audio.Music.Event = "event:/ricky06/CP-OST5";
    level.Session.Audio.Apply(false);
    this.boss.Sprite.Play("disappear", false, false);
    yield return (object) level.ZoomBack(0.5f);
    this.EndCutscene(level, true);
  }

  private IEnumerator shakeLevel(Level level, float duration, float factor = 1f)
  {
    List<int> directionsX = new List<int>()
    {
      1,
      0,
      -1,
      2,
      -1,
      -1,
      0,
      2,
      -2,
      1,
      -1
    };
    List<int> directionsY = new List<int>()
    {
      0,
      3,
      -1,
      2,
      -2,
      -1,
      -1,
      -1,
      -2,
      2,
      1
    };
    int i = 0;
    while ((double) duration > 0.0)
    {
      level.Camera.X += (float) directionsX[i % 11] * factor;
      level.Camera.Y += (float) directionsY[i % 11] * factor;
      ++i;
      duration -= Engine.DeltaTime;
      yield return (object) null;
    }
  }

  public override void OnEnd(Level level)
  {
    Distort.Anxiety = this.anxiety = this.anxietyFlicker = 0.0f;
    this.player = ((Entity) this).Scene.Tracker.GetEntity<Player>();
    if (this.player != null)
    {
      this.player.StateMachine.Locked = false;
      this.player.StateMachine.State = Player.StNormal;
      this.player.Speed.Y = 0.0f;
      while (((Entity) this.player).CollideCheck<Solid>())
      {
        Player player = this.player;
        ((Entity) player).Y = ((Entity) player).Y - 1f;
      }
      level.Camera.Position = this.player.CameraTarget;
    }
    ((Entity) this.boss).RemoveSelf();
    Scene scene = ((Entity) this).Scene;
    Rectangle bounds1 = level.Bounds;
    double num1 = (double) bounds1.Left - 60.0;
    Rectangle bounds2 = level.Bounds;
    double num2 = (double) bounds2.Top - 60.0;
    ConquerorBoss conquerorBoss = new ConquerorBoss(new Vector2((float) num1, (float) num2), 1, true, false);
    scene.Add((Entity) conquerorBoss);
    level.Session.Audio.Music.Event = "event:/ricky06/CP-OST5";
    level.Session.Audio.Apply(false);
    level.Session.SetFlag("boss_intro_cutscene", true);
  }

  public override void Update()
  {
    Distort.Anxiety = this.anxiety + this.anxiety * this.anxietyFlicker;
    if (((Entity) this).Scene.OnInterval(0.05f))
      this.anxietyFlicker = Calc.NextFloat(Calc.Random, 0.4f) - 0.2f;
    base.Update();
  }
}
