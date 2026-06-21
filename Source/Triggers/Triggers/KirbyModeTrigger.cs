using Celeste.Extensions;

namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger to enable or disable Kirby mode for the player.
    /// Integrates with KirbyPlayer system for full Kirby functionality.
    /// Useful for switching between normal and Kirby player modes in CollabUtils2 submaps.
    /// </summary>
    [CustomEntity("DesoloZantas/KirbyModeTrigger")]
    [Tracked(false)]
    [HotReloadable]
    public class KirbyModeTrigger : Trigger
    {
        // Configuration
        private readonly bool enableKirby;
        private readonly bool oneUse;
        private readonly bool playEffects;
        private readonly KirbyMode.KirbyPowerState initialPower;
        private readonly string requiredFlag;
        
        // State tracking
        private bool triggered;
        
        // SFX paths
        private const string SFX_ENABLE = "event:/pusheen/char/kirby/transform_in";
        private const string SFX_DISABLE = "event:/pusheen/char/kirby/transform_out";

        public KirbyModeTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            enableKirby = data.Bool("enableKirby", true);
            oneUse = data.Bool("oneUse", false);
            playEffects = data.Bool("playEffects", true);
            requiredFlag = data.Attr("requiredFlag", "");
            
            // Parse initial power state
            var powerStr = data.Attr("initialPower", "None");
            if (Enum.TryParse<KirbyMode.KirbyPowerState>(powerStr, out var power))
            {
                initialPower = power;
            }
            else
            {
                initialPower = KirbyMode.KirbyPowerState.None;
            }
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (oneUse && triggered) return;

            try
            {
                Level level = SceneAs<Level>();
                if (level == null) return;
                
                // Check required flag
                if (!string.IsNullOrEmpty(requiredFlag) && !level.Session.GetFlag(requiredFlag))
                {
                    return;
                }

                triggered = true;

                if (enableKirby)
                {
                    EnableKirbyModeForPlayer(player, level);
                }
                else
                {
                    DisableKirbyModeForPlayer(player, level);
                }
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error($"Error in KirbyModeTrigger: {ex.Message}");
            }
        }

        private void EnableKirbyModeForPlayer(global::Celeste.Player player, Level level)
        {
            if (player.IsKirbyMode()) return;
            
            player.EnableKirbyMode();
            
            // Set initial power state if specified
            if (initialPower != KirbyMode.KirbyPowerState.None)
            {
                player.SetKirbyPowerState(initialPower);
            }
            
            IngesteLogger.Info($"Kirby mode enabled via trigger{(initialPower != KirbyMode.KirbyPowerState.None ? $" with power: {initialPower}" : "")}");

            // Visual/audio feedback
            if (playEffects)
            {
                Audio.Play(SFX_ENABLE, player.Position);
                level.ParticlesFG?.Emit(ParticleTypes.SparkyDust, 15, player.Position, Vector2.One * 16f);
            }
        }

        private void DisableKirbyModeForPlayer(global::Celeste.Player player, Level level)
        {
            if (!player.IsKirbyMode()) return;
            
            player.DisableKirbyMode();
            
            IngesteLogger.Info("Kirby mode disabled via trigger");

            // Visual/audio feedback
            if (playEffects)
            {
                Audio.Play(SFX_DISABLE, player.Position);
                level.ParticlesFG?.Emit(ParticleTypes.Dust, 10, player.Position, Vector2.One * 16f);
            }
        }

        public override void OnLeave(global::Celeste.Player player)
        {
            base.OnLeave(player);
            
            // Reset triggered flag if not one-use
            if (!oneUse)
            {
                triggered = false;
            }
        }

        public override void Render()
        {
            base.Render();

            // Debug rendering
            if (Engine.Commands?.Open ?? false)
            {
                Color debugColor = enableKirby ? Color.Pink : Color.LightBlue;
                if (triggered && oneUse) debugColor = Color.Gray;
                
                Draw.HollowRect(Collider, debugColor);
            }
        }
    }
}
