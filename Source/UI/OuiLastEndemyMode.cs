using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.DZ
{
    /// <summary>
    /// "Last Endemy Mode" selection screen shown from the file-select slot.
    ///
    /// This is the DZ mod's CelesteNet online-multiplayer onboarding screen. It
    /// walks the player through a short paged guide (instructions, rules, a
    /// code-of-conduct agreement, support &amp; help info) and then asks them to
    /// accept the rules before enabling online play with other players.
    ///
    /// Selecting "yes" marks <see cref="DZModuleSettings.LastEndemyAgreed"/> and
    /// toggles <see cref="DZModuleSettings.LastEndemyMode"/> on. Uses the mod's
    /// own lastendemy UI audio events.
    /// </summary>
    public class OuiLastEndemyMode : Oui
    {
        public OuiFileSelectSlot FileSlot;

        private float fade;
        private List<Page> pages = new List<Page>();
        private int pageIndex;
        private int questionIndex = 1;
        private float questionEase;
        private Wiggler wiggler;
        private float dot;
        private FancyText.Text questionText;

        // Purple/magenta accent for the online multiplayer theme.
        private Color iconColor = Calc.HexToColor("c77dff");

        private float leftArrowEase;
        private float rightArrowEase;
        private EventInstance mainSfx;

        // Audio events (provided by the DZ audio bank).
        private const string Sfx_InfoWhistle = "event:/pusheen/ui/main/lastendemy_info_whistle";
        private const string Sfx_ButtonYes = "event:/pusheen/ui/main/lastendemy_button_yes";
        private const string Sfx_ButtonNo = "event:/pusheen/ui/main/lastendemy_button_no";

        public OuiLastEndemyMode()
        {
            Visible = false;
            Add(wiggler = Wiggler.Create(0.4f, 4f));
        }

        public override IEnumerator Enter(Oui from)
        {
            OuiLastEndemyMode self = this;
            self.Focused = false;
            self.Visible = true;
            self.pageIndex = 0;
            self.questionIndex = 1;
            self.questionEase = 0f;
            self.dot = 0f;

            bool alreadyEnabled = DZModule.Settings != null && DZModule.Settings.LastEndemyMode;
            self.questionText = FancyText.Parse(Dialog.Get("DZ_LASTENDEMY_ASK"), 1600, -1, defaultColor: Color.White);

            // Show the full guide when the player has not yet agreed to the
            // rules. Once agreed, jump straight to the enable/disable question.
            bool needsAgreement = DZModule.Settings == null || !DZModule.Settings.LastEndemyAgreed;
            if (needsAgreement || !alreadyEnabled)
            {
                for (int index = 0; Dialog.Has("DZ_LASTENDEMY_MODE_" + index); ++index)
                {
                    self.pages.Add(new Page
                    {
                        Text = FancyText.Parse(Dialog.Get("DZ_LASTENDEMY_MODE_" + index), 2000, -1, defaultColor: Color.White * 0.9f),
                        Ease = 0f
                    });
                }
                if (self.pages.Count > 0)
                    self.pages[0].Ease = 1f;
                self.mainSfx = Audio.Play(Sfx_InfoWhistle);
            }
            else
            {
                self.questionEase = 1f;
            }

            while (self.fade < 1f)
            {
                self.fade += Engine.DeltaTime * 4f;
                yield return null;
            }
            self.Focused = true;
            self.Add(new Coroutine(self.InputRoutine()));
        }

        public override IEnumerator Leave(Oui next)
        {
            OuiLastEndemyMode self = this;
            self.Focused = false;
            while (self.fade > 0f)
            {
                self.fade -= Engine.DeltaTime * 4f;
                yield return null;
            }
            if (self.mainSfx != null)
            {
                int _ = (int)self.mainSfx.release();
            }
            self.pages.Clear();
            self.Visible = false;
        }

        private IEnumerator InputRoutine()
        {
            OuiLastEndemyMode self = this;
            while (!Input.MenuCancel.Pressed)
            {
                int was = self.pageIndex;
                if ((Input.MenuConfirm.Pressed || Input.MenuRight.Pressed) && self.pageIndex < self.pages.Count)
                {
                    ++self.pageIndex;
                    Audio.Play("event:/ui/main/rollover_down");
                    Audio.SetParameter(self.mainSfx, "lastendemy_progress", self.pageIndex);
                }
                else if (Input.MenuLeft.Pressed && self.pageIndex > 0)
                {
                    Audio.Play("event:/ui/main/rollover_up");
                    --self.pageIndex;
                }

                if (was != self.pageIndex)
                {
                    if (was < self.pages.Count)
                    {
                        self.pages[was].Direction = Math.Sign(was - self.pageIndex);
                        while ((self.pages[was].Ease = Calc.Approach(self.pages[was].Ease, 0f, Engine.DeltaTime * 8f)) != 0f)
                            yield return null;
                    }
                    else
                    {
                        while ((self.questionEase = Calc.Approach(self.questionEase, 0f, Engine.DeltaTime * 8f)) != 0f)
                            yield return null;
                    }

                    if (self.pageIndex < self.pages.Count)
                    {
                        self.pages[self.pageIndex].Direction = Math.Sign(self.pageIndex - was);
                        while ((self.pages[self.pageIndex].Ease = Calc.Approach(self.pages[self.pageIndex].Ease, 1f, Engine.DeltaTime * 8f)) != 1f)
                            yield return null;
                    }
                    else
                    {
                        while ((self.questionEase = Calc.Approach(self.questionEase, 1f, Engine.DeltaTime * 8f)) != 1f)
                            yield return null;
                    }
                }

                if (self.pageIndex >= self.pages.Count)
                {
                    if (Input.MenuConfirm.Pressed)
                    {
                        bool enable = self.questionIndex == 0;
                        if (DZModule.Settings != null)
                        {
                            DZModule.Settings.LastEndemyMode = enable;
                            // Accepting "yes" records the rules agreement so the
                            // full guide is not forced on the player next time.
                            if (enable)
                                DZModule.Settings.LastEndemyAgreed = true;
                        }

                        // Refresh the slot buttons so the ON/OFF label updates.
                        if (self.FileSlot != null)
                            self.FileSlot.CreateButtons();

                        self.Focused = false;
                        self.Overworld.Goto<OuiFileSelect>();
                        Audio.Play(enable ? Sfx_ButtonYes : Sfx_ButtonNo);
                        Audio.SetParameter(self.mainSfx, "lastendemy_progress", enable ? 4f : 5f);
                        yield break;
                    }
                    if (Input.MenuUp.Pressed && self.questionIndex > 0)
                    {
                        Audio.Play("event:/ui/main/rollover_up");
                        --self.questionIndex;
                        self.wiggler.Start();
                    }
                    else if (Input.MenuDown.Pressed && self.questionIndex < 1)
                    {
                        Audio.Play("event:/ui/main/rollover_down");
                        ++self.questionIndex;
                        self.wiggler.Start();
                    }
                }
                yield return null;
            }
            self.Focused = false;
            self.Overworld.Goto<OuiFileSelect>();
            Audio.Play("event:/ui/main/button_back");
            Audio.SetParameter(self.mainSfx, "lastendemy_progress", 6f);
        }

        public override void Update()
        {
            dot = Calc.Approach(dot, pageIndex, Engine.DeltaTime * 8f);
            leftArrowEase = Calc.Approach(leftArrowEase, pageIndex > 0 ? 1f : 0f, Engine.DeltaTime * 4f);
            rightArrowEase = Calc.Approach(rightArrowEase, pageIndex < pages.Count ? 1f : 0f, Engine.DeltaTime * 4f);
            base.Update();
        }

        public override void Render()
        {
            if (!Visible)
                return;

            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * fade * 0.9f);

            for (int index = 0; index < pages.Count; ++index)
            {
                Page page = pages[index];
                float num = Ease.CubeOut(page.Ease);
                if (num > 0f)
                {
                    Vector2 position = new Vector2(960f, 620f);
                    position.X += (float)(page.Direction * (1.0 - num) * 256.0);
                    page.Text.DrawJustifyPerLine(position, new Vector2(0.5f, 0f), Vector2.One * 0.8f, num * fade);
                }
            }

            if (questionEase > 0f)
            {
                float num1 = Ease.CubeOut(questionEase);
                float num2 = wiggler.Value * 8f;
                Vector2 position = new Vector2((float)(960.0 + (1.0 - num1) * 256.0), 620f);
                float lineHeight = ActiveFont.LineHeight;
                questionText.DrawJustifyPerLine(position, new Vector2(0.5f, 0f), Vector2.One, num1 * fade);
                ActiveFont.DrawOutline(Dialog.Clean("DZ_LASTENDEMY_YES"),
                    position + new Vector2((float)((questionIndex == 0 ? num2 : 0.0) * 1.2000000476837158) * num1, (float)(lineHeight * 1.3999999761581421 + 10.0)),
                    new Vector2(0.5f, 0f), Vector2.One * 0.8f, SelectionColor(questionIndex == 0), 2f, Color.Black * num1 * fade);
                ActiveFont.DrawOutline(Dialog.Clean("DZ_LASTENDEMY_NO"),
                    position + new Vector2((float)((questionIndex == 1 ? num2 : 0.0) * 1.2000000476837158) * num1, (float)(lineHeight * 2.2000000476837158 + 20.0)),
                    new Vector2(0.5f, 0f), Vector2.One * 0.8f, SelectionColor(questionIndex == 1), 2f, Color.Black * num1 * fade);
            }

            if (pages.Count > 0)
            {
                int num3 = pages.Count + 1;
                MTexture mtexture = GFX.Gui["dot"];
                int num4 = mtexture.Width * num3;
                Vector2 vector2 = new Vector2(960f, (float)(960.0 - 40.0 * Ease.CubeOut(fade)));
                for (int index = 0; index < num3; ++index)
                    mtexture.DrawCentered(vector2 + new Vector2(-num4 / 2 + mtexture.Width * (index + 0.5f), 0f), Color.White * 0.25f);
                float x = (float)(1.0 + Calc.YoYo(dot % 1f) * 4.0);
                mtexture.DrawCentered(vector2 + new Vector2(-num4 / 2 + mtexture.Width * (dot + 0.5f), 0f), iconColor, new Vector2(x, 1f));
                GFX.Gui["dotarrow"].DrawCentered(vector2 + new Vector2(-num4 / 2 - 50, (float)(32.0 * (1.0 - Ease.CubeOut(leftArrowEase)))), iconColor * leftArrowEase, new Vector2(-1f, 1f));
                GFX.Gui["dotarrow"].DrawCentered(vector2 + new Vector2(num4 / 2 + 50, (float)(32.0 * (1.0 - Ease.CubeOut(rightArrowEase)))), iconColor * rightArrowEase);
            }

            // Header icon. Reuses the vanilla assist-mode GUI texture, tinted to
            // the online accent. Replace with "dz_lastendemy" once art is added
            // under Graphics/Gui/.
            MTexture header = GFX.Gui["assistmode"];
            if (header != null)
                header.DrawJustified(new Vector2(960f, (float)(540.0 + 64.0 * Ease.CubeOut(fade))), new Vector2(0.5f, 1f), iconColor * fade);
        }

        private Color SelectionColor(bool selected) =>
            selected ? (Settings.Instance.DisableFlashes || Scene.BetweenInterval(0.1f) ? TextMenu.HighlightColorA : TextMenu.HighlightColorB) * fade : Color.White * fade;

        private class Page
        {
            public FancyText.Text Text;
            public float Ease;
            public float Direction;
        }
    }
}
