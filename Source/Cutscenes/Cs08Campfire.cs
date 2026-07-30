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

        private NPC madeline;
        private Player player;
        private BigBonfire bonfire;
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
            Tag = (int)Tags.HUD;
            this.madeline = madeline;
            this.player = player;

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

            FadeWipe fade = new FadeWipe(level, false);
            fade.Duration = 1.5f;
            fade.EndTimer = 2.5f;
            yield return fade.Wait();
            cs08Campfire.bonfire.SetMode(BigBonfire.Mode.Lit);
            yield return 2.45f;
            camTo.Cancel();

            cs08Campfire.madeline.Position = cs08Campfire.madelineCampfirePosition;
            if (cs08Campfire.madeline.Sprite != null)
            {
                cs08Campfire.madeline.Sprite.Play("sleep");
                cs08Campfire.madeline.Sprite.SetAnimationFrame(cs08Campfire.madeline.Sprite.CurrentAnimationTotalFrames - 1);
            }

            cs08Campfire.player.Position = cs08Campfire.playerCampfirePosition;
            cs08Campfire.player.Facing = Facings.Left;
            cs08Campfire.player.Sprite.Play("asleep");

            if (cs08Campfire.badeline != null)
            {
                cs08Campfire.badeline.Position = cs08Campfire.bonfire.Position + new Vector2(50f, -16f);
                if (cs08Campfire.badeline.Sprite != null)
                    cs08Campfire.badeline.Sprite.Scale.X = -1f;
            }

            if (cs08Campfire.ralsei != null)
            {
                cs08Campfire.ralsei.Position = cs08Campfire.bonfire.Position + new Vector2(-80f, -16f);
                if (cs08Campfire.ralsei.Sprite != null)
                    cs08Campfire.ralsei.Sprite.Scale.X = 1f;
            }

            level.Session.SetFlag("starsbg");
            level.Session.SetFlag("duskbg", false);
            fade.EndTimer = 0.0f;
            FadeWipe fadeWipe1 = new FadeWipe(level, true);
            yield return null;
            level.ResetZoom();
            level.Camera.Position = new Vector2(cs08Campfire.bonfire.X - 160f, cs08Campfire.bonfire.Y - 140f);
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
                    Option selected = cs08Campfire.currentOptions[cs08Campfire.currentOptionIndex];
                    cs08Campfire.asked.Add(selected.Question);
                    cs08Campfire.currentOptions = null;
                    yield return Textbox.Say(selected.Question.Answer, cs08Campfire.WaitABit, cs08Campfire.SelfieSequence, cs08Campfire.BeerSequence);
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

        private IEnumerator SelfieSequence()
        {
            Cs08Campfire cs08Campfire = this;
            Level level = Scene as Level;
            if (level == null)
            {
                yield return 0.5f;
                yield break;
            }

            if (cs08Campfire.badeline != null && cs08Campfire.badeline.Sprite != null && cs08Campfire.badeline.Sprite.Has("holdOutPhone"))
            {
                cs08Campfire.badeline.Sprite.Play("holdOutPhone");
                yield return 1.5f;
            }

            cs08Campfire.selfie = new Selfie(cs08Campfire.SceneAs<Level>());
            cs08Campfire.Scene.Add(cs08Campfire.selfie);
            yield return cs08Campfire.selfie.PictureRoutine("selfieCampfire");
            cs08Campfire.selfie = null;
            yield return 0.5f;
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
            if (Level.Paused || currentOptions == null)
                return;
            int num = 0;
            foreach (Option currentOption in currentOptions)
            {
                currentOption.Render(new Vector2(260f, (float)(120.0 + 160.0 * num)), optionEase);
                ++num;
            }
        }

        private class Option
        {
            public Question Question;
            public string Goto;
            public List<Question> OnlyAppearIfAsked;
            public List<Question> DoNotAppearIfAsked;
            public bool CanRepeat;
            public float Highlight;
            public const float Width = 1400f;
            public const float Height = 140f;
            public const float Padding = 20f;
            public const float TextScale = 0.7f;

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

