namespace MaggyHelper.Entities
{
    [CustomEntity(ids: "MaggyHelper/BlackholeAngelBoss")]
    [Tracked]
    [HotReloadable]
    public class BlackholeAngelBoss : Boss
    {
        public enum AngelAttackType
        {
            VoidPulse,
            DimensionRift,
            AngelicBlast,
            BlackholeVortex,
            CosmicJudgment,
            FinalAnnihilation
        }
        
        private bool isInFinalPhase = false;
        private float voidEnergyLevel = 0f;
        private List<Entity> dimensionRifts = new List<Entity>();
        private float cosmicChargeTime = 0f;
        private bool isChargingFinalAttack = false;
        
        // Visual effects
        private VertexLight angelicLight;
        private SineWave voidPulse;
        private Wiggler cosmicWiggler;
        
        public BlackholeAngelBoss(EntityData data, Vector2 offset) : base(data, offset)
        {
            // Override base boss settings for final boss
            Tier = BossTier.Final;
            Gimmick = GimmickAbility.DimensionRift;
            BossType = "BlackholeAngel";
            
            // Final boss specific setup
            MaxHealth = 1500; // Override tier-based health for final boss
            Health = MaxHealth;
            Speed = 120f;
            arenaRadius = 300f; // Larger arena for final boss
            
            setupAngelEffects();
        }
        
        private void setupAngelEffects()
        {
            // Add ethereal light effect
            Add(angelicLight = new VertexLight(Color.White * 0.8f, 1f, 128, 192));
            
            // Add void pulsing effect
            Add(voidPulse = new SineWave(0.8f, 0f));
            voidPulse.Randomize();
            
            // Add cosmic wiggle effect
            Add(cosmicWiggler = Wiggler.Create(1.2f, 3f));
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Trigger dramatic entrance cutscene
            triggerAngelIntroSequence();
        }
        
        private void triggerAngelIntroSequence()
        {
            // Play dramatic entrance music
            Audio.SetMusic("event:/final_boss/blackhole_angel_theme");
            
            // Add screen effects
            var level = Scene as Level;
            level?.Displacement.AddBurst(Position, 0.8f, 64f, 128f, 2f);
            
            // Announce the final boss
            Audio.Play("event:/final_boss/angel_entrance", Position);
        }
        
        public override void Update()
        {
            base.Update();
            
            // Update void energy based on damage taken
            float healthPercent = (float)Health / MaxHealth;
            voidEnergyLevel = 1f - healthPercent; // More energy as health decreases
            
            // Check for final phase transition
            if (!isInFinalPhase && healthPercent <= 0.2f)
            {
                enterFinalPhase();
            }
            
            // Update visual effects
            updateAngelEffects();
        }
        
        private void updateAngelEffects()
        {
            // Pulsing light based on void energy
            if (angelicLight != null)
            {
                angelicLight.Alpha = 0.5f + voidEnergyLevel * 0.5f;
                angelicLight.Color = Color.Lerp(Color.White, Color.Purple, voidEnergyLevel);
            }
            
            // Cosmic wiggler based on charging state
            if (isChargingFinalAttack && cosmicWiggler != null)
            {
                cosmicWiggler.Start();
                sprite.Scale = Vector2.One * (1f + cosmicWiggler.Value * 0.3f);
            }
        }
        
        private void enterFinalPhase()
        {
            if (isInFinalPhase) return;
            
            isInFinalPhase = true;
            IsInvulnerable = true; // Temporarily invulnerable during transition
            
            // Dramatic phase transition
            Add(new Coroutine(finalPhaseTransition()));
        }
        
        private IEnumerator finalPhaseTransition()
        {
            // Stop all movement
            Speed = 0f;
            
            // Screen effects
            var level = Scene as Level;
            level?.Flash(Color.Purple);
            level?.Shake(2f);
            
            // Audio crescendo
            Audio.Play("event:/final_boss/final_phase_transition", Position);
            
            // Transformation time
            yield return 3f;
            
            // Restore movement and remove invulnerability
            Speed = 150f; // Faster in final phase
            IsInvulnerable = false;
            
            // Start final attack pattern
            Add(new Coroutine(finalAttackSequence()));
        }
        
