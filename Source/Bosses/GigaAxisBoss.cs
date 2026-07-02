using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// Giga Axis Boss - The ultimate machine overlord
    /// An evolved form of Axis with devastating technological power
    /// Multi-phase boss with mech transformations and weapon systems
    /// Sprite path: characters/DZ/gigaaxis/
    /// </summary>
    [CustomEntity(ids: "DZ/GigaAxisBoss")]
    [Tracked]
    public class GigaAxisBoss : BossActor
    {
        #region Enums and Constants
        public enum BossPhase
        {
            PowerUp,
            Phase1_Assault,
            Phase2_Artillery,
            Phase3_Overdrive,
            Phase4_GigaCore,
            Defeated
        }

        public enum AttackType
        {
            // Phase 1 Attacks - Assault Mode
            RapidFire,
            MissileSalvo,
            LaserSweep,
            BoostCharge,
            
            // Phase 2 Attacks - Artillery Mode
            OrbitalCannon,
            ClusterBombs,
            ElectromagneticPulse,
            SiegeBarrage,
            
            // Phase 3 Attacks - Overdrive Mode
            OverclockBurst,
            PlasmaStorm,
            QuantumShift,
            SystemsMaximum,
            
            // Phase 4 Attacks - Giga Core
            GigaBeam,
            CoreMeltdown,
            TotalAnnihilation,
            FinalProtocol
        }
        #endregion

        #region Properties
        private int health = 2500;
        private int maxHealth = 2500;
        private bool isDefeated = false;
        private BossPhase currentPhase = BossPhase.PowerUp;
        
        private Sprite bodySprite;
        private Sprite armsSprite;
        private Sprite cannonSprite;
        private Sprite wingsSprite;
        private Sprite coreSprite;
        private List<Sprite> drones = new List<Sprite>();
        private VertexLight coreGlow;
        private VertexLight eyeGlow;
        private VertexLight weaponGlow;
        private SoundSource engineLoop;
        private SoundSource warningAlarm;
        
        private bool isOverdriveActive = false;
        private float weaponCharge = 0f;
        private float shieldIntegrity = 100f;
        private List<Vector2> targetLocks = new List<Vector2>();
        #endregion

        #region Constructors
        public GigaAxisBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "giga_axis_boss", new Vector2(1.5f, 1.5f), 250f, true, true, 1f, 
                   new Hitbox(80f, 96f, -40f, -96f))
        {
            health = data.Int("health", 2500);
            maxHealth = data.Int("maxHealth", 2500);
            SetupVisuals();
        }

        public GigaAxisBoss(Vector2 position) 
            : base(position, "giga_axis_boss", new Vector2(1.5f, 1.5f), 250f, true, true, 1f, 
                   new Hitbox(80f, 96f, -40f, -96f))
        {
            SetupVisuals();
        }
        #endregion

        #region Setup
        private void SetupVisuals()
        {
            // Main body sprite
            Add(bodySprite = new Sprite(GFX.Game, "characters/DZ/gigaaxis/"));
            bodySprite.AddLoop("offline", "body_offline", 0.2f);
            bodySprite.AddLoop("idle", "body_idle", 0.1f);
            bodySprite.AddLoop("assault", "body_assault", 0.08f);
            bodySprite.AddLoop("artillery", "body_artillery", 0.1f);
            bodySprite.AddLoop("overdrive", "body_overdrive", 0.05f);
            bodySprite.AddLoop("giga", "body_giga", 0.04f);
            bodySprite.Play("offline");
            bodySprite.CenterOrigin();
            
            // Arms/weapons sprite
            Add(armsSprite = new Sprite(GFX.Game, "characters/DZ/gigaaxis/"));
            armsSprite.AddLoop("retracted", "arms_retracted", 0.1f);
            armsSprite.AddLoop("ready", "arms_ready", 0.08f);
            armsSprite.AddLoop("fire", "arms_fire", 0.04f);
            armsSprite.CenterOrigin();
            armsSprite.Position = new Vector2(0f, -48f);
            
            // Main cannon sprite
            Add(cannonSprite = new Sprite(GFX.Game, "characters/DZ/gigaaxis/"));
            cannonSprite.AddLoop("hidden", "cannon_hidden", 0.1f);
            cannonSprite.AddLoop("deploy", "cannon_deploy", 0.08f);
            cannonSprite.AddLoop("charge", "cannon_charge", 0.05f);
            cannonSprite.AddLoop("fire", "cannon_fire", 0.03f);
            cannonSprite.CenterOrigin();
            cannonSprite.Position = new Vector2(0f, -70f);
            cannonSprite.Visible = false;
            
            // Wings/thrusters sprite
            Add(wingsSprite = new Sprite(GFX.Game, "characters/DZ/gigaaxis/"));
            wingsSprite.AddLoop("folded", "wings_folded", 0.1f);
            wingsSprite.AddLoop("spread", "wings_spread", 0.08f);
            wingsSprite.AddLoop("boost", "wings_boost", 0.04f);
            wingsSprite.CenterOrigin();
            wingsSprite.Position = new Vector2(0f, -60f);
            
            // Core sprite (weak point)
            Add(coreSprite = new Sprite(GFX.Game, "characters/DZ/gigaaxis/"));
            coreSprite.AddLoop("protected", "core_protected", 0.1f);
            coreSprite.AddLoop("exposed", "core_exposed", 0.08f);
            coreSprite.AddLoop("critical", "core_critical", 0.04f);
            coreSprite.CenterOrigin();
            coreSprite.Position = new Vector2(0f, -48f);
            coreSprite.Visible = false;
            
            // Create drones
            for (int i = 0; i < 4; i++)
            {
                var drone = new Sprite(GFX.Game, "characters/DZ/gigaaxis/");
                drone.AddLoop("idle", "drone_idle", 0.08f);
                drone.AddLoop("attack", "drone_attack", 0.05f);
                drone.CenterOrigin();
                drone.Visible = false;
                Add(drone);
                drones.Add(drone);
            }
            
            // Core glow
            Add(coreGlow = new VertexLight(Color.Cyan, 1f, 48, 80));
            coreGlow.Position = new Vector2(0f, -48f);
            coreGlow.Alpha = 0f;
            
            // Eye glow
            Add(eyeGlow = new VertexLight(Color.Red, 1f, 24, 40));
            eyeGlow.Position = new Vector2(0f, -80f);
            
            // Weapon glow
            Add(weaponGlow = new VertexLight(Color.Orange, 0.8f, 32, 56));
            weaponGlow.Position = new Vector2(0f, -70f);
            weaponGlow.Alpha = 0f;
            
            // Sound sources
            Add(engineLoop = new SoundSource());
            Add(warningAlarm = new SoundSource());
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
            
            // Update drone positions (orbit around boss)
            for (int i = 0; i < drones.Count; i++)
            {
                if (drones[i].Visible)
                {
                    float angle = (i / (float)drones.Count) * MathHelper.TwoPi + Scene.TimeActive;
                    float radius = 80f;
                    drones[i].Position = new Vector2(
                        (float)Math.Cos(angle) * radius,
                        (float)Math.Sin(angle) * radius - 48f
                    );
                }
            }
            
            // Pulsing effects
            if (currentPhase != BossPhase.PowerUp && currentPhase != BossPhase.Defeated)
            {
                eyeGlow.Alpha = 0.8f + (float)Math.Sin(Scene.TimeActive * 3f) * 0.2f;
                
                if (isOverdriveActive)
                {
                    coreGlow.Alpha = 1.5f + (float)Math.Sin(Scene.TimeActive * 6f) * 0.5f;
                    bodySprite.Color = Color.Lerp(Color.White, Color.Red, (float)Math.Sin(Scene.TimeActive * 4f) * 0.3f + 0.3f);
                }
            }
            
            // Weapon charge visual
            if (weaponCharge > 0f)
            {
                weaponGlow.Alpha = weaponCharge / 100f;
            }
        }
        #endregion

        #region Main Boss Routine
        private IEnumerator BossRoutine()
        {
            yield return PowerUpSequence();
            
            while (currentPhase != BossPhase.Defeated)
            {
                switch (currentPhase)
                {
                    case BossPhase.Phase1_Assault:
                        yield return Phase1Loop();
                        break;
                    case BossPhase.Phase2_Artillery:
                        yield return Phase2Loop();
                        break;
                    case BossPhase.Phase3_Overdrive:
                        yield return Phase3Loop();
                        break;
                    case BossPhase.Phase4_GigaCore:
                        yield return Phase4Loop();
                        break;
                }
                
                CheckPhaseTransition();
            }
        }

        private IEnumerator PowerUpSequence()
        {
            // System boot sequence
            Audio.Play("event:/gigaaxis_boot", Position);
            
            var level = Scene as Level;
            
            // Eyes power on
            yield return 0.5f;
            eyeGlow.Alpha = 0f;
            for (float t = 0; t < 1f; t += Engine.DeltaTime)
            {
                eyeGlow.Alpha = t;
                yield return null;
            }
            
            Audio.Play("event:/gigaaxis_online", Position);
            bodySprite.Play("idle");
            engineLoop.Play("event:/gigaaxis_engine_loop");
            
            // Systems online
            armsSprite.Play("ready");
            wingsSprite.Play("spread");
            
            level?.Shake(1f);
            
            yield return 1f;
            
            currentPhase = BossPhase.Phase1_Assault;
            bodySprite.Play("assault");
        }

        private void CheckPhaseTransition()
        {
            float healthPercent = (float)health / maxHealth;
            
            if (healthPercent <= 0)
            {
                currentPhase = BossPhase.Defeated;
            }
            else if (healthPercent <= 0.2f && currentPhase != BossPhase.Phase4_GigaCore)
            {
                currentPhase = BossPhase.Phase4_GigaCore;
                Add(new Coroutine(EnterPhase4()));
            }
            else if (healthPercent <= 0.45f && currentPhase == BossPhase.Phase2_Artillery)
            {
                currentPhase = BossPhase.Phase3_Overdrive;
                Add(new Coroutine(EnterPhase3()));
            }
            else if (healthPercent <= 0.7f && currentPhase == BossPhase.Phase1_Assault)
            {
                currentPhase = BossPhase.Phase2_Artillery;
                Add(new Coroutine(EnterPhase2()));
            }
        }
        #endregion

        #region Phase Routines
        private IEnumerator Phase1Loop()
        {
            while (currentPhase == BossPhase.Phase1_Assault && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(0, 4);
                yield return ExecuteAttack(attack);
                yield return 2f;
            }
        }

        private IEnumerator EnterPhase2()
        {
            bodySprite.Play("artillery");
            Audio.Play("event:/gigaaxis_artillery_mode", Position);
            
            var level = Scene as Level;
            level?.Shake(1.5f);
            
            // Deploy main cannon
            cannonSprite.Visible = true;
            cannonSprite.Play("deploy");
            
            // Activate drones
            foreach (var drone in drones)
            {
                drone.Visible = true;
                drone.Play("idle");
            }
            
            yield return 2f;
        }

        private IEnumerator Phase2Loop()
        {
            while (currentPhase == BossPhase.Phase2_Artillery && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(4, 8);
                yield return ExecuteAttack(attack);
                yield return 1.8f;
            }
        }

        private IEnumerator EnterPhase3()
        {
            isOverdriveActive = true;
            bodySprite.Play("overdrive");
            Audio.Play("event:/gigaaxis_overdrive", Position);
            warningAlarm.Play("event:/gigaaxis_warning_alarm");
            
            var level = Scene as Level;
            level?.Shake(2f);
            level?.Flash(Color.Red * 0.4f, true);
            
            coreGlow.Color = Color.Red;
            coreGlow.Alpha = 1.5f;
            eyeGlow.Color = Color.Yellow;
            
            wingsSprite.Play("boost");
            
            yield return 2f;
        }

        private IEnumerator Phase3Loop()
        {
            while (currentPhase == BossPhase.Phase3_Overdrive && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(8, 12);
                yield return ExecuteAttack(attack);
                yield return 1.5f;
            }
        }

        private IEnumerator EnterPhase4()
        {
            bodySprite.Play("giga");
            Audio.Play("event:/gigaaxis_giga_mode", Position);
            
            var level = Scene as Level;
            level?.Shake(3f);
            level?.Flash(Color.Cyan * 0.5f, true);
            
            // Expose core
            coreSprite.Visible = true;
            coreSprite.Play("exposed");
            coreGlow.Color = Color.Cyan;
            coreGlow.Alpha = 2f;
            
            // All weapons active
            cannonSprite.Play("charge");
            
            yield return 2f;
        }

        private IEnumerator Phase4Loop()
        {
            while (currentPhase == BossPhase.Phase4_GigaCore && health > 0)
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
                AttackType.MissileSalvo => BossTelegraphType.PositioningOrange,
                AttackType.LaserSweep => BossTelegraphType.PositioningOrange,
                AttackType.BoostCharge => BossTelegraphType.DashCyan,
                AttackType.OrbitalCannon => BossTelegraphType.PositioningOrange,
                AttackType.ClusterBombs => BossTelegraphType.PositioningOrange,
                AttackType.ElectromagneticPulse => BossTelegraphType.SpecialPurple,
                AttackType.QuantumShift => BossTelegraphType.TeleportYellow,
                AttackType.SystemsMaximum => BossTelegraphType.SpecialPurple,
                AttackType.GigaBeam => BossTelegraphType.PositioningOrange,
                AttackType.CoreMeltdown => BossTelegraphType.SpecialPurple,
                AttackType.TotalAnnihilation => BossTelegraphType.SpecialPurple,
                AttackType.FinalProtocol => BossTelegraphType.SpecialPurple,
                _ => BossTelegraphType.DangerRed
            }, attack is AttackType.SystemsMaximum or AttackType.TotalAnnihilation or AttackType.FinalProtocol ? 0.65f : 0.5f);

            switch (attack)
            {
                // Phase 1
                case AttackType.RapidFire:
                    yield return RapidFireAttack();
                    break;
                case AttackType.MissileSalvo:
                    yield return MissileSalvoAttack();
                    break;
                case AttackType.LaserSweep:
                    yield return LaserSweepAttack();
                    break;
                case AttackType.BoostCharge:
                    yield return BoostChargeAttack();
                    break;
                    
                // Phase 2
                case AttackType.OrbitalCannon:
                    yield return OrbitalCannonAttack();
                    break;
                case AttackType.ClusterBombs:
                    yield return ClusterBombsAttack();
                    break;
                case AttackType.ElectromagneticPulse:
                    yield return ElectromagneticPulseAttack();
                    break;
                case AttackType.SiegeBarrage:
                    yield return SiegeBarrageAttack();
                    break;
                    
                // Phase 3
                case AttackType.OverclockBurst:
                    yield return OverclockBurstAttack();
                    break;
                case AttackType.PlasmaStorm:
                    yield return PlasmaStormAttack();
                    break;
                case AttackType.QuantumShift:
                    yield return QuantumShiftAttack();
                    break;
                case AttackType.SystemsMaximum:
                    yield return SystemsMaximumAttack();
                    break;
                    
                // Phase 4
                case AttackType.GigaBeam:
                    yield return GigaBeamAttack();
                    break;
                case AttackType.CoreMeltdown:
                    yield return CoreMeltdownAttack();
                    break;
                case AttackType.TotalAnnihilation:
                    yield return TotalAnnihilationAttack();
                    break;
                case AttackType.FinalProtocol:
                    yield return FinalProtocolAttack();
                    break;
            }
        }
        #endregion

        #region Phase 1 Attacks
        private IEnumerator RapidFireAttack()
        {
            armsSprite.Play("fire");
            Audio.Play("event:/gigaaxis_rapid_fire", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            for (int i = 0; i < 10; i++)
            {
                if (player != null)
                {
                    Vector2 firePos = Position + new Vector2(Calc.Random.Choose(-30f, 30f), -50f);
                    level?.Displacement.AddBurst(firePos, 0.3f, 12f, 32f, 0.2f);
                }
                
                yield return 0.1f;
            }
            
            armsSprite.Play("ready");
        }

        private IEnumerator MissileSalvoAttack()
        {
            Audio.Play("event:/gigaaxis_missile_salvo", Position);
            
            var level = Scene as Level;
            
            // Launch tracking missiles
            for (int i = 0; i < 6; i++)
            {
                Vector2 launchPos = Position + new Vector2((i - 2.5f) * 20f, -60f);
                level?.Displacement.AddBurst(launchPos, 0.5f, 16f, 40f, 0.3f);
                
                yield return 0.15f;
            }
            
            level?.Shake(0.8f);
        }

        private IEnumerator LaserSweepAttack()
        {
            armsSprite.Play("fire");
            Audio.Play("event:/gigaaxis_laser_sweep", Position);
            
            var level = Scene as Level;
            weaponGlow.Alpha = 1.5f;
            
            // Sweep laser across arena
            for (float angle = -0.5f; angle <= 0.5f; angle += 0.05f)
            {
                Vector2 laserDir = Calc.AngleToVector(-MathHelper.PiOver2 + angle, 1f);
                
                for (float dist = 30f; dist < 250f; dist += 30f)
                {
                    level?.Displacement.AddBurst(Position + laserDir * dist, 0.3f, 12f, 32f, 0.2f);
                }
                
                yield return 0.03f;
            }
            
            weaponGlow.Alpha = 0f;
            armsSprite.Play("ready");
        }

        private IEnumerator BoostChargeAttack()
        {
            wingsSprite.Play("boost");
            Audio.Play("event:/gigaaxis_boost_charge", Position);
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * 500f;
            }
            
            var level = Scene as Level;
            
            for (float t = 0; t < 0.5f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.4f, 24f, 56f, 0.3f);
                yield return null;
            }
            
            Speed = Vector2.Zero;
            level?.Shake(1.5f);
            level?.Displacement.AddBurst(Position, 0.8f, 48f, 96f, 0.5f);
            
            wingsSprite.Play("spread");
        }
        #endregion

        #region Phase 2 Attacks
        private IEnumerator OrbitalCannonAttack()
        {
            cannonSprite.Play("charge");
            Audio.Play("event:/gigaaxis_orbital_cannon_charge", Position);
            
            var level = Scene as Level;
            
            weaponCharge = 0f;
            for (float t = 0; t < 1.5f; t += Engine.DeltaTime)
            {
                weaponCharge = (t / 1.5f) * 100f;
                yield return null;
            }
            
            cannonSprite.Play("fire");
            Audio.Play("event:/gigaaxis_orbital_cannon_fire", Position);
            
            level?.Flash(Color.Orange * 0.4f, true);
            level?.Shake(2f);
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                // Massive beam to player position
                for (float y = Position.Y - 50f; y < Position.Y + 300f; y += 20f)
                {
                    level?.Displacement.AddBurst(new Vector2(player.Position.X, y), 0.8f, 32f, 80f, 0.5f);
                }
            }
            
            weaponCharge = 0f;
            cannonSprite.Play("deploy");
        }

        private IEnumerator ClusterBombsAttack()
        {
            Audio.Play("event:/gigaaxis_cluster_bombs", Position);
            
            var level = Scene as Level;
            
            // Drop cluster bombs
            for (int i = 0; i < 5; i++)
            {
                Vector2 bombPos = Position + new Vector2(Calc.Random.Range(-100f, 100f), 50f);
                
                level?.Displacement.AddBurst(bombPos, 0.6f, 24f, 64f, 0.4f);
                
                // Each bomb creates smaller explosions
                yield return 0.2f;
                
                for (int j = 0; j < 4; j++)
                {
                    Vector2 subPos = bombPos + Calc.Random.Range(Vector2.One * -30f, Vector2.One * 30f);
                    level?.Displacement.AddBurst(subPos, 0.4f, 16f, 40f, 0.3f);
                }
            }
            
            level?.Shake(1.5f);
        }

        private IEnumerator ElectromagneticPulseAttack()
        {
            Audio.Play("event:/gigaaxis_emp", Position);
            
            var level = Scene as Level;
            
            coreGlow.Alpha = 3f;
            
            yield return 0.5f;
            
            // EMP wave
            for (int i = 0; i < 4; i++)
            {
                level?.Displacement.AddBurst(Position, 1f, i * 60f, i * 60f + 80f, 0.6f);
                yield return 0.2f;
            }
            
            level?.Flash(Color.Cyan * 0.4f, true);
            level?.Shake(1.5f);
            
            coreGlow.Alpha = 1f;
        }

        private IEnumerator SiegeBarrageAttack()
        {
            Audio.Play("event:/gigaaxis_siege_barrage", Position);
            
            var level = Scene as Level;
            
            // Drones attack
            foreach (var drone in drones)
            {
                drone.Play("attack");
            }
            
            // Coordinated barrage from all weapons
            for (int wave = 0; wave < 3; wave++)
            {
                armsSprite.Play("fire");
                
                for (int i = 0; i < 8; i++)
                {
                    float angle = (i / 8f) * MathHelper.TwoPi + (wave * 0.3f);
                    Vector2 dir = Calc.AngleToVector(angle, 80f);
                    level?.Displacement.AddBurst(Position + dir, 0.5f, 20f, 50f, 0.4f);
                }
                
                yield return 0.4f;
            }
            
            armsSprite.Play("ready");
            foreach (var drone in drones)
            {
                drone.Play("idle");
            }
        }
        #endregion

        #region Phase 3 Attacks
        private IEnumerator OverclockBurstAttack()
        {
            Audio.Play("event:/gigaaxis_overclock", Position);
            
            var level = Scene as Level;
            
            coreGlow.Alpha = 4f;
            eyeGlow.Alpha = 2f;
            
            // Rapid series of attacks
            for (int i = 0; i < 15; i++)
            {
                armsSprite.Play("fire");
                
                float angle = Calc.Random.NextAngle();
                Vector2 dir = Calc.AngleToVector(angle, 60f);
                level?.Displacement.AddBurst(Position + dir, 0.4f, 16f, 48f, 0.3f);
                
                yield return 0.08f;
            }
            
            armsSprite.Play("ready");
            coreGlow.Alpha = 1.5f;
            eyeGlow.Alpha = 1f;
        }

        private IEnumerator PlasmaStormAttack()
        {
            Audio.Play("event:/gigaaxis_plasma_storm", Position);
            
            var level = Scene as Level;
            level?.Shake(1.5f);
            
            // Chaotic plasma projectiles
            for (int i = 0; i < 20; i++)
            {
                Vector2 spawnPos = Position + Calc.Random.Range(Vector2.One * -50f, Vector2.One * 50f);
                Vector2 targetPos = spawnPos + Calc.Random.Range(Vector2.One * -100f, Vector2.One * 100f);
                
                level?.Displacement.AddBurst(spawnPos, 0.5f, 16f, 48f, 0.4f);
                
                yield return 0.05f;
            }
        }

        private IEnumerator QuantumShiftAttack()
        {
            Audio.Play("event:/gigaaxis_quantum_shift", Position);
            
            // Teleport around the arena rapidly
            Collidable = false;
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            for (int i = 0; i < 4; i++)
            {
                // Fade out
                bodySprite.Color = Color.White * 0.3f;
                level?.Displacement.AddBurst(Position, 0.5f, 32f, 64f, 0.3f);
                
                yield return 0.1f;
                
                // Teleport
                if (player != null)
                {
                    Position = player.Position + Calc.Random.Range(Vector2.One * -80f, Vector2.One * 80f);
                }
                
                // Fade in and attack
                bodySprite.Color = Color.White;
                level?.Displacement.AddBurst(Position, 0.6f, 24f, 64f, 0.4f);
                
                armsSprite.Play("fire");
                yield return 0.15f;
            }
            
            Collidable = true;
            armsSprite.Play("ready");
        }

        private IEnumerator SystemsMaximumAttack()
        {
            Audio.Play("event:/gigaaxis_systems_maximum", Position);
            
            var level = Scene as Level;
            
            // All systems at maximum power
            coreGlow.Alpha = 5f;
            weaponGlow.Alpha = 3f;
            
            cannonSprite.Play("fire");
            armsSprite.Play("fire");
            foreach (var drone in drones)
            {
                drone.Play("attack");
            }
            
            // Massive multi-directional assault
            for (int wave = 0; wave < 4; wave++)
            {
                for (int i = 0; i < 16; i++)
                {
                    float angle = (i / 16f) * MathHelper.TwoPi + (wave * 0.1f);
                    Vector2 dir = Calc.AngleToVector(angle, 1f);
                    
                    for (float dist = 40f; dist < 200f; dist += 40f)
                    {
                        level?.Displacement.AddBurst(Position + dir * dist, 0.5f, 16f, 40f, 0.3f);
                    }
                }
                
                level?.Shake(1f);
                yield return 0.25f;
            }
            
            coreGlow.Alpha = 1.5f;
            weaponGlow.Alpha = 0f;
            cannonSprite.Play("deploy");
            armsSprite.Play("ready");
            foreach (var drone in drones)
            {
                drone.Play("idle");
            }
        }
        #endregion

        #region Phase 4 Attacks
        private IEnumerator GigaBeamAttack()
        {
            coreSprite.Play("critical");
            cannonSprite.Play("charge");
            Audio.Play("event:/gigaaxis_giga_beam_charge", Position);
            
            var level = Scene as Level;
            
            coreGlow.Alpha = 6f;
            
            // Long charge up
            for (float t = 0; t < 2f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.4f, 60f, 30f, 0.3f);
                level?.Shake(t * 0.5f);
                yield return null;
            }
            
            cannonSprite.Play("fire");
            Audio.Play("event:/gigaaxis_giga_beam_fire", Position);
            
            level?.Flash(Color.Cyan, true);
            level?.Shake(3f);
            
            // Massive beam sweep
            for (float angle = -0.3f; angle <= 0.3f; angle += 0.02f)
            {
                Vector2 beamDir = Calc.AngleToVector(-MathHelper.PiOver2 + angle, 1f);
                
                for (float dist = 50f; dist < 400f; dist += 30f)
                {
                    level?.Displacement.AddBurst(Position + beamDir * dist, 0.6f, 24f, 64f, 0.4f);
                }
                
                yield return 0.01f;
            }
            
            coreGlow.Alpha = 2f;
            coreSprite.Play("exposed");
            cannonSprite.Play("deploy");
        }

        private IEnumerator CoreMeltdownAttack()
        {
            coreSprite.Play("critical");
            Audio.Play("event:/gigaaxis_core_meltdown", Position);
            
            var level = Scene as Level;
            
            coreGlow.Color = Color.Red;
            coreGlow.Alpha = 8f;
            
            // Unstable energy release
            for (float t = 0; t < 2f; t += Engine.DeltaTime)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = Calc.Random.NextAngle();
                    float dist = Calc.Random.Range(30f, 100f);
                    level?.Displacement.AddBurst(Position + Calc.AngleToVector(angle, dist), 0.5f, 20f, 50f, 0.4f);
                }
                
                level?.Shake(0.5f + t);
                yield return null;
            }
            
            level?.Flash(Color.Red * 0.5f, true);
            
            coreGlow.Color = Color.Cyan;
            coreGlow.Alpha = 2f;
            coreSprite.Play("exposed");
        }

        private IEnumerator TotalAnnihilationAttack()
        {
            Audio.Play("event:/gigaaxis_total_annihilation", Position);
            
            var level = Scene as Level;
            
            // Everything fires at once
            coreGlow.Alpha = 10f;
            weaponGlow.Alpha = 5f;
            
            cannonSprite.Play("fire");
            armsSprite.Play("fire");
            foreach (var drone in drones)
            {
                drone.Play("attack");
            }
            
            // Ultimate barrage
            for (int wave = 0; wave < 6; wave++)
            {
                for (int i = 0; i < 24; i++)
                {
                    float angle = (i / 24f) * MathHelper.TwoPi + (wave * 0.15f);
                    Vector2 dir = Calc.AngleToVector(angle, 1f);
                    
                    level?.Displacement.AddBurst(Position + dir * (50f + wave * 30f), 0.6f, 20f, 56f, 0.4f);
                }
                
                level?.Shake(1.5f);
                level?.Flash(Color.Cyan * 0.2f, true);
                yield return 0.2f;
            }
            
            coreGlow.Alpha = 2f;
            weaponGlow.Alpha = 0f;
        }

        private IEnumerator FinalProtocolAttack()
        {
            coreSprite.Play("critical");
            Audio.Play("event:/gigaaxis_final_protocol", Position);
            
            var level = Scene as Level;
            
            // Activate all emergency systems
            warningAlarm.Play("event:/gigaaxis_critical_alarm");
            
            coreGlow.Alpha = 15f;
            bodySprite.Color = Color.Red;
            
            // Charge up everything
            for (float t = 0; t < 3f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.5f, 100f, 30f, 0.3f);
                level?.Shake(t);
                yield return null;
            }
            
            // Ultimate release
            level?.Flash(Color.White, true);
            level?.Shake(5f);
            
            for (int ring = 0; ring < 8; ring++)
            {
                level?.Displacement.AddBurst(Position, 2f, ring * 50f, ring * 50f + 80f, 0.8f);
                yield return 0.1f;
            }
            
            // Self damage from strain
            TakeDamage(50);
            
            warningAlarm.Stop();
            coreGlow.Alpha = 2f;
            bodySprite.Color = Color.White;
            coreSprite.Play("exposed");
        }
        #endregion

        #region Damage and Defeat
        public override void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            // Extra damage when core is exposed
            if (currentPhase == BossPhase.Phase4_GigaCore)
            {
                damage = (int)(damage * 1.5f);
            }
            
            health -= damage;
            Audio.Play("event:/gigaaxis_damage", Position);
            
            var level = Scene as Level;
            level?.Shake(0.4f);
            
            // Flash effect
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
            if (!isOverdriveActive)
            {
                bodySprite.Color = Color.White;
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
            engineLoop.Stop();
            warningAlarm.Stop();
            Audio.Play("event:/gigaaxis_critical_failure", Position);
            
            var level = Scene as Level;
            
            coreSprite.Play("critical");
            
            // Systems failing
            for (int i = 0; i < 8; i++)
            {
                Vector2 explosionPos = Position + Calc.Random.Range(Vector2.One * -50f, Vector2.One * 50f);
                Audio.Play("event:/gigaaxis_explosion", explosionPos);
                level?.Displacement.AddBurst(explosionPos, 0.8f, 32f, 80f, 0.5f);
                level?.Shake(1f);
                
                yield return 0.3f;
            }
            
            // Drones crash
            foreach (var drone in drones)
            {
                drone.Visible = false;
            }
            
            // Final explosion
            Audio.Play("event:/gigaaxis_final_explosion", Position);
            level?.Flash(Color.White, true);
            level?.Shake(4f);
            
            for (int i = 0; i < 5; i++)
            {
                level?.Displacement.AddBurst(Position, 2f, i * 60f, i * 60f + 100f, 0.8f);
            }
            
            yield return 2f;
            
            level?.Session.SetFlag("giga_axis_boss_defeated");
            RemoveSelf();
        }
        #endregion

        #region Cleanup
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            engineLoop?.Stop();
            warningAlarm?.Stop();
        }
        #endregion
    }
}
