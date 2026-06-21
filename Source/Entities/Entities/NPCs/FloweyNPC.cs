using System.Runtime.CompilerServices;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Flowey NPC entity for cutscenes and room interactions.
    /// Spawns Flowey with full sprite animations (idle, popin, knockout, grin, laugh, pissed, shout, evil).
    /// Used by FloweyIntroScene and other Chapter 10 cutscenes.
    /// </summary>
    [CustomEntity("DesoloZantas/FloweyNPC")]
    [Tracked]
    [HotReloadable]
    public class FloweyNPC : Entity
    {
        #region Constants
        private const string DEFAULT_DIALOG = "DZ_CH10_FLOWEY_INTRO";
        private const string SFX_EMERGE = "event:/pusheen/sfx/lvl10/flowey_emerge";
        private const string SFX_LAUGH = "event:/pusheen/char/others/Flowey_Laugh";
        private const float EMERGE_DURATION = 0.5f;
        #endregion

        #region Fields
        public Sprite Sprite;
        public TalkComponent Talker;
        public VertexLight Light;

        private readonly string dialogId;
        private readonly bool startHidden;
        private readonly bool autoEmerge;
        private readonly float emergeDelay;
        private bool hasEmerged;
        private Level level;
        #endregion

        #region Properties
        /// <summary>Whether Flowey has emerged from the ground and is visible.</summary>
        public bool HasEmerged => hasEmerged;

        /// <summary>Current facing direction. -1 = left, 1 = right.</summary>
        public int Facing
        {
            get => Math.Sign(Sprite.Scale.X) == 0 ? 1 : Math.Sign(Sprite.Scale.X);
            set => Sprite.Scale.X = value >= 0 ? 1f : -1f;
        }
        #endregion

        /// <summary>
        /// Constructor for Loenn/map placement via EntityData.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public FloweyNPC(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            dialogId = data.Attr("dialogId", DEFAULT_DIALOG);
            startHidden = data.Bool("startHidden", true);
            autoEmerge = data.Bool("autoEmerge", false);
            emergeDelay = data.Float("emergeDelay", 0.5f);

            SetupEntity();
        }

        /// <summary>
        /// Constructor for programmatic creation (e.g. from cutscenes).
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public FloweyNPC(Vector2 position, string dialogId = DEFAULT_DIALOG, bool startHidden = true)
            : base(position)
        {
            this.dialogId = dialogId;
            this.startHidden = startHidden;
            autoEmerge = false;
            emergeDelay = 0.5f;

            SetupEntity();
        }

        private void SetupEntity()
        {
            Collider = new Hitbox(12f, 12f, -6f, -12f);

            // Setup sprite from Sprites.xml bank
            try
            {
                Sprite = GFX.SpriteBank.Create("maggy_flowey");
                Add(Sprite);
                if (Sprite.Has("idle"))
                    Sprite.Play("idle");
                else if (Sprite.CurrentAnimationID != null)
                { /* Keep current animation */ }
                else
                    Logger.Log(LogLevel.Warn, "FloweyNPC", "Sprite bank 'maggy_flowey' loaded but 'idle' animation not found");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "FloweyNPC", $"Failed to load sprite bank 'maggy_flowey': {ex.Message}");
                Sprite = null;
            }

            // Talk component for player interaction
            Add(Talker = new TalkComponent(
                new Rectangle(-16, -24, 32, 24),
                new Vector2(0f, -24f),
                OnTalk
            ));

            // Lighting
            Add(Light = new VertexLight(Color.White, 1f, 16, 32));

            Depth = 100;

            if (startHidden)
            {
                Visible = false;
                Talker.Enabled = false;
                hasEmerged = false;
            }
            else
            {
                hasEmerged = true;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;

            if (autoEmerge && startHidden)
            {
                Add(new Coroutine(AutoEmergeSequence()));
            }
        }

        public override void Update()
        {
            base.Update();

            // Update light based on bounds
            if (Light != null && level != null)
            {
                Rectangle bounds = level.Bounds;
                Light.Alpha = Calc.Approach(Light.Alpha,
                    (X <= bounds.Left - 16 || Y <= bounds.Top - 16 ||
                     X >= bounds.Right + 16 || Y >= bounds.Bottom + 16 ||
                     level.Transitioning)
                        ? 0f
                        : 1f, Engine.DeltaTime * 2f);
            }
        }

        public override void Render()
        {
            base.Render();
        }

        #region Cutscene Actions

        /// <summary>
        /// Flowey emerges from the ground with screen shake and sound.
        /// </summary>
        public IEnumerator Emerge()
        {
            if (hasEmerged) yield break;

            // Make visible and play emerge animation
            Visible = true;
            // Note: "sprout" animation not available, use "idle" instead
            if (Sprite.Has("sprout"))
                Sprite.Play("sprout");
            else
                Sprite.Play("idle");
            try { Audio.Play(SFX_EMERGE, Position); }
            catch { }

            // Screen shake for dramatic effect
            if (level != null)
            {
                level.Shake(0.2f);
            }

            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);

            // Rise up from underground
            float startY = Y + 16f;
            float endY = Y;
            Y = startY;

            for (float t = 0f; t < 1f; t += Engine.DeltaTime / EMERGE_DURATION)
            {
                Y = MathHelper.Lerp(startY, endY, Ease.CubeOut(Math.Min(t, 1f)));
                yield return null;
            }

            Y = endY;

            yield return 0.2f;
            if (Sprite != null && Sprite.Has("idle"))
                Sprite.Play("idle");
            Talker.Enabled = true;
            hasEmerged = true;
        }

        /// <summary>
        /// Flowey retreats back into the ground and disappears.
        /// </summary>
        public IEnumerator Retreat()
        {
            if (!hasEmerged) yield break;

            Talker.Enabled = false;
            if (Sprite.Has("knockout"))
                Sprite.Play("knockout");
            else
                Sprite.Play("idle");

            float startY = Y;
            float endY = Y + 16f;

            for (float t = 0f; t < 1f; t += Engine.DeltaTime / EMERGE_DURATION)
            {
                Y = MathHelper.Lerp(startY, endY, Ease.CubeIn(Math.Min(t, 1f)));
                yield return null;
            }

            Visible = false;
            Y = startY;
            hasEmerged = false;
        }

        /// <summary>
        /// Play a specific expression/animation. 
        /// Valid: idle, angry, creepy, creepylaugh, idlecreepy, dancing, knockout, sprout, unsprout, hit, seedspins, grab, letgo
        /// </summary>
        public void PlayExpression(string expression)
        {
            if (Sprite.Has(expression))
            {
                Sprite.Play(expression);
            }
        }

        /// <summary>
        /// Make Flowey face a target position.
        /// </summary>
        public void FaceTarget(Vector2 target)
        {
            Facing = target.X > X ? 1 : -1;
        }

        /// <summary>
        /// Make Flowey face the player.
        /// </summary>
        public void FacePlayer()
        {
            var player = level?.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                FaceTarget(player.Position);
            }
        }

        /// <summary>
        /// Coroutine: Flowey laughs with sound and animation.
        /// </summary>
        public IEnumerator Laugh(float duration = 1.0f)
        {
            if (Sprite.Has("creepylaugh"))
                Sprite.Play("creepylaugh");
            else
                Sprite.Play("idle");
            try { Audio.Play(SFX_LAUGH, Position); }
            catch { }

            if (level != null)
            {
                level.Shake(0.2f);
            }

            yield return duration;
            if (Sprite != null && Sprite.Has("idle"))
                Sprite.Play("idle");
        }

        /// <summary>
        /// Coroutine: Flowey does the evil grin.
        /// </summary>
        public IEnumerator EvilGrin(float duration = 0.8f)
        {
            if (Sprite.Has("creepy"))
                Sprite.Play("creepy");
            else
                Sprite.Play("idle");
            yield return duration;
        }

        /// <summary>
        /// Coroutine: Flowey gets hit and knocked back.
        /// </summary>
        public IEnumerator GetHit(Vector2 knockbackDirection, float knockbackDistance = 32f)
        {
            if (Sprite.Has("knockout"))
                Sprite.Play("knockout");
            else
                Sprite.Play("idle");

            Vector2 start = Position;
            Vector2 target = Position + knockbackDirection * knockbackDistance;

            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 3f)
            {
                Position = Vector2.Lerp(start, target, Ease.CubeOut(Math.Min(t, 1f)));
                yield return null;
            }

            Position = target;
            yield return 0.3f;
        }

        #endregion

        #region Private Methods

        private IEnumerator AutoEmergeSequence()
        {
            yield return emergeDelay;
            yield return Emerge();
        }

        private void OnTalk(global::Celeste.Player player)
        {
            if (!string.IsNullOrEmpty(dialogId) && hasEmerged)
            {
                // Start the FloweyIntroScene cutscene if dialog matches
                level?.Add(new FloweyIntroSceneTrigger(player, this));
            }
        }

        #endregion
    }

    /// <summary>
    /// Helper entity that triggers the FloweyIntroScene cutscene when
    /// the player interacts with or enters a room containing a FloweyNPC.
    /// Place this as a trigger zone in the room to auto-start the cutscene on room entry.
    /// </summary>
    [CustomEntity("DesoloZantas/FloweyIntroTrigger")]
    [HotReloadable]
    public class FloweyIntroSceneTrigger : CutsceneEntity
    {
        private global::Celeste.Player player;
        private FloweyNPC flowey;

        /// <summary>
        /// Constructor for Everest entity loader (map data).
        /// Player and FloweyNPC are resolved in OnBegin.
        /// </summary>
        public FloweyIntroSceneTrigger(EntityData data, Vector2 offset)
            : base(true, false)
        {
        }

        /// <summary>
        /// Constructor for programmatic creation.
        /// </summary>
        public FloweyIntroSceneTrigger(global::Celeste.Player player, FloweyNPC flowey)
            : base(true, false)
        {
            this.player = player;
            this.flowey = flowey;
        }

        public override void OnBegin(Level level)
        {
            if (player == null)
                player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (flowey == null)
                flowey = level.Entities.OfType<FloweyNPC>().FirstOrDefault();

            if (player?.StateMachine != null)
            {
                player.StateMachine.State = global::Celeste.Player.StDummy;
            }

            Add(new Coroutine(TalkSequence(level)));
        }

        private IEnumerator TalkSequence(Level level)
        {
            yield return 0.2f;

            if (flowey != null)
            {
                flowey.FacePlayer();
                flowey.PlayExpression("idlecreepy");
            }

            yield return Textbox.Say(flowey != null ? "DZ_CH10_FLOWEY_INTRO" : "DZ_CH10_FLOWEY_INTRO");

            yield return 0.3f;
            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            if (player?.StateMachine != null)
            {
                player.StateMachine.State = global::Celeste.Player.StNormal;
            }

            if (flowey != null)
            {
                flowey.PlayExpression("idle");
            }
        }
    }
}
