namespace Celeste.Entities.Chapters.Ch10
{
    /// <summary>
    /// RuinsPuzzleSwitch - Pressure plate that requires specific movement patterns
    /// Can be configured for different puzzle types (step, hold, sequence)
    /// Controls connected BlockGates when activated
    /// Sprite path: objects/ruins_puzzle_switch/
    /// </summary>
    [CustomEntity("DZ/RuinsPuzzleSwitch")]
    [Tracked]
    public class RuinsPuzzleSwitch : Actor
    {
        #region Enums
        public enum SwitchType
        {
            Simple,         // Activate on step
            Hold,           // Must stay on switch
            Sequence,       // Part of a sequence
            Timed,          // Activates for limited time
            Dash           // Requires dash to activate
        }

        public enum SwitchState
        {
            Inactive,
            Active,
            Complete
        }
        #endregion

        #region Properties
        public SwitchType Type { get; private set; }
        public SwitchState State { get; private set; }
        public string GateId { get; private set; }
        public int SequenceOrder { get; private set; }
        public float HoldTime { get; private set; }
        public float TimerDuration { get; private set; }
        public bool IsActivated => State == SwitchState.Active || State == SwitchState.Complete;
        
        private Sprite sprite;
        private float holdTimer;
        private float activationTimer;
        private bool playerOnSwitch;
        private Player lastPlayer;
        private Level level;
        private List<BlockGate> connectedGates;
        private float glowAlpha;
        private SoundSource activationSound;
        #endregion

        #region Constructor
        public RuinsPuzzleSwitch(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Enum("switchType", SwitchType.Simple),
                data.Attr("gateId", ""),
                data.Int("sequenceOrder", 0),
                data.Float("holdTime", 1f),
                data.Float("timerDuration", 3f)
            );
        }

        public RuinsPuzzleSwitch(Vector2 position, SwitchType type = SwitchType.Simple,
            string gateId = "", int sequenceOrder = 0, float holdTime = 1f, float timerDuration = 3f)
            : base(position)
        {
            Initialize(type, gateId, sequenceOrder, holdTime, timerDuration);
        }

        private void Initialize(SwitchType type, string gateId, int sequenceOrder, float holdTime, float timerDuration)
        {
            Type = type;
            GateId = gateId;
            SequenceOrder = sequenceOrder;
            HoldTime = holdTime;
            TimerDuration = timerDuration;
            
            State = SwitchState.Inactive;
            holdTimer = 0f;
            activationTimer = 0f;
            playerOnSwitch = false;
            glowAlpha = 0f;
            connectedGates = new List<BlockGate>();
            
            // Setup collider - pressure plate on ground
            Collider = new Hitbox(32f, 8f, -16f, -8f);
            
            // Setup sprite
            Add(sprite = GFX.SpriteBank.Create("ruins_puzzle_switch"));
            sprite.Play("inactive");
            
            // Add subtle glow
            Add(new VertexLight(Color.Orange, 0f, 4, 16));
            
            // Sound
            Add(activationSound = new SoundSource());
        }
        #endregion

        #region Public Methods
        public void Activate()
        {
            if (State == SwitchState.Complete) return;
            
            State = SwitchState.Active;
            sprite.Play("active");
            glowAlpha = 1f;
            
            Audio.Play("event:/game/general/diamond_get", Position);
            
            // Notify connected gates
            foreach (var gate in connectedGates)
            {
                gate.OnSwitchActivated(this);
            }
            
            // Flash effect
            level?.Flash(Color.Orange * 0.2f);
        }

        public void Deactivate()
        {
            if (State == SwitchState.Complete) return;
            
            State = SwitchState.Inactive;
            sprite.Play("inactive");
            glowAlpha = 0f;
            
            // Notify connected gates
            foreach (var gate in connectedGates)
            {
                gate.OnSwitchDeactivated(this);
            }
        }

        public void Complete()
        {
            State = SwitchState.Complete;
            sprite.Play("complete");
            glowAlpha = 1f;
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            level?.Shake(0.2f);
        }

        public void RegisterGate(BlockGate gate)
        {
            if (!connectedGates.Contains(gate))
            {
                connectedGates.Add(gate);
            }
        }
        #endregion

        #region Private Methods
        private void CheckPlayerOnSwitch()
        {
            var player = Scene.Tracker.GetEntity<Player>();
            playerOnSwitch = player != null && Collide.Check(this, player);
            
            if (playerOnSwitch)
            {
                lastPlayer = player;
            }
        }

