using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using Celeste.NPCs;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes;

/// <summary>
/// Chapter 10 - Boss Outro: Wispy Woods Defeat
/// After defeating Wispy Woods, the forest guardian accepts its loss and offers a reward
/// </summary>
public class CS10BossOutro : CutsceneEntity
{
    private Player player;
    private Npc10Wispy wispyWoods;
    
#pragma warning disable CS0414
    private float victoryAlpha = 0f;
#pragma warning restore CS0414

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS10BossOutro(Player player)
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
        
        // Change music to victory theme
        Audio.SetMusic("event:/pusheen/ch10/music/boss_wispy_victory");
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

        // Find or spawn Wispy Woods (should exist from battle)
        wispyWoods = Scene.Tracker.GetEntity<Npc10Wispy>();
        if (wispyWoods == null)
        {
            wispyWoods = new Npc10Wispy(new EntityData(), player.Position + new Vector2(0f, -80f));
            wispyWoods.Add(wispyWoods.Sprite = GFX.SpriteBank.Create("wispy_woods"));
            Scene.Add(wispyWoods);
        }

        yield return 0.5f;

        // Wispy recovers from damage
        yield return WispyRecovers();

        yield return 0.8f;

        // Victory dialog with Wispy acknowledging defeat
        yield return Textbox.Say("MAGGYHELPER_CH10_BOSS_OUTRO", OnWispyApologizes);

        yield return 0.5f;

        // Wispy grants the reward
        yield return GrantHeartGem(level);

        yield return 1f;

        EndCutscene(level);
        yield break;
    }

    /// <summary>
    /// Wispy Woods recovers and accepts defeat
    /// </summary>
    private IEnumerator WispyRecovers()
    {
        if (wispyWoods != null)
        {
            // Play recovery animation
            wispyWoods.Sprite.Play("recovering", false);
            Audio.Play("event:/new_content/boss/wispy_woods/pain");
            
            // Branches shake and settle
            yield return ScreenShakeEffect(0.3f, 0.5f);
            
            yield return 1f;
            
            // Transition to normal state
            wispyWoods.Sprite.Play("normal", false);
        }

        yield return null;
    }

    /// <summary>
    /// Trigger: Wispy apologizes and acknowledges the party
    /// </summary>
    private IEnumerator OnWispyApologizes()
    {
        if (wispyWoods != null)
        {
            // Wispy's expression softens
            wispyWoods.Sprite.Play("sad", false);
            
            // Brief pause for reflection
            yield return 1.5f;
            
            wispyWoods.Sprite.Play("normal", false);
        }

        yield return null;
    }

    /// <summary>
    /// Grant the party a Heart Gem reward
    /// </summary>
    private IEnumerator GrantHeartGem(Level level)
    {
        // Create a glowing heart gem
        var heartGem = new Entity()
        {
            Position = wispyWoods.Center + new Vector2(0f, -40f),
            Depth = -100
        };

        // Add glow particle effect
        float glowTime = 0f;
        while (glowTime < 2f)
        {
            // ParticleTypes.Confetti doesn't exist; using Dust as alternative
            Level.ParticlesFG.Emit(ParticleTypes.Dust, 2, heartGem.Position, Vector2.One * 8f);
            Audio.Play("event:/ui/game/completed");
            yield return 0.3f;
            glowTime += Engine.DeltaTime;
        }

        // Heart floats to player
        Add(new Coroutine(FloatHeartToPlayer(heartGem, player)));
        yield return 1.5f;

        // Player receives the gem
        if (player != null)
        {
            player.Dashes = 2;
            level.Session.Inventory.Dashes = 2;
            Audio.Play("event:/ui/game/reflect_summit");
        }

        yield return null;
    }

    /// <summary>
    /// Animate heart floating to player
    /// </summary>
    private IEnumerator FloatHeartToPlayer(Entity heart, Player targetPlayer)
    {
        Vector2 startPos = heart.Position;
        Vector2 targetPos = targetPlayer.Center;
        float duration = 1.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Engine.DeltaTime;
            float progress = elapsed / duration;
            
            // Ease cubic out for smooth arrival
            float easeProgress = Ease.CubeOut(progress);
            heart.Position = Vector2.Lerp(startPos, targetPos, easeProgress);
            
            // Emit particles along the path
            if (elapsed % 0.1f < Engine.DeltaTime)
            {
                // ParticleTypes.Confetti doesn't exist; using Dust as alternative
                Level.ParticlesFG.Emit(ParticleTypes.Dust, 1, heart.Position, Vector2.One * 4f);
            }
            
            yield return null;
        }
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