namespace MaggyHelper.Entities
{
    /// <summary>
    /// Small heart gem collectible for submaps in chapters 10-14
    /// </summary>
    [Tracked(false)]
    public class SmallHeartGem : Entity
    {
        public const string COLLECTED_FLAG_PREFIX = "small_heart_gem_";
        
        private readonly string gemId;
        private readonly int chapterNumber;
        private readonly int submapNumber;
        private readonly Color gemColor;
        
        private Sprite sprite;
        private VertexLight light;
        private BloomPoint bloom;
        private SineWave sine;
        private ParticleType particles;
        private bool collected;
        private Wiggler scaleWiggler;
        private Wiggler moveWiggler;
        private Vector2 moveWiggleDir;
        private SoundSource collectSfx;

        public SmallHeartGem(Vector2 position, int chapter, int submap, string id = null) : base(position)
        {
            chapterNumber = chapter;
            submapNumber = submap;
            gemId = id ?? $"ch{chapter}_submap{submap}";
            
            // Set color based on chapter
            gemColor = getChapterColor(chapter);
            
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(OnPlayer));
            
            Depth = -100;
            
            Tag = Tags.TransitionUpdate;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Check if already collected
            Level level = SceneAs<Level>();
            string flag = COLLECTED_FLAG_PREFIX + gemId;
            collected = level?.Session.GetFlag(flag) ?? false;
            
            if (collected)
            {
                RemoveSelf();
                return;
            }
            
            createVisuals();
            createParticles();
        }

        private void createVisuals()
        {
            // Create sprite
            sprite = GFX.SpriteBank.Create("maggy_heartgem0");
            sprite.Color = gemColor;
            sprite.Play("spin");
            sprite.CenterOrigin();
            Add(sprite);
            
            // Create light effect
            light = new VertexLight(gemColor, 1f, 32, 64);
            Add(light);
            
            // Create bloom effect
            bloom = new BloomPoint(0.75f, 16f);
            Add(bloom);
            
            // Create floating animation
            sine = new SineWave(0.6f, 0f);
            sine.Randomize();
            Add(sine);
            
            // Create wigglers for collection animation
            scaleWiggler = Wiggler.Create(1f, 4f, v => sprite.Scale = Vector2.One * (1f + v * 0.2f));
            Add(scaleWiggler);
            
            moveWiggler = Wiggler.Create(0.8f, 2f);
            Add(moveWiggler);
            moveWiggleDir = new Vector2((float)Math.Cos(Calc.Random.NextAngle()), (float)Math.Sin(Calc.Random.NextAngle()));
        }

        private void createParticles()
        {
            particles = new ParticleType
            {
                Source = GFX.Game["particles/sparkle"],
                Color = gemColor,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SizeRange = 0.5f,
                Direction = (float)Math.PI / 2f,
                DirectionRange = (float)Math.PI / 3f,
                SpeedMin = 4f,
                SpeedMax = 20f,
                SpeedMultiplier = 0.01f,
                LifeMin = 0.6f,
                LifeMax = 1.2f
            };
        }

        public override void Update()
        {
            base.Update();
            
            if (collected) return;
            
            // Floating animation
            sprite.Y = sine.Value * 2f;
            light.Y = sprite.Y;
            bloom.Y = sprite.Y;
            
            // Move wiggle effect
            if (moveWiggler.Active)
            {
                sprite.Position += moveWiggleDir * moveWiggler.Value * 8f;
            }
            
            // Emit particles occasionally
            if (Scene.OnInterval(0.1f))
            {
                SceneAs<Level>()?.ParticlesBG.Emit(particles, 1, Center, Vector2.One * 4f);
            }
        }

        private void OnPlayerInteract(global::Celeste.Player player)
        {
            if (collected) return;
            
            collected = true;
            Collidable = false;

            MaggyProgressionManager.RecordMiniHeart(SceneAs<Level>(), gemId);
            
            // Set collection flag
            Level level = SceneAs<Level>();
            string flag = COLLECTED_FLAG_PREFIX + gemId;
            level?.Session.SetFlag(flag, true);
            
            // Start collection sequence
            Add(new Coroutine(collectionSequence(player)));
        }

