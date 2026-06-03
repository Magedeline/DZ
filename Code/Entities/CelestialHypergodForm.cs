namespace Celeste.Entities
{
    /// <summary>
    /// Celestial Hypergod form for Madeline and Badeline after death
    /// Features twin battle axe bonded between light and darkness
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/CelestialHypergodForm")]
    [Tracked]
    [HotReloadable]
    public class CelestialHypergodForm : Entity
    {
        private Sprite madelineSprite;
        private Sprite badelineSprite;
        private Sprite twinAxeSprite;
        
        private VertexLight lightAura;
        private VertexLight darkAura;
        private SineWave floatWave;
        private Wiggler powerWiggler;
        
        private bool isTransforming = false;
        private bool isActive = false;
        private float transformProgress = 0f;
        
        // Combat properties
        private List<Entity> activeProjectiles = new List<Entity>();
        private float attackCooldown = 0f;
        private const float ATTACK_COOLDOWN_TIME = 0.8f;
        
        public enum HypergodAttackType
        {
            TwinAxeSlash,
            LightDarkFusion,
            CelestialStorm,
            DualityBeam,
            HeavenlyJudgment
        }
        
        public CelestialHypergodForm(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = -1000000; // Render above everything
            Collider = new Hitbox(32f, 64f, -16f, -64f);
            
            setupSprites();
            setupEffects();
        }
        
        private void setupSprites()
        {
            // Madeline (light side)
            Add(madelineSprite = new Sprite(GFX.Game, "characters/player/"));
            madelineSprite.AddLoop("idle", "idle", 0.1f);
            madelineSprite.AddLoop("hypergod", "idle", 0.1f); // Placeholder
            madelineSprite.Play("idle");
            madelineSprite.Position = new Vector2(-12f, 0f);
            
            // Badeline (dark side)
            Add(badelineSprite = new Sprite(GFX.Game, "characters/badeline/"));
            badelineSprite.AddLoop("idle", "idle", 0.1f);
            badelineSprite.AddLoop("hypergod", "idle", 0.1f); // Placeholder
            badelineSprite.Play("idle");
            badelineSprite.Position = new Vector2(12f, 0f);
            
            // Twin battle axe (center)
            Add(twinAxeSprite = new Sprite(GFX.Game, "objects/"));
            twinAxeSprite.AddLoop("idle", "idle", 0.1f); // Placeholder
            twinAxeSprite.Play("idle");
            twinAxeSprite.Position = Vector2.Zero;
            twinAxeSprite.Visible = false; // Show only when active
        }
        
        private void setupEffects()
        {
            // Light aura (Madeline)
            Add(lightAura = new VertexLight(Color.LightBlue * 0.8f, 1f, 96, 128));
            lightAura.Position = new Vector2(-12f, 0f);
            
            // Dark aura (Badeline)
            Add(darkAura = new VertexLight(new Color(0.6f, 0.1f, 0.8f) * 0.8f, 1f, 96, 128));
            darkAura.Position = new Vector2(12f, 0f);
            
            // Float animation
            Add(floatWave = new SineWave(1.2f, 0f));
            floatWave.Randomize();
            
            // Power wiggler
            Add(powerWiggler = Wiggler.Create(1f, 4f));
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Start transformation sequence
            Add(new Coroutine(transformationSequence()));
        }
        
        private IEnumerator transformationSequence()
        {
            isTransforming = true;
            
            // Play transformation sound
            Audio.Play("event:/char/pusheen/kirby/celestial_transform", Position);
            
            // Fade in and grow
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 0.5f)
            {
                transformProgress = Ease.CubeOut(t);
                
                float scale = 0.5f + transformProgress * 0.5f;
                madelineSprite.Scale = Vector2.One * scale;
                badelineSprite.Scale = Vector2.One * scale;
                
                yield return null;
            }
            
            // Show twin axe
            twinAxeSprite.Visible = true;
            powerWiggler.Start();
            
            // Flash effect
            var level = Scene as Level;
            level?.Displacement.AddBurst(Position, 0.8f, 64f, 128f, 1f);
            
            Audio.Play("event:/char/pusheen/kirby/celestial_power_up", Position);
            
            yield return 0.5f;
            
            isTransforming = false;
            isActive = true;
        }
        
        public override void Update()
        {
            base.Update();
            
            if (!isActive) return;
            
            // Float animation
            float floatOffset = floatWave.Value * 4f;
            madelineSprite.Y = floatOffset;
            badelineSprite.Y = floatOffset;
            twinAxeSprite.Y = floatOffset;
            
            // Update attack cooldown
            if (attackCooldown > 0f)
            {
                attackCooldown -= Engine.DeltaTime;
            }
            
            // Update auras
            lightAura.Alpha = 0.6f + powerWiggler.Value * 0.4f;
            darkAura.Alpha = 0.6f + powerWiggler.Value * 0.4f;
        }
        
        public void ExecuteAttack(HypergodAttackType attackType)
        {
            if (!isActive || attackCooldown > 0f) return;
            
            switch (attackType)
            {
                case HypergodAttackType.TwinAxeSlash:
                    executeTwinAxeSlash();
                    break;
                case HypergodAttackType.LightDarkFusion:
                    executeLightDarkFusion();
                    break;
                case HypergodAttackType.CelestialStorm:
                    executeCelestialStorm();
                    break;
                case HypergodAttackType.DualityBeam:
                    executeDualityBeam();
                    break;
                case HypergodAttackType.HeavenlyJudgment:
                    executeHeavenlyJudgment();
                    break;
            }
            
            attackCooldown = ATTACK_COOLDOWN_TIME;
            powerWiggler.Start();
        }
        
        private void executeTwinAxeSlash()
        {
            Audio.Play("event:/game/general/sword_slash", Position);
            
            // Create slash projectile
            var level = Scene as Level;
            level?.Displacement.AddBurst(Position, 0.5f, 32f, 64f, 0.5f);
        }
        
        private void executeLightDarkFusion()
        {
            Audio.Play("event:/char/pusheen/kirby/celestial_fusion", Position);
            
            // Combine light and dark energies
            var level = Scene as Level;
            level?.ParticlesFG.Emit(global::Celeste.Player.P_DashA, 12, Position + new Vector2(-12f, 0f), Vector2.One * 4f);
            level?.ParticlesFG.Emit(global::Celeste.Player.P_DashB, 12, Position + new Vector2(12f, 0f), Vector2.One * 4f);
        }
        
        private void executeCelestialStorm()
        {
            Audio.Play("event:/char/pusheen/kirby/celestial_storm", Position);
            
            // Create storm of projectiles
            for (int i = 0; i < 8; i++)
            {
                float angle = (i / 8f) * MathHelper.TwoPi;
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                // Create projectile at position + direction * 32
            }
        }
        
        private void executeDualityBeam()
        {
            Audio.Play("event:/char/pusheen/kirby/celestial_beam", Position);
            
            // Fire dual beams
            lightAura.StartRadius = 256f;
            darkAura.StartRadius = 256f;
        }
        
        private void executeHeavenlyJudgment()
        {
            Audio.Play("event:/char/pusheen/kirby/celestial_judgment", Position);
            
            // Ultimate attack
            var level = Scene as Level;
            level?.Displacement.AddBurst(Position, 1.2f, 128f, 256f, 2f);
            level?.Shake(1f);
        }
        
        public override void Render()
        {
            if (isTransforming)
            {
                // Render with transformation effect
                Draw.Rect(Position.X - 64f, Position.Y - 64f, 128f, 128f, Color.White * (transformProgress * 0.3f));
            }
            
            base.Render();
        }
    }
}




