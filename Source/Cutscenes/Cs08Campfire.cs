using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Celeste.Entities;

namespace Celeste
{
    public class Cs08Campfire : CutsceneEntity
    {
        public const string Flag = "campfire_chat_mod";
        public const string GetFlag = "ch8_plat";
        public const string DuskBackgroundFlag = "duskbg";
        public const string StarsBackgroundFlag = "starsbg";

        private static readonly Vector2 CAMERA_START_OFFSET = new(0f, -144f);
        private static readonly Vector2 PLAYER_OFFSET = new(24f, -8f);
        private static readonly Vector2 MADELINE_OFFSET = new(-20f, -16f);
        private static readonly Vector2 RESPAWN_POINT = new(-176f, 312f);
        
        // NPC positioning offsets for campfire scene
        private static readonly Vector2 BADELINE_OFFSET = new(50f, -16f);
        private static readonly Vector2 RALSEI_OFFSET = new(-80f, -16f);

        private const float LIGHT_APPROACH_SPEED = 2f;
        private const float DIALOG_OPTION_HEIGHT = 140f;
        private const float DIALOG_OPTION_EASE_SPEED = 4f;
        // Option rows are drawn in HUD space (1920x1080), matching the vanilla campfire layout.
        private const float DIALOG_OPTION_LEFT = 260f;
        private const float DIALOG_OPTION_TOP = 120f;
        private const string music_event = "event:/DZ/music/lvl8/intro";
        private const string collapse_audio_event = "event:/DZ/sfx/player_collapse";
        private const string selfie_audio_event = "event:/game/02_old_site/theoselfie_foley";
        #endregion

        #region Fields
        private readonly global::Celeste.Player player;
        private readonly CelesteNPC madeline;
        private Bonfire bonfire;
        private PlateauMod plateau;
        private Vector2 cameraStart;
        private Vector2 playerCampfirePosition;
        private Vector2 madelineCampfirePosition;
        private BadelineDummy badeline;
        private RalseiDummy ralsei;
        private Selfie selfie;
        private float optionEase;
        private Dictionary<string, Option[]> nodes = new Dictionary<string, Option[]>();
        private HashSet<Question> asked = new HashSet<Question>();
        private List<Option> currentOptions = new List<Option>();
        private int currentOptionIndex;

        public Cs08Campfire(NPC madeline, Player player)
        {
            // The dialog options are drawn in HUD space, so this entity has to render there too.
            Tag = (int)Tags.HUD;

            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.madeline = madeline ?? throw new ArgumentNullException(nameof(madeline));

            // Create questions
            Question question1 = new Question("anything");
            Question question2 = new Question("zero");
            Question question3 = new Question("gaster");
            Question question4 = new Question("badeline");
            Question question5 = new Question("granny");
            Question question6 = new Question("void");
            Question question7 = new Question("evilgod");
            Question question8 = new Question("monster");
            Question question9 = new Question("goal");
            Question question10 = new Question("grandpa");
            Question question11 = new Question("story");
            Question question12 = new Question("tips");
            Question question13 = new Question(nameof(selfie));
            Question question14 = new Question("sleep");
            Question question15 = new Question("sleep_confirm");
            Question question16 = new Question("sleep_cancel");
            Question question17 = new Question("memories");
            Question question18 = new Question("journey");
            Question question19 = new Question("fears");
            Question question20 = new Question("hope");

            nodes.Add("start", new Option[]
            {
                new Option(question1, "start").ExcludedBy(question5),
                new Option(question2, "start").Require(question9),
                new Option(question9, "start").Require(question3),
                new Option(question10, "start").Require(question9, question5),
                new Option(question11, "start").Require(question10, question7),
                new Option(question12, "start").Require(question11),
                new Option(question3, "start"),
                new Option(question4, "start").Require(question3),
                new Option(question5, "start").Require(question3, question9),
                new Option(question6, "start").Require(question5),
                new Option(question7, "start").Require(question6),
                new Option(question8, "start").Require(question6),
                new Option(question13, "").Require(question7, question11),
                new Option(question14, "sleep").Require(question5).ExcludedBy(question7, question11).Repeatable(),
                new Option(question17, "start").Require(question4),
                new Option(question18, "start").Require(question17),
                new Option(question19, "start").Require(question8),
                new Option(question20, "start").Require(question18, question19)
            });

            nodes.Add("sleep", new Option[]
            {
                new Option(question16, "start").Repeatable(),
                new Option(question15, "")
            });
        }