        private IEnumerator collectionSequence(global::Celeste.Player player)
        {
            // Play collection sound
            collectSfx = new SoundSource();
            collectSfx.Position = Position;
            Add(collectSfx);
            collectSfx.Play("event:/game/general/seed_poof");
            
            // Visual effects
            Level level = SceneAs<Level>();
            level?.Displacement.AddBurst(Center, 0.35f, 8f, 32f, 0.8f);
            level?.Particles.Emit(particles, 12, Center, Vector2.One * 8f);
            
            // Scale and move animation
            scaleWiggler.Start();
            moveWiggler.Start();
            
            // Fade out over time
            Tween fadeTween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.6f, true);
            fadeTween.OnUpdate = t => {
                sprite.Color = Color.Lerp(gemColor, Color.Transparent, t.Eased);
                light.Alpha = 1f - t.Eased;
                bloom.Alpha = 1f - t.Eased;
            };
            fadeTween.OnComplete = t => RemoveSelf();
            Add(fadeTween);
            
            // Check if all gems in submap are collected
            yield return 0.3f;
            checkSubmapCompletion(level);
            
            yield return null;
        }

        private void checkSubmapCompletion(Level level)
        {
            if (level == null) return;
            
            // Count total small heart gems in this submap
            int totalGems = countTotalGemsInSubmap(level);
            int collectedGems = countCollectedGemsInSubmap(level);
            
            if (collectedGems >= totalGems)
            {
                // All gems collected - unlock next submap or complete chapter
                string completionFlag = $"submap_complete_ch{chapterNumber}_{submapNumber}";
                level.Session.SetFlag(completionFlag, true);
                
                // Show completion message
                level.Add(new MiniTextbox($"SUBMAP_COMPLETE_CH{chapterNumber}_{submapNumber}"));
                
                // Play completion sound
                Audio.Play("event:/game/general/crystalheart_blue_get", Position);
            }
        }

        private int countTotalGemsInSubmap(Level level)
        {
            // This would be set based on the specific submap design
            // For now, return a default value that can be overridden
            return getSubmapGemCount(chapterNumber, submapNumber);
        }

        private int countCollectedGemsInSubmap(Level level)
        {
            int count = 0;
            int totalGems = getSubmapGemCount(chapterNumber, submapNumber);
            
            for (int i = 1; i <= totalGems; i++)
            {
                string flag = COLLECTED_FLAG_PREFIX + $"ch{chapterNumber}_submap{submapNumber}_gem{i}";
                if (level.Session.GetFlag(flag))
                    count++;
            }
            
            return count;
        }

        private Color getChapterColor(int chapter)
        {
            return chapter switch
            {
                10 => Color.Orange,        // Ruins theme
                11 => Color.LightBlue,     // Snowdin theme  
                12 => Color.Purple,        // Wateredge theme
                13 => Color.Gold,          // Next chapter theme
                14 => Color.Silver,        // Final chapter theme
                _ => Color.White
            };
        }

        private int getSubmapGemCount(int chapter, int submap)
        {
            // Define gem counts for each submap
            // Chapters 10-14, submaps 3-6, varying gem counts
            switch (chapter)
            {
                case 10:
                    switch (submap)
                    {
                        case 3: return 3;
                        case 4: return 4;
                        case 5: return 5;
                        case 6: return 6;
                        default: return 3;
                    }
                case 11:
                    switch (submap)
                    {
                        case 3: return 3;
                        case 4: return 4;
                        case 5: return 5;
                        case 6: return 6;
                        default: return 3;
                    }
                case 12:
                    switch (submap)
                    {
                        case 3: return 4;
                        case 4: return 5;
                        case 5: return 6;
                        case 6: return 7;
                        default: return 3;
                    }
                case 13:
                    switch (submap)
                    {
                        case 3: return 5;
                        case 4: return 6;
                        case 5: return 7;
                        case 6: return 8;
                        default: return 3;
                    }
                case 14:
                    switch (submap)
                    {
                        case 3: return 6;
                        case 4: return 7;
                        case 5: return 8;
                        case 6: return 9;
                        default: return 3;
                    }
                default:
                    return 3;
            }
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            OnPlayerInteract(player);
        }

        public override void Render()
        {
            if (!collected)
            {
                base.Render();
            }
        }

        public void OnCollect()
        {
            // Find the player in the scene
            var player = Scene?.Tracker?.GetEntity<global::Celeste.Player>();
            if (player != null && !collected)
            {
                OnPlayerInteract(player);
            }
        }
    }
}



