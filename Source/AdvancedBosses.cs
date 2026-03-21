using MaggyHelper.Helpers;

namespace MaggyHelper.Entities
{
    /// <summary>
    /// Axis Terminator 2.0 Boss - Upgraded version with enhanced abilities
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/AxisTerminator2Boss")]
    [Tracked]
    public class AxisTerminator2Boss : BossActor
    {
        private int health = 800;
        private int maxHealth = 800;
        private bool isDefeated = false;
        private bool phase2Active = false;
        
        private Sprite robotSprite;
        private List<Sprite> weaponPods = new List<Sprite>();
        private VertexLight coreLight;
        private SoundSource mechanicalSfx;
        
        private enum AdvancedAttackType
        {
            OmnidirectionalBarrage,
            PlasmaCannon,
            ShieldDrone,
            QuantumDash,
            OrbitalStrike,
            SystemOverload
        }
        
        public AxisTerminator2Boss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "axis_terminator_2_boss", Vector2.One, 200f, true, true, 1f, new Hitbox(48f, 64f, -24f, -64f))
        {
            setupAdvancedVisuals();
        }
        
        private void setupAdvancedVisuals()
        {
            Add(robotSprite = new Sprite(GFX.Game, "characters/axis2/"));
            robotSprite.AddLoop("idle", "axis2_idle", 0.1f);
            robotSprite.AddLoop("attack", "axis2_attack", 0.06f);
            robotSprite.AddLoop("phase2", "axis2_phase2", 0.08f);
            robotSprite.Play("idle");
            robotSprite.CenterOrigin();
            
            // Create 4 weapon pods
            for (int i = 0; i < 4; i++)
            {
                var pod = new Sprite(GFX.Game, "characters/axis2/");
                pod.AddLoop("idle", "pod_idle", 0.1f);
                pod.CenterOrigin();
                Add(pod);
                weaponPods.Add(pod);
            }
            
            Add(coreLight = new VertexLight(Color.Cyan, 1f, 96, 128));
            Add(mechanicalSfx = new SoundSource());
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Audio.Play("event:/axis2_boot", Position);
            mechanicalSfx.Play("event:/axis2_mechanical_loop");
            
            Add(new Coroutine(advancedCombatLoop()));
        }
        
        private IEnumerator advancedCombatLoop()
        {
            while (!isDefeated && health > 0)
            {
                if (!phase2Active && health <= maxHealth * 0.5f)
                {
                    yield return enterPhase2();
                }
                
                var attack = (AdvancedAttackType)Calc.Random.Next(0, 6);
                yield return executeAdvancedAttack(attack);
                
                yield return phase2Active ? 1.5f : 2f;
            }
        }
        
        private IEnumerator enterPhase2()
        {
            phase2Active = true;
            
            Audio.Play("event:/axis2_phase2_transform", Position);
            robotSprite.Play("phase2");
            
            var level = Scene as Level;
            level?.Shake(1.5f);
            level?.Flash(Color.Cyan, true);
            
            coreLight.Color = Color.Red;
            coreLight.StartRadius = 128f;
            
            yield return 2f;
        }
        
        private IEnumerator executeAdvancedAttack(AdvancedAttackType attack)
        {
            robotSprite.Play("attack");
            
            switch (attack)
            {
                case AdvancedAttackType.OmnidirectionalBarrage:
                    yield return omnidirectionalBarrageAttack();
                    break;
                case AdvancedAttackType.PlasmaCannon:
                    yield return plasmaCannonAttack();
                    break;
                case AdvancedAttackType.ShieldDrone:
                    yield return shieldDroneAttack();
                    break;
                case AdvancedAttackType.QuantumDash:
                    yield return quantumDashAttack();
                    break;
                case AdvancedAttackType.OrbitalStrike:
                    yield return orbitalStrikeAttack();
                    break;
                case AdvancedAttackType.SystemOverload:
                    if (phase2Active)
                        yield return systemOverloadAttack();
                    break;
            }
            
            robotSprite.Play("idle");
        }
        
        private IEnumerator omnidirectionalBarrageAttack()
        {
            Audio.Play("event:/axis2_omni_barrage", Position);
            
            int waves = phase2Active ? 5 : 3;
            
            for (int w = 0; w < waves; w++)
            {
                // All weapon pods fire at once
                for (int i = 0; i < weaponPods.Count; i++)
                {
                    float angle = (i / (float)weaponPods.Count) * MathHelper.TwoPi;
                    Vector2 direction = new Vector2(
                        (float)System.Math.Cos(angle),
                        (float)System.Math.Sin(angle)
                    );
                    
                    // Create projectile from each pod
                }
                
                yield return 0.4f;
            }
        }
        
