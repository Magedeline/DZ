using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// Buzzo Mid-Boss - A chainsaw-wielding maniac
    /// Aggressive mid-boss with relentless pursuit attacks
    /// Sprite path: characters/buzzo/
    /// </summary>
    [CustomEntity(ids: "DZ/BuzzoBoss")]
    [Tracked]
    [HotReloadable]
    public class BuzzoBoss : BossActor
    {
        #region Enums and Constants
        public enum AttackType
        {
            ChainsawSwipe,
            LungeAttack,
            FrenzyMode,
            ThrowDebris,
            GrabAttack
        }

        public enum BossState
        {
            Idle,
            Chase,
            Attack,
            Frenzy,
            Stunned,
            Defeated
        }
        #endregion

        #region Properties
        private int health = 280;
        private int maxHealth = 280;
        private bool isDefeated = false;
        private BossState currentState = BossState.Idle;
        
        private Sprite bodySprite;
        private Sprite chainsawSprite;
        private VertexLight chainsawGlow;
        private SoundSource chainsawLoop;
        
        private bool isFrenzied = false;
        private float frenzyTimer = 0f;
        private float aggressionLevel = 1f;
        #endregion

        #region Constructors
        public BuzzoBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "buzzo_boss", new Vector2(1f, 1f), 120f, true, true, 1f, 
                   new Hitbox(32f, 48f, -16f, -48f))
        {
            health = data.Int("health", 280);
            maxHealth = data.Int("maxHealth", 280);
            SetupVisuals();
        }

        public BuzzoBoss(Vector2 position) 
            : base(position, "buzzo_boss", new Vector2(1f, 1f), 120f, true, true, 1f, 
                   new Hitbox(32f, 48f, -16f, -48f))
        {
            SetupVisuals();
        }
        #endregion

        #region Setup
        private void SetupVisuals()
        {
            // Body sprite
            Add(bodySprite = new Sprite(GFX.Game, "characters/buzzo/"));
            bodySprite.AddLoop("idle", "body_idle", 0.1f);
            bodySprite.AddLoop("walk", "body_walk", 0.06f);
            bodySprite.AddLoop("run", "body_run", 0.04f);
            bodySprite.AddLoop("attack", "body_attack", 0.05f);
            bodySprite.AddLoop("frenzy", "body_frenzy", 0.03f);
            bodySprite.AddLoop("stunned", "body_stunned", 0.1f);
            bodySprite.Add("defeat", "body_defeat", 0.1f);
            bodySprite.Play("idle");
            bodySprite.CenterOrigin();
            
            // Chainsaw sprite
            Add(chainsawSprite = new Sprite(GFX.Game, "characters/buzzo/"));
            chainsawSprite.AddLoop("off", "chainsaw_off", 0.15f);
            chainsawSprite.AddLoop("idle", "chainsaw_idle", 0.03f);
            chainsawSprite.AddLoop("rev", "chainsaw_rev", 0.02f);
            chainsawSprite.AddLoop("swing", "chainsaw_swing", 0.03f);
            chainsawSprite.CenterOrigin();
            chainsawSprite.Position = new Vector2(24f, -24f);
            
            // Chainsaw glow (orange/red)
            Add(chainsawGlow = new VertexLight(Color.Orange, 0.6f, 20, 40));
            chainsawGlow.Position = new Vector2(24f, -24f);
            
            // Chainsaw sound loop
            Add(chainsawLoop = new SoundSource());
        }
        #endregion

        #region Scene Management
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            chainsawSprite.Play("idle");
            chainsawLoop.Play("event:/buzzo_chainsaw_idle");
            
            Add(new Coroutine(BossRoutine()));
        }

        public override void Update()
        {
            base.Update();
            
            // Chainsaw glow pulsing
            chainsawGlow.Alpha = 0.5f + (float)Math.Sin(Scene.TimeActive * 10f) * 0.3f;
            
            // Frenzy timer
            if (isFrenzied)
            {
                frenzyTimer -= Engine.DeltaTime;
                if (frenzyTimer <= 0f)
                {
                    isFrenzied = false;
                    aggressionLevel = 1f;
                    chainsawSprite.Play("idle");
                    bodySprite.Play("idle");
                }
            }
            
            // Increase aggression as health decreases
            aggressionLevel = 1f + (1f - (float)health / maxHealth) * 0.5f;
            
            // Flip sprite based on player position
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && currentState != BossState.Attack)
            {
                bodySprite.FlipX = player.Position.X < Position.X;
                chainsawSprite.Position = new Vector2(bodySprite.FlipX ? -24f : 24f, -24f);
                chainsawGlow.Position = chainsawSprite.Position;
            }
        }
        #endregion

        #region Main Boss Routine
        private IEnumerator BossRoutine()
        {
            // Initial taunt
            Audio.Play("event:/buzzo_laugh", Position);
            yield return 0.5f;
            
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
                
                // Frenzy mode at low health
                if (health <= maxHealth * 0.3f && !isFrenzied && Calc.Random.Chance(0.2f))
                {
                    currentState = BossState.Frenzy;
                    yield return ActivateFrenzy();
                }
                
                if (isFrenzied)
                {
                    // Relentless pursuit in frenzy
                    yield return FrenzyChase(player);
                }
                else if (distToPlayer < 60f)
                {
                    // Close range attack
                    currentState = BossState.Attack;
                    yield return ExecuteAttack(ChooseCloseAttack());
                }
                else if (distToPlayer < 150f)
                {
                    // Medium range - charge or throw
                    currentState = BossState.Attack;
                    yield return ExecuteAttack(Calc.Random.Chance(0.6f) ? AttackType.LungeAttack : AttackType.ThrowDebris);
                }
                else
                {
                    // Chase
                    yield return ChasePlayer(player, 1.5f * aggressionLevel);
                }
                
                currentState = BossState.Idle;
                yield return (0.3f + Calc.Random.Range(0f, 0.4f)) / aggressionLevel;
            }
        }

        private AttackType ChooseCloseAttack()
        {
            float roll = Calc.Random.NextFloat();
            if (roll < 0.5f)
                return AttackType.ChainsawSwipe;
            else if (roll < 0.8f)
                return AttackType.FrenzyMode;
            else
                return AttackType.GrabAttack;
        }

        private IEnumerator ChasePlayer(global::Celeste.Player player, float duration)
        {
            currentState = BossState.Chase;
            bodySprite.Play("run");
            chainsawSprite.Play("rev");
            
            float timer = 0f;
            float speed = 150f * aggressionLevel;
            
            while (timer < duration && !isDefeated)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * speed;
                timer += Engine.DeltaTime;
                yield return null;
            }
            
            Speed = Vector2.Zero;
            bodySprite.Play("idle");
            chainsawSprite.Play("idle");
        }
        #endregion

        #region Attacks
        private IEnumerator ExecuteAttack(AttackType attack)
        {
            bodySprite.Play("attack");
            
            switch (attack)
            {
                case AttackType.ChainsawSwipe:
                    yield return ChainsawSwipeAttack();
                    break;
                case AttackType.LungeAttack:
                    yield return LungeAttackRoutine();
                    break;
                case AttackType.FrenzyMode:
                    yield return FrenzyModeAttack();
                    break;
                case AttackType.ThrowDebris:
                    yield return ThrowDebrisAttack();
                    break;
                case AttackType.GrabAttack:
                    yield return GrabAttackRoutine();
                    break;
            }
            
            bodySprite.Play("idle");
        }

        private IEnumerator ChainsawSwipeAttack()
        {
            chainsawSprite.Play("swing");
            Audio.Play("event:/buzzo_chainsaw_swing", Position);
            
            var level = Scene as Level;
            
            // Wide swipe
            for (int i = 0; i < 3; i++)
            {
                float xOffset = bodySprite.FlipX ? -30f - i * 15f : 30f + i * 15f;
                level?.Displacement.AddBurst(Position + new Vector2(xOffset, -24f), 0.4f, 16f, 40f, 0.3f);
                level?.Shake(0.4f);
                yield return 0.1f;
            }
            
            chainsawSprite.Play("idle");
        }

        private IEnumerator LungeAttackRoutine()
        {
            chainsawSprite.Play("rev");
            Audio.Play("event:/buzzo_lunge", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Wind up
            yield return 0.2f;
            
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * 350f * aggressionLevel;
            }
            
            chainsawSprite.Play("swing");
            
            for (float t = 0; t < 0.25f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.3f, 12f, 32f, 0.2f);
                yield return null;
            }
            
            Speed = Vector2.Zero;
            level?.Shake(0.8f);
            level?.Displacement.AddBurst(Position, 0.6f, 24f, 56f, 0.4f);
            
            chainsawSprite.Play("idle");
        }

        private IEnumerator FrenzyModeAttack()
        {
            chainsawSprite.Play("rev");
            bodySprite.Play("frenzy");
            Audio.Play("event:/buzzo_frenzy", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Rapid slashes
            for (int i = 0; i < 8; i++)
            {
                chainsawSprite.Play("swing");
                
                if (player != null)
                {
                    Vector2 direction = (player.Position - Position).SafeNormalize();
                    Speed = direction * 200f;
                }
                
                float xOffset = bodySprite.FlipX ? -30f : 30f;
                level?.Displacement.AddBurst(Position + new Vector2(xOffset, -24f), 0.3f, 12f, 32f, 0.2f);
                level?.Shake(0.3f);
                
                yield return 0.12f;
            }
            
            Speed = Vector2.Zero;
            chainsawSprite.Play("idle");
            bodySprite.Play("idle");
        }

        private IEnumerator ThrowDebrisAttack()
        {
            Audio.Play("event:/buzzo_throw", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Throw debris at player
            for (int i = 0; i < 3; i++)
            {
                if (player != null)
                {
                    Vector2 throwPos = Position + new Vector2(0f, -24f);
                    level?.Displacement.AddBurst(throwPos, 0.5f, 16f, 40f, 0.3f);
                }
                
                level?.Shake(0.3f);
                yield return 0.3f;
            }
        }

        private IEnumerator GrabAttackRoutine()
        {
            Audio.Play("event:/buzzo_grab", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Quick lunge forward
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * 250f;
            }
            
            yield return 0.15f;
            
            Speed = Vector2.Zero;
            
            // If close enough, "grab" animation with chainsaw follow-up
            if (player != null && Vector2.Distance(Position, player.Position) < 50f)
            {
                chainsawSprite.Play("swing");
                Audio.Play("event:/buzzo_chainsaw_swing", Position);
                
                level?.Shake(1.2f);
                level?.Displacement.AddBurst(Position, 0.8f, 32f, 72f, 0.5f);
            }
            
            chainsawSprite.Play("idle");
        }

        private IEnumerator ActivateFrenzy()
        {
            isFrenzied = true;
            frenzyTimer = 5f;
            aggressionLevel = 2f;
            
            Audio.Play("event:/buzzo_rage", Position);
            bodySprite.Play("frenzy");
            chainsawSprite.Play("rev");
            chainsawLoop.Stop();
            chainsawLoop.Play("event:/buzzo_chainsaw_rev");
            
            var level = Scene as Level;
            level?.Shake(1f);
            
            // Red glow during frenzy
            bodySprite.Color = Color.Lerp(Color.White, Color.Red, 0.3f);
            chainsawGlow.Color = Color.Red;
            
            yield return 0.5f;
        }

        private IEnumerator FrenzyChase(global::Celeste.Player player)
        {
            var level = Scene as Level;
            
            Vector2 direction = (player.Position - Position).SafeNormalize();
            Speed = direction * 250f;
            
            chainsawSprite.Play("swing");
            level?.Displacement.AddBurst(Position, 0.3f, 16f, 40f, 0.25f);
            level?.Shake(0.2f);
            
            yield return 0.1f;
        }
        #endregion

        #region Damage and Defeat
        public override void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            health -= damage;
            Audio.Play("event:/buzzo_hurt", Position);
            
            var level = Scene as Level;
            level?.Shake(0.3f);
            
            bodySprite.Color = Color.Red;
            Add(new Coroutine(FlashReset()));
            
            if (health <= 0)
            {
                Defeat();
            }
        }

        private IEnumerator FlashReset()
        {
            yield return 0.1f;
            if (isFrenzied)
                bodySprite.Color = Color.Lerp(Color.White, Color.Red, 0.3f);
            else
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
            chainsawLoop.Stop();
            
            bodySprite.Play("defeat");
            chainsawSprite.Play("off");
            Audio.Play("event:/buzzo_defeat", Position);
            
            var level = Scene as Level;
            
            // Drop chainsaw
            chainsawSprite.Position += new Vector2(0f, 10f);
            yield return 0.3f;

            // Stumble
            Speed = new Vector2(bodySprite.FlipX ? 50f : -50f, Speed.Y);
            yield return 0.5f;
            Speed = Vector2.Zero;
            
            // Collapse
            level?.Shake(0.8f);
            level?.Displacement.AddBurst(Position, 0.6f, 32f, 64f, 0.4f);
            
            yield return 1f;
            
            level?.Session.SetFlag("buzzo_boss_defeated");
            RemoveSelf();
        }
        #endregion

        #region Cleanup
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            chainsawLoop?.Stop();
        }
        #endregion
    }
}
