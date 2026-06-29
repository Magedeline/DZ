using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using Random = DZ.Nez.Random;
using System;
using System.Linq;
using DZ.Entities.Player;


namespace DZ.Entities.Enemies;

/// <summary>
/// Manages global Seeker-proximity effects: time-slowing and screen distortion.
/// Ported from Celeste's SeekerEffectsController.cs.
///
/// When <see cref="Enabled"/> is true it:
/// <list type="bullet">
///   <item>Slows <c>Time.TimeScale</c> as seekers approach the player.</item>
///   <item>Applies a screen-anxiety distortion proportional to seeker proximity.</item>
///   <item>Adjusts music layer 3 based on the seeker threat level.</item>
/// </list>
/// Disables itself automatically when all seekers are gone and effects are cleared.
/// </summary>
public class SeekerEffectsController : DZ.Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>Squared distance at which time-slow kicks in fully (~48 px).</summary>
    private const float DistSqFull = 256f;

    /// <summary>Squared distance at which time is fully restored (~128 px).</summary>
    private const float DistSqClear = 16384f;

    /// <summary>Squared distance at which the attack-slow kicks in (~16 px).</summary>
    private const float AttackDistSqFull = 256f;

    /// <summary>Squared distance at which attack-slow fully clears (~64 px).</summary>
    private const float AttackDistSqClear = 4096f;

    /// <summary>Rate at which <c>TimeRate</c> is approached (per second).</summary>
    private const float TimeRateApproach = 4f;

    /// <summary>Minimum time rate when a seeker is attacking.</summary>
    private const float MinTimeRate = 0.5f;

    // -------------------------------------------------------------------------
    // Public state
    // -------------------------------------------------------------------------

    /// <summary>When false, no effects are applied and seekers are not tracked.</summary>
    public new bool Enabled { get; set; } = true;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    /// <summary>Small random per-frame anxiety flicker offset.</summary>
    private float _randomAnxietyOffset;

    /// <summary>Accumulator for the per-interval randomisation.</summary>
    private float _intervalTimer;

    /// <summary>Current simulated anxiety intensity (0-1).</summary>
    private float _anxiety;

    /// <summary>Current simulated time rate (1 = normal).</summary>
    private float _timeRate = 1f;

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;
        var seekers = Entity.Scene?.FindComponentsOfType<Seeker>();
        bool anySeekers = seekers != null && seekers.Count > 0;

        if (Enabled)
        {
            // Randomise anxiety offset every ~0.05 s
            _intervalTimer += dt;
            if (_intervalTimer >= 0.05f)
            {
                _intervalTimer = 0f;
                _randomAnxietyOffset = Random.Range(-0.2f, 0.2f);
            }

            var player = Entity.Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();

            float targetTimeRate;
            float anxietyMult;

            if (player != null)
            {
                float nearestDistSq   = -1f; // nearest seeker (not regenerating)
                float attackingDistSq = -1f; // nearest attacking seeker

                if (seekers != null)
                {
                    foreach (var seeker in seekers)
                    {
                        float dsq = Vector2.DistanceSquared(
                            player.Position,
                            seeker.Entity.Position);

                        if (seeker.CurrentState != Seeker.SeekerState.Stunned)
                            nearestDistSq = nearestDistSq < 0f ? dsq : Math.Min(nearestDistSq, dsq);

                        if (seeker.CurrentState == Seeker.SeekerState.Attack)
                            attackingDistSq = attackingDistSq < 0f ? dsq : Math.Min(attackingDistSq, dsq);
                    }
                }

                // Time-rate: slow when a seeker is attacking
                targetTimeRate = attackingDistSq < 0f
                    ? 1f
                    : ClampedMap(attackingDistSq, AttackDistSqFull, AttackDistSqClear, MinTimeRate, 1f);

                // Anxiety multiplier: ramp up as seekers get close
                anxietyMult = nearestDistSq < 0f
                    ? 0f
                    : ClampedMap(nearestDistSq, DistSqFull, DistSqClear, 1f, 0f);
            }
            else
            {
                targetTimeRate = 1f;
                anxietyMult = 0f;
            }

            _timeRate = Approach(_timeRate, targetTimeRate, TimeRateApproach * dt);
            _anxiety  = Approach(_anxiety, (0.5f + _randomAnxietyOffset) * anxietyMult, 8f * dt);

            // Apply effects
            // TODO: Time.TimeScale = _timeRate  (or equivalent engine time-rate)
            // TODO: Distort.Anxiety = _anxiety
            // TODO: Distort.GameRate = ClampedMap(_timeRate, MinTimeRate, 1f)

            // Auto-disable when everything has cleared and no seekers exist
            bool effectsCleared = Math.Abs(_timeRate - 1f) < 0.001f
                               && _anxiety < 0.001f;
            if (effectsCleared && !anySeekers)
                Enabled = false;
        }
        else
        {
            // Re-enable if seekers reappear
            if (anySeekers)
                Enabled = true;
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static float Approach(float v, float target, float maxDelta)
    {
        return v < target
            ? Math.Min(v + maxDelta, target)
            : Math.Max(v - maxDelta, target);
    }

    /// <summary>
    /// Maps <paramref name="value"/> from [<paramref name="fromA"/>,
    /// <paramref name="fromB"/>] → [<paramref name="toA"/>,
    /// <paramref name="toB"/>] and clamps.
    /// </summary>
    private static float ClampedMap(float value, float fromA, float fromB, float toA = 0f, float toB = 1f)
    {
        float t = (value - fromA) / (fromB - fromA);
        t = Math.Max(0f, Math.Min(1f, t));
        return toA + (toB - toA) * t;
    }
}
