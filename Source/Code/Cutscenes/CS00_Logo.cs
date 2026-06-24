using System.Collections;
using global::Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using global::Celeste.Mod.MaggyHelper.Cutscenes;
using Celeste.Cutscenes;
using FMOD.Studio;

namespace Celeste.Mod.MaggyHelper.Cutscenes {
    public class CS00_Logo : CutsceneEntity {

        public class TitleLogo : Entity {

            private class Particle {
                public readonly MTexture texture = GFX.Gui["logo/sparkle"];
                public const float DefaultParticleLifetime = 1f;
                public Vector2 offset;
                public float opacity;
                public float size;

                private const float StartingSize = 0.3f;

                private float livedTime;
                private float lifetime;
                private float relSize;
                private float preLivedTime;
                private float appearTime;
                private float driftAngle;
                private float driftSpeed;

                public Particle(float timeOffset) {
                    Reset(timeOffset);
                }

                public void Update() {
                    if (preLivedTime < appearTime) {
                        preLivedTime += Engine.DeltaTime;
                        opacity = 0f;
                        return;
                    }
                    if (livedTime > lifetime) {
                        Reset(0f, rand.NextFloat(DefaultParticleLifetime) + 1.5f);
                    }
                    livedTime += Engine.DeltaTime;
                    float percLived = livedTime / lifetime;
                    float adjustedEaser = (-4 * (float)Math.Pow(percLived - 0.5f, 2)) + 1;
                    opacity = adjustedEaser * 0.6f;
                    size = ((adjustedEaser / (1 / StartingSize)) + StartingSize) * relSize;
                    offset.Y -= Engine.DeltaTime * driftSpeed;
                    offset.X += (float)Math.Sin(livedTime * 2f + driftAngle) * Engine.DeltaTime * 15f;
                }

                private void Reset(float time = 0f, float lifetime = DefaultParticleLifetime) {
                    this.lifetime = lifetime;
                    preLivedTime = 0f;
                    appearTime = rand.NextFloat(4f);
                    livedTime = time % lifetime;
                    relSize = rand.NextFloat(1f) + 0.25f;
                    offset = new Vector2(rand.Next(-700, 700), rand.Next(-200, 200));
                    driftAngle = rand.NextFloat((float)Math.PI * 2f);
                    driftSpeed = rand.NextFloat(20f) + 10f;
                    opacity = 0f;
                    size = 0.5f * relSize;
                }
            }

            private const int ParticleCount = 18;
            private ArrayList particles;

            private Sprite sprite;
            private float opacity;
            private float size;
            private float ghostTimer;
            private float ghostPulse;
            private bool fadeComplete;

            public Vector2 pos;
            private Vector2 initPos;
            private Vector2 basePos;

            private int renderPhase;

            private static Random rand;

            public TitleLogo() {
                rand = new Random();
                sprite = new Sprite(GFX.Gui, "logo/logo");
                renderPhase = 1;
                sprite.AddLoop("wave", "logo", 0.08f);
                sprite.AddLoop("idle", "logoIdle", 1.5f);
                sprite.Play("idle");
                sprite.OnLoop = delegate {
                    renderPhase++;
                    if (renderPhase == 2) {
                        sprite.Play("idle");
                    } else if (renderPhase == 3) {
                        renderPhase = 0;
                        sprite.Play("wave");
                    }
                };
                Tag = Tags.HUD;
                initPos = new Vector2(960, 0);
                pos = initPos;
                basePos = pos;
                opacity = 0f;
                size = 0f;
                ghostTimer = 0f;
                ghostPulse = 0f;
                fadeComplete = false;
                particles = new ArrayList();
                for (int i = 0; i < ParticleCount; i++) {
                    particles.Add(new Particle(rand.NextFloat(Particle.DefaultParticleLifetime)));
                }
            }

            public override void Render() {
                float ghostAlpha = opacity;

                // Ghostly double-image: faint larger copy behind the main logo
                if (ghostAlpha > 0.05f) {
                    float echoAlpha = ghostAlpha * 0.2f;
                    float echoSize = size * 1.05f;
                    sprite.Texture.DrawCentered(pos + new Vector2(0, -2f), Color.White * echoAlpha, echoSize);
                }

                sprite.Texture.DrawCentered(pos, Color.White * ghostAlpha, size);

                foreach (Particle particle in particles) {
                    particle.texture.DrawCentered(
                        pos + (particle.offset * size),
                        Color.White * particle.opacity * ghostAlpha,
                        particle.size * size * 0.15f
                    );
                }
            }

