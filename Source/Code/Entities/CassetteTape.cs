using Celeste;
using global::Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Entities
{
    /// <summary>
    /// A collectible cassette tape that can be played using the cassette player system.
    /// When collected, adds the tape to the player's inventory and can apply audio effects.
    /// </summary>
    [CustomEntity("MaggyHelper/CassetteTape")]
    [Tracked]
    [HotReloadable]
    public class CassetteTape : Entity
    {
        // Visual components
        private Sprite sprite;
        private Wiggler wiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private SineWave floatSine;
        private ParticleType particles;

        // Collection state
        private bool collected;
        private bool canInteract;
        private bool previewPlayed;
        
        // Audio settings
        private string tapeId;
        private string audioEvent;
        private string displayName;
        private string description;
        private Color tapeColor;
        private bool autoPlay;
        private bool oneTimeUse;
        private int remixIndex;
        private EventInstance previewEventInstance;
        
        // Interaction
        private const float InteractDistance = 32f;
        private TalkComponent talk;

        public CassetteTape(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            // Parse entity data
            tapeId = data.Attr("tapeId", "tape_default");
            audioEvent = data.Attr("audioEvent", "");
            displayName = data.Attr("displayName", "Cassette Tape");
            description = data.Attr("description", "A mysterious cassette tape.");
            tapeColor = data.HexColor("color", Color.Orange);
            autoPlay = data.Bool("autoPlay", false);
            oneTimeUse = data.Bool("oneTimeUse", false);
            remixIndex = data.Int("remixIndex", 0); // 0-17 for 18 different music options
            
            Depth = -100;
            Collider = new Hitbox(16f, 16f, -8f, -8f);

            // Add visual components
            Add(sprite = new Sprite(GFX.Game, "collectibles/maggy/tape/"));
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.AddLoop("shimmer", "shimmer", 0.08f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            sprite.Color = tapeColor;

            Add(wiggler = Wiggler.Create(0.5f, 4f, v => sprite.Scale = Vector2.One * (1f + v * 0.25f)));
            
            Add(floatSine = new SineWave(0.5f, 0f));
            floatSine.OnUpdate = f => sprite.Y = f * 2f;

            Add(bloom = new BloomPoint(0.5f, 8f));
            Add(light = new VertexLight(tapeColor, 1f, 16, 32));

            // Particle effect
            particles = new ParticleType
            {
                Source = GFX.Game["particles/rect"],
                Color = tapeColor,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SpeedMin = 4f,
                SpeedMax = 12f,
                LifeMin = 0.3f,
                LifeMax = 0.6f,
                Direction = -(float)Math.PI / 2f,
                DirectionRange = (float)Math.PI / 4f
            };

            // Add talk component for interaction
            Add(talk = new TalkComponent(
                new Rectangle(-16, -16, 32, 32),
                new Vector2(0f, -24f),
                p => OnTalk(p)
            ));
            talk.Enabled = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Check if already collected
            Level level = scene as Level;
            if (level != null && level.Session.GetFlag($"cassetteTape_{tapeId}_collected"))
            {
                if (oneTimeUse)
                {
                    RemoveSelf();
                    return;
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if (collected)
                return;

            // Check for nearby player
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                float distance = Vector2.Distance(Position, player.Center);
                canInteract = distance <= InteractDistance;
                talk.Enabled = canInteract && !collected;

                // Play preview sound when player gets close, stop when they leave
                if (canInteract && !previewPlayed)
                {
                    previewEventInstance = Audio.Play("event:/game/pusheen/general/tape_preview", Position);
                    Audio.SetParameter(previewEventInstance, "remix", remixIndex);
                    previewPlayed = true;
                }
                else if (!canInteract && previewPlayed)
                {
                    Audio.Stop(previewEventInstance);
                    previewPlayed = false;
                }

                // Particle effect when player is near
                if (canInteract && Scene.OnInterval(0.1f))
                {
                    (Scene as Level)?.ParticlesFG.Emit(particles, 1, Position, Vector2.One * 4f);
                }

                // Visual feedback when interactable
                if (canInteract)
                {
                    sprite.Play("shimmer");
                }
                else
                {
                    sprite.Play("idle");
                }
            }
        }

        private void OnTalk(global::Celeste.Player player)
        {
            if (collected)
                return;

            Collect(player);
        }

        private void Collect(global::Celeste.Player player)
        {
            if (collected)
                return;

            collected = true;
            Level level = Scene as Level;

            // Stop preview sound if playing
            Audio.Stop(previewEventInstance);

            // Set flag
            if (level != null)
            {
                level.Session.SetFlag($"cassetteTape_{tapeId}_collected", true);
            }

            // Visual/audio feedback
            Audio.Play("event:/game/pusheen/general/tape_unlocked", Position);
            wiggler.Start();
            
            // Particle burst
            for (int i = 0; i < 20; i++)
            {
                float angle = Calc.Random.NextFloat((float)Math.PI * 2f);
                level?.ParticlesFG.Emit(particles, Position, angle);
            }

            Add(new Coroutine(CollectRoutine(player)));
        }

        private IEnumerator CollectRoutine(global::Celeste.Player player)
        {
            Level level = Scene as Level;
            
            // Animate collection
            Vector2 startPos = Position;
            Vector2 targetPos = player.Center;
            
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 2f)
            {
                Position = Vector2.Lerp(startPos, targetPos, Ease.CubeIn(t));
                sprite.Scale = Vector2.One * (1f - t * 0.5f);
                yield return null;
            }

            // Flash and remove
            sprite.Visible = false;
            
            // Show collection message
            if (level != null)
            {
                yield return 0.1f;
                
                // Display tape info
                level.Session.SetFlag($"cassetteTape_{tapeId}_unlocked", true);
                
                // Auto-play if enabled
                if (autoPlay && !string.IsNullOrEmpty(audioEvent))
                {
                    yield return 0.3f;
                    Audio.SetMusic(audioEvent);
                }
            }

            RemoveSelf();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Audio.Stop(previewEventInstance);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Audio.Stop(previewEventInstance);
        }

        public override void Render()
        {
            if (collected)
                return;

            base.Render();
        }
    }
}

