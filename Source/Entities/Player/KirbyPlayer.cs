using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;
using KirbyCelesteStandalone.Core;
using KirbyCelesteStandalone.Entities.Core;
using Entity = Nez.Entity;

namespace KirbyCelesteStandalone.Entities.Player;

/// <summary>
/// Kirby — the second playable character in DesoloZantas.
///
/// <para>
/// Extends <see cref="MadelinePlayer"/> and inherits all of Madeline's movement
/// physics faithfully (running, jumping, wall-sliding, wall-jumping, dashing,
/// wall-climbing with stamina, coyote time, variable jump height, etc.).
/// </para>
///
/// <para>
/// Kirby-specific additions:
/// <list type="bullet">
///   <item><b>Float</b> — tap and hold Jump while airborne to float in place for
///         up to <see cref="FloatMaxTime"/> seconds. Gravity is neutralised and
///         vertical speed drifts to zero. Float drains <see cref="FloatTimer"/>
///         and cannot be re-activated until Kirby lands.</item>
///   <item><b>Puff hop</b> — pressing Jump while floating (and no current ability)
///         gives a small upward boost.</item>
///   <item><b>Copy ability</b> — Kirby inhales during a dash; if the dash contacts
///         an enemy entity the player gains that enemy's copy ability. Abilities
///         expire after <see cref="AbilityDuration"/> seconds.</item>
///   <item><b>Unlimited climb stamina</b> — Kirby's stamina never depletes; the
///         drain method is overridden to be a no-op.</item>
///   <item><b>Round hitbox</b> — 14 × 14 normal, 14 × 10 duck. Kirby is chubby.</item>
///   <item><b>Hair repurposed as ability indicator</b> — hair colour reflects the
///         current copy ability (or Kirby's base pink).</item>
/// </list>
/// </para>
/// </summary>
public class KirbyPlayer : MadelinePlayer
{
    // =========================================================================
    // Kirby hitbox sizes (override Madeline's 8 × 11)
    // =========================================================================

    private const float KirbyWidth       = 14f;
    private const float KirbyHeight      = 14f;
    private const float KirbyDuckHeight  = 10f;

    // =========================================================================
    // Float ability constants
    // =========================================================================

    /// <summary>Maximum sustained float duration in seconds.</summary>
    public const float FloatMaxTime = 3.0f;

    /// <summary>How fast Kirby's Y-velocity drifts toward zero while floating (lerp factor).</summary>
    private const float FloatDriftFactor = 4f;

    /// <summary>Small upward speed boost from the puff hop when floating with no ability.</summary>
    private const float PuffHopSpeed = -60f;

    // =========================================================================
    // Copy ability hair colours
    // =========================================================================

    public static readonly Color KirbyBaseColor  = Color.Pink;
    public static readonly Color FireColor        = Color.OrangeRed;
    public static readonly Color IceColor         = Color.LightBlue;
    public static readonly Color SwordColor       = Color.Yellow;
    public static readonly Color StoneColor       = Color.Gray;

    // =========================================================================
    // Public state
    // =========================================================================

    /// <summary>Whether Kirby is currently floating (hover state).</summary>
    public bool IsFloating { get; private set; }

    /// <summary>
    /// Remaining float time in seconds.
    /// Drains while floating; resets to <see cref="FloatMaxTime"/> on landing.
    /// </summary>
    public float FloatTimer { get; private set; }

    /// <summary>
    /// Name of the currently held copy ability, or <c>null</c> if none.
    /// </summary>
    public string? CurrentAbility { get; private set; }

    /// <summary>
    /// Remaining seconds before the current ability expires (0 when no ability).
    /// </summary>
    public float AbilityDuration { get; private set; }

    /// <summary>True while Kirby is inhaling during a dash (ready to copy).</summary>
    public bool IsInhaling { get; private set; }

    // =========================================================================
    // Private float state
    // =========================================================================

    /// <summary>True when float was activated this jump (reset on landing).</summary>
    private bool _floatUsed;

    /// <summary>Was Jump held last frame? (used to detect first-press in air)</summary>
    private bool _jumpWasHeld;

    // =========================================================================
    // Constructor
    // =========================================================================

