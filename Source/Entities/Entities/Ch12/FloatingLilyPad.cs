namespace Celeste.Entities.Chapters.Ch12
{
    /// <summary>
    /// FloatingLilyPad - Wateredgefall platform that bobs on the surface and sinks
    /// under the player's weight. Ride it too long and it submerges (turning
    /// non-solid) before floating back up, so crossings have to keep moving.
    ///
    /// Placeholder art: green square with a darker rim and a leaf notch.
    /// </summary>
    [CustomEntity("DZ/FloatingLilyPad")]
    [Tracked]
    public class FloatingLilyPad : Solid
    {
        #region Enums
        private enum PadState
        {
            Floating,
            Sinking,
            Submerged,
            Rising
        }
        #endregion

        #region Fields
        private static readonly Color PadFill = Calc.HexToColor("4f9e56");
        private static readonly Color PadOutline = Calc.HexToColor("22522c");
        private static readonly Color PadVein = Calc.HexToColor("7cc985");

        private readonly float sinkSpeed;
        private readonly float sinkDepth;
        private readonly float riseSpeed;
        private readonly float submergeDelay;
        private readonly float resurfaceDelay;
        private readonly float driftSpeed;
        private readonly float driftDistance;
        private readonly float bobAmplitude;
        private readonly float bobSpeed;
        private readonly bool submerges;

        private readonly float baseY;
        private readonly float baseX;
        private PadState state = PadState.Floating;
        private float sinkOffset;
        private float stateTimer;
        private float bobTimer;
        private int driftDirection = 1;
        private float alpha = 1f;
        #endregion

        #region Constructor
        public FloatingLilyPad(EntityData data, Vector2 offset)
            : base(data.Position + offset, Math.Max(8, data.Width), Math.Max(4, data.Height), safe: true)
        {
            sinkSpeed = Math.Max(0f, data.Float("sinkSpeed", 22f));
            sinkDepth = Math.Max(1f, data.Float("sinkDepth", 14f));
            riseSpeed = Math.Max(1f, data.Float("riseSpeed", 34f));
            submergeDelay = Math.Max(0f, data.Float("submergeDelay", 0.5f));
            resurfaceDelay = Math.Max(0f, data.Float("resurfaceDelay", 1.6f));
            driftSpeed = data.Float("driftSpeed", 0f);
            driftDistance = Math.Max(0f, data.Float("driftDistance", 0f));
            bobAmplitude = Math.Max(0f, data.Float("bobAmplitude", 2f));
            bobSpeed = Math.Max(0f, data.Float("bobSpeed", 2.4f));
            submerges = data.Bool("submerges", true);

            baseY = Y;
            baseX = X;
            bobTimer = Calc.Random.NextFloat() * MathHelper.TwoPi;

            SurfaceSoundIndex = SurfaceIndex.Wood;
            Depth = -9000;
        }
        #endregion

        #region Private
        private void UpdateVertical()
        {
            bool ridden = HasPlayerRider();

            switch (state)
            {
                case PadState.Floating:
                    if (ridden && sinkSpeed > 0f)
                    {
                        state = PadState.Sinking;
                        stateTimer = 0f;
                    }
                    else
                    {
                        sinkOffset = Calc.Approach(sinkOffset, 0f, riseSpeed * Engine.DeltaTime);
                    }
                    break;

                case PadState.Sinking:
                    if (!ridden)
                    {
                        state = PadState.Floating;
                        stateTimer = 0f;
                        break;
                    }

                    sinkOffset = Calc.Approach(sinkOffset, sinkDepth, sinkSpeed * Engine.DeltaTime);

                    if (sinkOffset >= sinkDepth && submerges)
                    {
                        stateTimer += Engine.DeltaTime;
                        if (stateTimer >= submergeDelay)
                        {
                            Submerge();
                        }
                    }
                    break;

                case PadState.Submerged:
                    sinkOffset = Calc.Approach(sinkOffset, sinkDepth + 10f, sinkSpeed * Engine.DeltaTime);
                    alpha = Calc.Approach(alpha, 0.35f, Engine.DeltaTime * 3f);

                    stateTimer -= Engine.DeltaTime;
                    if (stateTimer <= 0f)
                    {
                        state = PadState.Rising;
                    }
                    break;

                case PadState.Rising:
                    sinkOffset = Calc.Approach(sinkOffset, 0f, riseSpeed * Engine.DeltaTime);
                    alpha = Calc.Approach(alpha, 1f, Engine.DeltaTime * 3f);

                    if (sinkOffset <= 0f)
                    {
                        state = PadState.Floating;
                        Collidable = true;
                        Audio.Play("event:/char/madeline/water_out", Center);
                    }
                    else if (sinkOffset < sinkDepth * 0.5f)
                    {
                        Collidable = true;
                    }
                    break;
            }

            float bob = state == PadState.Floating && bobAmplitude > 0f
                ? (float)Math.Sin(bobTimer) * bobAmplitude
                : 0f;

            MoveToY(baseY + sinkOffset + bob);
        }

        private void Submerge()
        {
            state = PadState.Submerged;
            stateTimer = resurfaceDelay;
            Collidable = false;
            Audio.Play("event:/char/madeline/water_in", Center);
        }

        private void UpdateDrift()
        {
            if (driftSpeed == 0f)
            {
                return;
            }

            if (driftDistance > 0f)
            {
                float travelled = X - baseX;
                if (driftDirection > 0 && travelled >= driftDistance)
                {
                    driftDirection = -1;
                }
                else if (driftDirection < 0 && travelled <= -driftDistance)
                {
                    driftDirection = 1;
                }
            }

            MoveH(driftSpeed * driftDirection * Engine.DeltaTime);
        }
        #endregion

        #region Entity Overrides
        public override void Update()
        {
            base.Update();

            bobTimer += bobSpeed * Engine.DeltaTime;

            UpdateDrift();
            UpdateVertical();
        }

        public override void Render()
        {
            Placeholder.Square(X, Y, Width, Height, PadFill * alpha, PadOutline * alpha);

            // Leaf veins.
            for (float x = X + 3f; x < Right - 2f; x += 6f)
            {
                Draw.Rect(x, Y + 1f, 1f, Math.Max(1f, Height - 2f), PadVein * (alpha * 0.6f));
            }

            if (state == PadState.Submerged || state == PadState.Rising)
            {
                // Ripple line at the surface height it will return to.
                Draw.Rect(X - 2f, baseY, Width + 4f, 1f, PadVein * 0.5f);
            }
        }
        #endregion
    }
}