        private IEnumerator finalAttackSequence()
        {
            while (isInFinalPhase && !IsDefeated)
            {
                // Cycle through increasingly powerful attacks
                yield return performVoidPulse();
                yield return 1f;
                
                yield return performDimensionRift();
                yield return 1.5f;
                
                yield return performAngelicBlast();
                yield return 1f;
                
                yield return performBlackholeVortex();
                yield return 2f;
                
                if (Health <= MaxHealth * 0.1f) // Last 10% health
                {
                    yield return performCosmicJudgment();
                    yield return 2f;
                }
                
                if (Health <= MaxHealth * 0.05f) // Last 5% health
                {
                    yield return chargeFinalAnnihilation();
                }
            }
        }
        
        private IEnumerator performVoidPulse()
        {
            sprite.Play("void_pulse");
            
            // Create expanding void pulse
            var level = Scene as Level;
            level?.Displacement.AddBurst(Position, 0.6f, 32f, 64f, 1f);
            
            // Damage player if close
            if (TargetPlayer != null && Vector2.Distance(Position, TargetPlayer.Position) < 80f)
            {
                // Apply void damage to player
                Audio.Play("event:/final_boss/void_damage", TargetPlayer.Position);
            }
            
            Audio.Play("event:/final_boss/void_pulse", Position);
            yield return 1.5f;
        }
        
        private IEnumerator performDimensionRift()
        {
            sprite.Play("dimension_rift");
            
            // Create multiple dimension rifts around the arena
            for (int i = 0; i < 3; i++)
            {
                Vector2 riftPos = ArenaCenter + Calc.AngleToVector(i * 120f, arenaRadius * 0.8f);
                var rift = createDimensionRift(riftPos);
                dimensionRifts.Add(rift);
                Scene.Add(rift);
                
                yield return 0.3f;
            }
            
            Audio.Play("event:/final_boss/dimension_rift", Position);
            yield return 2f;
            
            // Clean up rifts
            foreach (var rift in dimensionRifts)
            {
                rift.RemoveSelf();
            }
            dimensionRifts.Clear();
        }
        
        private Entity createDimensionRift(Vector2 position)
        {
            // Create a dimension rift entity that shoots projectiles
            var rift = new Entity(position);
            rift.Add(new Sprite(GFX.Game, "objects/dimension_rift/"));
            
            // Add rift behavior
            rift.Add(new Coroutine(riftBehavior(rift)));
            
            return rift;
        }
        
        private IEnumerator riftBehavior(Entity rift)
        {
            yield return 0.5f; // Formation time
            
            // Shoot projectiles from rift
            for (int i = 0; i < 5; i++)
            {
                if (TargetPlayer != null)
                {
                    Vector2 direction = (TargetPlayer.Position - rift.Position).SafeNormalize();
                    // Create rift projectile
                    // var projectile = new RiftProjectile(rift.Position, direction);
                    // Scene.Add(projectile);
                }
                yield return 0.4f;
            }
        }
        
        private IEnumerator performAngelicBlast()
        {
            sprite.Play("angelic_blast");
            
            // Charge up time
            cosmicChargeTime = 0f;
            while (cosmicChargeTime < 2f)
            {
                cosmicChargeTime += Engine.DeltaTime;
                // Charging visual effects
                yield return null;
            }
            
            // Release powerful blast
            if (TargetPlayer != null)
            {
                Vector2 direction = (TargetPlayer.Position - Position).SafeNormalize();
                // Create angelic blast beam
                createAngelicBlastBeam(direction);
            }
            
            Audio.Play("event:/final_boss/angelic_blast", Position);
            yield return 1f;
        }
        
