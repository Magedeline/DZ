using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// Morpho Knight Delta Boss - The butterfly of paradise transformed
    /// A divine warrior that has absorbed the power of fallen heroes
    /// Uses elegant sword techniques combined with butterfly-based magic
    /// Multi-phase boss with escalating divine power
    /// Sprite path: characters/DZ/morphoknightdelta/
    /// </summary>
    [CustomEntity(ids: "DZ/MorphoKnightDeltaBoss")]
    [Tracked]
    [HotReloadable]
    public class MorphoKnightDeltaBoss : BossActor
    {
        #region Enums and Constants
        public enum BossPhase
        {
            Emergence,
            Phase1_Butterfly,
            Phase2_Metamorphosis,
            Phase3_Divine,
            Phase4_Transcendent,
            Defeated
        }

        public enum AttackType
        {
            // Phase 1 Attacks
            SwordSlash,
            ButterflyBarrage,
            GracefulDash,
            WingGust,
            
            // Phase 2 Attacks
            FlameWing,
            SoulAbsorb,
            TeleportStrike,
            ScaleStorm,
            
            // Phase 3 Attacks
            DivineJudgment,
            HeavenlyBlades,
            ParadiseLost,
            CelestialBeam,
            
            // Phase 4 Attacks
            TranscendentSlash,
            InfiniteButterflies,
            DeltaStrike,
            Apotheosis
        }
        #endregion

        #region Properties
        private int health = 2000;
        private int maxHealth = 2000;
        private bool isDefeated = false;
        private BossPhase currentPhase = BossPhase.Emergence;
        
        private Sprite knightSprite;
        private Sprite wingsSprite;
        private Sprite swordSprite;
        private Sprite auraSprite;
        private VertexLight bodyGlow;
        private VertexLight wingGlow;
        private VertexLight swordGlow;
        private SoundSource ambientLoop;
        
        private Color currentColor = Color.Orange;
        private float teleportCooldown = 0f;
        private bool isTranscendent = false;
        private List<Vector2> butterflyPositions = new List<Vector2>();
        #endregion

        #region Constructors
        public MorphoKnightDeltaBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "morpho_knight_delta_boss", Vector2.One, 0f, true, false, 0f, 
                   new Hitbox(32f, 48f, -16f, -48f))
        {
            health = data.Int("health", 2000);
            maxHealth = data.Int("maxHealth", 2000);
            SetupVisuals();
        }

        public MorphoKnightDeltaBoss(Vector2 position) 
            : base(position, "morpho_knight_delta_boss", Vector2.One, 0f, true, false, 0f, 
                   new Hitbox(32f, 48f, -16f, -48f))
        {
            SetupVisuals();
        }
        #endregion

        #region Setup
        private void SetupVisuals()
        {
            // Main body sprite
            Add(knightSprite = new Sprite(GFX.Game, "characters/DZ/morphoknightdelta/"));
            knightSprite.AddLoop("emerge", "morpho_emerge", 0.1f);
            knightSprite.AddLoop("idle", "morpho_idle", 0.08f);
            knightSprite.AddLoop("attack", "morpho_attack", 0.04f);
            knightSprite.AddLoop("dash", "morpho_dash", 0.05f);
            knightSprite.AddLoop("divine", "morpho_divine", 0.06f);
            knightSprite.AddLoop("transcend", "morpho_transcend", 0.04f);
            knightSprite.Play("emerge");
            knightSprite.CenterOrigin();
            
            // Wings sprite
            Add(wingsSprite = new Sprite(GFX.Game, "characters/DZ/morphoknightdelta/"));
            wingsSprite.AddLoop("folded", "wings_folded", 0.1f);
            wingsSprite.AddLoop("spread", "wings_spread", 0.08f);
            wingsSprite.AddLoop("flap", "wings_flap", 0.05f);
            wingsSprite.AddLoop("flame", "wings_flame", 0.04f);
            wingsSprite.AddLoop("divine", "wings_divine", 0.06f);
            wingsSprite.CenterOrigin();
            wingsSprite.Position = new Vector2(0f, -24f);
            
            // Sword sprite
            Add(swordSprite = new Sprite(GFX.Game, "characters/DZ/morphoknightdelta/"));
            swordSprite.AddLoop("idle", "sword_idle", 0.1f);
            swordSprite.AddLoop("slash", "sword_slash", 0.03f);
            swordSprite.AddLoop("thrust", "sword_thrust", 0.04f);
            swordSprite.AddLoop("divine", "sword_divine", 0.05f);
            swordSprite.CenterOrigin();
            swordSprite.Position = new Vector2(20f, -24f);
            
            // Aura effect sprite
            Add(auraSprite = new Sprite(GFX.Game, "characters/DZ/morphoknightdelta/"));
            auraSprite.AddLoop("none", "aura_none", 0.1f);
            auraSprite.AddLoop("active", "aura_active", 0.06f);
            auraSprite.AddLoop("transcendent", "aura_transcendent", 0.04f);
            auraSprite.CenterOrigin();
            auraSprite.Visible = false;
            
            // Body glow
            Add(bodyGlow = new VertexLight(Color.Orange, 1f, 48, 80));
            bodyGlow.Position = new Vector2(0f, -24f);
            
            // Wing glow
            Add(wingGlow = new VertexLight(Color.Orange, 0.8f, 64, 96));
            wingGlow.Position = new Vector2(0f, -24f);
            
            // Sword glow
            Add(swordGlow = new VertexLight(Color.White, 0.6f, 24, 40));
            swordGlow.Position = new Vector2(30f, -24f);
            
            // Ambient sound
            Add(ambientLoop = new SoundSource());
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
            
            // Update teleport cooldown
            if (teleportCooldown > 0f)
            {
                teleportCooldown -= Engine.DeltaTime;
            }
            
            // Pulsing glow effect
            float pulse = (float)Math.Sin(Scene.TimeActive * 3f) * 0.3f;
            bodyGlow.Alpha = 1f + pulse;
            wingGlow.Alpha = 0.8f + pulse;
            
            // Transcendent visual effects
            if (isTranscendent)
            {
                // Rainbow color cycling
                float hue = (Scene.TimeActive * 0.5f) % 1f;
                currentColor = ColorFromHSV(hue, 1f, 1f);
                bodyGlow.Color = currentColor;
                wingGlow.Color = currentColor;
                
                // Spawn butterfly particles
                if (Calc.Random.Chance(0.1f))
                {
                    butterflyPositions.Add(Position + Calc.Random.Range(Vector2.One * -60f, Vector2.One * 60f));
                }
            }
            
            // Update butterflies
            for (int i = butterflyPositions.Count - 1; i >= 0; i--)
            {
                butterflyPositions[i] += new Vector2(Calc.Random.Range(-1f, 1f), -2f);
                if (butterflyPositions[i].Y < Position.Y - 200f)
                {
                    butterflyPositions.RemoveAt(i);
                }
            }
        }

        private Color ColorFromHSV(float h, float s, float v)
        {
            float r, g, b;
            int i = (int)(h * 6);
            float f = h * 6 - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);
            
            switch (i % 6)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }
            
            return new Color(r, g, b);
        }

        public override void Render()
        {
            base.Render();
            
            // Render butterflies
            foreach (var pos in butterflyPositions)
            {
                Draw.Rect(pos - Vector2.One * 2f, 4f, 4f, currentColor * 0.7f);
            }
        }
        #endregion

        #region Main Boss Routine
        private IEnumerator BossRoutine()
        {
            yield return EmergenceSequence();
            
            while (currentPhase != BossPhase.Defeated)
            {
                switch (currentPhase)
                {
                    case BossPhase.Phase1_Butterfly:
                        yield return Phase1Loop();
                        break;
                    case BossPhase.Phase2_Metamorphosis:
                        yield return Phase2Loop();
                        break;
                    case BossPhase.Phase3_Divine:
                        yield return Phase3Loop();
                        break;
                    case BossPhase.Phase4_Transcendent:
                        yield return Phase4Loop();
                        break;
                }
                
                CheckPhaseTransition();
            }
        }

        private IEnumerator EmergenceSequence()
        {
            Audio.Play("event:/morpho_emergence", Position);
            knightSprite.Play("emerge");
            wingsSprite.Play("folded");
            
            var level = Scene as Level;
            
            // Butterfly cocoon opens
            for (float t = 0; t < 2f; t += Engine.DeltaTime)
            {
                bodyGlow.Alpha = t / 2f;
                yield return null;
            }
            
            // Wings spread dramatically
            wingsSprite.Play("spread");
            Audio.Play("event:/morpho_wings_spread", Position);
            level?.Shake(1f);
            level?.Flash(Color.Orange * 0.3f, true);
            
            yield return 1f;
            
            knightSprite.Play("idle");
            wingsSprite.Play("flap");
            swordSprite.Play("idle");
            ambientLoop.Play("event:/morpho_ambient_loop");
            
            currentPhase = BossPhase.Phase1_Butterfly;
        }

        private void CheckPhaseTransition()
        {
            float healthPercent = (float)health / maxHealth;
            
            if (healthPercent <= 0)
            {
                currentPhase = BossPhase.Defeated;
            }
            else if (healthPercent <= 0.2f && currentPhase != BossPhase.Phase4_Transcendent)
            {
                currentPhase = BossPhase.Phase4_Transcendent;
                Add(new Coroutine(EnterPhase4()));
            }
            else if (healthPercent <= 0.45f && currentPhase == BossPhase.Phase2_Metamorphosis)
            {
                currentPhase = BossPhase.Phase3_Divine;
                Add(new Coroutine(EnterPhase3()));
            }
            else if (healthPercent <= 0.7f && currentPhase == BossPhase.Phase1_Butterfly)
            {
                currentPhase = BossPhase.Phase2_Metamorphosis;
                Add(new Coroutine(EnterPhase2()));
            }
        }
        #endregion

        #region Phase Routines
        private IEnumerator Phase1Loop()
        {
            while (currentPhase == BossPhase.Phase1_Butterfly && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(0, 4);
                yield return ExecuteAttack(attack);
                yield return 2f;
            }
        }

        private IEnumerator EnterPhase2()
        {
            knightSprite.Play("attack");
            wingsSprite.Play("flame");
            Audio.Play("event:/morpho_metamorphosis", Position);
            
            var level = Scene as Level;
            level?.Shake(1.5f);
            level?.Flash(Color.Orange * 0.4f, true);
            
            currentColor = Color.OrangeRed;
            bodyGlow.Color = currentColor;
            wingGlow.Color = currentColor;
            
            yield return 2f;
            
            knightSprite.Play("idle");
        }

        private IEnumerator Phase2Loop()
        {
            while (currentPhase == BossPhase.Phase2_Metamorphosis && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(4, 8);
                yield return ExecuteAttack(attack);
                yield return 1.8f;
            }
        }

        private IEnumerator EnterPhase3()
        {
            knightSprite.Play("divine");
            wingsSprite.Play("divine");
            swordSprite.Play("divine");
            Audio.Play("event:/morpho_divine_awakening", Position);
            
            auraSprite.Visible = true;
            auraSprite.Play("active");
            
            var level = Scene as Level;
            level?.Shake(2f);
            level?.Flash(Color.Gold * 0.5f, true);
            
            currentColor = Color.Gold;
            bodyGlow.Color = currentColor;
            wingGlow.Color = currentColor;
            swordGlow.Color = Color.Gold;
            swordGlow.Alpha = 1f;
            
            yield return 2f;
            
            knightSprite.Play("idle");
        }

        private IEnumerator Phase3Loop()
        {
            while (currentPhase == BossPhase.Phase3_Divine && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(8, 12);
                yield return ExecuteAttack(attack);
                yield return 1.5f;
            }
        }

        private IEnumerator EnterPhase4()
        {
            isTranscendent = true;
            knightSprite.Play("transcend");
            wingsSprite.Play("divine");
            auraSprite.Play("transcendent");
            Audio.Play("event:/morpho_transcendence", Position);
            
            var level = Scene as Level;
            level?.Shake(3f);
            level?.Flash(Color.White * 0.6f, true);
            
            // Heal slightly
            health = Math.Min(health + 200, maxHealth / 4);
            
            yield return 2.5f;
        }

        private IEnumerator Phase4Loop()
        {
            while (currentPhase == BossPhase.Phase4_Transcendent && health > 0)
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
            yield return TelegraphIntent(attack switch
            {
                AttackType.GracefulDash => BossTelegraphType.DashCyan,
                AttackType.WingGust => BossTelegraphType.PositioningOrange,
                AttackType.TeleportStrike => BossTelegraphType.TeleportYellow,
                AttackType.ScaleStorm => BossTelegraphType.PositioningOrange,
                AttackType.DivineJudgment => BossTelegraphType.SpecialPurple,
                AttackType.ParadiseLost => BossTelegraphType.SpecialPurple,
                AttackType.CelestialBeam => BossTelegraphType.PositioningOrange,
                AttackType.TranscendentSlash => BossTelegraphType.DashCyan,
                AttackType.InfiniteButterflies => BossTelegraphType.SpecialPurple,
                AttackType.DeltaStrike => BossTelegraphType.DashCyan,
                AttackType.Apotheosis => BossTelegraphType.SpecialPurple,
                _ => BossTelegraphType.DangerRed
            }, attack is AttackType.DivineJudgment or AttackType.ParadiseLost or AttackType.InfiniteButterflies or AttackType.Apotheosis ? 0.65f : 0.5f);

            switch (attack)
            {
                // Phase 1
                case AttackType.SwordSlash:
                    yield return SwordSlashAttack();
                    break;
                case AttackType.ButterflyBarrage:
                    yield return ButterflyBarrageAttack();
                    break;
                case AttackType.GracefulDash:
                    yield return GracefulDashAttack();
                    break;
                case AttackType.WingGust:
                    yield return WingGustAttack();
                    break;
                    
                // Phase 2
                case AttackType.FlameWing:
                    yield return FlameWingAttack();
                    break;
                case AttackType.SoulAbsorb:
                    yield return SoulAbsorbAttack();
                    break;
                case AttackType.TeleportStrike:
                    yield return TeleportStrikeAttack();
                    break;
                case AttackType.ScaleStorm:
                    yield return ScaleStormAttack();
                    break;
                    
                // Phase 3
                case AttackType.DivineJudgment:
                    yield return DivineJudgmentAttack();
                    break;
                case AttackType.HeavenlyBlades:
                    yield return HeavenlyBladesAttack();
                    break;
                case AttackType.ParadiseLost:
                    yield return ParadiseLostAttack();
                    break;
                case AttackType.CelestialBeam:
                    yield return CelestialBeamAttack();
                    break;
                    
                // Phase 4
                case AttackType.TranscendentSlash:
                    yield return TranscendentSlashAttack();
                    break;
                case AttackType.InfiniteButterflies:
                    yield return InfiniteButterfliesAttack();
                    break;
                case AttackType.DeltaStrike:
                    yield return DeltaStrikeAttack();
                    break;
                case AttackType.Apotheosis:
                    yield return ApotheosisAttack();
                    break;
            }
        }
        #endregion

        #region Phase 1 Attacks
        private IEnumerator SwordSlashAttack()
        {
            knightSprite.Play("attack");
            swordSprite.Play("slash");
            Audio.Play("event:/morpho_sword_slash", Position);
            
            var level = Scene as Level;
            
            for (int i = 0; i < 3; i++)
            {
                level?.Displacement.AddBurst(Position + new Vector2(30f + i * 20f, -24f), 0.5f, 16f, 48f, 0.3f);
                yield return 0.15f;
            }
            
            swordSprite.Play("idle");
            knightSprite.Play("idle");
        }

        private IEnumerator ButterflyBarrageAttack()
        {
            wingsSprite.Play("flap");
            Audio.Play("event:/morpho_butterfly_barrage", Position);
            
            var level = Scene as Level;
            
            for (int i = 0; i < 8; i++)
            {
                float angle = (i / 8f) * MathHelper.TwoPi;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                Vector2 spawnPos = Position + dir * 30f;
                
                butterflyPositions.Add(spawnPos);
                level?.Displacement.AddBurst(spawnPos, 0.4f, 12f, 32f, 0.3f);
                
                yield return 0.1f;
            }
        }

        private IEnumerator GracefulDashAttack()
        {
            knightSprite.Play("dash");
            Audio.Play("event:/morpho_graceful_dash", Position);
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * 450f;
            }
            
            var level = Scene as Level;
            
            for (float t = 0; t < 0.4f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.3f, 16f, 40f, 0.2f);
                yield return null;
            }
            
            Speed = Vector2.Zero;
            knightSprite.Play("idle");
        }

        private IEnumerator WingGustAttack()
        {
            wingsSprite.Play("spread");
            Audio.Play("event:/morpho_wing_gust", Position);
            
            var level = Scene as Level;
            level?.Shake(0.8f);
            
            // Push wave
            for (int i = 0; i < 3; i++)
            {
                level?.Displacement.AddBurst(Position, 0.8f, 40f + i * 40f, 80f + i * 40f, 0.5f);
                yield return 0.2f;
            }
            
            wingsSprite.Play("flap");
        }
        #endregion

        #region Phase 2 Attacks
        private IEnumerator FlameWingAttack()
        {
            wingsSprite.Play("flame");
            Audio.Play("event:/morpho_flame_wing", Position);
            
            var level = Scene as Level;
            
            // Fire trails from wings
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 firePos = Position + new Vector2(side * (20f + i * 15f), -24f);
                    level?.Displacement.AddBurst(firePos, 0.5f, 16f, 48f, 0.4f);
                    yield return 0.1f;
                }
            }
            
            wingsSprite.Play("flap");
        }

        private IEnumerator SoulAbsorbAttack()
        {
            Audio.Play("event:/morpho_soul_absorb", Position);
            
            var level = Scene as Level;
            level?.Flash(Color.Orange * 0.3f, true);
            
            // Pull effect
            for (float t = 0; t < 1.5f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.5f, 100f, 30f, 0.3f);
                yield return null;
            }
            
            // Absorb (slight heal)
            health = Math.Min(health + 25, maxHealth);
            bodyGlow.Alpha = 2f;
            yield return 0.3f;
            bodyGlow.Alpha = 1f;
        }

        private IEnumerator TeleportStrikeAttack()
        {
            Audio.Play("event:/morpho_teleport", Position);
            
            // Fade out
            Collidable = false;
            for (float t = 0; t < 0.3f; t += Engine.DeltaTime)
            {
                knightSprite.Color = Color.White * (1f - t / 0.3f);
                yield return null;
            }
            
            // Teleport behind player
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                float side = Calc.Random.Choose(-1, 1);
                Position = player.Position + new Vector2(side * 50f, 0f);
            }
            
            // Fade in and strike
            for (float t = 0; t < 0.2f; t += Engine.DeltaTime)
            {
                knightSprite.Color = Color.White * (t / 0.2f);
                yield return null;
            }
            
            Collidable = true;
            knightSprite.Color = Color.White;
            
            Audio.Play("event:/morpho_teleport_strike", Position);
            knightSprite.Play("attack");
            swordSprite.Play("slash");
            
            var level = Scene as Level;
            level?.Shake(1f);
            level?.Displacement.AddBurst(Position, 0.7f, 32f, 80f, 0.4f);
            
            yield return 0.4f;
            
            swordSprite.Play("idle");
            knightSprite.Play("idle");
        }

        private IEnumerator ScaleStormAttack()
        {
            wingsSprite.Play("flap");
            Audio.Play("event:/morpho_scale_storm", Position);
            
            var level = Scene as Level;
            
            // Scatter wing scales
            for (int i = 0; i < 20; i++)
            {
                float angle = Calc.Random.NextAngle();
                float dist = Calc.Random.Range(30f, 100f);
                Vector2 scalePos = Position + Calc.AngleToVector(angle, dist);
                
                level?.Displacement.AddBurst(scalePos, 0.3f, 8f, 24f, 0.2f);
                yield return 0.05f;
            }
        }
        #endregion

        #region Phase 3 Attacks
        private IEnumerator DivineJudgmentAttack()
        {
            knightSprite.Play("divine");
            swordSprite.Play("divine");
            Audio.Play("event:/morpho_divine_judgment", Position);
            
            var level = Scene as Level;
            
            // Raise sword
            swordGlow.Alpha = 2f;
            yield return 1f;
            
            // Judgment strike
            level?.Flash(Color.Gold * 0.5f, true);
            level?.Shake(2f);
            level?.Displacement.AddBurst(Position, 1.5f, 64f, 192f, 0.8f);
            
            yield return 0.5f;
            swordGlow.Alpha = 1f;
            knightSprite.Play("idle");
            swordSprite.Play("idle");
        }

        private IEnumerator HeavenlyBladesAttack()
        {
            Audio.Play("event:/morpho_heavenly_blades", Position);
            
            var level = Scene as Level;
            
            // Summon floating swords
            for (int i = 0; i < 6; i++)
            {
                float angle = (i / 6f) * MathHelper.TwoPi;
                Vector2 bladePos = Position + Calc.AngleToVector(angle, 80f);
                
                level?.Displacement.AddBurst(bladePos, 0.6f, 16f, 48f, 0.4f);
                yield return 0.15f;
            }
            
            yield return 0.5f;
            
            // Fire all blades at player
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    level?.Displacement.AddBurst(player.Position, 0.4f, 16f, 40f, 0.3f);
                    yield return 0.1f;
                }
            }
        }

        private IEnumerator ParadiseLostAttack()
        {
            Audio.Play("event:/morpho_paradise_lost", Position);
            
            var level = Scene as Level;
            level?.Shake(1.5f);
            
            // Dark energy release
            auraSprite.Color = Color.Purple;
            
            for (int i = 0; i < 4; i++)
            {
                level?.Displacement.AddBurst(Position, 1f, 30f + i * 50f, 60f + i * 50f, 0.6f);
                yield return 0.3f;
            }
            
            auraSprite.Color = Color.White;
        }

        private IEnumerator CelestialBeamAttack()
        {
            knightSprite.Play("divine");
            Audio.Play("event:/morpho_celestial_beam", Position);
            
            bodyGlow.Alpha = 3f;
            
            var level = Scene as Level;
            
            // Charge
            yield return 1f;
            
            // Fire beam downward
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                Vector2 beamTarget = new Vector2(player.Position.X, Position.Y + 200f);
                
                level?.Flash(Color.Gold * 0.4f, true);
                level?.Shake(1.5f);
                
                for (float y = Position.Y; y < beamTarget.Y; y += 20f)
                {
                    level?.Displacement.AddBurst(new Vector2(player.Position.X, y), 0.5f, 24f, 48f, 0.3f);
                }
            }
            
            yield return 0.5f;
            bodyGlow.Alpha = 1f;
            knightSprite.Play("idle");
        }
        #endregion

        #region Phase 4 Attacks
        private IEnumerator TranscendentSlashAttack()
        {
            knightSprite.Play("transcend");
            swordSprite.Play("slash");
            Audio.Play("event:/morpho_transcendent_slash", Position);
            
            var level = Scene as Level;
            
            // Multi-directional slashes
            for (int i = 0; i < 8; i++)
            {
                float angle = (i / 8f) * MathHelper.TwoPi;
                Vector2 slashDir = Calc.AngleToVector(angle, 100f);
                
                level?.Displacement.AddBurst(Position + slashDir * 0.5f, 0.6f, 16f, 80f, 0.4f);
            }
            
            level?.Shake(1.5f);
            level?.Flash(currentColor * 0.3f, true);
            
            yield return 0.5f;
            
            swordSprite.Play("idle");
            knightSprite.Play("idle");
        }

        private IEnumerator InfiniteButterfliesAttack()
        {
            Audio.Play("event:/morpho_infinite_butterflies", Position);
            
            var level = Scene as Level;
            
            // Massive butterfly swarm
            for (int wave = 0; wave < 3; wave++)
            {
                for (int i = 0; i < 16; i++)
                {
                    float angle = (i / 16f) * MathHelper.TwoPi + (wave * 0.3f);
                    Vector2 dir = Calc.AngleToVector(angle, 40f + wave * 30f);
                    Vector2 spawnPos = Position + dir;
                    
                    butterflyPositions.Add(spawnPos);
                    level?.Displacement.AddBurst(spawnPos, 0.3f, 8f, 24f, 0.2f);
                }
                
                yield return 0.3f;
            }
        }

        private IEnumerator DeltaStrikeAttack()
        {
            knightSprite.Play("transcend");
            Audio.Play("event:/morpho_delta_strike", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Triangle formation attack
            Vector2[] trianglePoints = new Vector2[3];
            if (player != null)
            {
                trianglePoints[0] = player.Position + new Vector2(0f, -80f);
                trianglePoints[1] = player.Position + new Vector2(-70f, 60f);
                trianglePoints[2] = player.Position + new Vector2(70f, 60f);
            }
            
            for (int i = 0; i < 3; i++)
            {
                // Teleport to triangle point
                Collidable = false;
                knightSprite.Color = Color.White * 0.3f;
                
                yield return 0.1f;
                
                Position = trianglePoints[i];
                knightSprite.Color = Color.White;
                Collidable = true;
                
                // Strike
                swordSprite.Play("slash");
                level?.Displacement.AddBurst(Position, 0.7f, 32f, 80f, 0.4f);
                
                yield return 0.2f;
            }
            
            swordSprite.Play("idle");
            knightSprite.Play("idle");
        }

        private IEnumerator ApotheosisAttack()
        {
            knightSprite.Play("transcend");
            auraSprite.Play("transcendent");
            Audio.Play("event:/morpho_apotheosis", Position);
            
            var level = Scene as Level;
            
            // Ultimate divine transformation attack
            bodyGlow.Alpha = 4f;
            wingGlow.Alpha = 4f;
            swordGlow.Alpha = 4f;
            
            // Charge up
            for (float t = 0; t < 2f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.5f, 30f, 60f, 0.3f);
                yield return null;
            }
            
            // Release
            level?.Flash(Color.White, true);
            level?.Shake(3f);
            
            for (int i = 0; i < 10; i++)
            {
                level?.Displacement.AddBurst(Position, 1.5f, i * 30f, i * 30f + 50f, 0.6f);
                yield return 0.1f;
            }
            
            bodyGlow.Alpha = 1f;
            wingGlow.Alpha = 1f;
            swordGlow.Alpha = 1f;
            
            yield return 0.5f;
            knightSprite.Play("idle");
        }
        #endregion

        #region Damage and Defeat
        public override void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            health -= damage;
            Audio.Play("event:/morpho_damage", Position);
            
            var level = Scene as Level;
            level?.Shake(0.4f);
            
            // Flash effect
            knightSprite.Color = Color.Red;
            Add(new Coroutine(FlashReset()));
            
            if (health <= 0)
            {
                Defeat();
            }
        }

        private IEnumerator FlashReset()
        {
            yield return 0.1f;
            knightSprite.Color = Color.White;
        }

        private void Defeat()
        {
            isDefeated = true;
            currentPhase = BossPhase.Defeated;
            Add(new Coroutine(DefeatSequence()));
        }

        private IEnumerator DefeatSequence()
        {
            ambientLoop.Stop();
            knightSprite.Play("divine");
            Audio.Play("event:/morpho_defeat", Position);
            
            var level = Scene as Level;
            
            // Wings fold
            wingsSprite.Play("folded");
            
            // Light fades
            for (float t = 0; t < 2f; t += Engine.DeltaTime)
            {
                bodyGlow.Alpha = 1f - (t / 2f);
                wingGlow.Alpha = 1f - (t / 2f);
                swordGlow.Alpha = 1f - (t / 2f);
                
                level?.Displacement.AddBurst(Position, 0.4f, 20f, 50f, 0.3f);
                yield return null;
            }
            
            // Transform back to butterfly
            level?.Flash(Color.Orange * 0.5f, true);
            
            for (int i = 0; i < 30; i++)
            {
                butterflyPositions.Add(Position + Calc.Random.Range(Vector2.One * -30f, Vector2.One * 30f));
            }
            
            yield return 1f;
            
            level?.Session.SetFlag("morpho_knight_delta_boss_defeated");
            RemoveSelf();
        }
        #endregion

        #region Cleanup
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            ambientLoop?.Stop();
        }
        #endregion
    }
}
