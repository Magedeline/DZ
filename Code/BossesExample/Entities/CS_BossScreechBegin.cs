// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.CS_BossScreechBegin
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

public class CS_BossScreechBegin : CutsceneEntity
{
  private NPC boss;
  private Player player;
  private bool hardMode;
  private EventInstance screech;

  public CS_BossScreechBegin(NPC boss, bool hardMode)
    : base(true, false)
  {
    this.boss = boss;
    this.hardMode = hardMode;
  }

  public override void Added(Scene scene) => base.Added(scene);

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
    while (!((Actor) this.player).OnGround(1) || (double) this.player.Speed.Y < 0.0)
      yield return (object) null;
    yield return (object) 0.5f;
    ((Entity) this.boss).Visible = true;
    this.boss.Sprite.Play("quick_appear", false, false);
    global::Celeste.Audio.Play("event:/ricky06/FightSFX/cb-disappear");
    yield return (object) 0.3f;
    this.boss.Sprite.Play("screechfast", false, false);
    this.screech = global::Celeste.Audio.Play("event:/ricky06/CutsceneSFX/screech");
    yield return (object) 0.06f;
    Input.Rumble((RumbleStrength) 1, (RumbleLength) 4);
    Camera defaultCam = level.Camera;
    for (int i = 0; i < 10; ++i)
    {
      level.Displacement.AddBurst(((Entity) this.boss).Position, 0.5f, 20f, 80f, 0.5f, (Ease.Easer) null, (Ease.Easer) null);
      yield return (object) this.shakeLevel(level, 0.1f, (float) (1 - i / 10));
    }
    global::Celeste.Audio.SetMusic("event:/ricky06/CP-OST6", true, true);
    level.Camera = defaultCam;
    this.boss.Sprite.Play("disappear", false, false);
    global::Celeste.Audio.Play("event:/ricky06/FightSFX/cb-disappear");
    yield return (object) 0.5f;
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
      -1,
      0
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
      1,
      0
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
    }
    if (this.WasSkipped && (((HandleBase) this.screech) != ((HandleBase) null)))
      global::Celeste.Audio.Stop(this.screech, true);
    ((Entity) this.boss).Visible = false;
    Scene scene = ((Entity) this).Scene;
    Rectangle bounds1 = level.Bounds;
    double left = (double) bounds1.Left;
    Rectangle bounds2 = level.Bounds;
    double top = (double) bounds2.Top;
    ConquerorBoss conquerorBoss = new ConquerorBoss(new Vector2((float) left, (float) top), 4, false, this.hardMode);
    scene.Add((Entity) conquerorBoss);
    level.Session.SetFlag("cb_screech_intro", true);
    global::Celeste.Audio.SetMusic("event:/ricky06/CP-OST6", true, true);
  }
}
