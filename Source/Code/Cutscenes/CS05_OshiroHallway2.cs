using System.Runtime.CompilerServices;
using Celeste.NPCs;
using NPC = Celeste.NPCs.NPC;

namespace Celeste.Cutscenes;

[method: MethodImpl(MethodImplOptions.NoInlining)]
public class CS05_OshiroHallway2(global::Celeste.Player player, NPC oshiro) : CutsceneEntity
{
    public const string Flag = "oshiro_resort_talked_3";

    private global::Celeste.Player player = player;

    private readonly NPC oshiro = oshiro;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        level.Session.Audio.Music.Layer(1, value: false);
        level.Session.Audio.Music.Layer(2, value: true);
        level.Session.Audio.Apply(forceSixteenthNoteHack: false);
        player.StateMachine.State = Player.StDummy;
        player.StateMachine.Locked = true;
        yield return Textbox.Say("MAGGYHELPER_CH5_OSHIRO_HALLWAY_B");
        oshiro.MoveToAndRemove(new Vector2(level.Bounds.Right + 64, oshiro.Y));
        oshiro.Add(new SoundSource("event:/char/oshiro/move_03_08a_exit"));
        yield return 1f;
        EndCutscene(level);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        level.Session.Audio.Music.Layer(1, value: true);
        level.Session.Audio.Music.Layer(2, value: false);
        level.Session.Audio.Apply(forceSixteenthNoteHack: false);
        player.StateMachine.Locked = false;
        player.StateMachine.State = Player.StNormal;
        level.Session.SetFlag("oshiro_resort_talked_3");
        if (WasSkipped)
        {
            level.Remove(oshiro);
        }
    }
}






