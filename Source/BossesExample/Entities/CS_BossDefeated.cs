// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.CS_BossDefeated
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace Celeste.Mod.DZ.BossesExample.Entities;

public class CS_BossDefeated : CutsceneEntity
{
  private NPC_Boss boss;
  private Player player;
  private Level level;
  private Vector2 original_pos;
  private Tween tween;
  private SoundSource deathLoop;
  private SoundSource deathScreech;
  private SoundSource sfx;

  public CS_BossDefeated(NPC boss, Player player)
    : base(true, false)
  {
    this.boss = (NPC_Boss) boss;
    this.player = player;
    ((Entity) this).Add((Component) (this.deathLoop = new SoundSource()));
    ((Entity) this).Add((Component) (this.deathScreech = new SoundSource()));
    ((Entity) this).Add((Component) (this.sfx = new SoundSource()));
  }

  public override void Added(Scene scene)
  {
    base.Added(scene);
    this.level = ((Entity) this).SceneAs<Level>();
    this.original_pos = ((Entity) this.boss).Position;
    global::Celeste.Audio.SetMusic("event:/ricky06/cpostsilence", true, true);
  }

  public override void OnBegin(Level level)
  {
    ((Entity) this).Add((Component) new Coroutine(this.Cutscene(level), true));
  }

  private void GlitchLevel()
  {
    global::Celeste.Audio.Play("event:/new_content/game/10_farewell/glitch_short");
    this.level.Shake(0.3f);
    Input.Rumble((RumbleStrength) 0, (RumbleLength) 0);
    Tween tween1 = Tween.Create((Tween.TweenMode) 1, (Ease.Easer) null, 0.05f, true);
    tween1.OnUpdate = (Action<Tween>) (t => Glitch.Value = t.Eased);
    tween1.OnComplete = (Action<Tween>) (_param1 =>
    {
      Tween tween2 = Tween.Create((Tween.TweenMode) 1, (Ease.Easer) null, 0.05f, true);
      tween2.OnUpdate = (Action<Tween>) (t => Glitch.Value = 1f - t.Eased);
      ((Entity) this.player).Add((Component) tween2);
    });
    ((Entity) this.player).Add((Component) tween1);
  }

  private IEnumerator Cutscene(Level level)
  {
    this.player.StateMachine.State = Player.StDummy;
    this.player.StateMachine.Locked = true;
    while (!((Actor) this.player).OnGround(1) || (double) this.player.Speed.Y < 0.0)
      yield return (object) null;
    yield return (object) 0.5f;
    ((Entity) this).Add((Component) new Coroutine(this.zoomLevel(), true));
    ((Entity) this.boss).Visible = true;
    this.sfx.Play("event:/ricky06/FightSFX/cb-disappear", (string) null, 0.0f);
    this.boss.Sprite.Play("deathBegin", false, false);
    this.deathLoop.Play("event:/ricky06/CutsceneSFX/deathLoop", (string) null, 0.0f);
    yield return (object) 0.24f;
    this.boss.Sprite.Play("deathLoop", false, false);
    for (int i = 15; i > 0; --i)
    {
      this.boss.EmitParticles();
      NPC_Boss boss = this.boss;
      Rectangle bounds = level.Bounds;
      Vector2 vector2 = new Vector2((float) (bounds.Left + Calc.Random.Next(370, 580)), level.Camera.Top + (float) Calc.Random.Next(30, 120));
      ((Entity) boss).Position = vector2;
      this.GlitchLevel();
      yield return (object) (float) ((double) i / 30.0);
    }
    ((Entity) this.boss).Position = this.original_pos;
    yield return (object) 0.5f;
    this.boss.Sprite.Play("deathEnd", false, false);
    global::Celeste.Audio.Play("event:/ricky06/CutsceneSFX/deathExplosion2");
    this.tween = Tween.Create((Tween.TweenMode) 1, (Ease.Easer) null, 1.1f, true);
    this.tween.OnUpdate = (Action<Tween>) (t => Glitch.Value = t.Eased * 2f);
    this.tween.OnComplete = (Action<Tween>) (t => this.EndCutscene(level, true));
    ((Entity) this.player).Add((Component) this.tween);
  }

