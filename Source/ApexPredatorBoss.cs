using MaggyHelper.Helpers;

namespace MaggyHelper.Entities.Bosses
{
    /// <summary>
    /// Apex Predator Boss - The ultimate hunter
    /// A savage beast that stalks and ambushes with deadly precision
    /// Uses stealth, speed, and brutal close-combat attacks
    /// Sprite path: characters/apexpredator/
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/ApexPredatorBoss")]
    [Tracked]
    [HotReloadable]
    public class ApexPredatorBoss : BossActor
    {
        #region Enums and Constants
        public enum BossPhase
        {
            Stalking,
            Phase1_Hunter,
            Phase2_Frenzy,
            Phase3_Apex,
            Defeated
        }

        public enum AttackType
        {
            // Phase 1 Attacks
            SlashCombo,
            Pounce,
            TailSwipe,
            Ambush,
            
            // Phase 2 Attacks
            FrenzyClaws,
            BloodlustCharge,
            SavageBite,
            PackCall,
            
            // Phase 3 Attacks
            ApexStrike,
            DeathRoll,
            PrimalRoar,
            ExtinctionCombo
        }
        #endregion

        #region Properties
        private int health = 1200;
        private int maxHealth = 1200;
        private bool isDefeated = false;
        private BossPhase currentPhase = BossPhase.Stalking;
        
        private Sprite predatorSprite;
        private Sprite clawsSprite;
        private Sprite tailSprite;
        private VertexLight eyeGlow;
        private SoundSource growlLoop;
        
        private bool isInvisible = false;
        private bool isFrenzied = false;
        private float dashSpeed = 500f;
        private float stalkTimer = 0f;
        private Vector2 lastKnownPlayerPos;
        #endregion

        #region Constructors
        public ApexPredatorBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "apex_predator_boss", Vector2.One, 220f, true, true, 1f, 
                   new Hitbox(40f, 32f, -20f, -32f))
        {
            health = data.Int("health", 1200);
            maxHealth = data.Int("maxHealth", 1200);
            SetupVisuals();
        }

        public ApexPredatorBoss(Vector2 position) 
            : base(position, "apex_predator_boss", Vector2.One, 220f, true, true, 1f, 
                   new Hitbox(40f, 32f, -20f, -32f))
        {
            SetupVisuals();
        }
        #endregion

        #region Setup
        private void SetupVisuals()
        {
            // Main body sprite
            Add(predatorSprite = new Sprite(GFX.Game, "characters/apexpredator/"));
            predatorSprite.AddLoop("stalk", "predator_stalk", 0.12f);
            predatorSprite.AddLoop("idle", "predator_idle", 0.1f);
            predatorSprite.AddLoop("run", "predator_run", 0.06f);
            predatorSprite.AddLoop("attack", "predator_attack", 0.04f);
            predatorSprite.AddLoop("pounce", "predator_pounce", 0.05f);
            predatorSprite.AddLoop("frenzy", "predator_frenzy", 0.03f);
            predatorSprite.AddLoop("roar", "predator_roar", 0.08f);
            predatorSprite.Play("stalk");
            predatorSprite.CenterOrigin();
            
            // Claws sprite overlay
            Add(clawsSprite = new Sprite(GFX.Game, "characters/apexpredator/"));
            clawsSprite.AddLoop("idle", "claws_idle", 0.1f);
            clawsSprite.AddLoop("slash", "claws_slash", 0.03f);
            clawsSprite.CenterOrigin();
            clawsSprite.Position = new Vector2(20f, -16f);
            
            // Tail sprite
            Add(tailSprite = new Sprite(GFX.Game, "characters/apexpredator/"));
            tailSprite.AddLoop("idle", "tail_idle", 0.1f);
            tailSprite.AddLoop("swipe", "tail_swipe", 0.04f);
            tailSprite.CenterOrigin();
            tailSprite.Position = new Vector2(-30f, -8f);
            
            // Eye glow (predator vision)
            Add(eyeGlow = new VertexLight(Color.Yellow, 0.9f, 24, 40));
            eyeGlow.Position = new Vector2(15f, -24f);
            
            // Ambient growl
            Add(growlLoop = new SoundSource());
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
            
            // Track player position
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                lastKnownPlayerPos = player.Position;
            }
            
