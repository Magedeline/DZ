using MaggyHelper.Helpers;

namespace MaggyHelper.Entities.Bosses
{
    /// <summary>
    /// Embryo Mid-Boss - A mysterious gestating creature
    /// Eldritch mid-boss with psychic and body horror attacks
    /// Sprite path: characters/embryo/
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/EmbryoBoss")]
    [Tracked]
    public class EmbryoBoss : BossActor
    {
        #region Enums and Constants
        public enum AttackType
        {
            PsychicPulse,
            TendrilGrab,
            WombShield,
            BirthSpawn,
            MindScream
        }

        public enum BossState
        {
            Dormant,
            Awakening,
            Active,
            Gestating,
            Defeated
        }

        public enum GrowthStage
        {
            Embryonic,
            Fetal,
            Newborn
        }
        #endregion

        #region Properties
        private int health = 400;
        private int maxHealth = 400;
        private bool isDefeated = false;
        private BossState currentState = BossState.Dormant;
        private GrowthStage growthStage = GrowthStage.Embryonic;
        
        private Sprite bodySprite;
        private Sprite membraneSprite;
        private Sprite eyeSprite;
        private Sprite tendrilSprite;
        private VertexLight psychicGlow;
        private VertexLight eyeGlow;
        private SoundSource heartbeatLoop;
        private SoundSource whisperLoop;
        
        private bool isGestating = false;
        private float gestationProgress = 0f;
        private float pulseTimer = 0f;
        #endregion

        #region Constructors
        public EmbryoBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "embryo_boss", new Vector2(1.2f, 1.2f), 50f, false, true, 1f, 
                   new Hitbox(36f, 48f, -18f, -24f))
        {
            health = data.Int("health", 400);
            maxHealth = data.Int("maxHealth", 400);
            SetupVisuals();
        }

        public EmbryoBoss(Vector2 position) 
            : base(position, "embryo_boss", new Vector2(1.2f, 1.2f), 50f, false, true, 1f, 
                   new Hitbox(36f, 48f, -18f, -24f))
        {
            SetupVisuals();
        }
        #endregion

        #region Setup
        private void SetupVisuals()
        {
            // Membrane/sac sprite (outer layer)
            Add(membraneSprite = new Sprite(GFX.Game, "characters/embryo/"));
            membraneSprite.AddLoop("intact", "membrane_intact", 0.15f);
            membraneSprite.AddLoop("pulse", "membrane_pulse", 0.08f);
            membraneSprite.AddLoop("cracked", "membrane_cracked", 0.1f);
            membraneSprite.Add("break", "membrane_break", 0.06f);
            membraneSprite.Play("intact");
            membraneSprite.CenterOrigin();
            membraneSprite.Color = Color.White * 0.7f;
            
            // Body sprite (creature inside)
            Add(bodySprite = new Sprite(GFX.Game, "characters/embryo/"));
            bodySprite.AddLoop("dormant", "body_dormant", 0.2f);
            bodySprite.AddLoop("stir", "body_stir", 0.1f);
            bodySprite.AddLoop("awake", "body_awake", 0.08f);
            bodySprite.AddLoop("attack", "body_attack", 0.05f);
            bodySprite.AddLoop("grow", "body_grow", 0.06f);
            bodySprite.Add("defeat", "body_defeat", 0.1f);
            bodySprite.Play("dormant");
            bodySprite.CenterOrigin();
            
            // Eye sprite (single large eye)
            Add(eyeSprite = new Sprite(GFX.Game, "characters/embryo/"));
            eyeSprite.AddLoop("closed", "eye_closed", 0.2f);
            eyeSprite.AddLoop("opening", "eye_opening", 0.1f);
            eyeSprite.AddLoop("open", "eye_open", 0.12f);
            eyeSprite.AddLoop("stare", "eye_stare", 0.08f);
            eyeSprite.AddLoop("psychic", "eye_psychic", 0.04f);
            eyeSprite.Play("closed");
            eyeSprite.CenterOrigin();
            eyeSprite.Position = new Vector2(0f, -8f);
            
            // Tendril sprite
            Add(tendrilSprite = new Sprite(GFX.Game, "characters/embryo/"));
            tendrilSprite.AddLoop("hidden", "tendril_hidden", 0.15f);
            tendrilSprite.AddLoop("emerge", "tendril_emerge", 0.06f);
            tendrilSprite.AddLoop("grab", "tendril_grab", 0.04f);
            tendrilSprite.AddLoop("retract", "tendril_retract", 0.06f);
            tendrilSprite.CenterOrigin();
            tendrilSprite.Position = new Vector2(0f, 10f);
            tendrilSprite.Visible = false;
            
            // Psychic glow (purple/pink)
            Add(psychicGlow = new VertexLight(new Color(200, 100, 200), 0.4f, 40, 70));
            psychicGlow.Position = new Vector2(0f, 0f);
            
            // Eye glow
            Add(eyeGlow = new VertexLight(Color.Red, 0.6f, 16, 32));
            eyeGlow.Position = new Vector2(0f, -8f);
            eyeGlow.Alpha = 0f;
            
            // Sound sources
            Add(heartbeatLoop = new SoundSource());
            Add(whisperLoop = new SoundSource());
        }
        #endregion

