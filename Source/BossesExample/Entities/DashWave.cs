// Decompiled with JetBrains decompiler
// Type: Celeste.Mod.ricky06ModPack.Entities.DashWave
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Microsoft.Xna.Framework;
using Monocle;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

public class DashWave : Entity
{
  private Sprite sprite;
  private Player player;

  public DashWave(Player player)
    : base(Vector2.Zero)
  {
    this.Add((Component) (this.sprite = global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.SpriteBank.Create("dash_wave")));
    this.sprite.Play("idle", false, false);
    this.Depth = -11000;
    this.player = player;
    this.AddTag(((int) (Tags.Global)));
  }

  public virtual void Update()
  {
    base.Update();
    if (global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.hasDestroyDash)
      this.Visible = true;
    else
      this.Visible = false;
    this.Position = ((Entity) this.player).Position;
  }

  public void Remove() => this.RemoveSelf();
}
