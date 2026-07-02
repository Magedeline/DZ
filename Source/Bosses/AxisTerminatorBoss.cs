using Celeste.Helpers;

namespace Celeste.Entities
{
    /// <summary>
    /// Axis Terminator Boss - Terminator-style robot boss
    /// Relentless pursuit with heavy weaponry
    /// </summary>
    [CustomEntity(ids: "DZ/AxisTerminatorBoss")]
    [Tracked]
    [HotReloadable]
    public class AxisTerminatorBoss : BossActor
    {
        // Boss properties
        private int health = 500;
        private int maxHealth = 500;
        private bool isDefeated = false;
        private bool isEnraged = false;
        
        // Attack patterns
        private enum AttackType
        {
            MachineGunBarrage,
            RocketLauncher,
            LaserSight,
            ElectricPulse,
            ChargeTackle
        }
        
        // Visual components
        private Sprite robotSprite;
        private Sprite weaponSprite;
        private VertexLight redEye;
        private List<Sprite> armor = new List<Sprite>();
        private SoundSource mechanicalSfx;
        private Wiggler damageWiggler;
        
        // State
        private AttackType currentAttack;
#pragma warning disable CS0414
        private float attackTimer = 0f;
#pragma warning restore CS0414
        private float attackCooldown = 2f;
        private bool isCharging = false;
        private Vector2 chargeDirection;
        private float chargeSpeed = 300f;
        private int armorPieces = 3;
        
        // Targeting
        private global::Celeste.Player targetPlayer;
        private Vector2 aimDirection;
        private float laserSightAngle = 0f;
        
        public AxisTerminatorBoss(EntityData data, Vector2 offset) 
            : base(
                data.Position + offset,
                "axis_terminator_boss",
                new Vector2(1f, 1f),
                maxFall: 200f,
                collidable: true,
                solidCollidable: true,
                gravityMult: 1f,
                collider: new Hitbox(32f, 48f, -16f, -48f)
            )
        {
            setupVisuals();
        }
        
        private void setupVisuals()
        {
            // Robot sprite
            Add(robotSprite = new Sprite(GFX.Game, "characters/DZ/axis/"));
            robotSprite.AddLoop("idle", "terminator_idle", 0.1f);
            robotSprite.AddLoop("walk", "terminator_walk", 0.08f);
            robotSprite.AddLoop("attack", "terminator_attack", 0.06f);
            robotSprite.AddLoop("charge", "terminator_charge", 0.05f);
            robotSprite.AddLoop("damaged", "terminator_damaged", 0.12f);
            robotSprite.Play("idle");
            robotSprite.CenterOrigin();
            
            // Weapon sprite
            Add(weaponSprite = new Sprite(GFX.Game, "characters/DZ/axis/"));
            weaponSprite.AddLoop("machine_gun", "weapon_mg", 0.08f);
            weaponSprite.AddLoop("rocket_launcher", "weapon_rl", 0.1f);
            weaponSprite.AddLoop("laser", "weapon_laser", 0.05f);
            weaponSprite.Play("machine_gun");
            weaponSprite.CenterOrigin();
            weaponSprite.Position = new Vector2(16f, -24f);
            
            // Red targeting eye
            Add(redEye = new VertexLight(Color.Red, 1f, 48, 64));
            redEye.Position = new Vector2(0f, -36f);
            
            // Damage wiggler
            Add(damageWiggler = Wiggler.Create(0.5f, 3f));
            
            // Audio
            Add(mechanicalSfx = new SoundSource());
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            targetPlayer = scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Start intro
            Add(new Coroutine(introSequence()));
        }
        
        private IEnumerator introSequence()
        {
            var level = Scene as Level;
            
            // Mechanical boot-up
            Audio.Play("event:/axis_terminator_boot", Position);
            mechanicalSfx.Play("event:/axis_mechanical_loop");
            
            // Red eye glows
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 0.5f)
            {
                redEye.Alpha = t;
                yield return null;
            }
            
            redEye.Alpha = 1f;
            
            yield return 0.5f;
            
            // Target acquired
            Audio.Play("event:/axis_target_acquired", Position);
            
            yield return 0.5f;
            
            // Start combat
            Add(new Coroutine(combatLoop()));
        }
        
