using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// Spamton Neo Deluxe Boss - The ultimate salesman
    /// A corrupted digital entity seeking ultimate power
    /// Combines Deltarune's Spamton with enhanced NEO form abilities
    /// Multi-phase boss with projectile hell and transformation mechanics
    /// Sprite path: characters/DZ/spamtonneo/
    /// </summary>
    [CustomEntity(ids: "DZ/SpamtonNeoDeluxeBoss")]
    [Tracked]
    [HotReloadable]
    public class SpamtonNeoDeluxeBoss : BossActor
    {
        #region Enums and Constants
        public enum BossPhase
        {
            Intro,
            Phase1_BigShot,
            Phase2_Pipis,
            Phase3_HeartOnAChain,
            Phase4_FreedomPrayer,
            Defeated
        }

        public enum AttackType
        {
            // Phase 1 Attacks
            BigShot,
            PhoneSpam,
            MiniSpamtons,
            HeadCannon,
            
            // Phase 2 Attacks
            PipisBarrage,
            StringsOfFate,
            DealsDealsDeals,
            BlueAndYellowBullets,
            
            // Phase 3 Attacks
            HeartBlast,
            ChainedHeart,
            SoulSteal,
            NowsYourChanceToBe,
            
            // Phase 4 Attacks
            FreedomBlast,
            GarbageNoise,
            FinalBigShot,
            HyperLinkBlocked
        }
        
        // Spamton quotes for dialogue/audio cues
        private static readonly string[] SpamtonQuotes = new[]
        {
            "NOW'S YOUR CHANCE TO BE A [[BIG SHOT]]!",
            "KROMER! KROMER! KROMER!",
            "[[HYPERLINK BLOCKED]]",
            "[[$4.99]]",
            "DO YOU WANT TO BE A [[BIG SHOT]]?",
            "MIKE! MIKE! HELP ME MIKE!",
            "I'M A [[BIG SHOT]] NOW!",
            "DEAL'S A DEAL'S A DEAL!"
        };
        #endregion

        #region Properties
        private int health = 1800;
        private int maxHealth = 1800;
        private bool isDefeated = false;
        private BossPhase currentPhase = BossPhase.Intro;
        
        private Sprite spamtonSprite;
        private Sprite wingsSprite;
        private Sprite headSprite;
        private Sprite heartSprite;
        private List<Sprite> chainSprites = new List<Sprite>();
        private VertexLight glassesGlow;
        private VertexLight heartGlow;
        private SoundSource musicLoop;
        private SoundSource glitchSfx;
        
        private bool isGlitching = false;
        private float glitchTimer = 0f;
#pragma warning disable CS0414
        private int kronerCount = 0;
#pragma warning restore CS0414
        private bool heartExposed = false;
        #endregion

        #region Constructors
        public SpamtonNeoDeluxeBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "spamton_neo_boss", Vector2.One, 160f, true, true, 0.5f, 
                   new Hitbox(48f, 80f, -24f, -80f))
        {
            health = data.Int("health", 1800);
            maxHealth = data.Int("maxHealth", 1800);
            SetupVisuals();
        }

        public SpamtonNeoDeluxeBoss(Vector2 position) 
            : base(position, "spamton_neo_boss", Vector2.One, 160f, true, true, 0.5f, 
                   new Hitbox(48f, 80f, -24f, -80f))
        {
            SetupVisuals();
        }
        #endregion

        #region Setup
        private void SetupVisuals()
        {
            // Main body sprite
            Add(spamtonSprite = new Sprite(GFX.Game, "characters/DZ/spamtonneo/"));
            spamtonSprite.AddLoop("idle", "spamton_idle", 0.08f);
            spamtonSprite.AddLoop("attack", "spamton_attack", 0.05f);
            spamtonSprite.AddLoop("laugh", "spamton_laugh", 0.06f);
            spamtonSprite.AddLoop("glitch", "spamton_glitch", 0.03f);
            spamtonSprite.AddLoop("desperate", "spamton_desperate", 0.04f);
            spamtonSprite.AddLoop("freedom", "spamton_freedom", 0.07f);
            spamtonSprite.Play("idle");
            spamtonSprite.CenterOrigin();
            
            // Wings sprite
            Add(wingsSprite = new Sprite(GFX.Game, "characters/DZ/spamtonneo/"));
            wingsSprite.AddLoop("idle", "wings_idle", 0.1f);
            wingsSprite.AddLoop("spread", "wings_spread", 0.06f);
            wingsSprite.CenterOrigin();
            wingsSprite.Position = new Vector2(0f, -50f);
            
            // Detachable head
            Add(headSprite = new Sprite(GFX.Game, "characters/DZ/spamtonneo/"));
            headSprite.AddLoop("attached", "head_attached", 0.1f);
            headSprite.AddLoop("detached", "head_detached", 0.08f);
            headSprite.AddLoop("cannon", "head_cannon", 0.05f);
            headSprite.CenterOrigin();
            headSprite.Position = new Vector2(0f, -70f);
            headSprite.Visible = false; // Only visible when detached
            
            // Heart on chain
            Add(heartSprite = new Sprite(GFX.Game, "characters/DZ/spamtonneo/"));
            heartSprite.AddLoop("hidden", "heart_hidden", 0.1f);
            heartSprite.AddLoop("exposed", "heart_exposed", 0.12f);
            heartSprite.AddLoop("breaking", "heart_breaking", 0.06f);
            heartSprite.CenterOrigin();
            heartSprite.Position = new Vector2(0f, -40f);
            heartSprite.Visible = false;
            
            // Chain sprites (connecting to heart)
            for (int i = 0; i < 4; i++)
            {
                var chain = new Sprite(GFX.Game, "characters/DZ/spamtonneo/");
                chain.AddLoop("taut", "chain_taut", 0.1f);
                chain.AddLoop("loose", "chain_loose", 0.15f);
                chain.AddLoop("breaking", "chain_breaking", 0.04f);
                chain.CenterOrigin();
                chain.Visible = false;
                Add(chain);
                chainSprites.Add(chain);
            }
            
            // Glasses glow (yellow/pink)
            Add(glassesGlow = new VertexLight(Color.Yellow, 1.2f, 32, 56));
            glassesGlow.Position = new Vector2(0f, -65f);
            
            // Heart glow
            Add(heartGlow = new VertexLight(Color.Cyan, 0f, 24, 40));
            heartGlow.Position = new Vector2(0f, -40f);
            
            // Sound sources
            Add(musicLoop = new SoundSource());
            Add(glitchSfx = new SoundSource());
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
            
            // Glitch effect
            if (isGlitching)
            {
                glitchTimer += Engine.DeltaTime;
                spamtonSprite.Position = new Vector2(
                    Calc.Random.Range(-3f, 3f),
                    Calc.Random.Range(-3f, 3f)
                );
                
                if (glitchTimer > 0.5f)
                {
                    isGlitching = false;
                    glitchTimer = 0f;
                    spamtonSprite.Position = Vector2.Zero;
                }
            }
            
            // Pulsing glasses glow
            glassesGlow.Alpha = 1f + (float)Math.Sin(Scene.TimeActive * 4f) * 0.3f;
            
            // Random color flicker for glasses
            if (Calc.Random.Chance(0.02f))
            {
                glassesGlow.Color = Calc.Random.Choose(Color.Yellow, Color.Magenta, Color.Cyan);
            }
            
            // Heart glow when exposed
            if (heartExposed)
            {
                heartGlow.Alpha = 1f + (float)Math.Sin(Scene.TimeActive * 6f) * 0.4f;
            }
        }
        #endregion

        #region Main Boss Routine
        private IEnumerator BossRoutine()
        {
            yield return IntroSequence();
            
            while (currentPhase != BossPhase.Defeated)
            {
                switch (currentPhase)
                {
                    case BossPhase.Phase1_BigShot:
                        yield return Phase1Loop();
                        break;
                    case BossPhase.Phase2_Pipis:
                        yield return Phase2Loop();
                        break;
                    case BossPhase.Phase3_HeartOnAChain:
                        yield return Phase3Loop();
                        break;
                    case BossPhase.Phase4_FreedomPrayer:
                        yield return Phase4Loop();
                        break;
                }
                
                CheckPhaseTransition();
            }
        }

        private IEnumerator IntroSequence()
        {
            Audio.Play("event:/spamton_entrance", Position);
            spamtonSprite.Play("laugh");
            
            // Spamton dialogue
            yield return ShowSpamtonDialogue("NOW'S YOUR CHANCE TO BE A [[BIG SHOT]]!");
            
            var level = Scene as Level;
            level?.Shake(0.5f);
            
            wingsSprite.Play("spread");
            Audio.Play("event:/spamton_wings_spread", Position);
            
            yield return 1f;
            
            spamtonSprite.Play("idle");
            wingsSprite.Play("idle");
            currentPhase = BossPhase.Phase1_BigShot;
        }

        private IEnumerator ShowSpamtonDialogue(string text)
        {
            // This would integrate with your dialogue system
            // For now, just play audio and wait
            isGlitching = true;
            yield return 0.5f;
            isGlitching = false;
            yield return 1f;
        }

        private void CheckPhaseTransition()
        {
            float healthPercent = (float)health / maxHealth;
            
            if (healthPercent <= 0)
            {
                currentPhase = BossPhase.Defeated;
            }
            else if (healthPercent <= 0.2f && currentPhase != BossPhase.Phase4_FreedomPrayer)
            {
                currentPhase = BossPhase.Phase4_FreedomPrayer;
                Add(new Coroutine(EnterPhase4()));
            }
            else if (healthPercent <= 0.45f && currentPhase == BossPhase.Phase2_Pipis)
            {
                currentPhase = BossPhase.Phase3_HeartOnAChain;
                Add(new Coroutine(EnterPhase3()));
            }
            else if (healthPercent <= 0.7f && currentPhase == BossPhase.Phase1_BigShot)
            {
                currentPhase = BossPhase.Phase2_Pipis;
                Add(new Coroutine(EnterPhase2()));
            }
        }
        #endregion

        #region Phase Routines
        private IEnumerator Phase1Loop()
        {
            while (currentPhase == BossPhase.Phase1_BigShot && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(0, 4);
                yield return ExecuteAttack(attack);
                yield return 2f;
            }
        }

        private IEnumerator EnterPhase2()
        {
            spamtonSprite.Play("laugh");
            Audio.Play("event:/spamton_phase2", Position);
            
            yield return ShowSpamtonDialogue("PIPIS! [[PIPIS]]! P I P I S !");
            
            var level = Scene as Level;
            level?.Shake(1f);
            level?.Flash(Color.Cyan * 0.3f, true);
            
            glassesGlow.Color = Color.Cyan;
            
            yield return 1.5f;
            spamtonSprite.Play("idle");
        }

        private IEnumerator Phase2Loop()
        {
            while (currentPhase == BossPhase.Phase2_Pipis && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(4, 8);
                yield return ExecuteAttack(attack);
                yield return 1.8f;
            }
        }

        private IEnumerator EnterPhase3()
        {
            spamtonSprite.Play("desperate");
            Audio.Play("event:/spamton_heart_reveal", Position);
            
            yield return ShowSpamtonDialogue("NO! MY [[HEART]]! YOU CAN SEE MY [[HEART]]!");
            
            heartExposed = true;
            heartSprite.Visible = true;
            heartSprite.Play("exposed");
            heartGlow.Alpha = 1f;
            
            foreach (var chain in chainSprites)
            {
                chain.Visible = true;
                chain.Play("taut");
            }
            
            var level = Scene as Level;
            level?.Shake(1.5f);
            level?.Flash(Color.Magenta * 0.3f, true);
            
            yield return 2f;
        }

        private IEnumerator Phase3Loop()
        {
            while (currentPhase == BossPhase.Phase3_HeartOnAChain && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(8, 12);
                yield return ExecuteAttack(attack);
                yield return 1.5f;
            }
        }

        private IEnumerator EnterPhase4()
        {
            spamtonSprite.Play("freedom");
            Audio.Play("event:/spamton_freedom", Position);
            
            yield return ShowSpamtonDialogue("I JUST WANT TO BE [[FREE]]!");
            
            // Chains start breaking
            foreach (var chain in chainSprites)
            {
                chain.Play("breaking");
            }
            
            var level = Scene as Level;
            level?.Shake(2f);
            level?.Flash(Color.White * 0.4f, true);
            
            glassesGlow.Color = Color.White;
            glassesGlow.Alpha = 2f;
            
            yield return 2f;
        }

        private IEnumerator Phase4Loop()
        {
            while (currentPhase == BossPhase.Phase4_FreedomPrayer && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(12, 16);
                yield return ExecuteAttack(attack);
                yield return 1.2f;
            }
        }
        #endregion

        #region Attack Execution
        private IEnumerator ExecuteAttack(AttackType attack)
        {
            switch (attack)
            {
                // Phase 1
                case AttackType.BigShot:
                    yield return BigShotAttack();
                    break;
                case AttackType.PhoneSpam:
                    yield return PhoneSpamAttack();
                    break;
                case AttackType.MiniSpamtons:
                    yield return MiniSpamtonsAttack();
                    break;
                case AttackType.HeadCannon:
                    yield return HeadCannonAttack();
                    break;
                    
                // Phase 2
                case AttackType.PipisBarrage:
                    yield return PipisBarrageAttack();
                    break;
                case AttackType.StringsOfFate:
                    yield return StringsOfFateAttack();
                    break;
                case AttackType.DealsDealsDeals:
                    yield return DealsDealsDealsAttack();
                    break;
                case AttackType.BlueAndYellowBullets:
                    yield return BlueAndYellowBulletsAttack();
                    break;
                    
                // Phase 3
                case AttackType.HeartBlast:
                    yield return HeartBlastAttack();
                    break;
                case AttackType.ChainedHeart:
                    yield return ChainedHeartAttack();
                    break;
                case AttackType.SoulSteal:
                    yield return SoulStealAttack();
                    break;
                case AttackType.NowsYourChanceToBe:
                    yield return NowsYourChanceToBeAttack();
                    break;
                    
                // Phase 4
                case AttackType.FreedomBlast:
                    yield return FreedomBlastAttack();
                    break;
                case AttackType.GarbageNoise:
                    yield return GarbageNoiseAttack();
                    break;
                case AttackType.FinalBigShot:
                    yield return FinalBigShotAttack();
                    break;
                case AttackType.HyperLinkBlocked:
                    yield return HyperLinkBlockedAttack();
                    break;
            }
        }
        #endregion

        #region Phase 1 Attacks
        private IEnumerator BigShotAttack()
        {
            spamtonSprite.Play("attack");
            Audio.Play("event:/spamton_big_shot", Position);
            
            var level = Scene as Level;
            
            // Fire big shot projectile
            for (int i = 0; i < 3; i++)
            {
                level?.Displacement.AddBurst(Position + new Vector2(30f, -40f), 0.6f, 24f, 64f, 0.4f);
                yield return 0.3f;
            }
            
            spamtonSprite.Play("idle");
        }

        private IEnumerator PhoneSpamAttack()
        {
            spamtonSprite.Play("laugh");
            Audio.Play("event:/spamton_phone_spam", Position);
            
            var level = Scene as Level;
            
            // Spawn phone projectiles from random directions
            for (int i = 0; i < 8; i++)
            {
                float angle = Calc.Random.NextAngle();
                Vector2 spawnPos = Position + Calc.AngleToVector(angle, 150f);
                level?.Displacement.AddBurst(spawnPos, 0.4f, 16f, 40f, 0.3f);
                yield return 0.15f;
            }
            
            spamtonSprite.Play("idle");
        }

        private IEnumerator MiniSpamtonsAttack()
        {
            Audio.Play("event:/spamton_mini_spawn", Position);
            
            var level = Scene as Level;
            
            // Spawn mini Spamton heads
            for (int i = 0; i < 5; i++)
            {
                Vector2 spawnPos = Position + new Vector2(Calc.Random.Range(-80f, 80f), -100f);
                level?.Displacement.AddBurst(spawnPos, 0.5f, 16f, 48f, 0.3f);
                yield return 0.2f;
            }
        }

        private IEnumerator HeadCannonAttack()
        {
            headSprite.Visible = true;
            headSprite.Play("detached");
            Audio.Play("event:/spamton_head_detach", Position);
            
            yield return 0.5f;
            
            headSprite.Play("cannon");
            Audio.Play("event:/spamton_head_cannon", Position);
            
            var level = Scene as Level;
            level?.Shake(1f);
            level?.Displacement.AddBurst(headSprite.RenderPosition, 0.8f, 32f, 96f, 0.5f);
            
            yield return 1f;
            
            headSprite.Play("attached");
            headSprite.Visible = false;
        }
        #endregion

        #region Phase 2 Attacks
        private IEnumerator PipisBarrageAttack()
        {
            spamtonSprite.Play("attack");
            Audio.Play("event:/spamton_pipis", Position);
            
            var level = Scene as Level;
            
            // Fire blue egg-like projectiles that explode
            for (int i = 0; i < 10; i++)
            {
                float angle = -MathHelper.PiOver2 + (i - 5) * 0.2f;
                Vector2 direction = Calc.AngleToVector(angle, 1f);
                
                level?.Displacement.AddBurst(Position + direction * 30f, 0.4f, 16f, 40f, 0.3f);
                yield return 0.1f;
            }
            
            spamtonSprite.Play("idle");
        }

        private IEnumerator StringsOfFateAttack()
        {
            Audio.Play("event:/spamton_strings", Position);
            
            var level = Scene as Level;
            
            // Create vertical string hazards
            for (int i = 0; i < 6; i++)
            {
                float xPos = Position.X - 150f + (i * 60f);
                level?.Displacement.AddBurst(new Vector2(xPos, Position.Y - 100f), 0.6f, 8f, 200f, 0.5f);
                yield return 0.15f;
            }
            
            yield return 1f;
        }

        private IEnumerator DealsDealsDealsAttack()
        {
            spamtonSprite.Play("laugh");
            Audio.Play("event:/spamton_deals", Position);
            
            isGlitching = true;
            
            var level = Scene as Level;
            
            // Spawn deal popup projectiles
            for (int i = 0; i < 6; i++)
            {
                Vector2 spawnPos = Position + Calc.Random.Range(Vector2.One * -100f, Vector2.One * 100f);
                level?.Displacement.AddBurst(spawnPos, 0.5f, 24f, 56f, 0.4f);
                yield return 0.2f;
            }
            
            isGlitching = false;
            spamtonSprite.Play("idle");
        }

        private IEnumerator BlueAndYellowBulletsAttack()
        {
            Audio.Play("event:/spamton_bullets", Position);
            
            var level = Scene as Level;
            
            // Alternating blue (must stand still) and yellow (must move) bullets
            for (int wave = 0; wave < 3; wave++)
            {
                bool isBlue = wave % 2 == 0;
                Color bulletColor = isBlue ? Color.Cyan : Color.Yellow;
                
                glassesGlow.Color = bulletColor;
                
                for (int i = 0; i < 8; i++)
                {
                    float angle = (i / 8f) * MathHelper.TwoPi;
                    Vector2 dir = Calc.AngleToVector(angle, 1f);
                    level?.Displacement.AddBurst(Position + dir * 40f, 0.4f, 16f, 48f, 0.3f);
                }
                
                yield return 0.6f;
            }
            
            glassesGlow.Color = Color.Yellow;
        }
        #endregion

        #region Phase 3 Attacks
        private IEnumerator HeartBlastAttack()
        {
            heartSprite.Play("exposed");
            heartGlow.Alpha = 2f;
            Audio.Play("event:/spamton_heart_blast", Position);
            
            var level = Scene as Level;
            level?.Shake(1f);
            
            // Heart shoots energy beam
            level?.Displacement.AddBurst(heartSprite.RenderPosition, 1f, 32f, 128f, 0.6f);
            
            yield return 1f;
            heartGlow.Alpha = 1f;
        }

        private IEnumerator ChainedHeartAttack()
        {
            Audio.Play("event:/spamton_chains", Position);
            
            var level = Scene as Level;
            
            // Chains lash out at player
            foreach (var chain in chainSprites)
            {
                chain.Play("loose");
            }
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 chainDir = (player.Position - Position).SafeNormalize();
                    level?.Displacement.AddBurst(Position + chainDir * (40f + i * 30f), 0.5f, 16f, 48f, 0.3f);
                    yield return 0.2f;
                }
            }
            
            foreach (var chain in chainSprites)
            {
                chain.Play("taut");
            }
        }

        private IEnumerator SoulStealAttack()
        {
            spamtonSprite.Play("desperate");
            Audio.Play("event:/spamton_soul_steal", Position);
            
            var level = Scene as Level;
            level?.Flash(Color.Magenta * 0.3f, true);
            
            // Pull effect toward heart
            for (float t = 0; t < 1.5f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(heartSprite.RenderPosition, 0.5f, 80f, 20f, 0.3f);
                yield return null;
            }
            
            spamtonSprite.Play("idle");
        }

        private IEnumerator NowsYourChanceToBeAttack()
        {
            spamtonSprite.Play("laugh");
            Audio.Play("event:/spamton_big_shot_ultimate", Position);
            
            yield return ShowSpamtonDialogue("NOW'S YOUR CHANCE TO BE A [[BIG SHOT]]!");
            
            var level = Scene as Level;
            
            // Massive bullet hell pattern
            for (int wave = 0; wave < 3; wave++)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = (i / 12f) * MathHelper.TwoPi + (wave * 0.2f);
                    Vector2 dir = Calc.AngleToVector(angle, 1f);
                    level?.Displacement.AddBurst(Position + dir * 50f, 0.5f, 20f, 56f, 0.4f);
                }
                yield return 0.4f;
            }
            
            level?.Shake(1.5f);
            spamtonSprite.Play("idle");
        }
        #endregion

        #region Phase 4 Attacks
        private IEnumerator FreedomBlastAttack()
        {
            spamtonSprite.Play("freedom");
            Audio.Play("event:/spamton_freedom_blast", Position);
            
            // Break chains momentarily
            foreach (var chain in chainSprites)
            {
                chain.Play("breaking");
            }
            
            var level = Scene as Level;
            level?.Shake(2f);
            level?.Flash(Color.White * 0.4f, true);
            
            // Massive energy release
            for (int i = 0; i < 5; i++)
            {
                level?.Displacement.AddBurst(Position, 1.2f, i * 40f, i * 40f + 60f, 0.6f);
                yield return 0.15f;
            }
            
            foreach (var chain in chainSprites)
            {
                chain.Play("taut");
            }
        }

        private IEnumerator GarbageNoiseAttack()
        {
            isGlitching = true;
            glitchSfx.Play("event:/spamton_garbage_noise");
            
            var level = Scene as Level;
            level?.Shake(1f);
            
            // Random chaotic projectiles
            for (int i = 0; i < 15; i++)
            {
                Vector2 randomPos = Position + Calc.Random.Range(Vector2.One * -150f, Vector2.One * 150f);
                level?.Displacement.AddBurst(randomPos, 0.4f, 16f, 48f, 0.3f);
                yield return 0.08f;
            }
            
            isGlitching = false;
        }

        private IEnumerator FinalBigShotAttack()
        {
            spamtonSprite.Play("attack");
            Audio.Play("event:/spamton_final_big_shot", Position);
            
            glassesGlow.Alpha = 3f;
            
            var level = Scene as Level;
            
            // Charge up
            for (float t = 0; t < 1.5f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.3f, 20f, 40f, 0.2f);
                yield return null;
            }
            
            // Fire ultimate big shot
            level?.Flash(Color.Yellow * 0.5f, true);
            level?.Shake(2f);
            level?.Displacement.AddBurst(Position, 1.5f, 48f, 192f, 0.8f);
            
            yield return 1f;
            glassesGlow.Alpha = 1f;
            spamtonSprite.Play("idle");
        }

        private IEnumerator HyperLinkBlockedAttack()
        {
            Audio.Play("event:/spamton_hyperlink_blocked", Position);
            
            yield return ShowSpamtonDialogue("[[HYPERLINK BLOCKED]]");
            
            var level = Scene as Level;
            
            // Create blocked zones that damage player
            isGlitching = true;
            
            for (int i = 0; i < 4; i++)
            {
                Vector2 blockPos = Position + new Vector2(Calc.Random.Range(-120f, 120f), Calc.Random.Range(-80f, 80f));
                level?.Displacement.AddBurst(blockPos, 0.8f, 40f, 80f, 0.5f);
                level?.Flash(Color.Red * 0.2f, true);
                yield return 0.3f;
            }
            
            isGlitching = false;
        }
        #endregion

        #region Damage and Defeat
        public override void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            // Extra damage when heart is exposed
            if (heartExposed)
            {
                damage = (int)(damage * 1.5f);
            }
            
            health -= damage;
            Audio.Play("event:/spamton_damage", Position);
            
            isGlitching = true;
            
            var level = Scene as Level;
            level?.Shake(0.4f);
            
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
            musicLoop.Stop();
            spamtonSprite.Play("desperate");
            Audio.Play("event:/spamton_defeat", Position);
            
            yield return ShowSpamtonDialogue("I JUST WANTED TO BE... [[FREE]]...");
            
            var level = Scene as Level;
            
            // Chains breaking one by one
            foreach (var chain in chainSprites)
            {
                chain.Play("breaking");
                Audio.Play("event:/spamton_chain_break", Position);
                level?.Shake(0.5f);
                yield return 0.5f;
            }
            
            // Heart breaks
            heartSprite.Play("breaking");
            Audio.Play("event:/spamton_heart_break", Position);
            level?.Shake(2f);
            level?.Flash(Color.White, true);
            
            yield return 1f;
            
            // Collapse
            for (float t = 0; t < 2f; t += Engine.DeltaTime)
            {
                spamtonSprite.Scale = new Vector2(1f - t * 0.5f);
                spamtonSprite.Color = Color.White * (1f - t * 0.5f);
                yield return null;
            }
            
            level?.Session.SetFlag("spamton_neo_deluxe_boss_defeated");
            RemoveSelf();
        }
        #endregion

        #region Cleanup
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            musicLoop?.Stop();
            glitchSfx?.Stop();
        }
        #endregion
    }
}
