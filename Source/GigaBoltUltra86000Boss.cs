using Celeste.Effects;
using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// Giga Bolt Ultra 86000 Boss - standalone electrical superweapon for future DX chapter use.
    /// Uses the shared lightning effects pipeline so the encounter already has real visual output.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/GigaBoltUltra86000Boss")]
    [Tracked]
    [HotReloadable]
    public class GigaBoltUltra86000Boss : BossActor
    {
        private const string SpriteRoot = "characters/gigaboltultra86000/";

        private enum BossPhase
        {
            Boot,
            Surge,
            Overload,
            Defeated
        }

        private enum AttackType
        {
            ArcLance,
            ThunderCage,
            PlasmaVolley,
            EMPCrash
        }

        private int health = 1100;
        private int maxHealth = 1100;
        private bool defeatSequenceStarted;
        private BossPhase currentPhase = BossPhase.Boot;

        private Sprite bodySprite;
        private VertexLight electricGlow;
        private SoundSource chargeLoop;
        private Vector2 arenaAnchor;
        private float hoverTimer;
        private float hoverRadius = 88f;

        public GigaBoltUltra86000Boss(EntityData data, Vector2 offset)
            : base(data.Position + offset, "giga_bolt_ultra_86000_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(52f, 52f, -26f, -26f))
        {
            maxHealth = data.Int("maxHealth", 1100);
            health = Math.Min(data.Int("health", maxHealth), maxHealth);
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            arenaAnchor = Position;
            SetupVisuals();
        }

        public GigaBoltUltra86000Boss(Vector2 position)
            : base(position, "giga_bolt_ultra_86000_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(52f, 52f, -26f, -26f))
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

            Add(electricGlow = new VertexLight(Color.Cyan, 0.7f, 32, 84));
            electricGlow.Position = new Vector2(0f, -6f);

            Add(chargeLoop = new SoundSource());
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
            AddLoopIfExists(sprite, "overload", "overload", 0.04f);
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
                (float)Math.Cos(hoverTimer * 0.75f) * hoverRadius,
                -72f + (float)Math.Sin(hoverTimer * 2.1f) * 20f);

            Position = Vector2.Lerp(Position, hoverTarget, 2.1f * Engine.DeltaTime);

            if (electricGlow != null)
            {
                electricGlow.Color = currentPhase == BossPhase.Overload ? Color.Magenta : Color.Cyan;
                electricGlow.Alpha = 0.5f + (float)Math.Sin(hoverTimer * 6f) * 0.25f;
            }
        }

        private IEnumerator BossRoutine()
        {
            yield return 0.8f;

            while (!defeatSequenceStarted && health > 0)
            {
                UpdatePhase();
                yield return ExecuteAttack(ChooseAttack());
                yield return currentPhase == BossPhase.Overload ? 0.75f : 1.15f;
            }
        }

        private void UpdatePhase()
        {
            BossPhase nextPhase = health switch
            {
                <= 350 => BossPhase.Overload,
                <= 700 => BossPhase.Surge,
                _ => BossPhase.Boot
            };

            if (nextPhase == currentPhase)
                return;

            currentPhase = nextPhase;
            LightningEffects.ChargeEntity(this, currentPhase == BossPhase.Overload ? 0.9f : 0.6f, 1.4f);

            if (bodySprite == null)
                return;

            if (currentPhase == BossPhase.Overload && bodySprite.Has("overload"))
                bodySprite.Play("overload");
            else if (bodySprite.Has("charge"))
                bodySprite.Play("charge");
            else if (bodySprite.Has("idle"))
                bodySprite.Play("idle");
        }

        private AttackType ChooseAttack()
        {
            if (currentPhase == BossPhase.Overload && Calc.Random.Chance(0.4f))
                return AttackType.EMPCrash;

            return (AttackType)Calc.Random.Next(0, 3);
        }

        private IEnumerator ExecuteAttack(AttackType attack)
        {
            yield return TelegraphIntent(attack switch
            {
                AttackType.ArcLance => BossTelegraphType.DashCyan,
                AttackType.ThunderCage => BossTelegraphType.PositioningOrange,
                AttackType.EMPCrash => BossTelegraphType.SpecialPurple,
                _ => BossTelegraphType.DangerRed
            });

            if (bodySprite != null)
            {
                if (attack == AttackType.EMPCrash && bodySprite.Has("overload"))
                    bodySprite.Play("overload");
                else if (bodySprite.Has("attack"))
                    bodySprite.Play("attack");
            }

            switch (attack)
            {
                case AttackType.ArcLance:
                    yield return ArcLanceAttack();
                    break;
                case AttackType.ThunderCage:
                    yield return ThunderCageAttack();
                    break;
                case AttackType.PlasmaVolley:
                    yield return PlasmaVolleyAttack();
                    break;
                case AttackType.EMPCrash:
                    yield return EMPCrashAttack();
                    break;
            }

            if (!defeatSequenceStarted && bodySprite != null)
            {
                if (currentPhase == BossPhase.Overload && bodySprite.Has("overload"))
                    bodySprite.Play("overload");
                else if (bodySprite.Has("idle"))
                    bodySprite.Play("idle");
            }
        }

        private IEnumerator ArcLanceAttack()
        {
            var level = Scene as Level;
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (level == null || player == null)
                yield break;

            chargeLoop.Play(LightningEffects.SFX_LIGHTNING_CRACKLE);
            yield return 0.25f;

            Vector2 target = player.Position + new Vector2(0f, -8f);
            LightningEffects.CreateLightningStrike(level, Position, target);
            yield return 0.35f;
        }

        private IEnumerator ThunderCageAttack()
        {
            var level = Scene as Level;
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (level == null || player == null)
                yield break;

            Vector2 center = player.Position;
            float radius = currentPhase == BossPhase.Overload ? 68f : 54f;
            LightningEffects.CreateElectricField(level, center, radius, 1.8f);

            Vector2[] offsets =
            {
                new Vector2(-radius, 0f),
                new Vector2(radius, 0f),
                new Vector2(0f, -radius),
                new Vector2(0f, radius)
            };

            for (int i = 0; i < offsets.Length; i++)
            {
                LightningEffects.CreateLightningBolt(level, Position, center + offsets[i]);
                yield return 0.12f;
            }
        }

        private IEnumerator PlasmaVolleyAttack()
        {
            var level = Scene as Level;
            if (level == null)
                yield break;

            int burstCount = currentPhase == BossPhase.Overload ? 5 : 3;
            for (int i = 0; i < burstCount; i++)
            {
                Vector2 burstPos = Position + new Vector2(
                    Calc.Random.Range(-96f, 96f),
                    Calc.Random.Range(-48f, 64f));

                LightningEffects.CreatePlasmaBall(level, burstPos, currentPhase == BossPhase.Overload ? 30f : 22f);
                yield return 0.16f;
            }
        }

        private IEnumerator EMPCrashAttack()
        {
            var level = Scene as Level;
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (level == null)
                yield break;

            Vector2 impactCenter = player?.Position ?? Position;
            chargeLoop.Play(LightningEffects.SFX_LIGHTNING_STATIC);
            yield return 0.35f;

            LightningEffects.CreateEMP(level, impactCenter, 112f);
            LightningEffects.CreateChainLightning(level, impactCenter, 96f, 4);
            yield return 0.45f;
        }

        public override void TakeDamage(int damage)
        {
            if (defeatSequenceStarted)
                return;

            health = Math.Max(0, health - damage);
            Health = health;
            Audio.Play(LightningEffects.SFX_LIGHTNING_ZAP, Position);

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
            chargeLoop.Stop();
            LightningEffects.DischargeEntity(this);

            if (bodySprite != null && bodySprite.Has("defeat"))
                bodySprite.Play("defeat");

            var level = Scene as Level;
            if (level != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 burstPos = Position + Calc.Random.Range(Vector2.One * -48f, Vector2.One * 48f);
                    LightningEffects.CreatePlasmaBall(level, burstPos, 20f);
                    level.Shake(0.35f);
                    yield return 0.18f;
                }

                LightningEffects.CreateEMP(level, Position, 128f);
                level.Session.SetFlag("giga_bolt_ultra_86000_boss_defeated");
            }

            RemoveSelf();
        }
    }
}