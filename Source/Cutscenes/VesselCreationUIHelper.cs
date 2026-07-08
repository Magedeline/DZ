using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Encapsulates manual UI rendering logic for VesselCreationVignette.
    /// Separates rendering concerns from the main cutscene sequence.
    /// </summary>
    public static class VesselCreationUIHelper
    {
        #region Vessel Graphics Rendering

        public static void RenderVesselGraphics(
            float vesselAlpha,
            float soulBlurAlpha,
            Vector2 vesselPosition,
            MTexture depthTexture,
            MTexture soulBlurTexture,
            MTexture pinkSoulBlurTexture,
            MTexture[] gonerBodyTextures,
            MTexture[] gonerHeadTextures,
            MTexture[] gonerLegTextures,
            int selectedBodyIndex,
            int selectedHeadIndex,
            int selectedLegIndex)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);

            // Render depth layer as full-screen background at full opacity
            if (depthTexture != null)
            {
                float depthScaleX = 1920f / depthTexture.Width;
                float depthScaleY = 1080f / depthTexture.Height;
                float depthScale = Math.Max(depthScaleX, depthScaleY);
                depthTexture.Draw(Vector2.Zero, Vector2.Zero, Color.White * vesselAlpha, depthScale);
            }

            // Render soul blur effects slightly enlarged (1.5x) but not screen-filling
            const float SOUL_BLUR_SCALE = 1.5f;
            if (soulBlurTexture != null)
            {
                Vector2 blurPos = vesselPosition - new Vector2(soulBlurTexture.Width * SOUL_BLUR_SCALE / 2f, soulBlurTexture.Height * SOUL_BLUR_SCALE / 2f);
                soulBlurTexture.Draw(blurPos, Vector2.Zero, Color.White * soulBlurAlpha, SOUL_BLUR_SCALE);
            }

            if (pinkSoulBlurTexture != null)
            {
                Vector2 pinkBlurPos = vesselPosition - new Vector2(pinkSoulBlurTexture.Width * SOUL_BLUR_SCALE / 2f, pinkSoulBlurTexture.Height * SOUL_BLUR_SCALE / 2f);
                pinkSoulBlurTexture.Draw(pinkBlurPos, Vector2.Zero, Color.White * (soulBlurAlpha * 0.6f), SOUL_BLUR_SCALE);
            }

            // Render vessel parts stacked: legs at bottom, body in center, head at top
            MTexture bodyTex = (gonerBodyTextures.Length > 0) ? gonerBodyTextures[Math.Clamp(selectedBodyIndex, 0, gonerBodyTextures.Length - 1)] : null;
            MTexture headTex = (gonerHeadTextures.Length > 0) ? gonerHeadTextures[Math.Clamp(selectedHeadIndex, 0, gonerHeadTextures.Length - 1)] : null;
            MTexture legTex = (gonerLegTextures.Length > 0) ? gonerLegTextures[Math.Clamp(selectedLegIndex, 0, gonerLegTextures.Length - 1)] : null;
            float bodyHalfH = bodyTex != null ? bodyTex.Height / 2f : 60f;

            // Legs (bottom)
            if (legTex != null)
            {
                Vector2 legPos = new Vector2(
                    vesselPosition.X - legTex.Width / 2f,
                    vesselPosition.Y + bodyHalfH);
                legTex.Draw(legPos, Vector2.Zero, Color.White * vesselAlpha);
            }

            // Body (center)
            if (bodyTex != null)
            {
                Vector2 bodyPos = vesselPosition - new Vector2(bodyTex.Width / 2f, bodyTex.Height / 2f);
                bodyTex.Draw(bodyPos, Vector2.Zero, Color.White * vesselAlpha);
            }

            // Head (top)
            if (headTex != null)
            {
                Vector2 headPos = new Vector2(
                    vesselPosition.X - headTex.Width / 2f,
                    vesselPosition.Y - bodyHalfH - headTex.Height);
                headTex.Draw(headPos, Vector2.Zero, Color.White * vesselAlpha);
            }

            Draw.SpriteBatch.End();
        }

        #endregion

        #region Cycler Rendering

        public static void RenderVesselCycler(
            int currentPhase,
            int selectedLegIndex,
            int selectedBodyIndex,
            int selectedHeadIndex,
            int vesselCyclerCount,
            Vector2 vesselPosition)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);

            // Phase values: 1=LegSelection, 2=TorsoSelection, 3=HeadSelection
            string label = currentPhase switch
            {
                1 => $"< Legs {selectedLegIndex + 1} / {vesselCyclerCount} >",
                2 => $"< Body {selectedBodyIndex + 1} / {vesselCyclerCount} >",
                3 => $"< Head {selectedHeadIndex + 1} / {vesselCyclerCount} >",
                _ => string.Empty
            };

            if (!string.IsNullOrEmpty(label))
            {
                Vector2 center = new Vector2(Engine.Width / 2f, vesselPosition.Y + 145f);
                ActiveFont.DrawOutline(label, center, new Vector2(0.5f, 0.5f), Vector2.One * 0.65f, Color.White, 2f, Color.Black);
                ActiveFont.DrawOutline("Left / Right  *  Confirm to select", center + new Vector2(0f, 42f), new Vector2(0.5f, 0.5f), Vector2.One * 0.4f, Color.Gray, 2f, Color.Black);
            }

            Draw.SpriteBatch.End();
        }

        #endregion

        #region Text Input Rendering

        public static void RenderTextInput(
            float textInputEase,
            string textInputPrompt,
            string textInputValue,
            int textInputCursorIndex,
            int textInputSelectionAnchor,
            int textInputMaxLength,
            bool textInputPaletteActive,
            int textInputPaletteRow,
            int textInputPaletteColumn,
            string[][] textInputPaletteRows)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);

            float overlayAlpha = 0.9f * textInputEase;
            float boxWidth = 860f;
            float boxHeight = 78f;
            float boxX = (Engine.ViewWidth - boxWidth) / 2f;
            float boxY = Engine.ViewHeight / 2f + 60f;

            // Dark overlay behind input
            Draw.Rect(0f, 0f, Engine.ViewWidth, Engine.ViewHeight, Color.Black * overlayAlpha * 0.5f);

            // Prompt text
            if (!string.IsNullOrEmpty(textInputPrompt))
            {
                ActiveFont.Draw(textInputPrompt, new Vector2(Engine.ViewWidth / 2f, boxY - 70f), new Vector2(0.5f, 0.5f), Vector2.One * 0.65f, Color.White * textInputEase);
            }

            // Input box background
            Draw.Rect(boxX, boxY, boxWidth, boxHeight, Color.DarkGray * overlayAlpha);
            Draw.Rect(boxX + 2f, boxY + 2f, boxWidth - 4f, boxHeight - 4f, Color.Black * overlayAlpha);

            // Input text
            ActiveFont.Draw(textInputValue, new Vector2(boxX + 20f, boxY + boxHeight / 2f), new Vector2(0f, 0.5f), Vector2.One * TEXT_INPUT_SCALE, Color.White * textInputEase);

            // Cursor
            if (!textInputPaletteActive && textInputEase > 0.5f)
            {
                float cursorX = boxX + 20f + ActiveFont.Measure(textInputValue.Substring(0, Math.Min(textInputCursorIndex, textInputValue.Length))).X * TEXT_INPUT_SCALE;
                float cursorAlpha = (float)(Math.Sin(Engine.Scene.TimeActive * 8f) * 0.5f + 0.5f) * textInputEase;
                Draw.Rect(cursorX, boxY + 10f, 2f, boxHeight - 20f, Color.White * cursorAlpha);
            }

            // Character count
            string countText = $"{textInputValue.Length}/{textInputMaxLength}";
            ActiveFont.Draw(countText, new Vector2(boxX + boxWidth - 20f, boxY + boxHeight + 10f), new Vector2(1f, 0f), Vector2.One * 0.4f, Color.Gray * textInputEase);

            // Palette keyboard
            if (textInputPaletteActive)
            {
                RenderPaletteKeyboard(textInputPaletteRows, textInputPaletteRow, textInputPaletteColumn, textInputEase);
            }
            else
            {
                ActiveFont.Draw("Press DOWN for keyboard", new Vector2(Engine.ViewWidth / 2f, boxY + boxHeight + 30f), new Vector2(0.5f, 0f), Vector2.One * 0.35f, Color.Gray * textInputEase);
            }

            Draw.SpriteBatch.End();
        }

        private static void RenderPaletteKeyboard(string[][] paletteRows, int selectedRow, int selectedCol, float ease)
        {
            float startY = Engine.ViewHeight / 2f + 160f;
            float rowHeight = 45f;
            float charWidth = 55f;

            for (int row = 0; row < paletteRows.Length; row++)
            {
                string[] chars = paletteRows[row];
                float rowWidth = chars.Length * charWidth;
                float startX = (Engine.ViewWidth - rowWidth) / 2f;

                for (int col = 0; col < chars.Length; col++)
                {
                    bool isSelected = (row == selectedRow && col == selectedCol);
                    Vector2 pos = new Vector2(startX + col * charWidth + charWidth / 2f, startY + row * rowHeight);
                    Color bgColor = isSelected ? Color.Gold : Color.DarkGray;
                    Color textColor = isSelected ? Color.Black : Color.White;

                    Draw.Rect(pos.X - charWidth / 2f + 2f, pos.Y - rowHeight / 2f + 2f, charWidth - 4f, rowHeight - 4f, bgColor * ease);
                    ActiveFont.Draw(chars[col], pos, new Vector2(0.5f, 0.5f), Vector2.One * 0.45f, textColor * ease);
                }
            }
        }

        private const float TEXT_INPUT_SCALE = 0.7f;

        #endregion
    }
}