        private IEnumerator combatLoop()
        {
            while (!isDefeated && health > 0)
            {
                // Update target
                if (targetPlayer != null && !targetPlayer.Dead)
                {
                    aimDirection = (targetPlayer.Position - Position).SafeNormalize();
                }
                
                // Choose attack based on distance
                float distToPlayer = Vector2.Distance(Position, targetPlayer?.Position ?? Position);
                
                if (distToPlayer > 200f)
                {
                    // Long range attacks
                    currentAttack = Calc.Random.Choose(AttackType.MachineGunBarrage, AttackType.RocketLauncher);
                }
                else if (distToPlayer > 100f)
                {
                    // Mid range attacks
                    currentAttack = Calc.Random.Choose(AttackType.LaserSight, AttackType.ElectricPulse);
                }
                else
                {
                    // Close range attacks
                    currentAttack = AttackType.ChargeTackle;
                }
                
                yield return executeAttack(currentAttack);
                
                // Cooldown (faster when enraged)
                yield return isEnraged ? attackCooldown * 0.7f : attackCooldown;
            }
        }
        
        private IEnumerator executeAttack(AttackType attack)
        {
            yield return TelegraphIntent(attack switch
            {
                AttackType.RocketLauncher => BossTelegraphType.PositioningOrange,
                AttackType.LaserSight => BossTelegraphType.PositioningOrange,
                AttackType.ChargeTackle => BossTelegraphType.DashCyan,
                _ => BossTelegraphType.DangerRed
            });

            robotSprite.Play("attack");
            
            switch (attack)
            {
                case AttackType.MachineGunBarrage:
                    yield return machineGunBarrageAttack();
                    break;
                case AttackType.RocketLauncher:
                    yield return rocketLauncherAttack();
                    break;
                case AttackType.LaserSight:
                    yield return laserSightAttack();
                    break;
                case AttackType.ElectricPulse:
                    yield return electricPulseAttack();
                    break;
                case AttackType.ChargeTackle:
                    yield return chargeTackleAttack();
                    break;
            }
            
            robotSprite.Play("idle");
        }
        
        private IEnumerator machineGunBarrageAttack()
        {
            weaponSprite.Play("machine_gun");
            Audio.Play("event:/axis_machine_gun_start", Position);
            
            var level = Scene as Level;
            
            int bulletCount = isEnraged ? 20 : 12;
            
            for (int i = 0; i < bulletCount; i++)
            {
                Audio.Play("event:/axis_machine_gun_shot", Position);
                
                // Spawn bullet projectile
                Vector2 spread = aimDirection.Rotate(Calc.Random.Range(-0.2f, 0.2f));
                // level?.Add(new AxisBullet(Position + weaponSprite.Position, spread * 200f));
                
                // Recoil
                Speed -= spread * 5f;
                
                yield return 0.1f;
            }
            
            yield return 0.5f;
        }
        
        private IEnumerator rocketLauncherAttack()
        {
            weaponSprite.Play("rocket_launcher");
            Audio.Play("event:/axis_rocket_launch", Position);
            
            var level = Scene as Level;
            
            int rocketCount = isEnraged ? 3 : 2;
            
            for (int i = 0; i < rocketCount; i++)
            {
                // Spawn homing rocket
                // level?.Add(new AxisRocket(Position + weaponSprite.Position, aimDirection, targetPlayer));
                
                level?.Shake(0.3f);
                
                yield return 0.8f;
            }
            
            yield return 0.5f;
        }
        
        private IEnumerator laserSightAttack()
        {
            weaponSprite.Play("laser");
            Audio.Play("event:/axis_laser_charge", Position);
            
            var level = Scene as Level;
            
            // Laser sight tracking
            float trackingTime = 1.5f;
            for (float t = 0f; t < trackingTime; t += Engine.DeltaTime)
            {
                if (targetPlayer != null && !targetPlayer.Dead)
                {
                    Vector2 toPlayer = (targetPlayer.Position - Position);
                    laserSightAngle = toPlayer.Angle();
                }
                
                // Draw laser sight line
                // Render handled in Render()
                
                yield return null;
            }
            
            // Fire laser
            Audio.Play("event:/axis_laser_fire", Position);
            
            // Create laser beam
            // level?.Add(new AxisLaserBeam(Position, laserSightAngle));
            
            level?.Shake(0.5f);
            
            yield return 1f;
        }
        
        private IEnumerator electricPulseAttack()
        {
            Audio.Play("event:/axis_electric_charge", Position);
            
            var level = Scene as Level;
            
            // Charge up
            for (int i = 0; i < 3; i++)
            {
                redEye.Alpha = 0.3f;
                yield return 0.2f;
                redEye.Alpha = 1f;
                yield return 0.2f;
            }
            
            // Pulse release
            Audio.Play("event:/axis_electric_pulse", Position);
            
            level?.Displacement.AddBurst(Position, 0.8f, 96f, 192f, 0.5f);
            
            // Create electric shockwave
            for (int i = 0; i < 12; i++)
            {
                float angle = (i / 12f) * MathHelper.TwoPi;
                Vector2 direction = new Vector2(
                    (float)System.Math.Cos(angle),
                    (float)System.Math.Sin(angle)
                );
                
                // Create electric projectile
                // level?.Add(new AxisElectricBolt(Position, direction * 150f));
            }
            
            yield return 1f;
        }
        
