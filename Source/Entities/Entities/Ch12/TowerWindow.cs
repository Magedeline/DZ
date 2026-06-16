namespace Celeste.Entities.Chapters.Ch12
{
    #region Enums
    public enum WindowView
    {
        Sky,
        Stars,
        Moon,
        Sunrise,
        Storm,
        City
    }
    #endregion

    /// <summary>
    /// TowerWindow - Decorative window with light effects
    /// Can show different scenes and create atmosphere
    /// Sprite path: objects/tower_window/
    /// </summary>
    [CustomEntity("DZ/TowerWindow")]
    [Tracked]
    public class TowerWindow : Actor
    {
        #region Enums
        public enum WindowState
        {
            Dark,
            Lit,
            Flickering,
            Showing
        }
        #endregion

        #region Properties
        public WindowState State { get; private set; }
        public WindowView View { get; private set; }
        public float LightIntensity { get; private set; }
        public bool IsLit => State != WindowState.Dark;
        
        private Sprite frameSprite;
        private Sprite viewSprite;
        private VertexLight windowLight;
        private float flickerTimer;
        private Level level;
        private List<LightRay> lightRays;
        private bool showScene;
        #endregion

        #region Constructor
        public TowerWindow(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Enum("view", WindowView.Sky),
                data.Float("lightIntensity", 0.6f),
                data.Bool("startLit", true)
            );
        }

        public TowerWindow(Vector2 position, WindowView view = WindowView.Sky,
            float lightIntensity = 0.6f, bool startLit = true)
            : base(position)
        {
            Initialize(view, lightIntensity, startLit);
        }

        private void Initialize(WindowView view, float lightIntensity, bool startLit)
        {
            View = view;
            LightIntensity = lightIntensity;
            
            State = startLit ? WindowState.Lit : WindowState.Dark;
            flickerTimer = 0f;
            showScene = false;
            lightRays = new List<LightRay>();
            
            Collider = new Hitbox(48f, 64f, -24f, -64f);
            
            Add(frameSprite = GFX.SpriteBank.Create("tower_window_frame"));
            frameSprite.Play("default");
            
            Add(viewSprite = GFX.SpriteBank.Create("tower_window_view"));
            viewSprite.Play(view.ToString().ToLower());
            viewSprite.Position = new Vector2(0f, -32f);
            
            Color lightColor = GetViewColor(view);
            Add(windowLight = new VertexLight(lightColor, startLit ? lightIntensity : 0f, 8, 32));
            windowLight.Position = new Vector2(0f, -32f);
        }
        #endregion

        #region Public Methods
        public void Light()
        {
            State = WindowState.Lit;
            windowLight.Alpha = LightIntensity;
            
            Audio.Play("event:/game/general/diamond_get", Position);
        }

        public void Extinguish()
        {
            State = WindowState.Dark;
            windowLight.Alpha = 0f;
        }

        public void SetFlickering(bool flicker)
        {
            State = flicker ? WindowState.Flickering : WindowState.Lit;
        }

        public void SetView(WindowView view)
        {
            View = view;
            viewSprite.Play(view.ToString().ToLower());
            windowLight.Color = GetViewColor(view);
        }

        public void ShowScene()
        {
            showScene = true;
            State = WindowState.Showing;
            
            Add(new Coroutine(ShowSceneRoutine()));
        }
        #endregion

        #region Private Methods
        private IEnumerator ShowSceneRoutine()
        {
            // Dramatic reveal
            for (int i = 0; i < 10; i++)
            {
                CreateLightRay();
                yield return 0.1f;
            }
            
            yield return 2f;
            
            showScene = false;
            State = WindowState.Lit;
        }

        private void CreateLightRay()
        {
            var ray = new LightRay(
                Position + new Vector2(0f, -32f),
                GetViewColor(View)
            );
            lightRays.Add(ray);
            Scene.Add(ray);
        }

        private Color GetViewColor(WindowView view)
        {
            return view switch
            {
                WindowView.Sky => Color.SkyBlue,
                WindowView.Stars => Color.DarkBlue,
                WindowView.Moon => Color.Silver,
                WindowView.Sunrise => Color.Orange,
                WindowView.Storm => Color.DarkGray,
                WindowView.City => Color.Gold,
                _ => Color.White
            };
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
            
            if (State == WindowState.Flickering)
            {
                flickerTimer += Engine.DeltaTime * 15f;
                windowLight.Alpha = LightIntensity * (0.7f + (float)Math.Sin(flickerTimer) * 0.3f);
            }
            
            if (State == WindowState.Showing || showScene)
            {
                if (Scene.OnInterval(0.2f))
                {
                    CreateLightRay();
                }
            }
            
            lightRays.RemoveAll(r => r == null || r.Scene == null);
        }

        public override void Render()
        {
            // Draw window glow
            if (IsLit)
            {
                Draw.Rect(Position.X - 20, Position.Y - 60, 40, 56, GetViewColor(View) * 0.2f);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// LightRay - Ray of light from window
    /// </summary>
    public class LightRay : Actor
    {
        private Color color;
        private float lifetime;
        private float maxLifetime;
        private float length;
        private float angle;

        public LightRay(Vector2 position, Color color)
            : base(position)
        {
            this.color = color;
            maxLifetime = 1f;
            lifetime = maxLifetime;
            length = Calc.Random.NextFloat() * (100f - 60f) + 60f;
            angle = Calc.Random.NextFloat() * (0.5f - -0.5f) + -0.5f;
        }

        public override void Update()
        {
            base.Update();
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Vector2 end = Position + Calc.AngleToVector(angle, length);
            Draw.Line(Position, end, color * (alpha * 0.3f), 2f);
        }
    }

    /// <summary>
    /// WindowSequence - Controls multiple windows for scene transitions
    /// </summary>
    [CustomEntity("DZ/WindowSequence")]
    public class WindowSequence : Entity
    {
        private List<TowerWindow> windows;
        private List<WindowView> viewSequence;
        private float transitionInterval;
        private int currentIndex;

        public WindowSequence(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            windows = new List<TowerWindow>();
            viewSequence = new List<WindowView>();
            transitionInterval = data.Float("transitionInterval", 5f);
            currentIndex = 0;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            foreach (var window in scene.Tracker.GetEntities<TowerWindow>())
            {
                windows.Add((TowerWindow)window);
            }
        }

        public void PlaySequence()
        {
            Add(new Coroutine(SequenceRoutine()));
        }

        private IEnumerator SequenceRoutine()
        {
            foreach (var view in viewSequence)
            {
                foreach (var window in windows)
                {
                    window.SetView(view);
                }
                
                yield return transitionInterval;
            }
        }

        public void AddViewToSequence(WindowView view)
        {
            viewSequence.Add(view);
        }
    }
}
