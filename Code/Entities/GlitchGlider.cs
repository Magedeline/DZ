namespace Celeste.Entities
{
    /// <summary>
    /// Glitch Glider that allows throwing, dashing, and teleporting through barriers
    /// Limited to 5 teleport dashes per pickup
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/GlitchGlider")]
    [Tracked]
    [HotReloadable]
    public class GlitchGlider : Entity
    {
        public enum GliderState
        {
            Available,    // Ready to be picked up
            Carried,      // Player is carrying it
            Thrown,       // Glider is in flight
            Teleporting,  // Player is teleporting to glider
            Cooldown,     // Recovering after teleport
            Depleted      // No more uses remaining
        }

        private Sprite sprite;
        private VertexLight light;
        private SoundSource gliderSfx;
        private Level level;
        
        // State management
        private GliderState currentState;
        private global::Celeste.Player carriedBy;
        private int remainingUses;
        private const int MAX_USES = 5;
        private float throwSpeed = 200f;
        private Vector2 throwDirection;
        private Vector2 lastValidPosition;
        
        // Teleportation
        private bool canTeleport = false;
        private float teleportRange = 300f;
        private List<Entity> barriersPierced = new List<Entity>();
        
        // Visual effects
        private Color glitchColor1;
        private Color glitchColor2;
        private float glitchIntensity = 0f;
        private Vector2[] glitchParticles;
        private int particleCount = 16;
        
        // Cooldown and timing
        private float cooldownTimer = 0f;
        private const float COOLDOWN_TIME = 1f;
        private float throwCooldown = 0f;
        private new Vector2 Position;
        private const float THROW_COOLDOWN = 0.3f;

        public bool CanBePickedUp => currentState == GliderState.Available && remainingUses > 0;
        public bool CanBeTeleportedTo => currentState == GliderState.Thrown && canTeleport && remainingUses > 0;
        public int RemainingUses => remainingUses;

        public GlitchGlider(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            remainingUses = data.Int("maxUses", MAX_USES);
            throwSpeed = data.Float("throwSpeed", 200f);
            teleportRange = data.Float("teleportRange", 300f);
            
            // Glitch colors
            string color1Hex = data.Attr("glitchColor1", "FF00FF");
            string color2Hex = data.Attr("glitchColor2", "00FFFF");
            
            TryParseColor(color1Hex, out glitchColor1);
            TryParseColor(color2Hex, out glitchColor2);

            currentState = GliderState.Available;
            Depth = -100;
            Collider = new Hitbox(12f, 8f, -6f, -4f);
            
            SetupComponents();
            InitializeGlitchParticles();
        }

        private void SetupComponents()
        {
            // Sprite setup
            sprite = new Sprite(GFX.Game, "objects/Ingeste/glitchGlider/");
            sprite.AddLoop("available", "available", 0.1f);
            sprite.AddLoop("carried", "carried", 0.08f);
            sprite.AddLoop("thrown", "thrown", 0.05f);
            sprite.AddLoop("teleporting", "teleporting", 0.02f);
            sprite.AddLoop("depleted", "depleted", 0.15f);
            sprite.AddLoop("glitch", "glitch", 0.01f);
            sprite.Play("available");
            Add(sprite);

            // Lighting
            light = new VertexLight(glitchColor1, 0.8f, 32, 24);
            Add(light);

            // Sound
            gliderSfx = new SoundSource();
            Add(gliderSfx);
        }

        private void InitializeGlitchParticles()
        {
            glitchParticles = new Vector2[particleCount];
            for (int i = 0; i < particleCount; i++)
            {
                ResetGlitchParticle(i);
            }
        }

        private void ResetGlitchParticle(int index)
        {
            float angle = (index / (float)particleCount) * 360f * Calc.DegToRad;
            float distance = Calc.Random.Range(8f, 16f);
            glitchParticles[index] = Calc.AngleToVector(angle, distance);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();

            HandleCooldowns();
            
            switch (currentState)
            {
                case GliderState.Available:
                    UpdateAvailableState();
                    break;
                case GliderState.Carried:
                    UpdateCarriedState();
                    break;
                case GliderState.Thrown:
                    UpdateThrownState();
                    break;
                case GliderState.Teleporting:
                    UpdateTeleportingState();
                    break;
                case GliderState.Cooldown:
                    UpdateCooldownState();
                    break;
                case GliderState.Depleted:
                    UpdateDepletedState();
                    break;
            }

            UpdateVisualEffects();
        }

        private void HandleCooldowns()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Engine.DeltaTime;
            }
            
            if (throwCooldown > 0f)
            {
                throwCooldown -= Engine.DeltaTime;
            }
        }

        private void UpdateAvailableState()
        {
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && CollideCheck(player) && CanBePickedUp)
            {
                PickUpGlider(player);
            }
        }

        private void UpdateCarriedState()
        {
            if (carriedBy == null)
            {
                DropGlider();
                return;
            }

            // Follow player
            Position = carriedBy.Position + new Vector2(0f, -12f);

            // Check for throw input
            if (Input.Grab.Pressed && throwCooldown <= 0f)
            {
                ThrowGlider();
            }
        }

        private void UpdateThrownState()
        {
            // Move the glider
            Position += throwDirection * throwSpeed * Engine.DeltaTime;

            // Check for solid collisions (but allow passing through barriers)
            if (CheckSolidCollision())
            {
                StopGlider();
                return;
            }

            // Check for barrier piercing
            CheckBarrierPiercing();

            // Update last valid position
            lastValidPosition = Position;

            // Check for teleport input from player
            if (carriedBy != null && Input.Dash.Pressed && CanBeTeleportedTo)
            {
                float distanceToPlayer = Vector2.Distance(Position, carriedBy.Position);
                if (distanceToPlayer <= teleportRange)
                {
                    Add(new Coroutine(TeleportPlayerToGlider()));
                }
            }
        }

        private void UpdateTeleportingState()
        {
            // Handled by coroutine
        }

        private void UpdateCooldownState()
        {
            if (cooldownTimer <= 0f)
            {
                if (remainingUses > 0)
                {
                    currentState = GliderState.Available;
                    sprite.Play("available");
                }
                else
                {
                    currentState = GliderState.Depleted;
                    sprite.Play("depleted");
                }
            }
        }

        private void UpdateDepletedState()
        {
            // Slowly fade and remove
            sprite.Color *= 0.995f;
            light.Alpha *= 0.995f;
            
            if (sprite.Color.A < 0.1f)
            {
                RemoveSelf();
            }
        }

        private void PickUpGlider(global::Celeste.Player player)
        {
            carriedBy = player;
            currentState = GliderState.Carried;
            sprite.Play("carried");
            
            gliderSfx.Play("event:/game/general/thing_booped");
            CreatePickupEffect();
        }

        private void ThrowGlider()
        {
            if (carriedBy == null) return;

            // Determine throw direction
            throwDirection = GetThrowDirection();
            
            currentState = GliderState.Thrown;
            canTeleport = true;
            throwCooldown = THROW_COOLDOWN;
            
            sprite.Play("thrown");
            gliderSfx.Play("event:/game/06_reflection/badeline_disappear");
            
            CreateThrowEffect();
            
            // Don't null carriedBy yet - needed for teleportation
        }

        private Vector2 GetThrowDirection()
        {
            // Use analog stick direction if available
            Vector2 aimDirection = Input.Feather;
            
            if (aimDirection.LengthSquared() > 0.25f)
            {
                return aimDirection.SafeNormalize();
            }
            
            // Fall back to movement direction
            if (Math.Abs(Input.MoveX.Value) > 0.1f)
            {
                return new Vector2(Math.Sign(Input.MoveX.Value), -0.3f).SafeNormalize();
            }
            
            // Default forward direction
            float facing = carriedBy != null && carriedBy.Facing == Facings.Left ? -1f : 1f;
            return new Vector2(facing, -0.3f).SafeNormalize();
        }

        private bool CheckSolidCollision()
        {
            // Check for collision with solid entities (not barriers)
            foreach (var solid in Scene.Tracker.GetEntities<Solid>())
            {
                if (CollideCheck(solid) && !IsBarrier(solid))
                {
                    return true;
                }
            }
            
            // Check level boundaries
            return X < level.Bounds.Left + 8 || X > level.Bounds.Right - 8 ||
                   Y < level.Bounds.Top + 8 || Y > level.Bounds.Bottom - 8;
        }

        private void CheckBarrierPiercing()
        {
            // Only check if we have a scene
            if (Scene == null) return;

            // Check for barriers that can be pierced (like lightning barriers)
            // We use the tracker for common barrier types to improve performance.
            // If the entity is not a common type, we still check it but only if it's an Actor or Solid.
            
            // 1. Check tracked Lightning
            foreach (Entity entity in Scene.Tracker.GetEntities<Lightning>())
            {
                if (CollideCheck(entity) && !barriersPierced.Contains(entity))
                {
                    PierceBarrier(entity);
                }
            }
            
            // 2. Check tracked SeekerBarrier
            foreach (Entity entity in Scene.Tracker.GetEntities<SeekerBarrier>())
            {
                if (CollideCheck(entity) && !barriersPierced.Contains(entity))
                {
                    PierceBarrier(entity);
                }
            }

            // 3. Fallback for other potential barriers (only check Solids as most barriers are Solids)
            foreach (Solid solid in Scene.Tracker.GetEntities<Solid>())
            {
                if (CollideCheck(solid) && IsBarrier(solid) && !barriersPierced.Contains(solid))
                {
                    PierceBarrier(solid);
                }
            }
        }

        private static readonly Dictionary<Type, bool> IsBarrierCache = new();

        private bool IsBarrier(Entity entity)
        {
            if (entity == null) return false;
            Type type = entity.GetType();
            if (IsBarrierCache.TryGetValue(type, out bool result)) return result;

            string name = type.Name;
            result = name.Contains("Lightning") || name.Contains("Barrier") || name.Contains("EnergyBarrier");
            IsBarrierCache[type] = result;
            return result;
        }

        private void PierceBarrier(Entity barrier)
        {
            barriersPierced.Add(barrier);
            CreateBarrierPierceEffect(Position);
            
            gliderSfx.Play("event:/game/05_mirror_temple/crystaltheo_hit_side");
        }

        private void StopGlider()
        {
            currentState = GliderState.Available;
            canTeleport = false;
            throwDirection = Vector2.Zero;
            sprite.Play("available");
            
            barriersPierced.Clear();
        }

        private IEnumerator TeleportPlayerToGlider()
        {
            if (carriedBy == null || remainingUses <= 0) yield break;

            currentState = GliderState.Teleporting;
            remainingUses--;
            sprite.Play("teleporting");

            // Store player position
            Vector2 playerStartPos = carriedBy.Position;
            Vector2 gliderPos = Position;

            // Create teleport effects
            CreateTeleportEffect(playerStartPos, true);
            CreateTeleportEffect(gliderPos, false);

            // Play teleport sound
            gliderSfx.Play("event:/game/06_reflection/badeline_reappear");

            // Flash effect
            level?.Flash(Color.White, true);

            // Teleport player
            carriedBy.Position = gliderPos;
            
            // Restore dash if consumed
            if (level?.Session?.Inventory != null && level.Session.Inventory.Dashes == 0)
            {
                level.Session.Inventory.Dashes = 1;
            }

            yield return 0.2f;

            // Start cooldown
            currentState = GliderState.Cooldown;
            cooldownTimer = COOLDOWN_TIME;
            canTeleport = false;
            carriedBy = null;
            barriersPierced.Clear();
            
            CreateRecoveryEffect();
        }

        private void DropGlider()
        {
            carriedBy = null;
            currentState = GliderState.Available;
            canTeleport = false;
            sprite.Play("available");
        }

        private void CreatePickupEffect()
        {
            if (level == null) return;
            
            for (int i = 0; i < 8; i++)
            {
                Vector2 direction = Calc.AngleToVector(i * 45f * Calc.DegToRad, 20f);
                SceneAs<Level>().Particles.Emit(ParticleTypes.Dust, Position, direction.Angle());
            }
        }

        private void CreateThrowEffect()
        {
            if (level == null) return;
            
            Vector2 effectPos = Position - throwDirection * 16f;
            for (int i = 0; i < 6; i++)
            {
                Vector2 particleDir = throwDirection + new Vector2(Calc.Random.Range(-0.3f, 0.3f), Calc.Random.Range(-0.3f, 0.3f));
                SceneAs<Level>().Particles.Emit(ParticleTypes.Dust, effectPos, particleDir.Angle());
            }
        }

        private void CreateBarrierPierceEffect(Vector2 position)
        {
            if (level == null) return;
            
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f * Calc.DegToRad;
                Vector2 direction = Calc.AngleToVector(angle, 40f);
                SceneAs<Level>().Particles.Emit(ParticleTypes.Dust, position, direction.Angle());
            }
        }

        private void CreateTeleportEffect(Vector2 position, bool isStart)
        {
            if (level == null) return;
            
            for (int i = 0; i < 20; i++)
            {
                Vector2 direction = Calc.AngleToVector(Calc.Random.NextFloat() * 360f * Calc.DegToRad, 
                                                     Calc.Random.Range(20f, 60f));
                SceneAs<Level>().Particles.Emit(ParticleTypes.Dust, position, direction.Angle());
            }
        }

        private void CreateRecoveryEffect()
        {
            if (level == null) return;
            
            for (int i = 0; i < 16; i++)
            {
                Vector2 direction = Calc.AngleToVector(i * 22.5f * Calc.DegToRad, 25f);
                SceneAs<Level>().Particles.Emit(ParticleTypes.Dust, Position, direction.Angle());
            }
        }

        private void UpdateVisualEffects()
        {
            // Glitch intensity based on state
            switch (currentState)
            {
                case GliderState.Available:
                    glitchIntensity = 0.3f;
                    break;
                case GliderState.Carried:
                    glitchIntensity = 0.5f;
                    break;
                case GliderState.Thrown:
                    glitchIntensity = 0.8f;
                    break;
                case GliderState.Teleporting:
                    glitchIntensity = 1.5f;
                    break;
                case GliderState.Cooldown:
                    glitchIntensity = 0.2f;
                    break;
                case GliderState.Depleted:
                    glitchIntensity = 0.1f;
                    break;
            }

            // Update light
            Color currentColor = Color.Lerp(glitchColor1, glitchColor2, 
                                          (float)Math.Sin(Scene.TimeActive * 8f) * 0.5f + 0.5f);
            light.Color = currentColor;
            light.Alpha = 0.6f + glitchIntensity * 0.4f;

            // Sprite color glitching
            if (glitchIntensity > 0.5f && Scene.OnInterval(0.05f))
            {
                sprite.Color = Calc.Random.Choose(glitchColor1, glitchColor2, Color.White);
            }
            else
            {
                sprite.Color = Color.White;
            }
        }

        public override void Render()
        {
            base.Render();

            // Render glitch particles
            if (glitchIntensity > 0.3f)
            {
                RenderGlitchField();
            }

            // Render usage indicator
            RenderUsageIndicator();

            // Debug teleport range
            if (Engine.Commands.Open && currentState == GliderState.Thrown && carriedBy != null)
            {
                float distanceToPlayer = Vector2.Distance(Position, carriedBy.Position);
                Color rangeColor = distanceToPlayer <= teleportRange ? Color.Green : Color.Red;
                Draw.Line(Position, carriedBy.Position, rangeColor, 2);
                Draw.Circle(Position, teleportRange, Color.Yellow, 2);
            }
        }

        private void RenderGlitchField()
        {
            for (int i = 0; i < particleCount; i++)
            {
                float intensity = glitchIntensity * (0.5f + (float)Math.Sin(Scene.TimeActive * 10f + i) * 0.5f);
                Color particleColor = Color.Lerp(glitchColor1, glitchColor2, Calc.Random.NextFloat()) * intensity;
                
                Vector2 glitchOffset = new Vector2(
                    Calc.Random.Range(-2f, 2f) * intensity,
                    Calc.Random.Range(-2f, 2f) * intensity
                );
                
                Vector2 particlePos = Position + glitchParticles[i] + glitchOffset;
                Draw.Point(particlePos, particleColor);
            }
        }

        private void RenderUsageIndicator()
        {
            if (remainingUses <= 0) return;

            Vector2 indicatorPos = Position + new Vector2(-8f, -16f);
            float spacing = 3f;

            for (int i = 0; i < MAX_USES; i++)
            {
                Vector2 dotPos = indicatorPos + new Vector2(i * spacing, 0f);
                Color dotColor = i < remainingUses ? glitchColor1 : Color.Gray * 0.3f;
                
                Draw.Point(dotPos, dotColor);
            }
        }

        private static bool TryParseColor(string hex, out Color color)
        {
            color = Color.White;
            if (string.IsNullOrEmpty(hex)) return false;
            
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);
            
            try
            {
                if (hex.Length == 6)
                {
                    int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                    color = new Color(r, g, b);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "GlitchGlider", $"Failed to parse color hex '{hex}': {ex.Message}");
            }
            return false;
        }
    }
}



