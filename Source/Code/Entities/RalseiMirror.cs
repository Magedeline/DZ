#nullable disable
using Celeste.Cutscenes;

namespace Celeste.Entities
{
    [CustomEntity(new string[] { "MaggyHelper/RalseiMirror" })]
    public class RalseiMirror : Entity
    {
        public static ParticleType PShatter;
        private Monocle.Image frame;
        private MTexture glassbg = GFX.Game["objects/mirror/glassbg"];
        private MTexture glassfg = GFX.Game["objects/mirror/glassfg"];
        private Sprite breakingGlass;
        private Hitbox hitbox;
        private VirtualRenderTarget mirror;
        private float shineAlpha = 0.5f;
        private float shineOffset;
        private Entity reflection;
        private global::Celeste.PlayerSprite reflectionSprite;

        private float reflectionAlpha = 0.7f;
        private bool autoUpdateReflection = true;
        private RalseiDummy chara;
        private bool smashed;
        private bool smashEnded;
        private bool updateShine = true;
        private Coroutine smashCoroutine;
        private SoundSource sfx;
        private SoundSource sfxSting;
        private bool betacube;

        public bool Betacube => betacube;

        static RalseiMirror()
        {
            // Use a fallback particle type if 'Shatter' is not defined in ParticleTypes
            if (ParticleTypes.Dust != null) // Example check for fallback
            {
                PShatter = ParticleTypes.Dust;
            }
        }

        public RalseiMirror(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            this.betacube = data.Bool("betacube", false);
            this.Depth = 9500;
            this.Add((Component)(this.breakingGlass = GFX.SpriteBank.Create("glass")));
            this.breakingGlass.Play("idle");
            this.Add((Component)new BeforeRenderHook(new Action(this.beforeRender)));

            this.hitbox = new Hitbox(this.glassbg.Width, this.glassbg.Height, -this.glassbg.Width / 2f, -this.glassbg.Height);
            this.Collider = this.hitbox;

            // Add a frame image if you want a visible border/frame (optional)
            this.frame = new Monocle.Image(GFX.Game["objects/mirror/frame"]);
            this.frame.Origin = new Vector2(this.glassbg.Width / 2f, this.glassbg.Height);
            // Fix: Set the frame's position so it matches the mirror's position and aligns visually
            this.frame.Position = Vector2.Zero; // Frame will be drawn at the entity's position
            this.Add(this.frame);

            foreach (MTexture atlasSubtexture in GFX.Game.GetAtlasSubtextures("objects/mirror/mirrormask"))
            {
                MTexture shard = atlasSubtexture;
                MirrorSurface surface = new MirrorSurface();
                surface.OnRender = (Action)(() => shard.DrawJustified(this.Position, new Vector2(0.5f, 1f), surface.ReflectionColor * (this.smashEnded ? 1f : 0.0f)));
                surface.ReflectionOffset = new Vector2((float)(9 + Calc.Random.Range(-4, 4)), (float)(4 + Calc.Random.Range(-2, 2)));
                this.Add((Component)surface);
            }
        }

        public void Load()
        {
            // Allocate the mirror render target if not already created
            if (this.mirror == null)
            {
                this.mirror = VirtualContent.CreateRenderTarget("dream-mirror", this.glassbg.Width, this.glassbg.Height);
            }
            // Optionally, reset or initialize other state here if needed
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

            // Call Load when entity is added to scene
            Load();

            this.smashed = this.SceneAs<Level>().Session.Inventory.DreamDash;
            if (this.smashed)
            {
                this.breakingGlass.Play("broken");
                this.smashEnded = true;
            }
            else
            {
                this.reflection = new Entity();
                this.reflectionSprite = new global::Celeste.PlayerSprite(global::Celeste.PlayerSpriteMode.Madeline);
                this.reflection.Add((Component)this.reflectionSprite);

                this.reflectionSprite.OnFrameChange = (Action<string>)(anim =>
                {
                    if (this.smashed || !this.CollideCheck<global::Celeste.Player>())
                        return;
                    int currentAnimationFrame = this.reflectionSprite.CurrentAnimationFrame;
                    if ((anim != "walk" || (currentAnimationFrame != 0 && currentAnimationFrame != 6)) &&
                        (anim != "runSlow" || (currentAnimationFrame != 0 && currentAnimationFrame != 6)) &&
                        (anim != "runFast" || (currentAnimationFrame != 0 && currentAnimationFrame != 6)))
                        return;
                    Audio.Play("event:/char/badeline/footstep", this.Center);
                });
                this.Add((Component)(this.smashCoroutine = new Coroutine(this.interactRoutine())));
            }
        }

        public override void Update()
        {
            base.Update();
            if (this.reflection == null)
                return;
            this.reflection.Update();
            // Fixed: Removed references to non-existent Hair properties
        }

