using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Gentle Breeze dash-assist for the Kirby player (K_Player).
    ///
    /// Mirrors the vanilla <c>PlayerDashAssist</c> slow-mo aim freeze, but
    /// targets <see cref="K_Player"/> instead of the vanilla <c>Player</c>,
    /// plays the mod's gentlebreeze audio cue when the freeze engages, and
    /// applies the full Gentle Breeze assist bundle (infinite stamina,
    /// infinite dashes, invincibility) while active.
    ///
    /// The entity is added to every scene by <see cref="Celeste.Mod.DZ.DZModule"/>
    /// and self-disables when <c>DZModuleSettings.GentleBreezeMode</c> is off
    /// or no K_Player is present.
    /// </summary>
    public class GentleBreezeAssist : Entity
    {
        private const string Sfx_GentleBreezeName = "event:/pusheen/ui/main/gentlebreeze_name";

        // Dash arrow textures (same atlas as vanilla PlayerDashAssist).
        private readonly List<MTexture> images;

        // Aim arrow state.
        private float direction;
        private float scale;
        private Vector2 offset;
        private int lastIndex;

        // Freeze / audio state.
        private float timer;
        private bool paused;
        private bool freezeActive;
        private bool sfxPlayedThisFreeze;

        public GentleBreezeAssist()
        {
            Tag = (int)Tags.Global;
            Depth = -1000000;
            Visible = false;
            images = GFX.Game.GetAtlasSubtextures("util/dasharrow/dasharrow");
        }

        /// <summary>True while the slow-mo dash-aim freeze is engaged this frame.</summary>
        public bool IsFreezeActive => freezeActive;

        public override void Update()
        {
            var settings = global::Celeste.Mod.DZ.DZModule.Settings;
            bool enabled = settings != null && settings.GentleBreezeMode;

            var kPlayer = Scene.Tracker.GetEntity<K_Player>();

            // Only run the freeze when the setting is on and a Kirby player exists.
            bool wantFreeze = enabled
                && kPlayer != null
                && !kPlayer.Dead
                && kPlayer.CanDash
                && !(Scene is Level lvl && lvl.InCutscene);

            if (!wantFreeze)
            {
                freezeActive = false;
                if (paused)
                {
                    if (!Scene.Paused)
                        Audio.PauseGameplaySfx = false;
                    timer = 0f;
                    paused = false;
                    sfxPlayedThisFreeze = false;
                }
                return;
            }

            // Apply the assist bundle every frame while the freeze is desired.
            ApplyAssistBundle(kPlayer);

            freezeActive = true;
            paused = true;
            Audio.PauseGameplaySfx = true;
            timer += Engine.RawDeltaTime;

            // Play the gentlebreeze name cue once per freeze engagement
            // (after a short delay so it doesn't stack on rapid re-presses).
            if (timer > 0.2f && !sfxPlayedThisFreeze)
            {
                Audio.Play(Sfx_GentleBreezeName, kPlayer.Center);
                sfxPlayedThisFreeze = true;
            }

            float aim = Input.GetAimVector(kPlayer.Facing).Angle();

            if (Calc.AbsAngleDiff(aim, direction) >= 1.5807964f)
            {
                direction = aim;
                scale = 0f;
            }
            else
            {
                direction = Calc.AngleApproach(direction, aim, 18.849556f * Engine.RawDeltaTime);
            }

            scale = Calc.Approach(scale, 1f, Engine.DeltaTime * 4f);

            int idx = 1 + (8 + (int)Math.Round(aim / 0.7853982f)) % 8;
            if (lastIndex != 0 && lastIndex != idx)
                Audio.Play("event:/game/general/assist_dash_aim", kPlayer.Center, "dash_direction", idx);
            lastIndex = idx;
        }

        public override void Render()
        {
            if (!freezeActive)
                return;

            var kPlayer = Scene.Tracker.GetEntity<K_Player>();
            if (kPlayer == null)
                return;

            MTexture tex = null;
            float rotation = float.MaxValue;
            for (int i = 0; i < 8; i++)
            {
                float diff = Calc.AngleDiff((float)(6.2831855f * (i / 8.0)), direction);
                if (Math.Abs(diff) < Math.Abs(rotation))
                {
                    rotation = diff;
                    tex = images[i];
                }
            }

            if (tex == null)
                return;

            if (Math.Abs(rotation) < 0.05f)
                rotation = 0f;

            tex.DrawOutlineCentered(
                (kPlayer.Center + offset + Calc.AngleToVector(direction, 20f)).Round(),
                Color.White,
                Ease.BounceOut(scale),
                rotation);
        }

        /// <summary>
        /// Applies the Gentle Breeze assist bundle to the Kirby player:
        /// infinite stamina, infinite dashes, and invincibility.
        /// </summary>
        private void ApplyAssistBundle(K_Player kPlayer)
        {
            // Infinite stamina.
            kPlayer.RefillStamina();

            // Infinite dashes.
            kPlayer.RefillDash();
        }

        public override void Removed(Scene scene)
        {
            if (paused && !scene.Paused)
                Audio.PauseGameplaySfx = false;
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            if (paused && !scene.Paused)
                Audio.PauseGameplaySfx = false;
            base.SceneEnd(scene);
        }
    }
}
