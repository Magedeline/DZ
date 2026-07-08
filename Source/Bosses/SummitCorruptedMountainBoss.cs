using Celeste.Effects;
using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    [CustomEntity(ids: "DZ/SummitCorruptedMountainBoss")]
    [Tracked]
    [HotReloadable]
    public class SummitCorruptedMountainBoss : BossActor
    {
        private const string CrestTexturePath = "decals/DZ/9_beyond_summit/SummitFlag";
        private const string FallbackCrestTexturePath = "decals/7-summit/SummitFlag";
        private const string CoreTexturePath = "collectables/summitgems/0/gem";
        private const string FallbackCoreTexturePath = "collectables/heartGem/0/00";

        private enum BossPhase
        {
            Ascent,
            Collapse,
            Cataclysm,
            Defeated
        }

        private enum AttackType
        {
            AvalancheRush,
            CorruptionEruption,
            CrystalSpireRing,
            SummitCrash
        }

        private int health = 1500;
        private int maxHealth = 1500;
        private bool defeatSequenceStarted;
        private bool attackInProgress;
        private BossPhase currentPhase = BossPhase.Ascent;

        private Image crestImage;
        private Image coreImage;
        private VertexLight glow;
        private BloomPoint bloom;
        private Wiggler impactWiggler;
        private Vector2 arenaAnchor;
        private float idleTimer;

        public SummitCorruptedMountainBoss(EntityData data, Vector2 offset)
            : base(data.Position + offset, "summit_corrupted_mountain_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(88f, 88f, -44f, -44f))
        {
            maxHealth = data.Int("maxHealth", 1500);
            health = Math.Min(data.Int("health", maxHealth), maxHealth);
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            arenaAnchor = Position;
            SetupVisuals();
        }

        public SummitCorruptedMountainBoss(Vector2 position)
            : base(position, "summit_corrupted_mountain_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(88f, 88f, -44f, -44f))
        {
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            arenaAnchor = position;
            SetupVisuals();
        }

        private void SetupVisuals()
        {
            if (TryGetTexture(CrestTexturePath, out MTexture crestTexture) || TryGetTexture(FallbackCrestTexturePath, out crestTexture))
            {
                Add(crestImage = new Image(crestTexture));
                crestImage.CenterOrigin();
                crestImage.Scale = new Vector2(2.1f, 2.1f);
                crestImage.Color = Calc.HexToColor("6f394d");
            }

            if (TryGetTexture(CoreTexturePath, out MTexture coreTexture) || TryGetTexture(FallbackCoreTexturePath, out coreTexture))
            {
                Add(coreImage = new Image(coreTexture));
                coreImage.CenterOrigin();
                coreImage.Scale = new Vector2(1.35f, 1.35f);
                coreImage.Color = Calc.HexToColor("ff6f91");
            }

            Add(glow = new VertexLight(Calc.HexToColor("ff6f91"), 0.9f, 36, 110));
            Add(bloom = new BloomPoint(0.75f, 18f));
            Add(impactWiggler = Wiggler.Create(0.4f, 4f));
            Add(new PlayerCollider(OnPlayerTouch));
        }

        private static bool TryGetTexture(string path, out MTexture texture)
        {
            texture = null;

            if (GFX.Game == null)
                return false;

            if (GFX.Game.Has(path))
            {
                texture = GFX.Game[path];
                return true;
            }

            if (GFX.Game.Has(path + "00"))
            {
                texture = GFX.Game[path + "00"];
                return true;
            }

            return false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            arenaAnchor = Position;
            StartBossRoutine(BossRoutine());
        }

        public override void Update()
        {
            base.Update();

            if (!EncounterStarted)
                return;

            if (defeatSequenceStarted || currentPhase == BossPhase.Defeated)
                return;

            idleTimer += Engine.DeltaTime;
            UpdatePhase();
            UpdateIdleMotion();
            UpdateVisuals();
        }

        private void UpdateIdleMotion()
        {
            if (attackInProgress)
                return;

            Vector2 idleTarget = arenaAnchor + new Vector2((float)Math.Cos(idleTimer * 0.8f) * 10f, (float)Math.Sin(idleTimer * 1.4f) * 7f);
            Position = Vector2.Lerp(Position, idleTarget, 2.3f * Engine.DeltaTime);
        }

        private void UpdateVisuals()
        {
            float pulse = 1f + (float)Math.Sin(idleTimer * 3.8f) * 0.08f + impactWiggler.Value * 0.12f;
            Color crestColor = currentPhase switch
            {
                BossPhase.Ascent => Calc.HexToColor("6f394d"),
                BossPhase.Collapse => Calc.HexToColor("8f314a"),
                BossPhase.Cataclysm => Calc.HexToColor("b6193c"),
                _ => Color.White
            };

            Color coreColor = currentPhase switch
            {
                BossPhase.Ascent => Calc.HexToColor("ff6f91"),
                BossPhase.Collapse => Calc.HexToColor("ff4976"),
                BossPhase.Cataclysm => Calc.HexToColor("ff2455"),
                _ => Color.White
            };

            if (crestImage != null)
            {
                crestImage.Rotation = (float)Math.Sin(idleTimer * 0.8f) * 0.08f;
                crestImage.Color = crestColor;
            }

            if (coreImage != null)
            {
                coreImage.Scale = new Vector2(1.35f, 1.35f) * pulse;
                coreImage.Color = coreColor;
            }

            if (glow != null)
            {
                glow.Color = coreColor;
                glow.Alpha = 0.55f + (float)Math.Sin(idleTimer * 5f) * 0.18f;
            }

            if (bloom != null)
                bloom.Alpha = 0.55f + impactWiggler.Value * 0.25f;
        }

        private IEnumerator BossRoutine()
        {
            yield return 1f;

            while (!defeatSequenceStarted && health > 0)
            {
                attackInProgress = true;
                yield return ExecuteAttack(ChooseAttack());
                attackInProgress = false;
                yield return currentPhase == BossPhase.Cataclysm ? 0.75f : 1.15f;
            }
        }

        private void UpdatePhase()
        {
            BossPhase nextPhase = health switch
            {
                <= 420 => BossPhase.Cataclysm,
                <= 900 => BossPhase.Collapse,
                _ => BossPhase.Ascent
            };

            if (nextPhase == currentPhase)
                return;

            currentPhase = nextPhase;
            impactWiggler.Start();

            var level = Scene as Level;
            level?.Flash(Calc.HexToColor("ff4976") * 0.18f, false);
            level?.Shake(currentPhase == BossPhase.Cataclysm ? 0.65f : 0.4f);
        }

        private AttackType ChooseAttack()
        {
            if (currentPhase == BossPhase.Cataclysm && Calc.Random.Chance(0.35f))
                return AttackType.SummitCrash;

            return (AttackType)Calc.Random.Next(0, 4);
        }

        private IEnumerator ExecuteAttack(AttackType attack)
        {
            yield return TelegraphIntent(attack switch
            {
                AttackType.AvalancheRush => BossTelegraphType.DashCyan,
                AttackType.CrystalSpireRing => BossTelegraphType.PositioningOrange,
                AttackType.SummitCrash => BossTelegraphType.SpecialPurple,
                _ => BossTelegraphType.DangerRed
            });

            switch (attack)
            {
                case AttackType.AvalancheRush:
                    yield return AvalancheRushAttack();
                    break;
                case AttackType.CorruptionEruption:
                    yield return CorruptionEruptionAttack();
                    break;
                case AttackType.CrystalSpireRing:
                    yield return CrystalSpireRingAttack();
                    break;
                case AttackType.SummitCrash:
                    yield return SummitCrashAttack();
                    break;
            }

            yield return ReturnToAnchor(0.28f);
        }

        private IEnumerator AvalancheRushAttack()
        {
            var level = Scene as Level;
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();

            yield return Windup(new Vector2(0f, -26f), 0.22f);

            if (level != null)
            {
                AdditionalElementalEffects.CreateEarthquake(level, Position, currentPhase == BossPhase.Cataclysm ? 2.2f : 1.6f, 1f);
                AdditionalElementalEffects.CreateBoulderThrow(level, Position, player?.Center ?? (arenaAnchor + Vector2.UnitX * 96f));
            }

            Vector2 target = arenaAnchor + new Vector2(player != null && player.X < X ? -148f : 148f, Calc.Random.Range(-36f, 36f));
            yield return Slam(target, 0.18f);
        }

        private IEnumerator CorruptionEruptionAttack()
        {
            var level = Scene as Level;
            if (level == null)
                yield break;

            Audio.Play(AdditionalElementalEffects.SFX_DARK_CORRUPTION, Position);

            int bursts = currentPhase == BossPhase.Cataclysm ? 5 : 3;
            for (int i = 0; i < bursts; i++)
            {
                float angle = MathHelper.TwoPi * (i / (float)bursts) + idleTimer;
                Vector2 offset = Calc.AngleToVector(angle, 72f + i * 10f);
                Vector2 burstPosition = arenaAnchor + offset;
                AdditionalElementalEffects.CreateCorruption(level, burstPosition, 38f + i * 8f);
                PushNearbyPlayer((burstPosition - arenaAnchor).SafeNormalize(), 118f, 150f);
                yield return 0.12f;
            }
        }

        private IEnumerator CrystalSpireRingAttack()
        {
            var level = Scene as Level;
            if (level == null)
                yield break;

            yield return Windup(new Vector2(0f, -18f), 0.18f);
            AdditionalElementalEffects.CreateCrystalFormation(level, Position, currentPhase == BossPhase.Cataclysm ? 12 : 8);
            level.Shake(0.35f);
            PushNearbyPlayer(Vector2.UnitY * -1f, 100f, 140f);
            yield return 0.2f;
            AdditionalElementalEffects.CreateEarthquake(level, Position, 1.2f, 0.6f);
        }

        private IEnumerator SummitCrashAttack()
        {
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            Vector2 targetAbove = (player?.Center ?? arenaAnchor) + new Vector2(0f, -132f);

            yield return MoveTo(targetAbove, 0.22f);
            yield return 0.08f;
            yield return Slam((player?.Center ?? arenaAnchor) + new Vector2(0f, 84f), 0.16f);
        }

        private IEnumerator Windup(Vector2 offset, float duration)
        {
            Audio.Play("event:/game/07_summit/checkpoint_summit", Position);
            Vector2 start = Position;
            Vector2 target = Position + offset;

            for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime / duration)
            {
                Position = Vector2.Lerp(start, target, Ease.CubeOut(progress));
                yield return null;
            }

            Position = target;
        }

        private IEnumerator MoveTo(Vector2 target, float duration)
        {
            Vector2 start = Position;
            for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime / duration)
            {
                Position = Vector2.Lerp(start, target, Ease.SineInOut(progress));
                yield return null;
            }

            Position = target;
        }

        private IEnumerator Slam(Vector2 target, float duration)
        {
            Vector2 start = Position;
            for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime / duration)
            {
                Position = Vector2.Lerp(start, target, Ease.CubeIn(progress));
                yield return null;
            }

            Position = target;
            OnImpact();
        }

        private IEnumerator ReturnToAnchor(float duration)
        {
            yield return MoveTo(arenaAnchor, duration);
        }

        private void OnImpact()
        {
            var level = Scene as Level;
            impactWiggler.Start();
            Audio.Play("event:/game/06_reflection/fallblock_boss_impact", Position);

            if (level != null)
            {
                AdditionalElementalEffects.CreateEarthquake(level, Position, currentPhase == BossPhase.Cataclysm ? 1.8f : 1.15f, 0.8f);
                AdditionalElementalEffects.CreateCorruption(level, Position, currentPhase == BossPhase.Cataclysm ? 72f : 56f);
                level.Displacement.AddBurst(Position, 1f, 46f, 140f, 0.45f);
            }

            PushNearbyPlayer((Scene?.Tracker.GetEntity<global::Celeste.Player>()?.Center - Center ?? Vector2.UnitY).SafeNormalize(), 118f, currentPhase == BossPhase.Cataclysm ? 240f : 175f);
        }

        private void PushNearbyPlayer(Vector2 direction, float radius, float speed)
        {
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (player == null)
                return;

            if (Vector2.Distance(player.Center, Center) <= radius)
                player.Speed += direction.SafeNormalize() * speed;
        }

        private void OnPlayerTouch(global::Celeste.Player player)
        {
            if (player == null || defeatSequenceStarted)
                return;

            player.Die((player.Center - Center).SafeNormalize());
        }

        public override void TakeDamage(int damage)
        {
            if (defeatSequenceStarted)
                return;

            health = Math.Max(0, health - damage);
            Health = health;
            impactWiggler.Start();
            Audio.Play("event:/game/06_reflection/fallblock_boss_impact", Position);

            if (health <= 0)
            {
                defeatSequenceStarted = true;
                IsDefeated = true;
                Add(new Coroutine(DefeatSequence()));
            }
        }

        private IEnumerator DefeatSequence()
        {
            currentPhase = BossPhase.Defeated;

            var level = Scene as Level;
            if (level != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    AdditionalElementalEffects.CreateCorruption(level, Position + Calc.AngleToVector(i * MathHelper.TwoPi / 5f, 18f), 42f);
                    AdditionalElementalEffects.CreateEarthquake(level, Position, 1.1f + i * 0.15f, 0.4f);
                    yield return 0.16f;
                }

                level.Session.SetFlag("summit_corrupted_mountain_boss_defeated");
            }

            RemoveSelf();
        }
    }
}

