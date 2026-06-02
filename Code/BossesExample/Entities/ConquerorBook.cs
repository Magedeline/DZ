// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.ConquerorBook
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

internal class ConquerorBook : CutsceneEntity
{
  private Player player;
  private string imageKey;
  private string textKey;
  private ConquerorBook.ConquerorPage page;
  private bool isTomb;
  private bool noSound;
  private Level level;

  public ConquerorBook(Player player, string imageKey, bool isTomb, string textKey, bool noSound = false)
    : base(true, false)
  {
    this.player = player;
    this.imageKey = imageKey;
    this.isTomb = isTomb;
    this.textKey = textKey;
    this.noSound = noSound;
  }

  public override void Added(Scene scene)
  {
    base.Added(scene);
    this.level = ((Entity) this).SceneAs<Level>();
  }

  public override void OnBegin(Level level)
  {
    ((Entity) this).Add((Component) new Coroutine(this.Routine(), true));
  }

  private IEnumerator Routine()
  {
    this.player.StateMachine.State = Player.StDummy;
    this.player.StateMachine.Locked = true;
    if (this.textKey.Length != 0)
      yield return (object) Textbox.Say(this.textKey, Array.Empty<Func<IEnumerator>>());
    this.page = new ConquerorBook.ConquerorPage(this.imageKey, this.isTomb, this.noSound, this);
    ((Entity) this).Scene.Add((Entity) this.page);
    yield return (object) this.page.EaseIn();
    yield return (object) this.page.Wait();
    yield return (object) this.page.EaseOut();
    if (this.isTomb && !this.level.Session.GetFlag("cp_secret_reveal") && this.page != null && this.page.getImageKey().Length == 3)
    {
      global::Celeste.Audio.Play("event:/ricky06/CutsceneSFX/earthquake");
      this.level.Shake(1.1f);
      Rectangle bounds1 = this.level.Bounds;
      double num1 = (double) bounds1.Left + 1480.0;
      Rectangle bounds2 = this.level.Bounds;
      double num2 = (double) bounds2.Top + 1380.0;
      yield return (object) CutsceneEntity.CameraTo(new Vector2((float) num1, (float) num2), 1.5f, Ease.SineInOut, 0.0f);
      foreach (VanishingWall vw in ((Entity) this).Scene.Tracker.GetEntities<VanishingWall>())
        vw.RemoveBlock();
      yield return (object) 1f;
      yield return (object) this.level.ZoomBack(0.5f);
    }
    this.page = (ConquerorBook.ConquerorPage) null;
    this.EndCutscene(this.Level, true);
  }

  public bool getFlag(string flag) => this.level.Session.GetFlag(flag);

  public override void OnEnd(Level level)
  {
    this.player.StateMachine.Locked = false;
    this.player.StateMachine.State = Player.StNormal;
    if (this.page != null)
      this.page.RemoveSelf();
    if (!this.isTomb || this.page == null || this.page.getImageKey().Length != 3)
      return;
    foreach (VanishingWall entity in ((Entity) this).Scene.Tracker.GetEntities<VanishingWall>())
      entity.RemoveBlock();
    level.Session.SetFlag("cp_secret_reveal", true);
  }

  private class ConquerorPage : Entity
  {
    private MTexture paper;
    private float alpha = 1f;
    private float scale = 1f;
    private float rotation = 0.0f;
    private float timer = 0.0f;
    private bool easingOut;
    private VirtualRenderTarget target;
    private string imageKey;
    private bool noSound;

    public ConquerorPage(string imageKey, bool isTomb, bool noSound, ConquerorBook book)
    {
      this.Tag = ((int) (Tags.HUD));
      if (isTomb)
      {
        imageKey = "";
        if (global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.Settings.ResetKeysForSession)
        {
          if (book.getFlag("CP-ConquerorKeys-t_key"))
            imageKey += "t";
          if (book.getFlag("CP-ConquerorKeys-f_key"))
            imageKey += "f";
          if (book.getFlag("CP-ConquerorKeys-a_key"))
            imageKey += "a";
        }
        else
        {
          if (global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.SaveData.StoneFlags.Contains("t_key"))
            imageKey += "t";
          if (global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.SaveData.StoneFlags.Contains("f_key"))
            imageKey += "f";
          if (global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.SaveData.StoneFlags.Contains("a_key"))
            imageKey += "a";
        }
        if (imageKey.Length == 0)
          imageKey = "none";
      }
      this.paper = GFX.Gui[imageKey];
      this.Add((Component) new BeforeRenderHook(new Action(this.BeforeRender)));
      this.imageKey = imageKey;
      this.noSound = noSound;
    }

