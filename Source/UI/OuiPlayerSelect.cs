using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Entities;

namespace Celeste.Mod.DZ
{
    /// <summary>
    /// Player-selection screen shown from the chapter panel before entering a level.
    ///
    /// Lets the player choose between Kirby and Madeline, optionally respecting
    /// the map's recommended character.  An "Auto-select" toggle (persisted in
    /// <see cref="DZModuleSettings.PlayerAutoSelect"/>) lets the player skip
    /// this screen in the future and have the recommended character applied
    /// automatically.
    ///
    /// Flow:
    ///   OuiChapterPanel  ──(Play pressed)──►  OuiPlayerSelect  ──(confirmed)──►  Level
    ///                                                           ──(cancelled)──►  OuiChapterPanel
    /// </summary>
    public class OuiPlayerSelect : Oui
    {
        // ── State ────────────────────────────────────────────────────────────

        /// <summary>
        /// Called when the player confirms their choice.
        /// The bool argument is the pending auto-select state.
        /// </summary>
        public Action OnConfirm;

        /// <summary>
        /// Called when the player cancels (presses Back).
        /// </summary>
        public Action OnCancel;

        // UI layout: rows
        // 0 = Kirby, 1 = Madeline, 2 = Auto-select toggle
        private int selectedRow = 0;
        private const int RowCount = 3;
        private const int RowKirby = 0;
        private const int RowMadeline = 1;
        private const int RowAuto = 2;

        // Pending selection (confirmed on MenuConfirm)
        private PlayerSelectionManager.PlayerType pendingPlayer;
        private bool pendingAutoSelect;

        // Recommended player for the current map (nullable)
        private PlayerSelectionManager.PlayerType? recommendedPlayer;

        // Animation
        private float fade;
        private float contentEase;
        private Wiggler wiggler;

        // Colours
        private static readonly Color ColorKirby    = Calc.HexToColor("ff99cc"); // pink
        private static readonly Color ColorMadeline = Calc.HexToColor("99ccff"); // blue
        private static readonly Color ColorAuto     = Calc.HexToColor("ccffaa"); // green
        private static readonly Color ColorRecommend= Calc.HexToColor("ffdd55"); // gold

        // Sound effects (from GUIDs.txt)
        private const string Sfx_Open         = "event:/pusheen/ui/world_map/chapter/playerselect_open";
        private const string Sfx_Kirby        = "event:/pusheen/ui/world_map/chapter/playerselect_kirby";
        private const string Sfx_Madeline     = "event:/pusheen/ui/world_map/chapter/playerselect_madeline";
        private const string Sfx_Back         = "event:/pusheen/ui/world_map/chapter/playerselect_back";
        private const string Sfx_Confirm      = "event:/pusheen/ui/world_map/chapter/playerselect_confirm";
        private const string Sfx_AutoOn       = "event:/pusheen/ui/world_map/chapter/playerselect_auto_on";
        private const string Sfx_AutoOff      = "event:/pusheen/ui/world_map/chapter/playerselect_auto_off";
        private const string Sfx_RolloverUp   = "event:/pusheen/ui/main/rollover_up";
        private const string Sfx_RolloverDown = "event:/pusheen/ui/main/rollover_down";

        // ── Constructor ──────────────────────────────────────────────────────

        public OuiPlayerSelect()
        {
            Visible = false;
            Add(wiggler = Wiggler.Create(0.4f, 4f));
        }

        // ── Oui lifecycle ────────────────────────────────────────────────────

        public override IEnumerator Enter(Oui from)
        {
            Visible = true;
            Focused = false;
            fade = 0f;
            contentEase = 0f;

            // Snapshot current state so the player can browse without committing
            pendingPlayer = PlayerSelectionManager.GetSelectedPlayer();
            pendingAutoSelect = PlayerSelectionManager.AutoSelectEnabled;
            recommendedPlayer = PlayerSelectionManager.GetRecommendedPlayer();

            // Default cursor to the currently selected player row
            selectedRow = pendingPlayer == PlayerSelectionManager.PlayerType.Kirby
                ? RowKirby : RowMadeline;

            Audio.Play(Sfx_Open);

            while (fade < 1f)
            {
                fade = Calc.Approach(fade, 1f, Engine.DeltaTime * 4f);
                contentEase = Calc.Approach(contentEase, 1f, Engine.DeltaTime * 3f);
                yield return null;
            }
            Focused = true;
            Add(new Coroutine(InputRoutine()));
        }

        public override IEnumerator Leave(Oui next)
        {
            Focused = false;
            while (fade > 0f)
            {
                fade = Calc.Approach(fade, 0f, Engine.DeltaTime * 4f);
                yield return null;
            }
            Visible = false;
        }

