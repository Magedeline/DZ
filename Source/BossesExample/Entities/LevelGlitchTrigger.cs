// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.LevelGlitchTrigger
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

[CustomEntity(new string[] {"ricky06/LevelGlitchTrigger"})]
[Tracked(false)]
internal class LevelGlitchTrigger : Trigger
{
  public string newRoomID;
  public bool delay;
  public float time;
  private EntityID id;
  private float playerX;
  private float playerY;
  private bool differentSpawn;
  private bool deleteAfterEnter;
  private bool noMusicOnTeleport;
  private bool lowPass;
  private float lowPassValue;

  public LevelGlitchTrigger(EntityData data, Vector2 offset, EntityID entId)
    : base(data, offset)
  {
    this.newRoomID = data.Attr(nameof (newRoomID), "");
    this.delay = data.Bool(nameof (delay), false);
    this.time = data.Float(nameof (time), 0.0f);
    this.id = entId;
    this.differentSpawn = data.Bool(nameof (differentSpawn), false);
    this.playerX = data.Float(nameof (playerX), 0.0f);
    this.playerY = data.Float(nameof (playerY), 0.0f);
    this.deleteAfterEnter = data.Bool("DeleteAfterEnter", false);
    this.noMusicOnTeleport = data.Bool(nameof (noMusicOnTeleport), false);
    this.lowPass = data.Bool(nameof (lowPass), false);
    this.lowPassValue = data.Float(nameof (lowPassValue), 0.0f);
  }

  public override void OnEnter(Player player)
  {
    base.OnEnter(player);
    if ((((Entity) this).Scene as Level).Session.GetFlag("DoNotLoad" + this.id.ToString()) || this.delay && (double) this.time >= 0.0 || this.newRoomID.Length == 0)
      return;
    Level level = ((Entity) this).SceneAs<Level>();
    if (this.lowPass)
      Audio.SetMusicParam("lowpass", this.lowPassValue);
    Audio.Play("event:/new_content/game/10_farewell/glitch_short");
    LevelData leveldata = level.Session.LevelData;
    Session session = (((Entity) this).Scene as Level).Session;
    EntityID id = this.id;
    if (this.deleteAfterEnter)
    {
      session.SetFlag("DoNotLoad" + id.ToString(), true);
      foreach (TeleporterDeco entity in ((Entity) this).Scene.Tracker.GetEntities<TeleporterDeco>())
        session.SetFlag("DoNotLoad" + entity.eid.ToString(), true);
    }
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
      level.Session.Level = this.newRoomID;
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
      ((Entity) player).Position = this.differentSpawn ? ((leveldata.Position) + (new Vector2(this.playerX, this.playerY))) : ((((Entity) player).Position) + (leveldata.Position));
      player.Dashes = dashes;
      player.Stamina = stamina;
      player.Facing = facing;
      level.Camera.Position = this.differentSpawn ? level.GetFullCameraTargetAt(player, ((Entity) player).Position) : ((level.Camera.Position) + (leveldata.Position));
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
      if (this.noMusicOnTeleport)
        Audio.SetMusic("", true, false);
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

  public override void OnStay(Player player)
  {
    base.OnStay(player);
    if (this.delay && (double) this.time >= 0.0 || !this.delay)
      return;
    base.OnEnter(player);
  }

  public override void Update()
  {
    ((Entity) this).Update();
    if ((double) this.time < 0.0)
      return;
    this.time -= Engine.DeltaTime;
  }
}
