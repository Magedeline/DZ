using System.Runtime.CompilerServices;
using Celeste.NPCs;

namespace Celeste.Cutscenes;

public class CS05_OshiroClutter : CutsceneEntity
{
    private readonly int index;

    private readonly global::Celeste.Player player;

    private readonly NPC03_Oshiro_Cluttter oshiro;

    private List<ClutterDoor> doors;
    private readonly NPC05_Oshiro_Cluttter npc05;
    private readonly int sectionsComplete;

    // Helpers to dispatch to whichever NPC was provided
    private int EffectiveIndex => oshiro != null ? index : sectionsComplete;
    private float OshiroX => oshiro != null ? oshiro.X : npc05.X;
    private Vector2 OshiroPosition => oshiro != null ? oshiro.Position : npc05.Position;
    private Sprite OshiroSprite => oshiro != null ? oshiro.Sprite : npc05.Sprite;
    private Vector2 EffectiveZoomPoint => oshiro != null ? oshiro.ZoomPoint : npc05.ZoomPoint;
    private Vector2 EffectiveHomePosition => oshiro != null ? oshiro.HomePosition : npc05.HomePosition;
    private IEnumerator EffectivePaceLeft() => oshiro != null ? oshiro.PaceLeft() : npc05.PaceLeft();
    private IEnumerator EffectivePaceRight() => oshiro != null ? oshiro.PaceRight() : npc05.PaceRight();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS05_OshiroClutter(global::Celeste.Player player, NPC03_Oshiro_Cluttter oshiro, int index)
    {
        this.player = player;
        this.oshiro = oshiro;
        this.index = index;
    }

    public CS05_OshiroClutter(global::Celeste.Player player, NPC05_Oshiro_Cluttter npc05, int sectionsComplete)
    {
        this.player = player;
        this.npc05 = npc05;
        this.sectionsComplete = sectionsComplete;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        doors = base.Scene.Entities.FindAll<ClutterDoor>();
        doors.Sort((ClutterDoor a, ClutterDoor b) => (int)(a.Y - b.Y));
        Add(new Coroutine(Cutscene(level)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        var idx = EffectiveIndex;
        player.StateMachine.State = Player.StDummy;
        player.StateMachine.Locked = true;
        int num = ((idx != 1 && idx != 2) ? 1 : (-1));
        if (num == -1)
        {
            yield return player.DummyWalkToExact((int)OshiroX - 24);
            player.Facing = Facings.Right;
            OshiroSprite.Scale.X = -1f;
        }
        else
        {
            Add(new Coroutine(EffectivePaceRight()));
            yield return player.DummyWalkToExact((int)EffectiveHomePosition.X + 24);
            player.Facing = Facings.Left;
            OshiroSprite.Scale.X = 1f;
        }
        if (idx < 4)
        {
            yield return Level.ZoomTo(EffectiveZoomPoint, 2f, 0.5f);
            yield return Textbox.Say("MAGGYHELPER_CH5_OSHIRO_CLUTTER" + idx, Collapse, EffectivePaceLeft, EffectivePaceRight);
            yield return Level.ZoomBack(0.5f);
            level.Session.SetFlag("oshiro_clutter_mod_door_open");
            if (idx == 0)
            {
                SetMusic();
            }
            foreach (ClutterDoor door in doors)
            {
                if (!door.IsLocked(level.Session))
                {
                    yield return door.UnlockRoutine();
                }
            }
        }
        else
        {
            yield return CutsceneEntity.CameraTo(new Vector2(Level.Bounds.X, Level.Bounds.Y), 0.5f);
            yield return Level.ZoomTo(new Vector2(90f, 60f), 2f, 0.5f);
            yield return Textbox.Say("MAGGYHELPER_CH5_OSHIRO_CLUTTER_ENDING");
            if (oshiro != null)
            {
                yield return oshiro.MoveTo(new Vector2(oshiro.X, level.Bounds.Top - 32));
                oshiro.Add(new SoundSource("event:/char/oshiro/move_05_09b_exit"));
            }
            else
            {
                yield return npc05.MoveTo(level.Bounds.Top - 32);
                npc05.Add(new SoundSource("event:/char/oshiro/move_05_09b_exit"));
            }
            yield return Level.ZoomBack(0.5f);
        }
        EndCutscene(level);
    }

    private IEnumerator Collapse()
    {
        Audio.Play("event:/char/oshiro/chat_collapse", OshiroPosition);
        OshiroSprite.Play("fall");
        yield return 0.5f;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void SetMusic()
    {
        Level obj = base.Scene as Level;
        obj.Session.Audio.Music.Event = "event:/music/pusheen/lvl5/clean";
        obj.Session.Audio.Music.Progress = 1;
        obj.Session.Audio.Apply(forceSixteenthNoteHack: false);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        var idx = EffectiveIndex;
        player.StateMachine.Locked = false;
        player.StateMachine.State = Player.StNormal;
        if (OshiroSprite.CurrentAnimationID == "side")
        {
            (OshiroSprite as OshiroSprite).Pop("idle", flip: true);
        }
        if (idx < 4)
        {
            level.Session.SetFlag("oshiro_clutter_door_open");
            level.Session.SetFlag("oshiro_clutter_" + idx);
            if (idx == 0 && WasSkipped)
            {
                SetMusic();
            }
            foreach (ClutterDoor door in doors)
            {
                if (!door.IsLocked(level.Session))
                {
                    door.InstantUnlock();
                }
            }
            if (WasSkipped && idx == 0)
            {
                OshiroSprite.Play("idle_ground");
            }
        }
        else
        {
            level.Session.SetFlag("oshiro_clutter_finished");
            if (oshiro != null)
                base.Scene.Remove(oshiro);
            else
                base.Scene.Remove(npc05);
        }
    }
}