        private IEnumerator chargeTackleAttack()
        {
            Audio.Play("event:/axis_charge_start", Position);
            robotSprite.Play("charge");
            
            isCharging = true;
            
            if (targetPlayer != null && !targetPlayer.Dead)
            {
                chargeDirection = (targetPlayer.Position - Position).SafeNormalize();
            }
            
            var level = Scene as Level;
            
            // Charge forward
            float chargeDuration = 1.5f;
            for (float t = 0f; t < chargeDuration; t += Engine.DeltaTime)
            {
                Speed = chargeDirection * (isEnraged ? chargeSpeed * 1.3f : chargeSpeed);
                
                // Dust particles
                if (Grounded)
                {
                    level?.Particles.Emit(global::Celeste.Player.P_DashA, 2, Position, Vector2.One * 4f);
                }
                
                yield return null;
            }
            
            isCharging = false;
            Speed = Vector2.Zero;
            
            // Impact
            Audio.Play("event:/axis_charge_impact", Position);
            level?.Shake(0.8f);
            
            yield return 0.5f;
        }
        
        public override void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            health -= damage;
            robotSprite.Play("damaged");
            damageWiggler.Start();
            
            Audio.Play("event:/axis_take_damage", Position);
            
            var level = Scene as Level;
            level?.Shake(0.3f);
            
            // Check for armor break
            if (armorPieces > 0 && health <= maxHealth * (armorPieces / 4f))
            {
                breakArmor();
            }
            
            // Check for enrage
            if (!isEnraged && health <= maxHealth * 0.5f)
            {
                enrage();
            }
            
            if (health <= 0)
            {
                defeat();
            }
        }
        
        private void breakArmor()
        {
            armorPieces--;
            
            Audio.Play("event:/axis_armor_break", Position);
            
            var level = Scene as Level;
            level?.Shake(0.5f);
            level?.Displacement.AddBurst(Position, 0.5f, 64f, 128f, 0.3f);
        }
        
        private void enrage()
        {
            isEnraged = true;
            
            Audio.Play("event:/axis_enrage", Position);
            
            redEye.Color = Color.OrangeRed;
            redEye.StartRadius = 64f;
            redEye.EndRadius = 96f;
            
            var level = Scene as Level;
            level?.Shake(0.8f);
        }
        
        private void defeat()
        {
            isDefeated = true;
            
            Add(new Coroutine(defeatSequence()));
        }
        
        private IEnumerator defeatSequence()
        {
            Audio.Play("event:/axis_defeat", Position);
            mechanicalSfx.Stop();
            
            var level = Scene as Level;
            level?.Shake(1f);
            
            // Sparks and explosions
            for (int i = 0; i < 5; i++)
            {
                Audio.Play("event:/axis_spark", Position);
                level?.Displacement.AddBurst(Position + Calc.Random.Range(Vector2.One * -16f, Vector2.One * 16f), 0.3f, 32f, 64f, 0.2f);
                
                yield return 0.3f;
            }
            
            // Eye flickers out
            for (int i = 0; i < 3; i++)
            {
                redEye.Alpha = 0f;
                yield return 0.2f;
                redEye.Alpha = 1f;
                yield return 0.2f;
            }
            
            redEye.Alpha = 0f;
            
            yield return 1f;
            
            // Set defeat flag
            level?.Session.SetFlag("axis_terminator_boss_defeated");
            
            RemoveSelf();
        }
        
        public override void Update()
        {
            base.Update();
            
            // Face player
            if (targetPlayer != null && !isCharging)
            {
                robotSprite.Scale.X = System.Math.Sign(targetPlayer.X - X);
            }
            
            // Update weapon rotation
            if (targetPlayer != null)
            {
                weaponSprite.Rotation = aimDirection.Angle();
            }
        }
        
        public override void Render()
        {
            base.Render();
            
            // Render laser sight
            if (weaponSprite.CurrentAnimationID == "laser")
            {
                Vector2 laserStart = Position + weaponSprite.Position;
                Vector2 laserEnd = laserStart + new Vector2(
                    (float)System.Math.Cos(laserSightAngle),
                    (float)System.Math.Sin(laserSightAngle)
                ) * 500f;
                
                Draw.Line(laserStart, laserEnd, Color.Red * 0.7f, 2f);
            }
        }
    }
}