            public override void Update() {
                base.Update();
                sprite.Update();
                ghostTimer += Engine.DeltaTime;

                // After fade-in completes, apply a gentle breathing pulse and floating bob
                if (fadeComplete) {
                    ghostPulse += Engine.DeltaTime;
                    float breathe = (float)Math.Sin(ghostPulse * 1.2f) * 0.06f;
                    opacity = 0.78f + breathe;
                    float bob = (float)Math.Sin(ghostPulse * 0.8f) * 4f;
                    pos = basePos + new Vector2(0, bob);
                }

                foreach (Particle particle in particles) {
                    particle.Update();
                }
            }

            public IEnumerator EaseIn() {
                // Phase 1: Ghostly flicker-in (Deltarune style)
                // Opacity oscillates upward in waves rather than a clean ease
                float duration = 4f;
                for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration) {
                    float baseAlpha = Ease.SineOut(p);
                    // Add a flickering sine wave that diminishes as we approach full
                    float flicker = (float)Math.Sin(p * 18f) * 0.15f * (1f - p);
                    opacity = Math.Max(0f, (baseAlpha * 0.8f) + flicker);

                    pos = initPos + (Celeste.TargetCenter - initPos) * Ease.SineOut(p);
                    size = Ease.SineOut(p) * 0.5f + 0.5f;
                    yield return null;
                }
                // Phase 2: Settle to ghostly resting opacity (not fully opaque)
                float settleFrom = opacity;
                float settleTo = 0.78f;
                for (float p = 0f; p < 1f; p += Engine.DeltaTime / 1f) {
                    opacity = MathHelper.Lerp(settleFrom, settleTo, Ease.SineInOut(p));
                    yield return null;
                }
                opacity = settleTo;
                size = 1f;
                basePos = Celeste.TargetCenter;
                pos = basePos;
                fadeComplete = true;
            }
        }

        private Player player;
        private CS00_EndingMod basket;
        private TitleLogo logo;
        private Vector2 buttonTarget = new Vector2(1728, 972);
        private Vector2 buttonOffScreen = new Vector2(1728, 1188);
        private Vector2 buttonPos;

        public CS00_Logo(Player player, CS00_EndingMod basket) {
            this.player = player;
            this.basket = basket;
            Tag = Tags.HUD;
            buttonPos = buttonOffScreen;
        }

        public override void Render() {
            base.Render();
            MTexture confirmButton = Input.GuiButton(Input.MenuConfirm, "controls/keyboard/oemquestion");
            confirmButton.DrawCentered(buttonPos, Color.White, 1f);
        }

        public override void OnBegin(Level level) {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level) {
            player.StateMachine.State = Player.StDummy;
            player.Dashes = 1;
            global::Celeste.Audio.SetMusicParam("outro", 1);
            yield return 0.5f;
            yield return player.DummyWalkTo(basket.X - 12f);
            yield return 0.4f;
            player.Facing = Facings.Right;
            yield return 0.3f;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("sitDown");
            yield return 2f;
            Add(new Coroutine(PanCamera(level)));
            yield return 3f;
            logo = new TitleLogo();
            Scene.Add(logo);
            yield return logo.EaseIn();
            // Celeste 64 boot title hit â€” play when logo lands
            // Custom FMOD event, skipped if not available
            yield return 4f;
            yield return ShowConfirmButton();
            while (!Input.MenuConfirm.Pressed) {
                yield return null;
            }
            // Title confirm out sound (using vanilla audio)
            global::Celeste.Audio.Play("event:/ui/main/title_firstinput");
            EndCutscene(level);
        }

        private IEnumerator ShowConfirmButton() {
            Vector2 src = buttonPos;
            Vector2 dest = buttonTarget;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 3) {
                buttonPos = (dest - src) * Ease.CubeOut(p) + src;
                yield return null;
            }
        }

        private IEnumerator PanCamera(Level level) {
            Vector2 from = level.Camera.Position;
            Vector2 to = from - Vector2.UnitY * 3000f;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / 4.5f) {
                level.Camera.Position = Vector2.Lerp(from, to, Ease.CubeInOut(p));
                yield return null;
            }
        }

        public override void OnEnd(Level level) {
            level.CompleteArea(false, false, true);
        }
    }
}
