using System;
using System.Collections.Generic;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    /// <summary>
    /// Debug HUD for displaying all multiplayer players' health and info.
    /// Shows in top-left corner for testing CelesteNet health sync.
    /// </summary>
    public class MultiplayerDebugHUD : Entity
    {
        private class PlayerInfo
        {
            public string Name;
            public int CurrentHealth;
            public int MaxHealth;
            public bool IsDead;
            public bool IsKirbyMode;
            public float Stamina;
        }

        private Dictionary<int, PlayerInfo> playerInfos = new Dictionary<int, PlayerInfo>();
        private float updateTimer = 0f;
        private const float UPDATE_INTERVAL = 0.5f;

        public MultiplayerDebugHUD()
        {
            Visible = true;
            Tag = Tags.HUD | Tags.Global;
            Depth = -10000; // Render on top of everything
        }

        public override void Update()
        {
            base.Update();

            updateTimer += Engine.DeltaTime;
            if (updateTimer >= UPDATE_INTERVAL)
            {
                updateTimer = 0f;
                UpdatePlayerInfos();
            }
        }

        public override void Render()
        {
            if (!Visible || !CelesteNetIntegration.IsMultiplayerSession())
                return;

            Vector2 position = new Vector2(20f, 20f);
            float lineHeight = 20f;
            Color bgColor = Color.Black * 0.7f;
            Color textColor = Color.White;
            Color kirbyColor = Color.LightGreen;
            Color madelineColor = Color.LightBlue;

            // Draw background
            float height = 40f + (playerInfos.Count * lineHeight);
            Draw.Rect(position.X - 10f, position.Y - 10f, 250f, height, bgColor);

            // Draw header
            ActiveFont.DrawOutline("Multiplayer Debug HUD", position, Vector2.Zero, Vector2.One, 
                Color.Yellow, 2f, Color.Black * 0.7f);
            position.Y += lineHeight;

            // Draw each player's info
            int index = 0;
            foreach (var kvp in playerInfos)
            {
                PlayerInfo info = kvp.Value;
                string playerType = info.IsKirbyMode ? "[KIRBY]" : "[MAD]";
                Color typeColor = info.IsKirbyMode ? kirbyColor : madelineColor;
                
                string status = info.IsDead ? "DEAD" : $"{info.CurrentHealth}/{info.MaxHealth} HP";
                string stamina = $"Stamina: {info.Stamina:F1}";
                
                // Player name and type
                string line1 = $"{info.Name} {playerType}";
                ActiveFont.DrawOutline(line1, position, Vector2.Zero, Vector2.One, typeColor, 
                    2f, Color.Black * 0.7f);
                position.Y += 12f;

                // Health status
                string line2 = $"  {status}";
                ActiveFont.DrawOutline(line2, position, Vector2.Zero, Vector2.One, textColor, 
                    2f, Color.Black * 0.7f);
                position.Y += 12f;

                // Stamina
                string line3 = $"  {stamina}";
                ActiveFont.DrawOutline(line3, position, Vector2.Zero, Vector2.One, textColor, 
                    2f, Color.Black * 0.7f);
                position.Y += lineHeight + 5f;

                index++;
            }

            // Draw local player info
            Player localPlayer = Engine.Scene?.Tracker.GetEntity<Player>();
            var localHealthManager = PlayerHealthManager.Instance;
            if (localHealthManager != null && localHealthManager.IsKirbyMode)
            {
                position.Y += 10f;
                ActiveFont.DrawOutline("LOCAL PLAYER:", position, Vector2.Zero, Vector2.One, 
                    Color.Orange, 2f, Color.Black * 0.7f);
                position.Y += 15f;
                
                string localInfo = $"HP: {localHealthManager.CurrentHP}/{localHealthManager.MaxHP} | Stamina: {localPlayer?.Stamina ?? 0f:F1}";
                ActiveFont.DrawOutline(localInfo, position, Vector2.Zero, Vector2.One, textColor, 
                    2f, Color.Black * 0.7f);
            }
        }

        private void UpdatePlayerInfos()
        {
            // Get the local player from the scene
            Player localPlayer = Engine.Scene?.Tracker.GetEntity<Player>();
            
            // TODO: Get actual player info from CelesteNet
            // For now, add a placeholder entry for testing
            if (playerInfos.Count == 0)
            {
                // Add local player as placeholder
                var healthManager = PlayerHealthManager.Instance;
                if (healthManager != null && healthManager.IsKirbyMode)
                {
                    playerInfos[0] = new PlayerInfo
                    {
                        Name = "Local Player",
                        CurrentHealth = healthManager.CurrentHP,
                        MaxHealth = healthManager.MaxHP,
                        IsDead = healthManager.IsDead,
                        IsKirbyMode = true,
                        Stamina = localPlayer?.Stamina ?? 110f
                    };
                }
            }
            else
            {
                // Update local player info
                var healthManager = PlayerHealthManager.Instance;
                if (healthManager != null && healthManager.IsKirbyMode && playerInfos.ContainsKey(0))
                {
                    playerInfos[0].CurrentHealth = healthManager.CurrentHP;
                    playerInfos[0].MaxHealth = healthManager.MaxHP;
                    playerInfos[0].IsDead = healthManager.IsDead;
                    playerInfos[0].IsKirbyMode = true;
                    playerInfos[0].Stamina = localPlayer?.Stamina ?? 110f;
                }
            }
        }

        /// <summary>
        /// Update or add a remote player's info.
        /// </summary>
        public void UpdateRemotePlayer(int playerId, string name, int currentHealth, int maxHealth, 
            bool isDead, bool isKirbyMode, float stamina)
        {
            if (!playerInfos.ContainsKey(playerId))
            {
                playerInfos[playerId] = new PlayerInfo();
            }

            var info = playerInfos[playerId];
            info.Name = name;
            info.CurrentHealth = currentHealth;
            info.MaxHealth = maxHealth;
            info.IsDead = isDead;
            info.IsKirbyMode = isKirbyMode;
            info.Stamina = stamina;
        }

        /// <summary>
        /// Remove a player from the debug display.
        /// </summary>
        public void RemovePlayer(int playerId)
        {
            if (playerId != 0) // Don't remove local player
            {
                playerInfos.Remove(playerId);
            }
        }

        /// <summary>
        /// Clear all remote players (keep local).
        /// </summary>
        public void ClearRemotePlayers()
        {
            var localInfo = playerInfos.ContainsKey(0) ? playerInfos[0] : null;
            playerInfos.Clear();
            if (localInfo != null)
            {
                playerInfos[0] = localInfo;
            }
        }
    }
}
