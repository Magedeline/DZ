using DZ;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// A gate that opens when the player has defeated a required number of enemies and/or bosses.
    /// Reads from the session's EnemiesDefeated / BossesDefeated counters and, optionally,
    /// from the save data's TotalEnemiesDefeated / TotalBossesDefeated for cross-session tracking.
    /// 
    /// Configurable in Loenn:
    ///   - requiredEnemyDefeats: number of enemy kills needed (0 = ignored)
    ///   - requiredBossDefeats:  number of boss kills needed  (0 = ignored)
    ///   - useGlobalCounts:     if true, reads totals from save data instead of session
    ///   - flag:                optional session flag that is set when the gate opens
    ///   - persistent:          if true, stays open even after room reload once the flag is set
    /// </summary>
    [CustomEntity(ids: "DZ/DefeatSwitchGate")]
    [Tracked]
    [HotReloadable]
    public class DefeatSwitchGate : Solid
    {
        #region Fields

        // Requirements
        private readonly int requiredEnemyDefeats;
        private readonly int requiredBossDefeats;
        private readonly bool useGlobalCounts;
        private readonly string flag;
        private readonly bool persistent;

        // State
        private bool opened;
        private float openProgress;
        private readonly Vector2 closedPosition;
        private readonly float moveDistance;

        // Visuals
        private MTexture texture;
        private MTexture iconTexture;
        private float iconLerp;
        private Color gateColor;

        // Cached
        private Level level;

        #endregion

        #region Constructors

        public DefeatSwitchGate(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            requiredEnemyDefeats = data.Int("requiredEnemyDefeats", 0);
            requiredBossDefeats = data.Int("requiredBossDefeats", 0);
            useGlobalCounts = data.Bool("useGlobalCounts", false);
            flag = data.Attr("flag", "");
            persistent = data.Bool("persistent", false);

            closedPosition = Position;
            moveDistance = Height + 8;

            // Pick tint based on which requirement is set
            if (requiredBossDefeats > 0 && requiredEnemyDefeats > 0)
                gateColor = Calc.HexToColor("ff6600"); // orange â€“ both
            else if (requiredBossDefeats > 0)
                gateColor = Calc.HexToColor("cc0000"); // red â€“ bosses
            else
                gateColor = Calc.HexToColor("3366ff"); // blue â€“ enemies

            Depth = -9000;
        }

        #endregion

        #region Lifecycle

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();

            // Load textures safely
            texture = AtlasPathHelper.TryGetTexture("objects/DZ/DZ/defeatSwitchGate")
                ?? GFX.Game["objects/DZ/DZ/switchgate/icon00"];

            iconTexture = AtlasPathHelper.TryGetTexture("objects/DZ/DZ/defeatSwitchGateIcon");

            // If persistent and the flag is already set, open immediately
            if (persistent && !string.IsNullOrEmpty(flag) && level.Session.GetFlag(flag))
            {
                opened = true;
                openProgress = 1f;
                MoveTo(closedPosition + new Vector2(0, -moveDistance));
                Collidable = false;
            }
        }

        public override void Update()
        {
            base.Update();

            bool shouldBeOpen = CheckDefeatConditions();

            if (shouldBeOpen && !opened)
            {
                opened = true;
                if (!string.IsNullOrEmpty(flag))
                    level.Session.SetFlag(flag, true);
                Add(new Coroutine(OpenRoutine()));
            }

            // Icon pulse
            iconLerp = Calc.Approach(iconLerp, opened ? 1f : 0f, Engine.DeltaTime * 4f);
        }

        #endregion

        #region Logic

        private bool CheckDefeatConditions()
        {
            // If already opened via persistent flag, stay open
            if (opened) return true;

            int currentEnemyDefeats;
            int currentBossDefeats;

            if (useGlobalCounts)
            {
                var saveData = DZModule.SaveData;
                currentEnemyDefeats = saveData?.TotalEnemiesDefeated ?? 0;
                currentBossDefeats = saveData?.TotalBossesDefeated ?? 0;
            }
            else
            {
                var session = DZModule.Session;
                currentEnemyDefeats = session?.EnemiesDefeated ?? 0;
                currentBossDefeats = session?.BossesDefeated ?? 0;
            }

            bool enemyConditionMet = requiredEnemyDefeats <= 0 || currentEnemyDefeats >= requiredEnemyDefeats;
            bool bossConditionMet = requiredBossDefeats <= 0 || currentBossDefeats >= requiredBossDefeats;

            return enemyConditionMet && bossConditionMet;
        }

        #endregion

        #region Coroutines

        private IEnumerator OpenRoutine()
        {
            Audio.Play("event:/game/general/touchswitch_gate_open", Position);

            float duration = 0.8f;
            float elapsed = 0f;
            float startProgress = openProgress;

            while (elapsed < duration)
            {
                elapsed += Engine.DeltaTime;
                openProgress = Calc.LerpClamp(startProgress, 1f, Ease.CubeOut(elapsed / duration));
                MoveTo(closedPosition + new Vector2(0, -moveDistance * openProgress));
                yield return null;
            }

            openProgress = 1f;
            Collidable = false;

            Audio.Play("event:/game/general/touchswitch_gate_finish", Position);

            // Particle burst to celebrate
            if (level != null)
            {
                for (int i = 0; i < 16; i++)
                {
                    level.Particles.Emit(
                        SwitchGate.P_Behind,
                        Center + Calc.AngleToVector(Calc.Random.NextFloat() * MathHelper.TwoPi, 12f),
                        Calc.Random.NextFloat() * MathHelper.TwoPi
                    );
                }
            }
        }

        #endregion

        #region Rendering

        public override void Render()
        {
            // Draw gate body
            float w = Width;
            float h = Height;
            int texW = texture.Width;
            int texH = texture.Height;

            for (int x = 0; x < (int)w; x += texW)
            {
                for (int y = 0; y < (int)h; y += texH)
                {
                    Color tint = Color.Lerp(gateColor, Color.White, iconLerp * 0.4f);
                    texture.Draw(Position + new Vector2(x, y), Vector2.Zero, tint);
                }
            }

            // Draw icon in the center
            if (iconTexture != null)
            {
                float scale = 1f + (float)Math.Sin(Scene.TimeActive * 3f) * 0.05f;
                Color iconColor = Color.Lerp(Color.White, Color.LightGoldenrodYellow, iconLerp);
                iconTexture.DrawCentered(Center, iconColor, scale);
            }

            // Draw required-count text
            string display = BuildDisplayText();
            if (!string.IsNullOrEmpty(display) && !opened)
            {
                ActiveFont.DrawOutline(
                    display,
                    Center + new Vector2(0, -2f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * 0.5f,
                    Color.White,
                    2f,
                    Color.Black
                );
            }
        }

        private string BuildDisplayText()
        {
            int currentEnemies, currentBosses;

            if (useGlobalCounts)
            {
                var saveData = DZModule.SaveData;
                currentEnemies = saveData?.TotalEnemiesDefeated ?? 0;
                currentBosses = saveData?.TotalBossesDefeated ?? 0;
            }
            else
            {
                var session = DZModule.Session;
                currentEnemies = session?.EnemiesDefeated ?? 0;
                currentBosses = session?.BossesDefeated ?? 0;
            }

            var parts = new List<string>();

            if (requiredEnemyDefeats > 0)
                parts.Add($"E:{Math.Min(currentEnemies, requiredEnemyDefeats)}/{requiredEnemyDefeats}");

            if (requiredBossDefeats > 0)
                parts.Add($"B:{Math.Min(currentBosses, requiredBossDefeats)}/{requiredBossDefeats}");

            return string.Join(" ", parts);
        }

        #endregion
    }
}

