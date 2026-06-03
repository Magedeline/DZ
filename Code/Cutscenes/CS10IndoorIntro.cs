using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using Celeste.NPCs;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes;

/// <summary>
/// Chapter 10 - Indoor Introduction at Dreemurr House
/// Handles the exploration sequence inside Toriel's home with dialog about photos and memories
/// </summary>
public class CS10IndoorIntro : CutsceneEntity
{
    private Player player;
    private Npc10Badeline badelineNpc;
    private Npc10Chara charaNpc;
    private Npc10Toriel torielNpc;
    
    private bool hasExaminedButterscotchPie = false;
    private bool hasExaminedAsgoreTrophy = false;
    private bool hasExaminedAsrielPlushie = false;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS10IndoorIntro(Player player)
        : base(fadeInOnSkip: false)
    {
        this.player = player;
        base.Depth = -1000000;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        Level obj = scene as Level;
        obj.TimerStopped = true;
        obj.TimerHidden = true;
        obj.SaveQuitDisabled = true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        // Setup player state
        player.Dashes = 1;
        player.StateMachine.State = Player.StDummy;
        player.Sprite.Play("idle");

        // Set music for indoor scene
        Audio.SetMusic("event:/music/pusheen/lvl10/home");

        // Fade in from previous cutscene
        FadeWipe fadeWipe = new FadeWipe(level, wipeIn: true);
        fadeWipe.Duration = 1f;
        ScreenWipe.WipeColor = Color.White;
        yield return fadeWipe.Duration;
        yield return 0.5f;

        // Spawn Badeline and Chara NPCs inside the house
        yield return SpawnInsideCharacters();

        // Initial dialog about the house and photos
        yield return Textbox.Say("MAGGYHELPER_CH10_INDOOR_INTRO", ExamineButterscotchPie, ExamineAsgore, ExamineAsriel);

        yield return 0.5f;

        // Examine items in the house (sub-dialogs)
        if (!hasExaminedButterscotchPie)
        {
            yield return Textbox.Say("MAGGYHELPER_CH10_INDOOR_A", OnExamineButterscotchPie);
        }

        yield return 0.3f;

        if (!hasExaminedAsgoreTrophy)
        {
            yield return Textbox.Say("MAGGYHELPER_CH10_INDOOR_B", OnExamineAsgoreT);
        }

        yield return 0.3f;

        if (!hasExaminedAsrielPlushie)
        {
            yield return Textbox.Say("MAGGYHELPER_CH10_INDOOR_C", OnExamineAsrielPlushie);
        }

        yield return 0.8f;

        // Pre-meeting setup
        yield return Textbox.Say("MAGGYHELPER_CH10_INDOOR_PREMEETING", PrepareForMeeting);

        yield return 0.5f;

        // Main meeting with Toriel
        yield return Textbox.Say("MAGGYHELPER_CH10_INDOOR_MEETING", MeetToriel, OnMeetingComplete);

        yield return 1f;

        EndCutscene(level);
        yield break;
    }

    /// <summary>
    /// Spawn Badeline and Chara inside the house
    /// </summary>
    private IEnumerator SpawnInsideCharacters()
    {
        // Pan camera to show interior
        Add(new Coroutine(Level.ZoomTo(player.Center + new Vector2(40f, -20f), 1f, 1.5f)));
        yield return 0.5f;

        // Spawn Badeline near player
        if (badelineNpc == null)
        {
            badelineNpc = new Npc10Badeline(new EntityData(), player.Position + new Vector2(32f, 0f));
            badelineNpc.IdleAnim = "idle";
            badelineNpc.Add(badelineNpc.Sprite = GFX.SpriteBank.Create("maggy_badeline"));
            Scene.Add(badelineNpc);
        }

        yield return 0.3f;

        // Spawn Chara as a ghost-like figure
        if (charaNpc == null)
        {
            charaNpc = new Npc10Chara(new EntityData(), player.Position + new Vector2(-32f, -20f));
            charaNpc.IdleAnim = "idle";
            charaNpc.Add(charaNpc.Sprite = GFX.SpriteBank.Create("maggy_chara"));
            charaNpc.Sprite.Color = Color.White * 0.7f; // Semi-transparent for ghost effect
            Scene.Add(charaNpc);
        }

        yield return 0.5f;
        yield return null;
    }

