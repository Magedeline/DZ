namespace Celeste.Cutscenes;

[HotReloadable]
public class Cs20Intro : CutsceneEntity
{
    public const string FLAG = "ch20_intro_trigger";
    private global::Celeste.Player player;
    private float fade = 1f;

    public Cs20Intro(global::Celeste.Player player)
        : base()
    {
        Depth = -8500;
        this.player = player ?? throw new ArgumentNullException(nameof(player));
    }

    public override void OnBegin(Level level)
    {
        if (player?.StateMachine == null)
        {
            RemoveSelf();
            return;
        }

        player.StateMachine.State = Player.StDummy;
        if (level.Wipe != null)
            level.Wipe.Cancel();
        level.Wipe = (ScreenWipe) new FadeWipe((Scene) level, true);
        Add((Component) new Coroutine(cutscene(level)));
    }

    private IEnumerator cutscene(Level level)
    {
        player.StateMachine.State = Player.StDummy;
        Add((Component) new Coroutine(fadeIn(2f)));

        for (float t = 0.0f; t < 1f; t += Engine.DeltaTime / 0.9f)
        {
            level.Wipe.Percent = 0.0f;
            yield return null;
        }

        yield return 0.5f;
        yield return Textbox.Say("DZ_CH20_INTRO");
        yield return 0.5f;

        EndCutscene(level);
    }

    private IEnumerator fadeIn(float duration)
    {
        while (fade > 0.0f)
        {
            fade = Calc.Approach(fade, 0.0f, Engine.DeltaTime / duration);
            yield return null;
        }
    }

    public override void OnEnd(Level level)
    {
        if (player?.StateMachine == null)
            return;

        player.Depth = 0;
        player.Speed = Vector2.Zero;
        player.Position = level.GetSpawnPoint(player.Position) + new Vector2(0.0f, -32f);
        player.Active = true;
        player.Visible = true;
        player.StateMachine.State = Player.StNormal;
        player.X = (int) player.X;
        player.Y = (int) player.Y;
        while (!player.OnGround() && player.Bottom < level.Bounds.Bottom)
            player.MoveVExact(16);

        level.Session.SetFlag(FLAG);
    }

    public override void Render()
    {
        Camera camera = (Scene as Level).Camera;
        Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, Color.Black * fade);
    }
}
