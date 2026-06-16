using DZ;
using System.Runtime.CompilerServices;
using FMOD.Studio;

namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/Cartridge")]
    [HotReloadable]
    public class Cartridge : Entity
    {
        private class UnlockedRemixExtra : Entity
        {
            private float alpha, textAlpha;
            public string[] text;
            private bool waitForKeyPress;
            private float timer;
            public int textIndex = 0;
            private string menuSprite;

            public UnlockedRemixExtra(string[] unlockText, string menuSprite)
            {
                text = unlockText;
                this.menuSprite = menuSprite;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public override void Added(Scene scene)
            {
                base.Added(scene);
                Tag = Tags.HUD | Tags.PauseUpdate;
                for (int i = 0; i < text.Length; i++)
                    text[i] = ActiveFont.FontSize.AutoNewline(Dialog.Clean(text[i]), 900);
                Depth = -10000;
            }

            public IEnumerator EaseIn()
            {
                _ = Scene;
                while ((textAlpha = (alpha += Engine.DeltaTime / 0.5f)) < 1f)
                {
                    yield return null;
                }
                alpha = 1f;
                yield return 1.5f;
                waitForKeyPress = true;
            }

            public IEnumerator EaseOut()
            {
                waitForKeyPress = false;
                while ((textAlpha = (alpha -= Engine.DeltaTime / 0.5f)) > 0f)
                {
                    yield return null;
                }
                alpha = 0f;
                RemoveSelf();
            }

            public IEnumerator NextText()
            {
                while ((textAlpha -= Engine.DeltaTime / 0.5f) > 0f)
                {
                    yield return null;
                }
                textIndex++;
                while ((textAlpha += Engine.DeltaTime / 0.5f) < 1f)
                {
                    yield return null;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public override void Update()
            {
                timer += Engine.DeltaTime;
                base.Update();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public override void Render()
            {
                float num = Ease.CubeOut(alpha);
                float textCol = Ease.CubeOut(textAlpha);
                Vector2 value = global::Celeste.Celeste.TargetCenter + new Vector2(0f, 64f);
                Vector2 value2 = Vector2.UnitY * 64f * (1f - num);
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * num * 0.8f);
                GFX.Gui[menuSprite].DrawJustified(value - value2 + new Vector2(0f, 32f), new Vector2(0.5f, 1f), Color.White * num);
                ActiveFont.Draw(text[Math.Min(textIndex, text.Length - 1)], value + value2, new Vector2(0.5f, 0f), Vector2.One, Color.White * textCol);
                if (waitForKeyPress)
                {
                    GFX.Gui["textboxbutton"].DrawCentered(new Vector2(1824f, 984 + ((timer % 1f < 0.25f) ? 6 : 0)));
                }
            }
        }

        public static ParticleType P_Shine;
        public static ParticleType P_Collect;

        // Cartridge attributes
        public bool IsGhost;
        private Sprite sprite;
        private SineWave hover;
        private BloomPoint bloom;
        private VertexLight light;
        private Wiggler scaleWiggler;
        private bool collected;
        private Vector2[] nodes;
        private EventInstance remixSfx;
        private bool collecting;

        // Remix Extra attributes
        private string spritePath;
        private string menuSprite;
        private string[] unlockText;
        private string remixExtraToUnlock;
        
        // Enhanced collectible properties
        private string onCollect;
        private string customAudio;
        private Color particleColor;
        private float glowStrength;
        private float bloomStrength;
        private float wiggleIntensity;
        private float floatSpeed;
        private float floatRange;
        private float collectDelay;
        private bool persistent;
        private bool isChapter19Finale;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Cartridge(Vector2 position, Vector2[] nodes) : base(position)
        {
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            this.nodes = nodes;
            Add(new PlayerCollider(OnPlayer));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Cartridge(EntityData data, Vector2 offset) : this(data.Position + offset, data.NodesOffset(offset))
        {
            spritePath = data.Attr("spritePath", "collectables/cartridge/");
            
            // Determine unlock text based on remix extra to unlock or use custom text
            string customUnlockText = data.Attr("unlockText", "");
            if (string.IsNullOrEmpty(customUnlockText))
            {
                string remixToUnlock = data.Attr("remixExtraToUnlock", "");
                unlockText = DetermineUnlockText(remixToUnlock);
            }
            else
            {
                unlockText = customUnlockText.Split(',');
            }
            
            menuSprite = data.Attr("menuSprite", "collectables/cartridge");
            remixExtraToUnlock = data.Attr("remixExtraToUnlock", "");
            
            // Load enhanced properties
            onCollect = data.Attr("onCollect", "");
            customAudio = data.Attr("customAudio", "");
            particleColor = Calc.HexToColor(data.Attr("particleColor", "FFD700")); // Gold color for cartridge
            glowStrength = data.Float("glowStrength", 1.5f); // Stronger glow for cartridge
            bloomStrength = data.Float("bloomStrength", 1.0f);
            wiggleIntensity = data.Float("wiggleIntensity", 0.5f); // More dramatic wiggle
            floatSpeed = data.Float("floatSpeed", 1.5f); // Slower, more majestic float
            floatRange = data.Float("floatRange", 3.0f); // Larger float range
            collectDelay = data.Float("collectDelay", 0.5f);
            persistent = data.Bool("persistent", true);
            isChapter19Finale = data.Bool("isChapter19Finale", false);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Added(Scene scene)
        {
            base.Added(scene);

            // Check if this remix extra is already unlocked
            IsGhost = IngesteModule.SaveData.UnlockedRemixExtraIDs.Contains(remixExtraToUnlock);
            
            string path = IsGhost ? "ghost" : "idle";
            sprite = new Sprite(GFX.Game, spritePath);
            
            // Cartridge has more elaborate animations than cassette/tape
            sprite.Add("idle", path, 0.05f, "pulse", new int[] { 
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            });
            sprite.Add("spin", path, 0.05f, "spin", new int[] { 
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 
            });
            sprite.Add("pulse", path, 0.03f, "idle", new int[] { 
                16, 17, 18, 19, 20, 21, 22, 23 
            });
            sprite.CenterOrigin();
            Add(sprite);

            sprite.Play("idle");
            Add(scaleWiggler = Wiggler.Create(0.3f, 3f, delegate (float f) {
                sprite.Scale = Vector2.One * (1f + f * wiggleIntensity);
            }));
            Add(bloom = new BloomPoint(bloomStrength, 20f)); // Larger bloom for cartridge
            Add(light = new VertexLight(particleColor, glowStrength * 0.6f, 40, 80)); // Larger light
            Add(hover = new SineWave(floatSpeed, 0f));
            hover.OnUpdate = delegate (float f) {
                Sprite obj = sprite;
                VertexLight vertexLight = light;
                float num2 = bloom.Y = f * floatRange;
                float num5 = obj.Y = (vertexLight.Y = num2);
            };

            if (IsGhost)
            {
                sprite.Color = Color.White * 0.7f;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Audio.Stop(remixSfx);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Audio.Stop(remixSfx);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update()
        {
            base.Update();
            
            // Special rainbow effect for chapter 19 finale
            if (isChapter19Finale && !collecting && !IsGhost)
            {
                float hue = (Engine.Scene.TimeActive * 0.3f) % 1.0f;
                Color rainbowColor = Calc.HsvToColor(hue, 0.9f, 1.0f);
                sprite.Color = Color.Lerp(Color.White, rainbowColor, 0.4f);
                light.Color = rainbowColor;
            }
            
            if (!collecting && Scene.OnInterval(0.08f)) // More frequent particles
            {
                SceneAs<Level>().Particles.Emit(P_Shine, 2, base.Center, new Vector2(16f, 12f));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void OnPlayer(global::Celeste.Player player)
        {
            if (!collected)
            {
                player?.RefillStamina();
                
                // Use custom audio if provided, otherwise use cartridge-specific sound
                string collectSfx = !string.IsNullOrEmpty(customAudio) 
                    ? customAudio 
                    : "event:/game/pusheen/general/cartridge_get";
                Audio.Play(collectSfx, Position);
                
                collected = true;
                CelesteGame.Freeze(0.15f); // Longer freeze for more impact
                Add(new Coroutine(CollectRoutine(player)));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator CollectRoutine(global::Celeste.Player player)
        {
            collecting = true;
            Level level = Scene as Level;
            TapeBlockManager cbm = Scene.Tracker.GetEntity<TapeBlockManager>();
            level.PauseLock = true;
            level.Frozen = true;
            Tag = Tags.FrozenUpdate;
            
            // Mark cartridge as collected and unlock remix extra
            level.Session.Cassette = true; // Reuse cassette flag for consistency
            level.Session.RespawnPoint = level.GetSpawnPoint(nodes[1]);
            level.Session.UpdateLevelStartDashes();
            
            if (!string.IsNullOrEmpty(remixExtraToUnlock))
                IngesteModule.SaveData.UnlockedRemixExtraIDs.Add(remixExtraToUnlock);
            
            cbm?.StopBlocks();
            Depth = -1000000;
            
            // More dramatic screen effects for cartridge
            level.Shake(0.3f);
            level.Flash(Color.Gold);
            level.Displacement.Clear();
            
            Vector2 camWas = level.Camera.Position;
            Vector2 camTo = (Position - new Vector2(160f, 90f)).Clamp(
                level.Bounds.Left - 64, level.Bounds.Top - 32, 
                level.Bounds.Right + 64 - 320, level.Bounds.Bottom + 32 - 180);
            level.Camera.Position = camTo;
            level.ZoomSnap((Position - level.Camera.Position).Clamp(60f, 60f, 260f, 120f), 2.2f); // Slightly more zoom
            
            sprite.Play("spin", restart: true);
            sprite.Rate = 1.5f; // Slower initial spin for more grandeur
            
            for (float p3 = 0f; p3 < 2.0f; p3 += Engine.DeltaTime) // Longer spin duration
            {
                sprite.Rate += Engine.DeltaTime * 3f;
                yield return null;
            }
            
            sprite.Rate = 0f;
            sprite.SetAnimationFrame(0);
            scaleWiggler.Start();
            yield return 0.4f; // Longer pause for impact
            
            Vector2 from = Position;
            Vector2 to = new Vector2(X, level.Camera.Top - 20f); // Higher exit point
            float duration2 = 0.6f; // Slower exit animation
            
            for (float p3 = 0f; p3 < 1f; p3 += Engine.DeltaTime / duration2)
            {
                sprite.Scale.X = MathHelper.Lerp(1f, 0.05f, p3); // More dramatic scale change
                sprite.Scale.Y = MathHelper.Lerp(1f, 4f, p3);
                Position = Vector2.Lerp(from, to, Ease.CubeIn(p3));
                yield return null;
            }

            Visible = false;
            
            // Play remix extra preview music
            remixSfx = Audio.Play("event:/game/pusheen/general/cartridge_preview");
            
            UnlockedRemixExtra message = new UnlockedRemixExtra(unlockText, menuSprite);
            Scene.Add(message);
            yield return message.EaseIn();
            
            // Handle multi-page text
            while (message.textIndex < message.text.Length)
            {
                while (!Input.MenuConfirm.Pressed)
                {
                    yield return null;
                }
                if (message.textIndex != message.text.Length - 1)
                    yield return message.NextText();
                else
                    break;
            }

            Audio.SetParameter(remixSfx, "end", 1f);
            yield return message.EaseOut();
            
            duration2 = 0.4f;
            Add(new Coroutine(level.ZoomBack(duration2 - 0.05f)));
            
            for (float p3 = 0f; p3 < 1f; p3 += Engine.DeltaTime / duration2)
            {
                level.Camera.Position = Vector2.Lerp(camTo, camWas, Ease.SineInOut(p3));
                yield return null;
            }

            if (!player.Dead && nodes != null && nodes.Length >= 2)
            {
                Audio.Play("event:/game/general/cassette_bubblereturn", level.Camera.Position + new Vector2(160f, 90f));
                player.StartCassetteFly(nodes[1], nodes[0]);
            }

            foreach (SandwichLava item in level.Entities.FindAll<SandwichLava>())
            {
                item.Leave();
            }

            level.Frozen = false;
            yield return 0.4f; // Longer pause before cleanup
            cbm?.Finish();
            level.PauseLock = false;
            level.ResetZoom();
            
            // Execute custom onCollect action if specified
            ExecuteOnCollectAction(level, player);
            
            // Special handling for chapter 19 finale
            if (isChapter19Finale)
            {
                ExecuteChapter19Finale(level, player);
            }
            
            RemoveSelf();
        }

        // Execute custom onCollect action
        private void ExecuteOnCollectAction(Level level, global::Celeste.Player player)
        {
            if (string.IsNullOrEmpty(onCollect)) return;

            switch (onCollect.ToLower())
            {
                case "unlock_remix_mode":
                    // Unlock special remix mode
                    level.Session.SetFlag("cartridge_remix_mode", true);
                    break;
                case "activate_finale":
                    // Activate finale sequence
                    level.Session.SetFlag("cartridge_finale", true);
                    break;
                case "unlock_secret_area":
                    // Unlock secret area access
                    level.Session.SetFlag("cartridge_secret_area", true);
                    break;
                case "trigger_ending":
                    // Trigger special ending sequence
                    level.Session.SetFlag("cartridge_ending", true);
                    break;
                case "complete_chapter19":
                    // Mark chapter 19 as completed
                    level.Session.SetFlag("chapter19_complete", true);
                    break;
                case "unlock_developer_commentary":
                    // Unlock developer commentary mode
                    level.Session.SetFlag("cartridge_dev_commentary", true);
                    break;
                case "activate_rainbow_mode":
                    // Activate rainbow/celebration mode
                    level.Session.SetFlag("cartridge_rainbow_mode", true);
                    break;
                case "set_flag":
                    // Set a custom session flag
                    level.Session.SetFlag("cartridge_collected", true);
                    break;
                case "complete_level":
                    // Complete the current level
                    level.CompleteArea(true, false, false);
                    break;
                case "custom_script":
                    // Custom script execution point
                    break;
            }
        }

        // Special handling for chapter 19 finale
        private void ExecuteChapter19Finale(Level level, global::Celeste.Player player)
        {
            // Set special flags for chapter 19 completion
            level.Session.SetFlag("chapter19_cartridge_collected", true);
            level.Session.SetFlag("remix_extra_unlocked", true);
            
            // Trigger any special finale effects
            IngesteModule.SaveData.Chapter19Complete = true;
            
            // Add completion statistics or achievements here if needed
        }

        public static void LoadParticles()
        {
            P_Shine = new ParticleType
            {
                Color = Calc.HexToColor("FFD700"), // Gold color
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Fade,
                Size = 1.2f, // Slightly larger particles
                LifeMin = 0.8f,
                LifeMax = 1.5f,
                SpeedMin = 12f,
                SpeedMax = 25f,
                DirectionRange = (float)Math.PI * 2f
            };

            P_Collect = new ParticleType
            {
                Color = Calc.HexToColor("FFA500"), // Orange-gold color
                Color2 = Calc.HexToColor("FFD700"),
                ColorMode = ParticleType.ColorModes.Blink,
                Size = 1.5f,
                LifeMin = 1.0f,
                LifeMax = 2.0f,
                SpeedMin = 20f,
                SpeedMax = 40f,
                DirectionRange = (float)Math.PI * 2f
            };
        }

        // Determine unlock text based on remix extra being unlocked
        private string[] DetermineUnlockText(string remixToUnlock)
        {
            if (string.IsNullOrEmpty(remixToUnlock))
            {
                return new string[] { "Maggy_RemixExtra_unlocked" };
            }

            // Check if it's a specific remix extra unlock
            if (remixToUnlock.ToUpper().Contains("CHAPTER19") || remixToUnlock.ToUpper().Contains("FINALE"))
            {
                return new string[] { 
                    "Chapter_19_Complete",
                    "Remix_Extra_Unlocked", 
                    "Congratulations_on_completion" 
                };
            }
            else if (remixToUnlock.ToUpper().Contains("BONUS") || remixToUnlock.ToUpper().Contains("EXTRA"))
            {
                return new string[] { "Bonus_Content_Unlocked" };
            }
            else if (remixToUnlock.ToUpper().Contains("SECRET"))
            {
                return new string[] { "Secret_Area_Unlocked" };
            }
            else if (remixToUnlock.ToUpper().Contains("DEVELOPER") || remixToUnlock.ToUpper().Contains("DEV"))
            {
                return new string[] { "Developer_Content_Unlocked" };
            }

            // Default to remix extra
            return new string[] { "Maggy_RemixExtra_unlocked" };
        }
    }
}