            // Eye glow pulsing when frenzied
            if (isFrenzied)
            {
                eyeGlow.Alpha = 1f + (float)Math.Sin(Scene.TimeActive * 8f) * 0.5f;
                eyeGlow.Color = Color.Red;
            }
            
            // Stalking behavior
            if (currentPhase == BossPhase.Stalking)
            {
                stalkTimer += Engine.DeltaTime;
                predatorSprite.Color = Color.White * (0.3f + (float)Math.Sin(stalkTimer * 2f) * 0.2f);
            }
            else if (!isInvisible)
            {
                predatorSprite.Color = Color.White;
            }
        }

        public override void Render()
        {
            if (!isInvisible || currentPhase == BossPhase.Phase3_Apex)
            {
                base.Render();
            }
        }
        #endregion

        #region Main Boss Routine
        private IEnumerator BossRoutine()
        {
            yield return StalkingSequence();
            
            while (currentPhase != BossPhase.Defeated)
            {
                switch (currentPhase)
                {
                    case BossPhase.Phase1_Hunter:
                        yield return Phase1Loop();
                        break;
                    case BossPhase.Phase2_Frenzy:
                        yield return Phase2Loop();
                        break;
                    case BossPhase.Phase3_Apex:
                        yield return Phase3Loop();
                        break;
                }
                
                CheckPhaseTransition();
            }
        }

        private IEnumerator StalkingSequence()
        {
            growlLoop.Play("event:/apexpredator_growl_loop");
            isInvisible = true;
            
            // Stalk the player
            for (int i = 0; i < 3; i++)
            {
                yield return 2f;
                
                // Briefly reveal position
                isInvisible = false;
                Audio.Play("event:/apexpredator_stalk_reveal", Position);
                yield return 0.3f;
                isInvisible = true;
                
                // Reposition
                var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null)
                {
                    Position = player.Position + new Vector2(
                        Calc.Random.Choose(-1, 1) * Calc.Random.Range(80f, 150f),
                        Calc.Random.Range(-50f, 50f)
                    );
                }
            }
            
            // First strike
            isInvisible = false;
            predatorSprite.Color = Color.White;
            Audio.Play("event:/apexpredator_reveal_roar", Position);
            predatorSprite.Play("roar");
            
            var level = Scene as Level;
            level?.Shake(1f);
            
            yield return 1f;
            
            currentPhase = BossPhase.Phase1_Hunter;
            predatorSprite.Play("idle");
        }

        private void CheckPhaseTransition()
        {
            float healthPercent = (float)health / maxHealth;
            
            if (healthPercent <= 0)
            {
                currentPhase = BossPhase.Defeated;
            }
            else if (healthPercent <= 0.25f && currentPhase != BossPhase.Phase3_Apex)
            {
                currentPhase = BossPhase.Phase3_Apex;
                Add(new Coroutine(EnterPhase3()));
            }
            else if (healthPercent <= 0.55f && currentPhase == BossPhase.Phase1_Hunter)
            {
                currentPhase = BossPhase.Phase2_Frenzy;
                Add(new Coroutine(EnterPhase2()));
            }
        }
        #endregion

        #region Phase Routines
        private IEnumerator Phase1Loop()
        {
            while (currentPhase == BossPhase.Phase1_Hunter && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(0, 4);
                yield return ExecuteAttack(attack);
                yield return 2f;
            }
        }

        private IEnumerator EnterPhase2()
        {
            isFrenzied = true;
            predatorSprite.Play("frenzy");
            Audio.Play("event:/apexpredator_frenzy", Position);
            
            var level = Scene as Level;
            level?.Shake(1.5f);
            level?.Flash(Color.Red * 0.3f, true);
            
            eyeGlow.Color = Color.Red;
            eyeGlow.Alpha = 1.5f;
            dashSpeed = 650f;
            
            yield return 1.5f;
        }

        private IEnumerator Phase2Loop()
        {
            while (currentPhase == BossPhase.Phase2_Frenzy && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(4, 8);
                yield return ExecuteAttack(attack);
                yield return 1.5f;
            }
        }

        private IEnumerator EnterPhase3()
        {
            predatorSprite.Play("roar");
            Audio.Play("event:/apexpredator_apex_roar", Position);
            
            var level = Scene as Level;
            level?.Shake(2f);
            level?.Flash(Color.DarkRed * 0.4f, true);
            
            eyeGlow.Color = Color.DarkRed;
            eyeGlow.Alpha = 2f;
            dashSpeed = 800f;
            
            // Heal slightly
            health = Math.Min(health + 100, maxHealth / 3);
            
            yield return 2f;
            predatorSprite.Play("frenzy");
        }

        private IEnumerator Phase3Loop()
        {
            while (currentPhase == BossPhase.Phase3_Apex && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(8, 12);
                yield return ExecuteAttack(attack);
                yield return 1.2f;
            }
        }
        #endregion

        #region Attack Execution
        private IEnumerator ExecuteAttack(AttackType attack)
        {
            yield return TelegraphIntent(attack switch
            {
                AttackType.Pounce => BossTelegraphType.DashCyan,
                AttackType.Ambush => BossTelegraphType.TeleportYellow,
                AttackType.BloodlustCharge => BossTelegraphType.DashCyan,
                AttackType.PrimalRoar => BossTelegraphType.SpecialPurple,
                AttackType.ExtinctionCombo => BossTelegraphType.SpecialPurple,
                _ => BossTelegraphType.DangerRed
            }, attack == AttackType.ExtinctionCombo ? 0.6f : 0.5f);

            switch (attack)
            {
                // Phase 1
                case AttackType.SlashCombo:
                    yield return SlashComboAttack();
                    break;
                case AttackType.Pounce:
                    yield return PounceAttack();
                    break;
                case AttackType.TailSwipe:
                    yield return TailSwipeAttack();
                    break;
                case AttackType.Ambush:
                    yield return AmbushAttack();
                    break;
                    
                // Phase 2
                case AttackType.FrenzyClaws:
                    yield return FrenzyClawsAttack();
                    break;
                case AttackType.BloodlustCharge:
                    yield return BloodlustChargeAttack();
                    break;
                case AttackType.SavageBite:
                    yield return SavageBiteAttack();
                    break;
                case AttackType.PackCall:
                    yield return PackCallAttack();
                    break;
                    
                // Phase 3
                case AttackType.ApexStrike:
                    yield return ApexStrikeAttack();
                    break;
                case AttackType.DeathRoll:
                    yield return DeathRollAttack();
                    break;
                case AttackType.PrimalRoar:
                    yield return PrimalRoarAttack();
                    break;
                case AttackType.ExtinctionCombo:
                    yield return ExtinctionComboAttack();
                    break;
            }
        }
        #endregion

        #region Phase 1 Attacks
        private IEnumerator SlashComboAttack()
        {
            predatorSprite.Play("attack");
            clawsSprite.Play("slash");
            Audio.Play("event:/apexpredator_slash", Position);
            
            var level = Scene as Level;
            
            for (int i = 0; i < 3; i++)
            {
                level?.Displacement.AddBurst(Position + new Vector2(25f, -16f), 0.4f, 16f, 48f, 0.3f);
                yield return 0.2f;
            }
            
            clawsSprite.Play("idle");
            predatorSprite.Play("idle");
        }

        private IEnumerator PounceAttack()
        {
            predatorSprite.Play("pounce");
            Audio.Play("event:/apexpredator_pounce", Position);
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * dashSpeed + new Vector2(0f, -200f);
            }
            
            yield return 0.4f;
            Speed = new Vector2(Speed.X * 0.5f, 300f);
            
            yield return 0.3f;
            Speed = Vector2.Zero;
            
            var level = Scene as Level;
            level?.Shake(0.8f);
            level?.Displacement.AddBurst(Position, 0.6f, 32f, 80f, 0.4f);
            
            predatorSprite.Play("idle");
        }

        private IEnumerator TailSwipeAttack()
        {
            tailSprite.Play("swipe");
            Audio.Play("event:/apexpredator_tail_swipe", Position);
            
            var level = Scene as Level;
            level?.Displacement.AddBurst(Position + new Vector2(-40f, 0f), 0.5f, 24f, 64f, 0.4f);
            
            yield return 0.4f;
            tailSprite.Play("idle");
        }

        private IEnumerator AmbushAttack()
        {
            Audio.Play("event:/apexpredator_vanish", Position);
            isInvisible = true;
            Collidable = false;
            
            yield return 1f;
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                float side = Calc.Random.Choose(-1, 1);
                Position = player.Position + new Vector2(side * 60f, 0f);
            }
            
            isInvisible = false;
            Collidable = true;
            Audio.Play("event:/apexpredator_ambush_strike", Position);
            predatorSprite.Play("attack");
            clawsSprite.Play("slash");
            
            var level = Scene as Level;
            level?.Shake(1f);
            level?.Displacement.AddBurst(Position, 0.8f, 32f, 96f, 0.5f);
            
            yield return 0.5f;
            
            clawsSprite.Play("idle");
            predatorSprite.Play("idle");
        }
        #endregion

        #region Phase 2 Attacks
        private IEnumerator FrenzyClawsAttack()
        {
            predatorSprite.Play("frenzy");
            clawsSprite.Play("slash");
            Audio.Play("event:/apexpredator_frenzy_claws", Position);
            
            var level = Scene as Level;
            
            for (int i = 0; i < 8; i++)
            {
                level?.Displacement.AddBurst(Position + new Vector2(20f + Calc.Random.Range(-10f, 10f), -16f), 
                    0.3f, 12f, 36f, 0.2f);
                yield return 0.1f;
            }
            
            clawsSprite.Play("idle");
        }

        private IEnumerator BloodlustChargeAttack()
        {
            predatorSprite.Play("run");
            Audio.Play("event:/apexpredator_bloodlust_charge", Position);
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            for (int i = 0; i < 3; i++)
            {
                if (player != null)
                {
                    Vector2 direction = (player.Position - Position).SafeNormalize();
                    Speed = direction * dashSpeed;
                }
                
                yield return 0.3f;
                
                var level = Scene as Level;
                level?.Displacement.AddBurst(Position, 0.5f, 24f, 64f, 0.3f);
                
                Speed = Vector2.Zero;
                yield return 0.15f;
            }
            
            predatorSprite.Play("frenzy");
        }

        private IEnumerator SavageBiteAttack()
        {
            predatorSprite.Play("attack");
            Audio.Play("event:/apexpredator_savage_bite", Position);
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * dashSpeed * 0.8f;
            }
            
            yield return 0.25f;
            Speed = Vector2.Zero;
            
            // Bite hitbox
            var level = Scene as Level;
            level?.Shake(1f);
            level?.Displacement.AddBurst(Position + new Vector2(25f, -10f), 0.7f, 24f, 56f, 0.4f);
            
            yield return 0.5f;
            predatorSprite.Play("frenzy");
        }

        private IEnumerator PackCallAttack()
        {
            predatorSprite.Play("roar");
            Audio.Play("event:/apexpredator_pack_call", Position);
            
            var level = Scene as Level;
            level?.Shake(0.8f);
            
            yield return 0.5f;
            
            // Summon pack members (smaller predators)
            for (int i = 0; i < 3; i++)
            {
                Vector2 spawnPos = Position + new Vector2(Calc.Random.Range(-100f, 100f), -50f);
                Audio.Play("event:/apexpredator_pack_spawn", spawnPos);
                level?.Displacement.AddBurst(spawnPos, 0.5f, 16f, 48f, 0.3f);
                yield return 0.3f;
            }
            
            predatorSprite.Play("frenzy");
        }
        #endregion

        #region Phase 3 Attacks
        private IEnumerator ApexStrikeAttack()
        {
            predatorSprite.Play("pounce");
            Audio.Play("event:/apexpredator_apex_strike", Position);
            
            eyeGlow.Alpha = 3f;
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                // Lightning-fast strike
                Position = player.Position + new Vector2(Calc.Random.Choose(-1, 1) * 30f, 0f);
            }
            
            var level = Scene as Level;
            level?.Flash(Color.Red * 0.2f, true);
            level?.Shake(1.5f);
            level?.Displacement.AddBurst(Position, 1f, 48f, 128f, 0.6f);
            
            predatorSprite.Play("attack");
            clawsSprite.Play("slash");
            
            yield return 0.4f;
            
            eyeGlow.Alpha = 2f;
            clawsSprite.Play("idle");
            predatorSprite.Play("frenzy");
        }

        private IEnumerator DeathRollAttack()
        {
            Audio.Play("event:/apexpredator_death_roll", Position);
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * dashSpeed;
            }
            
            // Spinning attack
            for (float t = 0; t < 1f; t += Engine.DeltaTime)
            {
                predatorSprite.Rotation += Engine.DeltaTime * 20f;
                
                var level = Scene as Level;
                level?.Displacement.AddBurst(Position, 0.4f, 24f, 64f, 0.3f);
                
                yield return null;
            }
            
            Speed = Vector2.Zero;
            predatorSprite.Rotation = 0f;
            
            var lvl = Scene as Level;
            lvl?.Shake(1.5f);
            
            predatorSprite.Play("frenzy");
        }

        private IEnumerator PrimalRoarAttack()
        {
            predatorSprite.Play("roar");
            Audio.Play("event:/apexpredator_primal_roar", Position);
            
            var level = Scene as Level;
            level?.Shake(2f);
            
            // Shockwave rings
            for (int i = 0; i < 4; i++)
            {
                level?.Displacement.AddBurst(Position, 1f, 30f + i * 50f, 60f + i * 50f, 0.6f);
                yield return 0.2f;
            }
            
            // Fear effect (could slow player or cause screen distortion)
            level?.Flash(Color.DarkRed * 0.3f, true);
            
            yield return 0.5f;
            predatorSprite.Play("frenzy");
        }

        private IEnumerator ExtinctionComboAttack()
        {
            Audio.Play("event:/apexpredator_extinction_combo", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Multi-hit ultimate combo
            for (int i = 0; i < 6; i++)
            {
                if (player != null)
                {
                    // Teleport near player
                    float side = Calc.Random.Choose(-1, 1);
                    Position = player.Position + new Vector2(side * 40f, Calc.Random.Range(-20f, 20f));
                }
                
                predatorSprite.Play("attack");
                clawsSprite.Play("slash");
                
                level?.Displacement.AddBurst(Position, 0.5f, 20f, 56f, 0.3f);
                
                yield return 0.15f;
            }
            
            // Final strike
            eyeGlow.Alpha = 4f;
            level?.Flash(Color.Red * 0.3f, true);
            level?.Shake(2f);
            level?.Displacement.AddBurst(Position, 1.2f, 48f, 128f, 0.7f);
            
            yield return 0.5f;
            
            eyeGlow.Alpha = 2f;
            clawsSprite.Play("idle");
            predatorSprite.Play("frenzy");
        }
        #endregion

        #region Damage and Defeat
        public override void TakeDamage(int damage)
        {
            if (isDefeated || isInvisible) return;
            
            health -= damage;
            Audio.Play("event:/apexpredator_damage", Position);
            
            var level = Scene as Level;
            level?.Shake(0.4f);
            
            // Rage on damage in later phases
            if (isFrenzied)
            {
                eyeGlow.Alpha = 2.5f;
            }
            
            if (health <= 0)
            {
                Defeat();
            }
        }

        private void Defeat()
        {
            isDefeated = true;
            currentPhase = BossPhase.Defeated;
            Add(new Coroutine(DefeatSequence()));
        }

        private IEnumerator DefeatSequence()
        {
            growlLoop.Stop();
            predatorSprite.Play("roar");
            Audio.Play("event:/apexpredator_death_cry", Position);
            
            var level = Scene as Level;
            level?.Shake(2f);
            
            // Death throes
            for (int i = 0; i < 5; i++)
            {
                eyeGlow.Alpha = 3f - (i * 0.5f);
                level?.Displacement.AddBurst(Position, 0.6f, 24f, 64f, 0.4f);
                yield return 0.3f;
            }
            
            eyeGlow.Alpha = 0f;
            
            // Collapse
            for (float t = 0; t < 1f; t += Engine.DeltaTime)
            {
                predatorSprite.Scale = new Vector2(1f, 1f - t);
                yield return null;
            }
            
            yield return 0.5f;
            
            level?.Session.SetFlag("apex_predator_boss_defeated");
            RemoveSelf();
        }
        #endregion

        #region Cleanup
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            growlLoop?.Stop();
        }
        #endregion
    }
}
