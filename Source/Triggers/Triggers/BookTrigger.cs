// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.BookTrigger
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\AppData\Local\Temp\Mihavec\86958f101c\Code\ricky06ModPack.dll

#nullable disable
namespace Celeste.Entities
{
  [CustomEntity(new string[] { "DZ/BookTrigger" })]
  [Tracked(false)]
  [HotReloadable]
  internal class BookTrigger : Entity
  {
    public TalkComponent Talker;
    public string ImageKey;
    public string TextKey;
    private bool isTomb;
    private bool noSound;

    public BookTrigger(EntityData data, Vector2 offset)
      : base(data.Position + offset)
    {
      Collider = new Hitbox(data.Width, data.Height);
      ImageKey = data.Attr(nameof(ImageKey));
      isTomb = data.Bool("tomb");
      TextKey = data.Attr(nameof(TextKey));
      noSound = data.Bool(nameof(noSound));
      Vector2 drawAt = new Vector2(data.Width / 2, 0.0f);
      Add(Talker = new TalkComponent(new Rectangle(0, 0, data.Width, data.Height), drawAt, OnTalk));
      Talker.PlayerMustBeFacing = false;
    }

    private void OnTalk(global::Celeste.Player obj)
    {
      if (!string.IsNullOrEmpty(TextKey))
      {
        if (!noSound) Audio.Play("event:/ui/game/talk");

        if (isTomb)
          Scene.Add(new TombstoneDialog(TextKey, ImageKey));
        else
          Scene.Add(new BookDialog(TextKey, ImageKey));
      }
    }
  }

  [HotReloadable]
  internal class BookDialog : Entity
  {
    private string textKey;

    private string imageKey;

    public BookDialog(string textKey, string imageKey)
    {
      this.textKey = textKey;
      this.imageKey = imageKey;

      // Initialize the dialog with the provided text and image
      Add(new Image(GFX.Game[imageKey]));
    }
  }

  [HotReloadable]
  internal class TombstoneDialog : Entity
  {
    private string textKey;

    private string imageKey;

    public TombstoneDialog(string textKey, string imageKey)
    {
      this.textKey = textKey;
      this.imageKey = imageKey;

      // Initialize the dialog with the provided text and image
      Add(new Image(GFX.Game[imageKey]));
    }
  }
}
