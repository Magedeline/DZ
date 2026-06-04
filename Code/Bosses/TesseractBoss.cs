using Celeste.Entities.Projectiles;
using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    [CustomEntity(ids: "MaggyHelper/TesseractBoss")]
    [Tracked]
    [HotReloadable]
    public class TesseractBoss : BossActor
    {
        private const string TesseractTexturePath = "objects/tesseract_temple/dashButton";
        private const string TesseractMirrorTexturePath = "objects/tesseract_temple/dashButtonMirror";
        private const string PortalSurfaceTexturePath = "objects/temple/portal/surface";

        private enum BossPhase
        {
            Stable,
            Warped,
            Fractured,
            Defeated
        }

        private enum AttackType
        {
            WarpDash,
            PrismVolley,
            MirrorSlice,
            FrozenPrison
        }

        private int health = 1350;
        private int maxHealth = 1350;
        private bool defeatSequenceStarted;
        private bool attackInProgress;
        private BossPhase currentPhase = BossPhase.Stable;

        private readonly List<MTexture> normalFrames = new();
        private readonly List<MTexture> mirrorFrames = new();

        private Image shellImage;
        private Image auraImage;
        private VertexLight glow;
        private BloomPoint bloom;
        private Wiggler distortionWiggler;
        private Vector2 arenaAnchor;
        private float idleTimer;
        private float animationTimer;

        public TesseractBoss(EntityData data, Vector2 offset)
            : base(data.Position + offset, "tesseract_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(54f, 54f, -27f, -27f))
        {
            maxHealth = data.Int("maxHealth", 1350);
            health = Math.Min(data.Int("health", maxHealth), maxHealth);
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            arenaAnchor = Position;
            SetupVisuals();
        }

        public TesseractBoss(Vector2 position)
            : base(position, "tesseract_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(54f, 54f, -27f, -27f))
        {
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            arenaAnchor = position;
            SetupVisuals();
        }

        private void SetupVisuals()
        {
            normalFrames.AddRange(GetFrames(TesseractTexturePath));
            mirrorFrames.AddRange(GetFrames(TesseractMirrorTexturePath));

            if (normalFrames.Count > 0)
            {
                Add(shellImage = new Image(normalFrames[0]));
                shellImage.CenterOrigin();
                shellImage.Scale = new Vector2(1.6f, 1.6f);
            }

            if (TryGetTexture(PortalSurfaceTexturePath, out MTexture auraTexture))
            {
                Add(auraImage = new Image(auraTexture));
                auraImage.CenterOrigin();
                auraImage.Color = Color.Cyan * 0.45f;
                auraImage.Scale = new Vector2(0.75f, 0.75f);
            }

            Add(glow = new VertexLight(Color.Cyan, 0.9f, 28, 92));
            Add(bloom = new BloomPoint(0.8f, 20f));
            Add(distortionWiggler = Wiggler.Create(0.35f, 4f));
            Add(new PlayerCollider(OnPlayerTouch));
        }

        private static List<MTexture> GetFrames(string path)
        {
            if (GFX.Game == null || !GFX.Game.Has(path + "00"))
                return new List<MTexture>();

            return GFX.Game.GetAtlasSubtextures(path);
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
            animationTimer += Engine.DeltaTime;
            UpdatePhase();
            UpdateIdleMotion();
            UpdateVisuals();
        }

        private void UpdateIdleMotion()
        {
            if (attackInProgress)
                return;

            Vector2 idleTarget = arenaAnchor + new Vector2((float)Math.Cos(idleTimer * 1.4f) * 8f, (float)Math.Sin(idleTimer * 1.9f) * 10f);
            Position = Vector2.Lerp(Position, idleTarget, 3f * Engine.DeltaTime);
        }

        private void UpdateVisuals()
        {
            List<MTexture> frames = currentPhase == BossPhase.Stable ? normalFrames : mirrorFrames.Count > 0 ? mirrorFrames : normalFrames;
            if (shellImage != null && frames.Count > 0)
            {
                int frame = (int)(animationTimer * 18f) % frames.Count;
                shellImage.Texture = frames[frame];
                shellImage.Rotation += Engine.DeltaTime * (currentPhase == BossPhase.Fractured ? 2.2f : 1.1f);
                shellImage.Color = currentPhase switch
                {
                    BossPhase.Stable => Color.White,
                    BossPhase.Warped => Calc.HexToColor("a6f6ff"),
                    BossPhase.Fractured => Calc.HexToColor("6ff2ff"),
                    _ => Color.White
                };
            }

            if (auraImage != null)
            {
                float auraPulse = 0.72f + (float)Math.Sin(idleTimer * 4.6f) * 0.12f + distortionWiggler.Value * 0.2f;
                auraImage.Scale = new Vector2(auraPulse, auraPulse);
                auraImage.Rotation -= Engine.DeltaTime * 0.6f;
            }

            if (glow != null)
            {
                glow.Color = currentPhase == BossPhase.Fractured ? Calc.HexToColor("6ff2ff") : Color.Cyan;
                glow.Alpha = 0.55f + (float)Math.Sin(idleTimer * 7f) * 0.15f;
            }

            if (bloom != null)
                bloom.Alpha = 0.45f + distortionWiggler.Value * 0.2f;
        }

        private IEnumerator BossRoutine()
        {
            yield return 1f;

            while (!defeatSequenceStarted && health > 0)
            {
                attackInProgress = true;
                yield return ExecuteAttack(ChooseAttack());
                attackInProgress = false;
                yield return currentPhase == BossPhase.Fractured ? 0.65f : 1f;
            }
        }

        private void UpdatePhase()
        {
            BossPhase nextPhase = health switch
            {
                <= 380 => BossPhase.Fractured,
                <= 800 => BossPhase.Warped,
                _ => BossPhase.Stable
            };

            if (nextPhase == currentPhase)
                return;

            currentPhase = nextPhase;
            distortionWiggler.Start();
            var level = Scene as Level;
            level?.Flash(Color.Cyan * 0.16f, false);
            level?.Shake(currentPhase == BossPhase.Fractured ? 0.45f : 0.25f);
        }

        private AttackType ChooseAttack()
        {
            if (currentPhase == BossPhase.Fractured && Calc.Random.Chance(0.35f))
                return AttackType.FrozenPrison;

            return (AttackType)Calc.Random.Next(0, 4);
        }

        private IEnumerator ExecuteAttack(AttackType attack)
        {
            yield return TelegraphIntent(attack switch
            {
                AttackType.WarpDash => BossTelegraphType.TeleportYellow,
                AttackType.MirrorSlice => BossTelegraphType.PositioningOrange,
                AttackType.FrozenPrison => BossTelegraphType.SpecialPurple,
                _ => BossTelegraphType.DangerRed
            });

            switch (attack)
            {
                case AttackType.WarpDash:
                    yield return WarpDashAttack();
                    break;
                case AttackType.PrismVolley:
                    yield return PrismVolleyAttack();
                    break;
                case AttackType.MirrorSlice:
                    yield return MirrorSliceAttack();
                    break;
                case AttackType.FrozenPrison:
                    yield return FrozenPrisonAttack();
                    break;
            }

            yield return ReturnToAnchor(0.22f);
        }

        private IEnumerator WarpDashAttack()
        {
            Vector2[] points =
            {
                arenaAnchor + new Vector2(-132f, -88f),
                arenaAnchor + new Vector2(132f, -88f),
                arenaAnchor + new Vector2(132f, 88f),
                arenaAnchor + new Vector2(-132f, 88f)
            };

            int hops = currentPhase == BossPhase.Fractured ? 4 : 3;
            for (int i = 0; i < hops; i++)
            {
                Vector2 start = Position;
                Vector2 target = points[Calc.Random.Next(points.Length)];
                TeleportBurst(start, target);
                yield return 0.08f;
            }
        }

        private IEnumerator PrismVolleyAttack()
        {
            var level = Scene as Level;
            int volleys = currentPhase == BossPhase.Fractured ? 4 : 3;

            for (int i = 0; i < volleys; i++)
            {
                Scene?.Add(new IceProjectile(Center + new Vector2(-10f, Calc.Random.Range(-8f, 8f)), -1));
                Scene?.Add(new IceProjectile(Center + new Vector2(10f, Calc.Random.Range(-8f, 8f)), 1));
                level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, 6, Center, Vector2.One * 12f);
                Audio.Play("guid://{097d5975-dee1-42e2-a3f0-1524b9d60fc6}", Position);
                yield return 0.16f;
            }
        }

        private IEnumerator MirrorSliceAttack()
        {
            var level = Scene as Level;
            if (level == null)
                yield break;

            global::Celeste.Player player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            float targetY = player?.Y ?? Y;
            float targetX = player?.X ?? X;

            CreateBeam(level, new Vector2(arenaAnchor.X - 180f, targetY), new Vector2(arenaAnchor.X + 180f, targetY));
            yield return 0.12f;
            CreateBeam(level, new Vector2(targetX, arenaAnchor.Y - 140f), new Vector2(targetX, arenaAnchor.Y + 140f));
            yield return 0.12f;
        }

        private IEnumerator FrozenPrisonAttack()
        {
            var level = Scene as Level;
            if (level == null)
                yield break;

            Vector2[] offsets =
            {
                new Vector2(-96f, 0f),
                new Vector2(96f, 0f),
                new Vector2(0f, -96f),
                new Vector2(0f, 96f)
            };

            foreach (Vector2 offset in offsets)
            {
                Vector2 target = arenaAnchor + offset;
                TeleportBurst(Position, target);
                CreateBeam(level, target, arenaAnchor);
                Scene?.Add(new IceProjectile(target + new Vector2(-8f, 0f), -1));
                Scene?.Add(new IceProjectile(target + new Vector2(8f, 0f), 1));
                yield return 0.1f;
            }
        }

        private void CreateBeam(Level level, Vector2 start, Vector2 end)
        {
            Audio.Play("guid://{097d5975-dee1-42e2-a3f0-1524b9d60fc6}", start);
            level.Displacement.AddBurst((start + end) * 0.5f, 0.8f, 24f, 88f, 0.25f);

            Vector2 direction = end - start;
            int segments = Math.Max(1, (int)(direction.Length() / 12f));
            Vector2 step = direction / segments;
            for (int i = 0; i <= segments; i++)
            {
                Vector2 point = start + step * i;
                level.ParticlesFG.Emit(ParticleTypes.SparkyDust, 1, point, Vector2.One * 2f);
            }

            PushPlayerFromLine(start, end, 14f, currentPhase == BossPhase.Fractured ? 210f : 150f);
        }

        private void TeleportBurst(Vector2 from, Vector2 to)
        {
            var level = Scene as Level;
            distortionWiggler.Start();
            level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, 10, from, Vector2.One * 10f);
            level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, 10, to, Vector2.One * 10f);
            level?.Displacement.AddBurst(from, 0.8f, 18f, 56f, 0.2f);
            level?.Displacement.AddBurst(to, 0.8f, 18f, 56f, 0.2f);
            Audio.Play("guid://{097d5975-dee1-42e2-a3f0-1524b9d60fc6}", from);
            Position = to;
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

        private void PushPlayerFromLine(Vector2 start, Vector2 end, float thickness, float force)
        {
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (player == null)
                return;

            float distance = PointToSegmentDistance(player.Center, start, end);
            if (distance > thickness)
                return;

            Vector2 lineDirection = (end - start).SafeNormalize();
            Vector2 pushDirection = new Vector2(-lineDirection.Y, lineDirection.X);
            player.Speed += pushDirection * force;
        }

        private static float PointToSegmentDistance(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 segment = end - start;
            float segmentLengthSquared = segment.LengthSquared();
            if (segmentLengthSquared <= 0f)
                return Vector2.Distance(point, start);

            float projection = Vector2.Dot(point - start, segment) / segmentLengthSquared;
            projection = MathHelper.Clamp(projection, 0f, 1f);
            Vector2 closest = start + segment * projection;
            return Vector2.Distance(point, closest);
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
            distortionWiggler.Start();
            Audio.Play("guid://{097d5975-dee1-42e2-a3f0-1524b9d60fc6}", Position);

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
                for (int i = 0; i < 6; i++)
                {
                    Vector2 offset = Calc.AngleToVector(i * MathHelper.TwoPi / 6f, 32f);
                    TeleportBurst(Position, arenaAnchor + offset);
                    yield return 0.08f;
                }

                level.Session.SetFlag("tesseract_boss_defeated");
            }

            RemoveSelf();
        }
    }
}

