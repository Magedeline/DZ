using MaggyHelper.Extensions;

namespace MaggyHelper.Entities
{
    /// <summary>
    /// A dream block variant that works for both Kirby mode and the normal player.
    ///
    /// - Normal Madeline: requires the dream dash inventory item (standard vanilla behavior).
    ///   The block is tracked as DreamBlock so the Player's own dream-dash state machine
    ///   recognises it without any extra hooks.
    ///
    /// - Kirby mode player: can hover / float through the block without dream dash.
    ///   While the Kirby hover ability is active and the player is adjacent to or inside
    ///   the block, the solid becomes temporarily non-collidable so Kirby drifts through.
    ///   Collidability is restored as soon as Kirby exits or stops hovering.
    ///
    /// Visual appearance inherits from vanilla DreamBlock (active rainbow palette when the
    /// session has dream dash, grey-teal when it does not).  Kirby is force-shown the
    /// active palette via DynamicData when kirby mode is first detected.
    /// </summary>
    [CustomEntity("MaggyHelper/KirbyDreamBlock")]
    [TrackedAs(typeof(DreamBlock), true)]
    [HotReloadable]
    public class KirbyDreamBlock : DreamBlock
    {
        // ── state ────────────────────────────────────────────────────────────────
        private bool _kirbyPassActive;

        // ── constructor ──────────────────────────────────────────────────────────
        public KirbyDreamBlock(EntityData data, Vector2 offset)
            : base(data, offset) { }

        // ── lifecycle ────────────────────────────────────────────────────────────
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            TryActivateKirbyVisuals();
        }

        /// <summary>
        /// If a Kirby-mode player is present when the block wakes, force the dream
        /// block into its "active" (colourful) visual state so the player can see
        /// that the block is usable even without the dream-dash inventory item.
        /// </summary>
        private void TryActivateKirbyVisuals()
        {
            var player = Scene?.Tracker.GetEntity<Player>();
            if (player == null || !player.IsKirbyMode())
                return;

            // Write through the private field via DynamicData so the vanilla
            // DreamBlock.Render() picks up the active-state palette.
            var baseData = new MonoMod.Utils.DynamicData(typeof(DreamBlock), this);
            baseData.Set("playerHasDreamDash", true);

            // Re-initialise particles so they use the active colour set.
            var setup = typeof(DreamBlock).GetMethod("Setup",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            setup?.Invoke(this, null);
        }

        // ── update ───────────────────────────────────────────────────────────────
        public override void Update()
        {
            base.Update();
            HandleKirbyHover();
        }

        private void HandleKirbyHover()
        {
            var player = Scene?.Tracker.GetEntity<Player>();

            // Not in Kirby mode → make sure collidable is restored.
            if (player == null || !player.IsKirbyMode())
            {
                RestoreCollidable();
                return;
            }

            // Check whether the hover ability is active.
            var kirbyExt = (Scene as Level)?.Tracker.GetEntity<KirbyPlayerExtension>();
            bool isHovering = kirbyExt?.AbilityManager?.Get<KirbyHoverAbility>()?.IsHovering ?? false;

            if (isHovering)
            {
                // The block becomes passable when the player is inside or
                // moving into it (look-ahead of ~4 pixels in movement direction).
                bool nearOrInside = CollideCheck(player)
                    || (player.Speed.LengthSquared() > 4f
                        && CollideCheck(player, player.Position + player.Speed.SafeNormalize(4f)));

                if (nearOrInside)
                {
                    Collidable = false;
                    _kirbyPassActive = true;
                }
                else if (_kirbyPassActive && !CollideCheck(player))
                {
                    RestoreCollidable();
                }
            }
            else
            {
                RestoreCollidable();
            }
        }

        private void RestoreCollidable()
        {
            if (_kirbyPassActive)
            {
                Collidable = true;
                _kirbyPassActive = false;
            }
        }
    }
}
