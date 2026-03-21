namespace MaggyHelper.Cutscenes
{
  public class Cs16WelcomeHome : CutsceneEntity
  {
    private readonly global::Celeste.Player player;
    private readonly float targetX;

    public Cs16WelcomeHome(global::Celeste.Player player, float targetX)
    {
      this.player = player;
      this.targetX = targetX;
    }

    public override void OnBegin(Level level)
    {
      this.Add((Component) new Coroutine(this.cutscene(level)));
    }

    private IEnumerator cutscene(Level level)
    {
      this.player.StateMachine.State = 11;
      this.Add((Component)new Coroutine((IEnumerator)this.player.DummyWalkToExact((int)this.targetX)));
      this.Add((Component)new Coroutine(level.ZoomTo(new Vector2(this.targetX - level.Camera.X, 90f), 2f, 2f)));
      FadeWipe fadeWipe = new FadeWipe((Scene)level, false);
      fadeWipe.Duration = 2f;
      yield return fadeWipe.Wait();
      this.endCutscene(level, Int32.MaxValue);
    }

    private void endCutscene(Level level, int player) {
        // Restore player control
        this.player.StateMachine.State = player;

        // Trigger OnEnd logic
        OnEnd(level);

        // Remove the cutscene entity from the scene
        if (RemoveOnSkipped || !WasSkipped) level.Remove(this);
    }

    public override void OnEnd(Level level)
    {
      level.OnEndOfFrame += (Action)(() =>
      {
        level.Remove(entity: (Entity)this.player);
        level.UnloadLevel();
        level.Session.Level = "inside";
        Session session = level.Session;
        Level level1 = level;
        Rectangle bounds = level.Bounds;
        double left = (double)bounds.Left;
        bounds = level.Bounds;
        double top = (double)bounds.Top;
        Vector2 from = new Vector2((float)left, (float)top);
        Vector2? nullable = new Vector2?(level1.GetSpawnPoint(from));
        session.RespawnPoint = nullable;
        level.LoadLevel(global::Celeste.Player.IntroTypes.None);
        level.Add((Entity)new CS17_EndingMod());
      });
    }
  }
}




