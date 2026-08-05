using Celeste.Cutscenes;
using DZ;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Celeste;

[CustomEntity("DZ/FlingBirdIntro")]
[HotReloadable]
public class FlingBirdIntro : Entity
{
    [CompilerGenerated]
    private sealed class _003CFlyTo_003Ed__14 : IEnumerator<object>, IDisposable, IEnumerator
    {
        private int _003C_003E1__state;

        private object _003C_003E2__current;

        public FlingBirdIntro _003C_003E4__this;

        public Vector2 to;

        private Vector2 _003Cfrom_003E5__2;

        private float _003Csine_003E5__3;

        private float _003Cp_003E5__4;

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return _003C_003E2__current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return _003C_003E2__current;
            }
        }

        [DebuggerHidden]
        public _003CFlyTo_003Ed__14(int _003C_003E1__state)
        {
            this._003C_003E1__state = _003C_003E1__state;
        }

        [DebuggerHidden]
        void IDisposable.Dispose()
        {
        }

        private bool MoveNext()
        {
            int num = _003C_003E1__state;
            FlingBirdIntro flingBirdIntro = _003C_003E4__this;
            switch (num)
            {
                default:
                    return false;
                case 0:
                    _003C_003E1__state = -1;
                    flingBirdIntro.Add(new SoundSource().Play("event:/new_content/game/10_farewell/bird_flappyscene_entry"));
                    flingBirdIntro.Sprite.Play("fly");
                    _003Cfrom_003E5__2 = flingBirdIntro.Position;
                    _003Cp_003E5__4 = 0f;
                    goto IL_00d5;
                case 1:
                    _003C_003E1__state = -1;
                    _003Cp_003E5__4 += Engine.DeltaTime * 0.3f;
                    goto IL_00d5;
                case 2:
                    {
                        _003C_003E1__state = -1;
                        break;
                    }
                IL_00d5:
                    if (_003Cp_003E5__4 < 1f)
                    {
                        flingBirdIntro.Position = _003Cfrom_003E5__2 + (to - _003Cfrom_003E5__2) * Ease.SineOut(_003Cp_003E5__4);
                        _003C_003E2__current = null;
                        _003C_003E1__state = 1;
                        return true;
                    }
                    flingBirdIntro.Sprite.Play("hover");
                    _003Csine_003E5__3 = 0f;
                    break;
            }
            flingBirdIntro.Position = to + Vector2.UnitY * (float)Math.Sin(_003Csine_003E5__3) * 8f;
            _003Csine_003E5__3 += Engine.DeltaTime * 2f;
            _003C_003E2__current = null;
            _003C_003E1__state = 2;
            return true;
        }

        bool IEnumerator.MoveNext()
        {
            //ILSpy generated this explicit interface implementation from .override directive in MoveNext
            return this.MoveNext();
        }

        [DebuggerHidden]
        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }
    }

    [CompilerGenerated]
    private sealed class _003CDoGrabbingRoutine_003Ed__18 : IEnumerator<object>, IDisposable, IEnumerator
    {
        private int _003C_003E1__state;

        private object _003C_003E2__current;

        public FlingBirdIntro _003C_003E4__this;

        public Player player;

        private Level _003Clevel_003E5__2;

        private float _003Csin_003E5__3;

        private int _003Cindex_003E5__4;

        private SimpleCurve _003Ccurve_003E5__5;

        private float _003Cduration_003E5__6;

        private float _003Cp_003E5__7;

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return _003C_003E2__current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return _003C_003E2__current;
            }
        }

        [DebuggerHidden]
        public _003CDoGrabbingRoutine_003Ed__18(int _003C_003E1__state)
        {
            this._003C_003E1__state = _003C_003E1__state;
        }

        [DebuggerHidden]
        void IDisposable.Dispose()
        {
        }

        private bool MoveNext()
        {
            int num = _003C_003E1__state;
            FlingBirdIntro flingBirdIntro = _003C_003E4__this;
            switch (num)
            {
                default:
                    return false;
                case 0:
                    _003C_003E1__state = -1;
                    _003Clevel_003E5__2 = flingBirdIntro.Scene as Level;
                    flingBirdIntro.inCutscene = true;
                    if (!flingBirdIntro.crashes)
                    {
                        flingBirdIntro.CrashSfxEmitter = SoundEmitter.Play("event:/new_content/game/10_farewell/bird_flappyscene", flingBirdIntro);
                    }
                    else
                    {
                        flingBirdIntro.CrashSfxEmitter = SoundEmitter.Play("event:/new_content/game/10_farewell/bird_crashscene_start", flingBirdIntro);
                    }
                    player.StateMachine.State = 11;
                    player.DummyGravity = false;
                    player.DummyAutoAnimate = false;
                    player.ForceCameraUpdate = true;
                    player.Sprite.Play("jumpSlow_carry");
                    player.Speed = Vector2.Zero;
                    player.Facing = Facings.Right;
                    Celeste.Freeze(0.1f);
                    _003Clevel_003E5__2.Shake();
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
                    flingBirdIntro.emitParticles = true;
                    flingBirdIntro.Add(new Coroutine(_003Clevel_003E5__2.ZoomTo(new Vector2(140f, 120f), 1.5f, 4f)));
                    _003Csin_003E5__3 = 0f;
                    _003Cindex_003E5__4 = 0;
                    goto IL_045f;
                case 1:
                    _003C_003E1__state = -1;
                    _003Cp_003E5__7 += Engine.DeltaTime / _003Cduration_003E5__6;
                    goto IL_041a;
                case 2:
                    {
                        _003C_003E1__state = -1;
                        _003Clevel_003E5__2.Shake();
                        Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                        _003Clevel_003E5__2.Flash(Color.White);
                        flingBirdIntro.emitParticles = false;
                        flingBirdIntro.inCutscene = false;
                        return false;
                    }
                IL_041a:
                    if (_003Cp_003E5__7 < 1f)
                    {
                        _003Csin_003E5__3 += Engine.DeltaTime * 10f;
                        flingBirdIntro.Position = (_003Ccurve_003E5__5.GetPoint(_003Cp_003E5__7) + Vector2.UnitY * (float)Math.Sin(_003Csin_003E5__3) * 8f).Floor();
                        player.Position = flingBirdIntro.Position + new Vector2(2f, 10f);
                        switch (flingBirdIntro.Sprite.CurrentAnimationFrame)
                        {
                            case 1:
                                player.Position += new Vector2(1f, -1f);
                                break;
                            case 2:
                                player.Position += new Vector2(-1f, 0f);
                                break;
                            case 3:
                                player.Position += new Vector2(-1f, 1f);
                                break;
                            case 4:
                                player.Position += new Vector2(1f, 3f);
                                break;
                            case 5:
                                player.Position += new Vector2(2f, 5f);
                                break;
                        }
                        _003C_003E2__current = null;
                        _003C_003E1__state = 1;
                        return true;
                    }
                    _003Clevel_003E5__2.Shake();
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                    _003Cindex_003E5__4++;
                    _003Ccurve_003E5__5 = default(SimpleCurve);
                    goto IL_045f;
                IL_045f:
                    if (_003Cindex_003E5__4 < flingBirdIntro.nodes.Length - 1)
                    {
                        Vector2 position = flingBirdIntro.Position;
                        Vector2 vector = flingBirdIntro.nodes[_003Cindex_003E5__4];
                        _003Ccurve_003E5__5 = new SimpleCurve(position, vector, position + (vector - position) * 0.5f + new Vector2(0f, -24f));
                        float lengthParametric = _003Ccurve_003E5__5.GetLengthParametric(32);
                        _003Cduration_003E5__6 = lengthParametric / 100f;
                        if (vector.Y < position.Y)
                        {
                            _003Cduration_003E5__6 *= 1.1f;
                            flingBirdIntro.Sprite.Rate = 2f;
                        }
                        else
                        {
                            _003Cduration_003E5__6 *= 0.8f;
                            flingBirdIntro.Sprite.Rate = 1f;
                        }
                        if (!flingBirdIntro.crashes)
                        {
                            if (_003Cindex_003E5__4 == 0)
                            {
                                _003Cduration_003E5__6 = 0.7f;
                            }
                            if (_003Cindex_003E5__4 == 1)
                            {
                                _003Cduration_003E5__6 += 0.191f;
                            }
                            if (_003Cindex_003E5__4 == 2)
                            {
                                _003Cduration_003E5__6 += 0.191f;
                            }
                        }
                        _003Cp_003E5__7 = 0f;
                        goto IL_041a;
                    }
                    flingBirdIntro.Sprite.Rate = 1f;
                    Celeste.Freeze(0.05f);
                    _003C_003E2__current = null;
                    _003C_003E1__state = 2;
                    return true;
            }
        }

        bool IEnumerator.MoveNext()
        {
            //ILSpy generated this explicit interface implementation from .override directive in MoveNext
            return this.MoveNext();
        }

        [DebuggerHidden]
        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }
    }

    public Vector2 BirdEndPosition;

    public Sprite Sprite;

    public SoundEmitter CrashSfxEmitter;

    private Vector2[] nodes;

    private bool startedRoutine;

    private Vector2 start;

    private InvisibleBarrier fakeRightWall;

    private bool crashes;

    private Coroutine flyToRoutine;

    private bool emitParticles;

    private bool inCutscene;

    public FlingBirdIntro(Vector2 position, Vector2[] nodes, bool crashes)
        : base(position)
    {
        this.crashes = crashes;
        Add(Sprite = GFX.SpriteBank.Create("bird"));
        Sprite.Play(crashes ? "hoverStressed" : "hover");
        Sprite.Scale.X = ((!crashes) ? 1 : (-1));
        Sprite.OnFrameChange = delegate
        {
            if (!inCutscene)
            {
                BirdNPC.FlapSfxCheck(Sprite);
            }
        };
        base.Collider = new Circle(16f, 0f, -8f);
        Add(new PlayerCollider(OnPlayer));
        this.nodes = nodes;
        start = position;
        BirdEndPosition = nodes[nodes.Length - 1];
    }

    public FlingBirdIntro(EntityData data, Vector2 levelOffset)
        : this(data.Position + levelOffset, data.NodesOffset(levelOffset), data.Bool("crashes"))
    {
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (!crashes && (scene as Level).Session.GetFlag("MissTheBirdMod"))
        {
            RemoveSelf();
            return;
        }
        Player entity = base.Scene.Tracker.GetEntity<Player>();
        if (entity != null && entity.X > base.X)
        {
            if (crashes)
            {
                CS19_KillTheBird.HandlePostCutsceneSpawn(this, scene as Level);
            }
            TapeBlockManager entity2 = base.Scene.Tracker.GetEntity<TapeBlockManager>();
            if (entity2 != null)
            {
                entity2.StopBlocks();
                entity2.Finish();
            }
            RemoveSelf();
        }
        else
        {
            scene.Add(fakeRightWall = new InvisibleBarrier(new Vector2(base.X + 160f, base.Y - 200f), 8f, 400f));
        }
        if (!crashes)
        {
            Vector2 position = Position;
            Position = new Vector2(base.X - 150f, (scene as Level).Bounds.Top - 8);
            Add(flyToRoutine = new Coroutine(FlyTo(position)));
        }
    }

    [IteratorStateMachine(typeof(_003CFlyTo_003Ed__14))]
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

    public override void Removed(Scene scene)
    {
        if (fakeRightWall != null)
        {
            fakeRightWall.RemoveSelf();
        }
        fakeRightWall = null;
        base.Removed(scene);
    }

    private void OnPlayer(Player player)
    {
        if (player.Dead || startedRoutine)
        {
            return;
        }
        if (flyToRoutine != null)
        {
            flyToRoutine.RemoveSelf();
        }
        startedRoutine = true;
        player.Speed = Vector2.Zero;
        base.Depth = player.Depth - 5;
        Sprite.Play("hoverStressed");
        Sprite.Scale.X = 1f;
        fakeRightWall.RemoveSelf();
        fakeRightWall = null;
        if (!crashes)
        {
            base.Scene.Add(new CS19_MissTheBird(player, this));
            return;
        }
        TapeBlockManager entity = base.Scene.Tracker.GetEntity<TapeBlockManager>();
        if (entity != null)
        {
            entity.StopBlocks();
            entity.Finish();
        }
        base.Scene.Add(new CS19_KillTheBird(player, this));
    }

    public override void Update()
    {
        if (!startedRoutine && fakeRightWall != null)
        {
            Level level = base.Scene as Level;
            if (level.Camera.X > fakeRightWall.X - 320f - 16f)
            {
                level.Camera.X = fakeRightWall.X - 320f - 16f;
            }
        }
        if (emitParticles && base.Scene.OnInterval(0.1f))
        {
            SceneAs<Level>().ParticlesBG.Emit(FlingBird.P_Feather, 1, Position + new Vector2(0f, -8f), new Vector2(6f, 4f));
        }
        base.Update();
    }

    [IteratorStateMachine(typeof(_003CDoGrabbingRoutine_003Ed__18))]
    public IEnumerator DoGrabbingRoutine(Player player)
    {
        Level level = base.Scene as Level;
        inCutscene = true;
        if (!crashes)
        {
            CrashSfxEmitter = SoundEmitter.Play("event:/DZ/new_content/game/19_spaces/bird_flappyscene", this);
        }
        else
        {
            CrashSfxEmitter = SoundEmitter.Play("event:/DZ/new_content/game/19_spaces/bird_crashscene_start", this);
        }
        player.StateMachine.State = 11;
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
            float lengthParametric = curve.GetLengthParametric(32);
            float duration = lengthParametric / 100f;
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
                if (index == 0)
                {
                    duration = 0.7f;
                }
                if (index == 1)
                {
                    duration += 0.191f;
                }
                if (index == 2)
                {
                    duration += 0.191f;
                }
            }
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
            {
                sin += Engine.DeltaTime * 10f;
                Position = (curve.GetPoint(p) + Vector2.UnitY * (float)Math.Sin(sin) * 8f).Floor();
                player.Position = Position + new Vector2(2f, 10f);
                switch (Sprite.CurrentAnimationFrame)
                {
                    case 1:
                        player.Position += new Vector2(1f, -1f);
                        break;
                    case 2:
                        player.Position += new Vector2(-1f, 0f);
                        break;
                    case 3:
                        player.Position += new Vector2(-1f, 1f);
                        break;
                    case 4:
                        player.Position += new Vector2(1f, 3f);
                        break;
                    case 5:
                        player.Position += new Vector2(2f, 5f);
                        break;
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
        emitParticles = false;
        inCutscene = false;
    }
}