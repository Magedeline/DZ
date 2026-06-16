using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace DZ
{
    // =============================================
    // CameraShakeTrigger - Screen shake on entry
    // =============================================
    [CustomEntity("DZ/CameraShakeTrigger")]
    public class CameraShakeTrigger : Trigger
    {
        private float intensity;
        private float duration;
        private bool onlyOnce;
        private bool triggered = false;

        public CameraShakeTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            intensity = data.Float("intensity", 0.5f);
            duration = data.Float("duration", 0.5f);
            onlyOnce = data.Bool("onlyOnce", true);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (triggered && onlyOnce) return;
            triggered = true;
            (Scene as Level)?.Shake(duration);
        }
    }

    // =============================================
    // ZoomTrigger - Zooms camera in or out
    // =============================================
    [CustomEntity("DZ/ZoomTrigger")]
    public class ZoomTrigger : Trigger
    {
        private float targetZoom;
        private float zoomSpeed;

        public ZoomTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            targetZoom = data.Float("targetZoom", 2f);
            zoomSpeed = data.Float("zoomSpeed", 2f);
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            Level level = SceneAs<Level>();
            level.Camera.Zoom = Calc.Approach(level.Camera.Zoom, targetZoom, zoomSpeed * Engine.DeltaTime);
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            Level level = SceneAs<Level>();
            level.Camera.Zoom = 1f;
        }
    }

    // =============================================
    // ScreenFlashTrigger - Flashes the screen a color
    // =============================================
    [CustomEntity("DZ/ScreenFlashTrigger")]
    public class ScreenFlashTrigger : Trigger
    {
        private Color flashColor;
        private float flashAlpha;
        private bool onlyOnce;
        private bool triggered = false;

        public ScreenFlashTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            flashColor = Calc.HexToColor(data.Attr("color", "ffffff"));
            flashAlpha = data.Float("alpha", 0.5f);
            onlyOnce = data.Bool("onlyOnce", true);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (triggered && onlyOnce) return;
            triggered = true;
            (Scene as Level)?.Flash(flashColor * flashAlpha);
        }
    }

    // =============================================
    // PixelationTrigger - Retro pixelation effect
    // =============================================
    [CustomEntity("DZ/PixelationTrigger")]
    public class PixelationTrigger : Trigger
    {
        private float pixelSize;
        private float fadeSpeed;

        public PixelationTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            pixelSize = data.Float("pixelSize", 4f);
            fadeSpeed = data.Float("fadeSpeed", 1f);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            SceneAs<Level>().Session.SetFlag("pixelation_active", true);
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            SceneAs<Level>().Session.SetFlag("pixelation_active", false);
        }
    }

    // =============================================
    // VignetteTrigger - Adds dark vignette edges
    // =============================================
    [CustomEntity("DZ/VignetteTrigger")]
    public class VignetteTrigger : Trigger
    {
        private float vignetteStrength;

        public VignetteTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            vignetteStrength = data.Float("strength", 0.5f);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            SceneAs<Level>().Session.SetFlag("vignette_active", true);
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            Level level = SceneAs<Level>();
            level.Lighting.Alpha = Calc.Approach(level.Lighting.Alpha, vignetteStrength, Engine.DeltaTime);
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            SceneAs<Level>().Session.SetFlag("vignette_active", false);
        }
    }

    // =============================================
    // ColorShiftTrigger - Shifts room color palette
    // =============================================
    [CustomEntity("DZ/ColorShiftTrigger")]
    public class ColorShiftTrigger : Trigger
    {
        private string colorGrade;
        private float fadeSpeed;

        public ColorShiftTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            colorGrade = data.Attr("colorGrade", "templevoid");
            fadeSpeed = data.Float("fadeSpeed", 1f);
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            Level level = SceneAs<Level>();
            if (level.Session.ColorGrade != colorGrade)
            {
                level.SnapColorGrade(colorGrade);
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            Level level = SceneAs<Level>();
            level.SnapColorGrade(null);
        }
    }

    // =============================================
    // ParallaxShiftTrigger - Changes background parallax
    // =============================================
    [CustomEntity("DZ/ParallaxShiftTrigger")]
    public class ParallaxShiftTrigger : Trigger
    {
        private float scrollSpeedX;
        private float scrollSpeedY;

        public ParallaxShiftTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            scrollSpeedX = data.Float("scrollSpeedX", 1f);
            scrollSpeedY = data.Float("scrollSpeedY", 0f);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            SceneAs<Level>().Session.SetFlag("parallax_shift_active", true);
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            SceneAs<Level>().Session.SetFlag("parallax_shift_active", false);
        }
    }

    // =============================================
    // SplitScreenTrigger - Shows two areas at once
    // =============================================
    [CustomEntity("DZ/SplitScreenTrigger")]
    public class SplitScreenTrigger : Trigger
    {
        private Vector2 secondCameraTarget;
        private string splitDirection; // "horizontal", "vertical"

        public SplitScreenTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            splitDirection = data.Attr("splitDirection", "horizontal");
            if (data.Nodes.Length > 0)
                secondCameraTarget = data.NodesOffset(offset)[0];
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            SceneAs<Level>().Session.SetFlag("split_screen_active", true);
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            SceneAs<Level>().Session.SetFlag("split_screen_active", false);
        }
    }
}
