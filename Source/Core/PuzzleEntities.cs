using Celeste.Entities;
using Celeste.Utils;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DZ
{
    // =============================================
    // WeightSwitch - Pressure plate requiring weight
    // =============================================
    [CustomEntity("DZ/WeightSwitch")]
    [Tracked]
    public class WeightSwitch : Entity
    {
        private string flagName;
        private bool activated = false;
        private int requiredWeight;
        private int currentWeight = 0;
        private Level level;

        public WeightSwitch(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            flagName = data.Attr("flag", "weight_switch");
            requiredWeight = data.Int("requiredWeight", 1);
            Collider = new Hitbox(data.Width, 8f, 0f, -4f);
            Depth = 10;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();
            currentWeight = 0;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player))
                currentWeight++;

            // Check for pushable solids on top using spatial hashing (much faster than iterating all solids)
            foreach (Solid solid in CollideAll<Solid>())
            {
                currentWeight++;
            }

            bool wasActive = activated;
            activated = currentWeight >= requiredWeight;
            if (level != null)
                level.Session.SetFlag(flagName, activated);

            if (activated && !wasActive)
                Audio.Play("event:/game/general/touchswitch_last_cutoff", Position);
        }

        public override void Render()
        {
            Color c = activated ? Color.LimeGreen : Color.Gray;
            float h = activated ? 4f : 8f;
            Draw.Rect(Position.X, Position.Y - h / 2f, Collider.Width, h, c * 0.7f);
        }
    }

    // =============================================
    // ColorLens - Colored filter revealing/hiding blocks
    // =============================================
    [CustomEntity("DZ/ColorLens")]
    [Tracked]
    public class ColorLens : Entity
    {
        public string LensColor { get; private set; }
        private bool collected = false;

        public ColorLens(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            LensColor = data.Attr("lensColor", "red");
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            Depth = -1000;
        }

        public override void Update()
        {
            base.Update();
            if (collected) return;
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player))
            {
                collected = true;
                SceneAs<Level>().Session.SetFlag("lens_" + LensColor, true);
                // Clear other lens flags
                string[] colors = { "red", "blue", "green", "yellow" };
                foreach (string c in colors)
                {
                    if (c != LensColor)
                        SceneAs<Level>().Session.SetFlag("lens_" + c, false);
                }
                Audio.Play("event:/game/general/seed_touch", Position);
            }
        }

        public override void Render()
        {
            Color c = LensColor switch
            {
                "red" => Color.Red,
                "blue" => Color.Blue,
                "green" => Color.Green,
                "yellow" => Color.Yellow,
                _ => Color.White
            };
            ShapeRenderer.DrawCircleOutline(Position, 8f, c * 0.8f, 1f, 12);
            ShapeRenderer.DrawCircleOutline(Position, 6f, c * 0.4f, 1f, 12);
        }
    }

    // =============================================
    // ColorBlock - Visible only with matching lens
    // =============================================
    [CustomEntity("DZ/ColorFilterBlock")]
    [Tracked]
    public class ColorFilterBlock : Solid
    {
        private string blockColor;

        public ColorFilterBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            blockColor = data.Attr("blockColor", "red");
            Depth = 0;
        }

        public override void Update()
        {
            base.Update();
            bool visible = SceneAs<Level>().Session.GetFlag("lens_" + blockColor);
            Collidable = visible;
            Visible = visible;
        }

        public override void Render()
        {
            Color c = blockColor switch
            {
                "red" => Color.Red,
                "blue" => Color.Blue,
                "green" => Color.Green,
                "yellow" => Color.Yellow,
                _ => Color.White
            };
            Draw.Rect(Collider, c * 0.5f);
        }
    }

    // =============================================
    // EchoOrb - Records movement, replays as ghost
    // =============================================
    [CustomEntity("DZ/EchoOrb")]
    [Tracked]
    public class EchoOrb : Entity
    {
        private float recordDuration;
        private List<Vector2> recordedPositions = new List<Vector2>();
        private bool recording = false;
        private bool replaying = false;
        private int replayIndex = 0;
        private Entity ghost;
        private string flagOnComplete;

        public EchoOrb(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            recordDuration = data.Float("recordDuration", 3f);
            flagOnComplete = data.Attr("flag", "echo_complete");
            Collider = new Hitbox(14f, 14f, -7f, -7f);
            Depth = -500;
        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();

            if (!recording && !replaying && player != null && CollideCheck(player))
            {
                recording = true;
                recordedPositions.Clear();
                Audio.Play("event:/game/general/seed_touch", Position);
            }

            if (recording && player != null)
            {
                recordedPositions.Add(player.Position);
                if (recordedPositions.Count >= recordDuration * 60f)
                {
                    recording = false;
                    replaying = true;
                    replayIndex = 0;
                    ghost = new Entity(recordedPositions[0]);
                    ghost.Collider = new Hitbox(8f, 12f, -4f, -12f);
                    ghost.Depth = -200;
                    Scene.Add(ghost);
                }
            }

            if (replaying && ghost != null)
            {
                if (replayIndex < recordedPositions.Count)
                {
                    ghost.Position = recordedPositions[replayIndex];
                    replayIndex++;

                    // Check if ghost is pressing any weight switches
                    foreach (WeightSwitch ws in Scene.Tracker.GetEntities<WeightSwitch>())
                    {
                        // Ghost proximity acts as weight
                    }
                }
                else
                {
                    replaying = false;
                    ghost.RemoveSelf();
                    ghost = null;
                    SceneAs<Level>().Session.SetFlag(flagOnComplete, true);
                }
            }
        }

        public override void Render()
        {
            Color c = recording ? Color.Red : (replaying ? Color.Cyan : Color.White);
            float pulse = 1f + (float)Math.Sin(Scene.TimeActive * 3f) * 0.15f;
            ShapeRenderer.DrawCircleOutline(Position, 8f * pulse, c * 0.6f, 1f, 12);

            // Draw ghost
            if (replaying && ghost != null)
            {
                Draw.Rect(ghost.Position.X - 4f, ghost.Position.Y - 12f, 8f, 12f, Color.Cyan * 0.4f);
            }
        }
    }

    // =============================================
    // MirrorPortal - Step into a mirrored room
    // =============================================
    [CustomEntity("DZ/MirrorPortal")]
    [Tracked]
    public class MirrorPortal : Entity
    {
        private string targetRoom;
        private bool activated = false;

        public MirrorPortal(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            targetRoom = data.Attr("targetRoom", "");
            Collider = new Hitbox(16f, 32f, -8f, -32f);
            Depth = -200;
        }

        public override void Update()
        {
            base.Update();
            if (activated) return;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player) && Input.Talk.Pressed)
            {
                activated = true;
                Level level = SceneAs<Level>();
                level.Session.SetFlag("mirror_world", !level.Session.GetFlag("mirror_world"));
                Audio.Play("event:/game/general/mirror_temple_yourway_b", Position);
                activated = false;
            }
        }

        public override void Render()
        {
            Draw.Rect(Position.X - 8f, Position.Y - 32f, 16f, 32f, Color.MediumPurple * 0.3f);
            Draw.HollowRect(Position.X - 8f, Position.Y - 32f, 16f, 32f, Color.Silver * 0.8f);
        }
    }

    // =============================================
    // RuneStone - Collectible rune (order matters)
    // =============================================
    [CustomEntity("DZ/RuneStone")]
    [Tracked]
    public class RuneStone : Entity
    {
        public int RuneIndex { get; private set; }
        public string GroupId { get; private set; }
        public bool Collected { get; private set; } = false;

        public RuneStone(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            RuneIndex = data.Int("runeIndex", 0);
            GroupId = data.Attr("groupId", "rune_group_A");
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            Depth = -500;
        }

        public override void Update()
        {
            base.Update();
            if (Collected) return;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player))
            {
                Collected = true;
                Audio.Play("event:/game/general/seed_touch", Position);

                // Check if collected in correct order
                int expectedIndex = 0;
                foreach (RuneStone rune in Scene.Tracker.GetEntities<RuneStone>())
                {
                    if (rune.GroupId == GroupId && rune.Collected && rune != this)
                        expectedIndex++;
                }

                if (RuneIndex != expectedIndex)
                {
                    // Wrong order! Reset all in group
                    foreach (RuneStone rune in Scene.Tracker.GetEntities<RuneStone>())
                    {
                        if (rune.GroupId == GroupId) rune.Collected = false;
                    }
                    Audio.Play("event:/game/general/assist_screenbottom", Position);
                }
            }
        }

        public override void Render()
        {
            if (Collected) return;
            Color c = Color.Lerp(Color.Gold, Color.OrangeRed, RuneIndex * 0.2f);
            ShapeRenderer.DrawCircleOutline(Position, 7f, c * 0.7f, 1f, 8);
            // Draw rune number
        }
    }

    // =============================================
    // RuneGate - Opens when all runes collected in order
    // =============================================
    [CustomEntity("DZ/RuneGate")]
    [Tracked]
    public class RuneGate : Solid
    {
        private string groupId;
        private int requiredRunes;
        private bool opened = false;

        public RuneGate(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            groupId = data.Attr("groupId", "rune_group_A");
            requiredRunes = data.Int("requiredRunes", 3);
            Depth = 0;
        }

        public override void Update()
        {
            base.Update();
            if (opened) return;

            int collected = 0;
            foreach (RuneStone rune in Scene.Tracker.GetEntities<RuneStone>())
            {
                if (rune.GroupId == groupId && rune.Collected) collected++;
            }

            if (collected >= requiredRunes)
            {
                opened = true;
                Collidable = false;
                Visible = false;
                Audio.Play("event:/game/general/touchswitch_last_cutoff", Position);
                (Scene as Level)?.Shake(0.2f);
            }
        }

        public override void Render()
        {
            Draw.Rect(Collider, Color.DarkGoldenrod * 0.7f);
            Draw.HollowRect(Collider, Color.Gold * 0.9f);
        }
    }

    // =============================================
    // SoundPuzzleBlock - Musical note blocks
    // =============================================
    [CustomEntity("DZ/SoundPuzzleBlock")]
    [Tracked]
    public class SoundPuzzleBlock : Solid
    {
        private int noteIndex;
        private string groupId;
        private bool hit = false;
        private static Dictionary<string, List<int>> hitSequences = new Dictionary<string, List<int>>();
        private static Dictionary<string, int[]> cachedCorrectSequences = new Dictionary<string, int[]>();

        public SoundPuzzleBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            noteIndex = data.Int("noteIndex", 0);
            groupId = data.Attr("groupId", "music_puzzle_A");
            Depth = 0;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!hitSequences.ContainsKey(groupId))
                hitSequences[groupId] = new List<int>();
            // Invalidate cached sequence when blocks are added
            cachedCorrectSequences.Remove(groupId);
        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && !hit)
            {
                // Check if player dashes into this block
                if (HasPlayerRider() || (player.StateMachine.State == Player.StDash && CollideCheck(player)))
                {
                    HitBlock();
                }
            }
        }

        private void HitBlock()
        {
            hit = true;
            hitSequences[groupId].Add(noteIndex);
            Audio.Play("event:/game/general/seed_touch", Position);

            // Check sequence
            int[] correctSequence = GetCorrectSequence();
            var current = hitSequences[groupId];

            for (int i = 0; i < current.Count; i++)
            {
                if (i >= correctSequence.Length || current[i] != correctSequence[i])
                {
                    // Wrong! Reset
                    hitSequences[groupId].Clear();
                    foreach (SoundPuzzleBlock block in Scene.Tracker.GetEntities<SoundPuzzleBlock>())
                    {
                        if (block.groupId == groupId) block.hit = false;
                    }
                    Audio.Play("event:/game/general/assist_screenbottom", Position);
                    return;
                }
            }

            if (current.Count >= correctSequence.Length)
            {
                SceneAs<Level>().Session.SetFlag("music_puzzle_" + groupId, true);
                Audio.Play("event:/game/general/touchswitch_last_cutoff", Position);
            }

            Add(new Coroutine(ResetAfterDelay()));
        }

        private int[] GetCorrectSequence()
        {
            // Return cached sequence if available
            if (cachedCorrectSequences.TryGetValue(groupId, out int[] cached))
                return cached;

            // Collect all blocks in this group and sort by noteIndex to get correct order
            var blocks = new List<int>();
            foreach (SoundPuzzleBlock block in Scene.Tracker.GetEntities<SoundPuzzleBlock>())
            {
                if (block.groupId == groupId && !blocks.Contains(block.noteIndex))
                    blocks.Add(block.noteIndex);
            }
            blocks.Sort();
            int[] result = blocks.ToArray();
            cachedCorrectSequences[groupId] = result;
            return result;
        }

        private IEnumerator ResetAfterDelay()
        {
            yield return 1f;
            hit = false;
        }

        public override void Render()
        {
            Color c = hit ? Color.White : Color.Lerp(Color.DarkBlue, Color.MediumPurple, noteIndex * 0.15f);
            Draw.Rect(Collider, c * 0.7f);
        }
    }

    // =============================================
    // GravityWell - Circular gravity field
    // =============================================
    [CustomEntity("DZ/GravityWell")]
    [Tracked]
    public class GravityWell : Entity
    {
        private float radius;
        private float strength;
        private bool affectsEnemies;
        private bool affectsProjectiles;

        public GravityWell(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            radius = data.Float("radius", 80f);
            strength = data.Float("strength", 150f);
            affectsEnemies = data.Bool("affectsEnemies", true);
            affectsProjectiles = data.Bool("affectsProjectiles", true);
            Depth = 500;
        }

        public override void Update()
        {
            base.Update();

            // Pull player
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                ApplyGravity(player.Position, ref player.Speed);
            }

            if (affectsEnemies)
            {
                foreach (Enemy enemy in Scene.Tracker.GetEntities<Enemy>())
                {
                    Vector2 pos = enemy.Position;
                    Vector2 vel = Vector2.Zero;
                    ApplyGravity(pos, ref vel);
                    enemy.Position += vel * Engine.DeltaTime;
                }
            }

            if (affectsProjectiles)
            {
                foreach (Projectile proj in Scene.Tracker.GetEntities<Projectile>())
                {
                    Vector2 velocity = proj.Velocity;
                    ApplyGravity(proj.Position, ref velocity);
                    proj.Velocity = velocity;
                }
            }
        }

        private void ApplyGravity(Vector2 targetPos, ref Vector2 velocity)
        {
            float dist = Vector2.Distance(Position, targetPos);
            if (dist < radius && dist > 5f)
            {
                Vector2 pull = (Position - targetPos).SafeNormalize();
                float falloff = 1f - (dist / radius);
                velocity += pull * strength * falloff * Engine.DeltaTime;
            }
        }

        public override void Render()
        {
            for (float r = radius; r > 5; r -= 15f)
            {
                float alpha = (1f - r / radius) * 0.15f;
                Draw.Circle(Position, r, Color.Purple * alpha, 24);
            }
            Draw.Circle(Position, 4f, Color.White * 0.8f, 8);
        }
    }

    // =============================================
    // ShadowLantern - Reveals hidden platforms in darkness
    // =============================================
    [CustomEntity("DZ/ShadowLantern")]
    [Tracked]
    public class ShadowLantern : Entity
    {
        private float lightRadius;
        private bool carried = false;
        private string flagName;

        public ShadowLantern(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            lightRadius = data.Float("lightRadius", 100f);
            flagName = data.Attr("flag", "lantern_carried");
            Collider = new Hitbox(12f, 16f, -6f, -16f);
            Depth = -500;
            Add(new VertexLight(Color.Goldenrod, 1f, (int)lightRadius / 2, (int)lightRadius));
            Add(new BloomPoint(0.8f, lightRadius / 2f));
        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null) return;

            if (!carried && CollideCheck(player) && Input.Grab.Check)
            {
                carried = true;
                SceneAs<Level>().Session.SetFlag(flagName, true);
            }

            if (carried)
            {
                Position = player.Position + new Vector2(0, -20f);
            }
        }

        public override void Render()
        {
            Draw.Rect(X - 4f, Y - 12f, 8f, 12f, Color.DarkGoldenrod * 0.8f);
            float flicker = 1f + (float)Math.Sin(Scene.TimeActive * 10f) * 0.1f;
            Draw.Circle(Position + new Vector2(0, -14f), 4f * flicker, Color.Orange * 0.6f, 8);
        }
    }

    // =============================================
    // RewindCrystal - Rewinds entities to past positions
    // =============================================
    [CustomEntity("DZ/RewindCrystal")]
    [Tracked]
    public class RewindCrystal : Entity
    {
        private bool used = false;
        private float respawnTime;
        private float timer;
        private string flagName;

        public RewindCrystal(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            respawnTime = data.Float("respawnTime", 5f);
            flagName = data.Attr("flag", "rewind_active");
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            Depth = -1000;
        }

        public override void Update()
        {
            base.Update();
            if (used)
            {
                timer -= Engine.DeltaTime;
                if (timer <= 0)
                {
                    used = false;
                    Visible = true;
                    Collidable = true;
                }
                return;
            }

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player))
            {
                Activate();
            }
        }

        private void Activate()
        {
            used = true;
            timer = respawnTime;
            Visible = false;
            Collidable = false;

            SceneAs<Level>().Session.SetFlag(flagName, true);
            Audio.Play("event:/game/general/cassette_bubblereturn", Position);
            (Scene as Level)?.Flash(Color.Cyan * 0.3f);

            // Reset flag after short delay
            Add(new Coroutine(ClearFlagAfterDelay()));
        }

        private IEnumerator ClearFlagAfterDelay()
        {
            yield return 0.5f;
            SceneAs<Level>().Session.SetFlag(flagName, false);
        }

        public override void Render()
        {
            float pulse = 1f + (float)Math.Sin(Scene.TimeActive * 2f) * 0.15f;
            Draw.Circle(Position, 8f * pulse, Color.Cyan * 0.6f, 12);
            Draw.Circle(Position, 5f * pulse, Color.White * 0.4f, 8);
        }
    }

    // =============================================
    // ElementalPillar - Fire/Ice/Electric pillar
    // =============================================
    [CustomEntity("DZ/ElementalPillar")]
    [Tracked]
    public class ElementalPillar : Entity
    {
        private string element; // fire, ice, electric
        private bool activated = false;
        private string flagName;

        public ElementalPillar(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            element = data.Attr("element", "fire");
            flagName = data.Attr("flag", "");
            if (string.IsNullOrEmpty(flagName))
                flagName = element + "_pillar_active";
            Collider = new Hitbox(16f, 32f, -8f, -32f);
            Depth = -200;
        }

        public override void Update()
        {
            base.Update();
            if (activated) return;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player) && Input.Talk.Pressed)
            {
                Activate();
            }
        }

        private void Activate()
        {
            activated = true;
            Level level = SceneAs<Level>();

            // Clear other elemental flags
            level.Session.SetFlag("fire_pillar_active", false);
            level.Session.SetFlag("ice_pillar_active", false);
            level.Session.SetFlag("electric_pillar_active", false);

            level.Session.SetFlag(flagName, true);
            Audio.Play("event:/game/general/touchswitch_last_cutoff", Position);
            (Scene as Level)?.Flash(GetColor() * 0.3f);
        }

        private Color GetColor()
        {
            return element switch
            {
                "fire" => Color.OrangeRed,
                "ice" => Color.LightCyan,
                "electric" => Color.Yellow,
                _ => Color.White
            };
        }

        public override void Render()
        {
            Color c = GetColor();
            float alpha = activated ? 1f : 0.5f;
            Draw.Rect(X - 6f, Y - 32f, 12f, 32f, Color.Gray * 0.6f);
            Draw.Rect(X - 4f, Y - 30f, 8f, 28f, c * alpha * 0.7f);
            if (activated)
            {
                float pulse = 1f + (float)Math.Sin(Scene.TimeActive * 4f) * 0.2f;
                Draw.Circle(Position + new Vector2(0, -16f), 12f * pulse, c * 0.2f, 12);
            }
        }
    }

    // =============================================
    // HologramProjector - Projects solid holograms
    // =============================================
    [CustomEntity("DZ/HologramProjector")]
    [Tracked]
    public class HologramProjector : Entity
    {
        private Solid hologram;
        private string flagName;
        private bool active = true;
        private Vector2 hologramOffset;
        private float hologramWidth;
        private float hologramHeight;

        public HologramProjector(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            flagName = data.Attr("flag", "hologram_active");
            hologramWidth = data.Float("hologramWidth", 48f);
            hologramHeight = data.Float("hologramHeight", 8f);

            if (data.Nodes.Length > 0)
                hologramOffset = data.NodesOffset(offset)[0] - Position;
            else
                hologramOffset = new Vector2(0, -40f);

            Collider = new Hitbox(8f, 8f, -4f, -4f);
            Depth = -200;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            hologram = new Solid(Position + hologramOffset, hologramWidth, hologramHeight, safe: true);
            scene.Add(hologram);
        }

        public override void Update()
        {
            base.Update();
            active = !SceneAs<Level>().Session.GetFlag(flagName + "_off");
            if (hologram != null)
            {
                hologram.Collidable = active;
                hologram.Visible = active;
            }
        }

        public override void Render()
        {
            Draw.Rect(X - 4f, Y - 4f, 8f, 8f, Color.Gray * 0.8f);
            if (active && hologram != null)
            {
                float alpha = 0.3f + (float)Math.Sin(Scene.TimeActive * 3f) * 0.1f;
                Draw.Rect(hologram.Position, hologramWidth, hologramHeight, Color.Cyan * alpha);
                Draw.Line(Position, hologram.Position + new Vector2(hologramWidth / 2f, hologramHeight / 2f), Color.Cyan * 0.2f);
            }
        }
    }

    // =============================================
    // MemoryTile - Simon-says tiles
    // =============================================
    [CustomEntity("DZ/MemoryTile")]
    [Tracked]
    public class MemoryTile : Entity
    {
        public int TileIndex { get; private set; }
        public string GroupId { get; private set; }
        private bool lit = false;
        private float litTimer = 0f;
        private Color baseColor;

        public MemoryTile(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            TileIndex = data.Int("tileIndex", 0);
            GroupId = data.Attr("groupId", "memory_A");
            baseColor = Calc.HexToColor(data.Attr("color", "ffffff"));
            Collider = new Hitbox(16f, 16f);
            Depth = 10;
        }

        public void LightUp(float duration)
        {
            lit = true;
            litTimer = duration;
        }

        public override void Update()
        {
            base.Update();
            if (litTimer > 0)
            {
                litTimer -= Engine.DeltaTime;
                if (litTimer <= 0) lit = false;
            }
        }

        public override void Render()
        {
            Color c = lit ? Color.White : baseColor * 0.3f;
            Draw.Rect(Position, 16f, 16f, c);
            Draw.HollowRect(Position, 16f, 16f, baseColor * 0.8f);
        }
    }

    // =============================================
    // TeleportCrate - Pushable, teleportable crate
    // =============================================
    [CustomEntity("DZ/TeleportCrate")]
    [Tracked]
    public class TeleportCrate : Solid
    {
        private string portalId;
        private Vector2 startPos;

        public TeleportCrate(EntityData data, Vector2 offset)
            : base(data.Position + offset, 16f, 16f, safe: true)
        {
            portalId = data.Attr("portalId", "crate_portal_A");
            startPos = Position;
            Depth = 0;
        }

        public override void Update()
        {
            base.Update();
            // Check if on portal door
            foreach (PortalDoor door in Scene.Tracker.GetEntities<PortalDoor>())
            {
                if (CollideCheck(door))
                {
                    // Teleport to paired door
                    foreach (PortalDoor other in Scene.Tracker.GetEntities<PortalDoor>())
                    {
                        if (other != door)
                        {
                            MoveTo(other.Position);
                            Audio.Play("event:/game/general/cassette_bubblereturn", Position);
                            break;
                        }
                    }
                }
            }
        }

        public override void Render()
        {
            Draw.Rect(Position, 16f, 16f, Color.SaddleBrown * 0.7f);
            Draw.HollowRect(Position, 16f, 16f, Color.Tan);
        }
    }

    // =============================================
    // TimeCapsule - Lore collectible
    // =============================================
    [CustomEntity("DZ/TimeCapsule")]
    [Tracked]
    public class TimeCapsule : Entity
    {
        private string capsuleId;
        private string dialogId;
        private bool collected = false;

        public TimeCapsule(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            capsuleId = data.Attr("capsuleId", "time_capsule_1");
            dialogId = data.Attr("dialogId", "TIMECAPSULE_1");
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            Depth = -500;
            Add(new BloomPoint(0.4f, 8f));
            Add(new VertexLight(Color.Gold, 0.8f, 16, 32));
        }

        public override void Update()
        {
            base.Update();
            if (collected) return;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player))
            {
                collected = true;
                SceneAs<Level>().Session.SetFlag("capsule_" + capsuleId, true);
                Audio.Play("event:/game/general/seed_touch", Position);
                Add(new Coroutine(CollectRoutine(player)));
            }
        }

        private IEnumerator CollectRoutine(Player player)
        {
            player.StateMachine.State = Player.StDummy;
            yield return 0.5f;

            // Show dialog
            Scene.Add(new MiniTextbox(dialogId));
            yield return 2f;

            player.StateMachine.State = Player.StNormal;
            Visible = false;
            Collidable = false;
        }

        public override void Render()
        {
            float bob = (float)Math.Sin(Scene.TimeActive * 2f) * 3f;
            Draw.Rect(X - 6f, Y - 6f + bob, 12f, 12f, Color.Gold * 0.6f);
            Draw.HollowRect(X - 6f, Y - 6f + bob, 12f, 12f, Color.Goldenrod);
        }
    }

    // =============================================
    // PaintCanvas - Draw temporary platforms
    // =============================================
    [CustomEntity("DZ/PaintCanvas")]
    [Tracked]
    public class PaintCanvas : Entity
    {
        private float canvasWidth, canvasHeight;
        private List<Vector2> paintPoints = new List<Vector2>();
        private float paintDuration;
        private float paintTimer = 0f;
        private bool painting = false;

        public PaintCanvas(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            canvasWidth = data.Width;
            canvasHeight = data.Height;
            paintDuration = data.Float("paintDuration", 5f);
            Collider = new Hitbox(canvasWidth, canvasHeight);
            Depth = 100;
        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null) return;

            if (CollideCheck(player) && Input.Grab.Check &&
                SceneAs<Level>().Session.GetFlag("paint_ability_active"))
            {
                if (!painting)
                {
                    painting = true;
                    paintPoints.Clear();
                    paintTimer = paintDuration;
                }
                paintPoints.Add(player.Position);
            }
            else if (painting)
            {
                painting = false;
            }

            if (paintTimer > 0 && paintPoints.Count > 0)
            {
                paintTimer -= Engine.DeltaTime;
                if (paintTimer <= 0)
                {
                    paintPoints.Clear();
                }
            }
        }

        public override void Render()
        {
            Draw.HollowRect(Position, canvasWidth, canvasHeight, Color.White * 0.1f);
            foreach (var point in paintPoints)
            {
                float alpha = Math.Max(paintTimer / paintDuration, 0.2f);
                Draw.Rect(point.X - 2f, point.Y - 2f, 4f, 4f, Color.HotPink * alpha);
            }
        }
    }
}

