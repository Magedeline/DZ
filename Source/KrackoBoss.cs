using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// Kracko Boss - Standalone storm eye encounter for future DX chapter use.
    /// Keeps chapter 2 boss ownership separate from the grouped Kirby mid-boss path.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/KrackoBoss")]
    [Tracked]
    [HotReloadable]
    public class KrackoBoss : BossActor
    {
        private const string SpriteRoot = "bosses/kirby/kracko/";

        private enum BossPhase
        {
            Calm,
            Charged,
            Tempest,
            Defeated
        }

        private enum AttackType
        {
            LightningBurst,
            DiveCharge,
            ThunderRain,
            TempestBurst
        }

        private int health = 900;
        private int maxHealth = 900;
        private bool defeatSequenceStarted;
        private BossPhase currentPhase = BossPhase.Calm;

        private Sprite bodySprite;
        private VertexLight lightningGlow;
        private Vector2 arenaAnchor;
        private float hoverTimer;
        private float hoverRadius = 72f;

        public KrackoBoss(EntityData data, Vector2 offset)
            : base(data.Position + offset, "kracko_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(40f, 40f, -20f, -20f))
        {
            maxHealth = data.Int("maxHealth", 900);
            health = Math.Min(data.Int("health", maxHealth), maxHealth);
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            arenaAnchor = Position;
            SetupVisuals();
        }

        public KrackoBoss(Vector2 position)
            : base(position, "kracko_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(40f, 40f, -20f, -20f))
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

            Add(lightningGlow = new VertexLight(Color.Cyan, 0.65f, 28, 72));
            lightningGlow.Position = new Vector2(0f, -4f);
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
            AddLoopIfExists(sprite, "charge", "charge", 0.07f);
            AddLoopIfExists(sprite, "attack", "attack", 0.05f);
            AddLoopIfExists(sprite, "storm", "storm", 0.05f);
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

            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            Vector2 focus = player?.Position ?? arenaAnchor;
            Vector2 hoverTarget = focus + new Vector2(
                (float)Math.Sin(hoverTimer * 0.9f) * hoverRadius,
                -56f + (float)Math.Sin(hoverTimer * 2.4f) * 18f);

            Position = Vector2.Lerp(Position, hoverTarget, 2.2f * Engine.DeltaTime);

            if (lightningGlow != null)
            {
                lightningGlow.Alpha = 0.45f + (float)Math.Sin(hoverTimer * 5f) * 0.2f;
                lightningGlow.Color = currentPhase >= BossPhase.Charged ? Color.Yellow : Color.Cyan;
            }
        }

        private IEnumerator BossRoutine()
        {
            yield return 0.75f;

            while (!defeatSequenceStarted && health > 0)
            {
                UpdatePhase();
                yield return ExecuteAttack(ChooseAttack());
                yield return currentPhase == BossPhase.Tempest ? 0.8f : 1.2f;
            }
        }

        private void UpdatePhase()
        {
            BossPhase nextPhase = health switch
            {
                <= 300 => BossPhase.Tempest,
                <= 600 => BossPhase.Charged,
                _ => BossPhase.Calm
            };

            if (nextPhase == currentPhase)
                return;

            currentPhase = nextPhase;

            if (bodySprite != null)
            {
                if (currentPhase == BossPhase.Tempest && bodySprite.Has("storm"))
                    bodySprite.Play("storm");
                else if (bodySprite.Has("charge"))
                    bodySprite.Play("charge");
                else if (bodySprite.Has("idle"))
                    bodySprite.Play("idle");
            }
        }

        private AttackType ChooseAttack()
        {
            if (currentPhase == BossPhase.Tempest && Calc.Random.Chance(0.35f))
                return AttackType.TempestBurst;

            return (AttackType)Calc.Random.Next(0, 3);
        }

        private IEnumerator ExecuteAttack(AttackType attack)
        {
            yield return TelegraphIntent(attack switch
            {
                AttackType.DiveCharge => BossTelegraphType.DashCyan,
                AttackType.ThunderRain => BossTelegraphType.PositioningOrange,
                AttackType.TempestBurst => BossTelegraphType.SpecialPurple,
                _ => BossTelegraphType.DangerRed
            });

            if (bodySprite != null)
            {
                if ((attack == AttackType.LightningBurst || attack == AttackType.ThunderRain) && bodySprite.Has("charge"))
                    bodySprite.Play("charge");
                else if (bodySprite.Has("attack"))
                    bodySprite.Play("attack");
            }

            switch (attack)
            {
                case AttackType.LightningBurst:
                    yield return LightningBurstAttack();
                    break;
                case AttackType.DiveCharge:
                    yield return DiveChargeAttack();
                    break;
                case AttackType.ThunderRain:
                    yield return ThunderRainAttack();
                    break;
                case AttackType.TempestBurst:
                    yield return TempestBurstAttack();
                    break;
            }

            if (!defeatSequenceStarted && bodySprite != null)
            {
                if (currentPhase == BossPhase.Tempest && bodySprite.Has("storm"))
                    bodySprite.Play("storm");
                else if (bodySprite.Has("idle"))
                    bodySprite.Play("idle");
            }
        }

        private IEnumerator LightningBurstAttack()
        {
            var level = Scene as Level;

            for (int i = 0; i < 3; i++)
            {
                Audio.Play("event:/game/06_reflection/lightning_strike", Position);
                level?.Flash(Color.LightYellow, false);
                level?.Shake(0.3f);
                level?.Displacement.AddBurst(Position, 0.55f, 32f, 108f, 0.35f);
                yield return 0.25f;
            }
        }

        private IEnumerator DiveChargeAttack()
        {
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (player == null)
                yield break;

            Vector2 start = Position;
            Vector2 target = player.Position + new Vector2(0f, -8f);

            Audio.Play("event:/game/05_mirror_temple/eye_pulse", Position);

            for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime * 2.6f)
            {
                Position = Vector2.Lerp(start, target, progress);
                yield return null;
            }

            var level = Scene as Level;
            level?.Shake(0.6f);
            level?.Displacement.AddBurst(Position, 0.8f, 40f, 128f, 0.45f);
        }

        private IEnumerator ThunderRainAttack()
        {
            var level = Scene as Level;
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            Vector2 center = player?.Position ?? Position;
            int boltCount = currentPhase == BossPhase.Tempest ? 7 : 5;

            for (int i = 0; i < boltCount; i++)
            {
                Vector2 strikePos = center + new Vector2(Calc.Random.Range(-96f, 96f), Calc.Random.Range(-48f, 48f));
                Audio.Play("event:/game/06_reflection/lightning_strike", strikePos);
                level?.Displacement.AddBurst(strikePos, 0.45f, 24f, 72f, 0.25f);
                yield return 0.18f;
            }
        }

        private IEnumerator TempestBurstAttack()
        {
            var level = Scene as Level;

            for (float timer = 0f; timer < 1.4f; timer += Engine.DeltaTime)
            {
                float angle = timer * 9f;
                Vector2 burstPos = Position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 72f;
                level?.Displacement.AddBurst(burstPos, 0.35f, 20f, 64f, 0.2f);

                if (Scene.OnInterval(0.2f))
                    Audio.Play("event:/game/06_reflection/lightning_strike", burstPos);

                yield return null;
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
            for (int i = 0; i < 5; i++)
            {
                level?.Flash(Color.White, false);
                level?.Displacement.AddBurst(Position, 0.8f, 36f, 120f, 0.4f);
                yield return 0.2f;
            }

            level?.Session.SetFlag("kracko_boss_defeated");
            RemoveSelf();
        }
    }
}