        public override void OnBegin(Level level)
        {
            Audio.SetMusic(null, false, false);
            level.SnapColorGrade(null);
            level.Bloom.Base = 0.0f;
            level.Session.SetFlag("duskbg");
            plateau = Scene.Entities.FindFirst<PlateauMod>();
            bonfire = Scene.Tracker.GetEntity<BigBonfire>();
            badeline = Scene.Entities.FindFirst<BadelineDummy>();
            ralsei = Scene.Entities.FindFirst<RalseiDummy>();

            level.Camera.Position = new Vector2(level.Bounds.Left, bonfire.Y - 144f);
            level.ZoomSnap(new Vector2(80f, 120f), 2f);
            cameraStart = level.Camera.Position;
            madelineCampfirePosition = new Vector2(bonfire.X - 16f, bonfire.Y);
            player.Light.Alpha = 0.0f;
            player.X = level.Bounds.Left - 40;
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            playerCampfirePosition = new Vector2(bonfire.X + 20f, bonfire.Y);

            if (level.Session.GetFlag("campfire_chat_mod"))
            {
                WasSkipped = true;
                level.ResetZoom();
                level.EndCutscene();
                EndCutscene(level);
            }
            else
                Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator PlayerLightApproach()
        {
            while (player.Light.Alpha < 1.0)
            {
                player.Light.Alpha = Calc.Approach(player.Light.Alpha, 1f, Engine.DeltaTime * 2f);
                yield return null;
            }
        }

        private IEnumerator Cutscene(Level level)
        {
            Cs08Campfire cs08Campfire = this;
            yield return 0.1f;
            cs08Campfire.Add(new Coroutine(cs08Campfire.PlayerLightApproach()));
            Coroutine camTo;
            cs08Campfire.Add(camTo = new Coroutine(CutsceneEntity.CameraTo(new Vector2(level.Camera.X + 90f, level.Camera.Y), 6f, Ease.CubeIn)));
            cs08Campfire.player.DummyAutoAnimate = false;
            cs08Campfire.player.Sprite.Play("carryMaddyWalk");

            for (float p = 0.0f; p < 3.5; p += Engine.DeltaTime)
            {
                SpotlightWipe.FocusPoint = new Vector2(40f, 120f);
                cs08Campfire.player.NaiveMove(new Vector2(32f * Engine.DeltaTime, 0.0f));
                yield return null;
            }

            cs08Campfire.player.Sprite.Play("carryMaddyCollapse");
            Audio.Play("event:/DZ/sfx/player_collapse", cs08Campfire.player.Position);
            yield return 0.3f;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Vector2 position = cs08Campfire.player.Position + new Vector2(16f, 1f);
            cs08Campfire.Level.ParticlesFG.Emit(Payphone.P_Snow, 2, position, Vector2.UnitX * 4f);
            cs08Campfire.Level.ParticlesFG.Emit(Payphone.P_SnowB, 12, position, Vector2.UnitX * 10f);
            yield return 0.7f;

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
            bonfire?.SetMode(Bonfire.Mode.Lit);
            yield return 2.45f;
            camTo.Cancel();

            // Everything at the campfire is placed relative to the fire; fall back to
            // the sleeping spot if the room has no bonfire so the scene still plays out.
            Vector2 firePosition = bonfire?.Position ?? playerCampfirePosition;

            // Camera repositioning
            level.Camera.Position = player.CameraTarget;
            player.Position = playerCampfirePosition;
            player.Facing = Facings.Left;
            player.Sprite.Play("asleep");

            if (badeline != null)
            {
                badeline.Position = firePosition + BADELINE_OFFSET;
                if (badeline.Sprite != null)
                    badeline.Sprite.Scale.X = -1f; // Face left
            }

            if (ralsei != null)
            {
                ralsei.Position = firePosition + RALSEI_OFFSET;
                if (ralsei.Sprite != null)
                    ralsei.Sprite.Scale.X = 1f; // Face right
            }

            level.Session.SetFlag("starsbg");
            level.Session.SetFlag("duskbg", false);
            fade.EndTimer = 0.0f;
            FadeWipe fadeWipe1 = new FadeWipe(level, true);
            yield return null;
            level.ResetZoom();
            level.Camera.Position = new Vector2(firePosition.X - 160f, firePosition.Y - 140f);
            yield return 3f;
            Audio.SetMusic("event:/DZ/music/lvl8/vibing");
            yield return 1.5f;

            cs08Campfire.Add(Wiggler.Create(0.6f, 3f, delegate (float v)
            {
                madeline.Sprite.Scale = Vector2.One * (1f + 0.1f * v);
            }, true, true));
            cs08Campfire.Level.Particles.Emit(NPC01_Theo.P_YOLO, 4, cs08Campfire.madeline.Position + new Vector2(-4f, -14f), Vector2.One * 3f);
            yield return 1f;
            cs08Campfire.player.Sprite.Play("halfWakeUp");
            yield return 0.25f;
            yield return Textbox.Say("DZ_CH8_MADELINE_INTRO");

            string key = "start";
            while (!string.IsNullOrEmpty(key) && cs08Campfire.nodes.ContainsKey(key))
            {
                cs08Campfire.currentOptionIndex = 0;
                cs08Campfire.currentOptions = new List<Option>();
                foreach (Option option in cs08Campfire.nodes[key])
                {
                    if (option.CanAsk(cs08Campfire.asked))
                        cs08Campfire.currentOptions.Add(option);
                }
                if (cs08Campfire.currentOptions.Count > 0)
                {
                    Audio.Play("event:/ui/game/chatoptions_appear");
                    while ((cs08Campfire.optionEase += Engine.DeltaTime * 4f) < 1.0)
                        yield return null;
                    cs08Campfire.optionEase = 1f;
                    yield return 0.25f;
                    while (!Input.MenuConfirm.Pressed)
                    {
                        if (Input.MenuUp.Pressed && cs08Campfire.currentOptionIndex > 0)
                        {
                            Audio.Play("event:/ui/game/chatoptions_roll_up");
                            --cs08Campfire.currentOptionIndex;
                        }
                        else if (Input.MenuDown.Pressed && cs08Campfire.currentOptionIndex < cs08Campfire.currentOptions.Count - 1)
                        {
                            Audio.Play("event:/ui/game/chatoptions_roll_down");
                            ++cs08Campfire.currentOptionIndex;
                        }
                        yield return null;
                    }
                    Audio.Play("event:/ui/game/chatoptions_select");
                    while ((cs08Campfire.optionEase -= Engine.DeltaTime * 4f) > 0.0)
                        yield return null;
                    optionEase = 0f;
                    Option selected = currentOptions[currentOptionIndex];
                    askedQuestions.Add(selected.Question);
                    currentOptions = null;
                    // Trigger order is fixed and matches the {trigger N} tags in Dialog/English.txt:
                    // 0 = beat, 1 = group selfie, 2 = drink.
                    yield return Textbox.Say(selected.Question.Answer, WaitABit, SelfieSequence, BeerSequence);
                    key = selected.Goto;
                    if (!string.IsNullOrEmpty(key))
                        selected = null;
                    else
                        break;
                }
                else
                    break;
            }

            FadeWipe fadeWipe2 = new FadeWipe(level, false);
            fadeWipe2.Duration = 3f;
            yield return fadeWipe2.Wait();
            cs08Campfire.EndCutscene(level);
        }

        private IEnumerator WaitABit()
        {
            yield return 0.8f;
        }

        private IEnumerator BeerSequence()
        {
            yield return 0.5f;
        }

        /// <summary>
        /// Trigger 1 for the selfie dialog option: Badeline takes out her phone
        /// and captures a group photo around the campfire.
        /// </summary>
        private IEnumerator SelfieSequence()
        {
            if (Scene is not Level level)
            {
                yield return 0.5f;
                yield break;
            }

            // Push in on the group before the shot
            Add(new Coroutine(level.ZoomTo(new Vector2(160f, 105f), 2f, 0.5f)));
            yield return 0.6f;

            // Badeline takes out her phone
            if (badeline != null)
            {
                if (badeline.Sprite != null && badeline.Sprite.Has("holdOutPhone"))
                    badeline.Sprite.Play("holdOutPhone");
                Audio.Play(selfie_audio_event, badeline.Position);
            }
            else
            {
                Audio.Play(selfie_audio_event, player.Position);
            }
            yield return 1f;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);

            // PictureRoutine flashes the screen, shows the photo, waits for input,
            // then plays it out and removes the entity from the scene itself.
            selfie = new Selfie(level);
            level.Add(selfie);
            yield return selfie.PictureRoutine("selfieCampfire");
            selfie = null;

            yield return 0.5f;
            yield return level.ZoomBack(0.5f);
            yield return 0.2f;

            if (badeline != null && badeline.Sprite != null && badeline.Sprite.Has("idle"))
                badeline.Sprite.Play("idle");
        }

