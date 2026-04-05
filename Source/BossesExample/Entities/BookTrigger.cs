// Decompiled with JetBrains decompiler
// Type: Celeste.Mod.ricky06ModPack.Entities.BookTrigger
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/BookTrigger"})]
[Tracked(false)]
internal class BookTrigger : Entity
{
  public TalkComponent Talker;
  public string imageKey;
  public string textKey;
  private bool isTomb;
  private bool noSound;

  public BookTrigger(EntityData data, Vector2 offset)
    : base(((data.Position) + (offset)))
  {
    this.Collider = (Collider) new Hitbox((float) data.Width, (float) data.Height, 0.0f, 0.0f);
    this.imageKey = data.Attr(nameof (imageKey), "");
    this.isTomb = data.Bool("tomb", false);
    this.textKey = data.Attr(nameof (textKey), "");
    this.noSound = data.Bool(nameof (noSound), false);
    Vector2 vector2 = new Vector2((float) (data.Width / 2), 0.0f);
    this.Add((Component) (this.Talker = new TalkComponent(new Rectangle(0, 0, data.Width, data.Height), vector2, new Action<Player>(this.OnTalk), (TalkComponent.HoverDisplay) null)));
    this.Talker.PlayerMustBeFacing = false;
  }

  public void OnTalk(Player player)
  {
    this.Scene.Add((Entity) new ConquerorBook(player, this.imageKey, this.isTomb, this.textKey, this.noSound));
  }
}