        private IEnumerator plasmaCannonAttack()
        {
            Audio.Play("event:/axis2_plasma_charge", Position);
            
            // Charge up
            for (int i = 0; i < 3; i++)
            {
                coreLight.Alpha = 1.5f;
                yield return 0.2f;
                coreLight.Alpha = 1f;
                yield return 0.2f;
            }
            
            Audio.Play("event:/axis2_plasma_fire", Position);
            
            var level = Scene as Level;
            level?.Shake(1f);
            
            // Fire massive plasma beam
            yield return 1.5f;
        }
        
        private IEnumerator shieldDroneAttack()
        {
            Audio.Play("event:/axis2_deploy_drones", Position);
            
            // Deploy 3 shield drones
            for (int i = 0; i < 3; i++)
            {
                float angle = (i / 3f) * MathHelper.TwoPi;
                Vector2 offset = new Vector2(
                    (float)System.Math.Cos(angle) * 80f,
                    (float)System.Math.Sin(angle) * 80f
                );
                
                // Create shield drone
            }
            
            yield return 3f;
        }
        
        private IEnumerator quantumDashAttack()
        {
            Audio.Play("event:/axis2_quantum_dash", Position);
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Teleport dash 3 times
            for (int i = 0; i < 3; i++)
            {
                if (player != null)
                {
                    Vector2 targetPos = player.Position + Calc.Random.Range(Vector2.One * -64f, Vector2.One * 64f);
                    
                    // Fade out
                    yield return 0.1f;
                    
                    Position = targetPos;
                    
                    // Fade in and attack
                    var level = Scene as Level;
                    level?.Displacement.AddBurst(Position, 0.5f, 48f, 96f, 0.3f);
                }
                
                yield return 0.3f;
            }
        }
        
        private IEnumerator orbitalStrikeAttack()
        {
            Audio.Play("event:/axis2_orbital_strike", Position);
            
            var level = Scene as Level;
            
            // Mark strike locations
            int strikes = phase2Active ? 6 : 3;
            
            for (int i = 0; i < strikes; i++)
            {
                Vector2 strikePos = Position + Calc.Random.Range(Vector2.One * -200f, Vector2.One * 200f);
                
                // Warning indicator
                yield return 0.5f;
                
                // Strike hits
                Audio.Play("event:/axis2_strike_impact", strikePos);
                level?.Shake(0.8f);
                level?.Displacement.AddBurst(strikePos, 1f, 64f, 128f, 0.5f);
            }
        }
        
        private IEnumerator systemOverloadAttack()
        {
            Audio.Play("event:/axis2_system_overload", Position);
            
            robotSprite.Play("phase2");
            coreLight.Color = Color.White;
            coreLight.Alpha = 2f;
            
            var level = Scene as Level;
            level?.Shake(2f);
            
            // Massive energy release
            for (int i = 0; i < 20; i++)
            {
                float angle = (i / 20f) * MathHelper.TwoPi;
                Vector2 direction = new Vector2(
                    (float)System.Math.Cos(angle),
                    (float)System.Math.Sin(angle)
                );
                
                // Create energy projectile
            }
            
            level?.Flash(Color.White, true);
            
            yield return 2f;
            
            coreLight.Color = Color.Red;
            coreLight.Alpha = 1f;
        }
        
        public override void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            health -= damage;
            Audio.Play("event:/axis2_damage", Position);
            
            var level = Scene as Level;
            level?.Shake(0.4f);
            
