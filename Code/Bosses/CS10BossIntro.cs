using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using Celeste.NPCs;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes;

/// <summary>
/// Chapter 10 - Boss Introduction: Wispy Woods
/// The party encounters the angry forest guardian blocking their path
/// </summary>
public class CS10BossIntro : CutsceneEntity
{
    private Player player;
    private Npc10Wispy wispyWoods;
    private Npc10Theo theoNpc;
    

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS10BossIntro(Player player)
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

        // Set boss battle music
        Audio.SetMusic("event:/pusheen/ch10/music/boss_wispy_woods");

        // Fade in
        FadeWipe fadeWipe = new FadeWipe(level, wipeIn: true);
        fadeWipe.Duration = 1.5f;
        ScreenWipe.WipeColor = Color.Black;
        yield return fadeWipe.Duration;
        yield return 0.5f;

        // Dialog: Party notices the forest
        yield return Textbox.Say("MAGGYHELPER_CH10_BOSS_INTRO", OnTreeFaceEmerge, OnTheoReaction);

        yield return 0.8f;

        // Wispy Woods emerges from the forest
        yield return WispyWoodsAppears(level);

        yield return 1f;

        EndCutscene(level);
        yield break;
    }

    /// <summary>
    /// Trigger: Tree face emerges (trigger 0)
    /// </summary>
    private IEnumerator OnTreeFaceEmerge()
    {
        // Spawn Wispy Woods as a large tree entity
        if (wispyWoods == null)
        {
            wispyWoods = new Npc10Wispy(new EntityData(), player.Position + new Vector2(0f, -100f));
            wispyWoods.IdleAnim = "emerge";
            wispyWoods.Add(wispyWoods.Sprite = GFX.SpriteBank.Create("wispy_woods"));
            Scene.Add(wispyWoods);

            // Play emergence sound
            Audio.Play("event:/new_content/boss/wispy_woods/emerge");
        }

        // Camera zooms to show the massive tree
        Add(new Coroutine(Level.ZoomTo(player.Center + new Vector2(0f, -60f), 1.5f, 1.2f)));
        
        // Screen shake on emergence
        yield return ScreenShakeEffect(0.5f, 0.8f);
        
        yield return 1f;
        yield return null;
    }

    /// <summary>
    /// Trigger: Theo's worried reaction
    /// </summary>
    private IEnumerator OnTheoReaction()
    {
        if (theoNpc == null)
        {
            theoNpc = new Npc10Theo(new EntityData(), player.Position + new Vector2(40f, 0f));
            theoNpc.IdleAnim = "worried";
            theoNpc.Add(theoNpc.Sprite = GFX.SpriteBank.Create("theo"));
            Scene.Add(theoNpc);
        }

        // Theo steps back in shock
        yield return theoNpc.MoveTo(new Vector2(player.X + 60f, theoNpc.Position.Y));
        yield return 0.3f;
        yield return null;
    }

    /// <summary>
    /// Wispy Woods makes its threatening appearance
    /// </summary>
    private IEnumerator WispyWoodsAppears(Level level)
    {
        if (wispyWoods == null) yield break;

        // Wispy drops down menacingly
        wispyWoods.Sprite.Play("angry", false);
        Audio.Play("event:/new_content/boss/wispy_woods/roar");

        // Heavy screen shake
        yield return ScreenShakeEffect(0.3f, 1.2f);

        // Wispy speaks its threatening words via dialog
        yield return Textbox.Say("MAGGYHELPER_CH10_BOSS_INTRO_WISPY", OnWispyThreats);

        yield return null;
    }

    /// <summary>
    /// Trigger: Wispy makes angry threats
    /// </summary>
    private IEnumerator OnWispyThreats()
    {
        if (wispyWoods != null)
        {
            wispyWoods.Sprite.Play("attack_wind", false);
            
            // Wind effect VFX
            for (int i = 0; i < 5; i++)
            {
                Audio.Play("event:/new_content/boss/wispy_woods/wind");
                // ParticleTypes.Confetti doesn't exist; using Dust as alternative
                Level.ParticlesFG.Emit(ParticleTypes.Dust, 8, wispyWoods.Center, Vector2.One * 40f);
                //Level.ParticlesFG.Emit(ParticleTypes.Confetti, 8, wispyWoods.Center, Vector2.One * 40f);
                yield return 0.2f;
            }

            wispyWoods.Sprite.Play("angry", false);
        }

        yield return null;
    }

    /// <summary>
    /// Screen shake effect helper
    /// </summary>
    private IEnumerator ScreenShakeEffect(float duration, float intensity)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Level.Shake(intensity);
            yield return null;
            elapsed += Engine.DeltaTime;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        level.TimerStopped = false;
        level.TimerHidden = false;
        level.SaveQuitDisabled = false;
    }
}