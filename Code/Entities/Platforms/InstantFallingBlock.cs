// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.InstantFallingBlock
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\AppData\Local\Temp\Mihavec\86958f101c\Code\ricky06ModPack.dll

#nullable disable
namespace Celeste.Entities
{
  [CustomEntity(new string[] { "MaggyHelper/InstantFallingBlock" })]
  internal class InstantFallingBlock(EntityData data, Vector2 offset, EntityID entId) : FallingBlock(data, offset)
  {
    private bool manualTrigger = data.Bool(nameof(manualTrigger));

    public override void Added(Scene scene)
    {
      base.Added(scene);
      if (manualTrigger)
      {
        if (!SceneAs<Level>().Session.GetFlag("c-08-fallen"))
          return;
        Triggered = true;
      }
      else if ((scene as Level).Session.GetFlag("DoNotLoad" + entId.ToString()))
        Triggered = true;
    }

    public override void OnShake(Vector2 amount)
    {
      base.OnShake(amount);
      if (manualTrigger)
        return;
      (Scene as Level).Session.SetFlag("DoNotLoad" + entId.ToString());
    }
  }
}
