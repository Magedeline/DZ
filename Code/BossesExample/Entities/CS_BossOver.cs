// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.CS_BossOver
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

public class CS_BossOver : CutsceneEntity
{
  private NPC_Boss boss;
  private Player player;
  private Level level;
  private FadeWipe fade;
  private SoundSource deathLoop;
  private SoundSource deathExplosion;
  private SoundSource sfx;

  public CS_BossOver(NPC boss, Player player)
    : base(true, false)
  {
    this.boss = (NPC_Boss) boss;
    this.player = player;
    ((Entity) this).Add((Component) (this.deathLoop = new SoundSource()));
    ((Entity) this).Add((Component) (this.sfx = new SoundSource()));
    ((Entity) this).Add((Component) (this.deathExplosion = new SoundSource()));
  }

  public override void Added(Scene scene)
  {
    base.Added(scene);
    this.level = ((Entity) this).SceneAs<Level>();
    global::Celeste.Audio.SetMusic("event:/ricky06/cpostsilence", true, true);
  }

  public override void OnBegin(Level level)
  {
    ((Entity) this).Add((Component) new Coroutine(this.Cutscene(level), true));
  }

  public override void Update() => base.Update();

  private IEnumerator Cutscene(Level level)
  {
    this.player.StateMachine.State = Player.StDummy;
    this.player.StateMachine.Locked = true;
    while (!((Actor) this.player).OnGround(1) || (double) this.player.Speed.Y < 0.0)
      yield return (object) null;
    ((Entity) this).Add((Component) new Coroutine(this.zoomLevel(), true));
    ((Entity) this.boss).Visible = true;
    this.sfx.Play("event:/ricky06/FightSFX/cb-disappear", (string) null, 0.0f);
    this.boss.Sprite.Play("deathBegin", false, false);
    this.deathLoop.Play("event:/ricky06/CutsceneSFX/deathLoop", (string) null, 0.0f);
    yield return (object) 0.24f;
    ((Entity) this).Add((Component) new Coroutine(this.FadeOutLevel(), true));
    this.boss.Sprite.Play("deathLoop", false, false);
    for (int i = 0; i < 20; ++i)
    {
      this.boss.EmitParticles();
      level.Shake(0.25f);
      Input.Rumble((RumbleStrength) 1, (RumbleLength) 1);
      yield return (object) 0.25f;
    }
    while (this.fade != null && (double) ((ScreenWipe) this.fade).Percent < 1.0)
      yield return (object) null;
    this.EndCutscene(level, true);
  }

  private IEnumerator FadeDeathSfx()
  {
    float value = 1f;
    for (int i = 0; i < 50; ++i)
    {
      this.deathLoop.Param("fade", value);
      value -= 0.02f;
      yield return (object) 0.1f;
    }
  }

  private IEnumerator FadeOutLevel()
  {
    yield return (object) 1.5f;
    ((Entity) this).Add((Component) new Coroutine(this.FadeDeathSfx(), true));
    FadeWipe fadeWipe = new FadeWipe((Scene) this.Level, false, (Action) null);
    ((ScreenWipe) fadeWipe).Duration = 6f;
    this.fade = fadeWipe;
    ScreenWipe.WipeColor = Color.White;
  }

  private void TeleportToEnd()
  {
    LevelData leveldata = this.level.Session.LevelData;
    ((Scene) this.level).OnEndOfFrame += (Action) (() =>
    {
      Vector2 position = ((Entity) this.player).Position;
      Player player = this.player;
      ((Entity) player).Position = ((((Entity) player).Position) - (leveldata.Position));
      Camera camera = this.level.Camera;
      camera.Position = ((camera.Position) - (leveldata.Position));
      int dashes = this.player.Dashes;
      float stamina = this.player.Stamina;
      Facings facing = this.player.Facing;
      this.level.Session.Level = "d-end";
      Leader leader = ((Entity) this.player).Get<Leader>();
      foreach (Follower follower in leader.Followers)
      {
        if (((Component) follower).Entity != null)
        {
          ((Component) follower).Entity.AddTag(((int) (Tags.Global)));
          this.level.Session.DoNotLoad.Add(follower.ParentEntityID);
        }
      }
      ((Scene) this.level).Remove((Entity) this.player);
      this.level.UnloadLevel();
      ((Scene) this.level).Add((Entity) this.player);
      this.level.LoadLevel((Player.IntroTypes) 0, false);
      leveldata = this.level.Session.LevelData;
      this.level.Session.RespawnPoint = new Vector2?(Calc.ClosestTo(this.level.Session.LevelData.Spawns, new Vector2(-3000f, 690f)));
      ((Entity) this.player).Position = this.level.Session.RespawnPoint.Value;
      this.player.Dashes = dashes;
      this.player.Stamina = stamina;
      this.player.Facing = facing;
      this.level.Camera.Position = this.level.GetFullCameraTargetAt(this.player, ((Entity) this.player).Position);
      foreach (Follower follower in leader.Followers)
      {
        if (((Component) follower).Entity != null)
        {
          Entity entity = ((Component) follower).Entity;
          entity.Position = ((entity.Position) + (((((Entity) this.player).Position) - (position))));
          ((Component) follower).Entity.RemoveTag(((int) (Tags.Global)));
          this.level.Session.DoNotLoad.Remove(follower.ParentEntityID);
        }
      }
      for (int index1 = 0; index1 < leader.PastPoints.Count; ++index1)
      {
        List<Vector2> pastPoints = leader.PastPoints;
        int index2 = index1;
        pastPoints[index2] = ((pastPoints[index2]) + (((((Entity) this.player).Position) - (position))));
      }
      leader.TransferFollowers();
      FadeWipe fadeWipe1 = new FadeWipe((Scene) this.level, true, (Action) null);
      ((ScreenWipe) fadeWipe1).Duration = 2f;
      FadeWipe fadeWipe2 = fadeWipe1;
      ScreenWipe.WipeColor = Color.White;
      FadeWipe fadeWipe3 = fadeWipe2;
      ((ScreenWipe) fadeWipe3).OnComplete = ((ScreenWipe) fadeWipe3).OnComplete + (Action) (() => ScreenWipe.WipeColor = Color.Black);
    });
  }

  private IEnumerator zoomLevel()
  {
    yield return (object) this.level.ZoomTo(new Vector2(160f, 76f), 1.2f, 0.5f);
  }

  public override void OnEnd(Level level)
  {
    this.TeleportToEnd();
    this.player.StateMachine.Locked = false;
    this.player.StateMachine.State = Player.StNormal;
    this.player.Speed.Y = 0.0f;
    while (((Entity) this.player).CollideCheck<Solid>())
    {
      Player player = this.player;
      ((Entity) player).Y = ((Entity) player).Y - 1f;
    }
    global::Celeste.Audio.SetMusic("event:/ricky06/cpostsilence", true, true);
    Glitch.Value = 0.0f;
    level.ResetZoom();
    if (this.boss == null)
      return;
    ((Entity) this.boss).RemoveSelf();
  }
}
