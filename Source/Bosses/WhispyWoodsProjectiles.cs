namespace Celeste.Entities
{
    /// <summary>
    /// Falling apple hazard dropped by WhispyWoodsBoss's Apple Drop attack.
    /// Falls with gravity until it hits a solid or times out, damaging the player on touch.
    /// Placeholder circle rendering until dedicated art exists.
    /// </summary>
    [Tracked]
    public class WhispyApple : Entity
    {
        private Vector2 speed;
        private float lifeTimer;
        private bool pendingRemove;
        private readonly Color color;
        private const float Gravity = 500f;
        private const float MaxLifetime = 4f;

        public WhispyApple(Vector2 position, Color? tint = null) : base(position)
        {
            Collider = new Hitbox(8f, 8f, -4f, -4f);
            Add(new PlayerCollider(OnPlayer));
            Depth = -100;
            color = tint ?? Calc.HexToColor("C0392B");
        }

        public override void Update()
        {
            base.Update();
            if (pendingRemove) { RemoveSelf(); return; }

            speed.Y += Gravity * Engine.DeltaTime;
            Position += speed * Engine.DeltaTime;

            if (CollideCheck<Solid>())
            {
                RemoveSelf();
                return;
            }

            lifeTimer += Engine.DeltaTime;
            if (lifeTimer > MaxLifetime)
                RemoveSelf();
        }

        public override void Render()
        {
            Draw.Circle(Position, 4f, color, 8);
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            player.Die((player.Center - Position).SafeNormalize());
            pendingRemove = true;
        }
    }

    /// <summary>
    /// Poisoned variant of WhispyApple used by the enraged Poison Apple Barrage attack.
    /// Same falling hazard, tinted purple/green to read as poisonous.
    /// </summary>
    public class WhispyPoisonApple : WhispyApple
    {
        public WhispyPoisonApple(Vector2 position)
            : base(position, Calc.HexToColor("6B3FA0"))
        {
        }
    }

    /// <summary>
    /// Ground spike hazard spawned by WhispyWoodsBoss's Root Spike attack.
    /// Emerges from the ground, holds briefly as a damaging hazard, then retracts.
    /// Placeholder triangle rendering until dedicated art exists.
    /// </summary>
    [Tracked]
    public class WhispyRootSpike : Entity
    {
        private enum SpikeState { Emerging, Holding, Retracting }

        private SpikeState state = SpikeState.Emerging;
        private float stateTimer;
        private float emergeAmount;
        private readonly float baseY;
        private const float EmergeDuration = 0.25f;
        private const float HoldDuration = 0.6f;
        private const float RetractDuration = 0.2f;
        private const float SpikeHeight = 20f;

        public WhispyRootSpike(Vector2 position) : base(position)
        {
            baseY = position.Y;
            Collider = new Hitbox(10f, SpikeHeight, -5f, -SpikeHeight);
            Add(new PlayerCollider(OnPlayer));
            Depth = -99;
            Collidable = false;
        }

        public override void Update()
        {
            base.Update();
            stateTimer += Engine.DeltaTime;

            switch (state)
            {
                case SpikeState.Emerging:
                    emergeAmount = Calc.Approach(emergeAmount, 1f, Engine.DeltaTime / EmergeDuration);
                    Collidable = emergeAmount > 0.5f;
                    if (stateTimer >= EmergeDuration)
                    {
                        state = SpikeState.Holding;
                        stateTimer = 0f;
                    }
                    break;

                case SpikeState.Holding:
                    if (stateTimer >= HoldDuration)
                    {
                        state = SpikeState.Retracting;
                        stateTimer = 0f;
                    }
                    break;

                case SpikeState.Retracting:
                    emergeAmount = Calc.Approach(emergeAmount, 0f, Engine.DeltaTime / RetractDuration);
                    Collidable = emergeAmount > 0.5f;
                    if (stateTimer >= RetractDuration)
                        RemoveSelf();
                    break;
            }

            Position = new Vector2(Position.X, baseY - SpikeHeight * emergeAmount);
        }

        public override void Render()
        {
            Vector2 tip = Position + new Vector2(0f, -SpikeHeight * emergeAmount);
            Vector2 baseLeft = Position + new Vector2(-5f, 0f);
            Vector2 baseRight = Position + new Vector2(5f, 0f);
            Draw.Line(baseLeft, tip, Color.SaddleBrown);
            Draw.Line(baseRight, tip, Color.SaddleBrown);
            Draw.Line(baseLeft, baseRight, Color.SaddleBrown);
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            player.Die((player.Center - Position).SafeNormalize());
        }
    }

    /// <summary>
    /// Spinning leaf projectile fired by WhispyWoodsBoss's Leaf Tornado attack.
    /// Travels in a straight line while rotating, damaging the player on touch.
    /// Placeholder rotated-square rendering until dedicated art exists.
    /// </summary>
    [Tracked]
    public class WhispyLeaf : Entity
    {
        private readonly Vector2 velocity;
        private float rotation;
        private float lifeTimer;
        private bool pendingRemove;
        private const float RotationSpeed = 12f;
        private const float MaxLifetime = 3f;

        public WhispyLeaf(Vector2 position, Vector2 direction, float speed = 90f) : base(position)
        {
            velocity = direction.SafeNormalize() * speed;
            Collider = new Hitbox(6f, 6f, -3f, -3f);
            Add(new PlayerCollider(OnPlayer));
            Depth = -100;
        }

        public override void Update()
        {
            base.Update();
            if (pendingRemove) { RemoveSelf(); return; }
            Position += velocity * Engine.DeltaTime;
            rotation += RotationSpeed * Engine.DeltaTime;

            lifeTimer += Engine.DeltaTime;
            bool offCamera = Scene is Level level && !level.IsInCamera(Position, 32f);
            if (lifeTimer > MaxLifetime || (lifeTimer > 0.5f && offCamera))
                RemoveSelf();
        }

        public override void Render()
        {
            Draw.Rect(new Vector2(-3f, -3f).Rotate(rotation) + Position, 6f, 6f, Calc.HexToColor("6DA34D"));
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            player.Die((player.Center - Position).SafeNormalize());
            pendingRemove = true;
        }
    }
}