        private void createAngelicBlastBeam(Vector2 direction)
        {
            // Create a powerful beam attack
            var level = Scene as Level;
            level?.Displacement.AddBurst(Position, 1f, 48f, 96f, 1.5f);
            
            // Check for player hit along beam path
            Vector2 beamEnd = Position + direction * 400f;
            // Implement beam collision detection here
        }
        
        private IEnumerator performBlackholeVortex()
        {
            sprite.Play("blackhole_vortex");
            
            // Create gravitational pull effect
            var level = Scene as Level;
            level?.Shake(1.5f);
            
            // Pull player towards boss
            if (TargetPlayer != null)
            {
                Vector2 pullDirection = (Position - TargetPlayer.Position).SafeNormalize();
                // Apply gravitational force to player
                // targetPlayer.Speed += pullDirection * 80f * Engine.DeltaTime;
            }
            
            // Create visual vortex effect
            for (int i = 0; i < 360; i += 30)
            {
                Vector2 particlePos = Position + Calc.AngleToVector(i, 60f);
                // Add swirling particles
            }
            
            Audio.Play("event:/final_boss/blackhole_vortex", Position);
            yield return 3f;
        }
        
        private IEnumerator performCosmicJudgment()
        {
            sprite.Play("cosmic_judgment");
            
            // Screen-wide attack warning
            var level = Scene as Level;
            level?.Flash(Color.Gold * 0.5f);
            
            // Charge time with audio cue
            Audio.Play("event:/final_boss/cosmic_judgment_charge", Position);
            yield return 3f;
            
            // Massive screen-clearing attack
            level?.Flash(Color.White);
            level?.Shake(3f);
            
            // Damage everything in arena
            Audio.Play("event:/final_boss/cosmic_judgment_blast", Position);
            yield return 2f;
        }
        
        private IEnumerator chargeFinalAnnihilation()
        {
            isChargingFinalAttack = true;
            sprite.Play("final_annihilation_charge");
            
            // Extended charge sequence
            Audio.Play("event:/final_boss/final_annihilation_charge", Position);
            
            var level = Scene as Level;
            level?.Flash(Color.Red * 0.7f);

            float chargeTime = 0f;
            while (isChargingFinalAttack && chargeTime < 10f)
            {
                chargeTime += Engine.DeltaTime;
                yield return null;
            }

            if (!isChargingFinalAttack)
            {
                yield break;
            }

            isChargingFinalAttack = false;
            performFinalAnnihilation();
        }
        
        private void performFinalAnnihilation()
        {
            // Game over attack - should not be reached if player is skilled
            var level = Scene as Level;
            level?.Flash(Color.Black);
            
            Audio.Play("event:/final_boss/final_annihilation", Position);
            
            // Force game over or dramatic defeat
            // This would integrate with the game's defeat system
        }
        
        public override void TakeDamage(int damage)
        {
            // Interrupt final annihilation if damaged
            if (isChargingFinalAttack)
            {
                isChargingFinalAttack = false;
            }
            
            base.TakeDamage(damage);
            
            // Special effects when damaged
            var level = Scene as Level;
            level?.Displacement.AddBurst(Position, 0.3f, 16f, 32f, 0.5f);
        }
        
        protected override void beginDefeated()
        {
            base.beginDefeated();
            
            // Epic defeat sequence
            Add(new Coroutine(angelDefeatSequence()));
        }
        
        private IEnumerator angelDefeatSequence()
        {
            var level = Scene as Level;
            
            // Stop music
            Audio.SetMusic(null);
            
            // Dramatic light effects
            level?.Flash(Color.Purple * 0.6f);
            yield return 1f;
            
            level?.Flash(Color.White);
            yield return 2f;
            
            // Victory music
            Audio.Play("event:/final_boss/victory_theme");
            
            // Trigger ending cutscene
            // This would connect to the game's ending system
            
            yield return 3f;
            
            // Remove self with dramatic effect
            RemoveSelf();
        }
    }
}



