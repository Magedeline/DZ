namespace Celeste.Entities
{
    /// <summary>
    /// A solid barrier that blocks BlackholeFlareSideway and BlackholeRiser entities from passing through.
    /// When a blackhole entity collides with this blocker, it will be stopped, reversed, or destroyed based on settings.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/BlackholeFlareBlocker")]
    [Tracked]
    [HotReloadable]
    public class BlackholeFlareBlocker : Entity
    {
        public static ParticleType P_Block;

        public enum BlockBehavior
        {
            Stop,
            Reverse,
            Destroy
        }

        private float width;
        private float height;
        private BlockBehavior behavior;
        private bool affectsSideway;
        private bool affectsRiser;
        private bool visualEffect;
        private Color blockerColor;
        private float pulseTimer;

        public BlackholeFlareBlocker(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            width = data.Width;
            height = data.Height;
            
            string behaviorStr = data.Attr("behavior", "Stop");
            behavior = (BlockBehavior)Enum.Parse(typeof(BlockBehavior), behaviorStr, true);
            
            affectsSideway = data.Bool("affectsSideway", true);
            affectsRiser = data.Bool("affectsRiser", true);
            visualEffect = data.Bool("visualEffect", true);
            
            string colorHex = data.Attr("blockerColor", "8B00FF");
            blockerColor = Calc.HexToColor(colorHex);

            Collider = new Hitbox(width, height);
            Depth = -49; // Slightly in front of blackholes (-50)
            
            InitializeParticles();
        }

        private static void InitializeParticles()
        {
            if (P_Block == null)
            {
                P_Block = new ParticleType
                {
                    Source = GFX.Game["particles/blob"],
                    Color = Color.Purple,
                    Color2 = Color.White,
                    ColorMode = ParticleType.ColorModes.Blink,
                    FadeMode = ParticleType.FadeModes.Late,
                    LifeMin = 0.3f,
                    LifeMax = 0.6f,
                    Size = 0.6f,
                    SizeRange = 0.3f,
                    SpeedMin = 40f,
                    SpeedMax = 80f,
                    DirectionRange = (float)Math.PI * 2f,
                    SpinMin = -2f,
                    SpinMax = 2f
                };
            }
        }

        public override void Update()
        {
            base.Update();

            pulseTimer += Engine.DeltaTime * 3f;

            // Check for BlackholeFlareSideway collisions
            if (affectsSideway)
            {
                foreach (BlackholeFlareSideway sideway in Scene.Tracker.GetEntities<BlackholeFlareSideway>())
                {
                    if (CollideCheck(sideway))
                    {
                        HandleBlackholeCollision(sideway);
                    }
                }
            }

            // Check for BlackholeRiser collisions
            if (affectsRiser)
            {
                foreach (BlackholeRiser riser in Scene.Tracker.GetEntities<BlackholeRiser>())
                {
                    if (CollideCheck(riser))
                    {
                        HandleRiserCollision(riser);
                    }
                }
            }
        }

        private void HandleBlackholeCollision(BlackholeFlareSideway sideway)
        {
            if (visualEffect)
            {
                EmitBlockParticles(sideway.Center);
            }

            switch (behavior)
            {
                case BlockBehavior.Stop:
                    // Push the sideway back slightly so it doesn't keep colliding
                    PushSidewayBack(sideway);
                    break;
                    
                case BlockBehavior.Reverse:
                    ReverseSideway(sideway);
                    break;
                    
                case BlockBehavior.Destroy:
                    DestroySideway(sideway);
                    break;
            }
        }

        private void HandleRiserCollision(BlackholeRiser riser)
        {
            if (visualEffect)
            {
                EmitBlockParticles(riser.Center);
            }

            switch (behavior)
            {
                case BlockBehavior.Stop:
                    // Stop the riser from rising further
                    StopRiser(riser);
                    break;
                    
                case BlockBehavior.Reverse:
                    ReverseRiser(riser);
                    break;
                    
                case BlockBehavior.Destroy:
                    DestroyRiser(riser);
                    break;
            }
        }

        private void PushSidewayBack(BlackholeFlareSideway sideway)
        {
            // Determine which side we're blocking and push back
            float pushDistance = 4f;
            if (sideway.Center.X < Center.X)
            {
                sideway.X = Left - sideway.Width - pushDistance;
            }
            else
            {
                sideway.X = Right + pushDistance;
            }
            
            Audio.Play("event:/game/general/wall_break_stone_small", sideway.Position);
        }

        private void ReverseSideway(BlackholeFlareSideway sideway)
        {
            // Use reflection to access and modify the private direction field
            var directionField = typeof(BlackholeFlareSideway).GetField("direction", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (directionField != null)
            {
                var currentDirection = (BlackholeFlareSideway.Directions)directionField.GetValue(sideway);
                var newDirection = currentDirection == BlackholeFlareSideway.Directions.Left 
                    ? BlackholeFlareSideway.Directions.Right 
                    : BlackholeFlareSideway.Directions.Left;
                directionField.SetValue(sideway, newDirection);
            }
            
            // Push it away from the blocker
            PushSidewayBack(sideway);
            
            Audio.Play("guid://{5a1e6a52-fa6a-44fb-b7ef-931a000b7c95}", sideway.Position);
        }

        private void DestroySideway(BlackholeFlareSideway sideway)
        {
            Level level = Scene as Level;
            if (level != null)
            {
                level.Shake(0.2f);
                
                // Big particle burst
                for (int i = 0; i < 20; i++)
                {
                    level.ParticlesFG.Emit(P_Block, sideway.Center, Calc.Random.NextFloat() * (float)Math.PI * 2f);
                }
            }
            
            Audio.Play("guid://{92f75010-4e90-428e-834c-1315ddbd9460}", sideway.Position);
            sideway.RemoveSelf();
        }

        private void StopRiser(BlackholeRiser riser)
        {
            // Use reflection to stop the rising
            var risingField = typeof(BlackholeRiser).GetField("rising", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (risingField != null)
            {
                risingField.SetValue(riser, false);
            }
            
            // Push it down slightly
            riser.Y = Bottom + 2f;
            
            Audio.Play("event:/game/general/wall_break_stone_small", riser.Position);
        }

        private void ReverseRiser(BlackholeRiser riser)
        {
            // Use reflection to reverse the rising state
            var risingField = typeof(BlackholeRiser).GetField("rising", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var loopingField = typeof(BlackholeRiser).GetField("looping", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (risingField != null)
            {
                bool isRising = (bool)risingField.GetValue(riser);
                // The riser will handle its own reversal if looping is enabled
                // For now, we just stop it
                risingField.SetValue(riser, false);
            }
            
            // Push it down
            riser.Y = Bottom + 4f;
            
            Audio.Play("guid://{5a1e6a52-fa6a-44fb-b7ef-931a000b7c95}", riser.Position);
        }

        private void DestroyRiser(BlackholeRiser riser)
        {
            Level level = Scene as Level;
            if (level != null)
            {
                level.Shake(0.3f);
                
                // Big particle burst
                for (int i = 0; i < 25; i++)
                {
                    level.ParticlesFG.Emit(P_Block, riser.Center, Calc.Random.NextFloat() * (float)Math.PI * 2f);
                }
            }
            
            Audio.Play("guid://{92f75010-4e90-428e-834c-1315ddbd9460}", riser.Position);
            riser.RemoveSelf();
        }

        private void EmitBlockParticles(Vector2 position)
        {
            Level level = Scene as Level;
            if (level == null) return;

            ParticleType coloredBlock = new ParticleType(P_Block)
            {
                Color = blockerColor,
                Color2 = Color.White
            };

            for (int i = 0; i < 5; i++)
            {
                level.ParticlesFG.Emit(coloredBlock, position, Calc.Random.NextFloat() * (float)Math.PI * 2f);
            }
        }

        public override void Render()
        {
            // Draw the blocker as a semi-transparent barrier
            float pulse = (float)Math.Sin(pulseTimer) * 0.15f + 0.85f;
            Color renderColor = blockerColor * 0.4f * pulse;
            
            // Main fill
            Draw.Rect(Position, width, height, renderColor);
            
            // Border
            Color borderColor = blockerColor * 0.8f * pulse;
            float borderWidth = 2f;
            
            Draw.Rect(Position, width, borderWidth, borderColor); // Top
            Draw.Rect(Position + new Vector2(0, height - borderWidth), width, borderWidth, borderColor); // Bottom
            Draw.Rect(Position, borderWidth, height, borderColor); // Left
            Draw.Rect(Position + new Vector2(width - borderWidth, 0), borderWidth, height, borderColor); // Right
            
            // Corner highlights
            float cornerSize = 4f;
            Color cornerColor = Color.White * 0.6f * pulse;
            Draw.Rect(Position, cornerSize, cornerSize, cornerColor);
            Draw.Rect(Position + new Vector2(width - cornerSize, 0), cornerSize, cornerSize, cornerColor);
            Draw.Rect(Position + new Vector2(0, height - cornerSize), cornerSize, cornerSize, cornerColor);
            Draw.Rect(Position + new Vector2(width - cornerSize, height - cornerSize), cornerSize, cornerSize, cornerColor);
        }
    }
}

