using Celeste.Entities.Projectiles;
using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    [CustomEntity(ids: "MaggyHelper/AlphaApexPredatorBoss")]
    [Tracked]
    [HotReloadable]
    public class AlphaApexPredatorBoss : BossActor
    {
        private const string PredatorTexturePath = "characters/monsters/predator";
        private const string FallbackPredatorTexturePath = "characters/Enemies/monsters/predator";

        private enum BossPhase
        {
            Hunt,
            Alpha,
            Extinction,
            Defeated
        }

        private enum AttackType
        {
            HunterRush,
            PhantomAmbush,
            ClawCyclone,
            AlphaHowl
        }

        private int health = 1600;
        private int maxHealth = 1600;
        private bool defeatSequenceStarted;
        private bool attackInProgress;
        private BossPhase currentPhase = BossPhase.Hunt;

        private readonly List<MTexture> bodyFrames = new();
        private Image bodyImage;
        private VertexLight eyeGlow;
        private BloomPoint bloom;
        private Wiggler impactWiggler;
        private Vector2 arenaAnchor;
        private Vector2 lastKnownPlayerPos;
        private float idleTimer;
        private float animationTimer;

        public AlphaApexPredatorBoss(EntityData data, Vector2 offset)
            : base(data.Position + offset, "alpha_apex_predator_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(58f, 42f, -29f, -42f))
        {
            maxHealth = data.Int("maxHealth", 1600);
            health = Math.Min(data.Int("health", maxHealth), maxHealth);
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            arenaAnchor = Position;
            SetupVisuals();
        }

        public AlphaApexPredatorBoss(Vector2 position)
            : base(position, "alpha_apex_predator_boss", Vector2.One, 0f, true, false, 0f,
                new Hitbox(58f, 42f, -29f, -42f))
        {
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            arenaAnchor = position;
            SetupVisuals();
        }

        private void SetupVisuals()
        {
            bodyFrames.AddRange(GetFrames(PredatorTexturePath));
            if (bodyFrames.Count == 0)
                bodyFrames.AddRange(GetFrames(FallbackPredatorTexturePath));

            if (bodyFrames.Count > 0)
            {
                Add(bodyImage = new Image(bodyFrames[0]));
                bodyImage.CenterOrigin();
                bodyImage.Scale = new Vector2(1.65f, 1.65f);
            }

            Add(eyeGlow = new VertexLight(Calc.HexToColor("ffcf40"), 0.9f, 24, 54));
            eyeGlow.Position = new Vector2(12f, -18f);
            Add(bloom = new BloomPoint(0.7f, 16f));
            Add(impactWiggler = Wiggler.Create(0.32f, 4f));
            Add(new PlayerCollider(OnPlayerTouch));
        }

        private static List<MTexture> GetFrames(string path)
        {
            if (GFX.Game == null || !GFX.Game.Has(path + "00"))
                return new List<MTexture>();

            return GFX.Game.GetAtlasSubtextures(path);
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
            animationTimer += Engine.DeltaTime;
            UpdatePhase();
            UpdateIdleMotion();
            UpdateVisuals();
        }

        private void UpdateIdleMotion()
        {
            if (attackInProgress)
                return;

            Vector2 idleTarget = arenaAnchor + new Vector2((float)Math.Cos(idleTimer * 0.9f) * 12f, (float)Math.Sin(idleTimer * 1.7f) * 5f);
            Position = Vector2.Lerp(Position, idleTarget, 2.6f * Engine.DeltaTime);
        }

        private void UpdateVisuals()
        {
            if (bodyImage != null && bodyFrames.Count > 0)
            {
                int frame = (int)(animationTimer * (currentPhase == BossPhase.Extinction ? 22f : 14f)) % bodyFrames.Count;
                bodyImage.Texture = bodyFrames[frame];
                bodyImage.Scale.X = lastKnownPlayerPos.X < X ? -1.65f : 1.65f;
                bodyImage.Color = currentPhase switch
                {
                    BossPhase.Hunt => Color.White,
                    BossPhase.Alpha => new Color(255, 230, 230),
                    BossPhase.Extinction => new Color(255, 190, 190),
                    _ => Color.White
                };
            }

            if (eyeGlow != null)
            {
                eyeGlow.Color = currentPhase == BossPhase.Extinction ? Calc.HexToColor("ff5e3a") : Calc.HexToColor("ffcf40");
                eyeGlow.Alpha = 0.55f + (float)Math.Sin(idleTimer * 9f) * 0.18f;
            }

            if (bloom != null)
                bloom.Alpha = 0.4f + impactWiggler.Value * 0.28f;
        }

        private IEnumerator BossRoutine()
        {
            yield return 1f;

            while (!defeatSequenceStarted && health > 0)
            {
                attackInProgress = true;
                yield return ExecuteAttack(ChooseAttack());
                attackInProgress = false;
                yield return currentPhase == BossPhase.Extinction ? 0.65f : 0.95f;
            }
        }

        private void UpdatePhase()
        {
            BossPhase nextPhase = health switch
            {
                <= 420 => BossPhase.Extinction,
                <= 960 => BossPhase.Alpha,
                _ => BossPhase.Hunt
            };

            if (nextPhase == currentPhase)
                return;

            currentPhase = nextPhase;
            impactWiggler.Start();
            var level = Scene as Level;
            level?.Shake(currentPhase == BossPhase.Extinction ? 0.55f : 0.3f);
            Audio.Play(currentPhase == BossPhase.Extinction ? "event:/apexpredator_primal_roar" : "event:/apexpredator_apex_roar", Position);
        }

        private AttackType ChooseAttack()
        {
            if (currentPhase == BossPhase.Extinction && Calc.Random.Chance(0.35f))
                return AttackType.AlphaHowl;

            return (AttackType)Calc.Random.Next(0, 4);
        }

        private IEnumerator ExecuteAttack(AttackType attack)
        {
            yield return TelegraphIntent(attack switch
            {
                AttackType.HunterRush => BossTelegraphType.DashCyan,
                AttackType.PhantomAmbush => BossTelegraphType.TeleportYellow,
                AttackType.AlphaHowl => BossTelegraphType.SpecialPurple,
                _ => BossTelegraphType.DangerRed
            });

            switch (attack)
            {
                case AttackType.HunterRush:
                    yield return HunterRushAttack();
                    break;
                case AttackType.PhantomAmbush:
                    yield return PhantomAmbushAttack();
                    break;
                case AttackType.ClawCyclone:
                    yield return ClawCycloneAttack();
                    break;
                case AttackType.AlphaHowl:
                    yield return AlphaHowlAttack();
                    break;
            }

            yield return ReturnToAnchor(0.24f);
        }

        private IEnumerator HunterRushAttack()
        {
            global::Celeste.Player player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            float direction = player != null && player.X < X ? -1f : 1f;

            Audio.Play("event:/apexpredator_bloodlust_charge", Position);
            yield return MoveTo(arenaAnchor + new Vector2(-direction * 150f, -12f), 0.16f);
            yield return DashTo(arenaAnchor + new Vector2(direction * 170f, Calc.Random.Range(-18f, 18f)), 0.14f);
            PushNearbyPlayer(new Vector2(direction, 0f), 118f, currentPhase == BossPhase.Extinction ? 240f : 180f);
        }

        private IEnumerator PhantomAmbushAttack()
        {
            global::Celeste.Player player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            Vector2 target = (player?.Center ?? lastKnownPlayerPos) + new Vector2(player != null && player.X < X ? 92f : -92f, -12f);
            Audio.Play("event:/apexpredator_vanish", Position);
            TeleportBurst(Position, target);
            yield return 0.08f;
            Audio.Play("event:/apexpredator_ambush_strike", Position);
            yield return DashTo(player?.Center ?? arenaAnchor, 0.14f);
        }

        private IEnumerator ClawCycloneAttack()
        {
            Audio.Play("event:/apexpredator_frenzy_claws", Position);
            Vector2 center = lastKnownPlayerPos == Vector2.Zero ? arenaAnchor : lastKnownPlayerPos;
            float radius = currentPhase == BossPhase.Extinction ? 96f : 76f;

            for (int i = 0; i < 4; i++)
            {
                float angle = idleTimer * 4f + i * MathHelper.PiOver2;
                Vector2 target = center + Calc.AngleToVector(angle, radius);
                yield return DashTo(target, 0.1f);
                PushNearbyPlayer((target - center).SafeNormalize(), 88f, 125f);
            }
        }

        private IEnumerator AlphaHowlAttack()
        {
            var level = Scene as Level;
            Audio.Play("event:/apexpredator_primal_roar", Position);
            impactWiggler.Start();
            level?.Flash(Color.OrangeRed * 0.16f, false);
            level?.Shake(0.45f);

            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 velocity = Calc.AngleToVector(angle, currentPhase == BossPhase.Extinction ? 180f : 140f);
                Scene?.Add(new BossStarProjectile(Center, velocity));
            }

            yield return 0.24f;
        }

        private IEnumerator MoveTo(Vector2 target, float duration)
        {
            Vector2 start = Position;
            for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime / duration)
            {
                Position = Vector2.Lerp(start, target, Ease.CubeOut(progress));
                yield return null;
            }

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
            impactWiggler.Start();
            (Scene as Level)?.Displacement.AddBurst(Position, 0.8f, 24f, 84f, 0.22f);
        }

        private IEnumerator ReturnToAnchor(float duration)
        {
            yield return MoveTo(arenaAnchor, duration);
        }

        private void PushNearbyPlayer(Vector2 direction, float radius, float speed)
        {
            global::Celeste.Player player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (player == null)
                return;

            if (Vector2.Distance(player.Center, Center) <= radius)
                player.Speed += direction.SafeNormalize() * speed;
        }

        private void TeleportBurst(Vector2 from, Vector2 to)
        {
            Level level = Scene as Level;
            level?.ParticlesFG.Emit(ParticleTypes.Dust, 10, from, Vector2.One * 8f);
            level?.ParticlesFG.Emit(ParticleTypes.Dust, 10, to, Vector2.One * 8f);
            level?.Displacement.AddBurst(from, 0.8f, 18f, 48f, 0.18f);
            level?.Displacement.AddBurst(to, 0.8f, 18f, 48f, 0.18f);
            Position = to;
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
            Audio.Play("event:/apexpredator_damage", Position);

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
                    level.ParticlesFG.Emit(ParticleTypes.Dust, 8, Center, Vector2.One * 10f);
                    level.Displacement.AddBurst(Position, 0.8f, 18f + i * 8f, 60f + i * 10f, 0.2f);
                    yield return 0.12f;
                }

                level.Session.SetFlag("alpha_apex_predator_boss_defeated");
            }

            RemoveSelf();
        }
    }
}