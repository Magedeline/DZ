using System;
using System.Collections.Generic;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    /// <summary>
    /// Custom health bar UI for CelesteNet multiplayer.
    /// Shows above players on hover with different info for Kirby vs Madeline.
    /// </summary>
    public class MultiplayerHealthBar : Entity
    {
        private Player targetPlayer;
        private bool isKirbyMode;
        private string playerName;
        private float stamina;
        private float hoverTimer;
        private const float HOVER_DELAY = 0.3f;
        private const float FADE_DURATION = 0.2f;
        private float alpha = 0f;

        // Layout constants
        private const float WIDTH = 100f;
        private const float HEIGHT = 24f;
        private const float BAR_HEIGHT = 6f;
        private const float OFFSET_Y = -20f;

        public MultiplayerHealthBar(Player player)
            : base(player.Position)
        {
            targetPlayer = player;
            Depth = -100;
            Tag = Tags.HUD;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            UpdatePlayerInfo();
        }

        public override void Update()
        {
            base.Update();

            if (targetPlayer == null || targetPlayer.Scene == null)
            {
                RemoveSelf();
                return;
            }

            // Update position to follow player
            Position = targetPlayer.Position + new Vector2(0, OFFSET_Y);

            // Check if mouse is hovering near player
            Vector2 mousePos = MInput.Mouse.Position;
            Vector2 screenPos = Position;
            float distance = Vector2.Distance(mousePos, screenPos);

            if (distance < 50f)
            {
                // Mouse is hovering
                if (hoverTimer < HOVER_DELAY)
                    hoverTimer += Engine.DeltaTime;
            }
            else
            {
                // Mouse moved away
                hoverTimer = Math.Max(0, hoverTimer - Engine.DeltaTime);
            }

            // Update alpha based on hover state
            float targetAlpha = hoverTimer >= HOVER_DELAY ? 1f : 0f;
            alpha = Calc.Approach(alpha, targetAlpha, Engine.DeltaTime / FADE_DURATION);

            // Update player info periodically
            if (Engine.Scene.OnInterval(0.1f))
            {
                UpdatePlayerInfo();
            }
        }

        public override void Render()
        {
            if (alpha <= 0.01f || targetPlayer == null)
                return;

            Vector2 drawPos = Position;
            Color bgColor = Color.Black * (alpha * 0.8f);
            Color textColor = Color.White * alpha;
            Color barColor = Color.Lerp(Color.Red, Color.Green, 0.5f) * alpha;

            // Draw background
            Draw.Rect(drawPos.X - WIDTH / 2, drawPos.Y - HEIGHT / 2, WIDTH, HEIGHT, bgColor);

            // Draw player name
            string nameText = playerName ?? "Player";
            Vector2 nameSize = ActiveFont.Measure(nameText);
            ActiveFont.DrawOutline(
                nameText,
                new Vector2(drawPos.X - nameSize.X / 2, drawPos.Y - HEIGHT / 2 + 2),
                Vector2.Zero,
                Vector2.One,
                textColor,
                2f,
                Color.Black * alpha
            );

            // Draw stamina (for both Kirby and Madeline)
            string staminaText = $"Stamina: {stamina:F1}";
            Vector2 staminaSize = ActiveFont.Measure(staminaText);
            ActiveFont.DrawOutline(
                staminaText,
                new Vector2(drawPos.X - staminaSize.X / 2, drawPos.Y + HEIGHT / 2 - staminaSize.Y - 2),
                Vector2.Zero,
                Vector2.One,
                textColor,
                2f,
                Color.Black * alpha
            );

            // Draw health bar only for Kirby mode
            if (isKirbyMode)
            {
                var controller = KirbyHealthController.Instance;
                if (controller != null && controller.IsEnabled)
                {
                    float healthPercent = controller.HealthPercent;
                    float barY = drawPos.Y + HEIGHT / 2 - BAR_HEIGHT - staminaSize.Y - 4;

                    // Draw health bar background
                    Draw.Rect(
                        drawPos.X - WIDTH / 2 + 5,
                        barY,
                        WIDTH - 10,
                        BAR_HEIGHT,
                        Color.DarkGray * alpha
                    );

                    // Draw health bar fill
                    Draw.Rect(
                        drawPos.X - WIDTH / 2 + 5,
                        barY,
                        (WIDTH - 10) * healthPercent,
                        BAR_HEIGHT,
                        barColor
                    );

                    // Draw health text
                    string healthText = $"{controller.CurrentHealth}/{controller.MaxHealth}";
                    Vector2 healthTextSize = ActiveFont.Measure(healthText);
                    ActiveFont.DrawOutline(
                        healthText,
                        new Vector2(drawPos.X - healthTextSize.X / 2, barY - healthTextSize.Y - 2),
                        Vector2.Zero,
                        Vector2.One,
                        textColor,
                        2f,
                        Color.Black * alpha
                    );
                }
            }
        }

        private void UpdatePlayerInfo()
        {
            if (targetPlayer == null)
                return;

            // Check if player is in Kirby mode
            var controller = KirbyHealthController.Instance;
            isKirbyMode = controller != null && controller.IsEnabled;

            // Get player name (from CelesteNet if available)
            playerName = GetPlayerName(targetPlayer);

            // Get stamina (from player state)
            stamina = targetPlayer.Stamina;
        }

        private string GetPlayerName(Player player)
        {
            // TODO: Get actual player name from CelesteNet
            // For now, use a generic name
            return "Player";
        }

        /// <summary>
        /// Update the target player for this health bar.
        /// </summary>
        public void SetTargetPlayer(Player player)
        {
            if (targetPlayer != player)
            {
                targetPlayer = player;
                UpdatePlayerInfo();
                hoverTimer = 0f; // Reset hover when switching targets
            }
        }

        /// <summary>
        /// Create or update health bar for a player.
        /// </summary>
        public static MultiplayerHealthBar GetOrCreateForPlayer(Scene scene, Player player)
        {
            // Look for existing health bar for this player
            foreach (var entity in scene.Entities)
            {
                if (entity is MultiplayerHealthBar bar && bar.targetPlayer == player)
                {
                    return bar;
                }
            }

            // Create new health bar
            var newBar = new MultiplayerHealthBar(player);
            scene.Add(newBar);
            return newBar;
        }
    }
}
