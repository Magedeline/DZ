using MaggyHelper.Helpers;

namespace MaggyHelper.Entities.Bosses
{
    /// <summary>
    /// Mockby Mid-Boss - A mischievous prankster enemy
    /// Trickster mid-boss with deceptive attacks and illusions
    /// Sprite path: characters/mockby/
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/MockbyBoss")]
    [Tracked]
    [HotReloadable]
    public class MockbyBoss : BossActor
    {
        #region Enums and Constants
        public enum AttackType
        {
            MockingLaugh,
            IllusionClones,
            TricksterDash,
            ConfusionBomb,
            MirrorSwap
        }

        public enum BossState
        {
            Idle,
            Taunting,
            Attack,
            Invisible,
            Defeated
        }
        #endregion

        #region Properties
        private int health = 220;
        private int maxHealth = 220;
        private bool isDefeated = false;
        private BossState currentState = BossState.Idle;
        
        private Sprite bodySprite;
        private Sprite faceSprite;
        private Sprite effectSprite;
        private VertexLight prankGlow;
        private SoundSource laughLoop;
        
        private bool isInvisible = false;
        private float invisTimer = 0f;
        private List<Vector2> clonePositions = new List<Vector2>();
        private int realCloneIndex = -1;
        #endregion

        #region Constructors
        public MockbyBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "mockby_boss", new Vector2(1f, 1f), 150f, true, true, 1f, 
                   new Hitbox(28f, 40f, -14f, -40f))
        {
            health = data.Int("health", 220);
            maxHealth = data.Int("maxHealth", 220);
            SetupVisuals();
        }

        public MockbyBoss(Vector2 position) 
            : base(position, "mockby_boss", new Vector2(1f, 1f), 150f, true, true, 1f, 
                   new Hitbox(28f, 40f, -14f, -40f))
        {
            SetupVisuals();
        }
        #endregion

