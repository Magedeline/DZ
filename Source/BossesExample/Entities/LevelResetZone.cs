// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.LevelResetZone
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/LevelResetZone"})]
[Tracked(false)]
internal class LevelResetZone : SeekerBarrier
{
  public string roomID;
  public float respawnTimer = 1f;
  private int spawnX;
  private int spawnY;
  private List<Vector2> particles1 = new List<Vector2>();
  private List<Vector2> particles2 = new List<Vector2>();
  private new float[] speeds = new float[3]{ 12f, 20f, 40f };
  private float opacity = 1f;
  private bool grow = true;
  private float appear_opacity = 0.0f;
  private MTexture hourglassTexture = GFX.Game["util/hourglass"];

  public LevelResetZone(EntityData data, Vector2 offset)
    : base(data, offset)
  {
    this.roomID = data.Attr(nameof (roomID), "");
    this.spawnX = data.Int(nameof (spawnX), 0);
    this.spawnY = data.Int(nameof (spawnY), 0);
    for (int index = 0; (double) index < (double) ((Entity) this).Width * (double) ((Entity) this).Height / 32.0; ++index)
      this.particles1.Add(new Vector2(Calc.NextFloat(Calc.Random, ((Entity) this).Width - 1f), Calc.NextFloat(Calc.Random, ((Entity) this).Height - 1f)));
    for (int index = 0; (double) index < (double) ((Entity) this).Width * (double) ((Entity) this).Height / 300.0; ++index)
      this.particles2.Add(new Vector2(Calc.NextFloat(Calc.Random, ((Entity) this).Width - 5f), Calc.NextFloat(Calc.Random, ((Entity) this).Height - 9f)));
  }

  public override void Added(Scene scene)
  {
    base.Added(scene);
    scene.Tracker.GetEntity<SeekerBarrierRenderer>()?.Untrack((SeekerBarrier) this);
    scene.Tracker.GetEntity<ResetZoneRenderer>()?.Track((SeekerBarrier) this);
  }

  public override void Removed(Scene scene)
  {
    base.Removed(scene);
    scene.Tracker.GetEntity<ResetZoneRenderer>().Untrack((SeekerBarrier) this);
  }

  public override void Update()
  {
    base.Update();
    if (((Entity) this).Scene.OnInterval(1f))
      ((Entity) this).SceneAs<Level>().Displacement.AddBurst(((Entity) this).Center, 2f, 0.0f, 24f, 0.6f, (Ease.Easer) null, (Ease.Easer) null);
    if ((double) this.respawnTimer > 0.0)
      this.respawnTimer -= Engine.DeltaTime;
    if ((double) this.respawnTimer <= 0.0 && (double) this.appear_opacity < 1.0)
    {
      this.appear_opacity += 0.025f;
      if ((double) this.appear_opacity > 1.0)
        this.appear_opacity = 1f;
    }
    Player entity = ((Entity) this).Scene.Tracker.GetEntity<Player>();
    if (entity != null && ((Entity) this).CollideCheck((Entity) entity) && (double) this.respawnTimer < 0.0 && this.roomID.Length != 0)
      this.ResetLevel(this.roomID, "event:/new_content/game/10_farewell/glitch_short");
    int length1 = this.speeds.Length;
    float height1 = ((Entity) this).Height;
    int index1 = 0;
    for (int count = this.particles1.Count; index1 < count; ++index1)
    {
      Vector2 vector2 = ((this.particles1[index1]) + (((((Vector2.UnitY) * (this.speeds[index1 % length1]))) * (Engine.DeltaTime))));
      vector2.Y %= height1 - 1f;
      this.particles1[index1] = vector2;
    }
    int length2 = this.speeds.Length;
    float height2 = ((Entity) this).Height;
    int index2 = 0;
    for (int count = this.particles2.Count; index2 < count; ++index2)
    {
      Vector2 vector2 = ((this.particles2[index2]) + (((((((Vector2.UnitY) * (this.speeds[index2 % length2]))) / (5f))) * (Engine.DeltaTime))));
      vector2.Y %= height2 - 9f;
      this.particles2[index2] = vector2;
    }
    if ((double) this.opacity <= 0.0)
      this.grow = true;
    else if ((double) this.opacity >= 1.0)
      this.grow = false;
    if (this.grow)
      this.opacity += 0.01f;
    else
      this.opacity -= 0.01f;
  }