        // ── Input loop ───────────────────────────────────────────────────────

        private IEnumerator InputRoutine()
        {
            while (true)
            {
                // Navigate rows
                if (Input.MenuDown.Pressed)
                {
                    Audio.Play(Sfx_RolloverDown);
                    selectedRow = (selectedRow + 1) % RowCount;
                    wiggler.Start();
                }
                else if (Input.MenuUp.Pressed)
                {
                    Audio.Play(Sfx_RolloverUp);
                    selectedRow = (selectedRow - 1 + RowCount) % RowCount;
                    wiggler.Start();
                }

                if (Input.MenuConfirm.Pressed)
                {
                    if (selectedRow == RowKirby)
                    {
                        pendingPlayer = PlayerSelectionManager.PlayerType.Kirby;
                        Commit();
                        yield break;
                    }
                    else if (selectedRow == RowMadeline)
                    {
                        pendingPlayer = PlayerSelectionManager.PlayerType.Madeline;
                        Commit();
                        yield break;
                    }
                    else if (selectedRow == RowAuto)
                    {
                        // Toggle auto-select
                        pendingAutoSelect = !pendingAutoSelect;
                        Audio.Play(pendingAutoSelect ? Sfx_AutoOn : Sfx_AutoOff);
                        wiggler.Start();
                    }
                }

                // Left/Right on the player rows: quick-swap
                if (Input.MenuLeft.Pressed || Input.MenuRight.Pressed)
                {
                    if (selectedRow == RowKirby || selectedRow == RowMadeline)
                    {
                        pendingPlayer = pendingPlayer == PlayerSelectionManager.PlayerType.Kirby
                            ? PlayerSelectionManager.PlayerType.Madeline
                            : PlayerSelectionManager.PlayerType.Kirby;
                        Audio.Play(pendingPlayer == PlayerSelectionManager.PlayerType.Kirby
                            ? Sfx_Kirby : Sfx_Madeline);
                        wiggler.Start();
                    }
                    else if (selectedRow == RowAuto)
                    {
                        pendingAutoSelect = !pendingAutoSelect;
                        Audio.Play(pendingAutoSelect ? Sfx_AutoOn : Sfx_AutoOff);
                        wiggler.Start();
                    }
                }

                // Dash / dedicated start button = confirm and start level
                if (Input.MenuJournal.Pressed || (Input.Dash.Pressed && Focused))
                {
                    Commit();
                    yield break;
                }

                // B / Back = cancel
                if (Input.MenuCancel.Pressed)
                {
                    Audio.Play(Sfx_Back);
                    Focused = false;
                    OnCancel?.Invoke();
                    yield break;
                }

                yield return null;
            }
        }

        /// <summary>Apply the pending selection and fire <see cref="OnConfirm"/>.</summary>
        private void Commit()
        {
            // Commit player choice
            PlayerSelectionManager.SetDefaultPlayer(pendingPlayer);

            // Commit auto-select preference
            PlayerSelectionManager.AutoSelectEnabled = pendingAutoSelect;

            Audio.Play(Sfx_Confirm);
            Focused = false;
            OnConfirm?.Invoke();
        }

        // ── Render ──────────────────────────────────────────────────────────

        public override void Update()
        {
            base.Update();
        }

        public override void Render()
        {
            if (!Visible) return;

            float eased = Ease.CubeOut(contentEase);

            // Dark overlay
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * fade * 0.88f);

            float centerX = 960f;
            float startY  = 460f;
            float rowH    = 80f;

            // ── Title ──
            string title = Dialog.Clean("DZ_PLAYERSELECT_TITLE");
            ActiveFont.DrawOutline(
                title,
                new Vector2(centerX, startY - 120f + (1f - eased) * 60f),
                new Vector2(0.5f, 0f),
                Vector2.One * 1.2f,
                Color.White * fade * eased,
                2f,
                Color.Black * fade * eased);

            // ── Recommendation label ──
            if (recommendedPlayer.HasValue)
            {
                string recName = PlayerSelectionManager.GetPlayerName(recommendedPlayer.Value);
                string recText = string.Format(Dialog.Clean("DZ_PLAYERSELECT_RECOMMEND"), recName);
                ActiveFont.DrawOutline(
                    recText,
                    new Vector2(centerX, startY - 50f + (1f - eased) * 40f),
                    new Vector2(0.5f, 0f),
                    Vector2.One * 0.65f,
                    ColorRecommend * fade * eased,
                    2f,
                    Color.Black * fade * eased);
            }

