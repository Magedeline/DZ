using System;
using System.Collections.Generic;
using MaggyHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Entities
{
    /// <summary>
    /// Dangerous tentacle from the void
    /// Can be phased through with a well-timed dash
    /// </summary>
    public class VoidTendril : Entity
    {
        private int damage;
        private float swaySpeed;
        private bool phaseableOnDash;
        private float phaseWindow;
        
        private float swayTimer = 0f;
        private float swayOffset = 0f;
        private List<Vector2> nodes;
        
        private bool playerPhasing = false;
        private float phaseTimer = 0f;
        
        private Color tendrilColor;
        private Color coreColor;
        
        public VoidTendril(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            damage = data.Int("damage", 1);
            swaySpeed = data.Float("swaySpeed", 1.0f);
            phaseableOnDash = data.Bool("phaseableOnDash", true);
            phaseWindow = data.Float("phaseWindow", 0.3f);
            
            nodes = new List<Vector2>();
            nodes.Add(Position);
            foreach (var node in data.Nodes)
            {
                nodes.Add(node + offset);
            }
            
            Collider = new Hitbox(8f, 48f, -4f, 0f);
            Add(new PlayerCollider(OnPlayer));
            
            tendrilColor = new Color(80, 0, 80);
            coreColor = new Color(160, 0, 160);
            
            Depth = 100;
        }
        
        public override void Update()
        {
            base.Update();
            
            swayTimer += Engine.DeltaTime * swaySpeed;
            swayOffset = (float)Math.Sin(swayTimer) * 8f;
            
            // Update collision position with sway
            Collider.Position = new Vector2(-4f + swayOffset, 0f);
            
            if (playerPhasing)
            {
                phaseTimer -= Engine.DeltaTime;
                if (phaseTimer <= 0f)
                {
                    playerPhasing = false;
                }
            }
            
            // Check for player dash to start phasing
            if (phaseableOnDash && !playerPhasing)
            {
                Player player = Scene?.Tracker.GetEntity<Player>();
                if (player != null && player.StateMachine.State == Player.StDash)
                {
                    float distSq = Vector2.DistanceSquared(player.Center, Center);
                    if (distSq < 50 * 50)
                    {
                        StartPhasing();
                    }
                }
            }
        }
        
        private void StartPhasing()
        {
            playerPhasing = true;
            phaseTimer = phaseWindow;
            
            Audio.Play("event:/game/general/fallblock_shake", Position);
        }
        
        private void OnPlayer(Player player)
        {
            if (playerPhasing)
            {
                // Player successfully phased through
                return;
            }
            
            // Deal damage
            player.Die(Vector2.UnitX * (player.X < X ? -1f : 1f));
        }
        
        public override void Render()
        {
            float alpha = playerPhasing ? 0.3f : 1f;
            
            // Draw tendril body
            float height = 48f;
            for (float y = 0; y < height; y += 4f)
            {
                float width = 8f - (y / height) * 4f; // Tapers toward top
                float xOffset = swayOffset * (y / height); // More sway at top
                
                Color color = Color.Lerp(tendrilColor, coreColor, y / height) * alpha;
                Draw.Rect(X - width / 2f + xOffset, Y + y, width, 4f, color);
            }
            
            // Draw pulsing core
            float pulse = 0.7f + (float)Math.Sin(swayTimer * 3f) * 0.3f;
            Draw.Circle(Position + new Vector2(swayOffset * 0.5f, height * 0.3f), 4f * pulse, coreColor * alpha, 8);
            
            // Draw phase indicator if phaseable
            if (phaseableOnDash && !playerPhasing)
            {
                float indicatorAlpha = 0.3f + (float)Math.Sin(swayTimer * 2f) * 0.2f;
                Draw.Circle(Position + new Vector2(0, height * 0.5f), 16f, Color.Cyan * indicatorAlpha, 16);
            }
        }
    }
}
