using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Celeste.Entities
{
    /// <summary>
    /// Large door that requires collecting all Heart Staffs to open.
    /// Opens to reveal the final chapter. Plays an epic cutscene when all staffs are collected.
    /// Inspired by the final door mechanic from Kirby Star Allies.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/HeartStaffDoor")]
    [Tracked]
    public class HeartStaffDoor : Entity
    {
        // Constants
        private const string OPENED_FLAG = "heart_staff_door_opened_";
        private const int TOTAL_STAFFS_REQUIRED = 7;

        // Door properties
        private readonly int requiredStaffs;
        private readonly int doorWidth;
        private readonly float openDistance;
        private readonly bool startHidden;
        private readonly string doorId;

        // Visual components
        private List<MTexture> staffIcons;
        private Particle[] particles;
        private float rainbowTimer;

        // Door state
        private bool opened;
        private float openPercent;
        private Solid topSolid;
        private Solid bottomSolid;
        private float counter;
        private float staffDisplayAlpha = 1f;

        // Audio
        private EventInstance doorAmbience;
        private EventInstance openingMusic;

        private int CollectedStaffs
        {
            get
            {
                if (SaveData.Instance?.CheatMode ?? false)
                    return requiredStaffs;

                Level level = SceneAs<Level>();
                return level?.Session.GetCounter(HeartStaff.TOTAL_STAFFS_KEY) ?? 0;
            }
        }

        private float OpenAmount => openPercent * openDistance;

        public HeartStaffDoor(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            requiredStaffs = data.Int("requires", TOTAL_STAFFS_REQUIRED);
            doorWidth = data.Width;
            startHidden = data.Bool("startHidden", false);
            doorId = data.Attr("doorId", data.ID.ToString());

            // Calculate open distance from node or default
            Vector2? node = data.FirstNodeNullable(offset);
            openDistance = node.HasValue ? Math.Abs(node.Value.Y - Y) : 96f;

            // Initialize components
            Add(new CustomBloom(RenderBloom));

            // Load staff icon textures
            staffIcons = GFX.Game.GetAtlasSubtextures("objects/heart_staff_door/icon");
            if (staffIcons.Count == 0)
            {
                // Fallback to heart gem icons
                staffIcons = GFX.Game.GetAtlasSubtextures("objects/heart_spear_door_mod/icon");
            }

            // Initialize particles
            particles = new Particle[150];

            if (startHidden)
            {
                Visible = false;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            Level level = scene as Level;
            if (level == null) return;

            // Initialize particles
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Position = new Vector2(
                    Calc.Random.NextFloat(doorWidth),
                    Calc.Random.NextFloat(level.Bounds.Height)
                );
                particles[i].Speed = Calc.Random.Range(4, 14);
                particles[i].Color = GetRandomStaffColor() * Calc.Random.Range(0.2f, 0.6f);
            }

            // Create solid door pieces
            Rectangle bounds = level.Bounds;

            // Top solid (from top of level to door position)
            float topHeight = Y - bounds.Top + 48f;
            topSolid = new Solid(
                new Vector2(X, bounds.Top - 32),
                doorWidth,
                topHeight,
                true
            );
            topSolid.SurfaceSoundIndex = 32;
            level.Add(topSolid);

            // Bottom solid (from door position to bottom of level)
            float bottomHeight = bounds.Bottom - Y + 48f;
            bottomSolid = new Solid(
                new Vector2(X, Y),
                doorWidth,
                bottomHeight,
                true
            );
            bottomSolid.SurfaceSoundIndex = 32;
            level.Add(bottomSolid);

            // Check if already opened
            string openedFlag = OPENED_FLAG + doorId;
            if (level.Session.GetFlag(openedFlag))
            {
                opened = true;
                openPercent = 1f;
                counter = requiredStaffs;
                topSolid.Y -= openDistance;
                bottomSolid.Y += openDistance;
            }
            else
            {
                Add(new Coroutine(DoorRoutine()));
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            doorAmbience?.stop(STOP_MODE.ALLOWFADEOUT);
            openingMusic?.stop(STOP_MODE.ALLOWFADEOUT);
        }

        private IEnumerator DoorRoutine()
        {
            Level level = SceneAs<Level>();

            // Handle initially hidden door
            if (startHidden)
            {
                CelestePlayer player;
                do
                {
                    yield return null;
                    player = Scene.Tracker.GetEntity<CelestePlayer>();
                } while (player == null || Math.Abs(player.X - Center.X) >= 120f);

                // Reveal door with dramatic effect
                Audio.Play("event:/game/general/crystalheart_pulse", Position);
                Visible = true;
                staffDisplayAlpha = 0f;

                // Slide door into view
                float topTo = topSolid.Y;
                float bottomTo = bottomSolid.Y;
                topSolid.Y -= 300f;
                bottomSolid.Y -= 300f;

                for (float p = 0f; p < 1f; p += Engine.DeltaTime * 1.0f)
                {
                    float ease = Ease.CubeIn(p);
                    topSolid.MoveToY(topSolid.Y + (topTo - (topSolid.Y - 300f * (1f - ease))) * ease);
                    bottomSolid.MoveToY(bottomSolid.Y + (bottomTo - (bottomSolid.Y - 300f * (1f - ease))) * ease);
                    staffDisplayAlpha = ease;
                    yield return null;
                }

                topSolid.MoveToY(topTo);
                bottomSolid.MoveToY(bottomTo);
                staffDisplayAlpha = 1f;

                level.Shake(0.5f);
                Audio.Play("event:/game/general/seed_complete", Position);
            }

            // Start ambient sound
            doorAmbience = Audio.Play("event:/game/general/crystalheart_pulse", Position);

            // Wait for player to approach with enough staffs
            while (true)
            {
                CelestePlayer player = Scene.Tracker.GetEntity<CelestePlayer>();
                bool canOpen = CollectedStaffs >= requiredStaffs;

                if (player != null && canOpen && Math.Abs(player.X - Center.X) < 80f && Math.Abs(player.Y - Y) < 100f)
                {
                    break;
                }

                // Animate counter toward current collection
                counter = Calc.Approach(counter, CollectedStaffs, Engine.DeltaTime * 2f);

                yield return null;
            }

            // Begin opening sequence!
            yield return OpenDoorSequence();
        }

        private IEnumerator OpenDoorSequence()
        {
            Level level = SceneAs<Level>();
            CelestePlayer player = Scene.Tracker.GetEntity<CelestePlayer>();

            // Stop ambient and start epic music
            doorAmbience?.stop(STOP_MODE.ALLOWFADEOUT);
            openingMusic = Audio.Play("event:/game/general/seed_complete", Position);

            // Lock player in place for cutscene
            if (player != null)
            {
                player.StateMachine.State = CelestePlayer.StDummy;
            }

            // Initial camera focus on door
            level.Shake(0.3f);
            CelesteGame.Freeze(0.2f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);

            // Dramatic pause
            yield return 0.5f;

            // Display each staff being absorbed into the door
            for (int i = 0; i < 7; i++)
            {
                HeartStaff.StaffColor color = (HeartStaff.StaffColor)i;
                Vector2 staffPos = GetStaffDisplayPosition(i);

                // Particle burst for each staff
                XnaColor staffColor = GetStaffColorValue(color);
                for (int p = 0; p < 15; p++)
                {
                    float angle = Calc.Random.NextFloat() * (float)Math.PI * 2f;
                    level.ParticlesFG.Emit(
                        HeartGem.PBlueShine, // Use existing particle
                        staffPos + Calc.AngleToVector(angle, 8f),
                        staffColor,
                        angle
                    );
                }

                Audio.Play("event:/game/general/crystalheart_pulse", staffPos);
                yield return 0.3f;
            }

            // All staffs absorbed - grand opening!
            yield return 0.5f;

            Audio.Play("event:/game/general/strawberry_get", Center);
            level.Shake(0.6f);
            CelesteGame.Freeze(0.3f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);

            // Massive particle explosion
            for (int i = 0; i < 50; i++)
            {
                float angle = Calc.Random.NextFloat() * (float)Math.PI * 2f;
                XnaColor color = GetRandomStaffColor();
                level.ParticlesFG.Emit(
                    HeartGem.PBlueShine,
                    Center + Calc.AngleToVector(angle, Calc.Random.Range(0f, 32f)),
                    color,
                    angle
                );
            }

            yield return 0.3f;

            // Smoothly open the door
            float topStart = topSolid.Y;
            float bottomStart = bottomSolid.Y;

            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 0.5f)
            {
                openPercent = Ease.CubeInOut(p);

                topSolid.MoveToY(topStart - openDistance * openPercent);
                bottomSolid.MoveToY(bottomStart + openDistance * openPercent);

                // Continuous particle stream
                if (Scene.OnInterval(0.05f))
                {
                    XnaColor color = GetRandomStaffColor();
                    float angle = Calc.Random.NextFloat() * (float)Math.PI * 2f;
                    level.ParticlesBG.Emit(HeartGem.PBlueShine, Center, color, angle);
                }

                yield return null;
            }

            openPercent = 1f;
            opened = true;

            // Set opened flag
            string openedFlag = OPENED_FLAG + doorId;
            level.Session.SetFlag(openedFlag, true);

            // Release player
            if (player != null)
            {
                player.StateMachine.State = CelestePlayer.StNormal;
            }

            // Play completion jingle
            Audio.Play("event:/game/general/cassette_get", Center);

            // Final particle celebration
            for (int i = 0; i < 30; i++)
            {
                float angle = ((float)i / 30f) * (float)Math.PI * 2f;
                XnaColor color = GetRandomStaffColor();
                level.ParticlesFG.Emit(HeartGem.PGoldShine, Center, color, angle);
            }
        }

        private Vector2 GetStaffDisplayPosition(int index)
        {
            // Arrange staffs in a circle around door center
            float angle = ((float)index / 7f) * (float)Math.PI * 2f - (float)Math.PI / 2f;
            return Center + Calc.AngleToVector(angle, 40f);
        }

        private XnaColor GetStaffColorValue(HeartStaff.StaffColor color)
        {
            return color switch
            {
                HeartStaff.StaffColor.Red => Calc.HexToColor("ff4444"),
                HeartStaff.StaffColor.Blue => Calc.HexToColor("4488ff"),
                HeartStaff.StaffColor.Yellow => Calc.HexToColor("ffdd44"),
                HeartStaff.StaffColor.Green => Calc.HexToColor("44ff88"),
                HeartStaff.StaffColor.Purple => Calc.HexToColor("aa44ff"),
                HeartStaff.StaffColor.Orange => Calc.HexToColor("ff8844"),
                HeartStaff.StaffColor.Pink => Calc.HexToColor("ff88cc"),
                _ => XnaColor.White
            };
        }

        private XnaColor GetRandomStaffColor()
        {
            int index = Calc.Random.Next(7);
            return GetStaffColorValue((HeartStaff.StaffColor)index);
        }

        public override void Update()
        {
            base.Update();

            rainbowTimer += Engine.DeltaTime;

            // Update particles
            Level level = SceneAs<Level>();
            if (level != null && Visible)
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].Position.Y -= particles[i].Speed * Engine.DeltaTime;

                    if (particles[i].Position.Y < -8f)
                    {
                        particles[i].Position.Y = level.Bounds.Height + 8f;
                        particles[i].Position.X = Calc.Random.NextFloat(doorWidth);
                        particles[i].Color = GetRandomStaffColor() * Calc.Random.Range(0.2f, 0.6f);
                    }
                }
            }

            // Animate counter when not opened
            if (!opened)
            {
                counter = Calc.Approach(counter, CollectedStaffs, Engine.DeltaTime * 2f);
            }
        }

        public override void Render()
        {
            base.Render();

            if (!Visible) return;

            Level level = SceneAs<Level>();
            if (level == null) return;

            // Draw door frame
            Draw.Rect(X - 4, topSolid.Y, doorWidth + 8, topSolid.Height + 8, XnaColor.DarkBlue * 0.8f);
            Draw.Rect(X - 4, bottomSolid.Y - 8, doorWidth + 8, bottomSolid.Height + 8, XnaColor.DarkBlue * 0.8f);

            // Draw door surface
            XnaColor doorColor = opened ? XnaColor.Black * 0.3f : XnaColor.MidnightBlue;
            Draw.Rect(X, topSolid.Y + topSolid.Height - 4, doorWidth, 8, doorColor);
            Draw.Rect(X, bottomSolid.Y - 4, doorWidth, 8, doorColor);

            // Draw particles
            foreach (var particle in particles)
            {
                Draw.Rect(X + particle.Position.X, Y - level.Bounds.Height / 2 + particle.Position.Y, 2f, 2f, particle.Color);
            }

            // Draw staff icons if not fully opened
            if (!opened || openPercent < 1f)
            {
                DrawStaffIndicators();
            }

            // Draw counter
            if (!opened)
            {
                DrawCounter();
            }
        }

        private void DrawStaffIndicators()
        {
            int collected = CollectedStaffs;

            for (int i = 0; i < 7; i++)
            {
                Vector2 pos = GetStaffDisplayPosition(i);
                HeartStaff.StaffColor staffColor = (HeartStaff.StaffColor)i;
                XnaColor color = GetStaffColorValue(staffColor);

                bool hasStaff = i < collected;

                // Draw staff icon background
                Draw.Circle(pos, 12f, hasStaff ? color * 0.5f : XnaColor.Gray * 0.3f, 8);

                // Draw staff icon
                if (staffIcons.Count > 0)
                {
                    MTexture icon = staffIcons[Math.Min(i, staffIcons.Count - 1)];
                    float scale = hasStaff ? 0.8f : 0.5f;
                    XnaColor iconColor = hasStaff ? color * staffDisplayAlpha : XnaColor.Gray * 0.5f * staffDisplayAlpha;

                    // Pulse animation for collected staffs
                    if (hasStaff)
                    {
                        float pulse = (float)Math.Sin(rainbowTimer * 3f + i * 0.5f) * 0.1f;
                        scale += pulse;
                    }

                    icon.DrawCentered(pos, iconColor, scale);
                }
                else
                {
                    // Fallback: draw colored circle
                    Draw.Circle(pos, 8f, hasStaff ? color : XnaColor.Gray * 0.5f, 6);
                }
            }
        }

        private void DrawCounter()
        {
            Vector2 counterPos = new Vector2(Center.X, Y - 50f);
            string text = $"{(int)counter}/{requiredStaffs}";

            // Rainbow text color when all collected
            XnaColor textColor = CollectedStaffs >= requiredStaffs
                ? GetRainbowColor(rainbowTimer)
                : XnaColor.White;

            ActiveFont.DrawOutline(
                text,
                counterPos,
                new Vector2(0.5f, 0.5f),
                Vector2.One * 1.5f,
                textColor * staffDisplayAlpha,
                2f,
                XnaColor.Black * 0.7f * staffDisplayAlpha
            );

            // Draw subtitle
            string subtitle = CollectedStaffs >= requiredStaffs
                ? "The way is open!"
                : "Heart Staffs";

            ActiveFont.DrawOutline(
                subtitle,
                counterPos + new Vector2(0f, 30f),
                new Vector2(0.5f, 0.5f),
                Vector2.One * 0.7f,
                XnaColor.White * 0.8f * staffDisplayAlpha,
                1f,
                XnaColor.Black * 0.5f * staffDisplayAlpha
            );
        }

        private XnaColor GetRainbowColor(float time)
        {
            float hue = (time * 0.5f) % 1f;
            return Calc.HsvToColor(hue, 0.8f, 1f);
        }

        private void RenderBloom()
        {
            if (!Visible || opened) return;

            // Draw bloom around staff icons
            int collected = CollectedStaffs;
            for (int i = 0; i < Math.Min(collected, 7); i++)
            {
                Vector2 pos = GetStaffDisplayPosition(i);
                Draw.Circle(pos, 20f, XnaColor.White * 0.3f, 8);
            }

            // Bloom at door center when ready to open
            if (collected >= requiredStaffs)
            {
                float pulse = (float)Math.Sin(rainbowTimer * 4f) * 0.3f + 0.7f;
                Draw.Circle(Center, 60f * pulse, XnaColor.White * 0.2f, 16);
            }
        }

        private struct Particle
        {
            public Vector2 Position;
            public float Speed;
            public XnaColor Color;
        }
    }
}

