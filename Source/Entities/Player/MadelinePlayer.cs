using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;
using KirbyCelesteStandalone.Core;
using KirbyCelesteStandalone.Entities.Core;

namespace KirbyCelesteStandalone.Entities.Player;

/// <summary>
/// Faithful port of Celeste's Madeline player character to the Nez/MonoGame stack.
///
/// <para>
/// Implements the full movement ruleset from the NoelFB Celeste source:
/// running with acceleration/friction, variable-height jumping with coyote-time
/// (jumpGraceTimer), wall-sliding, wall-jumping, dash (8-direction) with cooldown
/// and dash-count management, and wall-climbing with stamina drain.
/// </para>
///
/// <para>
/// The state machine is a hand-rolled integer field (<see cref="_state"/>) rather
/// than Nez's built-in StateMachine, keeping the logic close to the original source.
/// All physics constants are declared as private/protected const floats matching
/// the NoelFB Player.cs values exactly.
/// </para>
///
/// <para>
/// Audio calls are represented as comments (<c>// TODO: play sound: …</c>).
/// Particle emissions are represented as comments (<c>// TODO: emit particles</c>).
/// Sprite/animation changes are stubbed through <see cref="PlayerSprite"/> calls.
/// </para>
/// </summary>
public class MadelinePlayer : CelesteActor
{
    // =========================================================================
    // State machine IDs
    // =========================================================================

    public const int StNormal    = 0;
    public const int StClimb     = 1;
    public const int StDash      = 2;
    public const int StBoost     = 4;
    public const int StDreamDash = 9;
    public const int StStarFly   = 19;
    public const int StDummy     = 11;

    // =========================================================================
    // Physics constants — copied verbatim from NoelFB Player.cs
    // =========================================================================

    public  const float MaxFall           = 160f;
    private const float Gravity           = 900f;
    private const float HalfGravThreshold = 40f;

    private const float FastMaxFall  = 240f;
    private const float FastMaxAccel = 300f;

    public  const float MaxRun    = 90f;
    public  const float RunAccel  = 1000f;
    private const float RunReduce = 400f;
    private const float AirMult   = 0.65f;

    private const float JumpGraceTime = 0.1f;
    private const float JumpSpeed     = -105f;
    private const float JumpHBoost    = 40f;
    private const float VarJumpTime   = 0.2f;
    private const int   UpwardCornerCorrection = 4;

    private const int   WallJumpCheckDist = 3;
    private const float WallJumpForceTime = 0.16f;
    private const float WallJumpHSpeed   = MaxRun + JumpHBoost;   // 130f

    public  const float WallSlideStartMax = 20f;
    private const float WallSlideTime     = 1.2f;

    private const float BounceSpeed            = -140f;
    private const float SuperBounceSpeed        = -185f;
    private const float SuperBounceVarJumpTime  = 0.2f;

    private const float DashSpeed      = 240f;
    private const float EndDashSpeed   = 160f;
    private const float EndDashUpMult  = 0.75f;
    private const float DashTime       = 0.15f;
    private const float DashCooldown   = 0.2f;
    private const float DashRefillCooldown = 0.1f;
    private const float DashAttackTime = 0.3f;

    public  const float ClimbMaxStamina  = 110f;
    private const float ClimbUpCost      = 100f / 2.2f;    // ≈ 45.45 /s
    private const float ClimbStillCost   = 100f / 10f;     // 10 /s
    private const float ClimbJumpCost    = 110f / 4f;      // 27.5
    private const int   ClimbCheckDist   = 2;
    private const float ClimbUpSpeed     = -45f;
    private const float ClimbDownSpeed   = 80f;
    private const float ClimbSlipSpeed   = 30f;
    private const float ClimbAccel       = 900f;
    private const float ClimbGrabYMult   = 0.2f;
    private const float ClimbJumpBoostTime = 0.2f;
    private const float ClimbNoMoveTime  = 0.1f;
    public  const float ClimbTiredThreshold = 20f;

    // =========================================================================
    // Hitbox dimensions
    // =========================================================================

    private const float NormalWidth  = 8f;
    private const float NormalHeight = 11f;
    private const float DuckHeight   = 6f;
    private const float StarFlyHeight = 8f;

    // =========================================================================
    // Hair colours (static, matching Celeste palette)
    // =========================================================================

    public static readonly Color NormalHairColor    = new Color(0xAC, 0x32, 0x32);
    public static readonly Color UsedHairColor      = new Color(0x44, 0xB7, 0xFF);
    public static readonly Color TwoDashesHairColor = new Color(0xFF, 0x6D, 0xEF);
    public static readonly Color FlyPowerHairColor  = new Color(0xF2, 0xEB, 0x6D);
    public static readonly Color FlashHairColor     = Color.White;

    // =========================================================================
    // Components
    // =========================================================================

