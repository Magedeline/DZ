using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using Component = DZ.Nez.Component;
using System;
using System.Collections.Generic;
using DZ.Core;
using DZ.Entities.Core;

namespace DZ.Entities.Level;

// ═══════════════════════════════════════════════════════════════════════════════
// CassetteBlock
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Port of Celeste's <c>CassetteBlock.cs</c>.
///
/// A solid platform that rhythmically activates and deactivates in sync with
/// <see cref="CassetteBlockManager"/>. Only blocks whose <see cref="Index"/>
/// matches the manager's current <see cref="CassetteBlockManager.BeatIndex"/>
/// are solid at any given moment; all others are passthrough.
///
/// <list type="bullet">
///   <item><b>Index 0</b> – Blue channel</item>
///   <item><b>Index 1</b> – Pink channel</item>
///   <item><b>Index 2</b> – Yellow channel</item>
///   <item><b>Index 3</b> – Green channel</item>
/// </list>
///
/// Call <see cref="Activate"/> / <see cref="Deactivate"/> from the manager to
/// toggle the block's <see cref="Activated"/> state and <see cref="CelestePlatform.Collidable"/>
/// flag. An optional brief scale-wiggle "pop" animation is tracked via <see cref="_wiggleTimer"/>.
/// </summary>
public class CassetteBlock : CelesteSolid
{
    // ── Per-index colours (matches Celeste originals) ─────────────────────────

    private static readonly Color[] IndexColors =
    {
        new Color(73,  170, 240), // 0 – Blue
        new Color(240, 73,  190), // 1 – Pink
        new Color(252, 220, 58),  // 2 – Yellow
        new Color(56,  224, 78),  // 3 – Green
    };

    // ── Configuration ─────────────────────────────────────────────────────────

