// Cutscene for Chapter 20 Bird Guidance Intro
// Based on CS19_AnotherDimensionIntro structure

#nullable disable
namespace Celeste.Cutscenes
{
  public class Cs20BirdGuidanceIntro : CutsceneEntity
  {
    public const string FLAG = "bird_guidance_intro";
    private global::Celeste.Player player;
    private BirdNPC bird;
    private float fade = 1f;

    public Cs20BirdGuidanceIntro(global::Celeste.Player player)
      : base()
    {
      this.Depth = -8500;
      this.player = player;
    }

    public override void OnBegin(Level level)
    {
      this.bird = this.Scene.Entities.FindFirst<BirdNPC>();
      this.player.StateMachine.State = Player.StDummy;
      if (level.Wipe != null)
        level.Wipe.Cancel();
      level.Wipe = (ScreenWipe) new FadeWipe((Scene) level, true);
      this.Add((Component) new Coroutine(this.cutscene(level)));
    }

    private IEnumerator cutscene(Level level)
    {
      Cs20BirdGuidanceIntro cs20BirdIntro = this;
      cs20BirdIntro.player.StateMachine.State = Player.StDummy;
      cs20BirdIntro.player.Dashes = 2;
      
      // Brief fade in
      cs20BirdIntro.Add((Component) new Coroutine(cs20BirdIntro.fadeIn(2f)));
      yield return (object) 0.5f;
      
      // Start the dialog with bird fly trigger
      yield return (object) Textbox.Say("DZ_CH20_BIRD_GUIDANCE_INTRO", new Func<IEnumerator>(cs20BirdIntro.birdFliesOffscreen));
      
      cs20BirdIntro.EndCutscene(level);
    }

    private IEnumerator birdFliesOffscreen()
    {
      Cs20BirdGuidanceIntro cs20BirdIntro = this;
      yield return (object) 0.3f;
      
      if (cs20BirdIntro.bird != null)
      {
        yield return (object) cs20BirdIntro.bird.StartleAndFlyAway();
        cs20BirdIntro.Level.Session.DoNotLoad.Add(cs20BirdIntro.bird.EntityID);
        cs20BirdIntro.bird = (BirdNPC) null;
      }
      
      yield return (object) 0.5f;
    }

    private IEnumerator fadeIn(float duration)
    {
      while ((double) this.fade > 0.0)
      {
        this.fade = Calc.Approach(this.fade, 0.0f, Engine.DeltaTime / duration);
        yield return (object) null;
      }
    }

    public override void OnEnd(Level level)
    {
      this.player.Depth = 0;
      this.player.Speed = Vector2.Zero;
      this.player.Active = true;
      this.player.Visible = true;
      this.player.StateMachine.State = Player.StNormal;
      
      if (this.bird != null)
      {
        this.bird.RemoveSelf();
        level.Session.DoNotLoad.Add(this.bird.EntityID);
      }
      
      level.Session.SetFlag("bird_guidance_intro");
    }

    public override void Render()
    {
      Camera camera = (this.Scene as Level).Camera;
      Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, Color.Black * this.fade);
    }
  }
}
