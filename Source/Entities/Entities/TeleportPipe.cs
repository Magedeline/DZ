namespace Celeste.Entities
{
    /// <summary>
    /// Mario-style pipe system that allows player to enter and exit across rooms
    /// </summary>
    [CustomEntity(ids: "DZ/TeleportPipe")]
    [Tracked]
    public class TeleportPipe : Entity
    {
        public enum PipeDirection
        {
            Up,
            Down,
            Left,
            Right
        }

        public enum PipeType
        {
            Entry,    // Can enter
            Exit,     // Can exit  
            Both      // Can enter and exit
        }

        private static Dictionary<string, TeleportPipeData> globalPipeRegistry = new Dictionary<string, TeleportPipeData>();

        private Sprite sprite;
        private VertexLight light;
        private SoundSource pipeSfx;
        private TalkComponent talkComponent;
        private Wiggler entranceWiggler;
        private Level level;
        
        // Configuration
        private PipeDirection direction;
        private PipeType pipeType;
        private string targetPipeId;
        private string targetRoom;
        private Vector2 targetPosition;
        private string pipeId;
        private bool autoEnter;
        private float enterDelay;
        private bool isPlayerInside = false;
        private bool isTransporting = false;
        
        // Visual effects
        private List<Vector2> warpParticles = new List<Vector2>();
        private Color pipeColor;

        public string PipeId => pipeId;
        public bool CanEnter => pipeType == PipeType.Entry || pipeType == PipeType.Both;
        public bool CanExit => pipeType == PipeType.Exit || pipeType == PipeType.Both;

        public TeleportPipe(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            pipeId = data.Attr("pipeId", System.Guid.NewGuid().ToString());
            targetPipeId = data.Attr("targetPipeId", "");
            targetRoom = data.Attr("targetRoom", "");
            targetPosition = new Vector2(data.Float("targetX", 0f), data.Float("targetY", 0f));
            
            direction = data.Enum<PipeDirection>("direction", PipeDirection.Down);
            pipeType = data.Enum<PipeType>("pipeType", PipeType.Both);
            autoEnter = data.Bool("autoEnter", false);
            enterDelay = data.Float("enterDelay", 0.5f);
            
            string colorHex = data.Attr("pipeColor", "228B22");
            if (TryParseColor(colorHex, out Color color))
            {
                pipeColor = color;
            }
            else
            {
                pipeColor = Color.ForestGreen;
            }

            Depth = -100;
            SetupCollider();
            SetupComponents();
        }

        private void SetupCollider()
        {
            switch (direction)
            {
                case PipeDirection.Up:
                case PipeDirection.Down:
                    Collider = new Hitbox(24f, 32f, -12f, -32f);
                    break;
                case PipeDirection.Left:
                case PipeDirection.Right:
                    Collider = new Hitbox(32f, 24f, -32f, -12f);
                    break;
            }
        }

        private void SetupComponents()
        {
            // Sprite setup
            sprite = new Sprite(GFX.Game, "objects/DZ/DZ/Ingeste/teleportPipe/");
            string animPrefix = direction.ToString().ToLower();
            sprite.AddLoop("idle", $"{animPrefix}_idle", 0.1f);
            sprite.AddLoop("active", $"{animPrefix}_active", 0.05f);
            sprite.AddLoop("transporting", $"{animPrefix}_transport", 0.02f);
            sprite.Play("idle");
            sprite.Color = pipeColor;
            Add(sprite);

            // Lighting
            light = new VertexLight(pipeColor, 1f, 48, 32);
            Add(light);

            // Sound
            pipeSfx = new SoundSource();
            Add(pipeSfx);

            // Talk component for manual entry
            if (!autoEnter && CanEnter)
            {
                Vector2 talkOffset = GetTalkOffset();
                talkComponent = new TalkComponent(
                    new Rectangle((int)talkOffset.X - 16, (int)talkOffset.Y - 16, 32, 32),
                    talkOffset,
                    OnTalkInteraction
                );
                Add(talkComponent);
            }

            // Visual effects
            entranceWiggler = Wiggler.Create(0.4f, 4f, null, false, false);
            Add(entranceWiggler);
        }

        private Vector2 GetTalkOffset()
        {
            switch (direction)
            {
                case PipeDirection.Up: return new Vector2(0f, -40f);
                case PipeDirection.Down: return new Vector2(0f, 8f);
                case PipeDirection.Left: return new Vector2(-40f, 0f);
                case PipeDirection.Right: return new Vector2(40f, 0f);
                default: return Vector2.Zero;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            
            // Register this pipe in the global registry
            RegisterPipe();
        }

        private void RegisterPipe()
        {
            globalPipeRegistry[pipeId] = new TeleportPipeData
            {
                PipeId = pipeId,
                RoomName = level?.Session?.Level ?? "",
                Position = Position,
                Direction = direction,
                Type = pipeType
            };
        }

        public override void Update()
        {
            base.Update();

            if (isTransporting) return;

            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                if (autoEnter && CanEnter && CollideCheck(player) && !isPlayerInside)
                {
                    Add(new Coroutine(EnterPipeSequence(player)));
                }
            }

            UpdateVisualEffects();
        }

        private void OnTalkInteraction(global::Celeste.Player player)
        {
            if (CanEnter && !isPlayerInside && !isTransporting)
            {
                Add(new Coroutine(EnterPipeSequence(player)));
            }
        }

        private IEnumerator EnterPipeSequence(global::Celeste.Player player)
        {
            if (isTransporting) yield break;
            
            isTransporting = true;
            isPlayerInside = true;

            // Play enter sound
            pipeSfx.Play("event:/game/09_core/conveyor_activate");
            Audio.Play("event:/game/general/thing_booped", Position);

            // Visual feedback
            sprite.Play("active");
            entranceWiggler.Start();
            CreateEnterEffect();

            // Player entering animation
            player.StateMachine.State = global::Celeste.Player.StDummy;
            
            Vector2 enterTarget = GetEnterPosition();
            yield return MovePlayerToPosition(player, enterTarget, 1f);

            // Wait for enter delay
            yield return enterDelay;

            // Make player invisible
            player.Visible = false;
            sprite.Play("transporting");

            // Transport delay with effects
            yield return TransportEffect();

            // Find target pipe and transport
            var targetPipe = FindTargetPipe();
            if (targetPipe != null)
            {
                yield return TransportToTarget(player, targetPipe);
            }
            else if (!string.IsNullOrEmpty(targetRoom))
            {
                yield return TransportToRoom(player);
            }
            else
            {
                // Fallback: transport to specified position
                yield return TransportToPosition(player, targetPosition);
            }

            isTransporting = false;
        }

        private Vector2 GetEnterPosition()
        {
            switch (direction)
            {
                case PipeDirection.Up: return Position + new Vector2(0f, -16f);
                case PipeDirection.Down: return Position + new Vector2(0f, 16f);
                case PipeDirection.Left: return Position + new Vector2(-16f, 0f);
                case PipeDirection.Right: return Position + new Vector2(16f, 0f);
                default: return Position;
            }
        }

        private IEnumerator MovePlayerToPosition(global::Celeste.Player player, Vector2 target, float duration)
        {
            Vector2 start = player.Position;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Engine.DeltaTime;
                float progress = Ease.CubeInOut(timer / duration);
                player.Position = Vector2.Lerp(start, target, progress);
                yield return null;
            }

            player.Position = target;
        }

        private void CreateEnterEffect()
        {
            if (level == null) return;

            // Create swirling particles
            for (int i = 0; i < 16; i++)
            {
                float angle = i * 22.5f * Calc.DegToRad;
                Vector2 particlePos = Position + Calc.AngleToVector(angle, 20f);
                level.ParticlesFG.Emit(ParticleTypes.Dust, particlePos, pipeColor);
            }
        }

        private IEnumerator TransportEffect()
        {
            float timer = 0f;
            while (timer < 1f)
            {
                timer += Engine.DeltaTime;
                
                // Create transport particles
                if (Scene.OnInterval(0.1f))
                {
                    Vector2 particlePos = Position + new Vector2(
                        Calc.Random.Range(-12f, 12f),
                        Calc.Random.Range(-12f, 12f)
                    );
                    level?.ParticlesFG.Emit(ParticleTypes.Dust, particlePos, pipeColor);
                }
                
                yield return null;
            }
        }

        private TeleportPipe FindTargetPipe()
        {
            if (string.IsNullOrEmpty(targetPipeId)) return null;

            // First check current level
            var pipesInLevel = Scene.Tracker.GetEntities<TeleportPipe>();
            var targetInLevel = pipesInLevel.OfType<TeleportPipe>()
                .FirstOrDefault(p => p.PipeId == targetPipeId && p.CanExit);
            
            if (targetInLevel != null) return targetInLevel;

            // If not in current level, we'll need to change rooms
            return null;
        }

        private IEnumerator TransportToTarget(global::Celeste.Player player, TeleportPipe targetPipe)
        {
            yield return targetPipe.ExitPipeSequence(player);
        }

        private IEnumerator TransportToRoom(global::Celeste.Player player)
        {
            // Room transition would be handled by the game's level system
            // For now, just teleport to position
            player.Position = targetPosition;
            yield return ExitPipeSequence(player);
        }

        private IEnumerator TransportToPosition(global::Celeste.Player player, Vector2 position)
        {
            player.Position = position;
            yield return ExitPipeSequence(player);
        }

        public IEnumerator ExitPipeSequence(global::Celeste.Player player)
        {
            // Position player at exit
            Vector2 exitPosition = GetExitPosition();
            player.Position = exitPosition;
            player.Visible = true;

            // Play exit sound
            Audio.Play("event:/game/general/thing_booped", Position);
            
            // Visual effects
            sprite.Play("active");
            CreateExitEffect();

            // Move player out of pipe
            Vector2 finalPosition = Position + GetExitOffset();
            yield return MovePlayerToPosition(player, finalPosition, 0.8f);

            // Restore player control
            player.StateMachine.State = global::Celeste.Player.StNormal;
            sprite.Play("idle");
            isPlayerInside = false;
        }

        private Vector2 GetExitPosition()
        {
            switch (direction)
            {
                case PipeDirection.Up: return Position + new Vector2(0f, -16f);
                case PipeDirection.Down: return Position + new Vector2(0f, 16f);
                case PipeDirection.Left: return Position + new Vector2(-16f, 0f);
                case PipeDirection.Right: return Position + new Vector2(16f, 0f);
                default: return Position;
            }
        }

        private Vector2 GetExitOffset()
        {
            switch (direction)
            {
                case PipeDirection.Up: return new Vector2(0f, -32f);
                case PipeDirection.Down: return new Vector2(0f, 32f);
                case PipeDirection.Left: return new Vector2(-32f, 0f);
                case PipeDirection.Right: return new Vector2(32f, 0f);
                default: return Vector2.Zero;
            }
        }

        private void CreateExitEffect()
        {
            if (level == null) return;

            // Create exit particles
            for (int i = 0; i < 12; i++)
            {
                float angle = Calc.Random.NextFloat() * 360f * Calc.DegToRad;
                float distance = Calc.Random.Range(30f, 60f);
                Vector2 direction = Calc.AngleToVector(angle, distance);
                level.ParticlesFG.Emit(ParticleTypes.Dust, Position, angle);
            }
        }

        private void UpdateVisualEffects()
        {
            // Pulsing light
            light.Alpha = 0.7f + (float)System.Math.Sin(Scene.TimeActive * 3f) * 0.3f;
            
            // Scale effect
            sprite.Scale = Vector2.One * (1f + entranceWiggler.Value * 0.1f);
        }

        public override void Render()
        {
            base.Render();

            // Render warp effect when transporting
            if (isTransporting)
            {
                RenderWarpEffect();
            }
        }

        private void RenderWarpEffect()
        {
            float alpha = (float)System.Math.Sin(Scene.TimeActive * 10f) * 0.3f + 0.7f;
            Color warpColor = pipeColor * alpha;
            
            Draw.Circle(Position, 16f, warpColor, 4);
            Draw.Circle(Position, 12f, Color.White * alpha * 0.6f, 2);
        }

        private static bool TryParseColor(string hex, out Color color)
        {
            color = Color.White;
            if (string.IsNullOrEmpty(hex)) return false;
            
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);
            
            try
            {
                if (hex.Length == 6)
                {
                    int r = System.Convert.ToInt32(hex.Substring(0, 2), 16);
                    int g = System.Convert.ToInt32(hex.Substring(2, 2), 16);
                    int b = System.Convert.ToInt32(hex.Substring(4, 2), 16);
                    color = new Color(r, g, b);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "TeleportPipe", $"Failed to parse color hex '{hex}': {ex.Message}");
            }
            return false;
        }
    }

    /// <summary>
    /// Data structure for storing pipe information in session
    /// </summary>
    public partial class TeleportPipeData
    {
        public string PipeId { get; set; }
        public string RoomName { get; set; }
        public Vector2 Position { get; set; }
        public TeleportPipe.PipeDirection Direction { get; set; }
        public TeleportPipe.PipeType Type { get; set; }
    }
}

