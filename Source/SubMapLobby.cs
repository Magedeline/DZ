

// Add this namespace to access the Player class

namespace MaggyHelper.Entities
{
    /// <summary>
    /// Lobby interface for entering submaps in chapters 10-14
    /// </summary>
    [Tracked(false)]
    public class SubMapLobby : Entity
    {
        private readonly int chapterNumber;
        private readonly List<SubMapPortal> portals;
        private SubMapUi ui;
        private bool uiActive;
        private global::Celeste.Player player;

        public SubMapLobby(Vector2 position, int chapter) : base(position)
        {
            chapterNumber = chapter;
            portals = new List<SubMapPortal>();
            
            Collider = new Hitbox(32f, 32f, -16f, -16f);
            Add(new PlayerCollider(OnPlayerInteract));
            
            Depth = -100;
            Tag = Tags.TransitionUpdate;
        }

        public override void Added(Scene scene)
        {
            base.Added(Scene);
            
            Level level = SceneAs<Level>();
            if (level != null)
            {
                createPortals();
                createUi();
                
                // Ensure SubMapManager exists
                if (SubMapManager.Instance == null)
                {
                    level.Add(new SubMapManager());
                }
            }
        }

        private void createPortals()
        {
            // Create portals for submaps 3-6
            for (int i = 3; i <= 6; i++)
            {
                Vector2 portalPos = Position + new Vector2((i - 3) * 80f - 120f, 0f);
                var portal = new SubMapPortal(portalPos, chapterNumber, i);
                portals.Add(portal);
                Scene.Add(portal);
            }
        }

        private void createUi()
        {
            ui = new SubMapUi(chapterNumber);
            ui.Visible = false;
            Scene.Add(ui);
        }

        private void OnPlayerInteract(global::Celeste.Player player)
        {
            this.player = player;
            
            if (!uiActive)
            {
                showUi();
            }
        }

        private void showUi()
        {
            uiActive = true;
            ui.Visible = true;
            ui.Show();
            
            // Freeze player input temporarily
            player.StateMachine.State = 11; // Use integer value for dummy state

            Add(new Coroutine(handleUiInput()));
        }

        private IEnumerator handleUiInput()
        {
            while (uiActive)
            {
                // Check for UI navigation input
                if (Input.MenuCancel.Pressed)
                {
                    hideUi();
                    yield break;
                }
                
                // Check for submap selection
                for (int i = 0; i < 4; i++) // Submaps 3-6
                {
                    if (Input.MenuLeft.Pressed && i == 0) // Left for submap 3
                    {
                        tryEnterSubmap(3);
                        yield break;
                    }
                    else if (Input.MenuUp.Pressed && i == 1) // Up for submap 4
                    {
                        tryEnterSubmap(4);
                        yield break;
                    }
                    else if (Input.MenuRight.Pressed && i == 2) // Right for submap 5
                    {
                        tryEnterSubmap(5);
                        yield break;
                    }
                    else if (Input.MenuDown.Pressed && i == 3) // Down for submap 6
                    {
                        tryEnterSubmap(6);
                        yield break;
                    }
                }
                
                yield return null;
            }
        }

        private void tryEnterSubmap(int submapNumber)
        {
            if (SubMapManager.Instance?.CanEnterSubmap(chapterNumber, submapNumber) == true)
            {
                hideUi();
                SubMapManager.Instance.EnterSubmap(SceneAs<Level>(), player, chapterNumber, submapNumber);
            }
            else
            {
                // Show locked message
                Audio.Play("event:/ui/main/button_invalid");
                Scene.Add(new MiniTextbox($"SUBMAP_LOCKED_CH{chapterNumber}_{submapNumber}"));
            }
        }

        private void hideUi()
        {
            uiActive = false;
            ui.Visible = false;
            ui.Hide();
            
            // Restore player control
            if (player != null)
            {
                player.StateMachine.State = 0; // Use 0 for normal state instead of Player.StNormal
            }
        }

        public override void Update()
        {
            base.Update();
            
            // Update portal visibility based on unlock status
            for (int i = 0; i < portals.Count; i++)
            {
                int submapNumber = i + 3;
                bool canEnter = SubMapManager.Instance?.CanEnterSubmap(chapterNumber, submapNumber) ?? false;
                portals[i].SetEnabled(canEnter);
            }
        }
    }

