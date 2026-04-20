// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.DoubleDashTrigger
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\AppData\Local\Temp\Mihavec\86958f101c\Code\ricky06ModPack.dll

#nullable disable
namespace Celeste.Entities
{
  [CustomEntity(new string[] { "MaggyHelper/DoubleDashTrigger" })]
  [Tracked(false)]
  [HotReloadable]
  internal class DoubleDashTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
  {
    private Level level;

    public override void Added(Scene scene)
    {
      base.Added(scene);
      level = SceneAs<Level>();
    }

    public override void OnEnter(global::Celeste.Player player) => level.Session.Inventory.Dashes = 2;
  }
}




