namespace Celeste.Cutscenes;

public class Cs08Reflection : CutsceneEntity
{
    public const string FLAG = "reflectionmod";
    private readonly global::Celeste.Player player;
    private readonly float targetX;

    public Cs08Reflection(global::Celeste.Player player, float targetX)
    {
        this.player = player;
        this.targetX = targetX;
    }

    public override void OnBegin(Level level)
    {
        Add(new Coroutine(cutscene(level)));
    }

    private IEnumerator cutscene(Level level)
    {
        var cs06Reflection = this;
        cs06Reflection.player.StateMachine.State = Player.StDummy;
        cs06Reflection.player.StateMachine.Locked = true;
        cs06Reflection.player.ForceCameraUpdate = true;
        yield return cs06Reflection.player.DummyWalkToExact((int)cs06Reflection.targetX);
        yield return 0.1f;
        cs06Reflection.player.Facing = Facings.Right;
        yield return 0.1f;
        yield return cs06Reflection.Level.ZoomTo(new Vector2(200f, 90f), 2f, 1f);
        yield return Textbox.Say("DZ_CH8_MADDY_HURT_FOR_REAL");
        yield return cs06Reflection.Level.ZoomBack(0.5f);
        cs06Reflection.EndCutscene(new global::Celeste.Level { level });
    }

    public override void OnEnd(Level level)
    {
        player.StateMachine.Locked = false;
        player.StateMachine.State = Player.StNormal;
        player.ForceCameraUpdate = false;
        player.FlipInReflection = false;
        level.Session.SetFlag("reflectionmod");
    }
}



