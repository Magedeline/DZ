using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// Frosty Mid-Boss - An icy snowman menace
    /// Cold-themed mid-boss with freezing attacks
    /// Sprite path: characters/frosty/
    /// </summary>
    [CustomEntity(ids: "DZ/FrostyBoss")]
    [Tracked]
    public class FrostyBoss : BossActor
    {
        #region Enums and Constants
        public enum AttackType
        {
            IceBreath,
            SnowballBarrage,
            FreezeSlam,
            BlizzardSpin,
            IcicleRain
        }

        public enum BossState
        {
            Idle,
            Attack,
            Moving,
            Reforming,
            Defeated
        }
        #endregion

        #region Properties
        private int health = 250;
        private int maxHealth = 250;
        private bool isDefeated = false;
#pragma warning disable CS0414
        private BossState currentState = BossState.Idle;
#pragma warning restore CS0414
        
        private Sprite bodySprite;
        private Sprite hatSprite;
        private Sprite armsSprite;
        private VertexLight frostGlow;
        private SoundSource blizzardLoop;
        
        private bool isReforming = false;
        private float reformProgress = 0f;
        private int reformCount = 0;
        private const int MaxReforms = 2;
        #endregion

        #region Constructors
        public FrostyBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "frosty_boss", new Vector2(1.5f, 1.5f), 70f, true, true, 1f, 
                   new Hitbox(40f, 56f, -20f, -56f))
        {
            health = data.Int("health", 250);
            maxHealth = data.Int("maxHealth", 250);
            SetupVisuals();
        }

        public FrostyBoss(Vector2 position) 
            : base(position, "frosty_boss", new Vector2(1.5f, 1.5f), 70f, true, true, 1f, 
                   new Hitbox(40f, 56f, -20f, -56f))
        {
            SetupVisuals();
        }
        #endregion

        #region Setup
        private void SetupVisuals()
        {
            // Body sprite (main snowman)
            Add(bodySprite = new Sprite(GFX.Game, "characters/frosty/"));
            bodySprite.AddLoop("idle", "body_idle", 0.12f);
            bodySprite.AddLoop("laugh", "body_laugh", 0.08f);
            bodySprite.AddLoop("attack", "body_attack", 0.06f);
            bodySprite.AddLoop("melting", "body_melting", 0.1f);
            bodySprite.Add("reform", "body_reform", 0.08f);
            bodySprite.Add("defeat", "body_defeat", 0.15f);
            bodySprite.Play("idle");
            bodySprite.CenterOrigin();
            
            // Hat sprite
            Add(hatSprite = new Sprite(GFX.Game, "characters/frosty/"));
            hatSprite.AddLoop("idle", "hat_idle", 0.15f);
            hatSprite.AddLoop("wobble", "hat_wobble", 0.08f);
            hatSprite.CenterOrigin();
            hatSprite.Position = new Vector2(0f, -52f);
            
            // Arms (branch arms)
            Add(armsSprite = new Sprite(GFX.Game, "characters/frosty/"));
            armsSprite.AddLoop("idle", "arms_idle", 0.12f);
            armsSprite.AddLoop("attack", "arms_attack", 0.06f);
            armsSprite.AddLoop("wave", "arms_wave", 0.08f);
            armsSprite.CenterOrigin();
            armsSprite.Position = new Vector2(0f, -28f);
            
            // Frost glow
            Add(frostGlow = new VertexLight(Color.Cyan, 0.8f, 32, 56));
            frostGlow.Position = new Vector2(0f, -30f);
            
            // Blizzard sound
            Add(blizzardLoop = new SoundSource());
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
            
            // Frost pulsing
            frostGlow.Alpha = 0.7f + (float)Math.Sin(Scene.TimeActive * 2f) * 0.2f;
            
            // Hat bobbing
            hatSprite.Position = new Vector2(0f, -52f + (float)Math.Sin(Scene.TimeActive * 1.5f) * 2f);
            
            // Reform progress effect
            if (isReforming)
            {
                bodySprite.Color = Color.Lerp(Color.White * 0.3f, Color.White, reformProgress);
            }
        }
        #endregion

        #region Main Boss Routine
        private IEnumerator BossRoutine()
        {
            bodySprite.Play("laugh");
            Audio.Play("event:/frosty_laugh", Position);
            yield return 1f;
            bodySprite.Play("idle");
            
            while (!isDefeated)
            {
                if (isReforming)
                {
                    yield return ReformRoutine();
                    continue;
                }
                
                var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player == null)
                {
                    yield return null;
                    continue;
                }
                
                // Random movement
                if (Calc.Random.Chance(0.3f))
                {
                    yield return HopMovement(player);
                }
                
                // Execute random attack
                currentState = BossState.Attack;
                yield return ExecuteAttack(ChooseAttack());
                currentState = BossState.Idle;
                
                yield return 1f + Calc.Random.Range(0f, 1f);
            }
        }

        private AttackType ChooseAttack()
        {
            return (AttackType)Calc.Random.Next(0, 5);
        }

        private IEnumerator HopMovement(global::Celeste.Player player)
        {
            currentState = BossState.Moving;
            Audio.Play("event:/frosty_hop", Position);
            
            Vector2 direction = (player.Position - Position).SafeNormalize();
            
            // Hop towards player
            Speed = direction * 100f + new Vector2(0f, -200f);
            
            yield return 0.3f;
            
            while (!Grounded)
            {
                yield return null;
            }
            
            Speed = Vector2.Zero;
            
            var level = Scene as Level;
            level?.Shake(0.3f);
            
            // Snow poof on landing
            level?.Displacement.AddBurst(Position, 0.3f, 16f, 32f, 0.2f);
        }
        #endregion

        #region Attacks
        private IEnumerator ExecuteAttack(AttackType attack)
        {
            bodySprite.Play("attack");
            armsSprite.Play("attack");
            
            switch (attack)
            {
                case AttackType.IceBreath:
                    yield return IceBreathAttack();
                    break;
                case AttackType.SnowballBarrage:
                    yield return SnowballBarrageAttack();
                    break;
                case AttackType.FreezeSlam:
                    yield return FreezeSlamAttack();
                    break;
                case AttackType.BlizzardSpin:
                    yield return BlizzardSpinAttack();
                    break;
                case AttackType.IcicleRain:
                    yield return IcicleRainAttack();
                    break;
            }
            
            bodySprite.Play("idle");
            armsSprite.Play("idle");
        }

        private IEnumerator IceBreathAttack()
        {
            Audio.Play("event:/frosty_ice_breath", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                
                // Cone of ice breath
                for (int i = 0; i < 12; i++)
                {
                    float spread = (i - 6) * 0.1f;
                    Vector2 particleDir = Calc.Rotate(direction, spread);
                    
                    for (float dist = 20f; dist < 120f; dist += 20f)
                    {
                        Vector2 pos = Position + particleDir * dist + new Vector2(0f, -30f);
                        level?.Displacement.AddBurst(pos, 0.3f, 12f, 28f, 0.2f);
                    }
                    
                    yield return 0.05f;
                }
            }
        }

        private IEnumerator SnowballBarrageAttack()
        {
            Audio.Play("event:/frosty_snowball", Position);
            armsSprite.Play("wave");
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Throw multiple snowballs
            for (int i = 0; i < 5; i++)
            {
                if (player != null)
                {
                    Vector2 throwPos = Position + new Vector2(0f, -30f);
                    level?.Displacement.AddBurst(throwPos, 0.4f, 16f, 40f, 0.3f);
                }
                
                level?.Shake(0.2f);
                yield return 0.25f;
            }
        }

        private IEnumerator FreezeSlamAttack()
        {
            Audio.Play("event:/frosty_slam", Position);

            // Jump up
            var speed = Speed;
            speed.Y = -250f;
            Speed = speed;

            yield return 0.3f;

            // Come down
            speed = Speed;
            speed.Y = 400f;
            Speed = speed;

            while (!Grounded)
            {
                yield return null;
            }

            Speed = Vector2.Zero;

            var level = Scene as Level;
            level?.Shake(1f);

            // Freezing shockwave
            for (int i = 0; i < 4; i++)
            {
                level?.Displacement.AddBurst(Position, 0.5f, i * 25f, i * 25f + 30f, 0.4f);
            }

            Audio.Play("event:/frosty_freeze", Position);
        }

        private IEnumerator BlizzardSpinAttack()
        {
            blizzardLoop.Play("event:/frosty_blizzard");
            hatSprite.Play("wobble");
            
            var level = Scene as Level;
            
            // Spin creating blizzard
            for (float t = 0; t < 2f; t += Engine.DeltaTime)
            {
                // Spawn ice particles in spiral
                for (int i = 0; i < 3; i++)
                {
                    float angle = t * 8f + (i / 3f) * MathHelper.TwoPi;
                    float radius = 30f + t * 30f;
                    Vector2 pos = Position + Calc.AngleToVector(angle, radius) + new Vector2(0f, -30f);
                    level?.Displacement.AddBurst(pos, 0.3f, 12f, 28f, 0.2f);
                }
                
                yield return null;
            }
            
            blizzardLoop.Stop();
            hatSprite.Play("idle");
            level?.Shake(0.5f);
        }

        private IEnumerator IcicleRainAttack()
        {
            Audio.Play("event:/frosty_icicle", Position);
            
            var level = Scene as Level;
            
            // Icicles fall from above
            for (int i = 0; i < 8; i++)
            {
                float xOffset = Calc.Random.Range(-100f, 100f);
                Vector2 iciclePos = Position + new Vector2(xOffset, -150f);
                
                // Icicle falls
                for (float y = iciclePos.Y; y < Position.Y + 50f; y += 20f)
                {
                    level?.Displacement.AddBurst(new Vector2(iciclePos.X, y), 0.3f, 8f, 24f, 0.2f);
                    yield return 0.02f;
                }
                
                level?.Shake(0.3f);
            }
        }
        #endregion

        #region Reform Mechanic
        private IEnumerator ReformRoutine()
        {
            currentState = BossState.Reforming;
            Audio.Play("event:/frosty_reform", Position);
            bodySprite.Play("reform");
            
            var level = Scene as Level;
            
            reformProgress = 0f;
            
            // Reform animation
            for (reformProgress = 0f; reformProgress < 1f; reformProgress += Engine.DeltaTime * 0.5f)
            {
                level?.Displacement.AddBurst(Position, 0.3f, 20f, 10f, 0.2f);
                yield return null;
            }
            
            reformProgress = 1f;
            isReforming = false;
            bodySprite.Color = Color.White;
            bodySprite.Play("laugh");
            Audio.Play("event:/frosty_laugh", Position);
            
            yield return 0.5f;
            
            bodySprite.Play("idle");
            currentState = BossState.Idle;
        }
        #endregion

        #region Damage and Defeat
        public override void TakeDamage(int damage)
        {
            if (isDefeated || isReforming) return;
            
            health -= damage;
            Audio.Play("event:/frosty_hurt", Position);
            
            var level = Scene as Level;
            level?.Shake(0.3f);
            
            bodySprite.Color = Color.LightBlue;
            Add(new Coroutine(FlashReset()));
            
            // Check for reform
            if (health <= 0)
            {
                if (reformCount < MaxReforms)
                {
                    reformCount++;
                    health = maxHealth / 2;
                    isReforming = true;
                    bodySprite.Play("melting");
                    Audio.Play("event:/frosty_melting", Position);
                }
                else
                {
                    Defeat();
                }
            }
        }

        private IEnumerator FlashReset()
        {
            yield return 0.1f;
            if (!isReforming)
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
            blizzardLoop.Stop();
            
            bodySprite.Play("defeat");
            hatSprite.Visible = false;
            Audio.Play("event:/frosty_defeat", Position);
            
            var level = Scene as Level;
            
            // Melting sequence
            for (int i = 0; i < 6; i++)
            {
                level?.Displacement.AddBurst(Position + new Vector2(0f, -20f + i * 5f), 0.4f, 20f, 40f, 0.3f);
                bodySprite.Scale.Y -= 0.1f;
                yield return 0.3f;
            }
            
            // Final puddle splash
            level?.Shake(0.5f);
            level?.Displacement.AddBurst(Position, 0.8f, 40f, 80f, 0.5f);
            
            yield return 1f;
            
            level?.Session.SetFlag("frosty_boss_defeated");
            RemoveSelf();
        }
        #endregion

        #region Cleanup
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            blizzardLoop?.Stop();
        }
        #endregion
    }
}
