namespace Celeste
{
    [HotReloadable]
    public class WaveFazeSlammerPage03 : WaveFazeSlammerPage
    {
        private FancyText.Text mistakeTitleText;
        private FancyText.Text mistakeInfoText;
        private FancyText.Text successTitleText;
        private FancyText.Text successInfoText;
        private WaveFazeSlammerPlaybackTutorial correctTutorial;
        private WaveFazeSlammerPlaybackTutorial wrongTutorial;
        private int mistakeTitleIndex;
        private int mistakeInfoIndex;
        private int successTitleIndex;
        private int successInfoIndex;
        private float displayTimer;
        private bool showingCorrect = false;

        public WaveFazeSlammerPage03()
        {
            Transition = Transitions.Blocky;
            ClearColor = Calc.HexToColor("fff2cc");
        }

        public override void Added(WaveFazeSlammerPresentation presentation)
        {
            base.Added(presentation);
            
            // Tutorial showing correct execution
            correctTutorial = new WaveFazeSlammerPlaybackTutorial(
                "wavefazeslammer",
                new Vector2(-150f, 0f),
                new Vector2(1f, -0.4f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, -1f)
            );
            
            correctTutorial.OnRender = () => 
            {
                Draw.Rect(-100f, 80f, 200f, 8f, Color.Green * 0.7f);
                Draw.Rect(120f, -60f, 8f, 140f, Color.Green * 0.7f);
            };
            
            // Tutorial showing wrong execution (for comparison)
            wrongTutorial = new WaveFazeSlammerPlaybackTutorial(
                "wavefazeslammer",
                new Vector2(-150f, 0f),
                new Vector2(1f, -0.4f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(0.5f, -0.5f) // Wrong angle
            );
            
            wrongTutorial.OnRender = () => 
            {
                Draw.Rect(-100f, 80f, 200f, 8f, Color.Red * 0.7f);
                Draw.Rect(120f, -60f, 8f, 140f, Color.Red * 0.7f);
                // Draw X mark
                Draw.Line(-20f, -20f, 20f, 20f, Color.Red, 4f);
                Draw.Line(20f, -20f, -20f, 20f, Color.Red, 4f);
            };
        }

        public override IEnumerator Routine()
        {
            yield return 0.5f;
            
            // Show mistake title
            mistakeTitleText = FancyText.Parse(Dialog.Get("WAVEFAZESLAMMER_PAGE3_MISTAKE_TITLE"), Width - 240, 32, defaultColor: Color.Black * 0.7f);
            float delay = 0.0f;
            for (; mistakeTitleIndex < mistakeTitleText.Nodes.Count; ++mistakeTitleIndex)
            {
                delay += 0.005f;
                if (delay >= 0.016f)
                {
                    delay -= 0.016f;
                    yield return 0.016f;
                }
            }
            
            yield return 0.3f;
            
            // Show mistake info
            mistakeInfoText = FancyText.Parse(Dialog.Get("WAVEFAZESLAMMER_PAGE3_MISTAKE_INFO"), Width - 260, 28, defaultColor: Color.Black * 0.7f);
            delay = 0.0f;
            for (; mistakeInfoIndex < mistakeInfoText.Nodes.Count; ++mistakeInfoIndex)
            {
                if (mistakeInfoText.Nodes[mistakeInfoIndex] is FancyText.NewLine)
                {
                    yield return 0.2f;
                }
                else
                {
                    delay += 0.005f;
                    if (delay >= 0.016f)
                    {
                        delay -= 0.016f;
                        yield return 0.016f;
                    }
                }
            }
            
            yield return PressButton();
            
            // Transition to showing correct execution
            showingCorrect = true;
            Audio.Play("event:/new_content/game/10_farewell/ppt_wavedash_whoosh");
            
            yield return 0.5f;
            
            // Show success title
            successTitleText = FancyText.Parse(Dialog.Get("WAVEFAZESLAMMER_PAGE3_SUCCESS_TITLE"), Width - 240, 32, defaultColor: Color.Black * 0.7f);
            delay = 0.0f;
            for (; successTitleIndex < successTitleText.Nodes.Count; ++successTitleIndex)
            {
                delay += 0.005f;
                if (delay >= 0.016f)
                {
                    delay -= 0.016f;
                    yield return 0.016f;
                }
            }
            
            yield return 0.3f;
            
            // Show success info
            successInfoText = FancyText.Parse(Dialog.Get("WAVEFAZESLAMMER_PAGE3_SUCCESS_INFO"), Width - 260, 28, defaultColor: Color.Black * 0.7f);
            delay = 0.0f;
            for (; successInfoIndex < successInfoText.Nodes.Count; ++successInfoIndex)
            {
                if (successInfoText.Nodes[successInfoIndex] is FancyText.NewLine)
                {
                    yield return 0.2f;
                }
                else
                {
                    delay += 0.005f;
                    if (delay >= 0.016f)
                    {
                        delay -= 0.016f;
                        yield return 0.016f;
                    }
                }
            }
            
            yield return 1f;
        }

        public override void Update()
        {
            displayTimer += Engine.DeltaTime;
            
            if (showingCorrect)
            {
                if (correctTutorial != null)
                    correctTutorial.Update();
            }
            else
            {
                if (wrongTutorial != null)
                    wrongTutorial.Update();
            }
        }

        public override void Render()
        {
            ActiveFont.DrawOutline(Dialog.Clean("WAVEFAZESLAMMER_PAGE3_TITLE"), new Vector2(Width / 2f, 80f), new Vector2(0.5f, 0f), Vector2.One * 1.5f, Color.White, 2f, Color.Black);
            
            if (!showingCorrect)
            {
                // Show mistakes section
                if (mistakeTitleText != null)
                {
                    mistakeTitleText.Draw(new Vector2(130f, 180f), new Vector2(0.0f, 0.0f), Vector2.One * 1.2f, 1f, end: mistakeTitleIndex);
                }
                
                if (wrongTutorial != null)
                {
                    wrongTutorial.Render(new Vector2(Width / 2f, Height / 2f - 100f), 3f);
                }
                
                if (mistakeInfoText != null)
                {
                    mistakeInfoText.Draw(new Vector2(130f, Height - 500f), new Vector2(0.0f, 0.0f), Vector2.One * 0.9f, 1f, end: mistakeInfoIndex);
                }
            }
            else
            {
                // Show correct execution section
                if (successTitleText != null)
                {
                    successTitleText.Draw(new Vector2(130f, 180f), new Vector2(0.0f, 0.0f), Vector2.One * 1.2f, 1f, end: successTitleIndex);
                }
                
                if (correctTutorial != null)
                {
                    correctTutorial.Render(new Vector2(Width / 2f, Height / 2f - 100f), 3f);
                }
                
                if (successInfoText != null)
                {
                    successInfoText.Draw(new Vector2(130f, Height - 500f), new Vector2(0.0f, 0.0f), Vector2.One * 0.9f, 1f, end: successInfoIndex);
                }
            }
        }
    }
}

