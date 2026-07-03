namespace Celeste.Entities
{
    /// <summary>
    /// Character-themed refill crystals that restore dashes and provide character-specific effects.
    /// Each character (Kirby, Madeline, Badeline, etc.) has their own themed refill variant.
    /// </summary>
    [CustomEntity(ids: "DZ/CharacterRefill")]
    [Tracked]
    [HotReloadable]
    public class CharacterRefill : Entity
    {
        /// <summary>
        /// Character types for themed refills
        /// </summary>
        public enum CharacterType
        {
            Kirby,          // Pink star-shaped, grants float ability
            Madeline,       // Red hair-colored, standard refill
            Badeline,       // Purple themed, grants dream dash charge
            Theo,           // Blue/selfie themed
            Granny,         // Mountain elder themed
            Oshiro,         // Ghost resort themed
            Chara,          // Undertale red soul
            Frisk,          // Undertale yellow soul
            Ralsei,         // Deltarune green soul
            Asriel,         // Rainbow/angel themed
            MetaKnight,     // Dark blue sword themed
            KingDedede,     // Royal hammer themed
            Magolor,        // Lor Starcutter themed
            MageKirby,      // Magic/beam themed
            Custom          // For modders to extend
        }

        // Particle types per character
        public static readonly Dictionary<CharacterType, ParticleType> P_Shatter = new();
        public static readonly Dictionary<CharacterType, ParticleType> P_Regen = new();
        public static readonly Dictionary<CharacterType, ParticleType> P_Glow = new();

        // Components
        private Sprite sprite;
        private Sprite flash;
        private Wiggler wiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private SineWave sine;

        // State
        private Level level;
        private float respawnTimer;
        private bool collected;

        // Properties
        public CharacterType Character { get; private set; }
        public int DashCount { get; private set; }
        public bool OneUse { get; private set; }
        public bool GrantsSpecialAbility { get; private set; }
        public string CustomSpritePath { get; private set; }
        public string CustomSoundEvent { get; private set; }
        public bool CharacterModeOnly { get; private set; }
        public bool RefillStamina { get; private set; }

        static CharacterRefill()
        {
            InitializeParticles();
        }

        private static void InitializeParticles()
        {
            // Kirby - Pink/magenta particles
            CreateParticlesForCharacter(CharacterType.Kirby, 
                Color.HotPink, Color.LightPink, Color.MediumVioletRed);

            // Madeline - Red/auburn particles
            CreateParticlesForCharacter(CharacterType.Madeline,
                new Color(230, 100, 100), new Color(255, 150, 150), Color.Salmon);

            // Badeline - Purple/violet particles
            CreateParticlesForCharacter(CharacterType.Badeline,
                new Color(139, 69, 255), new Color(186, 140, 255), Color.BlueViolet);

            // Theo - Blue particles
            CreateParticlesForCharacter(CharacterType.Theo,
                Color.DodgerBlue, Color.LightSkyBlue, Color.DeepSkyBlue);

            // Granny - Silver/grey particles
            CreateParticlesForCharacter(CharacterType.Granny,
                Color.Silver, Color.LightGray, Color.DarkGray);

            // Oshiro - Ghostly green/teal particles
            CreateParticlesForCharacter(CharacterType.Oshiro,
                new Color(100, 200, 180), Color.Cyan, Color.Teal);

            // Chara - Red soul particles
            CreateParticlesForCharacter(CharacterType.Chara,
                Color.Red, Color.DarkRed, new Color(200, 50, 50));

            // Frisk - Yellow soul particles
            CreateParticlesForCharacter(CharacterType.Frisk,
                Color.Gold, Color.Yellow, Color.LightGoldenrodYellow);

            // Ralsei - Green soul particles
            CreateParticlesForCharacter(CharacterType.Ralsei,
                Color.LimeGreen, Color.LightGreen, Color.ForestGreen);

            // Asriel - Rainbow/white particles
            CreateParticlesForCharacter(CharacterType.Asriel,
                Color.White, Color.Gold, Color.LightCyan);

            // Meta Knight - Dark blue particles
            CreateParticlesForCharacter(CharacterType.MetaKnight,
                Color.DarkBlue, Color.MidnightBlue, new Color(30, 30, 120));

            // King Dedede - Royal blue/gold particles
            CreateParticlesForCharacter(CharacterType.KingDedede,
                Color.RoyalBlue, Color.Gold, Color.Navy);

            // Magolor - Purple/orange particles
            CreateParticlesForCharacter(CharacterType.Magolor,
                new Color(100, 50, 150), Color.Orange, Color.Purple);

            // Mage Kirby - Cyan/magic particles
            CreateParticlesForCharacter(CharacterType.MageKirby,
                Color.Cyan, Color.Aquamarine, Color.DarkCyan);
        }

        private static void CreateParticlesForCharacter(CharacterType character, 
            Color primary, Color secondary, Color tertiary)
        {
            P_Shatter[character] = new ParticleType
            {
                Source = GFX.Game["particles/triangle"],
                Color = primary,
                Color2 = secondary,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.5f,
                LifeMax = 1f,
                Size = 1f,
                SpeedMin = 100f,
                SpeedMax = 200f,
                Direction = -(float)Math.PI / 2f,
                DirectionRange = (float)Math.PI
            };

            P_Regen[character] = new ParticleType
            {
                Source = GFX.Game["particles/blob"],
                Color = primary,
                Color2 = tertiary,
                ColorMode = ParticleType.ColorModes.Choose,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.5f,
                LifeMax = 1f,
                Size = 0.7f,
                SpeedMin = 10f,
                SpeedMax = 20f,
                SpeedMultiplier = 0.1f,
                Direction = -(float)Math.PI / 2f,
                DirectionRange = (float)Math.PI / 4f
            };

            P_Glow[character] = new ParticleType
            {
                Source = GFX.Game["particles/blob"],
                Color = primary,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.InAndOut,
                LifeMin = 0.6f,
                LifeMax = 1f,
                Size = 0.5f,
                SpeedMin = 4f,
                SpeedMax = 8f,
                DirectionRange = (float)Math.PI * 2f
            };
        }

        public CharacterRefill(Vector2 position, CharacterType character, int dashCount = 1, bool oneUse = false) 
            : base(position)
        {
            Character = character;
            DashCount = Math.Max(1, dashCount);
            OneUse = oneUse;
            GrantsSpecialAbility = GetDefaultSpecialAbility(character);
            RefillStamina = true;

            Initialize();
        }

        public CharacterRefill(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Character = (CharacterType)data.Int("characterType", 0);
            DashCount = Math.Max(1, data.Int("dashCount", 1));
            OneUse = data.Bool("oneUse", false);
            GrantsSpecialAbility = data.Bool("grantsSpecialAbility", GetDefaultSpecialAbility(Character));
            CustomSpritePath = data.Attr("customSpritePath", "");
            CustomSoundEvent = data.Attr("customSoundEvent", "");
            CharacterModeOnly = data.Bool("characterModeOnly", false);
            RefillStamina = data.Bool("refillStamina", true);

            Initialize();
        }

        private bool GetDefaultSpecialAbility(CharacterType character)
        {
            return character switch
            {
                CharacterType.Kirby => true,      // Float ability
                CharacterType.Badeline => true,   // Dream dash
                CharacterType.Asriel => true,     // Angel wings
                CharacterType.MetaKnight => true, // Sword dash
                _ => false
            };
        }

        private void Initialize()
        {
            Collider = new Hitbox(16f, 16f, -8f, -8f);

            // Get sprite path based on character
            string spritePath = GetSpritePath();
            string spriteId = GetSpriteId();

            // Main sprite
            try
            {
                sprite = GFX.SpriteBank.Create(spriteId);
            }
            catch
            {
                // Fallback to default refill
                sprite = DZModule.SpriteBank.Create("refill");
            }
            sprite.Play("idle");
            Add(sprite);

            // Flash effect
            Add(flash = new Sprite(GFX.Game, spritePath));
            flash.Add("flash", "flash", 0.05f);
            flash.OnFinish = delegate { flash.Visible = false; };
            flash.CenterOrigin();
            flash.Visible = false;

            // Wiggler for collection effect
            Add(wiggler = Wiggler.Create(1f, 4f, v =>
            {
                sprite.Scale = Vector2.One * (1f + v * 0.2f);
            }));

            Add(new MirrorReflection());
            
            // Lighting based on character color
            Color lightColor = GetCharacterColor();
            Add(bloom = new BloomPoint(0.8f, 16f));
            Add(light = new VertexLight(lightColor, 1f, 16, 48));
            
            Add(sine = new SineWave(0.6f, 0f));
            sine.Randomize();

            UpdateY();
            Depth = -100;

            // Player collision
            Add(new PlayerCollider(OnPlayer));
        }

        private string GetSpritePath()
        {
            if (!string.IsNullOrEmpty(CustomSpritePath))
                return CustomSpritePath;

            return Character switch
            {
                CharacterType.Kirby => "objects/DZ/DZ/characterRefill/kirby/",
                CharacterType.Madeline => "objects/DZ/DZ/characterRefill/madeline/",
                CharacterType.Badeline => "objects/DZ/DZ/characterRefill/badeline/",
                CharacterType.Theo => "objects/DZ/DZ/characterRefill/theo/",
                CharacterType.Granny => "objects/DZ/DZ/characterRefill/granny/",
                CharacterType.Oshiro => "objects/DZ/DZ/characterRefill/oshiro/",
                CharacterType.Chara => "objects/DZ/DZ/characterRefill/chara/",
                CharacterType.Frisk => "objects/DZ/DZ/characterRefill/frisk/",
                CharacterType.Ralsei => "objects/DZ/DZ/characterRefill/ralsei/",
                CharacterType.Asriel => "objects/DZ/DZ/characterRefill/asriel/",
                CharacterType.MetaKnight => "objects/DZ/DZ/characterRefill/metaknight/",
                CharacterType.KingDedede => "objects/DZ/DZ/characterRefill/dedede/",
                CharacterType.Magolor => "objects/DZ/DZ/characterRefill/magolor/",
                CharacterType.MageKirby => "objects/DZ/DZ/characterRefill/magekirby/",
                _ => "objects/DZ/DZ/refill/"
            };
        }

        private string GetSpriteId()
        {
            return Character switch
            {
                CharacterType.Kirby => "characterRefillKirby",
                CharacterType.Madeline => "characterRefillMadeline",
                CharacterType.Badeline => "characterRefillBadeline",
                CharacterType.Theo => "characterRefillTheo",
                CharacterType.Granny => "characterRefillGranny",
                CharacterType.Oshiro => "characterRefillOshiro",
                CharacterType.Chara => "characterRefillChara",
                CharacterType.Frisk => "characterRefillFrisk",
                CharacterType.Ralsei => "characterRefillRalsei",
                CharacterType.Asriel => "characterRefillAsriel",
                CharacterType.MetaKnight => "characterRefillMetaKnight",
                CharacterType.KingDedede => "characterRefillDedede",
                CharacterType.Magolor => "characterRefillMagolor",
                CharacterType.MageKirby => "characterRefillMageKirby",
                _ => DashCount >= 2 ? "refillTwo" : "refill"
            };
        }

        private Color GetCharacterColor()
        {
            return Character switch
            {
                CharacterType.Kirby => Color.HotPink,
                CharacterType.Madeline => new Color(230, 100, 100),
                CharacterType.Badeline => new Color(139, 69, 255),
                CharacterType.Theo => Color.DodgerBlue,
                CharacterType.Granny => Color.Silver,
                CharacterType.Oshiro => new Color(100, 200, 180),
                CharacterType.Chara => Color.Red,
                CharacterType.Frisk => Color.Gold,
                CharacterType.Ralsei => Color.LimeGreen,
                CharacterType.Asriel => Color.White,
                CharacterType.MetaKnight => Color.DarkBlue,
                CharacterType.KingDedede => Color.RoyalBlue,
                CharacterType.Magolor => new Color(100, 50, 150),
                CharacterType.MageKirby => Color.Cyan,
                _ => Color.White
            };
        }

        private string GetTouchSoundEvent()
        {
            if (!string.IsNullOrEmpty(CustomSoundEvent))
                return CustomSoundEvent;

            return Character switch
            {
                CharacterType.Kirby => "event:/char/kirby/refill_touch",
                CharacterType.Badeline => "event:/game/05_mirror_temple/redbooster_dash",
                CharacterType.Chara => "event:/game/09_core/death_bump",
                CharacterType.Asriel => "event:/final_content/game/19_TheEnd/gigadiamond_touch",
                CharacterType.MetaKnight => "event:/game/general/sword_touch",
                _ => DashCount >= 2 ? "event:/game/general/diamond_touch" : "event:/game/general/diamond_touch"
            };
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update()
        {
            base.Update();

            // Check character mode restriction
            if (CharacterModeOnly)
            {
                string flagName = GetCharacterModeFlag();
                bool isCharacterMode = level?.Session.GetFlag(flagName) ?? false;
                Collidable = isCharacterMode && respawnTimer <= 0f;
                Visible = isCharacterMode;
            }
            else
            {
                Collidable = respawnTimer <= 0f;
            }

            if (respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    Respawn();
                }
            }
            else if (Scene.OnInterval(0.1f) && sprite.Visible)
            {
                var particleType = P_Glow.TryGetValue(Character, out var pt) ? pt : global::Celeste.Refill.P_Glow;
                level?.ParticlesFG.Emit(particleType, 1, Position, Vector2.One * 5f);
            }

            UpdateY();
            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;

            if (Scene.OnInterval(2f) && sprite.Visible)
            {
                flash.Play("flash", restart: true);
                flash.Visible = true;
            }
        }

        private string GetCharacterModeFlag()
        {
            return Character switch
            {
                CharacterType.Kirby => "kirby_mode",
                CharacterType.Badeline => "badeline_mode",
                CharacterType.Chara => "chara_mode",
                CharacterType.Ralsei => "ralsei_mode",
                CharacterType.Asriel => "asriel_mode",
                _ => $"{Character.ToString().ToLower()}_mode"
            };
        }

        private void Respawn()
        {
            if (!Collidable)
            {
                Collidable = true;
                sprite.Visible = true;
                wiggler.Start();

                var particleType = P_Regen.TryGetValue(Character, out var pt) ? pt : global::Celeste.Refill.P_Regen;
                Audio.Play("event:/game/general/diamond_return", Position);
                level?.ParticlesFG.Emit(particleType, 16, Position, Vector2.One * 2f);
            }
        }

        private void UpdateY()
        {
            float yOffset = sine.Value * 2f;
            sprite.Y = bloom.Y = yOffset;
            flash.Y = yOffset;
        }

        public override void Render()
        {
            if (sprite.Visible)
            {
                sprite.DrawOutline();
            }
            base.Render();
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            if (collected) return;

            // Grant dashes
            player.RefillDash();
            for (int i = 1; i < DashCount; i++)
            {
                if (player.Dashes < player.MaxDashes + DashCount - 1)
                    player.Dashes++;
            }

            // Refill stamina if enabled
            if (RefillStamina)
            {
                player.RefillStamina();
            }

            // Grant special abilities based on character
            if (GrantsSpecialAbility)
            {
                ApplySpecialAbility();
            }

            // Play character-specific sound
            Audio.Play(GetTouchSoundEvent(), Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

            collected = true;
            Collidable = false;

            Add(new Coroutine(RefillRoutine(player)));
        }

        private void ApplySpecialAbility()
        {
            switch (Character)
            {
                case CharacterType.Kirby:
                    level?.Session.SetFlag("has_float_ability", true);
                    level?.Session.SetFlag("kirby_refill_boost", true);
                    break;
                    
                case CharacterType.Badeline:
                    level?.Session.SetFlag("dream_dashDZ_CHarged", true);
                    break;
                    
                case CharacterType.Asriel:
                    level?.Session.SetFlag("angel_wings_active", true);
                    break;
                    
                case CharacterType.MetaKnight:
                    level?.Session.SetFlag("sword_dash_ready", true);
                    break;
                    
                case CharacterType.Chara:
                    level?.Session.SetFlag("determination_boost", true);
                    break;
                    
                case CharacterType.Ralsei:
                    level?.Session.SetFlag("heal_aura_active", true);
                    break;
            }
        }

        private IEnumerator RefillRoutine(global::Celeste.Player player)
        {
            CelesteGame.Freeze(0.05f);
            yield return null;

            level?.Shake();
            sprite.Visible = false;
            Depth = 8999;

            // Emit character-specific particles
            var particleType = P_Shatter.TryGetValue(Character, out var pt) ? pt : global::Celeste.Refill.P_Shatter;
            level?.ParticlesFG.Emit(particleType, 5, Position, Vector2.One * 4f, -(float)Math.PI / 2f);
            level?.ParticlesFG.Emit(particleType, 5, Position, Vector2.One * 4f, (float)Math.PI / 2f);

            SlashFx.Burst(Position, 0f);

            if (OneUse)
            {
                RemoveSelf();
            }
            else
            {
                respawnTimer = 2.5f;
                collected = false;
            }
        }
    }
}

