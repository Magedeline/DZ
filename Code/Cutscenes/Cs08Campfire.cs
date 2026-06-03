using System.Xml;
using Celeste.Entities;
using Celeste.NPCs;
using BadelineDummy = Celeste.Entities.BadelineDummy;

namespace Celeste.Cutscenes
{
    [HotReloadable]
    public class Cs08Campfire : CutsceneEntity
    {
        #region Constants
        public const string FLAG = "campfire_chat_mod";
        public const string CH8_PLAT_FLAG = "ch8_plat";
        public const string DUSK_BACKGROUND_FLAG = "duskbg";
        public const string STARS_BACKGROUND_FLAG = "starsbg";

        private static readonly Vector2 CAMERA_START_OFFSET = new(0f, -144f);
        private static readonly Vector2 PLAYER_OFFSET = new(24f, 16f);
        private static readonly Vector2 MADELINE_OFFSET = new(-20f, -16f);
        private static readonly Vector2 RESPAWN_POINT = new(-176f, 312f);
        
        // NPC positioning offsets for campfire scene
        private static readonly Vector2 MAGOLOR_OFFSET = new(-50f, -16f);
        private static readonly Vector2 BADELINE_OFFSET = new(50f, -16f);
        private static readonly Vector2 RALSEI_OFFSET = new(-80f, -16f);

        private const float LIGHT_APPROACH_SPEED = 2f;
        private const float DIALOG_OPTION_WIDTH = 1400f;
        private const float DIALOG_OPTION_HEIGHT = 140f;
        private const float DIALOG_OPTION_EASE_SPEED = 4f;
        private const string music_event = "event:/music/pusheen/lvl8/intro";
        private const string collapse_audio_event = "event:/pusheen/sfx/player_collapse";
        #endregion

        #region Fields
        private readonly global::Celeste.Player player;
        private readonly CelesteNPC madeline;
        private Bonfire bonfire;
        private PlateauMod plateau;
        private readonly Vector2 cameraStart;
        private readonly Vector2 playerCampfirePosition;
        private readonly Vector2 madelineCampfirePosition;
        
        // Additional NPCs for campfire scene
        private BadelineDummy badeline;
        private RalseiDummy ralsei;
        
        private Dictionary<string, Option[]> dialogNodes;
        private HashSet<Question> askedQuestions;
        private List<Option> currentOptions;
        private float optionEase;
        private int currentOptionIndex;
        private Selfie selfie;
        #endregion

        public Cs08Campfire(Npc08MadelinePlateau madelineNPC, global::Celeste.Player player, CelesteNPC madeline) 
            : base(true, false)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.madeline = madeline ?? throw new ArgumentNullException(nameof(madeline));

            // Initialize positions
            cameraStart = player.Position;
            playerCampfirePosition = player.Position + PLAYER_OFFSET;
            madelineCampfirePosition = madeline.Position + MADELINE_OFFSET;

            // Initialize dialog system
            InitializeDialogSystem();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            badeline = scene.Entities.FindFirst<BadelineDummy>();
            ralsei = scene.Entities.FindFirst<RalseiDummy>();
            
            // Find other entities
            bonfire = scene.Entities.FindFirst<Bonfire>();
            plateau = scene.Entities.FindFirst<PlateauMod>();
        }

        public override void OnBegin(Level level)
        {
            if (ShouldSkipCutscene(level))
            {
                WasSkipped = true;
                SkipCutscene(level);
                return;
            }
            InitializeLevel(level);
            Add(new Coroutine(CutsceneSequence(level)));
        }

        private bool ShouldSkipCutscene(Level level)
        {
            // Removed TasSettings.Enabled check as it is not defined
            return level.Session.GetFlag(FLAG) || 
                   (level.Session.RespawnPoint != RESPAWN_POINT);
        }

        private new void SkipCutscene(Level level)
        {
            EndCutscene(level);
            WasSkipped = true;
        }

