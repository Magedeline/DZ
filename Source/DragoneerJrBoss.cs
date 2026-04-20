using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// Dragoneer Jr Mid-Boss - A young dragon rider in training
    /// Aerial mid-boss with fire and lance attacks
    /// Sprite path: characters/dragoneerjr/
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/DragoneerJrBoss")]
    [Tracked]
    public class DragoneerJrBoss : BossActor
    {
        #region Enums and Constants
        public enum AttackType
        {
            FireBreath,
            LanceThrust,
            DiveBomb,
            FireballBarrage,
            TailWhip
        }

        public enum BossState
        {
            Idle,
            Flying,
            Hovering,
            Attack,
            Landing,
            Defeated
        }
        #endregion

        #region Properties
        private int health = 320;
        private int maxHealth = 320;
        private bool isDefeated = false;
        private BossState currentState = BossState.Idle;
        
        private Sprite riderSprite;
        private Sprite dragonSprite;
        private Sprite lanceSprite;
        private Sprite wingsSprite;
        private Sprite flameSprite;
        private VertexLight flameGlow;
        private SoundSource wingLoop;
        
        private bool isAirborne = false;
        private float flyHeight = 0f;
        private float targetFlyHeight = 80f;
        #endregion

        #region Constructors
        public DragoneerJrBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "dragoneer_jr", new Vector2(1.5f, 1.5f), 100f, false, true, 1f, 
                   new Hitbox(48f, 40f, -24f, -40f))
        {
            health = data.Int("health", 320);
            maxHealth = data.Int("maxHealth", 320);
            SetupVisuals();
        }

        public DragoneerJrBoss(Vector2 position) 
            : base(position, "dragoneer_jr", new Vector2(1.5f, 1.5f), 100f, false, true, 1f, 
                   new Hitbox(48f, 40f, -24f, -40f))
        {
            SetupVisuals();
        }
        #endregion

        #region Setup
        private void SetupVisuals()
        {
            // Dragon body sprite
            Add(dragonSprite = new Sprite(GFX.Game, "characters/dragoneerjr/"));
            dragonSprite.AddLoop("idle", "dragon_idle", 0.1f);
            dragonSprite.AddLoop("fly", "dragon_fly", 0.06f);
            dragonSprite.AddLoop("dive", "dragon_dive", 0.04f);
            dragonSprite.AddLoop("attack", "dragon_attack", 0.05f);
            dragonSprite.Add("land", "dragon_land", 0.08f);
            dragonSprite.Add("defeat", "dragon_defeat", 0.1f);
            dragonSprite.Play("idle");
            dragonSprite.CenterOrigin();
            
            // Dragon wings sprite (separate for flapping animation)
            Add(wingsSprite = new Sprite(GFX.Game, "characters/dragoneerjr/"));
            wingsSprite.AddLoop("folded", "wings_folded", 0.12f);
            wingsSprite.AddLoop("flap", "wings_flap", 0.04f);
            wingsSprite.AddLoop("glide", "wings_glide", 0.1f);
            wingsSprite.CenterOrigin();
            wingsSprite.Position = new Vector2(0f, -10f);
            
            // Rider sprite
            Add(riderSprite = new Sprite(GFX.Game, "characters/dragoneerjr/"));
            riderSprite.AddLoop("idle", "rider_idle", 0.12f);
            riderSprite.AddLoop("attack", "rider_attack", 0.05f);
            riderSprite.AddLoop("charging", "rider_charging", 0.08f);
            riderSprite.Add("fall", "rider_fall", 0.1f);
            riderSprite.CenterOrigin();
            riderSprite.Position = new Vector2(0f, -30f);
            
            // Lance sprite
            Add(lanceSprite = new Sprite(GFX.Game, "characters/dragoneerjr/"));
            lanceSprite.AddLoop("idle", "lance_idle", 0.12f);
            lanceSprite.AddLoop("thrust", "lance_thrust", 0.04f);
            lanceSprite.AddLoop("ready", "lance_ready", 0.08f);
            lanceSprite.CenterOrigin();
            lanceSprite.Position = new Vector2(20f, -28f);
            
            // Fire/flame sprite
            Add(flameSprite = new Sprite(GFX.Game, "characters/dragoneerjr/"));
            flameSprite.AddLoop("idle", "flame_idle", 0.06f);
            flameSprite.AddLoop("breath", "flame_breath", 0.03f);
            flameSprite.AddLoop("fireball", "flame_fireball", 0.04f);
            flameSprite.CenterOrigin();
            flameSprite.Position = new Vector2(30f, -5f);
            flameSprite.Visible = false;
            
            // Flame glow
            Add(flameGlow = new VertexLight(Color.Orange, 0.6f, 24, 48));
            flameGlow.Position = new Vector2(30f, -5f);
            flameGlow.Alpha = 0f;
            
            // Wing flapping sound
            Add(wingLoop = new SoundSource());
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
            
            // Update flight offset
            if (isAirborne)
            {
                flyHeight = Calc.Approach(flyHeight, targetFlyHeight, Engine.DeltaTime * 100f);
                
                // Bobbing motion while flying
                float bob = (float)Math.Sin(Scene.TimeActive * 2f) * 5f;
                dragonSprite.Position = new Vector2(0f, -flyHeight + bob);
                wingsSprite.Position = new Vector2(0f, -flyHeight - 10f + bob);
                riderSprite.Position = new Vector2(0f, -flyHeight - 30f + bob);
                lanceSprite.Position = new Vector2(dragonSprite.FlipX ? -20f : 20f, -flyHeight - 28f + bob);
            }
            else
            {
                flyHeight = 0f;
                dragonSprite.Position = Vector2.Zero;
                wingsSprite.Position = new Vector2(0f, -10f);
                riderSprite.Position = new Vector2(0f, -30f);
                lanceSprite.Position = new Vector2(dragonSprite.FlipX ? -20f : 20f, -28f);
            }
            
            // Face player
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && currentState != BossState.Attack)
            {
                dragonSprite.FlipX = player.Position.X < Position.X;
                riderSprite.FlipX = dragonSprite.FlipX;
                wingsSprite.FlipX = dragonSprite.FlipX;
            }
            
            // Flame glow pulsing
            if (flameSprite.Visible)
            {
                flameGlow.Alpha = 0.7f + (float)Math.Sin(Scene.TimeActive * 8f) * 0.3f;
            }
        }
        #endregion

        #region Main Boss Routine
        private IEnumerator BossRoutine()
        {
            // Introduction - take off
            yield return TakeOff();
            
            while (!isDefeated)
            {
                var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player == null)
                {
                    yield return null;
                    continue;
                }
                
                // Fly around
                yield return FlyToPosition(player.Position + new Vector2(Calc.Random.Range(-80f, 80f), -targetFlyHeight));
                
                // Execute attack
                currentState = BossState.Attack;
                yield return ExecuteAttack(ChooseAttack());
                currentState = BossState.Hovering;
                
                yield return 0.8f + Calc.Random.Range(0f, 0.5f);
            }
        }

        private AttackType ChooseAttack()
        {
            return (AttackType)Calc.Random.Next(0, 5);
        }

        private IEnumerator TakeOff()
        {
            Audio.Play("event:/dragoneerjr_roar", Position);
            
            yield return 0.5f;
            
            wingsSprite.Play("flap");
            wingLoop.Play("event:/dragoneerjr_wings");
            dragonSprite.Play("fly");
            
            isAirborne = true;
            currentState = BossState.Flying;
            
            var level = Scene as Level;
            level?.Shake(0.5f);
            
            yield return 1f;
            
            currentState = BossState.Hovering;
        }

        private IEnumerator FlyToPosition(Vector2 targetPos)
        {
            currentState = BossState.Flying;
            wingsSprite.Play("flap");
            
            float moveTime = Vector2.Distance(Position, targetPos) / 200f;
            
            for (float t = 0; t < moveTime && !isDefeated; t += Engine.DeltaTime)
            {
                Vector2 direction = (targetPos - Position).SafeNormalize();
                Speed = direction * 200f;
                yield return null;
            }
            
            Speed = Vector2.Zero;
            wingsSprite.Play("glide");
        }
        #endregion

        #region Attacks
        private IEnumerator ExecuteAttack(AttackType attack)
        {
            dragonSprite.Play("attack");
            riderSprite.Play("attack");
            
            switch (attack)
            {
                case AttackType.FireBreath:
                    yield return FireBreathAttack();
                    break;
                case AttackType.LanceThrust:
                    yield return LanceThrustAttack();
                    break;
                case AttackType.DiveBomb:
                    yield return DiveBombAttack();
                    break;
                case AttackType.FireballBarrage:
                    yield return FireballBarrageAttack();
                    break;
                case AttackType.TailWhip:
                    yield return TailWhipAttack();
                    break;
            }
            
            dragonSprite.Play("fly");
            riderSprite.Play("idle");
        }

        private IEnumerator FireBreathAttack()
        {
            Audio.Play("event:/dragoneerjr_fire_breath", Position);
            flameSprite.Visible = true;
            flameSprite.Play("breath");
            flameGlow.Alpha = 1f;
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Sweeping fire breath
            for (float t = 0; t < 1.5f; t += Engine.DeltaTime)
            {
                if (player != null)
                {
                    Vector2 breathDir = (player.Position - (Position + dragonSprite.Position)).SafeNormalize();
                    flameSprite.Position = dragonSprite.Position + breathDir * 30f;
                    flameGlow.Position = flameSprite.Position;
                    
                    // Fire damage trail
                    for (float dist = 30f; dist < 100f; dist += 20f)
                    {
                        level?.Displacement.AddBurst(Position + dragonSprite.Position + breathDir * dist, 0.3f, 12f, 32f, 0.25f);
                    }
                }
                
                yield return null;
            }
            
            flameSprite.Visible = false;
            flameGlow.Alpha = 0f;
        }

        private IEnumerator LanceThrustAttack()
        {
            lanceSprite.Play("ready");
            riderSprite.Play("charging");
            Audio.Play("event:/dragoneerjr_lance_ready", Position);
            
            yield return 0.3f;
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Quick thrust towards player
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * 400f;
            }
            
            lanceSprite.Play("thrust");
            Audio.Play("event:/dragoneerjr_lance_thrust", Position);
            
            for (float t = 0; t < 0.2f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position + dragonSprite.Position, 0.3f, 16f, 40f, 0.25f);
                yield return null;
            }
            
            Speed = Vector2.Zero;
            level?.Shake(0.8f);
            level?.Displacement.AddBurst(Position + dragonSprite.Position, 0.6f, 24f, 56f, 0.4f);
            
            lanceSprite.Play("idle");
        }

        private IEnumerator DiveBombAttack()
        {
            dragonSprite.Play("dive");
            wingsSprite.Play("folded");
            Audio.Play("event:/dragoneerjr_dive", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Rise up first
            targetFlyHeight = 150f;
            yield return 0.5f;
            
            // Target player and dive
            if (player != null)
            {
                Position = new Vector2(player.Position.X, Position.Y);
            }

            // With these two lines:
            var speed = Speed;
            speed.Y = 500f;
            Speed = speed;

            for (float t = 0; t < 0.6f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position + dragonSprite.Position, 0.4f, 20f, 48f, 0.3f);
                yield return null;
            }
            
            // Pull up
            Speed = Vector2.Zero;
            Audio.Play("event:/dragoneerjr_impact", Position);
            level?.Shake(1.5f);
            
            for (int i = 0; i < 3; i++)
            {
                level?.Displacement.AddBurst(Position, 0.6f, i * 30f, i * 30f + 40f, 0.4f);
            }
            
            // Recover
            targetFlyHeight = 80f;
            wingsSprite.Play("flap");
            dragonSprite.Play("fly");
            
            yield return 0.5f;
        }

        private IEnumerator FireballBarrageAttack()
        {
            Audio.Play("event:/dragoneerjr_fireball", Position);
            flameSprite.Visible = true;
            flameSprite.Play("fireball");
            flameGlow.Alpha = 1f;
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Shoot multiple fireballs
            for (int i = 0; i < 5; i++)
            {
                if (player != null)
                {
                    Vector2 firePos = Position + dragonSprite.Position + new Vector2(dragonSprite.FlipX ? -30f : 30f, 0f);
                    level?.Displacement.AddBurst(firePos, 0.5f, 16f, 40f, 0.35f);
                }
                
                level?.Shake(0.3f);
                yield return 0.2f;
            }
            
            flameSprite.Visible = false;
            flameGlow.Alpha = 0f;
        }

        private IEnumerator TailWhipAttack()
        {
            Audio.Play("event:/dragoneerjr_tail", Position);
            
            var level = Scene as Level;
            
            // Quick spin
            for (int i = 0; i < 2; i++)
            {
                dragonSprite.FlipX = !dragonSprite.FlipX;
                
                float xOffset = dragonSprite.FlipX ? 40f : -40f;
                level?.Displacement.AddBurst(Position + dragonSprite.Position + new Vector2(xOffset, 0f), 0.5f, 20f, 48f, 0.35f);
                level?.Shake(0.4f);
                
                yield return 0.15f;
            }
        }
        #endregion

        #region Damage and Defeat
        public override void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            health -= damage;
            Audio.Play("event:/dragoneerjr_hurt", Position);
            
            var level = Scene as Level;
            level?.Shake(0.3f);
            
            dragonSprite.Color = Color.Red;
            riderSprite.Color = Color.Red;
            Add(new Coroutine(FlashReset()));
            
            if (health <= 0)
            {
                Defeat();
            }
        }

        private IEnumerator FlashReset()
        {
            yield return 0.1f;
            dragonSprite.Color = Color.White;
            riderSprite.Color = Color.White;
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
            wingLoop.Stop();
            
            dragonSprite.Play("defeat");
            riderSprite.Play("fall");
            wingsSprite.Play("folded");
            Audio.Play("event:/dragoneerjr_defeat", Position);
            
            var level = Scene as Level;

            // With these two lines:
            var speed = Speed;
            speed.Y = 300f;
            Speed = speed;

            while (flyHeight > 0f || !Grounded)
            {
                flyHeight = Math.Max(0f, flyHeight - Engine.DeltaTime * 200f);
                yield return null;
            }
            
            Speed = Vector2.Zero;
            
            // Impact
            Audio.Play("event:/dragoneerjr_crash", Position);
            level?.Shake(1.5f);
            level?.Displacement.AddBurst(Position, 1f, 48f, 96f, 0.6f);
            
            // Smoke/dust
            for (int i = 0; i < 4; i++)
            {
                level?.Displacement.AddBurst(Position + Calc.Random.Range(Vector2.One * -20f, Vector2.One * 20f), 0.5f, 20f, 40f, 0.3f);
                yield return 0.2f;
            }
            
            yield return 1f;
            
            level?.Session.SetFlag("dragoneer_jr_defeated");
            RemoveSelf();
        }
        #endregion

        #region Cleanup
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            wingLoop?.Stop();
        }
        #endregion
    }
}