        private void beforeRender()
        {
            if (this.smashed)
                return;
            Level scene = this.Scene as Level;
            global::Celeste.Player entity = this.Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (entity == null)
                return;
            if (this.autoUpdateReflection && this.reflection != null)
            {
                this.reflection.Position = new Vector2(this.X - entity.X, entity.Y - this.Y) + this.breakingGlass.Origin;
                this.reflectionSprite.Scale.X = (float)-(int)entity.Facing * Math.Abs(entity.Sprite.Scale.X);
                this.reflectionSprite.Scale.Y = entity.Sprite.Scale.Y;
                if (this.reflectionSprite.CurrentAnimationID != entity.Sprite.CurrentAnimationID && entity.Sprite.CurrentAnimationID != null && this.reflectionSprite.Has(entity.Sprite.CurrentAnimationID))
                    this.reflectionSprite.Play(entity.Sprite.CurrentAnimationID);
            }
            if (this.mirror == null)
                this.mirror = VirtualContent.CreateRenderTarget("nightmare-mirror", this.glassbg.Width, this.glassbg.Height);
            Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)this.mirror);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            if (this.updateShine)
                this.shineOffset = (float)(this.glassfg.Height - (int)((double)scene.Camera.Y * 0.800000011920929 % (double)this.glassfg.Height));
            this.glassbg.Draw(Vector2.Zero);
            if (this.reflection != null)
                this.reflection.Render();
            this.glassfg.Draw(new Vector2(0.0f, this.shineOffset), Vector2.Zero, Color.White * this.shineAlpha);
            this.glassfg.Draw(new Vector2(0.0f, this.shineOffset - (float)this.glassfg.Height), Vector2.Zero, Color.White * this.shineAlpha);
            Draw.SpriteBatch.End();
        }

        private IEnumerator interactRoutine()
        {
            RalseiMirror mirror = this;
            global::Celeste.Player player = null;
            while (player == null)
            {
                player = mirror.Scene.Tracker.GetEntity<global::Celeste.Player>();
                yield return null;
            }

            // Null check for hitbox
            if (mirror.hitbox == null)
                yield break;

            while (!mirror.hitbox.Collide((Entity)player))
                yield return null;

            mirror.hitbox.Width += 32f;
            mirror.hitbox.Position.X -= 16f;
            Audio.SetMusic(null);

            while (mirror.hitbox.Collide((Entity)player))
                yield return null;

            // Activate dream blocks when cutscene starts
            mirror.ActivateDreamBlocks();

            mirror.Scene.Add(new Cs04Mirror(player, mirror));
        }

        /// <summary>
        /// Activates all DreamBlock entities in the scene (base game dream blocks)
        /// </summary>
        private void ActivateDreamBlocks()
        {
            var tracker = this.Scene?.Tracker;
            if (tracker == null) return;

            // Try to activate base game DreamBlocks
            List<Entity> blocks = null;
            try
            {
                // Use reflection to find DreamBlock type from Celeste namespace
                var dreamBlockType = typeof(global::Celeste.Player).Assembly.GetType("Celeste.DreamBlock");
                if (dreamBlockType != null)
                {
                    // Get all DreamBlock entities
                    var method = typeof(Tracker).GetMethod("GetEntities", new Type[] { });
                    if (method != null)
                    {
                        var genericMethod = method.MakeGenericMethod(dreamBlockType);
                        blocks = genericMethod.Invoke(tracker, null) as List<Entity>;

                        if (blocks != null)
                        {
                            foreach (var entity in blocks)
                            {
                                // Activate the dream block by setting the player's DreamDash inventory flag
                                var level = this.Scene as Level;
                                if (level?.Session?.Inventory != null)
                                {
                                    level.Session.Inventory.DreamDash = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (KeyNotFoundException)
            {
                // DreamBlock type not tracked
            }
            catch (System.Exception)
            {
                // Handle any reflection errors
            }
        }

        public IEnumerator BreakRoutine(int direction)
        {
            RalseiMirror ralseiMirror = this;
            ralseiMirror.autoUpdateReflection = false;
            ralseiMirror.reflectionSprite.Play("runFast");
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
            while ((double)Math.Abs(ralseiMirror.reflection.X - ralseiMirror.breakingGlass.Width / 2f) > 3.0)
            {
                ralseiMirror.reflection.X += (float)(direction * 32) * Engine.DeltaTime;
                yield return null;
            }
            ralseiMirror.reflectionSprite.Play("idle");
            yield return 0.65f;
            ralseiMirror.Add((Component)(ralseiMirror.sfx = new SoundSource()));
            ralseiMirror.sfx.Play("event:/game/02_old_site/sequence_mirror");
            yield return 0.15f;
            ralseiMirror.Add((Component)(ralseiMirror.sfxSting = new SoundSource("event:/music/pusheen/lvl4/dreamblock_sting_pt2")));
            Input.Rumble(RumbleStrength.Light, RumbleLength.FullSecond);
            ralseiMirror.updateShine = false;
            while ((double)ralseiMirror.shineOffset != 33.0 || (double)ralseiMirror.shineAlpha < 1.0)
            {
                ralseiMirror.shineOffset = Calc.Approach(ralseiMirror.shineOffset, 33f, Engine.DeltaTime * 120f);
                ralseiMirror.shineAlpha = Calc.Approach(ralseiMirror.shineAlpha, 1f, Engine.DeltaTime * 4f);
                yield return null;
            }
            ralseiMirror.smashed = true;
            ralseiMirror.breakingGlass.Play("break");
            yield return 0.6f;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            (ralseiMirror.Scene as Level).Shake();
            for (float x = (float)(-(double)ralseiMirror.breakingGlass.Width / 2.0); (double)x < (double)ralseiMirror.breakingGlass.Width / 2.0; x += 8f)
            {
                for (float y = -ralseiMirror.breakingGlass.Height; (double)y < 0.0; y += 8f)
                {
                    if (Calc.Random.Chance(0.5f))
                        (ralseiMirror.Scene as Level).Particles.Emit(RalseiMirror.PShatter, 2, ralseiMirror.Position + new Vector2(x + 4f, y + 4f), new Vector2(8f, 8f), new Vector2(x, y).Angle());
                }
            }
            ralseiMirror.smashEnded = true;
            ralseiMirror.chara = new RalseiDummy(ralseiMirror.reflection.Position + ralseiMirror.Position - ralseiMirror.breakingGlass.Origin);
            ralseiMirror.chara.Floatness = 0.0f;
            ralseiMirror.Scene.Add((Entity)ralseiMirror.chara);
            ralseiMirror.chara.Sprite.Play("idle");
            ralseiMirror.chara.Sprite.Scale = ralseiMirror.reflectionSprite.Scale;
            ralseiMirror.reflection = null;
            yield return 1.2f;
            float speed = (float)-direction * 32f;
            ralseiMirror.chara.Sprite.Scale.X = (float)-direction;
            ralseiMirror.chara.Sprite.Play("runFast");
            while ((double)Math.Abs(ralseiMirror.chara.X - ralseiMirror.X) < 60.0)
            {
                speed += (float)((double)Engine.DeltaTime * (double)-direction * 128.0);
                ralseiMirror.chara.X += speed * Engine.DeltaTime;
                yield return null;
            }
            ralseiMirror.chara.Sprite.Play("jumpFast");
            while ((double)Math.Abs(ralseiMirror.chara.X - ralseiMirror.X) < 128.0)
            {
                speed += (float)((double)Engine.DeltaTime * (double)-direction * 128.0);
                ralseiMirror.chara.X += speed * Engine.DeltaTime;
                ralseiMirror.chara.Y -= (float)((double)Math.Abs(speed) * (double)Engine.DeltaTime * 0.800000011920929);
                yield return null;
            }
            ralseiMirror.chara.RemoveSelf();
            ralseiMirror.chara = null;
            yield return 1.5f;
        }

        public void Broken(bool wasSkipped)
        {
            this.updateShine = false;
            this.smashed = true;
            this.smashEnded = true;
            this.breakingGlass.Play("broken");
            if (wasSkipped && this.chara != null)
                this.chara.RemoveSelf();
            if (wasSkipped && this.sfx != null)
                this.sfx.Stop();
            if (!wasSkipped || this.sfxSting == null)
                return;
            this.sfxSting.Stop();
        }

        public override void Render()
        {
            if (this.smashed)
            {
                this.breakingGlass.Render();
            }
            else
            {
                if (this.mirror != null && this.mirror.Target != null)
                {
                    Draw.SpriteBatch.Draw((Texture2D)this.mirror.Target, this.Position - this.breakingGlass.Origin, Color.White * this.reflectionAlpha);
                }
            }
            if (this.frame != null)
                // Fix: Use the correct overload of the Render method
                this.frame.Render();
        }

        public override void SceneEnd(Scene scene)
        {
            Unload();
            base.SceneEnd(scene);
        }

        public override void Removed(Scene scene)
        {
            Unload();
            base.Removed(scene);
        }

        private void dispose()
        {
            if (this.mirror != null)
                this.mirror.Dispose();
            this.mirror = null;
        }
        public void Unload()
        {
            // Dispose of the mirror render target if it exists
            if (this.mirror != null)
            {
                this.mirror.Dispose();
                this.mirror = null;
            }
            // Optionally, clean up other resources or state here if needed
        }

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        {
            // You can read custom properties from entityData if needed
            return new RalseiMirror(entityData, offset);
        }
    }
}