        private void InitializeLevel(Level level)
        {
            // Reset audio and visual settings
            Audio.SetMusic(null, false, false);
            level.SnapColorGrade(null);
            level.Bloom.Base = 0.0f;
            level.Session.SetFlag(DUSK_BACKGROUND_FLAG);

            // Setup camera
            level.Camera.Position = new Vector2(level.Bounds.Left, 
                bonfire?.Y + CAMERA_START_OFFSET.Y ?? level.Camera.Y);
            level.ZoomSnap(new Vector2(80f, 120f), 2f);

            // Setup player state
            player.Light.Alpha = 0.0f;
            player.X = level.Bounds.Left - 40;
            player.StateMachine.State = global::Celeste.Player.StDummy;
            player.StateMachine.Locked = true;
        }

        private IEnumerator CutsceneSequence(Level level)
        {
            // Start light fade
            Add(new Coroutine(PlayerLightApproach()));

            // Camera movement
            var camTo = new Coroutine(CameraTo(
                new Vector2(level.Camera.X + 90f, level.Camera.Y), 
                6f, Ease.CubeIn));
            Add(camTo);

            // Player walk sequence
            yield return PlayerWalkSequence();

            // Transition to campfire scene
            yield return CampfireTransition(level);

            // Dialog sequence
            Audio.SetMusic(music_event);
            yield return DialogSequence();

            // End sequence
            yield return EndingTransition(level);
            EndCutscene(level);
        }

        private IEnumerator PlayerWalkSequence()
        {
            player.DummyAutoAnimate = false;
            player.Sprite.Play("carryMaddyWalk");

            // Walk animation
            for (float t = 0f; t < 3.5f; t += Engine.DeltaTime)
            {
                SpotlightWipe.FocusPoint = new Vector2(40f, 120f);
                player.NaiveMove(Vector2.UnitX * 32f * Engine.DeltaTime);
                yield return null;
            }

            // Collapse sequence
            player.Sprite.Play("carryMaddyCollapse");
            Audio.Play(collapse_audio_event, player.Position);
            yield return 0.3f;

            // Effects
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        }

        /// <summary>
        /// Transition to the campfire scene with fade and repositioning
        /// </summary>
        private IEnumerator CampfireTransition(Level level)
        {
            // player.Freeze(); // Removed, not available. Use Locked state instead.
            player.StateMachine.Locked = true;

            // Fade out
            var fadeOut = new FadeWipe(level, false);
            fadeOut.Duration = 1.5f;
            fadeOut.EndTimer = 2.5f;
            yield return fadeOut.Wait();

            // Bonfire lighting
            bonfire.SetMode(Bonfire.Mode.Lit);
            yield return 2.45f;

            // Camera repositioning
            level.Camera.Position = player.CameraTarget;
            player.Position = playerCampfirePosition;
            player.Facing = Facings.Left;
            player.Sprite.Play("asleep");
            
            if (badeline != null)
            {
                badeline.Position = bonfire.Position + BADELINE_OFFSET;
                if (badeline.Sprite != null)
                    badeline.Sprite.Scale.X = -1f; // Face left
            }
            
            if (ralsei != null)
            {
                ralsei.Position = bonfire.Position + RALSEI_OFFSET;
                if (ralsei.Sprite != null)
                    ralsei.Sprite.Scale.X = 1f; // Face right
            }

            // Background adjustment
            level.Session.SetFlag("starsbg");
            level.Session.SetFlag("duskbg", false);
            fadeOut.EndTimer = 0.0f;
            var fadeIn = new FadeWipe(level, true);
            yield return null;
            level.ResetZoom();
            level.Camera.Position = new Vector2(bonfire.X - 160f, bonfire.Y - 140f);
            yield return 3f;
            Audio.SetMusic("event:/music/pusheen/lvl8/intro");
            yield return 1.5f;
            // Particle effects
            Level.Particles.Emit(NPC01_Theo.P_YOLO, 4, madeline.Position + new Vector2(-4f, -14f), Vector2.One * 3f);
            yield return 1f;
            player.Sprite.Play("halfWakeUp");
            yield return 0.25f;
        }