    /// <summary>
    /// Visual portal for submap entry
    /// </summary>
    public class SubMapPortal : Entity
    {
        private readonly int chapterNumber;
        private readonly int submapNumber;
        private bool enabled;
        
        private Sprite sprite;
        private VertexLight light;
        private SineWave sine;
        
        public SubMapPortal(Vector2 position, int chapter, int submap) : base(position)
        {
            chapterNumber = chapter;
            submapNumber = submap;
            
            Depth = -50;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            createVisuals();
        }

        private void createVisuals()
        {
            // Create portal sprite (using heart gem as placeholder)
            sprite = GFX.SpriteBank.Create("maggy_heartgem0");
            sprite.Play("spin");
            sprite.CenterOrigin();
            Add(sprite);
            
            // Create light
            light = new VertexLight(getChapterColor(), 0.8f, 32, 48);
            Add(light);
            
            // Create floating animation
            sine = new SineWave(0.8f, 0f);
            sine.Randomize();
            Add(sine);
        }

        public void SetEnabled(bool enable)
        {
            enabled = enable;
            Visible = enable;
            light.Visible = enable;
        }

        private Color getChapterColor()
        {
            return chapterNumber switch
            {
                10 => Color.Orange,
                11 => Color.LightBlue,
                12 => Color.Purple,
                13 => Color.Gold,
                14 => Color.Silver,
                _ => Color.White
            };
        }

        public override void Update()
        {
            base.Update();
            
            if (enabled)
            {
                sprite.Y = sine.Value * 3f;
                light.Y = sprite.Y;
                
                // Scale based on distance to player
                global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null)
                {
                    float distance = Vector2.Distance(Position, player.Position);
                    float scale = Calc.ClampedMap(distance, 24f, 80f, 1.2f, 0.8f);
                    sprite.Scale = Vector2.One * scale;
                }
            }
        }
    }

    /// <summary>
    /// UI overlay for submap selection
    /// </summary>
    public class SubMapUi : Entity
    {
        private readonly int chapterNumber;
        private float alpha;

        public SubMapUi(int chapter) : base()
        {
            chapterNumber = chapter;
            Depth = -1000000;
            Tag = Tags.HUD | Tags.Global;
        }

        public void Show()
        {
            // Corrected the code to properly set up the tween without assigning it to a float variable  
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.3f, true);
            tween.OnUpdate = t => alpha = t.Eased;
            Add(tween);
        }

        public void Hide()
        {
            // Corrected the code to properly set up the tween without assigning it to a float variable  
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.2f, true);
            tween.OnUpdate = t => alpha = 1f - t.Eased;
            Add(tween);
        }

        public override void Render()
        {
            if (alpha <= 0f) return;

            Level level = SceneAs<Level>();
            if (level == null) return;

            // Draw semi-transparent background
            Draw.Rect(0, 0, 1920, 1080, Color.Black * (alpha * 0.6f));

            // Draw title
            Vector2 titlePos = new Vector2(960f, 200f);
            ActiveFont.DrawOutline($"Chapter {chapterNumber} Submaps", titlePos,
                new Vector2(0.5f, 0.5f), Vector2.One * 1.5f, Color.White * alpha, 2f, Color.Black * alpha);

            // Draw submap options
            var submaps = SubMapManager.Instance?.GetChapterSubmaps(chapterNumber);
            if (submaps != null)
            {
                for (int i = 0; i < submaps.Count; i++)
                {
                    var submap = submaps[i];
                    Vector2 pos = new Vector2(960f, 350f + i * 80f);

                    Color textColor = submap.IsUnlocked ? Color.White : Color.Gray;
                    string status = submap.IsCompleted ? " ?" : (submap.IsUnlocked ? "" : " ??");
                    string text = $"Submap {submap.SubmapNumber}{status}";

                    ActiveFont.DrawOutline(text, pos, new Vector2(0.5f, 0.5f), Vector2.One,
                        textColor * alpha, 2f, Color.Black * alpha);
                }
            }

            // Draw instructions
            Vector2 instructPos = new Vector2(960f, 700f);
            ActiveFont.DrawOutline("Use Arrow Keys to Select � ESC to Cancel", instructPos,
                new Vector2(0.5f, 0.5f), Vector2.One * 0.8f, Color.LightGray * alpha, 2f, Color.Black * alpha);
        }
    }
}



