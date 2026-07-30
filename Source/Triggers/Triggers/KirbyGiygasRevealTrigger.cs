using Celeste.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Triggers
{
    /// <summary>
    /// Extras trigger that plays a "Kirby turns back and brightens" beat (mirroring
    /// CS07_SeeMaddy's KirbyTurnsBackAndBrighten routine) and then reveals the Giygas
    /// distortion backdrop, similar to the effect used by the Asriel/Giygas soul barrier
    /// finale. Designed to be dropped into a map as a standalone extra, without needing
    /// a dedicated CutsceneEntity.
    /// </summary>
    [CustomEntity(ids: "DZ/KirbyGiygasRevealTrigger")]
    [Tracked]
    public class KirbyGiygasRevealTrigger : Trigger
    {
        private readonly bool triggerOnce;
        private readonly bool lockPlayer;
        private readonly bool stayOn;
        private readonly string sessionFlag;
        private readonly Facings turnFacing;
        private readonly float turnDelay;
        private readonly float brightenTargetAlpha;
        private readonly float distortionHold;
        private readonly bool screenShake;
        private readonly float shakeIntensity;
        private readonly string revealSound;

        private bool triggered;
        private Level level;

        public KirbyGiygasRevealTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            triggerOnce = data.Bool("triggerOnce", true);
            lockPlayer = data.Bool("lockPlayer", true);
            stayOn = data.Bool("stayOn", false);
            sessionFlag = data.Attr("sessionFlag", "");
            turnFacing = data.Enum("turnFacing", Facings.Right);
            turnDelay = data.Float("turnDelay", 0.2f);
            brightenTargetAlpha = data.Float("brightenTargetAlpha", 0.3f);
            distortionHold = data.Float("distortionHold", 1.5f);
            screenShake = data.Bool("screenShake", true);
            shakeIntensity = data.Float("shakeIntensity", 0.3f);
            revealSound = data.Attr("revealSound", "event:/final_content/game/19_TheEnd/giygas_glitch_medium");

            triggered = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            if (triggerOnce && triggered)
                return;

            if (!string.IsNullOrEmpty(sessionFlag) && level != null && level.Session.GetFlag(sessionFlag))
                return;

            triggered = true;

            if (!string.IsNullOrEmpty(sessionFlag) && level != null)
                level.Session.SetFlag(sessionFlag, true);

            Add(new Coroutine(Sequence(player)));
        }

        private IEnumerator Sequence(Player player)
        {
            if (level == null)
                yield break;

            if (lockPlayer)
            {
                player.StateMachine.State = Player.StDummy;
                player.StateMachine.Locked = true;
            }

            yield return turnDelay;

            // "Kirby turns back" - flip the active player (vanilla or Kirby form) to face the reveal.
            SetPlayerFacing(player, turnFacing);

            // "...and brighten" - fade the room lighting the same way CS07_SeeMaddy.Brighten does.
            Coroutine brighten = new Coroutine(Brighten());
            Add(brighten);

            yield return 0.2f;

            // Show the Giygas distortion.
            Coroutine giygas = new Coroutine(RevealGiygas());
            Add(giygas);

            while (brighten.Active || giygas.Active)
                yield return null;

            if (lockPlayer)
            {
                player.StateMachine.Locked = false;
                player.StateMachine.State = Player.StNormal;
            }
        }

        private static void SetPlayerFacing(Player player, Facings facing)
        {
            // Kirby mode is layered on top of the vanilla Player entity (see
            // KirbyPlayerController/PlayerExtensions), so setting Facing here
            // affects both the vanilla and Kirby-form sprites.
            player.Facing = facing;
        }

        private IEnumerator Brighten()
        {
            yield return level.ZoomBack(0.5f);
            yield return 0.3f;

            level.Session.DarkRoomAlpha = brightenTargetAlpha;
            float darkness = level.Session.DarkRoomAlpha;
            while (level.Lighting.Alpha != darkness)
            {
                level.Lighting.Alpha = Calc.Approach(level.Lighting.Alpha, darkness, Engine.DeltaTime * 0.5f);
                yield return null;
            }
        }

        private IEnumerator RevealGiygas()
        {
            ToggleGiygasBackdrop(true);

            if (!string.IsNullOrEmpty(revealSound))
                Audio.Play(revealSound);

            if (screenShake)
                level.Shake(shakeIntensity);

            Tween glitchTween = Tween.Create(Tween.TweenMode.Oneshot, duration: 0.3f, start: true);
            glitchTween.OnUpdate = t => Glitch.Value = 0.5f * t.Eased;
            Add(glitchTween);

            for (float a = 0f; a < 1f; a += Engine.DeltaTime / 0.4f)
            {
                FadeGiygasBackdrop(a, true);
                yield return null;
            }
            FadeGiygasBackdrop(1f);
            Glitch.Value = 0f;

            yield return distortionHold;

            if (!stayOn)
            {
                for (float a = 0f; a < 1f; a += Engine.DeltaTime / 0.4f)
                {
                    FadeGiygasBackdrop(1f - a);
                    yield return null;
                }
                FadeGiygasBackdrop(0f);
                ToggleGiygasBackdrop(false);
            }
        }

        private void ToggleGiygasBackdrop(bool on)
        {
            if (level == null)
                return;

            foreach (Backdrop backdrop in level.Background.Backdrops)
                if (backdrop is DZ.GiygasBackdrop giygas)
                    giygas.Visible = on;

            foreach (Backdrop backdrop in level.Foreground.Backdrops)
                if (backdrop is DZ.GiygasBackdrop giygas)
                    giygas.Visible = on;
        }

        private void FadeGiygasBackdrop(float intensity, bool max = false)
        {
            if (level == null)
                return;

            foreach (Backdrop backdrop in level.Background.Backdrops)
                if (backdrop is DZ.GiygasBackdrop giygas)
                    giygas.Intensity = max ? System.Math.Max(giygas.Intensity, intensity) : intensity;

            foreach (Backdrop backdrop in level.Foreground.Backdrops)
                if (backdrop is DZ.GiygasBackdrop giygas)
                    giygas.Intensity = max ? System.Math.Max(giygas.Intensity, intensity) : intensity;
        }

        public override void Removed(Scene scene)
        {
            Glitch.Value = 0f;
            base.Removed(scene);
        }
    }
}
