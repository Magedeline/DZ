namespace MaggyHelper
{
    [HotReloadable]
    public class WaveFazeSlammerPage04 : WaveFazeSlammerPage
    {
        private FancyText.Text introText;
        private FancyText.Text factsText;
        private FancyText.Text solutionText;
        private FancyText.Text bonusText;
        private int introIndex;
        private int factsIndex;
        private int solutionIndex;
        private int bonusIndex;
        private AreaCompleteTitle damageTitle;
        private float damageEase;
        private bool showDamageNumber;
        private bool showBonus;

        public WaveFazeSlammerPage04()
        {
            Transition = Transitions.Spiral;
            ClearColor = Calc.HexToColor("e6b8af");
        }

        public override IEnumerator Routine()
        {
            yield return 0.5f;
            
            // Show intro
            introText = FancyText.Parse(Dialog.Get("WAVEFAZESLAMMER_PAGE4_INTRO"), Width - 300, 32, defaultColor: Color.Black * 0.7f);
            float delay = 0.0f;
            for (; introIndex < introText.Nodes.Count; ++introIndex)
            {
                if (introText.Nodes[introIndex] is FancyText.NewLine)
                {
                    yield return 0.3f;
                }
                else
                {
                    delay += 0.008f;
                    if (delay >= 0.016f)
                    {
                        delay -= 0.016f;
                        yield return 0.016f;
                    }
                }
            }
            
            yield return PressButton();
            
            // Show facts about damage
            Audio.Play("event:/new_content/game/10_farewell/ppt_mouseclick");
            factsText = FancyText.Parse(Dialog.Get("WAVEFAZESLAMMER_PAGE4_FACTS"), Width - 300, 28, defaultColor: Color.Black * 0.7f);
            delay = 0.0f;
            for (; factsIndex < factsText.Nodes.Count; ++factsIndex)
            {
                if (factsText.Nodes[factsIndex] is FancyText.NewLine)
                {
                    yield return 0.25f;
                }
                else
                {
                    delay += 0.006f;
                    if (delay >= 0.016f)
                    {
                        delay -= 0.016f;
                        yield return 0.016f;
                    }
                }
            }
            
            yield return PressButton();
            
            // Show solution
            Audio.Play("event:/new_content/game/10_farewell/ppt_its_easy");
            solutionText = FancyText.Parse(Dialog.Get("WAVEFAZESLAMMER_PAGE4_SOLUTION"), Width - 300, 28, defaultColor: Color.Black * 0.7f);
            delay = 0.0f;
            
            for (; solutionIndex < solutionText.Nodes.Count; ++solutionIndex)
            {
                if (solutionText.Nodes[solutionIndex] is FancyText.NewLine)
                {
                    yield return 0.2f;
                }
                else
                {
                    delay += 0.006f;
                    if (delay >= 0.016f)
                    {
                        delay -= 0.016f;
                        yield return 0.016f;
                    }
                }
                
                // Show big damage number when we reach the total
                if (solutionIndex == solutionText.Nodes.Count / 2 && !showDamageNumber)
                {
                    showDamageNumber = true;
                    Audio.Play("event:/new_content/game/10_farewell/ppt_happy_wavedashing");
                    damageTitle = new AreaCompleteTitle(new Vector2(Width / 2f, Height / 2f - 100f), "335 DAMAGE!", 3f, true);
                }
            }
            
            yield return PressButton();
            
            // Show bonus copy ability info
            showBonus = true;
            Audio.Play("event:/new_content/game/10_farewell/ppt_wavedash_whoosh");
            bonusText = FancyText.Parse(Dialog.Get("WAVEFAZESLAMMER_PAGE4_BONUS"), Width - 300, 28, defaultColor: Color.Black * 0.7f);
            delay = 0.0f;
            
            for (; bonusIndex < bonusText.Nodes.Count; ++bonusIndex)
            {
                if (bonusText.Nodes[bonusIndex] is FancyText.NewLine)
                {
                    yield return 0.2f;
                }
                else
                {
                    delay += 0.006f;
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
            if (damageTitle != null)
            {
                damageTitle.Update();
            }
            {
                solutionText.Draw(new Vector2(150f, 320f), new Vector2(0.0f, 0.0f), Vector2.One * 0.85f, 1f, end: solutionIndex);
            }
            
            // Render bonus copy ability info
            if (bonusText != null && bonusIndex > 0 && showBonus)
            {
                bonusText.Draw(new Vector2(150f, 280f), new Vector2(0.0f, 0.0f), Vector2.One * 0.85f, 1f, end: bonusIndex);
            }
            
            if (showDamageNumber)
            {
                damageEase = Calc.Approach(damageEase, 1f, Engine.DeltaTime * 2f);
            }
        }

        public override void Render()
        {
            ActiveFont.DrawOutline(Dialog.Clean("WAVEFAZESLAMMER_PAGE4_TITLE"), new Vector2(Width / 2f, 80f), new Vector2(0.5f, 0f), Vector2.One * 1.5f, Color.White, 2f, Color.Black);
            
            // Render intro
            if (introText != null)
            {
                introText.Draw(new Vector2(150f, 200f), new Vector2(0.0f, 0.0f), Vector2.One * 1.1f, 1f, end: introIndex);
            }
            
            // Render facts
            if (factsText != null && factsIndex > 0)
            {
                factsText.Draw(new Vector2(150f, 320f), new Vector2(0.0f, 0.0f), Vector2.One * 0.85f, 1f, end: factsIndex);
            }
            
            // Render solution
            if (solutionText != null && solutionIndex > 0)
            {
                solutionText.Draw(new Vector2(150f, 320f), new Vector2(0.0f, 0.0f), Vector2.One * 0.85f, 1f, end: solutionIndex);
            }
            
            // Render big damage number with animation
            if (showDamageNumber && damageTitle != null)
            {
                float scale = 1f + (1f - damageEase) * 2f;
                float alpha = damageEase;
                
                // Draw pulsing effect
                if (damageEase >= 1f)
                {
                    float pulse = (float)Math.Sin(Engine.Scene.TimeActive * 3f) * 0.1f + 1f;
                    scale = pulse;
                }
                
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin();
                
                damageTitle.Render();
                
                // Draw additional visual effects
                if (damageEase >= 0.5f)
                {
                    float effectAlpha = Calc.Clamp(damageEase * 2f - 1f, 0f, 1f);
                    
                    // Draw impact lines
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = (MathF.PI * 2f / 8f) * i;
                        Vector2 dir = Calc.AngleToVector(angle, 1f);
                        Vector2 start = new Vector2(Width / 2f, Height / 2f - 100f);
                        Vector2 end = start + dir * 150f * effectAlpha;
                        Draw.Line(start, end, Color.Orange * effectAlpha, 3f);
                    }
                }
            }
        }
    }
}