    public string getImageKey() => this.imageKey;

    public IEnumerator EaseIn()
    {
      if (!this.noSound)
        global::Celeste.Audio.Play("event:/game/03_resort/memo_in");
      Vector2 center = ((new Vector2((float) Engine.Width, (float) Engine.Height)) / (2f));
      Vector2 from = ((center) + (new Vector2(0.0f, 200f)));
      Vector2 to = center;
      float rFrom = -0.1f;
      float rTo = 0.05f;
      for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime)
      {
        this.Position = ((from) + (((((to) - (from))) * (Ease.CubeOut.Invoke(p)))));
        this.alpha = Ease.CubeOut.Invoke(p);
        this.rotation = rFrom + (rTo - rFrom) * Ease.CubeOut.Invoke(p);
        yield return (object) null;
      }
    }

    public IEnumerator Wait()
    {
      while (!Input.MenuConfirm.Pressed)
        yield return (object) null;
      global::Celeste.Audio.Play("event:/ui/main/button_lowkey");
    }

    public IEnumerator EaseOut()
    {
      if (!this.noSound)
        global::Celeste.Audio.Play("event:/game/03_resort/memo_out");
      this.easingOut = true;
      Vector2 from = this.Position;
      Vector2 to = ((((new Vector2((float) Engine.Width, (float) Engine.Height)) / (2f))) + (new Vector2(0.0f, -200f)));
      float rFrom = this.rotation;
      float rTo = this.rotation + 0.1f;
      for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime * 1.5f)
      {
        this.Position = ((from) + (((((to) - (from))) * (Ease.CubeIn.Invoke(p)))));
        this.alpha = 1f - Ease.CubeIn.Invoke(p);
        this.rotation = rFrom + (rTo - rFrom) * Ease.CubeIn.Invoke(p);
        yield return (object) null;
      }
      this.RemoveSelf();
    }

    public void BeforeRender()
    {
      if (this.target == null)
        this.target = VirtualContent.CreateRenderTarget("conqueror-page", this.paper.Width, this.paper.Height, false, true, 0);
      Engine.Graphics.GraphicsDevice.SetRenderTarget(((RenderTarget2D) (this.target)));
      Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
      Draw.SpriteBatch.Begin((SpriteSortMode) 0, BlendState.AlphaBlend);
      this.paper.Draw(Vector2.Zero);
      Draw.SpriteBatch.End();
    }

    public override void Removed(Scene scene)
    {
      if (this.target != null)
        ((VirtualAsset) this.target).Dispose();
      this.target = (VirtualRenderTarget) null;
      base.Removed(scene);
    }

    public override void SceneEnd(Scene scene)
    {
      if (this.target != null)
        ((VirtualAsset) this.target).Dispose();
      this.target = (VirtualRenderTarget) null;
      base.SceneEnd(scene);
    }

    public override void Update()
    {
      this.timer += Engine.DeltaTime;
      base.Update();
    }

    public override void Render()
    {
      if (this.Scene is Level scene && (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null || scene.SkippingCutscene) || this.target == null)
        return;
      Draw.SpriteBatch.Draw((Texture2D) ((RenderTarget2D) (this.target)), this.Position, new Rectangle?(this.target.Bounds), ((Color.White) * (this.alpha)), this.rotation, ((new Vector2((float) ((VirtualAsset) this.target).Width, (float) ((VirtualAsset) this.target).Height)) / (2f)), this.scale, (SpriteEffects) 0, 0.0f);
      if (!this.easingOut)
        GFX.Gui["textboxbutton"].DrawCentered(((this.Position) + (new Vector2((float) (((VirtualAsset) this.target).Width / 2 + 40), (float) (((VirtualAsset) this.target).Height / 2 + ((double) this.timer % 1.0 < 0.25 ? 6 : 0))))));
    }
  }
}
