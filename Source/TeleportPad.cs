namespace MaggyHelper.Entities
{
    /// <summary>
    /// Teleportation pad that transports player to a target location
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/TeleportPad")]
    public class TeleportPad : Entity
    {
        public enum PadType
        {
            Instant,
            Charged,
            OneWay,
            TwoWay
        }

        private PadType padType;
        private Vector2 targetPosition;
        private string targetRoomName;
        private bool isActive;
        private bool requiresActivation;
        private float chargeTime;
        private float currentCharge;
        private bool isCharging;
        private Sprite sprite;
        private VertexLight light;
        private SoundSource teleportSound;
        private TalkComponent talkComponent;
        private Level level;
        private Wiggler wiggler;

        public bool IsActive => isActive;
        public bool IsReady => isActive && (!requiresActivation || currentCharge >= chargeTime);

        public TeleportPad(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            string typeString = data.Attr(nameof(padType), "instant");
            if (!System.Enum.TryParse(typeString, true, out padType))
            {
                padType = PadType.Instant;
            }

            targetPosition = new Vector2(data.Float("targetX", Position.X), data.Float("targetY", Position.Y));
            targetRoomName = data.Attr("targetRoom", "");
            isActive = data.Bool(nameof(isActive), true);
            requiresActivation = data.Bool(nameof(requiresActivation), false);
            chargeTime = data.Float(nameof(chargeTime), 3f);

            currentCharge = 0f;
            isCharging = false;

            setupSprite();
            setupComponents();

            Depth = 1000;
            Collider = new Hitbox(24f, 8f, -12f, -4f);
        }

        private void setupSprite()
        {
            string spriteName = $"teleport_{padType.ToString().ToLower()}";

            if (GFX.SpriteBank.Has(spriteName))
            {
                sprite = GFX.SpriteBank.Create(spriteName);
            }
            else
            {
                sprite = new Sprite(GFX.Game, "objects/temple/portal/");
                sprite.AddLoop("idle", "portal", 0.1f);
                sprite.AddLoop("charging", "portal", 0.05f);
                sprite.AddLoop("ready", "portal", 0.02f);
                sprite.AddLoop("teleport", "portal", 0.01f);
            }

            Add(sprite);
            updateSpriteState();
        }

        private void setupComponents()
        {
            // Lighting system
            Color lightColor = getPadColor();
            light = new VertexLight(lightColor, 1f, 32, 16);
            Add(light);

            // Sound system
            teleportSound = new SoundSource();
            Add(teleportSound);

            // Talk component for manual activation
            if (requiresActivation)
            {
                talkComponent = new TalkComponent(
                    new Rectangle(-12, -8, 24, 16),
                    new Vector2(0f, -8f),
                    onActivate
                );
                Add(talkComponent);
            }

            // Wiggler for visual effects
            wiggler = Wiggler.Create(0.4f, 4f, null, false, false);
            Add(wiggler);
        }

        private Color getPadColor()
        {
            switch (padType)
            {
                case PadType.Instant: return Color.Cyan;
                case PadType.Charged: return Color.Blue;
                case PadType.OneWay: return Color.Green;
                case PadType.TwoWay: return Color.Purple;
                default: return Color.White;
            }
        }

        public override void Update()
        {
            base.Update();

            level = Scene as Level;

            if (isActive)
            {
                updateCharging();
                updateVisuals();
                checkPlayerCollision();
            }
        }

        private void updateCharging()
        {
            if (padType == PadType.Charged && isCharging)
            {
                currentCharge += Engine.DeltaTime;

                if (currentCharge >= chargeTime)
                {
                    onChargeComplete();
                }
            }
        }

        private void onChargeComplete()
        {
            isCharging = false;
            currentCharge = chargeTime;

            // Play charge complete sound
            Audio.Play("event:/game/05_mirror_temple/crystaltheo_pulse", Position);

            // Visual effect
            wiggler?.Start();
        }

        private void updateVisuals()
        {
            updateSpriteState();

            if (IsReady)
            {
                light.Alpha = 0.9f + (float)System.Math.Sin(Scene.TimeActive * 8f) * 0.1f;
            }
            else if (isCharging)
            {
                float chargeProgress = currentCharge / chargeTime;
                light.Alpha = 0.3f + chargeProgress * 0.6f;
            }
            else
            {
                light.Alpha = 0.2f;
            }

            // Scale with wiggler
            if (wiggler != null)
            {
                sprite.Scale = Vector2.One * (1f + wiggler.Value * 0.1f);
            }
        }

        private void updateSpriteState()
        {
            if (!isActive)
            {
                sprite?.Play("idle");
                sprite.Color = Color.Gray;
            }
            else if (IsReady)
            {
                sprite?.Play("ready");
                sprite.Color = getPadColor();
            }
            else if (isCharging)
            {
                sprite?.Play("charging");
                sprite.Color = getPadColor() * (0.5f + currentCharge / chargeTime * 0.5f);
            }
            else
            {
                sprite?.Play("idle");
                sprite.Color = getPadColor() * 0.6f;
            }
        }

        private void checkPlayerCollision()
        {
            if (!IsReady) return;

            global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && CollideCheck(player))
            {
                if (!requiresActivation || padType == PadType.Instant)
                {
                    teleportPlayer(player);
                }
            }
        }

        private void onActivate(global::Celeste.Player player)
        {
            if (requiresActivation && IsReady)
            {
                teleportPlayer(player);
            }
            else if (padType == PadType.Charged && !isCharging && currentCharge < chargeTime)
            {
                startCharging();
            }
        }

        private void startCharging()
        {
            isCharging = true;
            currentCharge = 0f;

            // Play charging sound
            teleportSound?.Play("event:/game/05_mirror_temple/platform_activate", "charge", 0f);
        }

        private void teleportPlayer(global::Celeste.Player player)
        {
            if (player == null || level == null) return;

            // Play teleport sound
            Audio.Play("event:/game/06_reflection/badeline_disappear", Position);

            // Visual effect at departure
            createTeleportEffect(Position);

            // Perform teleportation
            if (!string.IsNullOrEmpty(targetRoomName))
            {
                // Teleport to different room
                teleportToRoom(player);
            }
            else
            {
                // Teleport within same room
                teleportToPosition(player);
            }

            // Reset charge if needed
            if (padType == PadType.Charged)
            {
                currentCharge = 0f;
                isCharging = false;
            }
        }

        private void teleportToPosition(global::Celeste.Player player)
        {
            player.Position = targetPosition;

            // Visual effect at arrival
            createTeleportEffect(targetPosition);

            // Play arrival sound
            Audio.Play("event:/game/06_reflection/badeline_reappear", targetPosition);
        }

        private void teleportToRoom(global::Celeste.Player player)
        {
            // This would need to be implemented based on the level system
            // For now, just teleport to position
            teleportToPosition(player);
        }

        private void createTeleportEffect(Vector2 position)
        {
            if (level != null)
            {
                Color effectColor = getPadColor();

                // Create particle spiral effect
                for (int i = 0; i < 16; i++)
                {
                    float angle = i * 0.393f + Scene.TimeActive * 2f;
                    Vector2 direction = Calc.AngleToVector(angle, 20f);
                    Vector2 particlePos = position + direction;

                    level.ParticlesBG.Emit(ParticleTypes.Dust, particlePos, effectColor);
                }
            }
        }

        public void SetActive(bool active)
        {
            isActive = active;

            if (!active)
            {
                isCharging = false;
                currentCharge = 0f;
                teleportSound?.Stop();
            }
        }

        public void SetTarget(Vector2 newTarget, string newRoom = "")
        {
            targetPosition = newTarget;
            targetRoomName = newRoom;
        }

        public void ResetCharge()
        {
            currentCharge = 0f;
            isCharging = false;
        }

        public override void Render()
        {
            base.Render();

            if (isActive && padType == PadType.Charged && (isCharging || currentCharge > 0f))
            {
                // Render charge indicator
                Vector2 barPos = Position + new Vector2(-10f, -16f);
                float progress = currentCharge / chargeTime;

                Draw.Rect(barPos, 20f, 2f, Color.Black);
                Draw.Rect(barPos, 20f * progress, 2f, getPadColor());
            }
        }
    }
}




