using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// Gigant Edge Mid-Boss - A massive armored knight
    /// Heavy hitting mid-boss with sword and shield attacks
    /// Sprite path: characters/gigantedge/
    /// </summary>
    [CustomEntity(ids: "DZ/GigantEdgeBoss")]
    [Tracked]
    [HotReloadable]
    public class GigantEdgeBoss : BossActor
    {
        #region Enums and Constants
        public enum AttackType
        {
            SwordSlash,
            ShieldBash,
            JumpSlam,
            SpinAttack,
            ChargeStrike
        }

        public enum BossState
        {
            Idle,
            Chase,
            Attack,
            Blocking,
            Stunned,
            Defeated
        }
        #endregion

        #region Properties
        private int health = 300;
        private int maxHealth = 300;
        private bool isDefeated = false;
        private BossState currentState = BossState.Idle;
        
        private Sprite bodySprite;
        private Sprite swordSprite;
        private Sprite shieldSprite;
        private Sprite armorEffectSprite;
        private VertexLight eyeGlow;
        private SoundSource armorLoop;
        
        private bool isBlocking = false;
        private float blockCooldown = 0f;
        private float staggerTimer = 0f;
        #endregion

        #region Constructors
        public GigantEdgeBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "gigant_edge", new Vector2(2f, 2f), 100f, true, true, 1f, 
                   new Hitbox(48f, 64f, -24f, -64f))
        {
            health = data.Int("health", 300);
            maxHealth = data.Int("maxHealth", 300);
            SetupVisuals();
        }

        public GigantEdgeBoss(Vector2 position) 
            : base(position, "gigant_edge", new Vector2(2f, 2f), 100f, true, true, 1f, 
                   new Hitbox(48f, 64f, -24f, -64f))
        {
            SetupVisuals();
        }
        #endregion

        #region Setup
        private void SetupVisuals()
        {
            // Main body/armor sprite
            Add(bodySprite = new Sprite(GFX.Game, "characters/gigantedge/"));
            bodySprite.AddLoop("idle", "body_idle", 0.12f);
            bodySprite.AddLoop("walk", "body_walk", 0.08f);
            bodySprite.AddLoop("attack", "body_attack", 0.06f);
            bodySprite.AddLoop("block", "body_block", 0.1f);
            bodySprite.AddLoop("stunned", "body_stunned", 0.1f);
            bodySprite.Add("defeat", "body_defeat", 0.1f);
            bodySprite.Play("idle");
            bodySprite.CenterOrigin();
            
            // Sword sprite
            Add(swordSprite = new Sprite(GFX.Game, "characters/gigantedge/"));
            swordSprite.AddLoop("idle", "sword_idle", 0.1f);
            swordSprite.Add("slash", "sword_slash", 0.04f);
            swordSprite.Add("spin", "sword_spin", 0.03f);
            swordSprite.Add("slam", "sword_slam", 0.05f);
            swordSprite.CenterOrigin();
            swordSprite.Position = new Vector2(24f, -32f);
            
            // Shield sprite
            Add(shieldSprite = new Sprite(GFX.Game, "characters/gigantedge/"));
            shieldSprite.AddLoop("idle", "shield_idle", 0.1f);
            shieldSprite.AddLoop("raise", "shield_raise", 0.06f);
            shieldSprite.Add("bash", "shield_bash", 0.05f);
            shieldSprite.CenterOrigin();
            shieldSprite.Position = new Vector2(-24f, -32f);
            
            // Armor effect (shiny/gleam)
            Add(armorEffectSprite = new Sprite(GFX.Game, "characters/gigantedge/"));
            armorEffectSprite.AddLoop("shine", "armor_shine", 0.15f);
            armorEffectSprite.CenterOrigin();
            armorEffectSprite.Color = Color.White * 0.5f;
            
            // Eye glow
            Add(eyeGlow = new VertexLight(Color.Red, 0.8f, 16, 32));
            eyeGlow.Position = new Vector2(0f, -56f);
            
            // Armor clanking sound loop
            Add(armorLoop = new SoundSource());
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
            
            if (blockCooldown > 0f)
                blockCooldown -= Engine.DeltaTime;
            
            if (staggerTimer > 0f)
            {
                staggerTimer -= Engine.DeltaTime;
                if (staggerTimer <= 0f)
                    currentState = BossState.Idle;
            }
            
            // Eye pulsing
            eyeGlow.Alpha = 0.7f + (float)Math.Sin(Scene.TimeActive * 2f) * 0.2f;
            
            // Update sprite facing
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && currentState != BossState.Attack)
            {
                bodySprite.FlipX = player.Position.X < Position.X;
                swordSprite.Position = new Vector2(bodySprite.FlipX ? -24f : 24f, -32f);
                shieldSprite.Position = new Vector2(bodySprite.FlipX ? 24f : -24f, -32f);
            }
        }
        #endregion

        #region Main Boss Routine
        private IEnumerator BossRoutine()
        {
            yield return 1f; // Initial delay
            
            while (!isDefeated)
            {
                if (currentState == BossState.Stunned)
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
                
                // Decide action based on distance
                if (distToPlayer < 80f)
                {
                    // Close range - attack
                    currentState = BossState.Attack;
                    yield return ExecuteAttack(ChooseAttack());
                }
                else if (distToPlayer < 200f)
                {
                    // Medium range - charge or chase
                    if (Calc.Random.Chance(0.4f))
                    {
                        currentState = BossState.Attack;
                        yield return ExecuteAttack(AttackType.ChargeStrike);
                    }
                    else
                    {
                        yield return ChasePlayer(player, 1.5f);
                    }
                }
                else
                {
                    // Far range - chase
                    yield return ChasePlayer(player, 2f);
                }
                
                // Random blocking
                if (Calc.Random.Chance(0.2f) && blockCooldown <= 0f)
                {
                    yield return BlockStance();
                }
                
                currentState = BossState.Idle;
                yield return 0.5f + Calc.Random.Range(0f, 0.5f);
            }
        }

        private AttackType ChooseAttack()
        {
            return (AttackType)Calc.Random.Next(0, 5);
        }

        private IEnumerator ChasePlayer(global::Celeste.Player player, float duration)
        {
            currentState = BossState.Chase;
            bodySprite.Play("walk");
            armorLoop.Play("event:/gigantedge_walk");
            
            float timer = 0f;
            while (timer < duration && !isDefeated)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * 80f;
                timer += Engine.DeltaTime;
                yield return null;
            }
            
            Speed = Vector2.Zero;
            armorLoop.Stop();
            bodySprite.Play("idle");
        }
        #endregion

        #region Attacks
        private IEnumerator ExecuteAttack(AttackType attack)
        {
            bodySprite.Play("attack");
            
            switch (attack)
            {
                case AttackType.SwordSlash:
                    yield return SwordSlashAttack();
                    break;
                case AttackType.ShieldBash:
                    yield return ShieldBashAttack();
                    break;
                case AttackType.JumpSlam:
                    yield return JumpSlamAttack();
                    break;
                case AttackType.SpinAttack:
                    yield return SpinAttackRoutine();
                    break;
                case AttackType.ChargeStrike:
                    yield return ChargeStrikeAttack();
                    break;
            }
            
            bodySprite.Play("idle");
        }

        private IEnumerator SwordSlashAttack()
        {
            swordSprite.Play("slash");
            Audio.Play("event:/gigantedge_slash", Position);
            
            var level = Scene as Level;
            
            // Triple slash
            for (int i = 0; i < 3; i++)
            {
                float xOffset = bodySprite.FlipX ? -40f : 40f;
                level?.Displacement.AddBurst(Position + new Vector2(xOffset, -32f), 0.4f, 16f, 48f, 0.3f);
                level?.Shake(0.3f);
                yield return 0.2f;
            }
        }

        private IEnumerator ShieldBashAttack()
        {
            shieldSprite.Play("bash");
            Audio.Play("event:/gigantedge_bash", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * 200f;
            }
            
            yield return 0.2f;
            
            Speed = Vector2.Zero;
            level?.Shake(0.8f);
            level?.Displacement.AddBurst(Position, 0.6f, 24f, 64f, 0.4f);
        }

        private IEnumerator JumpSlamAttack()
        {
            Audio.Play("event:/gigantedge_jump", Position);
            swordSprite.Play("slam");
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();

            // Jump up
            Speed = new Vector2(Speed.X, 600f);

            yield return 0.4f;
            
            // Target player position
            if (player != null)
            {
                Position = new Vector2(player.Position.X, Position.Y);
            }
            
            // Slam down
            Speed = new Vector2(Speed.X, 600f);
            
            while (!Grounded)
            {
                yield return null;
            }
            
            Speed = Vector2.Zero;
            Audio.Play("event:/gigantedge_slam", Position);
            level?.Shake(1.5f);
            
            // Ground slam shockwave
            for (int i = 0; i < 3; i++)
            {
                level?.Displacement.AddBurst(Position, 0.6f, i * 30f, i * 30f + 40f, 0.4f);
            }
        }

        private IEnumerator SpinAttackRoutine()
        {
            swordSprite.Play("spin");
            Audio.Play("event:/gigantedge_spin", Position);
            
            var level = Scene as Level;
            
            // Spin for a duration
            for (float t = 0; t < 1.5f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.4f, 24f, 56f, 0.3f);
                
                var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null)
                {
                    Vector2 direction = (player.Position - Position).SafeNormalize();
                    Speed = direction * 120f;
                }
                
                yield return null;
            }
            
            Speed = Vector2.Zero;
            level?.Shake(0.5f);
        }

        private IEnumerator ChargeStrikeAttack()
        {
            Audio.Play("event:/gigantedge_charge", Position);
            
            // Wind up
            yield return 0.5f;
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * 400f;
            }
            
            swordSprite.Play("slash");
            
            for (float t = 0; t < 0.3f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.3f, 16f, 40f, 0.2f);
                yield return null;
            }
            
            Speed = Vector2.Zero;
            level?.Shake(1f);
            level?.Displacement.AddBurst(Position, 0.8f, 32f, 72f, 0.5f);
        }

        private IEnumerator BlockStance()
        {
            isBlocking = true;
            currentState = BossState.Blocking;
            bodySprite.Play("block");
            shieldSprite.Play("raise");
            Audio.Play("event:/gigantedge_block", Position);
            
            yield return 1.5f;
            
            isBlocking = false;
            blockCooldown = 3f;
            shieldSprite.Play("idle");
        }
        #endregion

        #region Damage and Defeat
        public override void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            // Reduced damage when blocking
            if (isBlocking)
            {
                damage = damage / 3;
                Audio.Play("event:/gigantedge_block_hit", Position);
                
                var level = Scene as Level;
                level?.Displacement.AddBurst(shieldSprite.RenderPosition, 0.4f, 20f, 48f, 0.3f);
            }
            else
            {
                Audio.Play("event:/gigantedge_hurt", Position);
            }
            
            health -= damage;
            
            var scene = Scene as Level;
            scene?.Shake(0.3f);
            
            bodySprite.Color = Color.Red;
            Add(new Coroutine(FlashReset()));
            
            // Stagger on heavy hit
            if (damage >= 30 && !isBlocking)
            {
                currentState = BossState.Stunned;
                staggerTimer = 1f;
                bodySprite.Play("stunned");
            }
            
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
            armorLoop.Stop();
            
            bodySprite.Play("defeat");
            swordSprite.Visible = false;
            shieldSprite.Visible = false;
            Audio.Play("event:/gigantedge_defeat", Position);
            
            var level = Scene as Level;
            
            // Armor crumbling
            for (int i = 0; i < 5; i++)
            {
                Vector2 burstPos = Position + Calc.Random.Range(Vector2.One * -30f, Vector2.One * 30f);
                level?.Displacement.AddBurst(burstPos, 0.5f, 16f, 40f, 0.3f);
                level?.Shake(0.3f);
                yield return 0.2f;
            }
            
            // Final collapse
            level?.Shake(1f);
            level?.Displacement.AddBurst(Position, 1f, 40f, 96f, 0.6f);
            
            yield return 1f;
            
            level?.Session.SetFlag("gigant_edge_defeated");
            RemoveSelf();
        }
        #endregion

        #region Cleanup
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            armorLoop?.Stop();
        }
        #endregion
    }
}