        public override void Update()
        {
            if (currentOptions != null)
            {
                for (int i = 0; i < currentOptions.Count; i++)
                {
                    currentOptions[i].Update();
                    currentOptions[i].Highlight = Calc.Approach(
                        currentOptions[i].Highlight,
                        currentOptionIndex == i ? 1f : 0f,
                        Engine.DeltaTime * DIALOG_OPTION_EASE_SPEED);
                }
            }
            base.Update();
        }

        public override void Render()
        {
            if (Level == null || Level.Paused || currentOptions == null)
                return;

            for (int i = 0; i < currentOptions.Count; i++)
            {
                Vector2 position = new(
                    DIALOG_OPTION_LEFT,
                    DIALOG_OPTION_TOP + (DIALOG_OPTION_HEIGHT + Option.PADDING) * i);
                currentOptions[i].Render(position, optionEase);
            }
        }

        private IEnumerator BeerSequence()
        {
            yield return 0.5f;
        }

        public override void OnEnd(Level level)
        {
            if (!WasSkipped)
            {
                level.ZoomSnap(new Vector2(160f, 120f), 2f);
                FadeWipe fadeWipe = new FadeWipe(level, true);
                fadeWipe.Duration = 3f;
                Coroutine zoom = new Coroutine(level.ZoomBack(fadeWipe.Duration));
                fadeWipe.OnUpdate = f => zoom.Update();
            }
            if (selfie != null)
                selfie.RemoveSelf();
            level.Session.SetFlag("campfire_chat_mod");
            level.Session.SetFlag("starsbg", false);
            level.Session.SetFlag("duskbg", false);
            level.Session.Dreaming = true;
            level.Add(new StarJumpController());
            SetBloom(1f);
            bonfire.Activated = false;
            bonfire.SetMode(BigBonfire.Mode.Lit);

            if (madeline.Sprite != null)
            {
                madeline.Sprite.Play("sleep");
                madeline.Sprite.SetAnimationFrame(madeline.Sprite.CurrentAnimationTotalFrames - 1);
                madeline.Sprite.Scale.X = 1f;
            }
            madeline.Position = madelineCampfirePosition;

            player.Sprite.Play("asleep");
            player.Position = playerCampfirePosition;
            player.StateMachine.Locked = false;
            player.StateMachine.State = 15;
            player.Speed = Vector2.Zero;
            player.Facing = Facings.Left;
            level.Camera.Position = player.CameraTarget;
            if (WasSkipped)
                player.StateMachine.State = 0;
            RemoveSelf();
        }

