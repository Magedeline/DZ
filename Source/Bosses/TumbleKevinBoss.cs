using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// Tumble Kevin Boss - standalone tumbling crush-block fight for future DX chapter use.
    /// Falls back to vanilla crush block textures until a dedicated boss atlas exists.
    /// </summary>
    [CustomEntity(ids: "DZ/TumbleKevinBoss")]
    [Tracked]
    [HotReloadable]
    public class TumbleKevinBoss : BossActor
    {
        private const string BodyTexturePath = "objects/DZ/DZ/crushblock/block";
        private const string FaceTexturePath = "objects/DZ/DZ/crushblock/idle_face";
        private const string AngryFaceTexturePath = "objects/DZ/DZ/crushblock/lit_face";
        private const string FallbackTexturePath = "characters/DZ/kirby/stone/crush";

        private enum BossPhase
        {
            Calm,
            Frenzy,
            Cataclysm,
            Defeated
        }

        private enum AttackType
        {
            HorizontalSlam,
            VerticalSlam,
            CornerPinball,
            TremorBurst
        }

        private int health = 1200;
        private int maxHealth = 1200;
        private bool defeatSequenceStarted;
        private bool attackInProgress;
        private BossPhase currentPhase = BossPhase.Calm;

        private Image bodyImage;
        private Image faceImage;
        private VertexLight impactGlow;
        private Wiggler impactWiggler;
        private Vector2 arenaAnchor;
        private float idleTimer;

        public TumbleKevinBoss(EntityData data, Vector2 offset)
            : base(data.Position + offset, "tumble_kevin_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(64f, 64f, -32f, -32f))
        {
            maxHealth = data.Int("maxHealth", 1200);
            health = Math.Min(data.Int("health", maxHealth), maxHealth);
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            arenaAnchor = Position;
            SetupVisuals();
        }

        public TumbleKevinBoss(Vector2 position)
            : base(position, "tumble_kevin_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(64f, 64f, -32f, -32f))
        {
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            arenaAnchor = position;
            SetupVisuals();
        }

        private void SetupVisuals()
        {
            if (TryGetTexture(BodyTexturePath, out MTexture bodyTexture) || TryGetTexture(FallbackTexturePath, out bodyTexture))
            {
                Add(bodyImage = new Image(bodyTexture));
                bodyImage.CenterOrigin();
            }

            if (TryGetTexture(FaceTexturePath, out MTexture faceTexture))
            {
                Add(faceImage = new Image(faceTexture));
                faceImage.CenterOrigin();
                faceImage.Position = new Vector2(0f, -2f);
            }

            Add(impactGlow = new VertexLight(Color.OrangeRed, 0.7f, 28, 84));
            impactGlow.Position = new Vector2(0f, 0f);

            Add(impactWiggler = Wiggler.Create(0.35f, 4f));
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

            Vector2 idleTarget = arenaAnchor + new Vector2(0f, (float)Math.Sin(idleTimer * 1.8f) * 4f);
            Position = Vector2.Lerp(Position, idleTarget, 2.6f * Engine.DeltaTime);
        }

        private void UpdateVisuals()
        {
            float squish = 1f + impactWiggler.Value * 0.08f;
            Color tint = currentPhase switch
            {
                BossPhase.Calm => Color.White,
                BossPhase.Frenzy => new Color(255, 220, 220),
                BossPhase.Cataclysm => new Color(255, 180, 180),
                _ => Color.White
            };

            if (bodyImage != null)
            {
                bodyImage.Scale = new Vector2(squish, 1f / squish);
                bodyImage.Color = tint;
            }

            if (faceImage != null)
            {
                faceImage.Scale = new Vector2(squish, 1f / squish);
                faceImage.Color = tint;
            }

            if (impactGlow != null)
            {
                impactGlow.Color = currentPhase == BossPhase.Cataclysm ? Color.Red : Color.OrangeRed;
                impactGlow.Alpha = 0.45f + (float)Math.Sin(idleTimer * 6f) * 0.18f;
            }
        }

        private IEnumerator BossRoutine()
        {
            yield return 1f;

            while (!defeatSequenceStarted && health > 0)
            {
                attackInProgress = true;
                yield return ExecuteAttack(ChooseAttack());
                attackInProgress = false;
                yield return currentPhase == BossPhase.Cataclysm ? 0.7f : 1.05f;
            }
        }

        private void UpdatePhase()
        {
            BossPhase nextPhase = health switch
            {
                <= 360 => BossPhase.Cataclysm,
                <= 720 => BossPhase.Frenzy,
                _ => BossPhase.Calm
            };

            if (nextPhase == currentPhase)
                return;

            currentPhase = nextPhase;
            impactWiggler.Start();
            SetFace(isAngry: currentPhase != BossPhase.Calm);

            var level = Scene as Level;
            level?.Flash(currentPhase == BossPhase.Cataclysm ? Color.Red * 0.3f : Color.OrangeRed * 0.18f, false);
            level?.Shake(currentPhase == BossPhase.Cataclysm ? 0.55f : 0.3f);
        }

        private void SetFace(bool isAngry)
        {
            if (faceImage == null)
                return;

            string path = isAngry ? AngryFaceTexturePath : FaceTexturePath;
            if (TryGetTexture(path, out MTexture texture))
                faceImage.Texture = texture;
        }

        private AttackType ChooseAttack()
        {
            if (currentPhase == BossPhase.Cataclysm && Calc.Random.Chance(0.35f))
                return AttackType.CornerPinball;

            return (AttackType)Calc.Random.Next(0, 3);
        }

        private IEnumerator ExecuteAttack(AttackType attack)
        {
            yield return TelegraphIntent(attack switch
            {
                AttackType.TremorBurst => BossTelegraphType.SpecialPurple,
                _ => BossTelegraphType.DashCyan
            });

            switch (attack)
            {
                case AttackType.HorizontalSlam:
                    yield return HorizontalSlamAttack();
                    break;
                case AttackType.VerticalSlam:
                    yield return VerticalSlamAttack();
                    break;
                case AttackType.CornerPinball:
                    yield return CornerPinballAttack();
                    break;
                case AttackType.TremorBurst:
                    yield return TremorBurstAttack();
                    break;
            }

            Position = Vector2.Lerp(Position, arenaAnchor, 0.65f);
        }

        private IEnumerator HorizontalSlamAttack()
        {
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            Vector2 direction = player != null && player.X < X ? Vector2.UnitX * -1f : Vector2.UnitX;
            yield return Windup(direction * -20f, 0.18f);
            yield return Slam(arenaAnchor + direction * (currentPhase == BossPhase.Cataclysm ? 168f : 136f), direction, 0.16f);
            yield return ReturnToAnchor(0.24f);
        }

        private IEnumerator VerticalSlamAttack()
        {
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            Vector2 direction = player != null && player.Y < Y ? Vector2.UnitY * -1f : Vector2.UnitY;
            yield return Windup(direction * -20f, 0.18f);
            yield return Slam(arenaAnchor + direction * (currentPhase == BossPhase.Cataclysm ? 152f : 120f), direction, 0.16f);
            yield return ReturnToAnchor(0.24f);
        }

        private IEnumerator CornerPinballAttack()
        {
            Vector2[] corners =
            {
                arenaAnchor + new Vector2(-136f, -104f),
                arenaAnchor + new Vector2(136f, -104f),
                arenaAnchor + new Vector2(136f, 104f),
                arenaAnchor + new Vector2(-136f, 104f)
            };

            int startIndex = Calc.Random.Next(corners.Length);
            Vector2 current = Position;

            for (int i = 0; i < 3; i++)
            {
                Vector2 target = corners[(startIndex + i) % corners.Length];
                Vector2 direction = (target - current).SafeNormalize();
                yield return Slam(target, direction, 0.18f);
                current = target;
                yield return 0.08f;
            }

            yield return ReturnToAnchor(0.28f);
        }

        private IEnumerator TremorBurstAttack()
        {
            var level = Scene as Level;
            for (int i = 0; i < 4; i++)
            {
                Audio.Play("event:/game/06_reflection/fallblock_boss_impact", Position);
                impactWiggler.Start();
                level?.Shake(0.35f);
                level?.Displacement.AddBurst(Position, 0.75f, 36f + i * 20f, 96f + i * 24f, 0.35f);
                EmitImpactParticles(i % 2 == 0 ? Vector2.UnitX : Vector2.UnitY);
                PushNearbyPlayer((i % 2 == 0 ? Vector2.UnitX : Vector2.UnitY) * (i % 4 < 2 ? 1f : -1f), 92f, 120f);
                yield return 0.18f;
            }
        }

        private IEnumerator Windup(Vector2 offset, float duration)
        {
            Audio.Play("event:/game/05_mirror_temple/eye_pulse", Position);
            Vector2 start = Position;
            Vector2 target = Position + offset;

            for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime / duration)
            {
                Position = Vector2.Lerp(start, target, Ease.CubeOut(progress));
                yield return null;
            }

            Position = target;
        }

        private IEnumerator Slam(Vector2 target, Vector2 direction, float duration)
        {
            Vector2 start = Position;

            for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime / duration)
            {
                Position = Vector2.Lerp(start, target, Ease.CubeIn(progress));
                yield return null;
            }

            Position = target;
            OnImpact(direction);
        }

        private IEnumerator ReturnToAnchor(float duration)
        {
            Vector2 start = Position;
            for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime / duration)
            {
                Position = Vector2.Lerp(start, arenaAnchor, Ease.CubeOut(progress));
                yield return null;
            }

            Position = arenaAnchor;
        }

        private void OnImpact(Vector2 direction)
        {
            Audio.Play("event:/game/06_reflection/fallblock_boss_impact", Position);
            impactWiggler.Start();

            var level = Scene as Level;
            level?.Shake(currentPhase == BossPhase.Cataclysm ? 0.8f : 0.5f);
            level?.Displacement.AddBurst(Position, 1f, 42f, 132f, 0.45f);

            EmitImpactParticles(direction);
            PushNearbyPlayer(direction, currentPhase == BossPhase.Cataclysm ? 118f : 92f, currentPhase == BossPhase.Cataclysm ? 220f : 170f);
        }

        private void EmitImpactParticles(Vector2 moved)
        {
            Level level = SceneAs<Level>();
            if (level == null)
                return;

            if (moved.X != 0f)
            {
                float angle = moved.X > 0f ? MathHelper.Pi : 0f;
                float x = moved.X > 0f ? Right + 1f : Left - 1f;
                Vector2 spread = new Vector2(0f, 2f);
                for (int offset = 0; offset < Height; offset += 8)
                {
                    Vector2 point = new Vector2(x, Top + 4f + offset);
                    level.ParticlesFG.Emit(CrushBlock.P_Impact, point + spread, angle);
                    level.ParticlesFG.Emit(CrushBlock.P_Impact, point - spread, angle);
                }
            }

            if (moved.Y != 0f)
            {
                float angle = moved.Y > 0f ? -MathHelper.PiOver2 : MathHelper.PiOver2;
                float y = moved.Y > 0f ? Bottom + 1f : Top - 1f;
                Vector2 spread = new Vector2(2f, 0f);
                for (int offset = 0; offset < Width; offset += 8)
                {
                    Vector2 point = new Vector2(Left + 4f + offset, y);
                    level.ParticlesFG.Emit(CrushBlock.P_Impact, point + spread, angle);
                    level.ParticlesFG.Emit(CrushBlock.P_Impact, point - spread, angle);
                }
            }
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
            Audio.Play("event:/game/06_reflection/fallblock_boss_impact", Position);
            impactWiggler.Start();

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
            SetFace(isAngry: false);

            var level = Scene as Level;
            if (level != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 dir = i % 2 == 0 ? Vector2.UnitX : Vector2.UnitY;
                    OnImpact(dir * (i % 4 < 2 ? 1f : -1f));
                    yield return 0.15f;
                }

                level.Session.SetFlag("tumble_kevin_boss_defeated");
            }

            RemoveSelf();
        }
    }
}