  private IEnumerator zoomLevel()
  {
    yield return (object) this.level.ZoomTo(new Vector2(160f, 76f), 1.2f, 0.5f);
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
    this.player.StateMachine.Locked = false;
    this.player.StateMachine.State = Player.StNormal;
    this.player.Speed.Y = 0.0f;
    while (((Entity) this.player).CollideCheck<Solid>())
    {
      Player player = this.player;
      ((Entity) player).Y = ((Entity) player).Y - 1f;
    }
    if (this.tween != null)
      ((Entity) this.player).Remove((Component) this.tween);
    global::Celeste.Audio.SetMusic("event:/ricky06/cpostsilence", true, true);
    Glitch.Value = 0.0f;
    level.ResetZoom();
    LevelData leveldata = level.Session.LevelData;
    ((Scene) level).OnEndOfFrame += (Action) (() =>
    {
      Vector2 position = ((Entity) this.player).Position;
      Player player = this.player;
      ((Entity) player).Position = ((((Entity) player).Position) - (leveldata.Position));
      Camera camera = level.Camera;
      camera.Position = ((camera.Position) - (leveldata.Position));
      int dashes = this.player.Dashes;
      float stamina = this.player.Stamina;
      Facings facing = this.player.Facing;
      level.Session.Level = "d-heart";
      Leader leader = ((Entity) this.player).Get<Leader>();
      foreach (Follower follower in leader.Followers)
      {
        if (((Component) follower).Entity != null)
        {
          ((Component) follower).Entity.AddTag(((int) (Tags.Global)));
          level.Session.DoNotLoad.Add(follower.ParentEntityID);
        }
      }
      ((Scene) level).Remove((Entity) this.player);
      level.UnloadLevel();
      ((Scene) level).Add((Entity) this.player);
      level.LoadLevel((Player.IntroTypes) 0, false);
      leveldata = level.Session.LevelData;
      level.Session.RespawnPoint = new Vector2?(Calc.ClosestTo(level.Session.LevelData.Spawns, new Vector2(-3000f, 690f)));
      ((Entity) this.player).Position = level.Session.RespawnPoint.Value;
      this.player.Dashes = dashes;
      this.player.Stamina = stamina;
      this.player.Facing = facing;
      level.Camera.Position = level.GetFullCameraTargetAt(this.player, ((Entity) this.player).Position);
      foreach (Follower follower in leader.Followers)
      {
        if (((Component) follower).Entity != null)
        {
          Entity entity = ((Component) follower).Entity;
          entity.Position = ((entity.Position) + (((((Entity) this.player).Position) - (position))));
          ((Component) follower).Entity.RemoveTag(((int) (Tags.Global)));
          level.Session.DoNotLoad.Remove(follower.ParentEntityID);
        }
      }
      for (int index1 = 0; index1 < leader.PastPoints.Count; ++index1)
      {
        List<Vector2> pastPoints = leader.PastPoints;
        int index2 = index1;
        pastPoints[index2] = ((pastPoints[index2]) + (((((Entity) this.player).Position) - (position))));
      }
      leader.TransferFollowers();
      if (!this.WasSkipped)
      {
        Tween tween = Tween.Create((Tween.TweenMode) 1, (Ease.Easer) null, 1f, true);
        tween.OnUpdate = (Action<Tween>) (t => Glitch.Value = 1f - t.Eased);
        ((Entity) this.player).Add((Component) tween);
      }
      else
        Glitch.Value = 0.0f;
    });
    if (this.boss == null)
      return;
    ((Entity) this.boss).RemoveSelf();
  }
}