        private void SetBloom(float add)
        {
            Level.Session.BloomBaseAdd = add;
            Level.Bloom.Base = AreaData.Get(Level).BloomBase + add;
        }

        public override void Update()
        {
            if (currentOptions != null)
            {
                for (int index = 0; index < currentOptions.Count; ++index)
                {
                    currentOptions[index].Update();
                    currentOptions[index].Highlight = Calc.Approach(currentOptions[index].Highlight, currentOptionIndex == index ? 1f : 0.0f, Engine.DeltaTime * 4f);
                }
            }
            base.Update();
        }

        public override void Render()
        {
            dialogNodes = new Dictionary<string, Option[]>();
            askedQuestions = new HashSet<Question>();
            currentOptions = new List<Option>();

            // Create questions with new dialogue options
            var questions = CreateDialogueQuestions();
            SetupDialogueNodes(questions);
        }

        /// <summary>
        /// Create all dialogue questions. Every id here needs a matching
        /// DZ_CH8_MADELINE_ASK_&lt;ID&gt; and DZ_CH8_MADELINE_SAY_&lt;ID&gt; pair in
        /// Dialog/English.txt, otherwise the option row renders as a raw key.
        /// </summary>
        private Dictionary<string, Question> CreateDialogueQuestions()
        {
            return new Dictionary<string, Question>
            {
                ["gaster"] = new Question("gaster"),
                ["journey"] = new Question("journey"),
                ["badeline"] = new Question("badeline"),
                ["goal"] = new Question("goal"),
                ["granny"] = new Question("granny"),
                ["void"] = new Question("void"),
                ["monster"] = new Question("monster"),
                ["els"] = new Question("els"),
                ["anything"] = new Question("anything"),
                [nameof(selfie)] = new Question(nameof(selfie)),
                ["sleep"] = new Question("sleep"),
                ["sleep_confirm"] = new Question("sleep_confirm"),
                ["sleep_cancel"] = new Question("sleep_cancel")
            };
        }