            // ── Player rows ──
            RenderPlayerRow(RowKirby,    centerX, startY + rowH * 0f, eased,
                Dialog.Clean("DZ_PLAYERSELECT_KIRBY"),
                ColorKirby,
                pendingPlayer == PlayerSelectionManager.PlayerType.Kirby,
                recommendedPlayer == PlayerSelectionManager.PlayerType.Kirby);

            RenderPlayerRow(RowMadeline, centerX, startY + rowH * 1f, eased,
                Dialog.Clean("DZ_PLAYERSELECT_MADELINE"),
                ColorMadeline,
                pendingPlayer == PlayerSelectionManager.PlayerType.Madeline,
                recommendedPlayer == PlayerSelectionManager.PlayerType.Madeline);

            // ── Auto-select row ──
            RenderToggleRow(RowAuto, centerX, startY + rowH * 2.5f, eased,
                Dialog.Clean("DZ_PLAYERSELECT_AUTO"),
                ColorAuto,
                pendingAutoSelect);

            // ── Confirm prompt at bottom ──
            string confirmHint = Dialog.Clean("DZ_PLAYERSELECT_CONFIRM");
            ActiveFont.DrawOutline(
                confirmHint,
                new Vector2(centerX, startY + rowH * 4.2f + (1f - eased) * 30f),
                new Vector2(0.5f, 0f),
                Vector2.One * 0.55f,
                Color.White * 0.7f * fade * eased,
                2f,
                Color.Black * fade * eased);
        }

        private void RenderPlayerRow(int row, float cx, float cy, float eased,
            string label, Color accentColor, bool isSelected, bool isRecommended)
        {
            bool isCursor = selectedRow == row;
            float wiggle  = isCursor ? wiggler.Value * 10f : 0f;
            float alpha   = fade * eased;

            // Background pill
            Color pillColor = isSelected ? accentColor * 0.25f : Color.White * 0.06f;
            Draw.Rect(cx - 280f + wiggle, cy - 2f, 560f, 54f, pillColor);

            // Checkmark / radio indicator
            Color checkColor = isSelected ? accentColor : Color.White * 0.3f;
            Draw.Rect(cx - 260f + wiggle, cy + 14f, 24f, 24f, checkColor * alpha);
            if (isSelected)
                Draw.Rect(cx - 256f + wiggle, cy + 18f, 16f, 16f, Color.Black * alpha * 0.8f);

            // Label
            Color labelColor = isCursor
                ? (Settings.Instance.DisableFlashes || Scene.BetweenInterval(0.1f)
                    ? TextMenu.HighlightColorA : TextMenu.HighlightColorB)
                : (isSelected ? accentColor : Color.White);
            ActiveFont.DrawOutline(
                label,
                new Vector2(cx - 220f + wiggle, cy + 4f),
                Vector2.Zero,
                Vector2.One * 0.8f,
                labelColor * alpha,
                2f,
                Color.Black * alpha);

            // "Recommended" star badge
            if (isRecommended)
            {
                ActiveFont.DrawOutline(
                    "★ " + Dialog.Clean("DZ_PLAYERSELECT_STAR"),
                    new Vector2(cx + 120f + wiggle, cy + 12f),
                    Vector2.Zero,
                    Vector2.One * 0.55f,
                    ColorRecommend * alpha,
                    2f,
                    Color.Black * alpha);
            }
        }

        private void RenderToggleRow(int row, float cx, float cy, float eased,
            string label, Color accentColor, bool isOn)
        {
            bool isCursor = selectedRow == row;
            float wiggle  = isCursor ? wiggler.Value * 10f : 0f;
            float alpha   = fade * eased;

            Color pillColor = isOn ? accentColor * 0.18f : Color.White * 0.06f;
            Draw.Rect(cx - 280f + wiggle, cy - 2f, 560f, 54f, pillColor);

            // Toggle indicator
            string onOff = isOn ? Dialog.Clean("DZ_PLAYERSELECT_ON") : Dialog.Clean("DZ_PLAYERSELECT_OFF");
            Color togColor = isOn ? accentColor : Color.White * 0.5f;

            Color labelColor = isCursor
                ? (Settings.Instance.DisableFlashes || Scene.BetweenInterval(0.1f)
                    ? TextMenu.HighlightColorA : TextMenu.HighlightColorB)
                : Color.White;
            ActiveFont.DrawOutline(
                label,
                new Vector2(cx - 220f + wiggle, cy + 4f),
                Vector2.Zero,
                Vector2.One * 0.8f,
                labelColor * alpha,
                2f,
                Color.Black * alpha);

            ActiveFont.DrawOutline(
                "[" + onOff + "]",
                new Vector2(cx + 120f + wiggle, cy + 4f),
                Vector2.Zero,
                Vector2.One * 0.8f,
                togColor * alpha,
                2f,
                Color.Black * alpha);
        }
    }
}
