using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// Robobo Guard Mid-Boss - A robotic security unit
    /// Mechanical mid-boss with programmed attack patterns
    /// Sprite path: characters/DZ/roboboguard/
    /// </summary>
    [CustomEntity(ids: "DZ/RoboboGuardBoss")]
    [Tracked]
    [HotReloadable]
    public class RoboboGuardBoss : BossActor
    {
        #region Enums and Constants
        public enum AttackType
        {
            LaserBeam,
            RocketPunch,
            ShieldBash,
            ElectricField,
            SelfDestruct
        }

        public enum BossState
        {
            Idle,
            Patrol,
            Alert,
            Attack,
            Rebooting,
            Defeated
        }
        #endregion

        #region Properties
        private int health = 350;
        private int maxHealth = 350;
        private bool isDefeated = false;
        private BossState currentState = BossState.Idle;
        
        private Sprite bodySprite;
        private Sprite eyeSprite;
        private Sprite armsSprite;
        private Sprite shieldSprite;
        private VertexLight eyeGlow;
        private VertexLight shieldGlow;
        private SoundSource machineLoop;
        private SoundSource alarmLoop;
        
        private bool shieldActive = false;
        private float shieldEnergy = 100f;
        private bool isRebooting = false;
        private float rebootTimer = 0f;
        #endregion

        #region Constructors
        public RoboboGuardBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "robobo_guard", new Vector2(1.5f, 1.5f), 80f, true, true, 1f, 
                   new Hitbox(44f, 56f, -22f, -56f))
        {
            health = data.Int("health", 350);
            maxHealth = data.Int("maxHealth", 350);
            SetupVisuals();
        }

        public RoboboGuardBoss(Vector2 position) 
            : base(position, "robobo_guard", new Vector2(1.5f, 1.5f), 80f, true, true, 1f, 
                   new Hitbox(44f, 56f, -22f, -56f))
        {
            SetupVisuals();
        }
        #endregion

        #region Setup
        private void SetupVisuals()
        {
            // Body sprite
            Add(bodySprite = new Sprite(GFX.Game, "characters/DZ/roboboguard/"));
            bodySprite.AddLoop("idle", "body_idle", 0.12f);
            bodySprite.AddLoop("walk", "body_walk", 0.08f);
            bodySprite.AddLoop("alert", "body_alert", 0.1f);
            bodySprite.AddLoop("attack", "body_attack", 0.06f);
            bodySprite.AddLoop("reboot", "body_reboot", 0.15f);
            bodySprite.Add("defeat", "body_defeat", 0.1f);
            bodySprite.Play("idle");
            bodySprite.CenterOrigin();
            
            // Eye/visor sprite
            Add(eyeSprite = new Sprite(GFX.Game, "characters/DZ/roboboguard/"));
            eyeSprite.AddLoop("green", "eye_green", 0.1f);
            eyeSprite.AddLoop("yellow", "eye_yellow", 0.08f);
            eyeSprite.AddLoop("red", "eye_red", 0.05f);
            eyeSprite.AddLoop("off", "eye_off", 0.2f);
            eyeSprite.AddLoop("scan", "eye_scan", 0.03f);
            eyeSprite.Play("green");
            eyeSprite.CenterOrigin();
            eyeSprite.Position = new Vector2(0f, -44f);
            
            // Arms sprite
            Add(armsSprite = new Sprite(GFX.Game, "characters/DZ/roboboguard/"));
            armsSprite.AddLoop("idle", "arms_idle", 0.12f);
            armsSprite.AddLoop("punch", "arms_punch", 0.04f);
            armsSprite.AddLoop("guard", "arms_guard", 0.1f);
            armsSprite.AddLoop("laser", "arms_laser", 0.05f);
            armsSprite.CenterOrigin();
            armsSprite.Position = new Vector2(0f, -30f);
            
            // Energy shield sprite
            Add(shieldSprite = new Sprite(GFX.Game, "characters/DZ/roboboguard/"));
            shieldSprite.AddLoop("active", "shield_active", 0.04f);
            shieldSprite.AddLoop("flicker", "shield_flicker", 0.03f);
            shieldSprite.CenterOrigin();
            shieldSprite.Position = new Vector2(0f, -28f);
            shieldSprite.Visible = false;
            
            // Eye glow
            Add(eyeGlow = new VertexLight(Color.Green, 0.8f, 20, 40));
            eyeGlow.Position = new Vector2(0f, -44f);
            
            // Shield glow
            Add(shieldGlow = new VertexLight(Color.Cyan, 0.6f, 32, 56));
            shieldGlow.Position = new Vector2(0f, -28f);
            shieldGlow.Alpha = 0f;
            
            // Machine loop
            Add(machineLoop = new SoundSource());
            Add(alarmLoop = new SoundSource());
        }
        #endregion

        #region Scene Management
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            machineLoop.Play("event:/robobo_idle_loop");
            
            Add(new Coroutine(BossRoutine()));
        }

        public override void Update()
        {
            base.Update();
            
            // Eye glow effects
            if (!isRebooting && !isDefeated)
            {
                eyeGlow.Alpha = 0.7f + (float)Math.Sin(Scene.TimeActive * 3f) * 0.2f;
            }
            
            // Shield energy regeneration
            if (!shieldActive && shieldEnergy < 100f)
            {
                shieldEnergy += Engine.DeltaTime * 10f;
            }
            
            // Shield effects
            if (shieldActive)
            {
                shieldGlow.Alpha = 0.5f + (float)Math.Sin(Scene.TimeActive * 5f) * 0.3f;
                
                if (shieldEnergy <= 20f)
                {
                    shieldSprite.Play("flicker");
                }
            }
            
            // Reboot timer
            if (isRebooting)
            {
                rebootTimer -= Engine.DeltaTime;
                if (rebootTimer <= 0f)
                {
                    isRebooting = false;
                    eyeSprite.Play("green");
                    bodySprite.Play("idle");
                    Audio.Play("event:/robobo_online", Position);
                }
            }
            
            // Face player
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && currentState != BossState.Attack)
            {
                bodySprite.FlipX = player.Position.X < Position.X;
            }
        }
        #endregion

        #region Main Boss Routine
        private IEnumerator BossRoutine()
        {
            // Startup sequence
            Audio.Play("event:/robobo_startup", Position);
            eyeSprite.Play("scan");
            
            yield return 1f;
            
            eyeSprite.Play("green");
            
            while (!isDefeated)
            {
                if (isRebooting)
                {
                    yield return null;
                    continue;
                }
                
                var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player == null)
                {
                    yield return null;
                    continue;
                }
                
                float distToPlayer = Vector2.Distance(Position, player.Position);
                
                // Alert mode when player is close
                if (distToPlayer < 150f)
                {
                    if (currentState != BossState.Alert)
                    {
                        currentState = BossState.Alert;
                        eyeSprite.Play("yellow");
                        alarmLoop.Play("event:/robobo_alert");
                    }
                    
                    // Attack
                    currentState = BossState.Attack;
                    eyeSprite.Play("red");
                    yield return ExecuteAttack(ChooseAttack());
                    eyeSprite.Play("yellow");
                }
                else
                {
                    // Patrol
                    if (currentState != BossState.Patrol)
                    {
                        currentState = BossState.Patrol;
                        eyeSprite.Play("green");
                        alarmLoop.Stop();
                    }
                    
                    yield return PatrolMovement();
                }
                
                yield return 0.5f + Calc.Random.Range(0f, 0.5f);
            }
        }

        private AttackType ChooseAttack()
        {
            // Self-destruct only at low health
            if (health <= maxHealth * 0.2f && Calc.Random.Chance(0.3f))
            {
                return AttackType.SelfDestruct;
            }
            
            return (AttackType)Calc.Random.Next(0, 4);
        }

        private IEnumerator PatrolMovement()
        {
            bodySprite.Play("walk");
            
            // Simple patrol back and forth
            Vector2 direction = bodySprite.FlipX ? Vector2.UnitX * -1f : Vector2.UnitX;
            Speed = direction * 60f;
            
            yield return 1.5f;
            
            Speed = Vector2.Zero;
            bodySprite.FlipX = !bodySprite.FlipX;
            bodySprite.Play("idle");
        }
        #endregion

        #region Attacks
        private IEnumerator ExecuteAttack(AttackType attack)
        {
            bodySprite.Play("attack");
            
            switch (attack)
            {
                case AttackType.LaserBeam:
                    yield return LaserBeamAttack();
                    break;
                case AttackType.RocketPunch:
                    yield return RocketPunchAttack();
                    break;
                case AttackType.ShieldBash:
                    yield return ShieldBashAttack();
                    break;
                case AttackType.ElectricField:
                    yield return ElectricFieldAttack();
                    break;
                case AttackType.SelfDestruct:
                    yield return SelfDestructAttack();
                    break;
            }
            
            bodySprite.Play("alert");
        }

        private IEnumerator LaserBeamAttack()
        {
            armsSprite.Play("laser");
            eyeSprite.Play("scan");
            Audio.Play("event:/robobo_laser_charge", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Charge up
            eyeGlow.Color = Color.Red;
            eyeGlow.Alpha = 1.5f;
            
            yield return 0.5f;
            
            // Fire laser
            Audio.Play("event:/robobo_laser_fire", Position);
            
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                
                for (float dist = 30f; dist < 200f; dist += 15f)
                {
                    level?.Displacement.AddBurst(Position + direction * dist + new Vector2(0f, -30f), 0.4f, 12f, 32f, 0.3f);
                }
            }
            
            level?.Shake(0.8f);
            
            eyeGlow.Color = Color.Green;
            eyeGlow.Alpha = 0.8f;
            armsSprite.Play("idle");
        }

        private IEnumerator RocketPunchAttack()
        {
            armsSprite.Play("punch");
            Audio.Play("event:/robobo_punch", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Wind up
            yield return 0.2f;
            
            // Launch rocket punch
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                
                // Fist travels
                for (int i = 0; i < 8; i++)
                {
                    Vector2 fistPos = Position + direction * (i * 25f + 30f) + new Vector2(0f, -30f);
                    level?.Displacement.AddBurst(fistPos, 0.4f, 16f, 40f, 0.3f);
                    yield return 0.03f;
                }
            }
            
            level?.Shake(1f);
            level?.Displacement.AddBurst(Position, 0.6f, 32f, 64f, 0.4f);
            
            armsSprite.Play("idle");
        }

        private IEnumerator ShieldBashAttack()
        {
            armsSprite.Play("guard");
            ActivateShield();
            Audio.Play("event:/robobo_shield_charge", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            yield return 0.3f;
            
            // Charge forward with shield
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * 300f;
            }
            
            for (float t = 0; t < 0.3f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.3f, 20f, 48f, 0.25f);
                yield return null;
            }
            
            Speed = Vector2.Zero;
            
            Audio.Play("event:/robobo_shield_impact", Position);
            level?.Shake(1.2f);
            level?.Displacement.AddBurst(Position, 0.8f, 40f, 80f, 0.5f);
            
            DeactivateShield();
            shieldEnergy -= 30f;
            armsSprite.Play("idle");
        }

        private IEnumerator ElectricFieldAttack()
        {
            Audio.Play("event:/robobo_electric", Position);
            eyeGlow.Color = Color.Yellow;
            
            var level = Scene as Level;
            
            // Charge up
            for (float t = 0; t < 0.5f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.3f, 40f, 20f, 0.2f);
                yield return null;
            }
            
            // Release electric field
            for (int ring = 0; ring < 4; ring++)
            {
                level?.Displacement.AddBurst(Position, 0.6f, ring * 25f, ring * 25f + 35f, 0.4f);
                level?.Shake(0.3f);
                yield return 0.15f;
            }
            
            level?.Flash(Color.Yellow * 0.3f, true);
            
            eyeGlow.Color = Color.Green;
        }

        private IEnumerator SelfDestructAttack()
        {
            // Warning sequence
            alarmLoop.Play("event:/robobo_selfdestruct_warning");
            eyeSprite.Play("red");
            bodySprite.Play("reboot");
            
            var level = Scene as Level;
            
            for (int i = 0; i < 5; i++)
            {
                eyeGlow.Color = i % 2 == 0 ? Color.Red : Color.Yellow;
                eyeGlow.Alpha = 2f;
                level?.Shake(0.3f);
                yield return 0.3f;
            }
            
            // BOOM
            Audio.Play("event:/robobo_selfdestruct", Position);
            level?.Flash(Color.Orange * 0.5f, true);
            level?.Shake(2f);
            
            for (int i = 0; i < 5; i++)
            {
                level?.Displacement.AddBurst(Position, 1f, i * 40f, i * 40f + 60f, 0.6f);
            }
            
            alarmLoop.Stop();
            
            // Self damage
            TakeDamage(50);
            
            // Reboot required
            isRebooting = true;
            rebootTimer = 3f;
            eyeSprite.Play("off");
            eyeGlow.Alpha = 0f;
        }

        private void ActivateShield()
        {
            if (shieldEnergy >= 20f)
            {
                shieldActive = true;
                shieldSprite.Visible = true;
                shieldSprite.Play("active");
                shieldGlow.Alpha = 0.6f;
            }
        }

        private void DeactivateShield()
        {
            shieldActive = false;
            shieldSprite.Visible = false;
            shieldGlow.Alpha = 0f;
        }
        #endregion

        #region Damage and Defeat
        public override void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            // Shield absorbs damage
            if (shieldActive)
            {
                shieldEnergy -= damage;
                Audio.Play("event:/robobo_shield_hit", Position);
                
                var shieldLevel = Scene as Level;
                shieldLevel?.Displacement.AddBurst(Position, 0.4f, 32f, 48f, 0.3f);
                
                if (shieldEnergy <= 0f)
                {
                    shieldEnergy = 0f;
                    DeactivateShield();
                    Audio.Play("event:/robobo_shield_break", Position);
                }
                
                return;
            }
            
            // Reduced damage while rebooting
            if (isRebooting)
            {
                damage = damage / 2;
            }
            
            health -= damage;
            Audio.Play("event:/robobo_damage", Position);
            
            var level = Scene as Level;
            level?.Shake(0.3f);
            
            bodySprite.Color = Color.Orange;
            Add(new Coroutine(FlashReset()));
            
            if (health <= 0)
            {
                Defeat();
            }
        }

        private IEnumerator FlashReset()
        {
            yield return 0.1f;
            bodySprite.Color = Color.White;
        }

        private void Defeat()
        {
            isDefeated = true;
            currentState = BossState.Defeated;
            Add(new Coroutine(DefeatSequence()));
        }

        private IEnumerator DefeatSequence()
        {
            Speed = Vector2.Zero;
            machineLoop.Stop();
            alarmLoop.Stop();
            DeactivateShield();
            
            bodySprite.Play("defeat");
            eyeSprite.Play("off");
            eyeGlow.Alpha = 0f;
            Audio.Play("event:/robobo_shutdown", Position);
            
            var level = Scene as Level;
            
            // System failures
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkPos = Position + Calc.Random.Range(Vector2.One * -25f, Vector2.One * 25f);
                level?.Displacement.AddBurst(sparkPos, 0.4f, 12f, 32f, 0.3f);
                
                // Flicker eye briefly
                eyeSprite.Play(i % 2 == 0 ? "red" : "off");
                eyeGlow.Alpha = i % 2 == 0 ? 0.5f : 0f;
                
                yield return 0.25f;
            }
            
            eyeSprite.Play("off");
            eyeGlow.Alpha = 0f;
            
            // Final explosion
            Audio.Play("event:/robobo_explode", Position);
            level?.Shake(1.5f);
            level?.Displacement.AddBurst(Position, 1f, 48f, 96f, 0.6f);
            
            yield return 1f;
            
            level?.Session.SetFlag("robobo_guard_defeated");
            RemoveSelf();
        }
        #endregion

        #region Cleanup
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            machineLoop?.Stop();
            alarmLoop?.Stop();
        }
        #endregion
    }
}
