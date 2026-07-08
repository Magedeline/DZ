using Celeste.Entities.Projectiles;
using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    [CustomEntity(ids: "DZ/SansBoss")]
    [Tracked]
    [HotReloadable]
    public class SansBoss : BossActor
    {
        private enum BossPhase
        {
            Lazy,
            Serious,
            LastBreath,
            Defeated
        }

        private enum AttackType
        {
            ShortcutStep,
            GasterLine,
            BoneVolley,
            BlueSoulCrush
        }

        private int health = 1100;
        private int maxHealth = 1100;
        private bool defeatSequenceStarted;
        private bool attackInProgress;
        private BossPhase currentPhase = BossPhase.Lazy;

        private Sprite sansSprite;
        private VertexLight eyeGlow;
        private BloomPoint bloom;
        private Wiggler blinkWiggler;
        private Vector2 arenaAnchor;
        private Vector2 lastKnownPlayerPos;
        private float idleTimer;

        public SansBoss(EntityData data, Vector2 offset)
            : base(data.Position + offset, "sans_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(26f, 36f, -13f, -36f))
        {
            maxHealth = data.Int("maxHealth", 1100);
            health = Math.Min(data.Int("health", maxHealth), maxHealth);
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            arenaAnchor = Position;
            SetupVisuals();
        }

        public SansBoss(Vector2 position)
            : base(position, "sans_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(26f, 36f, -13f, -36f))
        {
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            arenaAnchor = position;
            SetupVisuals();
        }

        private void SetupVisuals()
        {
            Add(sansSprite = new Sprite(GFX.Game, "characters/DZ/sans/"));
            sansSprite.AddLoop("idle", "idle", 0.1f);
            sansSprite.AddLoop("walk", "walk", 0.08f);
            sansSprite.AddLoop("run", "walk", 0.06f);
            sansSprite.AddLoop("laugh", "laugh", 0.08f);
            sansSprite.AddLoop("ha", "ha", 0.06f);
            sansSprite.Play("idle");
            sansSprite.CenterOrigin();

            Add(eyeGlow = new VertexLight(Color.White, 0.85f, 16, 32));
            eyeGlow.Position = new Vector2(3f, -24f);
            Add(bloom = new BloomPoint(0.55f, 12f));
            Add(blinkWiggler = Wiggler.Create(0.25f, 3f));
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

            Position = Vector2.Lerp(Position, arenaAnchor + new Vector2(0f, (float)Math.Sin(idleTimer * 2f) * 2f), 4f * Engine.DeltaTime);
        }

        private void UpdateVisuals()
        {
            if (sansSprite != null)
            {
                sansSprite.Scale.X = lastKnownPlayerPos.X < X ? -1f : 1f;
                if (!attackInProgress && !sansSprite.CurrentAnimationID.Equals("idle"))
                    sansSprite.Play("idle");
            }

            if (eyeGlow != null)
            {
                eyeGlow.Color = currentPhase switch
                {
                    BossPhase.Lazy => Color.White,
                    BossPhase.Serious => Calc.HexToColor("66d9ff"),
                    BossPhase.LastBreath => Calc.HexToColor("2bbdff"),
                    _ => Color.White
                };
                eyeGlow.Alpha = 0.45f + (float)Math.Sin(idleTimer * 10f) * 0.15f;
            }

            if (bloom != null)
                bloom.Alpha = 0.35f + blinkWiggler.Value * 0.2f;
        }

        private IEnumerator BossRoutine()
        {
            yield return 1f;

            while (!defeatSequenceStarted && health > 0)
            {
                attackInProgress = true;
                yield return ExecuteAttack(ChooseAttack());
                attackInProgress = false;
                yield return currentPhase == BossPhase.LastBreath ? 0.55f : 0.9f;
            }
        }

        private void UpdatePhase()
        {
            BossPhase nextPhase = health switch
            {
                <= 250 => BossPhase.LastBreath,
                <= 650 => BossPhase.Serious,
                _ => BossPhase.Lazy
            };

            if (nextPhase == currentPhase)
                return;

            currentPhase = nextPhase;
            blinkWiggler.Start();
            (Scene as Level)?.Flash(Calc.HexToColor("66d9ff") * 0.14f, false);
        }

        private AttackType ChooseAttack()
        {
            if (currentPhase == BossPhase.LastBreath && Calc.Random.Chance(0.4f))
                return AttackType.BlueSoulCrush;

            return (AttackType)Calc.Random.Next(0, 4);
        }

        private IEnumerator ExecuteAttack(AttackType attack)
        {
            yield return TelegraphIntent(attack switch
            {
                AttackType.ShortcutStep => BossTelegraphType.TeleportYellow,
                AttackType.GasterLine => BossTelegraphType.PositioningOrange,
                AttackType.BlueSoulCrush => BossTelegraphType.SpecialPurple,
                _ => BossTelegraphType.DangerRed
            }, attack == AttackType.BlueSoulCrush ? 0.6f : 0.5f);

            switch (attack)
            {
                case AttackType.ShortcutStep:
                    yield return ShortcutStepAttack();
                    break;
                case AttackType.GasterLine:
                    yield return GasterLineAttack();
                    break;
                case AttackType.BoneVolley:
                    yield return BoneVolleyAttack();
                    break;
                case AttackType.BlueSoulCrush:
                    yield return BlueSoulCrushAttack();
                    break;
            }

            yield return ReturnToAnchor(0.18f);
        }

        private IEnumerator ShortcutStepAttack()
        {
            global::Celeste.Player player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            Vector2 center = player?.Center ?? lastKnownPlayerPos;
            Vector2[] offsets =
            {
                new Vector2(-96f, -18f),
                new Vector2(96f, -18f),
                new Vector2(0f, -88f)
            };

            sansSprite?.Play("walk");
            for (int i = 0; i < offsets.Length; i++)
            {
                TeleportTo(center + offsets[i]);
                Vector2 velocity = ((player?.Center ?? center) - Center).SafeNormalize() * (currentPhase == BossPhase.LastBreath ? 230f : 180f);
                Scene?.Add(new BossStarProjectile(Center, velocity));
                yield return 0.08f;
            }

            sansSprite?.Play("laugh");
            yield return 0.12f;
        }

        private IEnumerator GasterLineAttack()
        {
            Level level = Scene as Level;
            if (level == null)
                yield break;

            global::Celeste.Player player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            sansSprite?.Play("ha");

            CreateBeam(level, new Vector2(arenaAnchor.X - 180f, player?.Y ?? Y), new Vector2(arenaAnchor.X + 180f, player?.Y ?? Y));
            yield return 0.1f;
            CreateBeam(level, new Vector2(player?.X ?? X, arenaAnchor.Y - 132f), new Vector2(player?.X ?? X, arenaAnchor.Y + 132f));
            yield return 0.12f;
        }

        private IEnumerator BoneVolleyAttack()
        {
            global::Celeste.Player player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            Vector2 target = player?.Center ?? lastKnownPlayerPos;

            for (int i = 0; i < 6; i++)
            {
                float offset = -120f + i * 48f;
                Vector2 start = arenaAnchor + new Vector2(offset, i % 2 == 0 ? -118f : 118f);
                Vector2 velocity = (target - start).SafeNormalize() * 160f;
                Scene?.Add(new BossStarProjectile(start, velocity));
            }

            Audio.Play("event:/final_content/game/19_TheEnd/sans_throw", Position);
            yield return 0.18f;
        }

        private IEnumerator BlueSoulCrushAttack()
        {
            Level level = Scene as Level;
            global::Celeste.Player player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                player.Speed.Y = currentPhase == BossPhase.LastBreath ? 320f : 240f;
            }

            sansSprite?.Play("ha");
            blinkWiggler.Start();
            level?.Flash(Calc.HexToColor("2bbdff") * 0.25f, false);
            CreateBeam(level, new Vector2((player?.X ?? X) - 72f, arenaAnchor.Y - 128f), new Vector2((player?.X ?? X) - 72f, arenaAnchor.Y + 128f));
            yield return 0.08f;
            CreateBeam(level, new Vector2((player?.X ?? X) + 72f, arenaAnchor.Y - 128f), new Vector2((player?.X ?? X) + 72f, arenaAnchor.Y + 128f));
            yield return 0.16f;
        }

        private void CreateBeam(Level level, Vector2 start, Vector2 end)
        {
            if (level == null)
                return;

            Audio.Play("event:/game/05_mirror_temple/button_activate", start);
            Vector2 direction = end - start;
            int segments = Math.Max(1, (int)(direction.Length() / 10f));
            Vector2 step = direction / segments;
            for (int i = 0; i <= segments; i++)
            {
                level.ParticlesFG.Emit(ParticleTypes.SparkyDust, 1, start + step * i, Vector2.One * 2f);
            }

            level.Displacement.AddBurst((start + end) * 0.5f, 0.7f, 18f, 88f, 0.2f);
            PushPlayerFromLine(start, end, 12f, 165f);
        }

        private void PushPlayerFromLine(Vector2 start, Vector2 end, float thickness, float force)
        {
            global::Celeste.Player player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
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

        private void TeleportTo(Vector2 target)
        {
            Level level = Scene as Level;
            level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, 8, Position, Vector2.One * 8f);
            level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, 8, target, Vector2.One * 8f);
            level?.Displacement.AddBurst(Position, 0.6f, 14f, 44f, 0.15f);
            level?.Displacement.AddBurst(target, 0.6f, 14f, 44f, 0.15f);
            Audio.Play("event:/final_content/game/19_TheEnd/sans_throw", Position);
            Position = target;
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
            blinkWiggler.Start();
            Audio.Play("event:/final_content/game/19_TheEnd/sans_throw", Position);

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
            sansSprite?.Play("laugh");
            Level level = Scene as Level;
            if (level != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    level.ParticlesFG.Emit(ParticleTypes.SparkyDust, 10, Center, Vector2.One * 10f);
                    yield return 0.12f;
                }

                level.Session.SetFlag("sans_boss_defeated");
            }

            RemoveSelf();
        }
    }
}

