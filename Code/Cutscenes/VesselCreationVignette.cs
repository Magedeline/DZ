#nullable enable

using Celeste;
using FMOD.Studio;
using Microsoft.Xna.Framework.Input;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Vessel Creation Vignette - Interactive vessel creation cutscene 
    /// Based on Hollow Knight/Undertale inspired vessel creation sequence
    /// </summary>
    public class VesselCreationVignette : Scene
    {
        #region Constants
        private const float FADE_DURATION = 2f;
        private const float TEXT_FADE_SPEED = 3f;
        private const float CHOICE_EASE_SPEED = 4f;
        private const float VESSEL_DISPLAY_TIME = 3f;
        private const float WHITE_FADE_DURATION = 4f;
        private const float TEXT_INPUT_SCALE = 0.7f;
        private const int NAME_INPUT_MAX_LENGTH = 24;
        private const int FEELING_INPUT_MAX_LENGTH = 64;
        private const float TEXT_INPUT_BOX_WIDTH = 860f;
        private const float TEXT_INPUT_BOX_HEIGHT = 78f;
        
        // Graphics paths
        private const string VESSEL_GRAPHICS_PATH = "bgs/maggy/00/anotherhuman/";
        private const string GONERBODY_BASE  = "IMAGE_GONERBODY";   // 6 variants: 00-05
        private const string GONERHEAD_BASE  = "IMAGE_GONERHEAD";   // 8 variants: 00-07
        private const string GONERLEGS_BASE  = "IMAGE_GONERLEGS";   // 5 variants: 00-04
        private const string DEPTH_SPRITE = "IMAGE_DEPTH";
        private const string SOUL_BLUR_SPRITE = "IMAGE_SOUL_BLUR";
        private const string PINK_SOUL_BLUR_SPRITE = "IMAGE_PINK_SOUL_BLUR";
        
        // Audio events
        private const string CREATION_MUSIC_EVENT = "event:/music/pusheen/lvl0/creation/create";
        private const string DRONE_MUSIC_EVENT = "event:/music/pusheen/lvl0/creation/drone";
        private const string HEART_APPEAR_EVENT = "event:/music/pusheen/lvl0/creation/heart_appear";
        private const string HEART_CHANGE_EVENT = "event:/music/pusheen/lvl0/creation/heart_change";
        private const string CHOICE_APPEAR_EVENT = "guid://{c4d2289a-859e-42e9-ba38-eee1791a3a62}";
        private const string CHOICE_SELECT_EVENT = "guid://{bc008f9e-76c5-42d5-b2f0-574cb7dacf6f}";
        private const string CHOICE_MOVE_EVENT = "guid://{dd4cb027-0132-4039-9303-c9ad8589683e}";
        #endregion

        #region Vessel Creation Data
        // Part labels are generated dynamically from loaded texture counts
        private string selectedLeg = "";
        private string selectedTorso = "";
        private string selectedHead = "";
        private int selectedLegIndex   = 0;
        private int selectedBodyIndex  = 0;
        private int selectedHeadIndex  = 0;
        private string vesselName = "";
        private string playerFeeling = "";
        private string creatorName = "";
        private bool isHonestAnswer = false;
        #endregion

        #region Fields
        private Session session;
        private string? areaMusic;
        private float backgroundFade = 1f;
        private float textAlpha = 0f;
        private float choiceEase = 0f;
        private bool exiting = false;
        private float pauseFade = 0f;
        private float fade = 0f;
        
        // UI Components
        private TextMenu? pauseMenu;
        private HudRenderer hud;
        private Coroutine? sequenceCoroutine;
        
        // Current state
        private CreationPhase currentPhase = CreationPhase.Introduction;
        private int currentChoiceIndex = 0;
        private List<string> currentChoices = new List<string>();
        private bool textInputActive = false;
        private bool textInputPaletteActive = false;
        private string textInputPrompt = string.Empty;
        private string textInputValue = string.Empty;
        private int textInputCursorIndex = 0;
        private int textInputSelectionAnchor = 0;
        private int textInputMaxLength = NAME_INPUT_MAX_LENGTH;
        private int textInputPaletteRow = 0;
        private int textInputPaletteColumn = 0;
        private float textInputEase = 0f;
        private Action<string>? textInputOnComplete;
        
        // Graphics â€“ indexed arrays, one entry per sprite variant
        private MTexture[] gonerBodyTextures = Array.Empty<MTexture>();
        private MTexture[] gonerHeadTextures = Array.Empty<MTexture>();
        private MTexture[] gonerLegTextures  = Array.Empty<MTexture>();
        private MTexture? depthTexture;
        private MTexture? soulBlurTexture;
        private MTexture? pinkSoulBlurTexture;
        private float vesselAlpha = 0f;
        private float soulBlurAlpha = 0f;
        private Vector2 vesselPosition;
        
        // Audio handle for creation music EventInstance
        private EventInstance? creationMusic;
        private EventInstance? droneMusic;

        // Vessel part cycler state (Deltarune-style Left/Right navigation)
        private bool vesselCyclerActive = false;
        private int vesselCyclerCount = 0;

        private static readonly string[][] TextInputPaletteRows =
        {
            new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" },
            new[] { "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T" },
            new[] { "U", "V", "W", "X", "Y", "Z", "0", "1", "2", "3" },
            new[] { "4", "5", "6", "7", "8", "9", "Space", "-", "'", "." },
            new[] { "Back", "Del", "Clear", "OK" }
        };
        
        public bool CanPause => pauseMenu == null;
        #endregion

        #region Enums
        private enum CreationPhase
        {
            Introduction,
            LegSelection,
            TorsoSelection, 
            HeadSelection,
            VesselNaming,
            HonestQuestion,
            FeelingsQuestion,
            CreatorNaming,
            VesselDisplay,
            VesselDiscard,
            Transition
        }
        #endregion

        #region Constructor
        public VesselCreationVignette(Session session) : base()
        {
            IngesteLogger.Info("[VesselCreation] Initializing VesselCreationVignette");
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            
            // Store and clear current music
            areaMusic = session.Audio.Music.Event;
            IngesteLogger.Debug($"[VesselCreation] Stored area music: {areaMusic}");
            session.Audio.Music.Event = null;
            session.Audio.Apply(forceSixteenthNoteHack: false);
            
            // Load vessel graphics
            loadVesselGraphics();
            
            // Initialize vessel position (center of screen)
            vesselPosition = new Vector2(Engine.Width / 2f, Engine.Height / 2f);
            IngesteLogger.Debug($"[VesselCreation] Vessel position initialized: {vesselPosition}");
            
            // Initialize UI
            Add(hud = new HudRenderer());
            RendererList.UpdateLists();
            
            // Start creation sequence
            sequenceCoroutine = new Coroutine(vesselCreationSequence());
            IngesteLogger.Info("[VesselCreation] Creation sequence coroutine started");
            
            // Add fade-in effect
            Add(new FadeWipe(this, true));
            IngesteLogger.Debug("[VesselCreation] Fade-in effect added");
        }
        #endregion

        #region Main Sequence
        private IEnumerator vesselCreationSequence()
        {
            IngesteLogger.Info("[VesselCreation] === Starting vessel creation sequence ===");
            
            // Phase 1: Introduction
            yield return introductionPhase();
            
            // Phase 2: Vessel Creation (Legs, Torso, Head)
            yield return legSelectionPhase();
            yield return torsoSelectionPhase();
            yield return headSelectionPhase();
            IngesteLogger.Info($"[VesselCreation] Vessel parts selected - Legs: {selectedLeg}, Torso: {selectedTorso}, Head: {selectedHead}");
            
            // Phase 3: Vessel Naming
            yield return vesselNamingPhase();
            
            // Phase 4: Truth Question
            yield return honestQuestionPhase();
            
            // Phase 5: Feelings Question
            yield return feelingsQuestionPhase();
            
            // Phase 6: Creator Naming
            yield return creatorNamingPhase();
            IngesteLogger.Info($"[VesselCreation] All choices collected - Vessel: '{vesselName}', Creator: '{creatorName}', Honest: {isHonestAnswer}");
            
            // Phase 7: Display Created Vessel
            yield return vesselDisplayPhase();
            
            // Phase 8: Vessel Discard
            yield return vesselDiscardPhase();
            
            // Phase 9: Transition to IntroVignette
            yield return transitionToIntroVignette();
        }
        #endregion

        #region Creation Phases
        private IEnumerator introductionPhase()
        {
            currentPhase = CreationPhase.Introduction;
            IngesteLogger.Info("[VesselCreation] Phase: Introduction");
            
            // Audio sequence: drone -> heart appear -> heart change -> creation music
            try { droneMusic = Audio.Play(DRONE_MUSIC_EVENT); IngesteLogger.Debug("[VesselCreation] Drone music started"); }
            catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to start drone music: {ex.Message}"); }
            
            yield return 1f;
            
            try { Audio.Play(HEART_APPEAR_EVENT); IngesteLogger.Debug("[VesselCreation] Heart appear sound played"); }
            catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play heart appear: {ex.Message}"); }
            
            yield return 0.75f;
            
            try { Audio.Play(HEART_CHANGE_EVENT); IngesteLogger.Debug("[VesselCreation] Heart change sound played"); }
            catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play heart change: {ex.Message}"); }
            
            yield return 0.75f;
            
            try { creationMusic = Audio.Play(CREATION_MUSIC_EVENT); IngesteLogger.Debug($"[VesselCreation] Creation music started: {CREATION_MUSIC_EVENT}"); }
            catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to start creation music: {ex.Message}"); }
            
            yield return 0.5f;
            
            // Display introduction text
            yield return showText("VESSEL_CREATION_INTRO");
            
            yield return 2f;
        }

        private IEnumerator legSelectionPhase()
        {
            currentPhase = CreationPhase.LegSelection;
            IngesteLogger.Info("[VesselCreation] Phase: Leg Selection");

            // Fade the vessel in before the prompt â€” Deltarune shows the vessel throughout selection
            yield return fadeVesselIn();

            yield return showText("VESSEL_CREATION_LEG_CHOICE");
            try { Audio.Play(HEART_CHANGE_EVENT); }
            catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play heart change: {ex.Message}"); }

            // Left/Right cycler with live vessel preview (Deltarune-style)
            yield return showVesselPartCycleSelector(
                gonerLegTextures.Length > 0 ? gonerLegTextures.Length : 1,
                () => selectedLegIndex,
                idx => { selectedLegIndex = idx; selectedLeg = $"Legs {idx}"; }
            );

            IngesteLogger.Debug($"[VesselCreation] Legs selected: {selectedLeg} (index {selectedLegIndex})");
            yield return 0.5f;
        }

        private IEnumerator torsoSelectionPhase()
        {
            currentPhase = CreationPhase.TorsoSelection;
            IngesteLogger.Info("[VesselCreation] Phase: Torso Selection");

            yield return showText("VESSEL_CREATION_TORSO_CHOICE");
            try { Audio.Play(HEART_CHANGE_EVENT); }
            catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play heart change: {ex.Message}"); }

            yield return showVesselPartCycleSelector(
                gonerBodyTextures.Length > 0 ? gonerBodyTextures.Length : 1,
                () => selectedBodyIndex,
                idx => { selectedBodyIndex = idx; selectedTorso = $"Body {idx}"; }
            );

            IngesteLogger.Debug($"[VesselCreation] Body selected: {selectedTorso} (index {selectedBodyIndex})");
            yield return 0.5f;
        }

        private IEnumerator headSelectionPhase()
        {
            currentPhase = CreationPhase.HeadSelection;
            IngesteLogger.Info("[VesselCreation] Phase: Head Selection");

            yield return showText("VESSEL_CREATION_HEAD_CHOICE");
            try { Audio.Play(HEART_CHANGE_EVENT); }
            catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play heart change: {ex.Message}"); }

            yield return showVesselPartCycleSelector(
                gonerHeadTextures.Length > 0 ? gonerHeadTextures.Length : 1,
                () => selectedHeadIndex,
                idx => { selectedHeadIndex = idx; selectedHead = $"Head {idx}"; }
            );

            IngesteLogger.Debug($"[VesselCreation] Head selected: {selectedHead} (index {selectedHeadIndex})");
            yield return 0.5f;
        }

        private IEnumerator vesselNamingPhase()
        {
            currentPhase = CreationPhase.VesselNaming;
            IngesteLogger.Info("[VesselCreation] Phase: Vessel Naming");
            
            yield return showText("VESSEL_CREATION_NAME_PROMPT");
            yield return showTextInput("Enter your vessel's name:", (name) => 
            {
                vesselName = name;
                IngesteLogger.Info($"[VesselCreation] Vessel named: '{vesselName}'");
            }, NAME_INPUT_MAX_LENGTH);
            
            yield return 1f;
        }

        private IEnumerator honestQuestionPhase()
        {
            currentPhase = CreationPhase.HonestQuestion;
            IngesteLogger.Info("[VesselCreation] Phase: Honest Question");
            
            yield return showText("VESSEL_CREATION_HONEST_QUESTION");
            
            string[] honestChoices = { "Yes, I was honest", "No, I wasn't completely honest" };
            yield return showChoiceMenu(honestChoices, (choice) => 
            {
                isHonestAnswer = choice.StartsWith("Yes");
                IngesteLogger.Debug($"[VesselCreation] Honesty choice: '{choice}' (isHonest: {isHonestAnswer})");
            });
            
            // Show response based on honesty
            string responseKey = isHonestAnswer ? "VESSEL_CREATION_HONEST_RESPONSE" : "VESSEL_CREATION_DISHONEST_RESPONSE";
            yield return showText(responseKey);
            
            yield return 2f;
        }

        private IEnumerator feelingsQuestionPhase()
        {
            currentPhase = CreationPhase.FeelingsQuestion;
            IngesteLogger.Info("[VesselCreation] Phase: Feelings Question");
            
            yield return showText("VESSEL_CREATION_FEELINGS_QUESTION");
            yield return showTextInput("How do you feel about this game's nature?", (feeling) => 
            {
                playerFeeling = feeling;
                IngesteLogger.Debug($"[VesselCreation] Player feeling: '{playerFeeling}'");
            }, FEELING_INPUT_MAX_LENGTH);
            
            yield return 1f;
        }

        private IEnumerator creatorNamingPhase()
        {
            currentPhase = CreationPhase.CreatorNaming;
            IngesteLogger.Info("[VesselCreation] Phase: Creator Naming");
            
            yield return showText("VESSEL_CREATION_CREATOR_PROMPT");
            yield return showTextInput("Name yourself as the creator:", (name) => 
            {
                creatorName = name;
                IngesteLogger.Info($"[VesselCreation] Creator named: '{creatorName}'");
            }, NAME_INPUT_MAX_LENGTH);
            
            yield return 1f;
        }

        private IEnumerator vesselDisplayPhase()
        {
            currentPhase = CreationPhase.VesselDisplay;
            IngesteLogger.Info("[VesselCreation] Phase: Vessel Display");
            
            // Display the created vessel
            yield return showText("VESSEL_CREATION_DISPLAY");
            
            // Play heart appear sound when vessel is revealed
            try { Audio.Play(HEART_APPEAR_EVENT); }
            catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play heart appear: {ex.Message}"); }
            
            // Fade in the vessel sprite
            float fadeTimer = 0f;
            while (fadeTimer < 2f)
            {
                fadeTimer += Engine.DeltaTime;
                vesselAlpha = Math.Min(1f, fadeTimer / 2f);
                soulBlurAlpha = Math.Min(0.7f, fadeTimer / 2f);
                yield return null;
            }
            
            // Show vessel details
            string vesselDetails = $"Vessel Name: {vesselName}\n" +
                                   $"Legs: {selectedLeg}\n" +
                                   $"Torso: {selectedTorso}\n" +
                                   $"Head: {selectedHead}\n" +
                                   $"Creator: {creatorName}";
            
            IngesteLogger.Info($"[VesselCreation] Displaying vessel details:\n{vesselDetails}");
            yield return showCustomText(vesselDetails);
            yield return VESSEL_DISPLAY_TIME;
        }

        private IEnumerator vesselDiscardPhase()
        {
            currentPhase = CreationPhase.VesselDiscard;
            IngesteLogger.Info("[VesselCreation] Phase: Vessel Discard");
            
            // Show discard message
            yield return showText("VESSEL_CREATION_DISCARD");
            
            // Fade out the vessel sprite
            float fadeTimer = 0f;
            while (fadeTimer < 2f)
            {
                fadeTimer += Engine.DeltaTime;
                vesselAlpha = Math.Max(0f, 1f - (fadeTimer / 2f));
                soulBlurAlpha = Math.Max(0f, 0.7f - (fadeTimer / 2f));
                yield return null;
            }
            
            // No player choice â€” the vessel is simply discarded, faithful to Deltarune
            yield return 2f;
        }

        private IEnumerator transitionToIntroVignette()
        {
            currentPhase = CreationPhase.Transition;
            IngesteLogger.Info("[VesselCreation] Phase: Transition to IntroVignette");
            
            yield return showText("VESSEL_CREATION_TRANSITION");
            yield return 1f;
            
            // Stop creation music and restore area music
            try
            {
                creationMusic?.stop(STOP_MODE.ALLOWFADEOUT);
                droneMusic?.stop(STOP_MODE.ALLOWFADEOUT);
                IngesteLogger.Debug("[VesselCreation] Creation music stopped");
            }
            catch (Exception ex)
            {
                IngesteLogger.Warn($"[VesselCreation] Failed to stop music: {ex.Message}");
            }
            
            // Fade to white
            IngesteLogger.Info("[VesselCreation] Starting white fade transition");
            FadeWipe whiteFade = new FadeWipe(this, false, () =>
            {
                StopSfx();

                // Restore original music
                try
                {
                    if (!string.IsNullOrEmpty(areaMusic))
                    {
                        Audio.SetMusic(areaMusic);
                        IngesteLogger.Debug($"[VesselCreation] Restored area music: {areaMusic}");
                    }
                }
                catch (Exception ex)
                {
                    session.Audio.Music.Event = areaMusic;
                    IngesteLogger.Warn($"[VesselCreation] Failed to restore music via Audio.SetMusic: {ex.Message}");
                }
                
                // Transition to Chapter 0 IntroVignette
                IngesteLogger.Info("[VesselCreation] Transitioning to Cs00IntroVignette");

                // Mark the mod intro as seen so it never replays on subsequent launches.
                if (MaggyHelperModule.SaveData != null)
                {
                    MaggyHelperModule.SaveData.HasSeenModIntro = true;
                    UserIO.SaveHandler(file: true, settings: false);
                }

                Scene nextScene = new Cs00IntroVignette(session);
                logSceneSwitch("transition", nextScene);
                Engine.Scene = nextScene;
            });
            
            whiteFade.Duration = WHITE_FADE_DURATION;
            whiteFade.OnUpdate = (f) =>
            {
                textAlpha = Math.Min(textAlpha, 1f - f);
                backgroundFade = 1f - f; // Fade to white instead of black
            };
            
            exiting = true;
            yield return null;
        }
        #endregion

        #region UI Helper Methods
        private IEnumerator showText(string dialogKey)
        {
            var textbox = new Textbox(dialogKey);
            return showTextbox(textbox);
        }

        private IEnumerator showCustomText(string text)
        {
            var textbox = new Textbox("temp", Dialog.Languages["english"]);
            // This would need custom implementation to show arbitrary text
            // For now, we'll use a placeholder approach
            return showTextbox(textbox);
        }

        private IEnumerator showTextbox(Textbox textbox)
        {
            Engine.Scene.Add(textbox);
            while (textbox.Opened)
            {
                yield return true;
            }
        }

        /// <summary>Smoothly fades the vessel sprite in over the given duration.</summary>
        private IEnumerator fadeVesselIn(float duration = 1.5f)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Engine.DeltaTime;
                vesselAlpha   = Math.Min(1f, t / duration);
                soulBlurAlpha = Math.Min(0.7f, (t / duration) * 0.7f);
                yield return null;
            }
            vesselAlpha   = 1f;
            soulBlurAlpha = 0.7f;
        }

        /// <summary>
        /// Deltarune-style Left/Right vessel-part cycler with live sprite preview.
        /// <paramref name="getIndex"/> and <paramref name="setIndex"/> read/write the current
        /// index so the vessel renders in real-time while the player browses.
        /// </summary>
        private IEnumerator showVesselPartCycleSelector(int count, Func<int> getIndex, Action<int> setIndex)
        {
            if (count <= 0) yield break;
            setIndex(Math.Clamp(getIndex(), 0, count - 1));
            vesselCyclerActive = true;
            vesselCyclerCount  = count;

            yield return 0.1f; // short debounce before accepting input

            while (true)
            {
                if (Input.MenuLeft.Pressed || Input.MenuLeft.Repeating)
                {
                    setIndex((getIndex() - 1 + count) % count);
                    try { Audio.Play(HEART_CHANGE_EVENT); }
                    catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play heart change: {ex.Message}"); }
                }
                else if (Input.MenuRight.Pressed || Input.MenuRight.Repeating)
                {
                    setIndex((getIndex() + 1) % count);
                    try { Audio.Play(HEART_CHANGE_EVENT); }
                    catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play heart change: {ex.Message}"); }
                }

                if (Input.MenuConfirm.Pressed)
                {
                    try { Audio.Play(CHOICE_SELECT_EVENT); }
                    catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play choice select: {ex.Message}"); }
                    break;
                }

                yield return null;
            }

            vesselCyclerActive = false;
        }

        // Convenience overload â€“ callers that don't need the index
        private IEnumerator showChoiceMenu(string[] choices, Action<string> onSelect)
            => showChoiceMenu(choices, (choice, _) => onSelect(choice));

        /// <summary>
        /// Shows a choice menu using Celeste's built-in TextMenu instead of manual rendering.
        /// This replaces low-level ActiveFont.Draw calls with high-level UI components.
        /// </summary>
        private IEnumerator showChoiceMenu(string[] choices, Action<string, int> onSelect)
        {
            IngesteLogger.Debug($"[VesselCreation] Showing TextMenu choice with {choices.Length} options");
            try { Audio.Play(CHOICE_APPEAR_EVENT); }
            catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play choice appear: {ex.Message}"); }

            var menu = new TextMenu();
            menu.CompactWidthMode = true;

            for (int i = 0; i < choices.Length; i++)
            {
                int index = i;
                string label = choices[i];
                menu.Add(new TextMenu.Button(label).Pressed(() =>
                {
                    menu.OnCancel -= menu.Close;
                    menu.Close();
                    try { Audio.Play(CHOICE_SELECT_EVENT); }
                    catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play choice select: {ex.Message}"); }
                    onSelect(label, index);
                }));
            }

            // Add a title if we're in a vessel part selection phase
            string title = currentPhase switch
            {
                CreationPhase.LegSelection   => Dialog.Clean("VESSEL_CREATION_LEG_CHOICE"),
                CreationPhase.TorsoSelection => Dialog.Clean("VESSEL_CREATION_TORSO_CHOICE"),
                CreationPhase.HeadSelection  => Dialog.Clean("VESSEL_CREATION_HEAD_CHOICE"),
                _                            => ""
            };

            if (!string.IsNullOrEmpty(title))
            {
                menu.Insert(0, new TextMenu.Header(title));
            }

            // Open the menu as an overlay
            menu.OnCancel = () =>
            {
                // Prevent cancel on vessel selections - must pick something
                if (currentPhase != CreationPhase.LegSelection &&
                    currentPhase != CreationPhase.TorsoSelection &&
                    currentPhase != CreationPhase.HeadSelection)
                {
                    menu.Close();
                }
            };

            Add(menu);

            // Wait until the menu is closed
            while (menu.Focused)
            {
                yield return null;
            }

            yield return null; // one extra frame for cleanup
        }

        private IEnumerator showTextInput(string prompt, Action<string> onComplete, int maxLength = NAME_INPUT_MAX_LENGTH)
        {
            IngesteLogger.Debug($"[VesselCreation] Opening text input for prompt '{prompt}' with max length {maxLength}");

            textInputPrompt = prompt;
            textInputValue = string.Empty;
            textInputCursorIndex = 0;
            textInputSelectionAnchor = 0;
            textInputMaxLength = maxLength;
            textInputPaletteRow = 0;
            textInputPaletteColumn = 0;
            textInputPaletteActive = false;
            textInputOnComplete = onComplete;
            textInputActive = true;

            try { Audio.Play(CHOICE_APPEAR_EVENT); }
            catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play choice appear: {ex.Message}"); }

            while ((textInputEase += Engine.DeltaTime * CHOICE_EASE_SPEED) < 1f)
                yield return null;

            textInputEase = 1f;

            while (textInputActive)
                yield return null;

            while ((textInputEase -= Engine.DeltaTime * CHOICE_EASE_SPEED) > 0f)
                yield return null;

            textInputEase = 0f;
        }

        private void updateTextInput()
        {
            if (!textInputActive)
                return;

            bool shift = MInput.Keyboard.Check(Keys.LeftShift) || MInput.Keyboard.Check(Keys.RightShift);
            bool ctrl = MInput.Keyboard.Check(Keys.LeftControl) || MInput.Keyboard.Check(Keys.RightControl);

            if (ctrl && MInput.Keyboard.Pressed(Keys.A))
            {
                textInputSelectionAnchor = 0;
                textInputCursorIndex = textInputValue.Length;
            }

            if (textInputPaletteActive)
            {
                updateTextInputPalette();
            }
            else
            {
                updateTextInputCursorMovement(shift);

                if (Input.MenuDown.Pressed)
                {
                    textInputPaletteActive = true;
                    textInputPaletteColumn = Math.Min(textInputPaletteColumn, TextInputPaletteRows[textInputPaletteRow].Length - 1);
                    try { Audio.Play(CHOICE_MOVE_EVENT); }
                    catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play choice move: {ex.Message}"); }
                }
            }

            if (MInput.Keyboard.Pressed(Keys.Home))
                moveTextInputCursor(0, shift);

            if (MInput.Keyboard.Pressed(Keys.End))
                moveTextInputCursor(textInputValue.Length, shift);

            if (MInput.Keyboard.Pressed(Keys.Back) || Input.MenuCancel.Pressed)
            {
                deleteTextInputSelectionOrBackspace();
            }
            else if (MInput.Keyboard.Pressed(Keys.Delete))
            {
                deleteTextInputSelectionOrDeleteForward();
            }

            foreach (Keys key in MInput.Keyboard.CurrentState.GetPressedKeys())
            {
                if (!MInput.Keyboard.Pressed(key))
                    continue;

                if (tryTranslateKeyToText(key, shift, out string? text) && text != null)
                    insertTextInputText(text);
            }

            if (Input.MenuConfirm.Pressed || MInput.Keyboard.Pressed(Keys.Enter))
            {
                submitTextInput();
            }
        }

        private void updateTextInputCursorMovement(bool extendSelection)
        {
            if (Input.MenuLeft.Pressed || Input.MenuLeft.Repeating || MInput.Keyboard.Pressed(Keys.Left))
            {
                moveTextInputCursor(textInputCursorIndex - 1, extendSelection);
            }
            else if (Input.MenuRight.Pressed || Input.MenuRight.Repeating || MInput.Keyboard.Pressed(Keys.Right))
            {
                moveTextInputCursor(textInputCursorIndex + 1, extendSelection);
            }
        }

        private void updateTextInputPalette()
        {
            if (Input.MenuUp.Pressed)
            {
                if (textInputPaletteRow == 0)
                {
                    textInputPaletteActive = false;
                }
                else
                {
                    textInputPaletteRow--;
                    textInputPaletteColumn = Math.Min(textInputPaletteColumn, TextInputPaletteRows[textInputPaletteRow].Length - 1);
                }

                Audio.Play(CHOICE_MOVE_EVENT);
            }
            else if (Input.MenuDown.Pressed)
            {
                textInputPaletteRow = Math.Min(textInputPaletteRow + 1, TextInputPaletteRows.Length - 1);
                textInputPaletteColumn = Math.Min(textInputPaletteColumn, TextInputPaletteRows[textInputPaletteRow].Length - 1);
                Audio.Play(CHOICE_MOVE_EVENT);
            }
            else if (Input.MenuLeft.Pressed || Input.MenuLeft.Repeating)
            {
                textInputPaletteColumn = Math.Max(textInputPaletteColumn - 1, 0);
                Audio.Play(CHOICE_MOVE_EVENT);
            }
            else if (Input.MenuRight.Pressed || Input.MenuRight.Repeating)
            {
                textInputPaletteColumn = Math.Min(textInputPaletteColumn + 1, TextInputPaletteRows[textInputPaletteRow].Length - 1);
                Audio.Play(CHOICE_MOVE_EVENT);
            }

            if (Input.MenuConfirm.Pressed)
            {
                applyTextInputPaletteItem(TextInputPaletteRows[textInputPaletteRow][textInputPaletteColumn]);
            }
        }

        private void applyTextInputPaletteItem(string item)
        {
            switch (item)
            {
                case "Space":
                    insertTextInputText(" ");
                    break;

                case "Back":
                    deleteTextInputSelectionOrBackspace();
                    break;

                case "Del":
                    deleteTextInputSelectionOrDeleteForward();
                    break;

                case "Clear":
                    textInputValue = string.Empty;
                    textInputCursorIndex = 0;
                    textInputSelectionAnchor = 0;
                    try { Audio.Play(CHOICE_MOVE_EVENT); }
                    catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play choice move: {ex.Message}"); }
                    break;

                case "OK":
                    submitTextInput();
                    break;

                default:
                    insertTextInputText(item);
                    break;
            }
        }

        private void submitTextInput()
        {
            string sanitized = textInputValue.Trim();
            if (string.IsNullOrEmpty(sanitized))
                return;

            try { Audio.Play(CHOICE_SELECT_EVENT); }
            catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play choice select: {ex.Message}"); }
            textInputOnComplete?.Invoke(sanitized);
            textInputOnComplete = null;
            textInputActive = false;
            textInputPaletteActive = false;
        }

        private void moveTextInputCursor(int newIndex, bool extendSelection)
        {
            textInputCursorIndex = Math.Clamp(newIndex, 0, textInputValue.Length);
            if (!extendSelection)
                textInputSelectionAnchor = textInputCursorIndex;
        }

        private void insertTextInputText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            int selectionLength = getTextInputSelectionLength();
            int proposedLength = textInputValue.Length - selectionLength + text.Length;
            if (proposedLength > textInputMaxLength)
                return;

            replaceTextInputSelection(text);
            Audio.Play(CHOICE_MOVE_EVENT);
        }

        private void deleteTextInputSelectionOrBackspace()
        {
            if (hasTextInputSelection())
            {
                replaceTextInputSelection(string.Empty);
                Audio.Play(CHOICE_MOVE_EVENT);
                return;
            }

            if (textInputCursorIndex <= 0)
                return;

            textInputValue = textInputValue.Remove(textInputCursorIndex - 1, 1);
            textInputCursorIndex--;
            textInputSelectionAnchor = textInputCursorIndex;
            Audio.Play(CHOICE_MOVE_EVENT);
        }

        private void deleteTextInputSelectionOrDeleteForward()
        {
            if (hasTextInputSelection())
            {
                replaceTextInputSelection(string.Empty);
                Audio.Play(CHOICE_MOVE_EVENT);
                return;
            }

            if (textInputCursorIndex >= textInputValue.Length)
                return;

            textInputValue = textInputValue.Remove(textInputCursorIndex, 1);
            textInputSelectionAnchor = textInputCursorIndex;
            Audio.Play(CHOICE_MOVE_EVENT);
        }

        private void replaceTextInputSelection(string replacement)
        {
            int start = getTextInputSelectionStart();
            int length = getTextInputSelectionLength();
            textInputValue = textInputValue.Remove(start, length).Insert(start, replacement);
            textInputCursorIndex = start + replacement.Length;
            textInputSelectionAnchor = textInputCursorIndex;
        }

        private bool hasTextInputSelection()
            => textInputCursorIndex != textInputSelectionAnchor;

        private int getTextInputSelectionStart()
            => Math.Min(textInputCursorIndex, textInputSelectionAnchor);

        private int getTextInputSelectionLength()
            => Math.Abs(textInputCursorIndex - textInputSelectionAnchor);

        private static bool tryTranslateKeyToText(Keys key, bool shift, out string? text)
        {
            text = null;

            if (key >= Keys.A && key <= Keys.Z)
            {
                char c = (char)('a' + (key - Keys.A));
                text = shift ? char.ToUpperInvariant(c).ToString() : c.ToString();
                return true;
            }

            if (key >= Keys.D0 && key <= Keys.D9)
            {
                string shiftedDigits = ")!@#$%^&*(";
                int index = key - Keys.D0;
                text = shift ? shiftedDigits[index].ToString() : ((char)('0' + index)).ToString();
                return true;
            }

            if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
            {
                text = ((char)('0' + (key - Keys.NumPad0))).ToString();
                return true;
            }

            switch (key)
            {
                case Keys.Space:
                    text = " ";
                    return true;
                case Keys.OemMinus:
                    text = shift ? "_" : "-";
                    return true;
                case Keys.OemPlus:
                    text = shift ? "+" : "=";
                    return true;
                case Keys.OemComma:
                    text = shift ? "<" : ",";
                    return true;
                case Keys.OemPeriod:
                    text = shift ? ">" : ".";
                    return true;
                case Keys.OemQuestion:
                    text = shift ? "?" : "/";
                    return true;
                case Keys.OemSemicolon:
                    text = shift ? ":" : ";";
                    return true;
                case Keys.OemQuotes:
                    text = shift ? "\"" : "'";
                    return true;
                case Keys.OemOpenBrackets:
                    text = shift ? "{" : "[";
                    return true;
                case Keys.OemCloseBrackets:
                    text = shift ? "}" : "]";
                    return true;
                case Keys.OemPipe:
                    text = shift ? "|" : "\\";
                    return true;
                case Keys.OemTilde:
                    text = shift ? "~" : "`";
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region Graphics Loading
        private void loadVesselGraphics()
        {
            try
            {
                IngesteLogger.Debug($"[VesselCreation] Loading vessel graphics from: {VESSEL_GRAPHICS_PATH}");
                gonerBodyTextures = loadTextureArray(VESSEL_GRAPHICS_PATH + GONERBODY_BASE);
                gonerHeadTextures = loadTextureArray(VESSEL_GRAPHICS_PATH + GONERHEAD_BASE);
                gonerLegTextures  = loadTextureArray(VESSEL_GRAPHICS_PATH + GONERLEGS_BASE);
                depthTexture = GFX.Game.Has(VESSEL_GRAPHICS_PATH + DEPTH_SPRITE) ? GFX.Game[VESSEL_GRAPHICS_PATH + DEPTH_SPRITE] : null;
                soulBlurTexture = GFX.Game.Has(VESSEL_GRAPHICS_PATH + SOUL_BLUR_SPRITE) ? GFX.Game[VESSEL_GRAPHICS_PATH + SOUL_BLUR_SPRITE] : null;
                pinkSoulBlurTexture = GFX.Game.Has(VESSEL_GRAPHICS_PATH + PINK_SOUL_BLUR_SPRITE) ? GFX.Game[VESSEL_GRAPHICS_PATH + PINK_SOUL_BLUR_SPRITE] : null;
                
                IngesteLogger.Info($"[VesselCreation] Graphics loaded - Bodies: {gonerBodyTextures.Length}, Heads: {gonerHeadTextures.Length}, Legs: {gonerLegTextures.Length}, Depth: {depthTexture != null}, SoulBlur: {soulBlurTexture != null}, PinkSoulBlur: {pinkSoulBlurTexture != null}");
            }
            catch (Exception ex)
            {
                IngesteLogger.Error(ex, "[VesselCreation] Failed to load vessel graphics");
            }
        }

        /// <summary>Loads all sequentially numbered variants of a sprite (00, 01, ...) until one is missing.</summary>
        private static MTexture[] loadTextureArray(string basePath)
        {
            var list = new List<MTexture>();
            for (int i = 0; i < 99; i++)
            {
                string key = basePath + i.ToString("D2");
                if (GFX.Game.Has(key))
                    list.Add(GFX.Game[key]);
                else
                    break;
            }
            return list.ToArray();
        }

        /// <summary>Builds choice labels like "Legs 0", "Legs 1", ... for the given count.</summary>
        private static string[] buildIndexedLabels(string prefix, int count)
        {
            if (count == 0) return new[] { $"{prefix} 0" };
            var labels = new string[count];
            for (int i = 0; i < count; i++)
                labels[i] = $"{prefix} {i}";
            return labels;
        }
        #endregion

        #region Scene Overrides
        public override void Update()
        {
            if (pauseMenu == null)
            {
                base.Update();
                updateTextInput();
                if (!exiting)
                {
                    // Update the sequence coroutine directly
                    if (sequenceCoroutine != null && !sequenceCoroutine.Finished)
                    {
                        sequenceCoroutine.Update();
                    }
                    
                    if (!textInputActive && (Input.Pause.Pressed || Input.ESC.Pressed))
                    {
                        OpenPauseMenu();
                    }
                }
            }
            else if (!exiting)
            {
                pauseMenu.Update();
            }
            
            pauseFade = Calc.Approach(pauseFade, (pauseMenu != null) ? 1 : 0, Engine.DeltaTime * 8f);
            hud.BackgroundFade = Calc.Approach(hud.BackgroundFade, (pauseMenu != null) ? 0.6f : 0f, Engine.DeltaTime * 3f);
            fade = Calc.Approach(fade, 0f, Engine.DeltaTime);
        }
        
        public void OpenPauseMenu()
        {
            PauseSfx();
            try { Audio.Play("guid://{c6311b80-2ae7-4155-80f1-d3e2713efaa3}"); }
            catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play pause sound: {ex.Message}"); }
            Add(pauseMenu = new TextMenu());
            pauseMenu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_resume")).Pressed(ClosePauseMenu));
            pauseMenu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_skip")).Pressed(SkipVignette));
            pauseMenu.OnCancel = pauseMenu.OnESC = pauseMenu.OnPause = ClosePauseMenu;
        }
        
        private void ClosePauseMenu()
        {
            ResumeSfx();
            try { Audio.Play("guid://{6096b41c-7b56-4839-ad53-deb9e5693246}"); }
            catch (Exception ex) { IngesteLogger.Warn($"[VesselCreation] Failed to play unpause sound: {ex.Message}"); }
            if (pauseMenu != null)
            {
                pauseMenu.RemoveSelf();
            }
            pauseMenu = null;
        }
        
        private void SkipVignette()
        {
            StopSfx();
            sequenceCoroutine = null;
            session.Audio.Music.Event = areaMusic;
            if (pauseMenu != null)
            {
                pauseMenu.RemoveSelf();
                pauseMenu = null;
            }
            
            FadeWipe fadeWipe = new FadeWipe(this, false, delegate
            {
                // Skip directly to the next scene (Cs00IntroVignette or LevelLoader)
                IngesteLogger.Info("[VesselCreation] Skipping vignette - transitioning to Cs00IntroVignette");

                // Mark the mod intro as seen even when skipped.
                if (MaggyHelperModule.SaveData != null)
                {
                    MaggyHelperModule.SaveData.HasSeenModIntro = true;
                    UserIO.SaveHandler(file: true, settings: false);
                }

                Scene nextScene = new Cs00IntroVignette(session);
                logSceneSwitch("skip", nextScene);
                Engine.Scene = nextScene;
            })
            {
                OnUpdate = delegate(float f)
                {
                    textAlpha = Math.Min(textAlpha, 1f - f);
                }
            };
            exiting = true;
        }
        
        private void PauseSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Pause();
            }
            creationMusic?.setPaused(true);
            droneMusic?.setPaused(true);
        }
        
        private void ResumeSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Resume();
            }
            creationMusic?.setPaused(false);
            droneMusic?.setPaused(false);
        }
        
        private void StopSfx()
        {
            List<Component> components = new List<Component>();
            components.AddRange(Tracker.GetComponents<SoundSource>());
            foreach (SoundSource sound in components)
            {
                sound.RemoveSelf();
            }
            creationMusic?.stop(STOP_MODE.IMMEDIATE);
            droneMusic?.stop(STOP_MODE.IMMEDIATE);
        }

        private void logSceneSwitch(string source, Scene nextScene)
        {
            string currentScene = Engine.Scene?.GetType().Name ?? "null";
            string targetScene = nextScene.GetType().Name;
            IngesteLogger.Debug($"[VesselCreation] Scene switch [{source}] at {DateTime.UtcNow:O}: {currentScene} -> {targetScene}");
        }

        public override void Render()
        {
            // Draw background FIRST, before base.Render()
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);
            
            // Render solid background (black for most phases, white for transition)
            Color bgColor = currentPhase == CreationPhase.Transition ? Color.White : Color.Black;
            Draw.Rect(0f, 0f, Engine.ViewWidth, Engine.ViewHeight, bgColor);
            
            Draw.SpriteBatch.End();
            
            // Now call base render for HUD and other components
            base.Render();

            // Render text input behind vessel graphics so the image appears on top
            if (textInputActive || textInputEase > 0f)
            {
                renderTextInput();
            }
            
            // Render vessel graphics from leg selection onward (live preview + display phases)
            if (vesselAlpha > 0f && currentPhase != CreationPhase.Introduction && currentPhase != CreationPhase.Transition)
            {
                renderVesselGraphics();
            }

            // Render vessel part cycler UI when active (Deltarune-style Left/Right browsing)
            if (vesselCyclerActive)
            {
                renderVesselCycler();
            }

            // Choice menu now uses Celeste TextMenu (high-level UI) - no manual rendering needed
            
            // Render fade overlay if transitioning
            if (backgroundFade < 1f && currentPhase == CreationPhase.Transition)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);
                Draw.Rect(0f, 0f, Engine.ViewWidth, Engine.ViewHeight, Color.White * (1f - backgroundFade));
                Draw.SpriteBatch.End();
            }
            
            // Render fade overlay for intro/outro fade
            if (fade > 0f || textAlpha > 0f)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);
                if (fade > 0f)
                {
                    Draw.Rect(0f, 0f, Engine.ViewWidth, Engine.ViewHeight, Color.Black * fade);
                }
                Draw.SpriteBatch.End();
            }
        }

        private void renderVesselGraphics()
        {
            VesselCreationUIHelper.RenderVesselGraphics(
                vesselAlpha, soulBlurAlpha, vesselPosition,
                depthTexture, soulBlurTexture, pinkSoulBlurTexture,
                gonerBodyTextures, gonerHeadTextures, gonerLegTextures,
                selectedBodyIndex, selectedHeadIndex, selectedLegIndex);
        }

        // Choice menu now uses TextMenu - renderChoiceMenu removed (replaced by high-level UI)

        /// <summary>Renders the Deltarune-style Left/Right vessel part cycler HUD.</summary>
        private void renderVesselCycler()
        {
            VesselCreationUIHelper.RenderVesselCycler(
                (int)currentPhase, selectedLegIndex, selectedBodyIndex, selectedHeadIndex,
                vesselCyclerCount, vesselPosition);
        }

        private void renderTextInput()
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);

            float overlayAlpha = 0.9f * textInputEase;
            Draw.Rect(0f, 0f, Engine.ViewWidth, Engine.ViewHeight, Color.Black * (0.65f * textInputEase));

            Vector2 panelCenter = new Vector2(Engine.Width / 2f, Engine.Height / 2f);
            float panelWidth = 980f;
            float panelHeight = 430f;
            Vector2 panelTopLeft = panelCenter - new Vector2(panelWidth / 2f, panelHeight / 2f);
            Rectangle panelRect = new Rectangle((int)panelTopLeft.X, (int)panelTopLeft.Y, (int)panelWidth, (int)panelHeight);
            Draw.Rect(panelRect, Color.Black * overlayAlpha);

            ActiveFont.DrawOutline(textInputPrompt, panelCenter + new Vector2(0f, -150f), new Vector2(0.5f, 0.5f), Vector2.One * 0.8f, Color.White * textInputEase, 2f, Color.Black);

            Rectangle inputRect = new Rectangle(
                (int)(panelCenter.X - TEXT_INPUT_BOX_WIDTH / 2f),
                (int)(panelCenter.Y - TEXT_INPUT_BOX_HEIGHT / 2f - 45f),
                (int)TEXT_INPUT_BOX_WIDTH,
                (int)TEXT_INPUT_BOX_HEIGHT);

            Draw.Rect(inputRect, Color.Black * 0.95f * textInputEase);

            renderTextInputValue(inputRect);
            renderTextInputPalette(panelCenter + new Vector2(0f, 75f));

            string hint = "Type to enter | Left/Right move cursor | Shift+Arrows select | Ctrl+A select all | Down opens character picker | Enter confirms";
            ActiveFont.DrawOutline(hint, panelCenter + new Vector2(0f, 175f), new Vector2(0.5f, 0.5f), Vector2.One * 0.45f, Color.Gray * textInputEase, 2f, Color.Black);

            string lengthText = $"{textInputValue.Length}/{textInputMaxLength}";
            ActiveFont.DrawOutline(lengthText, new Vector2(inputRect.Right - 12f, inputRect.Bottom + 18f), new Vector2(1f, 0f), Vector2.One * 0.45f, Color.Gray * textInputEase, 2f, Color.Black);

            Draw.SpriteBatch.End();
        }

        private void renderTextInputValue(Rectangle inputRect)
        {
            const float innerPadding = 18f;
            float availableWidth = inputRect.Width - innerPadding * 2f;
            int visibleStart = getVisibleTextStart(availableWidth);
            int visibleLength = getVisibleTextLength(visibleStart, availableWidth);
            string visibleText = textInputValue.Substring(visibleStart, visibleLength);

            Vector2 basePosition = new Vector2(inputRect.X + innerPadding, inputRect.Y + inputRect.Height / 2f);
            Vector2 textScale = Vector2.One * TEXT_INPUT_SCALE;
            float lineHeight = ActiveFont.LineHeight * TEXT_INPUT_SCALE;
            Vector2 textPosition = basePosition - new Vector2(0f, lineHeight / 2f);

            if (visibleText.Length == 0)
            {
                ActiveFont.Draw("Type here...", textPosition, Vector2.Zero, textScale, Color.Gray * 0.8f * textInputEase);
            }
            else
            {
                if (hasTextInputSelection())
                {
                    int selectionStart = getTextInputSelectionStart();
                    int selectionEnd = selectionStart + getTextInputSelectionLength();
                    int visibleSelectionStart = Math.Max(selectionStart, visibleStart);
                    int visibleSelectionEnd = Math.Min(selectionEnd, visibleStart + visibleText.Length);

                    if (visibleSelectionStart < visibleSelectionEnd)
                    {
                        string preSelection = textInputValue.Substring(visibleStart, visibleSelectionStart - visibleStart);
                        string selectionText = textInputValue.Substring(visibleSelectionStart, visibleSelectionEnd - visibleSelectionStart);
                        float selectionX = ActiveFont.Measure(preSelection).X * TEXT_INPUT_SCALE;
                        float selectionWidth = ActiveFont.Measure(selectionText).X * TEXT_INPUT_SCALE;
                        Draw.Rect(textPosition.X + selectionX - 2f, textPosition.Y - 4f, selectionWidth + 4f, lineHeight + 8f, Calc.HexToColor("2B7FFF") * 0.6f * textInputEase);
                    }
                }

                ActiveFont.Draw(visibleText, textPosition, Vector2.Zero, textScale, Color.White * textInputEase);
            }

            bool showCursor = textInputActive && ((int)(TimeActive * 2f) % 2 == 0);
            if (showCursor)
            {
                int cursorWithinVisible = Math.Clamp(textInputCursorIndex - visibleStart, 0, visibleText.Length);
                string cursorPrefix = visibleText.Substring(0, cursorWithinVisible);
                float cursorX = ActiveFont.Measure(cursorPrefix).X * TEXT_INPUT_SCALE;
                Draw.Rect(textPosition.X + cursorX, textPosition.Y - 4f, 3f, lineHeight + 8f, Color.White * textInputEase);
            }

            if (visibleStart > 0)
                ActiveFont.Draw("<", new Vector2(inputRect.X + 4f, inputRect.Center.Y), new Vector2(0f, 0.5f), Vector2.One * 0.4f, Color.Gray * textInputEase);

            if (visibleStart + visibleText.Length < textInputValue.Length)
                ActiveFont.Draw(">", new Vector2(inputRect.Right - 8f, inputRect.Center.Y), new Vector2(1f, 0.5f), Vector2.One * 0.4f, Color.Gray * textInputEase);
        }

        private int getVisibleTextStart(float availableWidth)
        {
            int start = 0;
            while (start < textInputCursorIndex)
            {
                string between = textInputValue.Substring(start, textInputCursorIndex - start);
                if (ActiveFont.Measure(between).X * TEXT_INPUT_SCALE <= availableWidth - 40f)
                    break;
                start++;
            }

            return start;
        }

        private int getVisibleTextLength(int start, float availableWidth)
        {
            int length = 0;
            while (start + length < textInputValue.Length)
            {
                string candidate = textInputValue.Substring(start, length + 1);
                if (ActiveFont.Measure(candidate).X * TEXT_INPUT_SCALE > availableWidth)
                    break;
                length++;
            }

            return length;
        }

        private void renderTextInputPalette(Vector2 center)
        {
            float rowSpacing = 38f;
            float buttonPaddingX = 16f;
            float buttonPaddingY = 6f;

            for (int row = 0; row < TextInputPaletteRows.Length; row++)
            {
                string[] paletteRow = TextInputPaletteRows[row];
                float rowWidth = 0f;
                for (int col = 0; col < paletteRow.Length; col++)
                {
                    rowWidth += ActiveFont.Measure(paletteRow[col]).X * 0.45f + buttonPaddingX * 2f + 10f;
                }

                rowWidth -= 10f;
                float x = center.X - rowWidth / 2f;
                float y = center.Y + row * rowSpacing;

                for (int col = 0; col < paletteRow.Length; col++)
                {
                    string item = paletteRow[col];
                    Vector2 itemSize = ActiveFont.Measure(item) * 0.45f;
                    float buttonWidth = itemSize.X + buttonPaddingX * 2f;
                    Rectangle buttonRect = new Rectangle((int)x, (int)y, (int)buttonWidth, (int)(itemSize.Y + buttonPaddingY * 2f));
                    bool selected = textInputPaletteActive && row == textInputPaletteRow && col == textInputPaletteColumn;

                    Draw.Rect(buttonRect, (selected ? Color.White : Calc.HexToColor("1A1A1A")) * (selected ? 0.22f : 0.85f) * textInputEase);
                    Draw.HollowRect(buttonRect, (selected ? Color.White : Color.Gray) * textInputEase);
                    ActiveFont.Draw(item, new Vector2(buttonRect.Center.X, buttonRect.Center.Y), new Vector2(0.5f, 0.5f), Vector2.One * 0.45f, (selected ? Color.White : Color.Silver) * textInputEase);

                    x += buttonWidth + 10f;
                }
            }
        }
        #endregion

        #region Cleanup
        public override void End()
        {
            IngesteLogger.Info("[VesselCreation] VesselCreationVignette ending");
            StopSfx();
            // Ensure music is restored when vignette ends abruptly
            try
            {
                if (!string.IsNullOrEmpty(areaMusic))
                {
                    Audio.SetMusic(areaMusic);
                    IngesteLogger.Debug($"[VesselCreation] Restored music on end: {areaMusic}");
                }
            }
            catch (Exception ex)
            {
                IngesteLogger.Warn($"[VesselCreation] Failed to restore music on end: {ex.Message}");
            }
            base.End();
        }
        #endregion
    }
}

