using System;
using System.Collections;
using Celeste.Extensions;
using DZ;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Void Gate - Closes when player enters center, trapping them inside
    /// Opens after defeating required number of enemies
    /// Two gates close from left and right sides
    /// </summary>
    [CustomEntity(ids: "DZ/VoidGate")]
    [Tracked]
    [HotReloadable]
    public class VoidGate : Entity
    {
        #region Fields
        
        // Gate components
        private Solid leftGate;
        private Solid rightGate;
        
        // Gate positions
        private float gateWidth;
        private float gateHeight;
        private Vector2 leftClosedPosition;
        private Vector2 leftOpenPosition;
        private Vector2 rightClosedPosition;
        private Vector2 rightOpenPosition;
        
        // Gate state
        private bool isClosed;
        private bool isOpening;
        private bool isClosing;
        private float moveSpeed;
        
        // Trigger zone (center area)
        private Rectangle triggerZone;
        private bool playerInTrigger;
        
        // Arena reference
        private VoidGateArena arena;
        
        // Visual
        private Sprite leftGateSprite;
        private Sprite rightGateSprite;
        private VertexLight leftLight;
        private VertexLight rightLight;

        // Audio
        private SoundSource gateSound;
        
        // Level reference
        private Level level;
        
        #endregion
        
        #region Constructor
        
        public VoidGate(EntityData data, Vector2 offset) 
            : base(data.Position + offset)
        {
            gateWidth = data.Float("gateWidth", 16f);
            gateHeight = data.Float("gateHeight", 128f);
            moveSpeed = data.Float("moveSpeed", 100f);
            
            float triggerWidth = data.Float("triggerWidth", 96f);
            float triggerHeight = data.Float("triggerHeight", 128f);
            
            // Setup gate positions
            float centerX = data.Width / 2f;
            leftOpenPosition = Position + new Vector2(-gateWidth, 0);
            leftClosedPosition = Position + new Vector2(0, 0);
            rightOpenPosition = Position + new Vector2(data.Width, 0);
            rightClosedPosition = Position + new Vector2(data.Width - gateWidth, 0);
            
            // Setup trigger zone (center area)
            triggerZone = new Rectangle(
                (int)(Position.X + centerX - triggerWidth / 2),
                (int)Position.Y,
                (int)triggerWidth,
                (int)triggerHeight
            );
            
            SetupGates();
            SetupSprites();
            SetupLights();
            SetupAudio();
            
            Depth = -10000;
        }
        
        #endregion
        
        #region Setup
        
        private void SetupGates()
        {
            // Left gate
            leftGate = new Solid(leftOpenPosition, gateWidth, gateHeight, safe: false);
            leftGate.Depth = -9000;
            
            // Right gate
            rightGate = new Solid(rightOpenPosition, gateWidth, gateHeight, safe: false);
            rightGate.Depth = -9000;
        }
        
        private void SetupSprites()
        {
            // Left gate sprite
            leftGateSprite = AtlasPathHelper.CreateSprite("objects/DZ/DZ/voidgate/");
            leftGateSprite.AddLoop("idle", "gate_left", 0.1f);
            leftGateSprite.Add("close", "gate_left_close", 0.08f, "idle");
            leftGateSprite.Add("open", "gate_left_open", 0.08f, "idle");
            leftGateSprite.Play("idle");
            leftGateSprite.Position = Vector2.Zero;
            
            // Right gate sprite
            rightGateSprite = AtlasPathHelper.CreateSprite("objects/DZ/DZ/voidgate/");
            rightGateSprite.AddLoop("idle", "gate_right", 0.1f);
            rightGateSprite.Add("close", "gate_right_close", 0.08f, "idle");
            rightGateSprite.Add("open", "gate_right_open", 0.08f, "idle");
            rightGateSprite.Play("idle");
            rightGateSprite.Position = Vector2.Zero;
        }
        
        private void SetupLights()
        {
            leftLight = new VertexLight(Color.Purple * 0.8f, 1f, 32, 64);
            rightLight = new VertexLight(Color.Purple * 0.8f, 1f, 32, 64);
        }
        
        private void SetupAudio()
        {
            gateSound = new SoundSource();
            Add(gateSound);
        }
        
        #endregion
        
        #region Lifecycle
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            
            // Add gates to scene
            scene.Add(leftGate);
            scene.Add(rightGate);
            
            // Add lights to gates
            leftGate.Add(leftLight);
            rightGate.Add(rightLight);
            
            // Add sprites to gates
            leftGate.Add(leftGateSprite);
            rightGate.Add(rightGateSprite);
            
            // Find arena
            arena = scene.Tracker.GetEntity<VoidGateArena>();
            if (arena != null)
            {
                arena.RegisterGate(this);
            }
        }
        
        public override void Update()
        {
            base.Update();
            
            // Check if player is in trigger zone
            CheckPlayerInTrigger();
            
            // Handle gate closing/opening
            if (isClosing)
            {
                UpdateClosing();
            }
            else if (isOpening)
            {
                UpdateOpening();
            }
            
            // Emit particles when closed
            if (isClosed && Scene.OnInterval(0.1f))
            {
                EmitVoidParticles();
            }
        }
        
        #endregion
        
        #region Player Detection
        
        private void CheckPlayerInTrigger()
        {
            if (isClosed || isClosing || isOpening)
                return;
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player == null)
                return;
            
            // Check if player (or Kirby player) is in trigger zone
            bool inZone = triggerZone.Contains((int)player.X, (int)player.Y);
            bool isKirby = IsKirbyMode(player);

            if (isKirby)
            {
                // Kirby players may have different behavior/colliders; the same position check is used for now.
            }
            
            if (inZone && !playerInTrigger)
            {
                playerInTrigger = true;
                StartClosing();
            }
        }

        private static bool IsKirbyMode(global::Celeste.Player player)
        {
            return player?.IsKirbyMode() == true;
        }
        
        #endregion
        
        #region Gate Control
        
        public void StartClosing()
        {
            if (isClosed || isClosing)
                return;
            
            isClosing = true;
            leftGateSprite.Play("close");
            rightGateSprite.Play("close");
            
            Audio.Play("event:/game/03_resort/forcefield_activate", Position);
            gateSound.Play("event:/env/local/09_core/fireballs_idle");
            
            level?.Shake(0.5f);
            
            // Notify arena
            if (arena != null)
            {
                arena.OnGateClosed();
            }
            
            // Emit closing particles
            for (int i = 0; i < 30; i++)
            {
                EmitVoidParticles();
            }
        }
        
        public void StartOpening()
        {
            if (!isClosed || isOpening)
                return;
            
            isOpening = true;
            isClosing = false;
            leftGateSprite.Play("open");
            rightGateSprite.Play("open");
            
            Audio.Play("event:/game/03_resort/forcefield_deactivate", Position);
            gateSound.Stop();
            
            level?.Shake(0.5f);
            level?.Flash(Color.Purple);
            
            // Notify arena
            if (arena != null)
            {
                arena.OnGateOpened();
            }
            
            // Emit opening particles
            for (int i = 0; i < 50; i++)
            {
                EmitVoidParticles();
            }
        }
        
        private void UpdateClosing()
        {
            // Move gates toward closed position
            float moveAmount = moveSpeed * Engine.DeltaTime;
            
            // Move left gate right
            if (leftGate.X < leftClosedPosition.X)
            {
                float newX = Math.Min(leftGate.X + moveAmount, leftClosedPosition.X);
                leftGate.MoveTo(new Vector2(newX, leftGate.Y));
            }
            
            // Move right gate left
            if (rightGate.X > rightClosedPosition.X)
            {
                float newX = Math.Max(rightGate.X - moveAmount, rightClosedPosition.X);
                rightGate.MoveTo(new Vector2(newX, rightGate.Y));
            }
            
            // Check if fully closed
            if (leftGate.X >= leftClosedPosition.X && rightGate.X <= rightClosedPosition.X)
            {
                isClosed = true;
                isClosing = false;
                
                // Rumble effect
                level?.DirectionalShake(Vector2.UnitX, 0.3f);
                Audio.Play("event:/game/03_resort/forcefield_bump", Position);
            }
        }
        
        private void UpdateOpening()
        {
            // Move gates toward open position
            float moveAmount = moveSpeed * Engine.DeltaTime;
            
            // Move left gate left
            if (leftGate.X > leftOpenPosition.X)
            {
                float newX = Math.Max(leftGate.X - moveAmount, leftOpenPosition.X);
                leftGate.MoveTo(new Vector2(newX, leftGate.Y));
            }
            
            // Move right gate right
            if (rightGate.X < rightOpenPosition.X)
            {
                float newX = Math.Min(rightGate.X + moveAmount, rightOpenPosition.X);
                rightGate.MoveTo(new Vector2(newX, rightGate.Y));
            }
            
            // Check if fully open
            if (leftGate.X <= leftOpenPosition.X && rightGate.X >= rightOpenPosition.X)
            {
                isClosed = false;
                isOpening = false;
                playerInTrigger = false;
                
                // Rumble effect
                level?.DirectionalShake(Vector2.UnitX, 0.3f);
            }
        }
        
        #endregion
        
        #region Properties
        
        public bool IsClosed => isClosed;
        public bool IsFullyClosed => isClosed && !isClosing && !isOpening;
        
        #endregion
        
        #region Particles
        
        private void EmitVoidParticles()
        {
            if (level == null)
                return;
            
            var particle = new ParticleType
            {
                Color = Color.Purple,
                Color2 = Color.DarkViolet,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SizeRange = 0.5f,
                LifeMin = 0.5f,
                LifeMax = 1.2f,
                SpeedMin = 10f,
                SpeedMax = 30f,
                DirectionRange = (float)Math.PI * 2f
            };
            
            // Emit from left gate
            Vector2 leftEmitPos = leftGate.Position + new Vector2(gateWidth, Calc.Random.Range(0f, gateHeight));
            level.ParticlesFG.Emit(particle, leftEmitPos);
            
            // Emit from right gate
            Vector2 rightEmitPos = rightGate.Position + new Vector2(0, Calc.Random.Range(0f, gateHeight));
            level.ParticlesFG.Emit(particle, rightEmitPos);
        }
        
        #endregion
        
        #region Rendering
        
        public override void Render()
        {
            base.Render();
            
            // Draw trigger zone in debug mode
            if (Engine.Commands != null && Engine.Commands.Open)
            {
                Draw.HollowRect(triggerZone, Color.Yellow * 0.5f);
            }
            
            // Draw connection between gates when closed
            if (isClosed || isClosing)
            {
                Vector2 leftEdge = leftGate.Position + new Vector2(gateWidth, gateHeight / 2);
                Vector2 rightEdge = rightGate.Position + new Vector2(0, gateHeight / 2);
                
                // Draw energy barrier
                for (int i = 0; i < 5; i++)
                {
                    float t = i / 5f;
                    Vector2 pos = Vector2.Lerp(leftEdge, rightEdge, t);
                    float alpha = (float)Math.Sin(Scene.TimeActive * 2f + t * (float)Math.PI) * 0.5f + 0.5f;
                    Draw.Circle(pos, 4f, Color.Purple * alpha, 8);
                }
            }
        }
        
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            
            // Draw gate bounds
            Draw.HollowRect(leftGate.Collider, Color.Red);
            Draw.HollowRect(rightGate.Collider, Color.Blue);
            Draw.HollowRect(triggerZone, Color.Yellow);
        }
        
        #endregion
        
        #region Cleanup
        
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            
            // Remove gates from scene
            leftGate?.RemoveSelf();
            rightGate?.RemoveSelf();
        }
        
        #endregion
    }
}

