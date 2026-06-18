using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 21 - Special Thanks / Dodge Credits
    /// A scrolling "Special Thanks" credits screen that runs after AsrielJoking.
    /// Uses the dodge_credit music track and chains into CS21_FinalCutscenes.
    /// </summary>
    [Tracked]
    public class CS21_SpecialThanksDodgeCredits : CutsceneEntity
    {
        // ── layout ──────────────────────────────────────────────────────────
        private const float ScreenW      = 1920f;
        private const float ScreenH      = 1080f;
        private const float ScrollSpeed  = 4.1f;
        private const float FastScrollMult = 6f;

        // ── colours ──────────────────────────────────────────────────────────
        private static readonly Color ColourHeader  = new Color(255, 220, 80);   // warm gold
        private static readonly Color ColourName    = Color.White;
        private static readonly Color ColourSection = new Color(130, 200, 255);  // sky blue

        // ── state ────────────────────────────────────────────────────────────
        private readonly Player player;
        private float scrollY       = 0f;
        private float totalContentH = 0f;
        private bool  scrollActive  = false;
        private float globalAlpha   = 0f;
        private bool  fastScroll    = false;

        private struct CreditRow
        {
            public string Text;
            public Color  Colour;
            public float  Scale;
            public float  ContentY;
        }

        private readonly List<CreditRow> rows = new List<CreditRow>();

        // ────────────────────────────────────────────────────────────────────

        public CS21_SpecialThanksDodgeCredits(Player player) : base(false, true)
        {
            this.player = player;
            Tag = Tags.HUD;
            RemoveOnSkipped = false;
            buildRows();
        }

        private void buildRows()
        {
            addHeader("Special Thanks");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");
            addName("test"); addName("test"); addName("test"); addName("test"); addName("test");

            // ── And You ──────────────────────────────────────────────────────
            addSection("And You");
            addName("Thank you for playing.");
            addName("This journey was yours too.");

            // trailing spacer
            addName("");
            addName("");

            // assign Y positions
            float y = ScreenH + 60f;
            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                float rowH = r.Scale > 0.9f ? 90f : 56f;
                r.ContentY = y;
                rows[i]    = r;
                y += rowH;
            }
            totalContentH = y;
        }

        private void addHeader(string text)
        {
            rows.Add(new CreditRow { Text = text, Colour = ColourHeader, Scale = 1.6f });
        }

        private void addSection(string text)
        {
            rows.Add(new CreditRow { Text = text, Colour = ColourSection, Scale = 1.0f });
        }

        private void addName(string text)
        {
            rows.Add(new CreditRow { Text = text, Colour = ColourName, Scale = 0.75f });
        }

        // ── lifecycle ───────────────────────────────────────────────────────

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }

        public override void OnBegin(Level level)
        {
            level.TimerStopped     = true;
            level.TimerHidden      = true;
            level.SaveQuitDisabled = true;
            level.PauseLock        = true;
            level.AllowHudHide     = false;

            if (player?.StateMachine != null)
                player.StateMachine.State = Player.StDummy;

            Audio.SetAmbience(null, true);
            Audio.SetMusic("event:/Mods/pusheen/music/menu/dodge_credit");

            Add(new Coroutine(RunSequence(level)));
        }

        private IEnumerator RunSequence(Level level)
        {
            // Fade in
            for (float t = 0f; t < 1.5f; t += Engine.DeltaTime)
            {
                globalAlpha = Ease.CubeOut(t / 1.5f);
                yield return null;
            }
            globalAlpha = 1f;

            yield return 0.5f;

            scrollActive = true;

            float endY = totalContentH + ScreenH;
            while (scrollY < endY)
            {
                float extra = fastScroll ? ScrollSpeed * (FastScrollMult - 1f) : 0f;
                scrollY += ScrollSpeed + extra;
                yield return null;
            }

            // Fade out
            for (float t = 0f; t < 2f; t += Engine.DeltaTime)
            {
                globalAlpha = 1f - Ease.CubeIn(t / 2f);
                yield return null;
            }
            globalAlpha = 0f;

            yield return 0.5f;

            level.Session.SetFlag("special_thanks_dodge_credits_played");
            EndCutscene(level);
        }

        public override void Update()
        {
            base.Update();
            if (scrollActive)
                fastScroll = Input.MenuConfirm.Check;
        }

        public override void OnEnd(Level level)
        {
            level.PauseLock        = false;
            level.SaveQuitDisabled = false;
            level.TimerHidden      = false;
            level.TimerStopped     = false;
            level.AllowHudHide     = true;

            if (player?.StateMachine != null)
                player.StateMachine.State = Player.StNormal;

            // Chain into the branching final cutscenes
            level.Add(new CS21_FinalCutscenes(player));
        }

        public override void Render()
        {
            if (globalAlpha <= 0f) return;

            Draw.Rect(0f, 0f, ScreenW, ScreenH, Color.Black * globalAlpha);

            // thin accent bars
            Draw.Rect(0f,           0f,          ScreenW, 4f, ColourHeader * globalAlpha * 0.4f);
            Draw.Rect(0f, ScreenH - 4f,          ScreenW, 4f, ColourHeader * globalAlpha * 0.4f);

            foreach (var row in rows)
            {
                float screenY = row.ContentY - scrollY;
                if (screenY < -80f || screenY > ScreenH + 80f) continue;
                if (string.IsNullOrEmpty(row.Text)) continue;

                ActiveFont.DrawOutline(
                    row.Text,
                    new Vector2(ScreenW * 0.5f, screenY),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * row.Scale,
                    row.Colour * globalAlpha,
                    2f,
                    Color.Black * globalAlpha
                );
            }
        }
    }
}