        private IEnumerator DialogSequence()
        {
            // Initial dialog display
            yield return Textbox.Say("MAGGYHELPER_CH8_MADELINE_INTRO");
            string key = "start";
            while (!string.IsNullOrEmpty(key) && dialogNodes.ContainsKey(key))
            {
                currentOptionIndex = 0;
                currentOptions = new List<Option>();
                foreach (Option option in dialogNodes[key])
                {
                    if (option.CanAsk(askedQuestions))
                        currentOptions.Add(option);
                }
                if (currentOptions.Count > 0)
                {
                    Audio.Play("event:/ui/game/chatoptions_appear");
                    while ((optionEase += Engine.DeltaTime * DIALOG_OPTION_EASE_SPEED) < 1.0)
                        yield return null;
                    optionEase = 1f;
                    yield return 0.25f;
                    while (!Input.MenuConfirm.Pressed)
                    {
                        if (Input.MenuUp.Pressed && currentOptionIndex > 0)
                        {
                            Audio.Play("event:/ui/game/chatoptions_roll_up");
                            --currentOptionIndex;
                        }
                        else if (Input.MenuDown.Pressed && currentOptionIndex < currentOptions.Count - 1)
                        {
                            Audio.Play("event:/ui/game/chatoptions_roll_down");
                            ++currentOptionIndex;
                        }
                        yield return null;
                    }
                    Audio.Play("event:/ui/game/chatoptions_select");
                    while ((optionEase -= Engine.DeltaTime * DIALOG_OPTION_EASE_SPEED) > 0.0)
                        yield return null;
                    Option selected = currentOptions[currentOptionIndex];
                    askedQuestions.Add(selected.Question);
                    currentOptions = null;
                    yield return Textbox.Say(selected.Question.Answer, WaitABit, BeerSequence);
                    key = selected.Goto;
                    if (!string.IsNullOrEmpty(key))
                        selected = null;
                    else
                        break;
                }
                else
                    break;
            }
        }

        private IEnumerator EndingTransition(Level level)
        {
            var fadeWipe = new FadeWipe(level, false);
            fadeWipe.Duration = 3f;
            yield return fadeWipe.Wait();
        }

        private IEnumerator WaitABit()
        {
            yield return 0.8f;
        }

        private IEnumerator BeerSequence()
        {
            yield return 0.5f;
        }

        private IEnumerator PlayerLightApproach()
        {
            while (player.Light.Alpha < 1.0)
            {
                player.Light.Alpha = Calc.Approach(player.Light.Alpha, 1f, Engine.DeltaTime * LIGHT_APPROACH_SPEED);
                yield return null;
            }
        }

        public override void OnEnd(Level level)
        {
            if (!WasSkipped)
            {
                FinalizeNormalEnd(level);
            }

            CleanupCutscene(level);
        }

        private void FinalizeNormalEnd(Level level)
        {
            level.ZoomSnap(new Vector2(160f, 120f), 2f);
            var fadeWipe = new FadeWipe(level, true) { Duration = 3f };
            var zoom = new Coroutine(level.ZoomBack(fadeWipe.Duration));
            fadeWipe.OnUpdate = f => zoom.Update();
        }

        private void CleanupCutscene(Level level)
        {
            selfie?.RemoveSelf();
            SetupFinalState(level);
            RemoveSelf();
        }

        private void SetupFinalState(Level level)
        {
            // Set session flags
            level.Session.SetFlag(FLAG, true);
            level.Session.SetFlag(CH8_PLAT_FLAG, true);
            level.Session.SetFlag(STARS_BACKGROUND_FLAG, false);
            level.Session.SetFlag(DUSK_BACKGROUND_FLAG, false);
            level.Session.Dreaming = true;

            // Setup entities
            level.Add(new StarJumpController());
            SetupBonfireState();
            SetupCharacterStates();

            // Final camera position
            level.Camera.Position = player.CameraTarget;
        }

        private void SetupBonfireState()
        {
            if (bonfire != null)
            {
                bonfire.Activated = false;
                bonfire.SetMode(Bonfire.Mode.Lit);
            }
        }

        private void SetupCharacterStates()
        {
            // Setup Madeline
            if (madeline != null && madeline.Sprite != null)
            {
                madeline.Sprite.Play("sleep");
                madeline.Sprite.SetAnimationFrame(madeline.Sprite.CurrentAnimationTotalFrames - 1);
                madeline.Sprite.Scale.X = 1f;
                madeline.Position = madelineCampfirePosition;
            }

            // Setup Player
            if (player != null)
            {
                player.Sprite.Play("asleep");
                player.Position = playerCampfirePosition;
                player.StateMachine.Locked = false;
                player.StateMachine.State = WasSkipped ? 0 : global::Celeste.Player.StDummy;
                player.Speed = Vector2.Zero;
                player.Facing = Facings.Left;
            }
        }

