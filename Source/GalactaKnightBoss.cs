using MaggyHelper.Entities.Projectiles;
using MaggyHelper.Helpers;

namespace MaggyHelper.Entities.Bosses
{
    [CustomEntity(ids: "MaggyHelper/GalactaKnightBoss")]
    [Tracked]
    [HotReloadable]
    public class GalactaKnightBoss : BossActor
    {
        private const string SpriteBankEntry = "galacta_knight_clone";
        private const string FallbackTexturePath = "particles/blob";

        private enum BossPhase
        {
            Awakened,
            Radiant,
            Aeon,
            Defeated
        }

        private enum AttackType
        {
            WarpLance,
            CrescentSlash,
            StarPiercer,
            JudgmentRain
        }

        private int health = 1700;
        private int maxHealth = 1700;
        private bool defeatSequenceStarted;
        private bool attackInProgress;
        private BossPhase currentPhase = BossPhase.Awakened;

        private Sprite knightSprite;
        private Image fallbackImage;
        private VertexLight bodyGlow;
        private BloomPoint bloom;
        private Wiggler slashWiggler;
        private Vector2 arenaAnchor;
        private Vector2 lastKnownPlayerPos;
        private float idleTimer;

        public GalactaKnightBoss(EntityData data, Vector2 offset)
            : base(data.Position + offset, "galacta_knight_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(34f, 48f, -17f, -48f))
        {
            maxHealth = data.Int("maxHealth", 1700);
            health = Math.Min(data.Int("health", maxHealth), maxHealth);
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            arenaAnchor = Position;
            SetupVisuals();
        }

        public GalactaKnightBoss(Vector2 position)
            : base(position, "galacta_knight_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(34f, 48f, -17f, -48f))
        {
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            arenaAnchor = position;
            SetupVisuals();
        }

        private void SetupVisuals()
        {
            bool createdSprite = false;

            try
            {
                if (GFX.SpriteBank != null && GFX.SpriteBank.Has(SpriteBankEntry))
                {
                    Add(knightSprite = GFX.SpriteBank.Create(SpriteBankEntry));
                    knightSprite.Play("idle");
                    knightSprite.CenterOrigin();
                    createdSprite = true;
                }
            }
            catch
            {
                createdSprite = false;
            }

            if (!createdSprite && GFX.Game != null && GFX.Game.Has(FallbackTexturePath))
            {
                Add(fallbackImage = new Image(GFX.Game[FallbackTexturePath]));
                fallbackImage.CenterOrigin();
                fallbackImage.Scale = new Vector2(2.2f, 2.8f);
                fallbackImage.Color = Calc.HexToColor("ff8ab8");
            }

            Add(bodyGlow = new VertexLight(Calc.HexToColor("ff8ab8"), 0.95f, 28, 88));
            bodyGlow.Position = new Vector2(0f, -26f);
            Add(bloom = new BloomPoint(0.8f, 20f));
            Add(slashWiggler = Wiggler.Create(0.3f, 4f));
            Add(new PlayerCollider(OnPlayerTouch));
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

            global::Celeste.Player player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
                lastKnownPlayerPos = player.Center;

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

            Vector2 idleTarget = arenaAnchor + new Vector2((float)Math.Cos(idleTimer * 1.3f) * 9f, (float)Math.Sin(idleTimer * 1.6f) * 8f);
            Position = Vector2.Lerp(Position, idleTarget, 3f * Engine.DeltaTime);
        }

        private void UpdateVisuals()
        {
            if (knightSprite != null)
            {
                knightSprite.Scale.X = lastKnownPlayerPos.X < X ? -1f : 1f;
                if (!attackInProgress && !knightSprite.CurrentAnimationID.Equals("idle"))
                    knightSprite.Play("idle");
            }

            if (fallbackImage != null)
            {
                float pulse = 1f + (float)Math.Sin(idleTimer * 5f) * 0.08f + slashWiggler.Value * 0.16f;
                fallbackImage.Scale = new Vector2(2.2f, 2.8f) * pulse;
                fallbackImage.Rotation = (float)Math.Sin(idleTimer * 1.2f) * 0.08f;
                fallbackImage.Color = currentPhase switch
                {
                    BossPhase.Awakened => Calc.HexToColor("ff8ab8"),
                    BossPhase.Radiant => Calc.HexToColor("ff6fa6"),
                    BossPhase.Aeon => Calc.HexToColor("ff4c92"),
                    _ => Color.White
                };
            }

            if (bodyGlow != null)
            {
                bodyGlow.Color = currentPhase == BossPhase.Aeon ? Calc.HexToColor("ff4c92") : Calc.HexToColor("ff8ab8");
                bodyGlow.Alpha = 0.55f + (float)Math.Sin(idleTimer * 8f) * 0.16f;
            }

            if (bloom != null)
                bloom.Alpha = 0.45f + slashWiggler.Value * 0.22f;
        }

        private IEnumerator BossRoutine()
        {
            yield return 1f;

            while (!defeatSequenceStarted && health > 0)
            {
                attackInProgress = true;
                yield return ExecuteAttack(ChooseAttack());
                attackInProgress = false;
                yield return currentPhase == BossPhase.Aeon ? 0.6f : 0.9f;
            }
        }

        private void UpdatePhase()
        {
            BossPhase nextPhase = health switch
            {
                <= 420 => BossPhase.Aeon,
                <= 980 => BossPhase.Radiant,
                _ => BossPhase.Awakened
            };

            if (nextPhase == currentPhase)
                return;

            currentPhase = nextPhase;
            slashWiggler.Start();
            (Scene as Level)?.Flash(Calc.HexToColor("ff8ab8") * 0.18f, false);
        }

        private AttackType ChooseAttack()
        {
            if (currentPhase == BossPhase.Aeon && Calc.Random.Chance(0.35f))
                return AttackType.JudgmentRain;

            return (AttackType)Calc.Random.Next(0, 4);
        }

        private IEnumerator ExecuteAttack(AttackType attack)
        {
            yield return TelegraphIntent(attack switch
            {
                AttackType.WarpLance => BossTelegraphType.TeleportYellow,
                AttackType.StarPiercer => BossTelegraphType.DashCyan,
                AttackType.JudgmentRain => BossTelegraphType.SpecialPurple,
                _ => BossTelegraphType.DangerRed
            });

            switch (attack)
            {
                case AttackType.WarpLance:
                    yield return WarpLanceAttack();
                    break;
                case AttackType.CrescentSlash:
                    yield return CrescentSlashAttack();
                    break;
                case AttackType.StarPiercer:
                    yield return StarPiercerAttack();
                    break;
                case AttackType.JudgmentRain:
                    yield return JudgmentRainAttack();
                    break;
            }

            yield return ReturnToAnchor(0.22f);
        }

        private IEnumerator WarpLanceAttack()
        {
            global::Celeste.Player player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            Vector2[] offsets =
            {
                new Vector2(-108f, -72f),
                new Vector2(108f, -72f),
                new Vector2(0f, -120f)
            };

            for (int i = 0; i < offsets.Length; i++)
            {
                PlayIfAvailable("warp");
                TeleportTo((player?.Center ?? lastKnownPlayerPos) + offsets[i]);
                yield return 0.06f;
                PlayIfAvailable("slash");
                yield return DashTo(player?.Center ?? arenaAnchor, 0.12f);
                yield return 0.05f;
            }
        }

        private IEnumerator CrescentSlashAttack()
        {
            PlayIfAvailable("charge");
            yield return 0.08f;
            PlayIfAvailable("slash");
            Audio.Play("event:/metaknight_sword_beam", Position);

            float speed = currentPhase == BossPhase.Aeon ? 260f : 210f;
            Scene?.Add(new SwordBeam(Center + new Vector2(-8f, -14f), new Vector2(-speed, 0f)));
            Scene?.Add(new SwordBeam(Center + new Vector2(8f, -14f), new Vector2(speed, 0f)));
            Scene?.Add(new SwordBeam(Center + new Vector2(0f, -8f), new Vector2(0f, -speed)));
            slashWiggler.Start();
            yield return 0.18f;
        }

        private IEnumerator StarPiercerAttack()
        {
            PlayIfAvailable("move");
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 velocity = Calc.AngleToVector(angle, currentPhase == BossPhase.Aeon ? 185f : 145f);
                Scene?.Add(new BossStarProjectile(Center, velocity));
            }

            Audio.Play("event:/game/general/diamond_touch", Position);
            yield return 0.16f;
        }

        private IEnumerator JudgmentRainAttack()
        {
            global::Celeste.Player player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            float targetX = player?.X ?? X;

            for (int i = -2; i <= 2; i++)
            {
                Vector2 start = new Vector2(targetX + i * 42f, arenaAnchor.Y - 170f);
                Scene?.Add(new SwordBeam(start, new Vector2(0f, 240f)));
            }

            PlayIfAvailable("charge");
            yield return 0.1f;
        }

        private void PlayIfAvailable(string animation)
        {
            if (knightSprite != null && knightSprite.Has(animation))
                knightSprite.Play(animation);
        }

        private void TeleportTo(Vector2 target)
        {
            Level level = Scene as Level;
            level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, 10, Position, Vector2.One * 8f);
            level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, 10, target, Vector2.One * 8f);
            level?.Displacement.AddBurst(Position, 0.7f, 14f, 48f, 0.15f);
            level?.Displacement.AddBurst(target, 0.7f, 14f, 48f, 0.15f);
            Audio.Play("event:/metaknight_cape_vanish", Position);
            Position = target;
        }

        private IEnumerator DashTo(Vector2 target, float duration)
        {
            Vector2 start = Position;
            for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime / duration)
            {
                Position = Vector2.Lerp(start, target, Ease.CubeIn(progress));
                yield return null;
            }

            Position = target;
            slashWiggler.Start();
            (Scene as Level)?.Displacement.AddBurst(Position, 0.8f, 22f, 76f, 0.22f);
        }

        private IEnumerator ReturnToAnchor(float duration)
        {
            Vector2 start = Position;
            for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime / duration)
            {
                Position = Vector2.Lerp(start, arenaAnchor, Ease.SineInOut(progress));
                yield return null;
            }

            Position = arenaAnchor;
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
            slashWiggler.Start();
            Audio.Play("event:/metaknight_damage", Position);

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
            Level level = Scene as Level;
            if (level != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    level.ParticlesFG.Emit(ParticleTypes.SparkyDust, 8, Center, Vector2.One * 10f);
                    yield return 0.12f;
                }

                level.Session.SetFlag("galacta_knight_boss_defeated");
            }

            RemoveSelf();
        }
    }
}