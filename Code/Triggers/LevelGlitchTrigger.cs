// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.LevelGlitchTrigger
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\AppData\Local\Temp\Mihavec\86958f101c\Code\ricky06ModPack.dll

#nullable disable
namespace Celeste.Entities
{
  [CustomEntity(new string[] { "MaggyHelper/LevelGlitchTrigger" })]
  [Tracked(false)]
  internal class LevelGlitchTrigger : Trigger
  {
    public string NewRoomId;
    public bool Delay;
    public float Time;
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
      NewRoomId = data.Attr(nameof(NewRoomId));
      Delay = data.Bool(nameof(Delay));
      Time = data.Float(nameof(Time));
      id = entId;
      differentSpawn = data.Bool(nameof(differentSpawn));
      playerX = data.Float(nameof(playerX));
      playerY = data.Float(nameof(playerY));
      deleteAfterEnter = data.Bool("DeleteAfterEnter");
      noMusicOnTeleport = data.Bool(nameof(noMusicOnTeleport));
      lowPass = data.Bool(nameof(lowPass));
      lowPassValue = data.Float(nameof(lowPassValue));
    }

    public override void OnEnter(global::Celeste.Player player)
    {
      base.OnEnter(player);
      if ((Scene as Level).Session.GetFlag("DoNotLoad" + this.id.ToString()) || Delay && Time >= 0.0 || NewRoomId.Length == 0)
        return;
      Level level = SceneAs<Level>();
      if (lowPass)
        Audio.SetMusicParam("lowpass", lowPassValue);
      Audio.Play("event:/new_content/game/10_farewell/glitch_short");
      LevelData leveldata = level.Session.LevelData;
      Session session = (Scene as Level).Session;
      EntityID id = this.id;
      if (deleteAfterEnter)
      {
        session.SetFlag("DoNotLoad" + id.ToString());
        foreach (TeleporterDeco entity in Scene.Tracker.GetEntities<TeleporterDeco>())
          session.SetFlag("DoNotLoad" + entity.Eid.ToString());
      }
      Tween tween1 = Tween.Create(Tween.TweenMode.Oneshot, duration: 0.1f, start: true);
      tween1.OnUpdate = t => Glitch.Value = 0.7f * t.Eased;
      tween1.OnComplete = param1 => level.OnEndOfFrame += () =>
      {
        Vector2 position = player.Position;
        global::Celeste.Player player1 = player;
        player1.Position = player1.Position - leveldata.Position;
        level.Camera.Position -= leveldata.Position;
        int dashes = player.Dashes;
        float stamina = player.Stamina;
        Facings facing = player.Facing;
        level.Session.Level = NewRoomId;
        Leader leader = player.Get<Leader>();
        foreach (Follower follower in leader.Followers)
        {
          if (follower.Entity != null)
          {
            follower.Entity.AddTag((int)Tags.Global);
            level.Session.DoNotLoad.Add(follower.ParentEntityID);
          }
        }
        level.Remove(player);
        level.UnloadLevel();
        level.Add(player);
        level.LoadLevel(global::Celeste.Player.IntroTypes.Transition);
        leveldata = level.Session.LevelData;
        player.Position = differentSpawn ? leveldata.Position + new Vector2(playerX, playerY) : player.Position + leveldata.Position;
        player.Dashes = dashes;
        player.Stamina = stamina;
        player.Facing = facing;
        level.Camera.Position = differentSpawn ? level.GetFullCameraTargetAt(player, player.Position) : level.Camera.Position + leveldata.Position;
        level.Session.RespawnPoint = new Vector2?(level.Session.LevelData.Spawns.ClosestTo(player.Position));
        foreach (Follower follower in leader.Followers)
        {
          if (follower.Entity != null)
          {
            follower.Entity.Position += player.Position - position;
            follower.Entity.RemoveTag((int)Tags.Global);
            level.Session.DoNotLoad.Remove(follower.ParentEntityID);
          }
        }
        if (noMusicOnTeleport)
          Audio.SetMusic("", allowFadeOut: false);
        for (int index = 0; index < leader.PastPoints.Count; ++index)
          leader.PastPoints[index] += player.Position - position;
        leader.TransferFollowers();
        Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, duration: 0.3f, start: true);
        tween2.OnUpdate = t => Glitch.Value = (float)(0.699999988079071 * (1.0 - (double)t.Eased));
        player.Add(tween2);
      };
      player.Add(tween1);
    }

    public override void OnStay(global::Celeste.Player player)
    {
      base.OnStay(player);
      if (Delay && Time >= 0.0 || !Delay)
        return;
      OnEnter(player);
    }

    public override void Update()
    {
      base.Update();
      if (Time < 0.0)
        return;
      Time -= Engine.DeltaTime;
    }
  }
}
