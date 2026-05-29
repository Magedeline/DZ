using System.Collections;
using System.Runtime.CompilerServices;

namespace Celeste.Entities
{
    [CustomEntity(new string[] { "MaggyHelper/FlingBirdIntroCutscene" })]
    [Tracked(false)]
    [HotReloadable]
    public class FlingBirdIntroCutscene : Entity
    {
        public Vector2 BirdEndPosition;

        public Sprite Sprite;

        public SoundEmitter CrashSfxEmitter;

        private Vector2[] nodes;

        private bool startedRoutine;

        private InvisibleBarrier fakeRightWall;

        private bool crashes;

        private Coroutine flyToRoutine;

        private bool emitParticles;

        private bool inCutscene;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public FlingBirdIntroCutscene(Vector2 position, Vector2[] nodes, bool crashes)
            : base(position)
        {
            this.crashes = crashes;
            Add(Sprite = GFX.SpriteBank.Create("bird"));
            Sprite.Play(crashes ? "hoverStressed" : "hover");
            Sprite.Scale.X = crashes ? -1 : 1;
            Sprite.OnFrameChange = (string anim) =>
            {
                if (!inCutscene)
                    BirdNPC.FlapSfxCheck(Sprite);
            };
            base.Collider = new Circle(16f, 0f, -8f);
            Add(new PlayerCollider(OnPlayer));
            this.nodes = nodes;
            BirdEndPosition = nodes[^1];
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public FlingBirdIntroCutscene(EntityData data, Vector2 levelOffset)
            : this(data.Position + levelOffset, data.NodesOffset(levelOffset), data.Bool("crashes"))
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null && entity.X > base.X)
            {
                if (crashes)
                    CS19_BirdIntroCutscene.HandlePostCutsceneSpawn(this, scene as Level);
                RemoveSelf();
                return;
            }
            scene.Add(fakeRightWall = new InvisibleBarrier(new Vector2(base.X + 160f, base.Y - 200f), 8f, 400f));
            if (!crashes)
            {
                Vector2 position = Position;
                Position = new Vector2(base.X - 150f, (scene as Level).Bounds.Top - 8);
                Add(flyToRoutine = new Coroutine(FlyTo(position)));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator FlyTo(Vector2 to)
        {
            Add(new SoundSource().Play("event:/new_content/game/10_farewell/bird_flappyscene_entry"));
            Sprite.Play("fly");
            Vector2 from = Position;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 0.3f)
            {
                Position = from + (to - from) * Ease.SineOut(p);
                yield return null;
            }
            Sprite.Play("hover");
            float sine = 0f;
            while (true)
            {
                Position = to + Vector2.UnitY * (float)Math.Sin(sine) * 8f;
                sine += Engine.DeltaTime * 2f;
                yield return null;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Removed(Scene scene)
        {
            if (fakeRightWall != null)
                fakeRightWall.RemoveSelf();
            fakeRightWall = null;
            base.Removed(scene);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void OnPlayer(global::Celeste.Player player)
        {
            if (player == null || player.Dead || startedRoutine || base.Scene == null)
                return;
            if (flyToRoutine != null)
                flyToRoutine.RemoveSelf();
            startedRoutine = true;
            base.Depth = player.Depth - 5;
            Sprite.Play("hoverStressed");
            Sprite.Scale.X = 1f;
            if (fakeRightWall != null)
            {
                fakeRightWall.RemoveSelf();
                fakeRightWall = null;
            }
            base.Scene.Add(new CS19_BirdIntroCutscene(player, this, crashes));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update()
        {
            if (!startedRoutine && fakeRightWall != null)
            {
                Level level = base.Scene as Level;
                if (level.Camera.X > fakeRightWall.X - 320f - 16f)
                    level.Camera.X = fakeRightWall.X - 320f - 16f;
            }
            if (emitParticles && base.Scene.OnInterval(0.1f))
                SceneAs<Level>().ParticlesBG.Emit(FlingBird.P_Feather, 1, Position + new Vector2(0f, -8f), new Vector2(6f, 4f));
            base.Update();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerator DoGrabbingRoutine(global::Celeste.Player player)
        {
            Level level = Scene as Level;
            inCutscene = true;
            CrashSfxEmitter = crashes
                ? SoundEmitter.Play("event:/pusheen/final_content/game/19_the_end/killscene_start", this)
                : SoundEmitter.Play("event:/pusheen/final_content/game/19_the_end/flappybird", this);
            player.StateMachine.State = Player.StDummy;
            player.DummyGravity = false;
            player.DummyAutoAnimate = false;
            player.ForceCameraUpdate = true;
            player.Sprite.Play("jumpSlow_carry");
            player.Speed = Vector2.Zero;
            player.Facing = Facings.Right;
            Celeste.Freeze(0.1f);
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
            emitParticles = true;
            Add(new Coroutine(level.ZoomTo(new Vector2(140f, 120f), 1.5f, 4f)));
            float sin = 0f;
            for (int index = 0; index < nodes.Length - 1; index++)
            {
                Vector2 position = Position;
                Vector2 vector = nodes[index];
                SimpleCurve curve = new SimpleCurve(position, vector, position + (vector - position) * 0.5f + new Vector2(0f, -24f));
                float duration = curve.GetLengthParametric(32) / 100f;
                if (vector.Y < position.Y)
                {
                    duration *= 1.1f;
                    Sprite.Rate = 2f;
                }
                else
                {
                    duration *= 0.8f;
                    Sprite.Rate = 1f;
                }
                if (!crashes)
                {
                    if (index == 0) duration = 0.7f;
                    if (index == 1) duration += 0.191f;
                    if (index == 2) duration += 0.191f;
                }
                for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
                {
                    sin += Engine.DeltaTime * 10f;
                    Position = (curve.GetPoint(p) + Vector2.UnitY * (float)Math.Sin(sin) * 8f).Floor();
                    player.Position = Position + new Vector2(2f, 10f);
                    switch (Sprite.CurrentAnimationFrame)
                    {
                        case 1: player.Position += new Vector2(1f, -1f); break;
                        case 2: player.Position += new Vector2(-1f, 0f); break;
                        case 3: player.Position += new Vector2(-1f, 1f); break;
                        case 4: player.Position += new Vector2(1f, 3f); break;
                        case 5: player.Position += new Vector2(2f, 5f); break;
                    }
                    yield return null;
                }
                level.Shake();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            }
            Sprite.Rate = 1f;
            Celeste.Freeze(0.05f);
            yield return null;
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            level.Flash(Color.White);
            Glitch.Value = 0.7f;
            emitParticles = false;
            inCutscene = false;
            yield return null;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime / 0.4f)
            {
                Glitch.Value = 0.7f * (1f - Ease.CubeIn(t));
                yield return null;
            }
            Glitch.Value = 0f;
        }
    }
}
