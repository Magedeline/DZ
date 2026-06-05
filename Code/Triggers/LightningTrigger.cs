// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.LightningTrigger
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\AppData\Local\Temp\Mihavec\86958f101c\Code\ricky06ModPack.dll

#nullable disable
namespace Celeste.Entities
{
  [CustomEntity(new string[] { "MaggyHelper/LightningTrigger" })]
  [Tracked(false)]
  internal class LightningTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
  {
    private bool triggered = false;
    private Level level;

    public override void Added(Scene scene)
    {
      base.Added(scene);
      level = SceneAs<Level>();
      if (!level.Session.GetFlag("ch20_lightning_trigger_1"))
        return;
      RemoveSelf();
    }

    public override void OnEnter(global::Celeste.Player player)
    {
      if (triggered)
        return;
      base.OnEnter(player);
      Audio.Play("event:/new_content/game/10_farewell/lightning_strike");
      level.Flash(Color.White);
      level.Shake();
      level.Add(new LightningStrike(new Vector2(player.X + 60f, level.Bounds.Bottom - 180), 10, 200f));
      level.Add(new LightningStrike(new Vector2(player.X + 220f, level.Bounds.Bottom - 180), 40, 200f, 0.25f));
      triggered = true;
      level.Session.SetFlag("ch20_lightning_trigger_1");
    }
  }
}