        /// <summary>
        /// Setup dialogue node structure. The selfie closes the night out, so it only
        /// unlocks once every other thread has been asked; "sleep" is the early exit
        /// and disappears at exactly the point the selfie becomes available.
        /// </summary>
        private void SetupDialogueNodes(Dictionary<string, Question> questions)
        {
            dialogNodes.Add("start", new Option[]
            {
                new Option(questions["gaster"], "start"),
                new Option(questions["journey"], "start"),
                new Option(questions["badeline"], "start").Require(questions["gaster"]),
                new Option(questions["goal"], "start").Require(questions["gaster"]),
                new Option(questions["granny"], "start").Require(questions["journey"]),
                new Option(questions["void"], "start").Require(questions["granny"]),
                new Option(questions["monster"], "start").Require(questions["void"]),
                new Option(questions["els"], "start").Require(questions["monster"]),
                new Option(questions["anything"], "start").Require(questions["badeline"], questions["goal"]),
                new Option(questions[nameof(selfie)], "")
                    .Require(questions["journey"], questions["monster"], questions["els"], questions["anything"]),
                new Option(questions["sleep"], "sleep")
                    .Require(questions["gaster"])
                    .ExcludedBy(questions["journey"], questions["monster"], questions["els"], questions["anything"])
                    .Repeatable()
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
            /// <summary>Eased 0-1 hover weight, driven by the cutscene's Update().</summary>
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

            public void Update()
            {
                if (Question.Portrait != null)
                    Question.Portrait.Update();
            }

            public void Render(Vector2 position, float ease)
            {
                float num1 = Ease.CubeOut(ease);
                float amount = Ease.CubeInOut(Highlight);
                position.Y += (float)(-32.0 * (1.0 - num1));
                position.X += amount * 32f;
                Color color1 = Color.Lerp(Color.Gray, Color.White, amount) * num1;
                float alpha = MathHelper.Lerp(0.6f, 1f, amount) * num1;
                Color color2 = Color.White * (float)(0.5 + amount * 0.5);
                if (Question.Textbox != null && GFX.Portraits.Has(Question.Textbox))
                    GFX.Portraits[Question.Textbox].Draw(position, Vector2.Zero, color1);
                Facings facings = Question.PortraitSide;
                if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                    facings = (Facings)(-(int)facings);
                float num2 = 100f;
                if (Question.Portrait != null)
                {
                    Question.Portrait.Scale = Vector2.One * (num2 / Question.PortraitSize);
                    if (facings == Facings.Right)
                    {
                        Question.Portrait.Position = position + new Vector2((float)(1380.0 - num2 * 0.5), 70f);
                        Question.Portrait.Scale.X *= -1f;
                    }
                    else
                        Question.Portrait.Position = position + new Vector2((float)(20.0 + num2 * 0.5), 70f);
                    Question.Portrait.Color = color2 * num1;
                    Question.Portrait.Render();
                }
                float num3 = (float)((140.0 - ActiveFont.LineHeight * 0.699999988079071) / 2.0);
                Vector2 position1 = new Vector2(0.0f, position.Y + 70f);
                Vector2 justify = new Vector2(0.0f, 0.5f);
                if (facings == Facings.Right)
                {
                    justify.X = 1f;
                    position1.X = (float)(position.X + 1400.0 - 20.0) - num3 - num2;
                }
                else
                    position1.X = position.X + 20f + num3 + num2;
                Question.AskText.Draw(position1, justify, Vector2.One * 0.7f, alpha);
            }
        }

        private class Question
        {
            public string Ask;
            public string Answer;
            public string Textbox;
            public FancyText.Text AskText;
            public Sprite Portrait;
            public Facings PortraitSide;
            public float PortraitSize;

            public Question(string id)
            {
                int maxLineWidth = 1828;
                Ask = "DZ_CH8_MADELINE_ASK_" + id.ToUpperInvariant();
                Answer = "DZ_CH8_MADELINE_SAY_" + id.ToUpperInvariant();
                AskText = FancyText.Parse(Dialog.Get(Ask), maxLineWidth, -1);
                foreach (FancyText.Node node in AskText.Nodes)
                {
                    if (node is FancyText.Portrait)
                    {
                        FancyText.Portrait portrait = node as FancyText.Portrait;
                        if (!GFX.PortraitsSpriteBank.Has(portrait.SpriteId))
                            continue;
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
    }
}