    /// <summary>
    /// Examine the butterscotch pie on the kitchen desk
    /// </summary>
    private IEnumerator ExamineButterscotchPie()
    {
        // Camera focuses on kitchen area
        Add(new Coroutine(Level.ZoomTo(player.Center + new Vector2(60f, 0f), 1.2f, 1f)));
        hasExaminedButterscotchPie = true;
        yield return 0.5f;
        yield return null;
    }

    /// <summary>
    /// Examine the Asgore trophy
    /// </summary>
    private IEnumerator ExamineAsgore()
    {
        // Camera focuses on trophy on shelf
        Add(new Coroutine(Level.ZoomTo(player.Center + new Vector2(0f, -60f), 1.2f, 1f)));
        hasExaminedAsgoreTrophy = true;
        yield return 0.5f;
        yield return null;
    }

    /// <summary>
    /// Examine the Asriel plushie on the bed
    /// </summary>
    private IEnumerator ExamineAsriel()
    {
        // Camera focuses on dusty bed area
        Add(new Coroutine(Level.ZoomTo(player.Center + new Vector2(-40f, -50f), 1.2f, 1f)));
        hasExaminedAsrielPlushie = true;
        yield return 0.5f;
        yield return null;
    }

    /// <summary>
    /// Trigger for butterscotch pie examination dialog
    /// </summary>
    private IEnumerator OnExamineButterscotchPie()
    {
        // Brief pause while looking at the pie
        yield return 1f;
        yield return null;
    }

    /// <summary>
    /// Trigger for Asgore trophy examination dialog
    /// </summary>
    private IEnumerator OnExamineAsgoreT()
    {
        // Charming laugh or reaction
        if (player != null)
        {
            player.Sprite.Play("laugh", false);
            Audio.Play("event:/char/pusheen/kirby/laugh");
        }
        yield return 1.2f;
        if (player != null)
        {
            player.Sprite.Play("idle", false);
        }
        yield return null;
    }

    /// <summary>
    /// Trigger for Asriel plushie examination dialog
    /// </summary>
    private IEnumerator OnExamineAsrielPlushie()
    {
        // Emotional moment looking at the plushie
        yield return 0.8f;
        
        // Subtle tremor or emotion in player
        if (player != null && Level != null)
        {
            Level.Shake(0.15f);
        }
        
        yield return 0.5f;
        yield return null;
    }

    /// <summary>
    /// Prepare for meeting with Toriel
    /// </summary>
    private IEnumerator PrepareForMeeting()
    {
        // Reset camera to normal view
        Add(new Coroutine(Level.ZoomBack(1f)));
        
        // Group characters near the living area
        if (badelineNpc != null)
        {
            yield return badelineNpc.MoveTo(new Vector2(player.X - 24f, badelineNpc.Position.Y));
        }

        if (charaNpc != null)
        {
            yield return charaNpc.MoveTo(new Vector2(player.X + 24f, charaNpc.Position.Y));
        }

        yield return null;
    }

    /// <summary>
    /// Spawn Toriel for the meeting
    /// </summary>
    private IEnumerator MeetToriel()
    {
        if (torielNpc == null)
        {
            torielNpc = new Npc10Toriel(new EntityData(), player.Position + new Vector2(80f, 0f));
            torielNpc.IdleAnim = "idle";
            torielNpc.MoveAnim = "walk";
            torielNpc.Maxspeed = 25f;
            torielNpc.Add(torielNpc.Sprite = GFX.SpriteBank.Create("toriel"));
            Scene.Add(torielNpc);
        }

        // Toriel walks toward the group
        yield return torielNpc.MoveTo(new Vector2(player.X + 40f, torielNpc.Position.Y));
        yield return 0.5f;
        yield return null;
    }

    /// <summary>
    /// Callback when meeting with Toriel completes
    /// </summary>
    private IEnumerator OnMeetingComplete()
    {
        // Brief pause after dialog
        yield return 0.5f;
        yield return null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        level.TimerStopped = false;
        level.TimerHidden = false;
        level.SaveQuitDisabled = false;
    }
}