using Celeste.Entities;
using DreamBlock = Celeste.DreamBlock;
using Facings = Celeste.Facings;

namespace Celeste.Cutscenes
{
    [HotReloadable]
    public class Cs04Mirror : CutsceneEntity
    {
        private global::Celeste.Player player;
        private DreamMirror mirror;
        private float playerEndX;
        private int direction = 1;
        private SoundSource sfx;
        private RalseiDummy chara;
        private RalseiMirror mirror1;

        public Cs04Mirror(global::Celeste.Player v, DreamMirror v1) {
            player = v;
            mirror = v1;
            playerEndX = mirror.X + direction * 32f;
            sfx = new SoundSource();
            Add(sfx);
        }

        public Cs04Mirror(global::Celeste.Player player, RalseiMirror mirror1)
        {
            this.player = player;
            this.mirror1 = mirror1;
        }

        public override void OnBegin(Level level)
        {
            this.Add(new Coroutine(this.cutscene(level)));
        }

        private IEnumerator cutscene(Level level)
        {
            this.Add(this.sfx = new SoundSource());
            this.sfx.Position = this.mirror.Center;
            
            this.direction = Math.Sign(this.player.X - this.mirror.X);
            this.player.StateMachine.State = Player.StDummy;
            this.playerEndX = 8 * this.direction;
            
            // Pre-intro dialog - Badeline talking about the place
            var preIntroTextbox = new Textbox("DZ_CH4_RALSEI_PREINTRO");
            yield return say(preIntroTextbox);
            
            yield return 0.5f;
            this.player.Facing = (global::Celeste.Facings)(-this.direction);
            yield return 0.4f;
            yield return this.player.DummyRunTo(this.mirror.X + this.playerEndX);
            yield return 0.5f;
            
            // Ralsei introduction dialog
            var introTextbox = new Textbox("RALSEI_INTRO");
            yield return say(introTextbox);
            
            yield return 0.5f;
            yield return level.ZoomTo(this.mirror.Position - level.Camera.Position - Vector2.UnitY * 24f, 2f, 1f);
            yield return 0.5f;
            
            // Play sound effect when breaking the mirror
            this.sfx.Play("event:/Mods/pusheen/music/lvl4/dreamblock_sting_pt1");
            yield return this.mirror.BreakRoutine(this.direction);
            
            // Ralsei freed dialog
            var freeTextbox = new Textbox("RALSEI_FREE");
            yield return say(freeTextbox);
            
            this.player.DummyAutoAnimate = false;
            if (true)
            {
                ((global::Celeste.PlayerSprite)this.player.Sprite).Play(global::Celeste.PlayerSprite.LookUp);
            }
            Vector2 from = level.Camera.Position;
            Vector2 to = level.Camera.Position + new Vector2(0.0f, -80f);
            for (float ease = 0.0f; ease < 1.0f; ease += Engine.DeltaTime * 1.2f)
            {
                level.Camera.Position = from + (to - from) * Ease.CubeInOut(ease);
                yield return null;
            }
            this.Add(new Coroutine(this.zoomBack()));

            // --- Reworked block: Safely get NightmareBlock entities ---
            var tracker = this.Scene?.Tracker;
            List<Entity> blocks = null;
            if (tracker != null)
            {
                try
                {
                    blocks = tracker.GetEntities<DreamBlock>();
                }
                catch (KeyNotFoundException)
                {
                    blocks = null;
                }
            }
            if (blocks != null)
            {
                foreach (DreamBlock block in blocks)
                {
                    yield return block.Activate();
                }
            }
            // ---------------------------------------------------------

            // Make chara disappear here if it exists
            DisappearChara();

            yield return 0.5f;
            
            // Transition to Legend Vignette
            level.Session.SetFlag("mirror_shattered");
            level.Session.SetFlag("ralsei_freed");
            FadeWipe fadeWipe = new FadeWipe(level, false, () =>
            {
                Engine.Scene = new Cs04LegendVignette(level.Session);
            });
            yield return fadeWipe.Duration;
        }

        private IEnumerator say(Textbox textbox) {
            Engine.Scene.Add(textbox);
            while (textbox.Opened) {
                yield return true;
            }
        }

        public static void EndCutscene(Level level)
        {
            level.Session.SetFlag("mirror_shattered");
        }

        public void DisappearChara()
        {
            if (this.chara != null)
            {
                this.chara.Add();
                this.chara = null;
            }
        }

        private IEnumerator zoomBack()
        {
            yield return 1.2f;
            yield return this.Level.ZoomBack(3f);
        }

        public override void OnEnd(Level level)
        {
            this.mirror.Broken(this.WasSkipped);
            if (this.WasSkipped)
                this.SceneAs<Level>().ParticlesFG.Clear();

            global::Celeste.Player entity1 = this.Scene?.Tracker?.GetEntity<global::Celeste.Player>();
            if (entity1 != null)
            {
                entity1.StateMachine.State = Player.StNormal;
                entity1.DummyAutoAnimate = true;
                entity1.Speed = Vector2.Zero;
                entity1.X = this.mirror.X + this.playerEndX;
                entity1.Facing = this.direction == 0 ? Facings.Right : (global::Celeste.Facings)(-this.direction);
            }

            // --- Safely get NightmareBlock entities ---
            var tracker = this.Scene?.Tracker;
            List<Entity> blocks = null;
            if (tracker != null)
            {
                try
                {
                    blocks = tracker.GetEntities<DreamBlock>();
                }
                catch (KeyNotFoundException)
                {
                    blocks = null;
                }
            }
            if (blocks != null)
            {
                foreach (var entity in blocks)
                {
                    var entity2 = (DreamBlock)entity;
                    entity2.ActivateNoRoutine();
                }
            }
            // ------------------------------------------

            level.ResetZoom();
            level.Session.Inventory.DreamDash = true;
            level.Session.Audio.Music.Event = "event:/Mods/pusheen/music/lvl4/alliances";
            level.Session.Audio.Apply();
        }
    }
}

