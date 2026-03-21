// Decompiled with JetBrains decompiler
// Type: Celeste.CS02_Journal
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Users\User\OneDrive\Desktop\Celeste!\Celeste\Celeste.exe

#nullable disable
namespace MaggyHelper.Cutscenes
{
  [HotReloadable]
  public class Cs02JournalMod : CutsceneEntity
  {
    private const string read_once_flag = "poem_read";
    private global::Celeste.Player player;
    private PoemPage poem;

    public Cs02JournalMod(global::Celeste.Player player)
      : base()
    {
      this.player = player;
    }

    public override void OnBegin(Level level)
    {
      Add( new Coroutine(routine()));
    }

    private IEnumerator routine()
    {
      var cs02Journal = this;
      cs02Journal.player.StateMachine.State = 11;
      cs02Journal.player.StateMachine.Locked = true;
      if (!cs02Journal.Level.Session.GetFlag("poem_read"))
      {
        yield return  Textbox.Say("ch2_cloverjournal");
        yield return  0.1f;
      }
      cs02Journal.poem = new PoemPage();
      cs02Journal.Scene.Add( cs02Journal.poem);
      yield return  cs02Journal.poem.EaseIn();
      while (!Input.MenuConfirm.Pressed)
        yield return  null;
      Audio.Play("event:/ui/main/button_lowkey");
      yield return  cs02Journal.poem.EaseOut();
      cs02Journal.poem =  null;
      cs02Journal.endCutscene(cs02Journal.Level);
    }

        private void endCutscene(Level level)
        {
            level.EndCutscene();
        }

        public override void OnEnd(Level level)
    {
      player.StateMachine.Locked = false;
      player.StateMachine.State = 0;
      level.Session.SetFlag("poem_read");
      if (poem == null)
        return;
      poem.RemoveSelf();
    }

    private class PoemPage : Entity
    {
      private const float text_scale = 0.7f;
      private MTexture paper;
      private VirtualRenderTarget target;
      private FancyText.Text text;
      private float alpha = 1f;
      private float scale = 1f;
      private float rotation;
      private float timer;
      private bool easingOut;

      public PoemPage()
      {
        Tag = (int) Tags.HUD;
        paper = GFX.Gui["poempage"];
        text = FancyText.Parse(Dialog.Get("CH2_CLOVER_POEM"), (int) ( (paper.Width - 120) / 0.699999988079071), -1, defaultColor: new Color?(Color.Black * 0.6f));
        Add( new BeforeRenderHook(new Action(BeforeRender)));
      }

      public IEnumerator EaseIn()
      {
        var poemPage = this;
        Audio.Play("event:/game/03_resort/memo_in");
        var vector2 = new Vector2( Engine.Width,  Engine.Height) / 2f;
        var from = vector2 + new Vector2(0.0f, 200f);
        var to = vector2;
        var rFrom = -0.1f;
        var rTo = 0.05f;
        for (var p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime)
        {
          poemPage.Position = from + (to - from) * Ease.CubeOut(p);
          poemPage.alpha = Ease.CubeOut(p);
          poemPage.rotation = rFrom + (rTo - rFrom) * Ease.CubeOut(p);
          yield return  null;
        }
      }

      public IEnumerator EaseOut()
      {
        var poemPage = this;
        Audio.Play("event:/game/03_resort/memo_out");
        poemPage.easingOut = true;
        var from = poemPage.Position;
        var to = new Vector2( Engine.Width,  Engine.Height) / 2f + new Vector2(0.0f, -200f);
        var rFrom = poemPage.rotation;
        var rTo = poemPage.rotation + 0.1f;
        for (var p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime * 1.5f)
        {
          poemPage.Position = from + (to - from) * Ease.CubeIn(p);
          poemPage.alpha = 1f - Ease.CubeIn(p);
          poemPage.rotation = rFrom + (rTo - rFrom) * Ease.CubeIn(p);
          yield return  null;
        }
        poemPage.RemoveSelf();
      }

      public void BeforeRender()
      {
        if (target == null)
          target = VirtualContent.CreateRenderTarget("journal-poem", paper.Width, paper.Height);
        Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D) target);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        paper.Draw(Vector2.Zero);
        text.DrawJustifyPerLine(new Vector2( paper.Width,  paper.Height) / 2f, new Vector2(0.5f, 0.5f), Vector2.One * 0.7f, 1f);
        Draw.SpriteBatch.End();
      }

      public override void Removed(Scene scene)
      {
        if (target != null)
          target.Dispose();
        target =  null;
        base.Removed(scene);
      }

      public override void SceneEnd(Scene scene)
      {
        if (target != null)
          target.Dispose();
        target =  null;
        base.SceneEnd(scene);
      }

      public override void Update()
      {
        timer += Engine.DeltaTime;
        base.Update();
      }

      public override void Render()
      {
        if (Scene is Level scene && (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null || scene.SkippingCutscene) || target == null)
          return;
        Draw.SpriteBatch.Draw( (RenderTarget2D) target, Position, new Rectangle?(target.Bounds), Color.White * alpha, rotation, new Vector2( target.Width,  target.Height) / 2f, scale, SpriteEffects.None, 0.0f);
        if (easingOut)
          return;
        GFX.Gui["textboxbutton"].DrawCentered(Position + new Vector2( target.Width / 2 + 40,  target.Height / 2 + ( timer % 1.0 < 0.25 ? 6 : 0)));
      }
    }
  }
}




