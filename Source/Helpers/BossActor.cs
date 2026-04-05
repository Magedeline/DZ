using System;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Helpers
{
    public enum BossTelegraphType
    {
        None,
        DangerRed,
        RecoverGreen,
        TeleportYellow,
        PositioningOrange,
        DashCyan,
        GuardGold,
        SpecialPurple
    }

    /// <summary>
    /// Base class for boss entities that behave like Celeste's FinalBoss.
    /// Extends Entity with sprite management, collision, and basic boss infrastructure.
    /// </summary>
    [Tracked(true)]
    public class BossActor : Entity
    {
        public Sprite Sprite { get; protected set; }
        protected string spriteName;
        protected Vector2 spriteScale;
        protected bool Grounded => Scene != null && Collider != null && Scene.CollideCheck<Solid>(Position + Vector2.UnitY);
        protected bool EncounterStarted { get; private set; }
        protected bool AutoStartEncounter { get; private set; } = true;
        protected bool HideUntilEncounterStarts { get; private set; }

        public Vector2 Speed;
        protected float maxFall;
        protected float gravityMult;
        protected bool solidCollidable;
        private readonly bool defaultCollidable;
        private BossTelegraphType activeTelegraph = BossTelegraphType.None;
        private float telegraphTimer;
        private float telegraphPulse;

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public bool IsDefeated { get; set; }

        public BossActor(
            Vector2 position,
            string spriteName,
            Vector2 spriteScale,
            float maxFall,
            bool collidable,
            bool solidCollidable,
            float gravityMult,
            Collider collider)
            : base(position)
        {
            this.spriteName = spriteName;
            this.spriteScale = spriteScale;
            this.maxFall = maxFall;
            this.Collidable = collidable;
            this.solidCollidable = solidCollidable;
            this.gravityMult = gravityMult;
            this.defaultCollidable = collidable;

            if (collider != null)
                this.Collider = collider;

            Depth = -8500;

            try
            {
                if (GFX.SpriteBank != null && GFX.SpriteBank.Has(spriteName))
                {
                    Sprite = GFX.SpriteBank.Create(spriteName);
                    Sprite.Scale = spriteScale;
                    Add(Sprite);
                }
            }
            catch
            {
                // Sprite bank entry not found at load time; subclasses can handle this
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (AutoStartEncounter)
                StartBossFight();
        }

        public override void Update()
        {
            base.Update();

            if (telegraphTimer > 0f)
            {
                telegraphTimer = Math.Max(0f, telegraphTimer - Engine.DeltaTime);
                telegraphPulse += Engine.DeltaTime;

                if (telegraphTimer <= 0f)
                    activeTelegraph = BossTelegraphType.None;
            }

            if (!EncounterStarted && !AutoStartEncounter)
                return;

            if (gravityMult > 0f)
            {
                Speed.Y = Math.Min(Speed.Y + 900f * gravityMult * Engine.DeltaTime, maxFall);
            }

            Position += Speed * Engine.DeltaTime;
        }

        public override void Render()
        {
            base.Render();

            if (!Visible || activeTelegraph == BossTelegraphType.None || telegraphTimer <= 0f)
                return;

            float baseSize = 18f;
            float pulseScale = 1f + (float)Math.Sin(telegraphPulse * 16f) * 0.08f;
            float size = baseSize * pulseScale;
            float alpha = telegraphTimer < 0.12f ? telegraphTimer / 0.12f : 1f;
            Vector2 drawPos = GetTelegraphDrawPosition(size);
            Color color = GetTelegraphColor(activeTelegraph) * alpha;

            Draw.Rect(drawPos.X - 2f, drawPos.Y - 2f, size + 4f, size + 4f, Color.Black * (0.8f * alpha));
            Draw.Rect(drawPos.X, drawPos.Y, size, size, color);
            Draw.Rect(drawPos.X + 3f, drawPos.Y + 3f, size - 6f, size - 6f, Color.White * (0.18f * alpha));
        }

        protected void ConfigureEncounter(bool autoStart, bool hideUntilStart)
        {
            AutoStartEncounter = autoStart;
            HideUntilEncounterStarts = hideUntilStart;

            if (!autoStart && hideUntilStart)
            {
                Visible = false;
                Collidable = false;
            }
        }

        protected void StartBossRoutine(IEnumerator routine)
        {
            Add(new Coroutine(RunEncounterRoutine(routine)));
        }

        protected IEnumerator TelegraphIntent(BossTelegraphType telegraph, float duration = 0.5f)
        {
            if (telegraph == BossTelegraphType.None || duration <= 0f)
                yield break;

            ShowTelegraph(telegraph, duration);
            yield return duration;
        }

        protected void ShowTelegraph(BossTelegraphType telegraph, float duration = 0.5f)
        {
            activeTelegraph = telegraph;
            telegraphTimer = duration;
            telegraphPulse = 0f;
        }

        protected void ClearTelegraph()
        {
            activeTelegraph = BossTelegraphType.None;
            telegraphTimer = 0f;
        }

        protected BossTelegraphType InferTelegraphFromAttackName(string attackName)
        {
            if (string.IsNullOrWhiteSpace(attackName))
                return BossTelegraphType.DangerRed;

            if (attackName.Contains("Teleport", StringComparison.OrdinalIgnoreCase)
                || attackName.Contains("Warp", StringComparison.OrdinalIgnoreCase)
                || attackName.Contains("Step", StringComparison.OrdinalIgnoreCase)
                || attackName.Contains("Ambush", StringComparison.OrdinalIgnoreCase))
            {
                return BossTelegraphType.TeleportYellow;
            }

            if (attackName.Contains("Dash", StringComparison.OrdinalIgnoreCase)
                || attackName.Contains("Rush", StringComparison.OrdinalIgnoreCase)
                || attackName.Contains("Charge", StringComparison.OrdinalIgnoreCase)
                || attackName.Contains("Slam", StringComparison.OrdinalIgnoreCase)
                || attackName.Contains("Lance", StringComparison.OrdinalIgnoreCase)
                || attackName.Contains("Piercer", StringComparison.OrdinalIgnoreCase))
            {
                return BossTelegraphType.DashCyan;
            }

            if (attackName.Contains("Guard", StringComparison.OrdinalIgnoreCase)
                || attackName.Contains("Shield", StringComparison.OrdinalIgnoreCase)
                || attackName.Contains("Shell", StringComparison.OrdinalIgnoreCase))
            {
                return BossTelegraphType.GuardGold;
            }

            if (attackName.Contains("Line", StringComparison.OrdinalIgnoreCase)
                || attackName.Contains("Cage", StringComparison.OrdinalIgnoreCase)
                || attackName.Contains("Prison", StringComparison.OrdinalIgnoreCase)
                || attackName.Contains("Rift", StringComparison.OrdinalIgnoreCase)
                || attackName.Contains("Rain", StringComparison.OrdinalIgnoreCase)
                || attackName.Contains("Ring", StringComparison.OrdinalIgnoreCase))
            {
                return BossTelegraphType.PositioningOrange;
            }

            return BossTelegraphType.DangerRed;
        }

        private IEnumerator RunEncounterRoutine(IEnumerator routine)
        {
            while (!EncounterStarted)
                yield return null;

            while (routine.MoveNext())
                yield return routine.Current;
        }

        public virtual void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health < 0) Health = 0;
            if (Health <= 0) IsDefeated = true;
        }

        public virtual void StartBossFight()
        {
            if (EncounterStarted)
                return;

            EncounterStarted = true;

            if (HideUntilEncounterStarts)
            {
                Visible = true;
                Collidable = defaultCollidable;
            }
        }

        public Level GetLevel()
        {
            return SceneAs<Level>();
        }

        private Vector2 GetTelegraphDrawPosition(float size)
        {
            float top = Collider != null ? Top : Y;
            return new Vector2(Center.X - size * 0.5f, top - size - 12f);
        }

        private static Color GetTelegraphColor(BossTelegraphType telegraph)
        {
            return telegraph switch
            {
                BossTelegraphType.DangerRed => Calc.HexToColor("ff5166"),
                BossTelegraphType.RecoverGreen => Calc.HexToColor("74ff90"),
                BossTelegraphType.TeleportYellow => Calc.HexToColor("ffe866"),
                BossTelegraphType.PositioningOrange => Calc.HexToColor("ffad5a"),
                BossTelegraphType.DashCyan => Calc.HexToColor("6cecff"),
                BossTelegraphType.GuardGold => Calc.HexToColor("ffd24d"),
                BossTelegraphType.SpecialPurple => Calc.HexToColor("cb7dff"),
                _ => Color.White
            };
        }
    }
}