  public void ResetLevel(string roomID, string audioID)
  {
    Player player = ((Entity) this).Scene.Tracker.GetEntity<Player>();
    Level level = ((Entity) this).SceneAs<Level>();
    LevelData leveldata = level.Session.LevelData;
    if (audioID != null)
      Audio.Play(audioID, ((Entity) player).Position);
    level.Flash(((Color.White) * (0.2f)), false);
    ((Scene) level).OnEndOfFrame += (Action) (() =>
    {
      Vector2 position = ((Entity) player).Position;
      Vector2 cameraOffset = level.CameraOffset;
      Player player1 = player;
      ((Entity) player1).Position = ((((Entity) player1).Position) - (leveldata.Position));
      Camera camera1 = level.Camera;
      camera1.Position = ((camera1.Position) - (leveldata.Position));
      int dashes = player.Dashes;
      float stamina = player.Stamina;
      Facings facing = player.Facing;
      level.Session.Level = roomID;
      bool hasDestroyDash = global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.hasDestroyDash;
      bool destroyDashActive = global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.destroyDashActive;
      Leader leader = ((Entity) player).Get<Leader>();
      foreach (Follower follower in leader.Followers)
      {
        if (((Component) follower).Entity != null)
        {
          ((Component) follower).Entity.AddTag(((int) (Tags.Global)));
          level.Session.DoNotLoad.Add(follower.ParentEntityID);
        }
      }
      foreach (NoResetMoveBlock entity in ((Entity) player).Scene.Tracker.GetEntities<NoResetMoveBlock>())
      {
        if (entity.startedMoving)
        {
          foreach (StaticMover component in ((Entity) player).Scene.Tracker.GetComponents<StaticMover>())
          {
            if (component.IsRiding((Solid) entity))
            {
              ((Component) component).Entity.AddTag(((int) (Tags.Global)));
              try
              {
                level.Session.DoNotLoad.Add(global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.noResetDict[((Component) component).Entity]);
              }
              catch
              {
                Logger.Log("ricky06ModPack KeyError", "Key does not exist");
              }
            }
          }
          entity.makeGlobal();
          level.Session.DoNotLoad.Add(entity.eid);
        }
      }
      global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.lrzTransition = true;
      ((Scene) level).Remove((Entity) player);
      level.UnloadLevel();
      ((Scene) level).Add((Entity) player);
      level.LoadLevel((Player.IntroTypes) 0, false);
      leveldata = level.Session.LevelData;
      Player player2 = player;
      ((Entity) player2).Position = ((((Entity) player2).Position) + (leveldata.Position));
      player.Dashes = dashes;
      player.Stamina = stamina;
      player.Facing = facing;
      Camera camera2 = level.Camera;
      camera2.Position = ((camera2.Position) + (leveldata.Position));
      level.CameraOffset = cameraOffset;
      global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.hasDestroyDash = hasDestroyDash;
      global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.destroyDashActive = destroyDashActive;
      if (this.spawnX != 0 && this.spawnY != 0)
      {
        level.Session.RespawnPoint = new Vector2?(Calc.ClosestTo(level.Session.LevelData.Spawns, new Vector2((float) this.spawnX, (float) this.spawnY)));
        ((Entity) player).Position = level.Session.RespawnPoint.Value;
      }
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
      foreach (NoResetMoveBlock entity in ((Entity) player).Scene.Tracker.GetEntities<NoResetMoveBlock>())
      {
        if (entity.startedMoving)
        {
          foreach (StaticMover component in ((Entity) player).Scene.Tracker.GetComponents<StaticMover>())
          {
            if (component.IsRiding((Solid) entity))
            {
              ((Component) component).Entity.RemoveTag(((int) (Tags.Global)));
              try
              {
                level.Session.DoNotLoad.Remove(global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.noResetDict[((Component) component).Entity]);
              }
              catch
              {
              }
            }
          }
          entity.removeGlobal();
          level.Session.DoNotLoad.Remove(entity.eid);
        }
      }
      for (int index1 = 0; index1 < leader.PastPoints.Count; ++index1)
      {
        List<Vector2> pastPoints = leader.PastPoints;
        int index2 = index1;
        pastPoints[index2] = ((pastPoints[index2]) + (((((Entity) player).Position) - (position))));
      }
      leader.TransferFollowers();
      global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.lrzTransition = false;
      level.Displacement.AddBurst(((Entity) this).Center, 0.1f, 12f, 60f, 1f, (Ease.Easer) null, (Ease.Easer) null);
      level.Displacement.AddBurst(((Entity) this).Center, 0.1f, 24f, 72f, 1f, (Ease.Easer) null, (Ease.Easer) null);
      level.Displacement.AddBurst(((Entity) this).Center, 0.1f, 36f, 84f, 1f, (Ease.Easer) null, (Ease.Easer) null);
    });
  }

  public override void Render()
  {
    if ((double) this.respawnTimer > 0.0)
      return;
    Color color = ((Color.Red) * (0.5f));
    foreach (Vector2 vector2 in this.particles1)
      Draw.Pixel.Draw(((((Entity) this).Position) + (vector2)), Vector2.Zero, ((color) * (this.appear_opacity)));
    foreach (Vector2 vector2 in this.particles2)
    {
      this.opacity = (double) this.opacity < 0.0 ? 0.0f : this.opacity;
      this.opacity = (double) this.opacity > 1.0 ? 1f : this.opacity;
      Draw.SpriteBatch.Draw(this.hourglassTexture.Texture.Texture_Safe, ((((Entity) this).Position) + (vector2)), ((((Color.Red) * (this.opacity))) * (this.appear_opacity)));
    }
    if (this.Flashing)
      Draw.Rect(((Entity) this).Collider, ((((Color.White) * (this.Flash))) * (0.5f)));
  }
}
