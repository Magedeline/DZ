using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Franchise identifier for cast members.
    /// </summary>
    public enum CastFranchise
    {
        DesoloZantas
    }

    /// <summary>
    /// Represents a single entry in the cast roll.
    /// </summary>
    public struct CastMember
    {
        public string Name;
        public string Role;
        public CastFranchise Franchise;

        public CastMember(string name, string role, CastFranchise franchise)
        {
            Name = name;
            Role = role;
            Franchise = franchise;
        }
    }

    /// <summary>
    /// Chapter 21 - Cast Roll
    /// Scrolling credits showing every Kirby and Undertale/Deltarune character,
    /// modelled after Undertale's obj_castroll with two-column layout and
    /// franchise-colour accents. Chains to CS21_EpilogueCredits when finished.
    /// </summary>
    [Tracked]
    public class CS21_Cast : CutsceneEntity
    {
        // ── layout constants ────────────────────────────────────────────────
        private const float ScreenW        = 1920f;
        private const float ScreenH        = 1080f;
        private const float Col1X          = 480f;   // left column x-centre
        private const float Col2X          = 1440f;  // right column x-centre
        private const float RowHeight      = 260f;   // vertical space per entry pair
        private const float ScrollSpeed    = 1.2f;   // pixels per frame at normal speed
        private const float FastScrollMult = 6f;     // multiplier when accelerating out
        private const float NameScale      = 1.0f;
        private const float RoleScale      = 0.65f;
        private const float SectionTitleScale = 1.4f;

        // ── franchise colours (mirroring Undertale's white/yellow scheme) ──
        private static readonly Color ColourDesoloZantas = new Color(255, 180, 220); // soft pink
        private static readonly Color ColourDeltarune    = new Color(130, 200, 255); // sky blue
        private static readonly Color ColourSection      = Color.White;
        private static readonly Color ColourRole         = new Color(180, 180, 180);

        // ── group colours for each cast section ──
        private static readonly Color ColourGroup1 = new Color(255, 100, 100); // red-ish (Main Characters)
        private static readonly Color ColourGroup2 = new Color(100, 255, 100); // green-ish (Undertale Part 1)
        private static readonly Color ColourGroup3 = new Color(100, 100, 255); // blue-ish (Undertale Part 2)
        private static readonly Color ColourGroup4 = new Color(255, 200, 50);  // yellow/gold (Kirby Dream Friends)
        private static readonly Color ColourGroup5 = new Color(255, 150, 50);  // orange (Kirby Small Enemies)
        private static readonly Color ColourGroup6 = new Color(200, 100, 255); // purple (Kirby Mid-Bosses)
        private static readonly Color ColourGroup7 = new Color(100, 255, 255); // cyan (Kirby Main Bosses)
        private static readonly Color ColourGroup8 = new Color(255, 100, 200); // pink/magenta (Farewell)

        // ── state ───────────────────────────────────────────────────────────
        private readonly global::Celeste.Player player;
        private List<ScrollEntry> entries = new List<ScrollEntry>();
        private float scrollY   = 0f;          // current top of the "tape" in content-space
        private float totalContentH = 0f;
        private bool  scrollActive   = false;
        private bool  accelerating   = false;
        private float accelAmount    = 0f;
        private float globalAlpha    = 0f;      // fade in/out
        private bool  fadingOut      = false;

        // ── inner type for a single rendered row ────────────────────────────

        private struct ScrollEntry
        {
            public string   Line1;        // character name  OR  section header
            public string   Line2;        // role subtitle
            public Color    Line1Colour;
            public bool     IsSection;    // true = section banner row
            public float    ContentY;     // y in content-space (computed at build time)
        }

        // ────────────────────────────────────────────────────────────────────

        public CS21_Cast(global::Celeste.Player player) : base(false, true)
        {
            this.player = player;
            Tag = Tags.HUD;
            RemoveOnSkipped = false;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            buildEntries();
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
            Audio.SetMusic("event:/pusheen/music/menu/true_cast");

            Add(new Coroutine(RunCastMember(level)));
        }

        // ── entry list builder ───────────────────────────────────────────────

        private void buildEntries()
        {
            entries.Clear();

            // ── Main Characters ──────────────────────────────────────────
            addSection("~ Main Characters ~", ColourGroup1);
            addPairs(TrueCast.GetFirstCharacters());

            // ── Undertale Character Part 1 ──────────────────────────────────────────
            addSection("~ Undertale Character Part 1 ~", ColourGroup2);
            addPairs(TrueCast.GetSecondCharacters());

            // ── Undertale Character Part 2 ─────────────────────────────────────────────
            addSection("~ Undertale Character Part 2 ~", ColourGroup3);
            addPairs(TrueCast.GetThirdCharacters());

            // ── Kirby Main Bosses ────────────────────────────────────────────
            addSection("~ Kirby Dream Friends ~", ColourGroup4);
            addPairs(TrueCast.GetFourthCharacters());

            // ── Kirby Small Enemies ───────────────────────────────────────────
            addSection("~ Kirby Small Enemies ~", ColourGroup5);
            addPairs(TrueCast.GetFifthCharacters());

            // ── Kirby Mid-Bosses ─────────────────────────────────────────────
            addSection("~ Kirby Mid-Bosses ~", ColourGroup6);
            addPairs(TrueCast.GetSixthCharacters());

            // ── Kirby Main Bosses ─────────────────────────────────────────────
            addSection("~ Kirby Main Bosses ~", ColourGroup7);
            addPairs(TrueCast.GetSeventhCharacters());

            // ── Farewell ─────────────────────────────────────────────────────
            addSection("~ Farewell ~", ColourGroup8);
            addPairs(TrueCast.GetEighthCharacters());

            // ── closing padding ──────────────────────────────────────────────
            addSection("", Color.Transparent);   // spacer

            // ── assign content-space Y positions ────────────────────────────
            float y = ScreenH + 80f;
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                e.ContentY = y;
                entries[i] = e;
                y += e.IsSection ? RowHeight * 0.9f : RowHeight * 0.7f;
            }
            totalContentH = y;
        }

        private void addSection(string title, Color colour)
        {
            entries.Add(new ScrollEntry
            {
                Line1       = title,
                Line2       = "",
                Line1Colour = colour,
                IsSection   = true,
            });
        }

        private void addPairs(List<CastMember> cast)
        {
            // Two-column layout: left col gets even indices, right col odd
            // We store each as its own row tagged with column in Line1Colour
            for (int i = 0; i < cast.Count; i++)
            {
                var m = cast[i];
                Color nameCol = m.Franchise switch
                {
                    CastFranchise.DesoloZantas => ColourDesoloZantas,
                    _ => Color.White
                };
                entries.Add(new ScrollEntry
                {
                    Line1       = m.Name,
                    Line2       = m.Role,
                    Line1Colour = nameCol,
                    IsSection   = false,
                });
            }
        }

        // ── main coroutine ───────────────────────────────────────────────────

        private IEnumerator RunCastMember(Level level)
        {
            // Fade in
            for (float t = 0f; t < 1f; t += Engine.DeltaTime)
            {
                globalAlpha = Ease.CubeOut(t);
                yield return null;
            }
            globalAlpha = 1f;

            yield return 1f;

            scrollActive = true;

            // Scroll until all content has passed the top of the screen
            float endY = totalContentH + ScreenH;
            while (scrollY < endY && !accelerating)
            {
                scrollY += ScrollSpeed;
                yield return null;
            }

            // Accelerate out (mirrors Undertale's exper variable)
            accelerating = true;
            while (scrollY < endY + ScreenH * 0.5f)
            {
                accelAmount += 0.15f;
                scrollY     += ScrollSpeed + accelAmount;
                yield return null;
            }

            // Fade out
            fadingOut = true;
            for (float t = 0f; t < 1.5f; t += Engine.DeltaTime)
            {
                globalAlpha = 1f - Ease.CubeIn(t / 1.5f);
                yield return null;
            }
            globalAlpha = 0f;

            yield return 0.5f;

            // Transition to fake Asriel God of Hyper Death
            level.Add(new FakeAsrielGodOFHD(player));
            RemoveSelf();
        }

        // ── Update ───────────────────────────────────────────────────────────

        public override void Update()
        {
            base.Update();

            // Allow player to hold confirm to fast-scroll
            if (scrollActive && !fadingOut && Input.MenuConfirm.Check)
            {
                scrollY += ScrollSpeed * (FastScrollMult - 1f);
            }
        }

        // ── Render ───────────────────────────────────────────────────────────

        public override void Render()
        {
            if (globalAlpha <= 0f) return;

            // Background
            Draw.Rect(0f, 0f, ScreenW, ScreenH, Color.Black * globalAlpha);

            // Header bar
            Draw.Rect(0f, 0f, ScreenW, 6f, Color.White * globalAlpha * 0.3f);
            Draw.Rect(0f, ScreenH - 6f, ScreenW, 6f, Color.White * globalAlpha * 0.3f);

            foreach (var entry in entries)
            {
                // Convert content-space Y to screen Y
                float screenY = entry.ContentY - scrollY;

                // Skip fully off-screen entries (generous margin)
                if (screenY < -RowHeight * 2f || screenY > ScreenH + RowHeight * 2f)
                    continue;

                Color col1 = entry.Line1Colour * globalAlpha;

                if (entry.IsSection)
                {
                    if (string.IsNullOrEmpty(entry.Line1)) continue;

                    // Centred section banner
                    ActiveFont.DrawOutline(
                        entry.Line1,
                        new Vector2(ScreenW * 0.5f, screenY),
                        new Vector2(0.5f, 0.5f),
                        Vector2.One * SectionTitleScale,
                        col1,
                        2f,
                        Color.Black * globalAlpha
                    );

                    // Divider lines
                    float lineW = 600f;
                    float ly    = screenY;
                    Draw.Line(
                        new Vector2(ScreenW * 0.5f - lineW * 0.5f - 20f, ly),
                        new Vector2(ScreenW * 0.5f - lineW * 0.5f - 320f, ly),
                        col1 * 0.5f
                    );
                    Draw.Line(
                        new Vector2(ScreenW * 0.5f + lineW * 0.5f + 20f, ly),
                        new Vector2(ScreenW * 0.5f + lineW * 0.5f + 320f, ly),
                        col1 * 0.5f
                    );
                }
                else
                {
                    // Two-column layout: alternate left/right based on index in entries list
                    int idx = entries.IndexOf(entry);
                    float xCentre = (idx % 2 == 0) ? Col1X : Col2X;

                    // Name
                    ActiveFont.DrawOutline(
                        entry.Line1,
                        new Vector2(xCentre, screenY - 18f),
                        new Vector2(0.5f, 0.5f),
                        Vector2.One * NameScale,
                        col1,
                        2f,
                        Color.Black * globalAlpha
                    );

                    // Role / subtitle
                    if (!string.IsNullOrEmpty(entry.Line2))
                    {
                        ActiveFont.DrawOutline(
                            entry.Line2,
                            new Vector2(xCentre, screenY + 26f),
                            new Vector2(0.5f, 0.5f),
                            Vector2.One * RoleScale,
                            ColourRole * globalAlpha,
                            2f,
                            Color.Black * globalAlpha
                        );
                    }
                }
            }
        }

        // ── OnEnd ────────────────────────────────────────────────────────────

        public override void OnEnd(Level level)
        {
            level.PauseLock        = false;
            level.SaveQuitDisabled = false;
            level.TimerHidden      = false;
            level.TimerStopped     = false;

            if (player?.StateMachine != null)
                player.StateMachine.State = Player.StNormal;

            Audio.SetMusic(null, true, true);
        }
    }
}
