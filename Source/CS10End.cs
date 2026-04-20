using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using Celeste.NPCs;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes;

/// <summary>
/// Chapter 10 - Ending: Farewell and Departure
/// The party says goodbye to Wispy Woods and prepares to continue their journey
/// </summary>
public class CS10End : CutsceneEntity
{
    private Player player;
    private Npc10Wispy wispyWoods;
    private Npc10Madeline madelineNpc;
    

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS10End(Player player)
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
        player.StateMachine.State = 11;
        player.Sprite.Play("idle");

        // Find Wispy Woods entity
        wispyWoods = Scene.Tracker.GetEntity<Npc10Wispy>();
        if (wispyWoods == null)
        {
            wispyWoods = new Npc10Wispy(new EntityData(), player.Position + new Vector2(0f, -80f));
            wispyWoods.Add(wispyWoods.Sprite = GFX.SpriteBank.Create("wispy_woods"));
            Scene.Add(wispyWoods);
        }

        // Find or spawn Madeline NPC
        madelineNpc = Scene.Tracker.GetEntity<Npc10Madeline>();
        if (madelineNpc == null)
        {
            madelineNpc = new Npc10Madeline(new EntityData(), player.Position + new Vector2(48f, 0f));
            madelineNpc.Add(madelineNpc.Sprite = GFX.SpriteBank.Create("madeline"));
            Scene.Add(madelineNpc);
        }

        yield return 0.5f;

        // Set music for ending
        Audio.SetMusic("event:/desolozantas/ch10/music/chapter_end");

        // Final dialog: Madeline thanks Wispy
        yield return Textbox.Say("CH10_END", OnMadelineGratitude, OnWispyFarewell);

        yield return 1f;

        // Wispy returns to sleep
        yield return WispyGoesToSleep();

        yield return 0.5f;

        // Fade to next scene
        FadeWipe fadeWipe = new FadeWipe(level, wipeIn: false);
        fadeWipe.Duration = 2f;
        ScreenWipe.WipeColor = Color.Black;
        yield return fadeWipe.Duration;

        EndCutscene(level);
        yield break;
    }

    /// <summary>
    /// Trigger: Madeline expresses gratitude
    /// </summary>
    private IEnumerator OnMadelineGratitude()
    {
        if (madelineNpc != null)
        {
            // Madeline bows respectfully
            madelineNpc.Sprite.Play("thanks", false);
            Audio.Play("event:/desolozantas/char/kirby/screenentry");
            yield return 0.8f;
            madelineNpc.Sprite.Play("idle", false);
        }

        yield return null;
    }

    /// <summary>
    /// Trigger: Wispy accepts the thanks and says farewell
    /// </summary>
    private IEnumerator OnWispyFarewell()
    {
        if (wispyWoods != null)
        {
            // Wispy's peaceful state
            wispyWoods.Sprite.Play("peaceful", false);
            Audio.Play("event:/new_content/boss/wispy_woods/yawn");
            yield return 0.5f;
        }

        yield return null;
    }

    /// <summary>
    /// Wispy Woods returns to sleep
    /// </summary>
    private IEnumerator WispyGoesToSleep()
    {
        if (wispyWoods != null)
        {
            // Wispy closes its eyes and yawns
            wispyWoods.Sprite.Play("sleeping", false);
            
            // Gentle fade out for the boss
            float sleepTime = 0f;
            while (sleepTime < 1.5f)
            {
                wispyWoods.Sprite.Color = Color.White * (1f - (sleepTime / 1.5f));
                yield return null;
                sleepTime += Engine.DeltaTime;
            }

            wispyWoods.Visible = false;
        }

        // Camera pulls back for final view
        Add(new Coroutine(Level.ZoomBack(1.5f)));
        yield return 1f;

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