    /// <summary>Hair spring-physics component attached to this entity.</summary>
    public PlayerHair Hair { get; private set; }

    /// <summary>Animation controller component attached to this entity.</summary>
    public PlayerSprite Sprite { get; private set; }

    // =========================================================================
    // Public state
    // =========================================================================

    /// <summary>Current velocity in pixels per second.</summary>
    public Vector2 Speed;

    /// <summary>Number of remaining dash charges.</summary>
    public int Dashes;

    /// <summary>Maximum dash charges for this character (default 1).</summary>
    public int MaxDashes = 1;

    /// <summary>Climb stamina. Depleted while climbing; refilled on ground.</summary>
    public float Stamina;

    /// <summary>Horizontal facing: 1 = right, −1 = left.</summary>
    public int Facing { get; protected set; } = 1;

    /// <summary>True if the player has been killed (suppresses further updates).</summary>
    public bool Dead { get; private set; }

    /// <summary>True while the player is in the active attack window of a dash.</summary>
    public bool DashAttacking => _dashAttackTimer > 0f;

    /// <summary>Whether the player is currently ducking (crouched hitbox).</summary>
    public bool Ducking
    {
        get => _ducking;
        set
        {
            if (_ducking == value) return;
            _ducking = value;

            if (_ducking)
            {
                Height = DuckHeight;
            }
            else
            {
                Height = NormalHeight;
            }

            // Rebuild the BoxCollider to match the new height.
            if (Collider != null)
            {
                RemoveComponent(Collider);
                Collider = AddComponent(new BoxCollider(0f, 0f, Width, Height));
                Collider.PhysicsLayer = PhysicsLayers.Actor;
            }

            UpdateBounds();
        }
    }
    private bool _ducking;

    /// <summary>Whether the player is in a state where they can take input.</summary>
    public bool InControl => _state != StDummy;

    /// <summary>True while the player is climbing a wall.</summary>
    public bool IsClimbing => _state == StClimb;

    /// <summary>True while the player is in a dash.</summary>
    public bool IsDashing => _state == StDash;

    /// <summary>
    /// True when the actor is on ground that should be considered "safe"
    /// (not a moving crusher etc.).  Kept for API parity with CelesteActor uses.
    /// </summary>
    public bool OnSafeGround { get; protected set; }

    // =========================================================================
    // Dash direction (used by external entities such as DashBlocks)
    // =========================================================================

    /// <summary>Normalised direction of the most recent dash.</summary>
    public Vector2 DashDir { get; protected set; }

    // =========================================================================
    // Private timer / state fields
    // =========================================================================

    private int   _state          = StNormal;

    private float _jumpGraceTimer;
    private float _varJumpTimer;
    private float _varJumpSpeed;

    private float _dashCooldownTimer;
    private float _dashRefillCooldownTimer;
    private float _dashAttackTimer;

    private float _forceMoveXTimer;
    private int   _forceMoveX;

    private float _wallBoostTimer;
    private int   _wallBoostDir;

    private float _wallSlideTimer = WallSlideTime;
    private int   _wallSlideDir;

    private float _climbNoMoveTimer;
    private int   _lastClimbMove;

    private float _maxFall;

    private bool  _onGround;
    private bool  _wasOnGround;

    private int   _moveX;

    // Dash internal
    private Vector2 _beforeDashSpeed;
    private bool    _dashStartedOnGround;

    // =========================================================================
    // Constructor
    // =========================================================================

    /// <summary>
    /// Creates a new Madeline player at <paramref name="position"/>.
    /// </summary>
    /// <param name="position">World-space spawn position (top-left of hitbox).</param>
    public MadelinePlayer(Vector2 position)
        : base(position, NormalWidth, NormalHeight)
    {
        // Dash / stamina defaults.
        Dashes   = 1;
        MaxDashes = 1;
        Stamina  = ClimbMaxStamina;
        _maxFall = MaxFall;

        // Add sprite + hair components.
        Sprite = AddComponent(new PlayerSprite());
        Hair   = AddComponent(new PlayerHair());

        Hair.HairColor = NormalHairColor;
        Hair.Facing    = Facing;
    }

    // =========================================================================
    // Nez lifecycle
    // =========================================================================

