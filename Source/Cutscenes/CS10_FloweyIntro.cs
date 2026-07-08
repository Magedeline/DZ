namespace Celeste.Cutscenes;

/// <summary>
/// Chapter 10 - Flowey Intro cutscene entry point.
/// This is a thin facade that delegates to <see cref="FloweyIntroScene"/>, which
/// contains the full sequence: Flowey emerges from the ground, threatens Madeline,
/// gets hit by Kirby's star bullet, and Kirby/Theo/Chara arrive. It also handles
/// the Normal / Returning / Assist dialog variants, camera zoom, screen shake,
/// and music. Keeping this class lets existing direct constructions and the
/// "ch10_flowey_intro" event registration continue to work.
/// </summary>
[HotReloadable]
public class Cs10FloweyIntro : CutsceneEntity
{
    public const string FLAG = "ch10_flowey_intro_trigger";

    private readonly global::Celeste.Player player;

    public Cs10FloweyIntro(global::Celeste.Player player) : base(true, false)
    {
        this.player = player ?? throw new ArgumentNullException(nameof(player));
    }

    public override void OnBegin(Level level)
    {
        // Spawn the full implementation; it owns player state, Flowey, camera, and audio.
        level.Add(new FloweyIntroScene(player));
        // End this facade immediately - FloweyIntroScene manages the rest.
        EndCutscene(level);
    }

    public override void OnEnd(Level level)
    {
        // Intentionally empty: FloweyIntroScene restores player state in its own OnEnd.
    }
}