        #region Scene Management
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            heartbeatLoop.Play("event:/embryo_heartbeat");
            
            Add(new Coroutine(BossRoutine()));
        }

        public override void Update()
        {
            base.Update();
            
            // Heartbeat pulse effect
            pulseTimer += Engine.DeltaTime;
            float pulse = (float)Math.Sin(pulseTimer * 2f) * 0.5f + 0.5f;
            psychicGlow.Alpha = 0.3f + pulse * 0.4f;
            
            // Membrane pulsing
            if (currentState != BossState.Dormant)
            {
                membraneSprite.Scale = Vector2.One * (1f + pulse * 0.05f);
            }
            
            // Eye tracking player
            if (currentState == BossState.Active || currentState == BossState.Gestating)
            {
                var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null)
                {
                    Vector2 toPlayer = (player.Position - Position).SafeNormalize();
                    eyeSprite.Position = new Vector2(toPlayer.X * 3f, -8f + toPlayer.Y * 2f);
                }
            }
            
            // Growth stage effects
            UpdateGrowthEffects();
        }

        private void UpdateGrowthEffects()
        {
            switch (growthStage)
            {
                case GrowthStage.Embryonic:
                    bodySprite.Scale = new Vector2(0.8f, 0.8f);
                    break;
                case GrowthStage.Fetal:
                    bodySprite.Scale = new Vector2(1f, 1f);
                    membraneSprite.Play("cracked");
                    break;
                case GrowthStage.Newborn:
                    bodySprite.Scale = new Vector2(1.2f, 1.2f);
                    membraneSprite.Visible = false;
                    break;
            }
        }
        #endregion

        #region Main Boss Routine
        private IEnumerator BossRoutine()
        {
            // Wait dormant
            yield return 1f;
            
            // Awakening
            yield return AwakeningSequence();
            
            while (!isDefeated)
            {
                if (isGestating)
                {
                    yield return GestationRoutine();
                    continue;
                }
                
                var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player == null)
                {
                    yield return null;
                    continue;
                }
                
                // Check for growth stage transition
                CheckGrowthTransition();
                
                // Float towards player slowly
                yield return FloatTowardsPlayer(player, 1f);
                
                // Execute attack
                currentState = BossState.Active;
                yield return ExecuteAttack(ChooseAttack());
                
                yield return 1.2f + Calc.Random.Range(0f, 0.8f);
            }
        }

        private IEnumerator AwakeningSequence()
        {
            currentState = BossState.Awakening;
            
            bodySprite.Play("stir");
            membraneSprite.Play("pulse");
            Audio.Play("event:/embryo_stir", Position);
            
            yield return 1f;
            
            // Eye opens
            eyeSprite.Play("opening");
            Audio.Play("event:/embryo_eye_open", Position);
            
            yield return 0.5f;
            
            eyeSprite.Play("open");
            eyeGlow.Alpha = 0.6f;
            
            whisperLoop.Play("event:/embryo_whispers");
            
            bodySprite.Play("awake");
            currentState = BossState.Active;
            
            var level = Scene as Level;
            level?.Shake(0.5f);
        }

        private void CheckGrowthTransition()
        {
            float healthPercent = (float)health / maxHealth;
            
            if (healthPercent <= 0.33f && growthStage == GrowthStage.Fetal)
            {
                growthStage = GrowthStage.Newborn;
                Add(new Coroutine(GrowthTransition()));
            }
            else if (healthPercent <= 0.66f && growthStage == GrowthStage.Embryonic)
            {
                growthStage = GrowthStage.Fetal;
                isGestating = true;
            }
        }

        private IEnumerator GrowthTransition()
        {
            bodySprite.Play("grow");
            Audio.Play("event:/embryo_growth", Position);
            
            var level = Scene as Level;
            level?.Shake(1f);
            
            // Burst out of membrane
            membraneSprite.Play("break");
            
            yield return 0.5f;
            
            membraneSprite.Visible = false;
            level?.Displacement.AddBurst(Position, 0.8f, 40f, 80f, 0.5f);
            
            bodySprite.Play("awake");
        }

        private AttackType ChooseAttack()
        {
            // More powerful attacks in later stages
            if (growthStage == GrowthStage.Newborn)
            {
                return (AttackType)Calc.Random.Next(0, 5);
            }
            else if (growthStage == GrowthStage.Fetal)
            {
                return (AttackType)Calc.Random.Next(0, 4);
            }
            else
            {
                return (AttackType)Calc.Random.Next(0, 3);
            }
        }

        private IEnumerator FloatTowardsPlayer(global::Celeste.Player player, float duration)
        {
            float timer = 0f;
            
            while (timer < duration && !isDefeated)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * 30f;

                // Bob up and down
                Speed = new Vector2(Speed.X, Speed.Y + (float)Math.Sin(Scene.TimeActive * 2f) * 10f);

                timer += Engine.DeltaTime;
                yield return null;
            }
            
            Speed = Vector2.Zero;
        }
        #endregion

        #region Attacks
        private IEnumerator ExecuteAttack(AttackType attack)
        {
            bodySprite.Play("attack");
            
            switch (attack)
            {
                case AttackType.PsychicPulse:
                    yield return PsychicPulseAttack();
                    break;
                case AttackType.TendrilGrab:
                    yield return TendrilGrabAttack();
                    break;
                case AttackType.WombShield:
                    yield return WombShieldAttack();
                    break;
                case AttackType.BirthSpawn:
                    yield return BirthSpawnAttack();
                    break;
                case AttackType.MindScream:
                    yield return MindScreamAttack();
                    break;
            }
            
            bodySprite.Play("awake");
        }

        private IEnumerator PsychicPulseAttack()
        {
            eyeSprite.Play("psychic");
            Audio.Play("event:/embryo_psychic", Position);
            
            psychicGlow.Alpha = 1.5f;
            eyeGlow.Alpha = 1.5f;
            eyeGlow.Color = new Color(200, 100, 200);
            
            var level = Scene as Level;
            
            // Expanding psychic rings
            for (int wave = 0; wave < 4; wave++)
            {
                level?.Displacement.AddBurst(Position, 0.6f, wave * 35f, wave * 35f + 45f, 0.4f);
                level?.Shake(0.3f);
                yield return 0.2f;
            }
            
            psychicGlow.Alpha = 0.4f;
            eyeGlow.Alpha = 0.6f;
            eyeGlow.Color = Color.Red;
            eyeSprite.Play("open");
        }

        private IEnumerator TendrilGrabAttack()
        {
            tendrilSprite.Visible = true;
            tendrilSprite.Play("emerge");
            Audio.Play("event:/embryo_tendril", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            yield return 0.3f;
            
            tendrilSprite.Play("grab");
            
            // Tendrils shoot towards player
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                
                for (float dist = 20f; dist < 150f; dist += 20f)
                {
                    Vector2 pos = Position + direction * dist + new Vector2(0f, 10f);
                    level?.Displacement.AddBurst(pos, 0.4f, 12f, 32f, 0.3f);
                    yield return 0.03f;
                }
            }
            
            level?.Shake(0.6f);
            
            yield return 0.3f;
            
            tendrilSprite.Play("retract");
            
            yield return 0.3f;
            
            tendrilSprite.Visible = false;
        }

        private IEnumerator WombShieldAttack()
        {
            membraneSprite.Visible = true;
            membraneSprite.Play("pulse");
            membraneSprite.Color = Color.White;
            Audio.Play("event:/embryo_shield", Position);
            
            var level = Scene as Level;
            
            // Protective membrane surrounds
            psychicGlow.Alpha = 2f;
            psychicGlow.Color = Color.Cyan;
            
            // Shield up for duration
            Collidable = false;
            
            for (float t = 0; t < 2f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.3f, 50f, 30f, 0.2f);
                yield return null;
            }
            
            Collidable = true;
            psychicGlow.Color = new Color(200, 100, 200);
            psychicGlow.Alpha = 0.4f;
            
            // Only hide membrane if not in newborn stage
            if (growthStage != GrowthStage.Embryonic)
            {
                membraneSprite.Visible = false;
            }
            else
            {
                membraneSprite.Color = Color.White * 0.7f;
            }
        }

        private IEnumerator BirthSpawnAttack()
        {
            bodySprite.Play("grow");
            Audio.Play("event:/embryo_birth", Position);
            
            var level = Scene as Level;
            
            level?.Shake(1f);
            
            // Spawn smaller creatures
            for (int i = 0; i < 3; i++)
            {
                float angle = (i / 3f) * MathHelper.TwoPi + Calc.Random.Range(-0.2f, 0.2f);
                Vector2 spawnPos = Position + Calc.AngleToVector(angle, 40f);
                
                level?.Displacement.AddBurst(spawnPos, 0.5f, 16f, 40f, 0.35f);
                Audio.Play("event:/embryo_spawn", spawnPos);
                
                yield return 0.2f;
            }
            
            // Self damage from birthing
            TakeDamage(10);
        }

        private IEnumerator MindScreamAttack()
        {
            eyeSprite.Play("stare");
            eyeGlow.Alpha = 2f;
            eyeGlow.Color = Color.Red;
            Audio.Play("event:/embryo_scream", Position);
            
            var level = Scene as Level;
            
            whisperLoop.Stop();
            
            // Intense psychic attack
            level?.Flash(new Color(200, 100, 200) * 0.3f, true);
            level?.Shake(1.5f);
            
            psychicGlow.Alpha = 3f;
            
            // Damaging waves in all directions
            for (int wave = 0; wave < 6; wave++)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = (i / 8f) * MathHelper.TwoPi + (wave * 0.1f);
                    Vector2 dir = Calc.AngleToVector(angle, 30f + wave * 20f);
                    level?.Displacement.AddBurst(Position + dir, 0.5f, 16f, 40f, 0.35f);
                }
                
                yield return 0.1f;
            }
            
            psychicGlow.Alpha = 0.4f;
            eyeGlow.Alpha = 0.6f;
            eyeSprite.Play("open");
            
            whisperLoop.Play("event:/embryo_whispers");
        }

        private IEnumerator GestationRoutine()
        {
            currentState = BossState.Gestating;
            bodySprite.Play("grow");
            membraneSprite.Play("pulse");
            Audio.Play("event:/embryo_gestate", Position);
            
            var level = Scene as Level;
            
            // Healing during gestation
            gestationProgress = 0f;
            
            while (gestationProgress < 1f && !isDefeated)
            {
                gestationProgress += Engine.DeltaTime * 0.3f;
                
                // Heal slightly
                health = Math.Min(health + 1, maxHealth);
                
                // Pulsing effect
                level?.Displacement.AddBurst(Position, 0.3f, 40f, 20f, 0.2f);
                
                yield return null;
            }
            
            isGestating = false;
            
            // Growth burst
            level?.Shake(1.2f);
            level?.Displacement.AddBurst(Position, 1f, 50f, 100f, 0.6f);
            
            bodySprite.Play("awake");
            currentState = BossState.Active;
        }
        #endregion

        #region Damage and Defeat
        public override void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            // Reduced damage during gestation
            if (isGestating)
            {
                damage = damage / 2;
            }
            
            health -= damage;
            Audio.Play("event:/embryo_hurt", Position);
            
            var level = Scene as Level;
            level?.Shake(0.3f);
            
            bodySprite.Color = new Color(200, 100, 200);
            eyeSprite.Play("stare");
            Add(new Coroutine(FlashReset()));
            
            if (health <= 0)
            {
                Defeat();
            }
        }

        private IEnumerator FlashReset()
        {
            yield return 0.15f;
            bodySprite.Color = Color.White;
            if (currentState == BossState.Active)
                eyeSprite.Play("open");
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
            heartbeatLoop.Stop();
            whisperLoop.Stop();
            
            bodySprite.Play("defeat");
            eyeSprite.Play("closed");
            eyeGlow.Alpha = 0f;
            Audio.Play("event:/embryo_death", Position);
            
            var level = Scene as Level;
            
            // Creature shrinks/deflates
            for (float t = 0; t < 1.5f; t += Engine.DeltaTime)
            {
                bodySprite.Scale = Vector2.Lerp(bodySprite.Scale, Vector2.One * 0.3f, t / 1.5f);
                psychicGlow.Alpha = Calc.Approach(psychicGlow.Alpha, 0f, Engine.DeltaTime);
                
                if (Calc.Random.Chance(0.3f))
                {
                    level?.Displacement.AddBurst(Position + Calc.Random.Range(Vector2.One * -15f, Vector2.One * 15f), 0.3f, 12f, 28f, 0.2f);
                }
                
                yield return null;
            }
            
            // Final psychic burst
            level?.Shake(1f);
            level?.Flash(new Color(200, 100, 200) * 0.4f, true);
            level?.Displacement.AddBurst(Position, 1f, 60f, 120f, 0.7f);
            
            yield return 1f;
            
            level?.Session.SetFlag("embryo_boss_defeated");
            RemoveSelf();
        }
        #endregion

        #region Cleanup
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            heartbeatLoop?.Stop();
            whisperLoop?.Stop();
        }
        #endregion
    }
}
