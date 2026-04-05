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
    
}