        #region Dialog System
        /// <summary>
        /// Initialize the dialogue system with all questions and options
        /// </summary>
        private void InitializeDialogSystem()
        {
            dialogNodes = new Dictionary<string, Option[]>();
            askedQuestions = new HashSet<Question>();
            currentOptions = new List<Option>();

            // Create questions with new dialogue options
            var questions = CreateDialogueQuestions();
            SetupDialogueNodes(questions);
        }

        /// <summary>
        /// Create all dialogue questions including new ones
        /// </summary>
        private Dictionary<string, Question> CreateDialogueQuestions()
        {
            return new Dictionary<string, Question>
            {
                ["anything"] = new Question("anything"),
                ["zero"] = new Question("zero"),
                ["gaster"] = new Question("gaster"),
                ["badeline"] = new Question("badeline"),
                ["granny"] = new Question("granny"),
                ["void"] = new Question("void"),
                ["evilgod"] = new Question("evilgod"),
                ["monster"] = new Question("monster"),
                ["goal"] = new Question("goal"),
                ["grandpa"] = new Question("grandpa"),
                ["story"] = new Question("story"),
                ["tips"] = new Question("tips"),
                [nameof(selfie)] = new Question(nameof(selfie)),
                ["sleep"] = new Question("sleep"),
                ["sleep_confirm"] = new Question("sleep_confirm"),
                ["sleep_cancel"] = new Question("sleep_cancel"),
                ["memories"] = new Question("memories"),
                ["journey"] = new Question("journey"),
                ["fears"] = new Question("fears"),
                ["hope"] = new Question("hope")
            };
        }

        /// <summary>
        /// Setup dialogue node structure with new options
        /// </summary>
        private void SetupDialogueNodes(Dictionary<string, Question> questions)
        {
            dialogNodes.Add("start", new Option[]
            {
                new Option(questions["anything"], "start").ExcludedBy(questions["granny"]),
                new Option(questions["zero"], "start").Require(questions["goal"]),
                new Option(questions["goal"], "start").Require(questions["gaster"]),
                new Option(questions["grandpa"], "start").Require(questions["goal"], questions["granny"]),
                new Option(questions["story"], "start").Require(questions["grandpa"], questions["evilgod"]),
                new Option(questions["tips"], "start").Require(questions["story"]),
                new Option(questions["gaster"], "start"),
                new Option(questions["badeline"], "start").Require(questions["gaster"]),
                new Option(questions["granny"], "start").Require(questions["gaster"], questions["goal"]),
                new Option(questions["void"], "start").Require(questions["granny"]),
                new Option(questions["evilgod"], "start").Require(questions["void"]),
                new Option(questions["monster"], "start").Require(questions["void"]),
                new Option(questions[nameof(selfie)], "").Require(questions["evilgod"], questions["story"]),
                new Option(questions["sleep"], "sleep").Require(questions["granny"]).ExcludedBy(questions["evilgod"], questions["story"]).Repeatable(),
                new Option(questions["memories"], "start").Require(questions["badeline"]),
                new Option(questions["journey"], "start").Require(questions["memories"]),
                new Option(questions["fears"], "start").Require(questions["monster"]),
                new Option(questions["hope"], "start").Require(questions["journey"], questions["fears"])
            });

            dialogNodes.Add("sleep", new Option[]
            {
                new Option(questions["sleep_cancel"], "start").Repeatable(),
                new Option(questions["sleep_confirm"], "")
            });
        }

        private class Option
        {
            public Question Question;
            public string Goto;
            public List<Question> OnlyAppearIfAsked;
            public List<Question> DoNotAppearIfAsked;
            public bool CanRepeat;
            public float Highlight;
            public const float WIDTH = 1400f;
            public const float HEIGHT = 140f;
            public const float PADDING = 20f;
            public const float TEXT_SCALE = 0.7f;

            public Option(Question question, string go)
            {
                Question = question;
                Goto = go;
            }