    /// <inheritdoc/>
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        // BoxCollider is created by CelesteActor.OnAddedToScene via Width/Height.
        UpdateBounds();
    }

    // =========================================================================
    // Main update loop
    // =========================================================================

    /// <inheritdoc/>
    public override void Update()
    {
        if (Dead) return;

        base.Update(); // decays LiftSpeed grace timer

        float dt = Time.DeltaTime;

        // ---- 1. Tick timers -------------------------------------------------

        if (_jumpGraceTimer > 0f)         _jumpGraceTimer         -= dt;
        if (_varJumpTimer > 0f)           _varJumpTimer           -= dt;
        if (_dashCooldownTimer > 0f)      _dashCooldownTimer      -= dt;
        if (_dashRefillCooldownTimer > 0f) _dashRefillCooldownTimer -= dt;
        if (_dashAttackTimer > 0f)        _dashAttackTimer        -= dt;
        if (_forceMoveXTimer > 0f)        _forceMoveXTimer        -= dt;
        if (_climbNoMoveTimer > 0f)       _climbNoMoveTimer       -= dt;

        // Wall boost: if player inputs in wallBoostDir within ClimbJumpBoostTime,
        // snap to WallJumpHSpeed (lets climb-jump convert to wall-jump).
        if (_wallBoostTimer > 0f)
        {
            _wallBoostTimer -= dt;
            if (_moveX == _wallBoostDir)
            {
                Speed.X = WallJumpHSpeed * _moveX;
                Stamina += ClimbJumpCost;
                _wallBoostTimer = 0f;
            }
        }

        // Wall-slide dir resets each frame (set again in NormalUpdate if still sliding).
        if (_wallSlideDir != 0)
        {
            _wallSlideTimer = Math.Max(_wallSlideTimer - dt, 0f);
            _wallSlideDir   = 0;
        }

        // ---- 2. Read horizontal input ---------------------------------------

        _moveX = 0;
        if (Nez.Input.IsKeyDown(Keys.Left)  || Nez.Input.IsKeyDown(Keys.A)) _moveX -= 1;
        if (Nez.Input.IsKeyDown(Keys.Right) || Nez.Input.IsKeyDown(Keys.D)) _moveX += 1;

        // Force-move overrides input during wall-jump runout.
        if (_forceMoveXTimer > 0f) _moveX = _forceMoveX;

        // ---- 3. Update facing -----------------------------------------------

        if (_moveX != 0 && _state != StClimb) Facing = _moveX;
        Hair.Facing = Facing;

        // ---- 4. Ground check ------------------------------------------------

        _wasOnGround = _onGround;

        if (_state == StDash && DashDir.Y != 0)
            _onGround = OnSafeGround = false;
        else if (Speed.Y >= 0f)
            _onGround = OnGround();
        else
            _onGround = false;

        OnSafeGround = _onGround;

        // ---- 5. Per-ground-landing bookkeeping ------------------------------

        if (_onGround)
        {
            _jumpGraceTimer  = JumpGraceTime;
            _wallSlideTimer  = WallSlideTime;
            Stamina          = ClimbMaxStamina;

            // Refill dash on ground (only when not in dash and refill cooldown elapsed).
            if (_state != StClimb && _dashRefillCooldownTimer <= 0f)
                RefillDash(false);
        }
        else if (_wasOnGround)
        {
            // Just left ground — coyote-time grace already set above.
        }

        // ---- 6. State machine update ----------------------------------------

        int nextState = _state switch
        {
            StNormal    => NormalUpdate(dt),
            StClimb     => ClimbUpdate(dt),
            StDash      => DashUpdate(dt),
            StDummy     => StDummy,
            _           => _state,
        };

        if (nextState != _state)
        {
            OnStateExit(_state);
            _state = nextState;
            OnStateEnter(_state);
        }

        // ---- 7. Apply movement ----------------------------------------------

        MoveH(Speed.X * dt, OnCollideH);
        MoveV(Speed.Y * dt, OnCollideV);

        // ---- 8. Hair post-update -------------------------------------------

        UpdateHairColor();
        Hair.AfterUpdate();

        // ---- 9. Sprite animation -------------------------------------------

        UpdateSprite();
    }

    // =========================================================================
    // State enter / exit hooks
    // =========================================================================

    private void OnStateEnter(int state)
    {
        switch (state)
        {
            case StNormal:
                _maxFall = MaxFall;
                break;

            case StClimb:
                Speed.X = 0f;
                Speed.Y *= ClimbGrabYMult;
                _wallSlideTimer  = WallSlideTime;
                _climbNoMoveTimer = ClimbNoMoveTime;
                _wallBoostTimer  = 0f;
                _lastClimbMove   = 0;
                // TODO: play sound: char_mad_grab
                break;

            case StDash:
                DashBegin();
                break;
        }
    }

    private void OnStateExit(int state)
    {
        switch (state)
        {
            case StNormal:
                _wallBoostTimer = 0f;
                break;

            case StDash:
                // CallDashEvents equivalent — already handled inline.
                break;
        }
    }

    // =========================================================================
    // NormalUpdate — the main ground/air movement state
    // =========================================================================

    private int NormalUpdate(float dt)
    {
        // --- Grab / Climb ---------------------------------------------------
        bool grabHeld = Nez.Input.IsKeyDown(Keys.C) || Nez.Input.IsKeyDown(Keys.LeftControl);
        bool isTired  = Stamina < ClimbTiredThreshold;

        if (grabHeld && !isTired && !Ducking && Speed.Y >= 0f && Math.Sign(Speed.X) != -Facing)
        {
            // Check wall in facing direction.
            if (WallCheckSolid(Facing, ClimbCheckDist))
            {
                Ducking = false;
                return StClimb;
            }
        }

        // --- Dash -----------------------------------------------------------
        bool dashPressed = Nez.Input.IsKeyPressed(Keys.X)
                        || Nez.Input.IsKeyPressed(Keys.LeftShift)
                        || Nez.Input.IsKeyPressed(Keys.RightShift);

        if (dashPressed && _dashCooldownTimer <= 0f && Dashes > 0)
        {
            _beforeDashSpeed = Speed + LiftSpeed;
            Dashes = Math.Max(0, Dashes - 1);
            return StDash;
        }

        // --- Ducking --------------------------------------------------------
        bool downHeld = Nez.Input.IsKeyDown(Keys.Down) || Nez.Input.IsKeyDown(Keys.S);

        if (!Ducking)
        {
            if (_onGround && downHeld && Speed.Y >= 0f)
            {
                Ducking = true;
                // TODO: emit particles (duck)
            }
        }
        else
        {
            if (_onGround && !downHeld)
            {
                // Attempt to un-duck: check if full-height hitbox is clear.
                if (!CollideSolidAtSize(Width, NormalHeight))
                {
                    Ducking = false;
                }
            }
        }

        // --- Horizontal movement --------------------------------------------
        if (Ducking && _onGround)
        {
            // Duck friction.
            Speed.X = Approach(Speed.X, 0f, 500f * dt);
        }
        else
        {
            float mult = _onGround ? 1f : AirMult;

            if (Math.Abs(Speed.X) > MaxRun && Math.Sign(Speed.X) == _moveX)
                Speed.X = Approach(Speed.X, MaxRun * _moveX, RunReduce * mult * dt);
            else
                Speed.X = Approach(Speed.X, MaxRun * _moveX, RunAccel * mult * dt);
        }

        // --- Vertical / gravity / fast-fall ---------------------------------
        if (!_onGround)
        {
            // Fast-fall: pressing Down and speed already at or past MaxFall.
            if (downHeld && Speed.Y >= MaxFall)
                _maxFall = Approach(_maxFall, FastMaxFall, FastMaxAccel * dt);
            else
                _maxFall = Approach(_maxFall, MaxFall, FastMaxAccel * dt);

            // Wall-slide detection (reduces max fall speed).
            bool movingIntoWall = _moveX == Facing || (grabHeld && _moveX == 0);
            if (movingIntoWall && !downHeld)
            {
                if (Speed.Y >= 0f && _wallSlideTimer > 0f && !Ducking
                    && WallCheckSolid(Facing, 1))
                {
                    _wallSlideDir = Facing;
                }
            }

            float localMax = _maxFall;
            if (_wallSlideDir != 0)
            {
                // Interpolate max fall toward WallSlideStartMax over timer.
                localMax = MathHelper.Lerp(MaxFall, WallSlideStartMax, _wallSlideTimer / WallSlideTime);
                // TODO: emit wall-slide particles
            }

            // Half-grav when near apex and jump held.
            bool jumpHeld = Nez.Input.IsKeyDown(Keys.Z)
                         || Nez.Input.IsKeyDown(Keys.Space)
                         || Nez.Input.IsKeyDown(Keys.C);
            float gravMult = (Math.Abs(Speed.Y) < HalfGravThreshold && jumpHeld) ? 0.5f : 1f;

            Speed.Y = Approach(Speed.Y, localMax, Gravity * gravMult * dt);
        }

        // --- Variable jump --------------------------------------------------
        if (_varJumpTimer > 0f)
        {
            bool jumpHeld = Nez.Input.IsKeyDown(Keys.Z)
                         || Nez.Input.IsKeyDown(Keys.Space)
                         || Nez.Input.IsKeyDown(Keys.C);
            if (jumpHeld)
                Speed.Y = Math.Min(Speed.Y, _varJumpSpeed);
            else
                _varJumpTimer = 0f;
        }

        // --- Jump -----------------------------------------------------------
        bool jumpPressed = Nez.Input.IsKeyPressed(Keys.Z)
                        || Nez.Input.IsKeyPressed(Keys.Space);

        if (jumpPressed)
        {
            if (_jumpGraceTimer > 0f)
            {
                // Normal ground / coyote jump.
                DoJump();
            }
            else
            {
                // Wall jump — check both sides.
                if (WallCheckSolid(1, WallJumpCheckDist))
                    DoWallJump(-1);
                else if (WallCheckSolid(-1, WallJumpCheckDist))
                    DoWallJump(1);
            }
        }

        return StNormal;
    }

    // =========================================================================
    // ClimbUpdate — player is hanging on a wall
    // =========================================================================

    private int ClimbUpdate(float dt)
    {
        _climbNoMoveTimer -= dt;

        // Refill stamina when feet touch the ground.
        if (_onGround)
            Stamina = ClimbMaxStamina;

        // Facing must remain toward the wall.
        // (already set on enter; don't reset from moveX input)

        // --- Wall-jump / jump while climbing --------------------------------
        bool jumpPressed = Nez.Input.IsKeyPressed(Keys.Z)
                        || Nez.Input.IsKeyPressed(Keys.Space);

        if (jumpPressed)
        {
            if (_moveX == -Facing)
            {
                // Player is pushing away from wall → pure wall-jump.
                DoWallJump(-Facing);
            }
            else
            {
                // Climb-jump: straight up with a boost and a short grace for
                // switching to a wall-jump direction.
                if (!_onGround)
                    Stamina -= ClimbJumpCost;

                Speed.Y  = JumpSpeed;
                Speed.X  = -Facing * WallJumpHSpeed;

                _jumpGraceTimer  = 0f;
                _varJumpTimer    = VarJumpTime;
                _varJumpSpeed    = Speed.Y;
                _wallBoostDir    = -Facing;
                _wallBoostTimer  = ClimbJumpBoostTime;

                // TODO: play sound: char_mad_jump_climb
                // TODO: emit particles (climb jump)
            }
            return StNormal;
        }

        // --- Dash from climb ------------------------------------------------
        bool dashPressed = Nez.Input.IsKeyPressed(Keys.X)
                        || Nez.Input.IsKeyPressed(Keys.LeftShift)
                        || Nez.Input.IsKeyPressed(Keys.RightShift);

        if (dashPressed && _dashCooldownTimer <= 0f && Dashes > 0)
        {
            _beforeDashSpeed = Speed + LiftSpeed;
            Dashes = Math.Max(0, Dashes - 1);
            return StDash;
        }

        // --- Release grab ---------------------------------------------------
        bool grabHeld = Nez.Input.IsKeyDown(Keys.C) || Nez.Input.IsKeyDown(Keys.LeftControl);
        if (!grabHeld)
        {
            // TODO: play sound: char_mad_grab_letgo
            return StNormal;
        }

        // --- Check wall is still there --------------------------------------
        if (!WallCheckSolid(Facing, 1))
        {
            // Climbed over ledge or wall ended.
            if (Speed.Y < 0f)
            {
                // Hop onto the ledge surface.
                Speed.X = Facing * 100f;
                Speed.Y = Math.Min(Speed.Y, -120f);
            }
            return StNormal;
        }

        // --- Vertical movement while climbing --------------------------------
        bool upHeld   = Nez.Input.IsKeyDown(Keys.Up)   || Nez.Input.IsKeyDown(Keys.W);
        bool downHeld = Nez.Input.IsKeyDown(Keys.Down)  || Nez.Input.IsKeyDown(Keys.S);

        float target = 0f;
        bool trySlip = false;

        if (_climbNoMoveTimer <= 0f)
        {
            if (upHeld)
            {
                target = ClimbUpSpeed;

                // If something is above us, stop and slip.
                if (WallCheckSolid(0, -1))
                {
                    if (Speed.Y < 0f) Speed.Y = 0f;
                    target   = 0f;
                    trySlip  = true;
                }
            }
            else if (downHeld)
            {
                target = ClimbDownSpeed;

                // Stop at ground.
                if (_onGround)
                {
                    if (Speed.Y > 0f) Speed.Y = 0f;
                    target = 0f;
                }
                else
                {
                    // TODO: emit wall-slide particles (climbing down)
                }
            }
            else
            {
                trySlip = true;
            }
        }
        else
        {
            trySlip = true;
        }

        _lastClimbMove = Math.Sign(target);

        // Slip check: if the player's grip is at the top edge of the wall segment,
        // let them slide down.
        if (trySlip && CheckClimbSlip())
            target = ClimbSlipSpeed;

        Speed.Y = Approach(Speed.Y, target, ClimbAccel * dt);

        // Prevent moving down if wall ends below.
        if (!downHeld && Speed.Y > 0f && !WallCheckSolid(Facing, 1))
            Speed.Y = 0f;

        // --- Stamina drain --------------------------------------------------
        if (_climbNoMoveTimer <= 0f)
        {
            DrainClimbStamina(dt);
        }

        // Slip off if out of stamina.
        if (Stamina <= 0f)
        {
            // TODO: play sound: char_mad_tired_death
            return StNormal;
        }

        return StClimb;
    }

    /// <summary>
    /// Returns true if the grip-point is above the solid edge (player would slip).
    /// Mirrors Celeste's SlipCheck.
    /// </summary>
    private bool CheckClimbSlip()
    {
        // Check one pixel above and 4 pixels into the wall face.
        Vector2 gripPoint = Position + new Vector2(Facing * 4f, -4f);
        return CollideSolid(gripPoint - Position) == null;
    }

    /// <summary>Drains stamina according to climb move direction.</summary>
    protected virtual void DrainClimbStamina(float dt)
    {
        if (_lastClimbMove == -1)
            Stamina -= ClimbUpCost * dt;
        else if (_lastClimbMove == 0)
            Stamina -= ClimbStillCost * dt;
        // Moving down is free.
    }

    // =========================================================================
    // DashUpdate — player is dashing
    // =========================================================================

    private void DashBegin()
    {
        _dashCooldownTimer       = DashCooldown;
        _dashRefillCooldownTimer = DashRefillCooldown;
        _dashAttackTimer         = DashAttackTime;
        _dashStartedOnGround     = _onGround;
        _wallSlideTimer          = WallSlideTime;

        Speed    = Vector2.Zero;
        DashDir  = Vector2.Zero;

        // Determine 8-directional dash aim from input.
        Vector2 aim = Vector2.Zero;
        if (Nez.Input.IsKeyDown(Keys.Left)  || Nez.Input.IsKeyDown(Keys.A)) aim.X -= 1f;
        if (Nez.Input.IsKeyDown(Keys.Right) || Nez.Input.IsKeyDown(Keys.D)) aim.X += 1f;
        if (Nez.Input.IsKeyDown(Keys.Up)    || Nez.Input.IsKeyDown(Keys.W)) aim.Y -= 1f;
        if (Nez.Input.IsKeyDown(Keys.Down)  || Nez.Input.IsKeyDown(Keys.S)) aim.Y += 1f;

        if (aim == Vector2.Zero)
            aim.X = Facing;

        if (aim != Vector2.Zero)
            aim.Normalize();

        // Preserve X speed if it exceeds dash speed in dash direction.
        float newSpeedX = aim.X * DashSpeed;
        if (Math.Sign(_beforeDashSpeed.X) == Math.Sign(newSpeedX)
            && Math.Abs(_beforeDashSpeed.X) > Math.Abs(newSpeedX))
            newSpeedX = _beforeDashSpeed.X;

        Speed   = new Vector2(newSpeedX, aim.Y * DashSpeed);
        DashDir = aim;

        if (DashDir.X != 0)
            Facing = Math.Sign((int)DashDir.X);

        // Ground-dash slide: downward diagonal on ground → convert to horizontal.
        if (_onGround && DashDir.X != 0 && DashDir.Y > 0f && Speed.Y > 0f)
        {
            DashDir = new Vector2(Math.Sign(DashDir.X), 0f);
            Speed.Y = 0f;
            Speed.X *= 1.2f; // DodgeSlideSpeedMult
            Ducking  = true;
        }

        if (!_onGround && Ducking && !CollideSolidAtSize(Width, NormalHeight))
            Ducking = false;

        // TODO: play sound: char_mad_dash_red_right / _left
        // TODO: emit particles (dash attack)
    }

    private int DashUpdate(float dt)
    {
        // Dash-attack timer is ticked in main Update().
        // Transition to normal when DashTime expires.
        // We track elapsed dash time via _dashAttackTimer being set to DashAttackTime on begin
        // and then also a _dashTime tracker below.

        // NOTE: In the original source the dash has two phases managed by a coroutine:
        //   Phase 1 (0 → DashTime=0.15s):  full DashSpeed, freeze gravity.
        //   Phase 2 (DashTime → end):       speed snaps to EndDashSpeed direction.
        // We replicate this with a separate field.

        _dashTimer -= dt;

        if (_dashTimer <= 0f)
        {
            // End of dash: reduce speed.
            if (DashDir.Y <= 0f)
            {
                Speed = DashDir * EndDashSpeed;
            }
            if (Speed.Y < 0f)
                Speed.Y *= EndDashUpMult;

            // TODO: play sound: dash end
            return StNormal;
        }

        // During dash: no gravity.
        // Speed is already set in DashBegin and maintained at full dash speed.
        // (Do not apply gravity here; MoveH/MoveV will carry the entity.)

        // Wall/super jump opportunity during dash.
        bool jumpPressed = Nez.Input.IsKeyPressed(Keys.Z)
                        || Nez.Input.IsKeyPressed(Keys.Space);

        if (jumpPressed && DashDir.Y == 0f && _jumpGraceTimer > 0f)
        {
            // Super-jump: retains dash X + full jump Y.
            DoJump();
            return StNormal;
        }

        if (jumpPressed)
        {
            if (WallCheckSolid(1, WallJumpCheckDist))
            {
                DoWallJump(-1);
                return StNormal;
            }
            else if (WallCheckSolid(-1, WallJumpCheckDist))
            {
                DoWallJump(1);
                return StNormal;
            }
        }

        // TODO: emit dash trail particles

        return StDash;
    }

    // The dash phase 1 duration tracked separately (DashAttackTime is for hitbox,
    // DashTime controls when we transition out of StDash).
    private float _dashTimer;

    // =========================================================================
    // Jump helpers
    // =========================================================================

    /// <summary>Performs a standard ground jump.</summary>
    protected void DoJump()
    {
        _jumpGraceTimer = 0f;
        _varJumpTimer   = VarJumpTime;
        _varJumpSpeed   = JumpSpeed;

        Speed.X += JumpHBoost * _moveX;
        Speed.Y  = JumpSpeed;

        // Absorb lift speed.
        Speed += new Vector2(0f, Math.Min(LiftSpeed.Y, 0f));

        _wallSlideTimer = WallSlideTime;
        _wallBoostTimer = 0f;

        // TODO: play sound: char_mad_jump
        // TODO: emit particles (jump dust)
    }

    /// <summary>Performs a wall jump in direction <paramref name="dir"/>.</summary>
    protected void DoWallJump(int dir)
    {
        _jumpGraceTimer = 0f;
        _varJumpTimer   = VarJumpTime;
        _varJumpSpeed   = JumpSpeed;

        Speed.X = WallJumpHSpeed * dir;
        Speed.Y = JumpSpeed;

        Speed += new Vector2(0f, Math.Min(LiftSpeed.Y, 0f));

        if (_moveX != 0)
        {
            _forceMoveX      = dir;
            _forceMoveXTimer = WallJumpForceTime;
        }

        _wallSlideTimer = WallSlideTime;
        _wallBoostTimer = 0f;

        Ducking = false;

        // TODO: play sound: char_mad_jump_wall
        // TODO: emit particles (wall-jump dust)
    }

    // =========================================================================
    // Public mechanics API
    // =========================================================================

    /// <summary>
    /// Spring / bounce entity upward (vertical bounce block, spring, etc.).
    /// </summary>
    /// <param name="fromY">Y-coordinate of the bounce surface (top of spring).</param>
    public virtual void SuperBounce(float fromY)
    {
        _state = StNormal;

        Speed.Y        = SuperBounceSpeed;
        _varJumpSpeed  = Speed.Y;
        _varJumpTimer  = SuperBounceVarJumpTime;
        _jumpGraceTimer = 0f;

        RefillDash(false);
        Stamina = ClimbMaxStamina;

        // TODO: play sound: char_mad_jump_superwall
        // TODO: emit particles (bounce)
    }

    /// <summary>
    /// Side-bounce from a wall (e.g. bumper).
    /// </summary>
    /// <param name="dir">+1 = bounce right, −1 = bounce left.</param>
    /// <param name="pushX">Horizontal push velocity.</param>
    /// <param name="centerY">Y centre of the bouncer.</param>
    /// <returns>True if the bounce was applied (player was in range).</returns>
    public virtual bool SideBounce(int dir, float pushX, float centerY)
    {
        float bottom = Position.Y + Height;
        float top    = Position.Y;

        // Only bounce if the player vertically overlaps the bumper.
        if (bottom < centerY - 8f || top > centerY + 8f)
            return false;

        Speed.X  = dir * WallJumpHSpeed;
        Speed.Y  = Math.Min(Speed.Y, BounceSpeed);

        RefillDash(false);
        Stamina = ClimbMaxStamina;

        _state = StNormal;

        // TODO: play sound: char_mad_hurt
        // TODO: emit particles (side bounce)

        return true;
    }

    /// <summary>
    /// Refills the player's dash charge(s).
    /// </summary>
    /// <param name="twoDashes">If true, refills up to 2 dashes (two-dash crystal).</param>
    /// <returns>True if dashes were actually replenished.</returns>
    public virtual bool RefillDash(bool twoDashes)
    {
        int before = Dashes;

        if (twoDashes)
            Dashes = Math.Min(2, MaxDashes + 1);
        else if (Dashes < MaxDashes)
            Dashes = MaxDashes;

        return Dashes != before;
    }

    // =========================================================================
    // Damage / death
    // =========================================================================

    /// <summary>
    /// Inflicts damage and applies a knockback impulse.
    /// </summary>
    public virtual void TakeDamage(int amount, Vector2 knockback)
    {
        if (Dead) return;

        Dead   = true;
        Speed  = knockback * 100f;
        _state = StDummy;

        // TODO: play sound: char_mad_death
        // TODO: emit particles (death effect)
        // TODO: trigger respawn sequence
    }

    /// <summary>Kills the player and sends them in <paramref name="direction"/>.</summary>
    public virtual void Die(Vector2 direction)
    {
        TakeDamage(1, direction);
    }

    // =========================================================================
    // Collision callbacks
    // =========================================================================

    /// <summary>Invoked by MoveH when the player hits a horizontal solid.</summary>
    protected virtual void OnCollideH(CelesteSolid solid)
    {
        // During a dash: emit a "rebound" effect if we still have attack time.
        if (DashAttacking && DashDir.X != 0f)
        {
            // TODO: play sound: dash wall impact
            // TODO: emit particles (dash impact)
        }

        Speed.X = 0f;
    }

    /// <summary>Invoked by MoveV when the player hits a vertical solid.</summary>
    protected virtual void OnCollideV(CelesteSolid solid)
    {
        if (Speed.Y > 0f)
        {
            // Landed.
            Speed.Y = 0f;
            _dashAttackTimer = 0f;

            // TODO: play sound: landing
            // TODO: emit particles (landing dust)
        }
        else if (Speed.Y < 0f)
        {
            // Hit ceiling — apply upward corner correction.
            Speed.Y = 0f;
            _varJumpTimer = 0f;

            // Corner-correct: slide up to UpwardCornerCorrection pixels sideways
            // to let the player pass a tight ceiling gap.
            for (int i = 1; i <= UpwardCornerCorrection; i++)
            {
                if (CollideSolid(new Vector2(i, -1)) == null)
                {
                    Position += new Vector2(i, -1f);
                    UpdateBounds();
                    return;
                }
                if (CollideSolid(new Vector2(-i, -1)) == null)
                {
                    Position += new Vector2(-i, -1f);
                    UpdateBounds();
                    return;
                }
            }
        }
    }

    // =========================================================================
    // Hair colour logic
    // =========================================================================

    private void UpdateHairColor()
    {
        Color target;
        if (_state == StStarFly)
        {
            target = FlyPowerHairColor;
        }
        else if (Dashes >= 2)
        {
            target = TwoDashesHairColor;
        }
        else if (Dashes == 1)
        {
            target = NormalHairColor;
        }
        else
        {
            target = UsedHairColor;
        }

        Hair.HairColor = target;
    }

    // =========================================================================
    // Sprite update
    // =========================================================================

    private void UpdateSprite()
    {
        if (_state == StDash || DashAttacking)
        {
            Sprite.Play(PlayerSprite.Animations.Dash);
        }
        else if (_state == StClimb)
        {
            if (_lastClimbMove < 0)
                Sprite.Play(PlayerSprite.Animations.Climb);
            else
                Sprite.Play(PlayerSprite.Animations.Fall);
        }
        else if (_onGround)
        {
            if (_moveX != 0)
                Sprite.Play(PlayerSprite.Animations.Run);
            else
                Sprite.Play(PlayerSprite.Animations.Idle);
        }
        else if (Speed.Y < 0f)
        {
            Sprite.Play(PlayerSprite.Animations.Jump);
        }
        else
        {
            Sprite.Play(PlayerSprite.Animations.Fall);
        }
    }

    // =========================================================================
    // Collision helper utilities
    // =========================================================================

    /// <summary>
    /// Returns true if there is a solid <paramref name="dist"/> pixels in the
    /// given horizontal direction (or vertical if dir==0).
    /// </summary>
    protected bool WallCheckSolid(int dir, int dist)
    {
        if (dir == 0)
        {
            // Vertical check (used for ceiling in climb).
            return CollideSolid(new Vector2(0f, dist)) != null;
        }
        return CollideSolid(new Vector2(dir * dist, 0f)) != null;
    }

    /// <summary>
    /// Tests whether the player's current position (top-left anchor) overlaps a
    /// solid when the hitbox is resized to <paramref name="w"/> × <paramref name="h"/>.
    /// Used for unduck checks.
    /// </summary>
    private bool CollideSolidAtSize(float w, float h)
    {
        if (Scene == null) return false;
        var testBounds = new RectangleF(Position.X, Position.Y, w, h);
        for (int i = 0; i < Scene.Entities.Count; i++)
        {
            var entity = Scene.Entities[i];
            if (entity == this) continue;
            if (entity is CelesteSolid solid && solid.Collidable && solid.Bounds.Intersects(testBounds))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Linear approach (mirrors Monocle's Calc.Approach).
    /// </summary>
    protected static float Approach(float val, float target, float maxMove)
    {
        if (val < target)
            return Math.Min(val + maxMove, target);
        if (val > target)
            return Math.Max(val - maxMove, target);
        return val;
    }

    // =========================================================================
    // State machine property (read-only external access)
    // =========================================================================

    /// <summary>The current state machine ID.</summary>
    public int State => _state;

    /// <summary>Forces a state transition (use sparingly from external code).</summary>
    public void ForceState(int newState)
    {
        if (newState == _state) return;
        OnStateExit(_state);
        _state = newState;
        OnStateEnter(_state);
    }
}
