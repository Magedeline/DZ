using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// Titantis Boss - Colossal titan of destruction
    /// A massive entity that commands earth and stone
    /// Multi-phase boss with devastating area attacks
    /// Sprite path: characters/titantis/
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/TitantisBoss")]
    [Tracked]
    [HotReloadable]
    public class TitantisBoss : BossActor
    {
        #region Enums and Constants
        public enum BossPhase
        {
            Awakening,
            Phase1_Earthshaker,
            Phase2_Stoneform,
            Phase3_Cataclysm,
            Defeated
        }

        public enum AttackType
        {
            // Phase 1 Attacks
            EarthquakeSlam,
            BoulderToss,
            GroundSpike,
            Stomp,
            
            // Phase 2 Attacks
            StonePillar,
            RockSlide,
            SeismicWave,
            ArmorCrush,
            
            // Phase 3 Attacks
            CataclysmicStrike,
            MeteorShower,
            TectonicRift,
            WorldEnder
        }
        #endregion

        #region Properties
        private int health = 1500;
        private int maxHealth = 1500;
        private bool isDefeated = false;
        private BossPhase currentPhase = BossPhase.Awakening;
        
        private Sprite titanSprite;
        private Sprite armLeftSprite;
        private Sprite armRightSprite;
        private VertexLight coreGlow;
        private VertexLight eyeGlow;
        private SoundSource rumbleLoop;
        
        private List<Sprite> debrisSprites = new List<Sprite>();
        private float shakeIntensity = 0f;
        private bool isStoneForm = false;
        #endregion

        #region Constructors
        public TitantisBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "titantis_boss", new Vector2(2f, 2f), 400f, true, true, 1.5f, 
                   new Hitbox(96f, 160f, -48f, -160f))
        {
            health = data.Int("health", 1500);
            maxHealth = data.Int("maxHealth", 1500);
            SetupVisuals();
        }

        public TitantisBoss(Vector2 position) 
            : base(position, "titantis_boss", new Vector2(2f, 2f), 400f, true, true, 1.5f, 
                   new Hitbox(96f, 160f, -48f, -160f))
        {
            SetupVisuals();
        }
        #endregion

        #region Setup
        private void SetupVisuals()
        {
            // Main body sprite
            Add(titanSprite = new Sprite(GFX.Game, "characters/titantis/"));
            titanSprite.AddLoop("dormant", "titan_dormant", 0.15f);
            titanSprite.AddLoop("idle", "titan_idle", 0.1f);
            titanSprite.AddLoop("attack", "titan_attack", 0.06f);
            titanSprite.AddLoop("stoneform", "titan_stone", 0.12f);
            titanSprite.AddLoop("cataclysm", "titan_cataclysm", 0.05f);
            titanSprite.AddLoop("defeat", "titan_defeat", 0.2f);
            titanSprite.Play("dormant");
            titanSprite.CenterOrigin();
            
            // Left arm sprite
            Add(armLeftSprite = new Sprite(GFX.Game, "characters/titantis/"));
            armLeftSprite.AddLoop("idle", "arm_left_idle", 0.1f);
            armLeftSprite.AddLoop("slam", "arm_left_slam", 0.05f);
            armLeftSprite.CenterOrigin();
            armLeftSprite.Position = new Vector2(-60f, -80f);
            
            // Right arm sprite
            Add(armRightSprite = new Sprite(GFX.Game, "characters/titantis/"));
            armRightSprite.AddLoop("idle", "arm_right_idle", 0.1f);
            armRightSprite.AddLoop("slam", "arm_right_slam", 0.05f);
            armRightSprite.CenterOrigin();
            armRightSprite.Position = new Vector2(60f, -80f);
            
            // Core glow (chest)
            Add(coreGlow = new VertexLight(Color.Orange, 1.2f, 80, 128));
            coreGlow.Position = new Vector2(0f, -100f);
            
            // Eye glow
            Add(eyeGlow = new VertexLight(Color.Red, 0.8f, 32, 48));
            eyeGlow.Position = new Vector2(0f, -140f);
            
            // Rumble sound
            Add(rumbleLoop = new SoundSource());
        }
        #endregion

        #region Scene Management
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(BossRoutine()));
        }

        public override void Update()
        {
            base.Update();
            
            // Apply screen shake
            if (shakeIntensity > 0f)
            {
                var level = Scene as Level;
                level?.Shake(shakeIntensity * Engine.DeltaTime);
                shakeIntensity = Math.Max(0f, shakeIntensity - Engine.DeltaTime * 2f);
            }
            
            // Pulsing core glow
            coreGlow.Alpha = 1f + (float)Math.Sin(Scene.TimeActive * 2f) * 0.3f;
        }
        #endregion

        #region Main Boss Routine
        private IEnumerator BossRoutine()
        {
            // Wait for player to approach
            yield return AwakeningSequence();
            
            while (currentPhase != BossPhase.Defeated)
            {
                switch (currentPhase)
                {
                    case BossPhase.Phase1_Earthshaker:
                        yield return Phase1Loop();
                        break;
                    case BossPhase.Phase2_Stoneform:
                        yield return Phase2Loop();
                        break;
                    case BossPhase.Phase3_Cataclysm:
                        yield return Phase3Loop();
                        break;
                }
                
                // Check phase transitions
                CheckPhaseTransition();
            }
        }

        private IEnumerator AwakeningSequence()
        {
            yield return 1f;
            
            Audio.Play("event:/titantis_awakening", Position);
            titanSprite.Play("idle");
            rumbleLoop.Play("event:/titantis_rumble_loop");
            
            var level = Scene as Level;
            level?.Shake(2f);
            
            eyeGlow.Alpha = 0f;
            for (float t = 0; t < 2f; t += Engine.DeltaTime)
            {
                eyeGlow.Alpha = t / 2f;
                yield return null;
            }
            
            currentPhase = BossPhase.Phase1_Earthshaker;
        }

        private void CheckPhaseTransition()
        {
            float healthPercent = (float)health / maxHealth;
            
            if (healthPercent <= 0)
            {
                currentPhase = BossPhase.Defeated;
            }
            else if (healthPercent <= 0.3f && currentPhase != BossPhase.Phase3_Cataclysm)
            {
                currentPhase = BossPhase.Phase3_Cataclysm;
                Add(new Coroutine(EnterPhase3()));
            }
            else if (healthPercent <= 0.6f && currentPhase == BossPhase.Phase1_Earthshaker)
            {
                currentPhase = BossPhase.Phase2_Stoneform;
                Add(new Coroutine(EnterPhase2()));
            }
        }
        #endregion

        #region Phase Routines
        private IEnumerator Phase1Loop()
        {
            while (currentPhase == BossPhase.Phase1_Earthshaker && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(0, 4);
                yield return ExecuteAttack(attack);
                yield return 2.5f;
            }
        }

        private IEnumerator EnterPhase2()
        {
            Audio.Play("event:/titantis_phase2_transform", Position);
            titanSprite.Play("stoneform");
            isStoneForm = true;
            
            var level = Scene as Level;
            level?.Shake(2f);
            level?.Flash(Color.Brown * 0.5f, true);
            
            coreGlow.Color = Color.DarkOrange;
            
            yield return 2f;
            
            titanSprite.Play("idle");
        }

        private IEnumerator Phase2Loop()
        {
            while (currentPhase == BossPhase.Phase2_Stoneform && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(4, 8);
                yield return ExecuteAttack(attack);
                yield return 2f;
            }
        }

        private IEnumerator EnterPhase3()
        {
            Audio.Play("event:/titantis_phase3_cataclysm", Position);
            titanSprite.Play("cataclysm");
            isStoneForm = false;
            
            var level = Scene as Level;
            level?.Shake(3f);
            level?.Flash(Color.Red * 0.5f, true);
            
            coreGlow.Color = Color.Red;
            eyeGlow.Color = Color.DarkRed;
            eyeGlow.Alpha = 2f;
            
            yield return 3f;
        }

        private IEnumerator Phase3Loop()
        {
            while (currentPhase == BossPhase.Phase3_Cataclysm && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(8, 12);
                yield return ExecuteAttack(attack);
                yield return 1.5f;
            }
        }
        #endregion

        #region Attack Execution
        private IEnumerator ExecuteAttack(AttackType attack)
        {
            titanSprite.Play("attack");
            
            switch (attack)
            {
                // Phase 1 Attacks
                case AttackType.EarthquakeSlam:
                    yield return EarthquakeSlamAttack();
                    break;
                case AttackType.BoulderToss:
                    yield return BoulderTossAttack();
                    break;
                case AttackType.GroundSpike:
                    yield return GroundSpikeAttack();
                    break;
                case AttackType.Stomp:
                    yield return StompAttack();
                    break;
                    
                // Phase 2 Attacks
                case AttackType.StonePillar:
                    yield return StonePillarAttack();
                    break;
                case AttackType.RockSlide:
                    yield return RockSlideAttack();
                    break;
                case AttackType.SeismicWave:
                    yield return SeismicWaveAttack();
                    break;
                case AttackType.ArmorCrush:
                    yield return ArmorCrushAttack();
                    break;
                    
                // Phase 3 Attacks
                case AttackType.CataclysmicStrike:
                    yield return CataclysmicStrikeAttack();
                    break;
                case AttackType.MeteorShower:
                    yield return MeteorShowerAttack();
                    break;
                case AttackType.TectonicRift:
                    yield return TectonicRiftAttack();
                    break;
                case AttackType.WorldEnder:
                    yield return WorldEnderAttack();
                    break;
            }
            
            titanSprite.Play("idle");
        }
        #endregion

        #region Phase 1 Attacks
        private IEnumerator EarthquakeSlamAttack()
        {
            armLeftSprite.Play("slam");
            armRightSprite.Play("slam");
            Audio.Play("event:/titantis_earthquake_slam", Position);
            
            yield return 0.5f;
            
            var level = Scene as Level;
            level?.Shake(2f);
            shakeIntensity = 3f;
            
            level?.Displacement.AddBurst(Position, 1.5f, 64f, 256f, 0.8f);
            
            yield return 1f;
            
            armLeftSprite.Play("idle");
            armRightSprite.Play("idle");
        }

        private IEnumerator BoulderTossAttack()
        {
            Audio.Play("event:/titantis_boulder_toss", Position);
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            for (int i = 0; i < 3; i++)
            {
                armRightSprite.Play("slam");
                
                if (player != null)
                {
                    // Create boulder projectile toward player
                    Vector2 direction = (player.Position - Position).SafeNormalize();
                }
                
                yield return 0.5f;
                armRightSprite.Play("idle");
                yield return 0.3f;
            }
        }

        private IEnumerator GroundSpikeAttack()
        {
            Audio.Play("event:/titantis_ground_spike", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            for (int i = 0; i < 5; i++)
            {
                if (player != null)
                {
                    Vector2 spikePos = player.Position + new Vector2(Calc.Random.Range(-50f, 50f), 0f);
                    level?.Displacement.AddBurst(spikePos, 0.6f, 24f, 64f, 0.4f);
                }
                
                yield return 0.3f;
            }
        }

        private IEnumerator StompAttack()
        {
            Audio.Play("event:/titantis_stomp", Position);
            
            yield return 0.3f;
            
            var level = Scene as Level;
            level?.Shake(1.5f);
            level?.Displacement.AddBurst(Position + new Vector2(0f, 10f), 1f, 48f, 160f, 0.6f);
            
            yield return 0.5f;
        }
        #endregion

        #region Phase 2 Attacks
        private IEnumerator StonePillarAttack()
        {
            Audio.Play("event:/titantis_stone_pillar", Position);
            
            var level = Scene as Level;
            
            for (int i = 0; i < 6; i++)
            {
                float xOffset = -150f + (i * 60f);
                Vector2 pillarPos = Position + new Vector2(xOffset, 0f);
                
                level?.Displacement.AddBurst(pillarPos, 0.8f, 32f, 96f, 0.5f);
                Audio.Play("event:/titantis_pillar_rise", pillarPos);
                
                yield return 0.2f;
            }
        }

        private IEnumerator RockSlideAttack()
        {
            Audio.Play("event:/titantis_rock_slide", Position);
            
            var level = Scene as Level;
            
            for (int i = 0; i < 10; i++)
            {
                Vector2 rockPos = Position + new Vector2(Calc.Random.Range(-200f, 200f), -300f);
                // Create falling rock projectile
                level?.Displacement.AddBurst(rockPos, 0.4f, 16f, 48f, 0.3f);
                
                yield return 0.15f;
            }
            
            yield return 1f;
        }

        private IEnumerator SeismicWaveAttack()
        {
            Audio.Play("event:/titantis_seismic_wave", Position);
            
            var level = Scene as Level;
            shakeIntensity = 2f;
            
            for (int i = 0; i < 4; i++)
            {
                float radius = 50f + (i * 80f);
                level?.Displacement.AddBurst(Position, 1f, radius, radius + 60f, 0.6f);
                yield return 0.4f;
            }
        }

        private IEnumerator ArmorCrushAttack()
        {
            titanSprite.Play("stoneform");
            Audio.Play("event:/titantis_armor_crush", Position);
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * 300f;
            }
            
            yield return 0.8f;
            Speed = Vector2.Zero;
            
            var level = Scene as Level;
            level?.Shake(2f);
            level?.Displacement.AddBurst(Position, 1.2f, 64f, 192f, 0.7f);
            
            yield return 0.5f;
            titanSprite.Play("idle");
        }
        #endregion

        #region Phase 3 Attacks
        private IEnumerator CataclysmicStrikeAttack()
        {
            titanSprite.Play("cataclysm");
            Audio.Play("event:/titantis_cataclysmic_strike", Position);
            
            // Charge up
            coreGlow.Alpha = 3f;
            yield return 1f;
            
            var level = Scene as Level;
            level?.Shake(3f);
            level?.Flash(Color.Orange * 0.5f, true);
            level?.Displacement.AddBurst(Position, 2f, 96f, 320f, 1f);
            
            shakeIntensity = 5f;
            
            yield return 1.5f;
            coreGlow.Alpha = 1f;
        }

        private IEnumerator MeteorShowerAttack()
        {
            Audio.Play("event:/titantis_meteor_shower", Position);
            
            var level = Scene as Level;
            
            for (int i = 0; i < 15; i++)
            {
                Vector2 meteorPos = Position + new Vector2(Calc.Random.Range(-300f, 300f), -400f);
                Vector2 targetPos = Position + new Vector2(Calc.Random.Range(-250f, 250f), 50f);
                
                // Create meteor projectile
                level?.Displacement.AddBurst(targetPos, 0.8f, 32f, 80f, 0.5f);
                
                yield return 0.1f;
            }
            
            level?.Shake(2f);
            yield return 0.5f;
        }

        private IEnumerator TectonicRiftAttack()
        {
            Audio.Play("event:/titantis_tectonic_rift", Position);
            
            armLeftSprite.Play("slam");
            armRightSprite.Play("slam");
            
            yield return 0.5f;
            
            var level = Scene as Level;
            level?.Shake(3f);
            shakeIntensity = 4f;
            
            // Create rift line across arena
            for (int i = -5; i <= 5; i++)
            {
                Vector2 riftPos = Position + new Vector2(i * 50f, 0f);
                level?.Displacement.AddBurst(riftPos, 1f, 24f, 80f, 0.6f);
            }
            
            yield return 1.5f;
            
            armLeftSprite.Play("idle");
            armRightSprite.Play("idle");
        }

        private IEnumerator WorldEnderAttack()
        {
            titanSprite.Play("cataclysm");
            Audio.Play("event:/titantis_world_ender", Position);
            
            var level = Scene as Level;
            
            // Massive charge up
            for (float t = 0; t < 2f; t += Engine.DeltaTime)
            {
                coreGlow.Alpha = 1f + t * 2f;
                eyeGlow.Alpha = 1f + t * 1.5f;
                level?.Shake(t);
                yield return null;
            }
            
            // Ultimate attack
            level?.Flash(Color.White, true);
            level?.Shake(5f);
            shakeIntensity = 8f;
            
            for (int i = 0; i < 8; i++)
            {
                level?.Displacement.AddBurst(Position, 2f, i * 50f, i * 50f + 100f, 0.8f);
                yield return 0.1f;
            }
            
            yield return 2f;
            
            coreGlow.Alpha = 1f;
            eyeGlow.Alpha = 1f;
        }
        #endregion

        #region Damage and Defeat
        public override void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            // Stone form reduces damage
            if (isStoneForm)
            {
                damage = damage / 2;
                Audio.Play("event:/titantis_stone_block", Position);
            }
            else
            {
                Audio.Play("event:/titantis_damage", Position);
            }
            
            health -= damage;
            
            var level = Scene as Level;
            level?.Shake(0.5f);
            
            if (health <= 0)
            {
                Defeat();
            }
        }

        private void Defeat()
        {
            isDefeated = true;
            currentPhase = BossPhase.Defeated;
            Add(new Coroutine(DefeatSequence()));
        }

        private IEnumerator DefeatSequence()
        {
            rumbleLoop.Stop();
            titanSprite.Play("defeat");
            Audio.Play("event:/titantis_defeat", Position);
            
            var level = Scene as Level;
            
            // Crumbling sequence
            for (int i = 0; i < 10; i++)
            {
                Vector2 crumblePos = Position + Calc.Random.Range(Vector2.One * -60f, Vector2.One * 60f);
                Audio.Play("event:/titantis_crumble", crumblePos);
                level?.Displacement.AddBurst(crumblePos, 0.8f, 32f, 80f, 0.5f);
                level?.Shake(0.8f);
                
                yield return 0.3f;
            }
            
            // Final collapse
            Audio.Play("event:/titantis_collapse", Position);
            level?.Flash(Color.Brown * 0.6f, true);
            level?.Shake(3f);
            
            yield return 2f;
            
            level?.Session.SetFlag("titantis_boss_defeated");
            RemoveSelf();
        }
        #endregion

        #region Cleanup
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            rumbleLoop?.Stop();
        }
        #endregion
    }
}
