// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.BossDebris
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Microsoft.Xna.Framework;
using Monocle;
using System;

#nullable disable
namespace Celeste.Mod.DZ.BossesExample.Entities;

internal class BossDebris : Entity
{
  public Sprite debris;

  public BossDebris()
    : base(Vector2.Zero)
  {
    this.Add((Component) (this.debris = global::Celeste.Mod.DZ.BossesExample.BossesExampleModule.SpriteBank.Create("summit_debris")));
    ((Component) this.debris).Visible = false;
    this.debris.OnFinish = (Action<string>) (_param1 => this.RemoveSelf());
  }

  public BossDebris Init(Vector2 pos)
  {
    ((GraphicsComponent) this.debris).RenderPosition = pos;
    ((Component) this.debris).Visible = true;
    this.debris.Play("shake", false, false);
    return this;
  }
}
