using Celeste.Entities;

namespace Celeste.Cutscenes
{
  [HotReloadable]
  public class Cs05SeeMaddy(global::Celeste.Player player, int index) : CutsceneEntity
  {
    private const float new_darkness_alpha = 0.3f;
    public const string FLAG = "seeMaddyInCrystal";
    private Entity crystal;

    public override void OnBegin(Level level)
    {
      this.Add(new Coroutine(this.Cutscene(level)));
    }

    private IEnumerator Cutscene(Level level)
    {
      while (player.Scene == null || !player.OnGround())
        yield return null;

      player.StateMachine.State = 11;
      player.StateMachine.Locked = true;
      yield return 0.25f;

      this.crystal = this.Scene.Tracker.GetEntity<MaddyCrystal>() as Entity
        ?? this.Scene.Tracker.GetEntity<TheoCrystal>();
      if (this.crystal != null && Math.Sign(player.X - this.crystal.X) != 0)
        player.Facing = (global::Celeste.Facings)Math.Sign(this.crystal.X - player.X);

      yield return 0.25f;

      if (index == 0)
      {
        yield return Textbox.Say("ch7_see_maddy",
          this.ZoomIn,
          this.MadelineTurnsAround,
          this.WaitABit,
          this.Brighten,
          this.MadelineTurnsBackAndBrighten);
      }
      else if (index == 1)
      {
        yield return Textbox.Say("ch7_see_maddy_b");
      }

      yield return this.Level.ZoomBack(0.5f);
      this.EndCutscene(level);
    }

    private IEnumerator ZoomIn()
    {
      Vector2 zoomTarget = crystal != null
        ? Vector2.Lerp(player.Position, this.crystal.Position, 0.5f)
        : player.Position;
      yield return this.Level.ZoomTo(zoomTarget - this.Level.Camera.Position + new Vector2(0.0f, -20f), 2f, 0.5f);
    }

    private IEnumerator MadelineTurnsAround()
    {
      yield return 0.3f;
      player.Facing = Facings.Left;
      yield return 0.1f;
    }

    private IEnumerator WaitABit()
    {
      yield return 1f;
    }

    private IEnumerator MadelineTurnsBackAndBrighten()
    {
      yield return 0.1f;
      Coroutine brightenCoroutine = new(this.Brighten());
      this.Add(brightenCoroutine);
      yield return 0.2f;
      player.Facing = Facings.Right;
      yield return 0.1f;

      while (brightenCoroutine.Active)
        yield return null;
    }

    private IEnumerator Brighten()
    {
      yield return this.Level.ZoomBack(0.5f);
      yield return 0.3f;
      this.Level.Session.DarkRoomAlpha = new_darkness_alpha;
      float targetAlpha = this.Level.Session.DarkRoomAlpha;
      while (!WithinEpsilon(this.Level.Lighting.Alpha, targetAlpha, 0.01f))
      {
        this.Level.Lighting.Alpha = Calc.Approach(this.Level.Lighting.Alpha, targetAlpha, Engine.DeltaTime * 0.5f);
        yield return null;
      }
    }
    private static bool WithinEpsilon(float value1, float value2, float epsilon)
    {
      return Math.Abs(value1 - value2) <= epsilon;
    }

    public override void OnEnd(Level level)
    {
      player.StateMachine.Locked = false;
      player.StateMachine.State = 0;
      player.ForceCameraUpdate = false;
      player.DummyAutoAnimate = true;
      level.Session.DarkRoomAlpha = new_darkness_alpha;
      level.Lighting.Alpha = level.Session.DarkRoomAlpha;
      level.Session.SetFlag("seeMaddyInCrystal");
    }
  }
}