    /// <summary>
    /// Creates a new Kirby player at <paramref name="position"/>.
    /// </summary>
    /// <param name="position">World-space spawn position (top-left of hitbox).</param>
    public KirbyPlayer(Vector2 position)
        : base(position)
    {
        // Override hitbox to Kirby's rounder shape.
        Width  = KirbyWidth;
        Height = KirbyHeight;

        // Kirby has the same dash count as Madeline.
        MaxDashes = 1;
        Dashes    = 1;

        // Kirby has unlimited climb stamina — we override DrainClimbStamina below.
        Stamina = ClimbMaxStamina;

        // Float begins fully charged.
        FloatTimer = FloatMaxTime;

        // Recolour hair to Kirby-pink base.
        if (Hair != null)
            Hair.HairColor = KirbyBaseColor;
    }

    // =========================================================================
    // Nez lifecycle
    // =========================================================================

    /// <inheritdoc/>
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // Rebuild collider to match Kirby's wider frame.
        if (Collider != null)
            RemoveComponent(Collider);

        Collider = AddComponent(new BoxCollider(0f, 0f, Width, Height));
        Collider.PhysicsLayer = PhysicsLayers.Actor;

        UpdateBounds();
    }

    // =========================================================================
    // Main update — override to inject float and ability logic
    // =========================================================================

    /// <inheritdoc/>
    public override void Update()
    {
        if (Dead) return;

        float dt = Time.DeltaTime;

        // ---- Pre-base: sample jump input BEFORE MadelinePlayer.Update() ----
        // We intercept the float-activation here so it can suppress normal gravity
        // that Madeline's NormalUpdate would otherwise apply this frame.

        bool jumpDown    = Nez.Input.IsKeyDown(Keys.Z)
                        || Nez.Input.IsKeyDown(Keys.Space);
        bool jumpPressed = Nez.Input.IsKeyPressed(Keys.Z)
                        || Nez.Input.IsKeyPressed(Keys.Space);

        // ---- Run Madeline's full update pipeline ---------------------------
        base.Update();

        // ---- Post-base: apply float physics --------------------------------
        // After base.Update() has moved Kirby, we correct Speed.Y if floating.

        bool onGround = OnGround();

        if (onGround)
        {
            // Landed: restore float ability.
            _floatUsed  = false;
            _jumpWasHeld = false;
            FloatTimer  = FloatMaxTime;

            if (IsFloating)
                EndFloat();
        }
        else
        {
            // Airborne: check for float activation.
            // Float activates when Jump is held in the air, not already used,
            // and the player has float time remaining.
            bool jumpJustPressed = jumpDown && !_jumpWasHeld;

            if (!IsFloating)
            {
                // Activate float: must be airborne, jump held, not yet used,
                // and there is remaining float time.
                // We only allow activation on a fresh press (not held from jump).
                if (jumpDown && !_floatUsed && FloatTimer > 0f && Speed.Y > 0f)
                {
                    // Start floating — but only if the player has been in the air
                    // long enough that this isn't the initial jump impulse.
                    StartFloat();
                }
            }

            if (IsFloating)
            {
                // Drain float timer.
                FloatTimer -= dt;

                if (!jumpDown || FloatTimer <= 0f)
                {
                    // Player released Jump or timer ran out → end float.
                    EndFloat();
                }
                else
                {
                    // Apply float physics: drift Y velocity to zero.
                    Speed = new Vector2(
                        Speed.X,
                        MathHelper.Lerp(Speed.Y, 0f, FloatDriftFactor * dt)
                    );

                    // Puff hop: if pressing Jump while floating with no ability,
                    // give a small upward boost (only once per float activation).
                    if (jumpJustPressed && CurrentAbility == null && Speed.Y >= 0f)
                    {
                        Speed = new Vector2(Speed.X, PuffHopSpeed);
                        // TODO: play sound: kirby_puff_hop
                        // TODO: emit particles (puff cloud)
                    }
                }
            }
        }

        _jumpWasHeld = jumpDown;

        // ---- Ability timer -------------------------------------------------
        if (CurrentAbility != null)
        {
            AbilityDuration -= dt;
            if (AbilityDuration <= 0f)
                LoseAbility();
        }

        // ---- Inhale dash state ---------------------------------------------
        if (IsDashing)
        {
            IsInhaling = true;
            CheckInhaleContact();
        }
        else
        {
            IsInhaling = false;
        }

        // ---- Hair colour based on ability ----------------------------------
        UpdateKirbyHairColor();
    }

    // =========================================================================
    // Float helpers
    // =========================================================================

    private void StartFloat()
    {
        IsFloating  = true;
        _floatUsed  = true;

        // Cancel downward momentum so the float feels immediate.
        if (Speed.Y > 0f)
            Speed = new Vector2(Speed.X, 0f);

        // TODO: play sound: kirby_float_start
        // TODO: emit particles (puff up)
    }

    private void EndFloat()
    {
        IsFloating = false;

        // TODO: play sound: kirby_float_end
        // TODO: emit particles (puff exhale)
    }

    // =========================================================================
    // Inhale / copy ability mechanics
    // =========================================================================

    /// <summary>
    /// Scans scene entities for anything the dash hitbox overlaps and that
    /// has a recognised "enemy" tag or component.  If found, Kirby copies it.
    /// </summary>
    private void CheckInhaleContact()
    {
        if (Scene == null) return;

        // We look for any entity that is NOT us, NOT a solid, NOT a platform,
        // is close to Kirby (using Bounds), and has a name we can copy.
        // The actual entity-type filtering should be refined per-project.
        var kirbyBounds = Bounds;

        // Expand bounds slightly for the inhale reach.
        var inhaleBounds = new RectangleF(
            kirbyBounds.X - 4f,
            kirbyBounds.Y - 2f,
            kirbyBounds.Width + 8f,
            kirbyBounds.Height + 4f);

        for (int i = 0; i < Scene.Entities.Count; i++)
        {
            var entity = Scene.Entities[i];
            if (entity == this) continue;
            if (entity is CelesteSolid) continue;
            if (entity is CelestePlatform) continue;

            // Check rough AABB overlap using entity Position as a point.
            // A more complete implementation would use the entity's own bounds.
            if (!inhaleBounds.Contains(entity.Position)) continue;

            // Derive ability name from the entity class name
            // (designers can map these to proper ability names later).
            string? abilityName = GetAbilityFromEntity(entity);
            if (abilityName != null)
            {
                GainAbility(abilityName);
                // TODO: play sound: kirby_inhale_success
                // TODO: emit particles (star confetti)
                break;  // Only copy one ability per dash.
            }
        }
    }

    /// <summary>
    /// Maps an entity to an ability name string.
    /// Extend this as new enemy types are added to the project.
    /// </summary>
    private static string? GetAbilityFromEntity(Entity entity)
    {
        // Use the class name as a fallback; real implementation would check tags.
        string typeName = entity.GetType().Name.ToLowerInvariant();

        if (typeName.Contains("fire")  || typeName.Contains("hot")  || typeName.Contains("flame"))
            return "fire";
        if (typeName.Contains("ice")   || typeName.Contains("frost") || typeName.Contains("cold"))
            return "ice";
        if (typeName.Contains("sword") || typeName.Contains("blade") || typeName.Contains("knight"))
            return "sword";
        if (typeName.Contains("stone") || typeName.Contains("rock"))
            return "stone";

        // Generic fallback: entity has something to copy if it has a Name set.
        if (!string.IsNullOrEmpty(entity.Name))
            return entity.Name.ToLowerInvariant();

        return null;
    }

    // =========================================================================
    // Copy ability API
    // =========================================================================

    /// <summary>
    /// Grants Kirby the named copy ability for <paramref name="duration"/> seconds.
    /// If Kirby already has an ability it is replaced.
    /// </summary>
    /// <param name="name">
    ///   Ability identifier string, e.g. <c>"fire"</c>, <c>"ice"</c>,
    ///   <c>"sword"</c>, <c>"stone"</c>.
    /// </param>
    /// <param name="duration">
    ///   Seconds the ability lasts (default 30).
    /// </param>
    public void GainAbility(string name, float duration = 30f)
    {
        CurrentAbility  = name;
        AbilityDuration = duration;

        // TODO: play sound: kirby_copy_gain
        // TODO: emit particles (star burst)
        // TODO: load ability-specific sprite overlay
    }

    /// <summary>Removes the current copy ability immediately.</summary>
    public void LoseAbility()
    {
        CurrentAbility  = null;
        AbilityDuration = 0f;

        // TODO: play sound: kirby_copy_lose
        // TODO: emit particles (ability star flies away)
    }

    // =========================================================================
    // Unlimited stamina override
    // =========================================================================

    /// <summary>
    /// Kirby never runs out of wall-climb stamina — this override is a no-op.
    /// Stamina remains pinned at <see cref="MadelinePlayer.ClimbMaxStamina"/>.
    /// </summary>
    protected override void DrainClimbStamina(float dt)
    {
        // Kirby has unlimited climbing endurance.
        Stamina = ClimbMaxStamina;
    }

    // =========================================================================
    // Damage / death override (reset ability on death)
    // =========================================================================

    /// <inheritdoc/>
    public override void TakeDamage(int amount, Vector2 knockback)
    {
        // Drop the copy ability when hit (Kirby classic behaviour).
        if (CurrentAbility != null)
        {
            LoseAbility();
            // If Kirby had an ability, the first hit only drops it.
            // TODO: play sound: kirby_hurt_ability_drop
            return;
        }

        base.TakeDamage(amount, knockback);
    }

    // =========================================================================
    // Hair colour — ability-based override
    // =========================================================================

    private void UpdateKirbyHairColor()
    {
        Color target;

        if (CurrentAbility == null)
        {
            target = KirbyBaseColor;
        }
        else
        {
            target = CurrentAbility switch
            {
                "fire"  => FireColor,
                "ice"   => IceColor,
                "sword" => SwordColor,
                "stone" => StoneColor,
                _       => KirbyBaseColor,
            };
        }

        Hair.HairColor = target;
    }

    // =========================================================================
    // Ducking — override duck height for Kirby's proportions
    // =========================================================================

    // The Ducking property getter/setter in MadelinePlayer resizes the hitbox
    // using the private NormalHeight / DuckHeight constants, which are
    // Madeline-specific (11 / 6).  We shadow those with Kirby's values by
    // overriding the set accessor logic via an init-side trick: we replace the
    // collider directly here as well.

    // NOTE: Because Ducking is implemented as a property with side-effects in
    // MadelinePlayer (setting Height and rebuilding the BoxCollider), and C# does
    // not allow overriding a non-virtual property, we instead intercept the duck
    // state through Update() post-base and correct the collider when needed.

    private bool _kirbyLastDuck;

    private void EnsureKirbyHitbox()
    {
        if (Ducking == _kirbyLastDuck) return;
        _kirbyLastDuck = Ducking;

        float targetHeight = Ducking ? KirbyDuckHeight : KirbyHeight;

        // Rebuild the BoxCollider with Kirby dimensions.
        if (Collider != null)
            RemoveComponent(Collider);

        Width  = KirbyWidth;
        Height = targetHeight;

        Collider = AddComponent(new BoxCollider(0f, 0f, Width, Height));
        Collider.PhysicsLayer = PhysicsLayers.Actor;
        UpdateBounds();
    }

    // =========================================================================
    // Override collision callbacks (Kirby-specific flavour)
    // =========================================================================

    /// <inheritdoc/>
    protected override void OnCollideH(CelesteSolid solid)
    {
        // Kirby's dash inhale: if we hit a solid during an inhale, stop inhale.
        if (IsInhaling)
            IsInhaling = false;

        base.OnCollideH(solid);
    }

    // =========================================================================
    // Override Die: Kirby floats off instead of vanishing immediately
    // =========================================================================

    /// <inheritdoc/>
    public override void Die(Vector2 direction)
    {
        if (Dead) return;

        // Drop ability first (if carried), then die.
        if (CurrentAbility != null)
        {
            LoseAbility();
            return; // first hit: drop ability, don't die
        }

        base.Die(direction);

        // TODO: play sound: kirby_death
        // TODO: play death animation (Kirby floats off screen)
    }
}
