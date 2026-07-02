using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// Another Vessel Corrupted Boss - standalone vessel fight for future DX chapter use.
    /// Falls back to the existing vessel creation art path until a dedicated animated boss atlas exists.
    /// </summary>
    [CustomEntity(ids: "DZ/AnotherVesselCorruptedBoss")]
    [Tracked]
    [HotReloadable]
    public class AnotherVesselCorruptedBoss : BossActor
    {
        private const string SpriteRoot = "characters/anothervesselcorrupted/";
        private const string FallbackTexturePath = "bgs/DZ/00/anotherhuman/VESSEL";

        private enum BossPhase
        {
            Dormant,
            Fractured,
            Corrupted,
            Defeated
        }

        private enum AttackType
        {
            VoidDash,
            CorruptionWave,
            SoulRift,
            MemoryLance
        }

        private int health = 1000;
        private int maxHealth = 1000;
        private bool defeatSequenceStarted;
        private BossPhase currentPhase = BossPhase.Dormant;

        private Sprite bodySprite;
        private Image fallbackImage;
        private VertexLight corruptionGlow;
        private Vector2 arenaAnchor;
        private float hoverTimer;

        public AnotherVesselCorruptedBoss(EntityData data, Vector2 offset)
            : base(data.Position + offset, "another_vessel_corrupted_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(34f, 58f, -17f, -58f))
        {
            maxHealth = data.Int("maxHealth", 1000);
            health = Math.Min(data.Int("health", maxHealth), maxHealth);
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            arenaAnchor = Position;
            SetupVisuals();
        }

        public AnotherVesselCorruptedBoss(Vector2 position)
            : base(position, "another_vessel_corrupted_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(34f, 58f, -17f, -58f))
        {
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            arenaAnchor = position;
            SetupVisuals();
        }

        private void SetupVisuals()
        {
            bodySprite = TryCreateSprite();
            if (bodySprite != null)
                Add(bodySprite);

            if (GFX.Game.Has(FallbackTexturePath))
            {
                Add(fallbackImage = new Image(GFX.Game[FallbackTexturePath]));
                fallbackImage.CenterOrigin();
                fallbackImage.Color = new Color(170, 150, 190);
            }

            Add(corruptionGlow = new VertexLight(new Color(180, 80, 220), 0.65f, 24, 72));
            corruptionGlow.Position = new Vector2(0f, -16f);
        }

        private static bool HasFrames(string animationPath)
        {
            return !string.IsNullOrWhiteSpace(animationPath)
                && GFX.Game != null
                && GFX.Game.HasAtlasSubtextures(SpriteRoot + animationPath);
        }

        private static void AddLoopIfExists(Sprite sprite, string id, string animationPath, float delay)
        {
            if (sprite != null && HasFrames(animationPath))
                sprite.AddLoop(id, animationPath, delay);
        }

        private static void AddAnimIfExists(Sprite sprite, string id, string animationPath, float delay)
        {
            if (sprite != null && HasFrames(animationPath))
                sprite.Add(id, animationPath, delay);
        }

        private static Sprite TryCreateSprite()
        {
            if (!HasFrames("idle"))
                return null;

            Sprite sprite = new Sprite(GFX.Game, SpriteRoot);
            AddLoopIfExists(sprite, "idle", "idle", 0.1f);
            AddLoopIfExists(sprite, "charge", "charge", 0.06f);
            AddLoopIfExists(sprite, "attack", "attack", 0.05f);
            AddLoopIfExists(sprite, "fracture", "fracture", 0.05f);
            AddAnimIfExists(sprite, "defeat", "defeat", 0.08f);
            sprite.CenterOrigin();

            if (sprite.Has("idle"))
                sprite.Play("idle");

            return sprite;
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

            hoverTimer += Engine.DeltaTime;
            UpdatePhase();
            UpdateMovement();
            UpdateVisuals();
        }

        private void UpdateMovement()
        {
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            Vector2 focus = player?.Position ?? arenaAnchor;
            Vector2 hoverTarget = focus + new Vector2(
                (float)Math.Sin(hoverTimer * 0.85f) * 76f,
                -40f + (float)Math.Sin(hoverTimer * 2.2f) * 15f);

            Position = Vector2.Lerp(Position, hoverTarget, 2f * Engine.DeltaTime);
        }

        private void UpdateVisuals()
        {
            float pulse = 0.85f + (float)Math.Sin(hoverTimer * 4f) * 0.08f;
            Color phaseColor = currentPhase switch
            {
                BossPhase.Dormant => new Color(170, 150, 190),
                BossPhase.Fractured => new Color(170, 95, 205),
                BossPhase.Corrupted => new Color(220, 60, 180),
                _ => Color.White
            };

            if (bodySprite != null)
            {
                bodySprite.Color = phaseColor;
                bodySprite.Scale = Vector2.One * pulse;
            }

            if (fallbackImage != null)
            {
                fallbackImage.Color = phaseColor;
                fallbackImage.Scale = Vector2.One * pulse;
                fallbackImage.Rotation = (float)Math.Sin(hoverTimer * 1.5f) * 0.035f;
            }

            if (corruptionGlow != null)
            {
                corruptionGlow.Color = phaseColor;
                corruptionGlow.Alpha = 0.45f + (float)Math.Sin(hoverTimer * 5f) * 0.2f;
            }
        }

        private IEnumerator BossRoutine()
        {
            yield return 0.9f;

            while (!defeatSequenceStarted && health > 0)
            {
                yield return ExecuteAttack(ChooseAttack());
                yield return currentPhase == BossPhase.Corrupted ? 0.8f : 1.2f;
            }
        }

        private void UpdatePhase()
        {
            BossPhase nextPhase = health switch
            {
                <= 320 => BossPhase.Corrupted,
                <= 650 => BossPhase.Fractured,
                _ => BossPhase.Dormant
            };

            if (nextPhase == currentPhase)
                return;

            currentPhase = nextPhase;
            Audio.Play("event:/game/05_mirror_temple/eye_pulse", Position);

            var level = Scene as Level;
            if (currentPhase == BossPhase.Corrupted)
            {
                level?.Flash(new Color(120, 0, 140), true);
                level?.Shake(0.6f);
            }
            else
            {
                level?.Flash(new Color(90, 0, 120), false);
            }

            if (bodySprite == null)
                return;

            if (currentPhase == BossPhase.Corrupted && bodySprite.Has("fracture"))
                bodySprite.Play("fracture");
            else if (bodySprite.Has("charge"))
                bodySprite.Play("charge");
            else if (bodySprite.Has("idle"))
                bodySprite.Play("idle");
        }

        private AttackType ChooseAttack()
        {
            if (currentPhase == BossPhase.Corrupted && Calc.Random.Chance(0.35f))
                return AttackType.MemoryLance;

            return (AttackType)Calc.Random.Next(0, 3);
        }

        private IEnumerator ExecuteAttack(AttackType attack)
        {
            yield return TelegraphIntent(attack switch
            {
                AttackType.VoidDash => BossTelegraphType.DashCyan,
                AttackType.SoulRift => BossTelegraphType.PositioningOrange,
                AttackType.MemoryLance => BossTelegraphType.SpecialPurple,
                _ => BossTelegraphType.DangerRed
            });

            if (bodySprite != null && bodySprite.Has("attack"))
                bodySprite.Play("attack");

            switch (attack)
            {
                case AttackType.VoidDash:
                    yield return VoidDashAttack();
                    break;
                case AttackType.CorruptionWave:
                    yield return CorruptionWaveAttack();
                    break;
                case AttackType.SoulRift:
                    yield return SoulRiftAttack();
                    break;
                case AttackType.MemoryLance:
                    yield return MemoryLanceAttack();
                    break;
            }

            if (!defeatSequenceStarted && bodySprite != null && bodySprite.Has("idle"))
                bodySprite.Play("idle");
        }

        private IEnumerator VoidDashAttack()
        {
            var level = Scene as Level;
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (player == null)
                yield break;

            Audio.Play("event:/game/05_mirror_temple/eye_pulse", Position);
            Vector2 start = Position;
            Vector2 target = player.Position + new Vector2(player.Facing == Facings.Left ? -28f : 28f, -10f);

            for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime * 2.8f)
            {
                Position = Vector2.Lerp(start, target, Ease.CubeIn(progress));
                yield return null;
            }

            level?.Shake(0.45f);
            level?.Displacement.AddBurst(Position, 0.8f, 36f, 112f, 0.45f);

            if (Vector2.Distance(player.Position, Position) < 36f)
                player.Speed += (player.Position - Position).SafeNormalize() * 170f;
        }

        private IEnumerator CorruptionWaveAttack()
        {
            var level = Scene as Level;
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (level == null)
                yield break;

            Audio.Play("event:/game/06_reflection/feather_bubble_get", Position);
            level.Flash(new Color(120, 0, 160), false);

            for (int ring = 0; ring < 4; ring++)
            {
                level.Displacement.AddBurst(Position, 0.6f, 30f + ring * 30f, 74f + ring * 36f, 0.28f);
                yield return 0.18f;
            }

            if (player != null && Vector2.Distance(player.Position, Position) < 120f)
                player.Speed += (player.Position - Position).SafeNormalize() * 110f;
        }

        private IEnumerator SoulRiftAttack()
        {
            var level = Scene as Level;
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (level == null)
                yield break;

            Vector2 center = player?.Position ?? Position;
            Audio.Play("event:/game/05_mirror_temple/eyeball_burst", center);

            for (int i = 0; i < 3; i++)
            {
                Vector2 burstPos = center + new Vector2(Calc.Random.Range(-72f, 72f), Calc.Random.Range(-36f, 36f));
                level.Displacement.AddBurst(burstPos, 0.55f, 24f, 86f, 0.3f);
                yield return 0.2f;
            }
        }

        private IEnumerator MemoryLanceAttack()
        {
            var level = Scene as Level;
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (level == null || player == null)
                yield break;

            Vector2 center = player.Position;
            Vector2 start = center + new Vector2(-120f, -20f);
            Vector2 end = center + new Vector2(120f, -20f);
            Audio.Play("event:/game/05_mirror_temple/eye_pulse", center);
            level.Flash(new Color(180, 60, 200), true);

            for (int i = 0; i <= 5; i++)
            {
                Vector2 burstPos = Vector2.Lerp(start, end, i / 5f);
                level.Displacement.AddBurst(burstPos, 0.42f, 18f, 66f, 0.25f);
                yield return 0.08f;
            }

            for (int i = 0; i <= 5; i++)
            {
                Vector2 burstPos = Vector2.Lerp(start + new Vector2(0f, 40f), end + new Vector2(0f, 40f), i / 5f);
                level.Displacement.AddBurst(burstPos, 0.42f, 18f, 66f, 0.25f);
                yield return 0.08f;
            }
        }

        public override void TakeDamage(int damage)
        {
            if (defeatSequenceStarted)
                return;

            health = Math.Max(0, health - damage);
            Health = health;
            Audio.Play("event:/game/06_reflection/feather_bubble_get", Position);

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

            if (bodySprite != null && bodySprite.Has("defeat"))
                bodySprite.Play("defeat");

            var level = Scene as Level;
            if (level != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 burstPos = Position + Calc.Random.Range(Vector2.One * -42f, Vector2.One * 42f);
                    level.Flash(Color.White * 0.12f, false);
                    level.Displacement.AddBurst(burstPos, 0.75f, 26f, 96f, 0.35f);
                    yield return 0.18f;
                }

                level.Session.SetFlag("another_vessel_corrupted_boss_defeated");
            }

            RemoveSelf();
        }
    }
}