            public Option Require(
                params Question[] onlyAppearIfAsked)
            {
                OnlyAppearIfAsked = new List<Question>(onlyAppearIfAsked);
                return this;
            }

            public Option ExcludedBy(
                params Question[] doNotAppearIfAsked)
            {
                DoNotAppearIfAsked = new List<Question>(doNotAppearIfAsked);
                return this;
            }

            public Option Repeatable()
            {
                CanRepeat = true;
                return this;
            }

            public bool CanAsk(HashSet<Question> asked)
            {
                if (!CanRepeat && asked.Contains(Question))
                    return false;
                if (OnlyAppearIfAsked != null)
                {
                    foreach (Question question in OnlyAppearIfAsked)
                    {
                        if (!asked.Contains(question))
                            return false;
                    }
                }
                if (DoNotAppearIfAsked != null)
                {
                    bool flag = true;
                    foreach (Question question in DoNotAppearIfAsked)
                    {
                        if (!asked.Contains(question))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                        return false;
                }
                return true;
            }

            public void Update() => Question.Portrait.Update();

            public void Render(Vector2 position, float ease)
            {
                float easeOut = Ease.CubeOut(ease);
                float amount = Ease.CubeInOut(Highlight);
                position.Y += (float)(-32.0 * (1.0 - easeOut));
                position.X += amount * 32f;
                Color color1 = Color.Lerp(Color.Gray, Color.White, amount) * easeOut;
                float alpha = MathHelper.Lerp(0.6f, 1f, amount) * easeOut;
                Color color2 = Color.White * (float)(0.5 + amount * 0.5);
                GFX.Portraits[Question.Textbox].Draw(position, Vector2.Zero, color1);
                Facings facings = Question.PortraitSide;
                if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                    facings = (Facings)(-(int)facings);
                float portraitSize = 100f;
                Question.Portrait.Scale = Vector2.One * (portraitSize / Question.PortraitSize);
                if (facings == Facings.Right)
                {
                    Question.Portrait.Position = position + new Vector2((float)(1380.0 - portraitSize * 0.5), 70f);
                    Question.Portrait.Scale.X *= -1f;
                }
                else
                    Question.Portrait.Position = position + new Vector2((float)(20.0 + portraitSize * 0.5), 70f);
                Question.Portrait.Color = color2 * easeOut;
                Question.Portrait.Render();
                float textVerticalPad = (float)((140.0 - ActiveFont.LineHeight * 0.699999988079071) / 2.0);
                Vector2 position1 = new Vector2(0.0f, position.Y + 70f);
                Vector2 justify = new Vector2(0.0f, 0.5f);
                if (facings == Facings.Right)
                {
                    justify.X = 1f;
                    position1.X = (float)(position.X + 1400.0 - 20.0) - textVerticalPad - portraitSize;
                }
                else
                    position1.X = position.X + 20f + textVerticalPad + portraitSize;
                Question.AskText.Draw(position1, justify, Vector2.One * 0.7f, alpha);
            }
        }

        private class Question
        {
            public readonly string Ask;
            public readonly string Answer;
            public readonly string Textbox;
            public readonly FancyText.Text AskText;
            public readonly Sprite Portrait;
            public readonly Facings PortraitSide;
            public readonly float PortraitSize;

            public Question(string id)
            {
                int maxLineWidth = 1828;
                Ask = "ch8_madeline_ask_" + id;
                Answer = "ch8_madeline_say_" + id;
                AskText = FancyText.Parse(Dialog.Get(Ask), maxLineWidth, -1);
                foreach (FancyText.Node node in AskText.Nodes)
                {
                    if (node is FancyText.Portrait)
                    {
                        FancyText.Portrait portrait = node as FancyText.Portrait;
                        Portrait = GFX.PortraitsSpriteBank.Create(portrait.SpriteId);
                        Portrait.Play(portrait.IdleAnimation);
                        PortraitSide = (Facings)portrait.Side;
                        Textbox = "textbox/" + portrait.Sprite + "_ask";
                        XmlElement xml = GFX.PortraitsSpriteBank.SpriteData[portrait.SpriteId].Sources[0].XML;
                        if (xml == null)
                            break;
                        PortraitSize = xml.AttrInt("size", 160);
                        break;
                    }
                }
            }
        }
        #endregion
    }
}
