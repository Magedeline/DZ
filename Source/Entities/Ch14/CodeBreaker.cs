namespace Celeste.Entities.Chapters.Ch14
{
    /// <summary>
    /// CodeBreaker - Puzzle entity requiring correct code input
    /// Player must input correct sequence to unlock
    /// Sprite path: objects/code_breaker/
    /// </summary>
    [CustomEntity("MaggyHelper/CodeBreaker")]
    [Tracked]
    public class CodeBreaker : Actor
    {
        #region Enums
        public enum CodeState
        {
            Locked,
            Inputting,
            Checking,
            Correct,
            Incorrect,
            Unlocked
        }
        #endregion

        #region Properties
        public CodeState State { get; private set; }
        public int CodeLength { get; private set; }
        public float InputTimeout { get; private set; }
        public bool IsUnlocked => State == CodeState.Unlocked;
        
        private Sprite sprite;
        private List<int> correctCode;
        private List<int> currentInput;
        private float inputTimer;
        private int currentDigit;
        private Level level;
        private List<CodeDigit> digitDisplays;
        private List<CodeParticle> particles;
        private TalkComponent talkComponent;
        private Player interactingPlayer;
        private VertexLight codeLight;
        #endregion

        #region Constructor
        public CodeBreaker(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Attr("code", "1234"),
                data.Float("inputTimeout", 5f)
            );
        }

        public CodeBreaker(Vector2 position, string code = "1234", float inputTimeout = 5f)
            : base(position)
        {
            Initialize(code, inputTimeout);
        }

        private void Initialize(string code, float inputTimeout)
        {
            CodeLength = code.Length;
            InputTimeout = inputTimeout;
            
            // Parse code
            correctCode = new List<int>();
            foreach (char c in code)
            {
                correctCode.Add(int.Parse(c.ToString()));
            }
            
            State = CodeState.Locked;
            currentInput = new List<int>();
            currentDigit = 0;
            inputTimer = 0f;
            digitDisplays = new List<CodeDigit>();
            particles = new List<CodeParticle>();
            
            Collider = new Hitbox(48f, 40f, -24f, -40f);
            
            Add(sprite = GFX.SpriteBank.Create("code_breaker"));
            sprite.Play("locked");
            
            Add(codeLight = new VertexLight(Color.Red, 0.4f, 8, 24));
            
            Add(talkComponent = new TalkComponent(
                new Rectangle(-28, -48, 56, 56),
                new Vector2(0f, -56f),
                _ => Interact()
            ));
        }
        #endregion

        #region Public Methods
        public void Interact()
        {
            var player = Scene.Tracker.GetEntity<Player>();
            if (player == null) return;
            if (IsUnlocked) return;
            
            interactingPlayer = player;
            Add(new Coroutine(InputRoutine()));
        }

        public void InputDigit(int digit)
        {
            if (State != CodeState.Inputting) return;
            
            currentInput.Add(digit);
            currentDigit++;
            inputTimer = InputTimeout;
            
            // Update display
            UpdateDigitDisplays();
            
            Audio.Play("event:/game/general/diamond_get", Position);
            CreateCodeParticle();
            
            // Check if complete
            if (currentInput.Count >= CodeLength)
            {
                CheckCode();
            }
        }

        public void ClearInput()
        {
            currentInput.Clear();
            currentDigit = 0;
            inputTimer = InputTimeout;
            UpdateDigitDisplays();
        }
        #endregion

        #region Private Methods
        private IEnumerator InputRoutine()
        {
            interactingPlayer.StateMachine.State = Player.StDummy;
            interactingPlayer.StateMachine.Locked = true;
            
            State = CodeState.Inputting;
            sprite.Play("inputting");
            codeLight.Color = Color.Yellow;
            
            // Create digit displays
            CreateDigitDisplays();
            
            inputTimer = InputTimeout;
            
            while (State == CodeState.Inputting)
            {
                inputTimer -= Engine.DeltaTime;
                
                // Check for digit input
                if (Input.MenuLeft.Pressed)
                {
                    InputDigit((currentDigit + 9) % 10); // Previous digit
                }
                else if (Input.MenuRight.Pressed)
                {
                    InputDigit((currentDigit + 1) % 10); // Next digit
                }
                else if (Input.MenuDown.Pressed)
                {
                    InputDigit(Calc.Random.Next(10)); // Random
                }
                else if (Input.Grab.Pressed)
                {
                    // Confirm current digit
                    if (currentInput.Count < CodeLength)
                    {
                        InputDigit(Calc.Random.Next(10));
                    }
                }
                else if (Input.Jump.Pressed)
                {
                    ClearInput();
                }
                
                // Timeout check
                if (inputTimer <= 0f)
                {
                    State = CodeState.Locked;
                    sprite.Play("locked");
                    codeLight.Color = Color.Red;
                    break;
                }
                
                yield return null;
            }
            
            // Wait for check result
            while (State == CodeState.Checking)
            {
                yield return null;
            }
            
            // Handle result
            if (State == CodeState.Correct)
            {
                yield return CorrectRoutine();
            }
            else if (State == CodeState.Incorrect)
            {
                yield return IncorrectRoutine();
            }
            
            interactingPlayer.StateMachine.Locked = false;
            interactingPlayer.StateMachine.State = Player.StNormal;
        }

        private void CheckCode()
        {
            State = CodeState.Checking;
            
            Add(new Coroutine(CheckRoutine()));
        }

        private IEnumerator CheckRoutine()
        {
            // Dramatic check animation
            for (int i = 0; i < 5; i++)
            {
                CreateCodeParticle();
                yield return 0.1f;
            }
            
            // Compare codes
            bool correct = true;
            for (int i = 0; i < CodeLength; i++)
            {
                if (currentInput[i] != correctCode[i])
                {
                    correct = false;
                    break;
                }
            }
            
            if (correct)
            {
                State = CodeState.Correct;
            }
            else
            {
                State = CodeState.Incorrect;
            }
        }

        private IEnumerator CorrectRoutine()
        {
            sprite.Play("correct");
            codeLight.Color = Color.Green;
            
            // Celebration particles
            for (int i = 0; i < 20; i++)
            {
                CreateCodeParticle();
                yield return 0.05f;
            }
            
            level?.Flash(Color.Green * 0.3f);
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            yield return 0.5f;
            
            State = CodeState.Unlocked;
            sprite.Play("unlocked");
            
            // Set flag
            level?.Session.SetFlag("code_breaker_unlocked", true);
        }

        private IEnumerator IncorrectRoutine()
        {
            sprite.Play("incorrect");
            codeLight.Color = Color.Red;
            
            // Error particles
            for (int i = 0; i < 10; i++)
            {
                CreateCodeParticle();
                yield return 0.03f;
            }
            
            level?.Shake(0.2f);
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            yield return 1f;
            
            // Reset
            ClearInput();
            State = CodeState.Locked;
            sprite.Play("locked");
        }

        private void CreateDigitDisplays()
        {
            digitDisplays.Clear();
            
            for (int i = 0; i < CodeLength; i++)
            {
                var digit = new CodeDigit(
                    Position + new Vector2((i - CodeLength / 2f) * 16f, -30f),
                    i
                );
                digitDisplays.Add(digit);
                Scene.Add(digit);
            }
        }

        private void UpdateDigitDisplays()
        {
            for (int i = 0; i < digitDisplays.Count; i++)
            {
                int digit = i < currentInput.Count ? currentInput[i] : -1;
                digitDisplays[i].SetDigit(digit);
            }
        }

        private void CreateCodeParticle()
        {
            var particle = new CodeParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 40f - 20f, Calc.Random.NextFloat() * 40f - 20f),
                new Vector2(Calc.Random.NextFloat() * 60f - 30f, -Calc.Random.NextFloat() * 60f)
            );
            particles.Add(particle);
            Scene.Add(particle);
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
            particles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw code display
            // (Would need proper font rendering here)
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// CodeDigit - Single digit display
    /// </summary>
    public class CodeDigit : Actor
    {
        private int digit;
        private int index;
        private float scale;

        public CodeDigit(Vector2 position, int index)
            : base(position)
        {
            this.index = index;
            digit = -1;
            scale = 1f;
        }

        public void SetDigit(int d)
        {
            digit = d;
            scale = 1.3f;
        }

        public override void Update()
        {
            base.Update();
            scale = Calc.Approach(scale, 1f, 2f * Engine.DeltaTime);
        }

        public override void Render()
        {
            string display = digit >= 0 ? digit.ToString() : "_";
            Color color = digit >= 0 ? Color.Cyan : Color.Gray;
            // Would need proper font rendering here
        }
    }

    /// <summary>
    /// CodeParticle - Particle for code effects
    /// </summary>
    public class CodeParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;

        public CodeParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (0.6f - 0.3f) + 0.3f;
            lifetime = maxLifetime;
            
            Color[] colors = { Color.Cyan, Color.Green, Color.Yellow };
            color = colors[Calc.Random.Next(colors.Length)];
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity.Y += 50f * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 4f, color * (alpha * 0.6f), 4);
        }
    }
}