    /// <summary>
    /// Beat channel this block responds to (0–3).
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Tempo multiplier passed in from the entity data (kept for potential future use;
    /// the manager uses its own <see cref="CassetteBlockManager.BeatsPerMinute"/>).
    /// </summary>
    public float Tempo { get; }

    /// <summary>
    /// Colour associated with this block's <see cref="Index"/> channel.
    /// </summary>
    public Color BlockColor { get; }

    // ── State ────────────────────────────────────────────────────────────────

    /// <summary>
    /// <c>true</c> when this block is currently solid and the player can stand on it;
    /// <c>false</c> when it is in the "off" phase and the player passes through.
    /// </summary>
    public bool Activated { get; private set; }

    // ── Animation ────────────────────────────────────────────────────────────

    /// <summary>Remaining time of the pop-in/pop-out wiggle animation (seconds).</summary>
    private float _wiggleTimer;

    /// <summary>Duration of the activate/deactivate wiggle animation.</summary>
    private const float WiggleDuration = 0.16f;

    // ── Constructor ──────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="CassetteBlock"/>.
    /// </summary>
    /// <param name="position">Top-left world position in pixels.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="index">Beat channel index (0–3).</param>
    /// <param name="tempo">Tempo multiplier (default 1.0, informational only).</param>
    public CassetteBlock(
        Vector2 position,
        float   width,
        float   height,
        int     index,
        float   tempo = 1f)
        : base(position, width, height)
    {
        Index      = Math.Clamp(index, 0, 3);
        Tempo      = tempo;
        BlockColor = IndexColors[Index];

        // Blocks start deactivated; the manager will activate the correct channel.
        Activated  = false;
        Collidable = false;

        Name = $"CassetteBlock_{Index}";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        // TODO: load sprite – add SpriteRenderer tinted with BlockColor
    }

    /// <inheritdoc/>
    public override void Update()
    {
        base.Update();

        // Decay the wiggle animation timer.
        if (_wiggleTimer > 0f)
            _wiggleTimer -= Time.DeltaTime;
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Activates this block, making it solid and starting the pop-in animation.
    /// Called by <see cref="CassetteBlockManager"/> when this block's channel becomes active.
    /// </summary>
    public void Activate()
    {
        if (Activated) return;

        Activated  = true;
        Collidable = true;

        // Start pop-in wiggle.
        _wiggleTimer = WiggleDuration;

        // TODO: play sound: event:/game/09_core/cassette_block_activate
        // TODO: emit particles – sparkle/pulse effect at block surface
    }

    /// <summary>
    /// Deactivates this block, making it passthrough and starting the pop-out animation.
    /// Called by <see cref="CassetteBlockManager"/> when this block's channel goes inactive.
    /// </summary>
    public void Deactivate()
    {
        if (!Activated) return;

        Activated  = false;
        Collidable = false;

        // Start pop-out wiggle.
        _wiggleTimer = WiggleDuration;

        // TODO: play sound: event:/game/09_core/cassette_block_deactivate
    }

    /// <summary>
    /// Normalised wiggle scale [0, 1] that goes to 0 over <see cref="WiggleDuration"/> seconds.
    /// Bind to a renderer's <c>Scale</c> for the pop animation, e.g.:
    /// <c>1f + 0.1f * block.WiggleScale</c>
    /// </summary>
    public float WiggleScale =>
        WiggleDuration > 0f ? MathHelper.Clamp(_wiggleTimer / WiggleDuration, 0f, 1f) : 0f;
}

// ═══════════════════════════════════════════════════════════════════════════════
// CassetteBlockManager
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Port of Celeste's <c>CassetteBlockManager.cs</c>, merged into a single scene component.
///
/// Drives all <see cref="CassetteBlock"/> entities in the scene by advancing a beat counter
/// at a configurable BPM and toggling the active channel (0–3) each beat.
///
/// <para>
/// Attach this as a component to any persistent scene entity or a dedicated manager entity.
/// It will automatically gather all <see cref="CassetteBlock"/> instances from the scene
/// each beat.
/// </para>
///
/// <para>
/// <see cref="OnBeat"/> is fired every time the beat index advances, passing the new index.
/// Subscribe to drive music/visual synchronisation effects.
/// </para>
///
/// Beat timing details (matching Celeste originals at default BPM):
/// <list type="bullet">
///   <item>Default BPM: 102</item>
///   <item>BPS = BPM / 60 = 1.7 beats per second</item>
///   <item>Each beat flips the active channel: 0 → 1 → 2 → 3 → 0 …</item>
/// </list>
/// </summary>
public class CassetteBlockManager : Component, IUpdatable
{
    // ── Events ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Fired whenever the beat index advances.
    /// The argument is the new <see cref="BeatIndex"/> (0–3).
    /// </summary>
    public static event Action<int> OnBeat;

    // ── Beat configuration ────────────────────────────────────────────────────

    /// <summary>
    /// Current active beat channel (0–3). Blocks with this <c>Index</c> are solid.
    /// </summary>
    public int BeatIndex { get; private set; }

    /// <summary>
    /// Accumulated time within the current beat interval (seconds).
    /// </summary>
    public float BeatTimer { get; private set; }

    /// <summary>
    /// Tempo in beats per minute. Change via <see cref="SetBPM"/> at runtime.
    /// </summary>
    public float BeatsPerMinute { get; private set; } = 102f;

    /// <summary>
    /// Derived: beats per second (<c>BPM / 60</c>).
    /// </summary>
    public float BeatsPerSecond => BeatsPerMinute / 60f;

    /// <summary>
    /// Seconds per beat interval (<c>1 / BeatsPerSecond</c>).
    /// </summary>
    public float SecondsPerBeat => 1f / BeatsPerSecond;

    /// <summary>
    /// Sixteenth-notes per beat used in the cassette timing model.
    /// Kept as a named constant matching the Celeste original.
    /// </summary>
    public const int SixteenthsPerBeat = 6;

    // ── Internal state ────────────────────────────────────────────────────────

    private const int ChannelCount = 4;

    // ── Constructor ──────────────────────────────────────────────────────────

    /// <summary>Creates a <see cref="CassetteBlockManager"/> with default BPM (102).</summary>
    public CassetteBlockManager() { }

    /// <summary>Creates a <see cref="CassetteBlockManager"/> with a custom BPM.</summary>
    /// <param name="beatsPerMinute">Initial tempo in beats per minute.</param>
    public CassetteBlockManager(float beatsPerMinute)
    {
        BeatsPerMinute = beatsPerMinute;
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        BeatIndex = 0;
        BeatTimer = 0f;

        // Activate channel 0 immediately so the first frame is correct.
        ActivateChannel(0, initialise: true);
    }

    // ── IUpdatable ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void Update()
    {
        BeatTimer += Time.DeltaTime;

        if (BeatTimer < SecondsPerBeat) return;

        // Advance to next beat.
        BeatTimer -= SecondsPerBeat;
        int nextBeat = (BeatIndex + 1) % ChannelCount;

        ActivateChannel(nextBeat, initialise: false);
        BeatIndex = nextBeat;

        OnBeat?.Invoke(BeatIndex);

        // TODO: play sound: event:/game/09_core/cassette_block_pulse (on each beat)
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Changes the BPM at runtime (e.g. for a speed-up effect during a boss fight).
    /// </summary>
    /// <param name="bpm">New tempo in beats per minute.</param>
    public void SetBPM(float bpm)
    {
        BeatsPerMinute = bpm;
        // Don't reset BeatTimer so the rhythm stays consistent during a live change.
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Iterates all <see cref="CassetteBlock"/> entities in the scene and
    /// activates those matching <paramref name="channelIndex"/>, deactivating all others.
    /// </summary>
    private void ActivateChannel(int channelIndex, bool initialise)
    {
        if (Entity?.Scene == null) return;

        // Gather all CassetteBlock entities from the scene.
        // Using FindEntitiesOfType which searches by component presence is the Nez pattern,
        // but CassetteBlock is an Entity subclass so we iterate scene entities directly.
        var allBlocks = Entity.Scene.EntitiesOfType<CassetteBlock>();
        if (allBlocks == null) return;

        foreach (var block in allBlocks)
        {
            if (block.Index == channelIndex)
            {
                if (!block.Activated || initialise)
                    block.Activate();
            }
            else
            {
                if (block.Activated || initialise)
                    block.Deactivate();
            }
        }
    }
}