        private void HandleSwitchType()
        {
            switch (Type)
            {
                case SwitchType.Simple:
                    if (playerOnSwitch && State == SwitchState.Inactive)
                    {
                        Activate();
                    }
                    break;
                    
                case SwitchType.Hold:
                    if (playerOnSwitch)
                    {
                        holdTimer += Engine.DeltaTime;
                        if (holdTimer >= HoldTime && State == SwitchState.Inactive)
                        {
                            Activate();
                        }
                    }
                    else
                    {
                        holdTimer = 0f;
                        if (State == SwitchState.Active)
                        {
                            Deactivate();
                        }
                    }
                    break;
                    
                case SwitchType.Sequence:
                    if (playerOnSwitch && State == SwitchState.Inactive)
                    {
                        // Check if this is the next in sequence
                        var manager = Scene.Tracker.GetEntity<SequencePuzzleManager>();
                        if (manager != null && manager.CanActivate(this))
                        {
                            Activate();
                            manager.OnSwitchActivated(this);
                        }
                    }
                    break;
                    
                case SwitchType.Timed:
                    if (playerOnSwitch && State == SwitchState.Inactive)
                    {
                        Activate();
                        activationTimer = TimerDuration;
                    }
                    
                    if (State == SwitchState.Active && activationTimer > 0f)
                    {
                        activationTimer -= Engine.DeltaTime;
                        if (activationTimer <= 0f)
                        {
                            Deactivate();
                        }
                    }
                    break;
                    
                case SwitchType.Dash:
                    // Check for dash collision
                    if (lastPlayer != null && lastPlayer.DashAttacking && Collide.Check(this, lastPlayer))
                    {
                        if (State == SwitchState.Inactive)
                        {
                            Activate();
                        }
                    }
                    break;
            }
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            
            // Find connected gates by ID
            if (!string.IsNullOrEmpty(GateId))
            {
                foreach (var gate in scene.Tracker.GetEntities<BlockGate>())
                {
                    var typedGate = (BlockGate)gate;
                    if (typedGate.GateId == GateId)
                    {
                        connectedGates.Add(typedGate);
                        typedGate.RegisterSwitch(this);
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            
            CheckPlayerOnSwitch();
            HandleSwitchType();
            
            // Glow fade
            if (glowAlpha > 0f && State != SwitchState.Active && State != SwitchState.Complete)
            {
                glowAlpha = Calc.Approach(glowAlpha, 0f, Engine.DeltaTime * 2f);
            }
        }

        public override void Render()
        {
            base.Render();
            
            // Draw glow effect when active
            if (glowAlpha > 0f)
            {
                Draw.Circle(Collider.Center, 20f, Color.Orange * (glowAlpha * 0.3f), 8);
            }
        }
        #endregion
    }

    /// <summary>
    /// BlockGate - Gate controlled by RuinsPuzzleSwitch
    /// Can open/close based on switch activation
    /// Sprite path: objects/block_gate/
    /// </summary>
    [CustomEntity("DZ/BlockGate")]
    [Tracked]
    public class BlockGate : Solid
    {
        #region Enums
        public enum GateState
        {
            Closed,
            Opening,
            Open,
            Closing
        }
        #endregion

        #region Properties
        public string GateId { get; private set; }
        public GateState State { get; private set; }
        public bool StartOpen { get; private set; }
        public float OpenSpeed { get; private set; }
        public int RequiredSwitches { get; private set; }
        
        private Sprite sprite;
        private float openAmount;
        private float targetOpenAmount;
        private List<RuinsPuzzleSwitch> connectedSwitches;
        private int activatedCount;
        private Level level;
        #endregion

        #region Constructor
        public BlockGate(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, false)
        {
            Initialize(
                data.Attr("gateId", ""),
                data.Bool("startOpen", false),
                data.Float("openSpeed", 2f),
                data.Int("requiredSwitches", 1)
            );
        }

        public BlockGate(Vector2 position, int width, int height, string gateId = "",
            bool startOpen = false, float openSpeed = 2f, int requiredSwitches = 1)
            : base(position, width, height, false)
        {
            Initialize(gateId, startOpen, openSpeed, requiredSwitches);
        }

        private void Initialize(string gateId, bool startOpen, float openSpeed, int requiredSwitches)
        {
            GateId = gateId;
            StartOpen = startOpen;
            OpenSpeed = openSpeed;
            RequiredSwitches = requiredSwitches;
            
            connectedSwitches = new List<RuinsPuzzleSwitch>();
            activatedCount = 0;
            openAmount = startOpen ? 1f : 0f;
            targetOpenAmount = startOpen ? 1f : 0f;
            
            State = startOpen ? GateState.Open : GateState.Closed;
            
            // Setup sprite
            Add(sprite = GFX.SpriteBank.Create("block_gate"));
        }
        #endregion

        #region Public Methods
        public void RegisterSwitch(RuinsPuzzleSwitch switchEntity)
        {
            if (!connectedSwitches.Contains(switchEntity))
            {
                connectedSwitches.Add(switchEntity);
            }
        }

        public void OnSwitchActivated(RuinsPuzzleSwitch switchEntity)
        {
            activatedCount++;
            
            if (activatedCount >= RequiredSwitches)
            {
                Open();
            }
        }

        public void OnSwitchDeactivated(RuinsPuzzleSwitch switchEntity)
        {
            activatedCount = Math.Max(0, activatedCount - 1);
            
            if (activatedCount < RequiredSwitches && State == GateState.Open)
            {
                Close();
            }
        }

        public void Open()
        {
            if (State == GateState.Open || State == GateState.Opening) return;
            
            State = GateState.Opening;
            targetOpenAmount = 1f;
            Audio.Play("event:/game/general/diamond_get", Position);
        }

        public void Close()
        {
            if (State == GateState.Closed || State == GateState.Closing) return;
            
            State = GateState.Closing;
            targetOpenAmount = 0f;
            Audio.Play("event:/game/general/diamond_get", Position);
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();
            
            // Animate open/close
            if (State == GateState.Opening || State == GateState.Closing)
            {
                openAmount = Calc.Approach(openAmount, targetOpenAmount, OpenSpeed * Engine.DeltaTime);
                
                // Update collision
                Collider.Height = Height * (1f - openAmount);
                
                if (openAmount >= 1f)
                {
                    State = GateState.Open;
                }
                else if (openAmount <= 0f)
                {
                    State = GateState.Closed;
                }
            }
        }

        public override void Render()
        {
            // Draw gate with open amount
            sprite.Scale.Y = 1f - openAmount;
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// SequencePuzzleManager - Manages sequence-based puzzle switches
    /// Tracks correct order of switch activation
    /// </summary>
    [CustomEntity("DZ/SequencePuzzleManager")]
    [Tracked]
    public class SequencePuzzleManager : Entity
    {
        #region Properties
        public int CurrentIndex { get; private set; }
        public bool IsComplete { get; private set; }
        public string GateId { get; private set; }
        
        private List<RuinsPuzzleSwitch> sequenceSwitches;
        private List<int> correctOrder;
        private Level level;
        #endregion

        #region Constructor
        public SequencePuzzleManager(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            GateId = data.Attr("gateId", "");
            sequenceSwitches = new List<RuinsPuzzleSwitch>();
            correctOrder = new List<int>();
            CurrentIndex = 0;
            IsComplete = false;
        }
        #endregion

        #region Public Methods
        public bool CanActivate(RuinsPuzzleSwitch switchEntity)
        {
            return switchEntity.SequenceOrder == CurrentIndex;
        }

        public void OnSwitchActivated(RuinsPuzzleSwitch switchEntity)
        {
            if (IsComplete) return;
            
            if (switchEntity.SequenceOrder == CurrentIndex)
            {
                CurrentIndex++;
                
                // Check if sequence complete
                if (CurrentIndex >= sequenceSwitches.Count)
                {
                    IsComplete = true;
                    CompleteSequence();
                }
            }
            else
            {
                // Wrong order - reset
                ResetSequence();
            }
        }

        private void CompleteSequence()
        {
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            level?.Shake(0.3f);
            
            // Open connected gates
            foreach (var gate in level.Tracker.GetEntities<BlockGate>())
            {
                var typedGate = (BlockGate)gate;
                if (typedGate.GateId == GateId)
                {
                    typedGate.Open();
                }
            }
        }

        private void ResetSequence()
        {
            CurrentIndex = 0;
            
            // Reset all switches
            foreach (var sw in sequenceSwitches)
            {
                sw.Deactivate();
            }
            
            Audio.Play("event:/game/general/diamond_get", Position);
        }

        public void RegisterSwitch(RuinsPuzzleSwitch switchEntity)
        {
            if (!sequenceSwitches.Contains(switchEntity))
            {
                sequenceSwitches.Add(switchEntity);
                // Sort by sequence order
                sequenceSwitches.Sort((a, b) => a.SequenceOrder.CompareTo(b.SequenceOrder));
            }
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }
        #endregion
    }
}