        #region Setup
        private void SetupVisuals()
        {
            // Body sprite
            Add(bodySprite = new Sprite(GFX.Game, "characters/mockby/"));
            bodySprite.AddLoop("idle", "body_idle", 0.1f);
            bodySprite.AddLoop("laugh", "body_laugh", 0.05f);
            bodySprite.AddLoop("sneak", "body_sneak", 0.08f);
            bodySprite.AddLoop("attack", "body_attack", 0.06f);
            bodySprite.AddLoop("fade", "body_fade", 0.04f);
            bodySprite.Add("defeat", "body_defeat", 0.1f);
            bodySprite.Play("idle");
            bodySprite.CenterOrigin();
            
            // Expressive face sprite (overlays body)
            Add(faceSprite = new Sprite(GFX.Game, "characters/mockby/"));
            faceSprite.AddLoop("normal", "face_normal", 0.12f);
            faceSprite.AddLoop("laugh", "face_laugh", 0.04f);
            faceSprite.AddLoop("smirk", "face_smirk", 0.1f);
            faceSprite.AddLoop("surprise", "face_surprise", 0.08f);
            faceSprite.CenterOrigin();
            faceSprite.Position = new Vector2(0f, -28f);
            
            // Special effects sprite
            Add(effectSprite = new Sprite(GFX.Game, "characters/mockby/"));
            effectSprite.AddLoop("sparkle", "effect_sparkle", 0.05f);
            effectSprite.AddLoop("smoke", "effect_smoke", 0.06f);
            effectSprite.AddLoop("question", "effect_question", 0.08f);
            effectSprite.CenterOrigin();
            effectSprite.Position = new Vector2(0f, -50f);
            effectSprite.Visible = false;
            
            // Prank glow (purple/pink mischief color)
            Add(prankGlow = new VertexLight(Color.Magenta, 0.5f, 20, 40));
            prankGlow.Position = new Vector2(0f, -20f);
            
            // Laugh sound
            Add(laughLoop = new SoundSource());
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
            
            // Prank glow pulsing
            prankGlow.Alpha = 0.4f + (float)Math.Sin(Scene.TimeActive * 3f) * 0.3f;
            
            // Invisibility
            if (isInvisible)
            {
                invisTimer -= Engine.DeltaTime;
                bodySprite.Color = Color.White * 0.2f;
                faceSprite.Color = Color.White * 0.2f;
                Collidable = false;
                
                if (invisTimer <= 0f)
                {
                    isInvisible = false;
                    bodySprite.Color = Color.White;
                    faceSprite.Color = Color.White;
                    Collidable = true;
                }
            }
            
            // Face player
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && currentState != BossState.Attack)
            {
                bodySprite.FlipX = player.Position.X < Position.X;
                faceSprite.FlipX = bodySprite.FlipX;
            }
        }
        #endregion

        #region Main Boss Routine
        private IEnumerator BossRoutine()
        {
            // Intro taunt
            currentState = BossState.Taunting;
            faceSprite.Play("laugh");
            bodySprite.Play("laugh");
            laughLoop.Play("event:/mockby_laugh");
            
            yield return 1f;
            
            faceSprite.Play("smirk");
            bodySprite.Play("idle");
            laughLoop.Stop();
            
            while (!isDefeated)
            {
                var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player == null)
                {
                    yield return null;
                    continue;
                }
                
                // Random taunting
                if (Calc.Random.Chance(0.15f))
                {
                    yield return QuickTaunt();
                }
                
                // Execute attack
                currentState = BossState.Attack;
                yield return ExecuteAttack(ChooseAttack());
                currentState = BossState.Idle;
                
                yield return 1f + Calc.Random.Range(0f, 0.8f);
            }
        }

        private AttackType ChooseAttack()
        {
            return (AttackType)Calc.Random.Next(0, 5);
        }

        private IEnumerator QuickTaunt()
        {
            currentState = BossState.Taunting;
            faceSprite.Play("laugh");
            bodySprite.Play("laugh");
            Audio.Play("event:/mockby_taunt", Position);
            
            yield return 0.5f;
            
            faceSprite.Play("smirk");
            bodySprite.Play("idle");
        }
        #endregion

        #region Attacks
        private IEnumerator ExecuteAttack(AttackType attack)
        {
            bodySprite.Play("attack");
            
            switch (attack)
            {
                case AttackType.MockingLaugh:
                    yield return MockingLaughAttack();
                    break;
                case AttackType.IllusionClones:
                    yield return IllusionClonesAttack();
                    break;
                case AttackType.TricksterDash:
                    yield return TricksterDashAttack();
                    break;
                case AttackType.ConfusionBomb:
                    yield return ConfusionBombAttack();
                    break;
                case AttackType.MirrorSwap:
                    yield return MirrorSwapAttack();
                    break;
            }
            
            bodySprite.Play("idle");
            faceSprite.Play("smirk");
        }

        private IEnumerator MockingLaughAttack()
        {
            faceSprite.Play("laugh");
            laughLoop.Play("event:/mockby_mocking_laugh");
            
            var level = Scene as Level;
            
            // Damaging sound waves
            for (int wave = 0; wave < 4; wave++)
            {
                level?.Displacement.AddBurst(Position, 0.5f, wave * 30f, wave * 30f + 40f, 0.4f);
                level?.Shake(0.3f);
                yield return 0.25f;
            }
            
            laughLoop.Stop();
        }

        private IEnumerator IllusionClonesAttack()
        {
            Audio.Play("event:/mockby_clone", Position);
            effectSprite.Visible = true;
            effectSprite.Play("smoke");
            
            var level = Scene as Level;
            
            // Create illusion clones
            clonePositions.Clear();
            int numClones = 4;
            realCloneIndex = Calc.Random.Next(numClones);
            
            Vector2 originalPos = Position;
            
            // Spawn smoke at original position
            level?.Displacement.AddBurst(Position, 0.6f, 32f, 64f, 0.4f);
            
            // Create fake positions
            for (int i = 0; i < numClones; i++)
            {
                Vector2 clonePos = originalPos + new Vector2((i - numClones / 2f) * 60f, 0f);
                clonePositions.Add(clonePos);
                level?.Displacement.AddBurst(clonePos, 0.5f, 24f, 48f, 0.3f);
            }
            
            // Teleport to real position
            Position = clonePositions[realCloneIndex];
            
            yield return 1.5f;
            
            // All clones "attack"
            for (int i = 0; i < clonePositions.Count; i++)
            {
                level?.Displacement.AddBurst(clonePositions[i], 0.4f, 16f, 40f, 0.3f);
            }
            
            level?.Shake(0.5f);
            
            effectSprite.Visible = false;
            clonePositions.Clear();
        }

        private IEnumerator TricksterDashAttack()
        {
            bodySprite.Play("sneak");
            Audio.Play("event:/mockby_dash", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Quick zigzag dashes
            for (int i = 0; i < 3; i++)
            {
                if (player != null)
                {
                    Vector2 baseDir = (player.Position - Position).SafeNormalize();
                    Vector2 perpendicular = new Vector2(-baseDir.Y, baseDir.X);
                    Vector2 dashDir = baseDir + perpendicular * (i % 2 == 0 ? 0.5f : -0.5f);
                    dashDir.Normalize();
                    
                    Speed = dashDir * 350f;
                }
                
                level?.Displacement.AddBurst(Position, 0.4f, 16f, 40f, 0.25f);
                
                yield return 0.15f;
                
                Speed = Vector2.Zero;
            }
            
            // Final attack at player
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * 400f;
            }
            
            level?.Displacement.AddBurst(Position, 0.5f, 24f, 56f, 0.35f);
            
            yield return 0.2f;
            
            Speed = Vector2.Zero;
            level?.Shake(0.6f);
        }

        private IEnumerator ConfusionBombAttack()
        {
            effectSprite.Visible = true;
            effectSprite.Play("question");
            Audio.Play("event:/mockby_confusion", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Throw confusion bombs
            for (int i = 0; i < 3; i++)
            {
                Vector2 bombTarget;
                if (player != null)
                {
                    bombTarget = player.Position + Calc.Random.Range(Vector2.One * -40f, Vector2.One * 40f);
                }
                else
                {
                    bombTarget = Position + Calc.Random.Range(Vector2.One * -80f, Vector2.One * 80f);
                }
                
                // Arc to target
                level?.Displacement.AddBurst(Position, 0.3f, 12f, 28f, 0.2f);
                
                yield return 0.15f;
                
                // Explosion at target
                level?.Displacement.AddBurst(bombTarget, 0.6f, 32f, 72f, 0.5f);
                level?.Shake(0.4f);
                
                yield return 0.2f;
            }
            
            effectSprite.Visible = false;
        }

        private IEnumerator MirrorSwapAttack()
        {
            faceSprite.Play("surprise");
            Audio.Play("event:/mockby_swap", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Brief invisibility
            isInvisible = true;
            invisTimer = 0.3f;
            
            level?.Displacement.AddBurst(Position, 0.6f, 40f, 80f, 0.4f);
            
            yield return 0.3f;
            
            // Teleport to opposite side of player
            if (player != null)
            {
                Vector2 offset = Position - player.Position;
                Position = player.Position - offset;
            }
            
            // Reappear
            isInvisible = false;
            invisTimer = 0f;
            bodySprite.Color = Color.White;
            faceSprite.Color = Color.White;
            Collidable = true;
            
            level?.Displacement.AddBurst(Position, 0.6f, 40f, 80f, 0.4f);
            Audio.Play("event:/mockby_appear", Position);
            
            yield return 0.2f;
            
            // Surprise attack
            bodySprite.Play("attack");
            level?.Displacement.AddBurst(Position, 0.5f, 24f, 56f, 0.35f);
            level?.Shake(0.5f);
        }
        #endregion

        #region Damage and Defeat
        public override void TakeDamage(int damage)
        {
            if (isDefeated || isInvisible) return;
            
            health -= damage;
            Audio.Play("event:/mockby_hurt", Position);
            
            var level = Scene as Level;
            level?.Shake(0.3f);
            
            faceSprite.Play("surprise");
            bodySprite.Color = Color.Magenta;
            Add(new Coroutine(FlashReset()));
            
            // Chance to become briefly invisible when hurt
            if (Calc.Random.Chance(0.3f) && health > 0)
            {
                isInvisible = true;
                invisTimer = 1f;
                Audio.Play("event:/mockby_escape", Position);
            }
            
            if (health <= 0)
            {
                Defeat();
            }
        }

        private IEnumerator FlashReset()
        {
            yield return 0.1f;
            if (!isInvisible)
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
            laughLoop.Stop();
            isInvisible = false;
            bodySprite.Color = Color.White;
            faceSprite.Color = Color.White;
            Collidable = true;
            
            faceSprite.Play("surprise");
            bodySprite.Play("defeat");
            Audio.Play("event:/mockby_defeat", Position);
            
            var level = Scene as Level;
            
            // Pranks backfire - gets hit by own effects
            effectSprite.Visible = true;
            effectSprite.Play("sparkle");
            
            for (int i = 0; i < 5; i++)
            {
                Vector2 effectPos = Position + Calc.Random.Range(Vector2.One * -20f, Vector2.One * 20f);
                level?.Displacement.AddBurst(effectPos, 0.4f, 16f, 40f, 0.3f);
                level?.Shake(0.2f);
                yield return 0.2f;
            }
            
            // Final poof
            level?.Shake(0.8f);
            level?.Displacement.AddBurst(Position, 0.8f, 48f, 96f, 0.6f);
            
            yield return 1f;
            
            level?.Session.SetFlag("mockby_boss_defeated");
            RemoveSelf();
        }
        #endregion

        #region Cleanup
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            laughLoop?.Stop();
        }
        #endregion
    }
}