            if (health <= 0)
            {
                defeat();
            }
        }
        
        private void defeat()
        {
            isDefeated = true;
            Add(new Coroutine(defeatSequence()));
        }
        
        private IEnumerator defeatSequence()
        {
            Audio.Play("event:/axis2_critical_damage", Position);
            mechanicalSfx.Stop();
            
            var level = Scene as Level;
            
            // Critical explosions
            for (int i = 0; i < 8; i++)
            {
                Vector2 explosionPos = Position + Calc.Random.Range(Vector2.One * -32f, Vector2.One * 32f);
                Audio.Play("event:/axis2_explosion", explosionPos);
                level?.Displacement.AddBurst(explosionPos, 0.8f, 48f, 96f, 0.4f);
                level?.Shake(0.6f);
                
                yield return 0.25f;
            }
            
            // Final explosion
            Audio.Play("event:/axis2_final_explosion", Position);
            level?.Flash(Color.White, true);
            level?.Shake(2f);
            
            yield return 1f;
            
            level?.Session.SetFlag("axis_terminator_2_boss_defeated");
            
            RemoveSelf();
        }
        
        public override void Update()
        {
            base.Update();
            
            // Update weapon pod positions in orbit
            for (int i = 0; i < weaponPods.Count; i++)
            {
                float angle = (i / (float)weaponPods.Count) * MathHelper.TwoPi + Scene.TimeActive;
                float radius = 60f;
                
                weaponPods[i].Position = new Vector2(
                    (float)System.Math.Cos(angle) * radius,
                    (float)System.Math.Sin(angle) * radius - 32f
                );
            }
        }
    }
    
    /// <summary>
    /// King Titan Boss - Ultimate titan battle
    /// Massive end-game boss encounter
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/KingTitanBoss")]
    [Tracked]
    public class KingTitanBoss : BossActor
    {
        private int health = 2000;
        private int maxHealth = 2000;
        private bool isDefeated = false;
        private int currentPhase = 1;
        
        private Sprite titanSprite;
        private VertexLight titanGlow;
        private List<Sprite> armorPlates = new List<Sprite>();
        private SoundSource titanRoarSfx;
        
        private enum TitanAttackType
        {
            TitanicSlam,
            EarthquakeWave,
            MeteorRain,
            TitanRoar,
            ColossusStomp,
            RagingTempest,
            CataclysmicPillar
        }
        
        public KingTitanBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "king_titan_boss", new Vector2(1.5f, 1.5f), 300f, true, true, 1.2f, new Hitbox(80f, 120f, -40f, -120f))
        {
            setupTitanVisuals();
        }
        
        private void setupTitanVisuals()
        {
            Add(titanSprite = new Sprite(GFX.Game, "characters/kingtitan/"));
            titanSprite.AddLoop("idle", "titan_idle", 0.08f);
            titanSprite.AddLoop("attack", "titan_attack", 0.06f);
            titanSprite.AddLoop("phase2", "titan_phase2", 0.07f);
            titanSprite.AddLoop("phase3", "titan_phase3", 0.05f);
            titanSprite.Play("idle");
            titanSprite.CenterOrigin();
            
            Add(titanGlow = new VertexLight(Color.Orange, 1f, 192, 256));
            titanGlow.Position = new Vector2(0f, -60f);
            
            Add(titanRoarSfx = new SoundSource());
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            Add(new Coroutine(titanBattleSequence()));
        }
        
        private IEnumerator titanBattleSequence()
        {
            var level = Scene as Level;
            
            // Epic intro
            Audio.Play("event:/titan_awakening", Position);
            titanRoarSfx.Play("event:/titan_roar_loop");
            
            level?.Shake(2f);
            level?.Flash(Color.Orange, true);
            
            yield return 3f;
            
            // Start combat
            while (!isDefeated && health > 0)
            {
                // Check phase transitions
                if (currentPhase == 1 && health <= maxHealth * 0.66f)
                {
                    yield return enterPhase2();
                }
                else if (currentPhase == 2 && health <= maxHealth * 0.33f)
                {
                    yield return enterPhase3();
                }
                
                var attack = (TitanAttackType)Calc.Random.Next(0, 7);
                yield return executeTitanAttack(attack);
                
                float cooldown = currentPhase switch
                {
                    1 => 3f,
                    2 => 2.5f,
                    3 => 2f,
                    _ => 3f
                };
                
                yield return cooldown;
            }
        }
        
        private IEnumerator enterPhase2()
        {
            currentPhase = 2;
            
            Audio.Play("event:/titan_phase2_transform", Position);
            titanSprite.Play("phase2");
            
            var level = Scene as Level;
            level?.Shake(2.5f);
            level?.Flash(Color.Red, true);
            
            titanGlow.Color = Color.Red;
            titanGlow.StartRadius = 256f;
            
            yield return 2f;
        }
        
        private IEnumerator enterPhase3()
        {
            currentPhase = 3;
            
            Audio.Play("event:/titan_phase3_transform", Position);
            titanSprite.Play("phase3");
            
            var level = Scene as Level;
            level?.Shake(3f);
            level?.Flash(Color.Purple, true);
            
            titanGlow.Color = Color.Purple;
            titanGlow.StartRadius = 320f;
            
            yield return 2f;
        }
        
        private IEnumerator executeTitanAttack(TitanAttackType attack)
        {
            titanSprite.Play("attack");
            
            switch (attack)
            {
                case TitanAttackType.TitanicSlam:
                    yield return titanicSlamAttack();
                    break;
                case TitanAttackType.EarthquakeWave:
                    yield return earthquakeWaveAttack();
                    break;
                case TitanAttackType.MeteorRain:
                    yield return meteorRainAttack();
                    break;
                case TitanAttackType.TitanRoar:
                    yield return titanRoarAttack();
                    break;
                case TitanAttackType.ColossusStomp:
                    yield return colossusStompAttack();
                    break;
                case TitanAttackType.RagingTempest:
                    if (currentPhase >= 2)
                        yield return ragingTempestAttack();
                    break;
                case TitanAttackType.CataclysmicPillar:
                    if (currentPhase >= 3)
                        yield return cataclysmicPillarAttack();
                    break;
            }
            
            titanSprite.Play("idle");
        }
        
        private IEnumerator titanicSlamAttack()
        {
            Audio.Play("event:/titan_titanic_slam", Position);
            
            var level = Scene as Level;
            level?.Shake(3f);
            
            // Massive ground slam
            level?.Displacement.AddBurst(Position, 2f, 192f, 384f, 1.5f);
            
            yield return 2f;
        }
        
        private IEnumerator earthquakeWaveAttack()
        {
            Audio.Play("event:/titan_earthquake", Position);
            
            var level = Scene as Level;
            
            // Create earthquake waves spreading outward
            for (int i = 0; i < 5; i++)
            {
                level?.Shake(1.5f);
                yield return 0.3f;
            }
        }
        
        private IEnumerator meteorRainAttack()
        {
            Audio.Play("event:/titan_meteor_rain", Position);
            
            var level = Scene as Level;
            
            // Rain meteors from sky
            int meteorCount = currentPhase * 5;
            
            for (int i = 0; i < meteorCount; i++)
            {
                Vector2 meteorPos = Position + Calc.Random.Range(Vector2.One * -300f, Vector2.One * 300f);
                meteorPos.Y = level.Bounds.Top - 50f;
                
                // Create falling meteor
                
                yield return 0.2f;
            }
            
            yield return 1f;
        }
        
        private IEnumerator titanRoarAttack()
        {
            Audio.Play("event:/titan_roar_attack", Position);
            
            var level = Scene as Level;
            level?.Shake(2f);
            level?.Flash(Color.Orange, false);
            
            // Shockwave from roar
            level?.Displacement.AddBurst(Position, 1.5f, 256f, 512f, 1f);
            
            yield return 2f;
        }
        
        private IEnumerator colossusStompAttack()
        {
            Audio.Play("event:/titan_colossus_stomp", Position);
            
            var level = Scene as Level;
            
            // Powerful stomp
            for (int i = 0; i < 3; i++)
            {
                level?.Shake(2.5f);
                level?.Displacement.AddBurst(Position, 1.5f, 128f, 256f, 0.8f);
                
                yield return 0.5f;
            }
        }
        
        private IEnumerator ragingTempestAttack()
        {
            Audio.Play("event:/titan_raging_tempest", Position);
            
            var level = Scene as Level;
            
            // Create swirling tempest
            for (float t = 0f; t < 3f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.5f, 96f, 192f, 0.3f);
                
                yield return null;
            }
        }
        
        private IEnumerator cataclysmicPillarAttack()
        {
            Audio.Play("event:/titan_cataclysmic_pillar", Position);
            
            var level = Scene as Level;
            
            // Summon pillars of destruction
            for (int i = 0; i < 4; i++)
            {
                float angle = (i / 4f) * MathHelper.TwoPi;
                Vector2 pillarPos = Position + new Vector2(
                    (float)System.Math.Cos(angle) * 150f,
                    (float)System.Math.Sin(angle) * 150f
                );
                
                // Create pillar
                level?.Shake(1f);
                level?.Displacement.AddBurst(pillarPos, 1.2f, 96f, 192f, 0.8f);
                
                yield return 0.5f;
            }
        }
        
        public override void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            health -= damage;
            Audio.Play("event:/titan_damage", Position);
            
            var level = Scene as Level;
            level?.Shake(0.5f);
            
            if (health <= 0)
            {
                defeat();
            }
        }
        
        private void defeat()
        {
            isDefeated = true;
            Add(new Coroutine(defeatSequence()));
        }
        
        private IEnumerator defeatSequence()
        {
            Audio.Play("event:/titan_final_roar", Position);
            titanRoarSfx.Stop();
            
            var level = Scene as Level;
            
            // Epic death sequence
            level?.Shake(4f);
            level?.Flash(Color.White, true);
            
            for (int i = 0; i < 10; i++)
            {
                Vector2 explosionPos = Position + Calc.Random.Range(Vector2.One * -60f, Vector2.One * 60f);
                Audio.Play("event:/titan_explosion", explosionPos);
                level?.Displacement.AddBurst(explosionPos, 1.5f, 96f, 192f, 0.6f);
                
                yield return 0.3f;
            }
            
            // Titan falls
            Audio.Play("event:/titan_fall", Position);
            level?.Shake(5f);
            
            yield return 2f;
            
            level?.Session.SetFlag("king_titan_boss_defeated");
            
            // Trigger CS10_TitanBossBattle completion
            level?.Session.SetFlag("ch12_titan_boss_complete");
            
            RemoveSelf();
        }
    }
}




