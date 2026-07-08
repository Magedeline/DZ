#nullable enable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

// Nez -> Monocle/Celeste compatibility shim.
//
// The DZ Celeste mod does not reference the Nez framework.  The standalone
// fangame sources under the DZ.* namespaces were originally
// written against Nez (Entity, Component, Scene, Collider, ...).
// To compile them inside the DZ project without adding a Nez assembly reference,
// the small subset of the Nez API surface they actually use is provided here as
// self-contained types in the DZ.Nez namespace.  They are independent of Monocle's
// own Entity/Scene/Component types (which live in the Monocle namespace), so the
// two hierarchies never collide: ported files use `using DZ.Nez;`, DZ files use
// `using Monocle;`, and no source file imports both.
//
// These shim types are intentionally minimal.  The standalone entities are not
// instantiated by the DZ mod at runtime; they only need to compile.  Physics
// queries therefore return empty results and render calls are no-ops.

namespace DZ.Nez
{
    // -------------------------------------------------------------------------
    // RectangleF
    // -------------------------------------------------------------------------

    /// <summary>Float-precision axis-aligned rectangle (Nez API parity).</summary>
    public struct RectangleF
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public static readonly RectangleF Empty = new(0f, 0f, 0f, 0f);

        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float Left => X;
        public float Right => X + Width;
        public float Top => Y;
        public float Bottom => Y + Height;
        public Vector2 Center => new(X + Width * 0.5f, Y + Height * 0.5f);
        public Vector2 Location
        {
            get => new(X, Y);
            set { X = value.X; Y = value.Y; }
        }
        public Vector2 Size
        {
            get => new(Width, Height);
            set { Width = value.X; Height = value.Y; }
        }

        public bool Intersects(RectangleF other)
            => !(other.Left >= Right || other.Right <= Left ||
                 other.Top >= Bottom || other.Bottom <= Top);

        public bool Contains(Vector2 point)
            => point.X >= Left && point.X < Right && point.Y >= Top && point.Y < Bottom;

        public bool Contains(float x, float y)
            => x >= Left && x < Right && y >= Top && y < Bottom;

        public static implicit operator RectangleF(Rectangle r)
            => new(r.X, r.Y, r.Width, r.Height);

        public static explicit operator Rectangle(RectangleF r)
            => new((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);

        public static RectangleF FromCenter(float cx, float cy, float w, float h)
            => new(cx - w * 0.5f, cy - h * 0.5f, w, h);

        public override string ToString() => $"{{X:{X} Y:{Y} W:{Width} H:{Height}}}";
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    /// <summary>Implemented by entities/components that participate in update loops.</summary>
    public interface IUpdatable
    {
        bool Enabled { get; }
        int UpdateOrder { get; }
        void Update();
    }

    // -------------------------------------------------------------------------
    // Entity
    // -------------------------------------------------------------------------

    /// <summary>Base scene object (Nez API parity, independent of Monocle.Entity).</summary>
    public class Entity
    {
        private readonly List<Component> _components = new();
        private Scene? _scene;

        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public int UpdateOrder { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Tag { get; set; }
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;

        public Scene? Scene
        {
            get => _scene;
            internal set => _scene = value;
        }

        public Entity() { }
        public Entity(string name) { Name = name; }
        public Entity(Vector2 position) { Position = position; }
        public Entity(string name, Vector2 position) { Name = name; Position = position; }

        public virtual void OnAddedToScene() { }
        public virtual void OnRemovedFromScene() { }
        public virtual void Update() { }
        public virtual void Render() { }

        public T AddComponent<T>(T component) where T : Component
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));
            component.Entity = this;
            _components.Add(component);
            component.OnAddedToEntity();
            if (_scene != null)
                component.OnAddedToScene();
            return component;
        }

        public T? GetComponent<T>() where T : class
        {
            for (int i = 0; i < _components.Count; i++)
                if (_components[i] is T t)
                    return t;
            return null;
        }

        public IEnumerable<T> GetComponents<T>() where T : class
        {
            for (int i = 0; i < _components.Count; i++)
                if (_components[i] is T t)
                    yield return t;
        }

        public void RemoveComponent(Component? component)
        {
            if (component == null) return;
            if (_components.Remove(component))
            {
                component.OnRemovedFromScene();
                component.OnRemovedFromEntity();
                component.Entity = null;
            }
        }

        public void RemoveAllComponents()
        {
            while (_components.Count > 0)
                RemoveComponent(_components[0]);
        }

        /// <summary>Removes this entity from its scene (Entity.Destroy parity).</summary>
        public void Destroy()
        {
            if (_scene != null)
            {
                _scene.Entities.Remove(this);
                _scene = null;
            }
        }

        internal void NotifyAddedToScene()
        {
            OnAddedToScene();
            for (int i = 0; i < _components.Count; i++)
                _components[i].OnAddedToScene();
        }

        internal void NotifyRemovedFromScene()
        {
            for (int i = 0; i < _components.Count; i++)
                _components[i].OnRemovedFromScene();
            OnRemovedFromScene();
        }
    }

    // -------------------------------------------------------------------------
    // Component
    // -------------------------------------------------------------------------

    /// <summary>Base component type (Nez API parity, independent of Monocle.Component).</summary>
    public class Component
    {
        public Entity? Entity { get; set; }
        public Scene? Scene => Entity?.Scene;
        public bool Enabled { get; set; } = true;
        public int UpdateOrder { get; set; }

        public virtual void OnAddedToEntity() { }
        public virtual void OnRemovedFromEntity() { }
        public virtual void OnAddedToScene() { }
        public virtual void OnRemovedFromScene() { }
        public virtual void Update() { }
        public virtual void Render() { }

        public void SetEnabled(bool enabled) => Enabled = enabled;

        public T? GetComponent<T>() where T : class => Entity?.GetComponent<T>();

        public IEnumerable<T> GetComponents<T>() where T : class
            => Entity?.GetComponents<T>() ?? System.Linq.Enumerable.Empty<T>();

        /// <summary>Detaches this component from its entity (Nez parity).</summary>
        public void Destroy() => Entity?.RemoveComponent(this);
    }

    // -------------------------------------------------------------------------
    // Scene / EntityList
    // -------------------------------------------------------------------------

    public class Scene
    {
        public EntityList Entities { get; } = new EntityList();
        public Camera Camera { get; set; } = new Camera();
        public float TimeActive { get; set; }
        public bool TimeActiveFlag { get; set; } = true;
        public Monocle.Tracker Tracker { get; set; } = new Monocle.Tracker();

        public bool TimeActive_get => TimeActive > 0f;

        public List<Entity> FindEntitiesWithTag(int tag)
        {
            var result = new List<Entity>();
            var entities = Entities;
            for (int i = 0; i < entities.Count; i++)
            {
                var e = entities[i];
                if (e.Tag == tag)
                    result.Add(e);
            }
            return result;
        }

        public void Add(Entity entity) => Entities.Add(entity);
        public void Remove(Entity entity) => Entities.Remove(entity);

        public Entity AddEntity(Entity entity) { Entities.Add(entity); entity.Scene = this; return entity; }
        public Entity CreateEntity(string name) { var e = new Entity(name); AddEntity(e); return e; }
        public Entity CreateEntity(string name, Vector2 position) { var e = new Entity(name, position); AddEntity(e); return e; }

        public Entity? FindEntity(string name)
        {
            for (int i = 0; i < Entities.Count; i++)
                if (Entities[i].Name == name)
                    return Entities[i];
            return null;
        }

        public T? FindEntityOfType<T>() where T : Entity
        {
            for (int i = 0; i < Entities.Count; i++)
                if (Entities[i] is T t)
                    return t;
            return null;
        }

        public List<T> EntitiesOfType<T>() where T : Entity
        {
            var result = new List<T>();
            for (int i = 0; i < Entities.Count; i++)
                if (Entities[i] is T t)
                    result.Add(t);
            return result;
        }

        public List<T> FindComponentsOfType<T>() where T : Component
        {
            var result = new List<T>();
            foreach (var e in Entities)
            {
                var comps = e.GetComponents<Component>();
                foreach (var c in comps)
                    if (c is T t)
                        result.Add(t);
            }
            return result;
        }
    }

    public class EntityList : IEnumerable<Entity>
    {
        private readonly List<Entity> _list = new();

        public int Count => _list.Count;
        public Entity this[int index] => _list[index];

        public void Add(Entity entity)
        {
            if (entity == null) return;
            _list.Add(entity);
        }

        public bool Remove(Entity entity)
        {
            if (entity == null) return false;
            bool removed = _list.Remove(entity);
            if (removed) entity.Scene = null;
            return removed;
        }

        public void Clear()
        {
            for (int i = 0; i < _list.Count; i++)
                _list[i].Scene = null;
            _list.Clear();
        }

        public IEnumerable<Entity> EntitiesWithTag(int tag)
        {
            for (int i = 0; i < _list.Count; i++)
                if (_list[i].Tag == tag)
                    yield return _list[i];
        }

        public IEnumerator<Entity> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    }

    // -------------------------------------------------------------------------
    // Collider / BoxCollider / CircleCollider
    // -------------------------------------------------------------------------

    public class Collider : Component
    {
        public Vector2 LocalOffset { get; set; }
        public virtual float Width { get; set; }
        public virtual float Height { get; set; }
        public int PhysicsLayer { get; set; }
        public bool IsTrigger { get; set; }

        /// <summary>Local-space bounding rectangle (offset-based, Nez parity).</summary>
        public virtual RectangleF Bounds => new(LocalOffset.X, LocalOffset.Y, Width, Height);

        /// <summary>World-space bounds (entity position + local offset).</summary>
        public RectangleF AbsoluteBounds
        {
            get
            {
                var p = Entity?.Position ?? Vector2.Zero;
                return new RectangleF(p.X + LocalOffset.X, p.Y + LocalOffset.Y, Width, Height);
            }
        }

        public virtual bool Overlaps(Collider? other)
            => other != null && AbsoluteBounds.Intersects(other.AbsoluteBounds);

        public void SetLocalOffset(Vector2 offset) => LocalOffset = offset;
    }

    public class BoxCollider : Collider
    {
        public BoxCollider() { }
        public BoxCollider(float width, float height) { Width = width; Height = height; }
        public BoxCollider(float x, float y, float width, float height)
        {
            LocalOffset = new Vector2(x, y);
            Width = width;
            Height = height;
        }
    }

    public class CircleCollider : Collider
    {
        public float Radius { get; set; }
        public CircleCollider(float radius)
        {
            Radius = radius;
            Width = Height = radius * 2f;
        }
    }

    // -------------------------------------------------------------------------
    // Camera
    // -------------------------------------------------------------------------

    public class Camera
    {
        public Vector2 Position { get; set; }
        public RectangleF Bounds { get; set; }
        public Rectangle Viewport { get; set; }
        public float Zoom { get; set; } = 1f;
        public float Rotation { get; set; }
    }

    // -------------------------------------------------------------------------
    // Time
    // -------------------------------------------------------------------------

    public static class Time
    {
        /// <summary>Scaled delta time in seconds (delegates to Monocle.Engine).</summary>
        public static float DeltaTime => Monocle.Engine.DeltaTime;

        /// <summary>Unscaled delta time in seconds.</summary>
        public static float UnscaledDeltaTime => Monocle.Engine.RawDeltaTime;

        /// <summary>Total elapsed scene time in seconds.</summary>
        public static float TotalTime => Monocle.Engine.Scene?.TimeActive ?? 0f;

        /// <summary>Global time scale (maps to Monocle.Engine.TimeRate).</summary>
        public static float TimeScale
        {
            get => Monocle.Engine.TimeRate;
            set => Monocle.Engine.TimeRate = value;
        }
    }

    // -------------------------------------------------------------------------
    // PhysicsLayers
    // -------------------------------------------------------------------------

    public static class PhysicsLayers
    {
        public const int None = 0;
        public const int Actor = 1;
        public const int Solid = 2;
        public const int JumpThru = 4;
        public const int Enemy = 8;
        public const int Player = Actor;
        public const int Trigger = 16;
        public const int All = int.MaxValue;
    }

    // -------------------------------------------------------------------------
    // Physics (stub queries — return empty results)
    // -------------------------------------------------------------------------

    public struct RaycastHit
    {
        public Collider? Collider;
        public Entity? Entity;
        public Vector2 Point;
        public Vector2 Normal;
        public float Fraction;
        public float Distance;
    }

    public static class Physics
    {
        public static int OverlapRectangleAll(ref RectangleF rect, Collider[] results) => 0;
        public static int OverlapCircleAll(Vector2 center, float radius, Collider[] results) => 0;
        public static int OverlapCircleAll(Vector2 center, float radius, Collider[] results, int layerMask) => 0;

        public static RaycastHit Linecast(Vector2 from, Vector2 to) => default;
        public static RaycastHit Linecast(Vector2 from, Vector2 to, int layerMask) => default;

        public static RaycastHit Raycast(Vector2 origin, Vector2 direction, float distance) => default;
    }

    // -------------------------------------------------------------------------
    // Core (global scene accessor)
    // -------------------------------------------------------------------------

    public static class Core
    {
        public static Scene? Scene { get; set; }
        public static Camera? Camera => Scene?.Camera;

        private static readonly List<(float timer, Action<Entity> action)> _scheduled = new();
        private static readonly List<IEnumerator> _coroutines = new();

        /// <summary>Schedules an action to run after <paramref name="delay"/> seconds.</summary>
        public static void Schedule(float delay, Action<Entity> action)
        {
            _scheduled.Add((delay, action));
        }

        /// <summary>Schedules an action to run after <paramref name="delay"/> seconds (no-arg overload).</summary>
        public static void Schedule(float delay, Action action)
        {
            _scheduled.Add((delay, _ => action()));
        }

        /// <summary>Starts a coroutine (stub: advances immediately each call).</summary>
        public static void StartCoroutine(IEnumerator routine)
        {
            _coroutines.Add(routine);
        }

        /// <summary>Advances all scheduled timers and coroutines. Called by the host each frame.</summary>
        public static void Tick(float dt)
        {
            for (int i = _scheduled.Count - 1; i >= 0; i--)
            {
                var s = _scheduled[i];
                s.timer -= dt;
                if (s.timer <= 0f)
                {
                    _scheduled.RemoveAt(i);
                    s.action(null!);
                }
                else
                {
                    _scheduled[i] = s;
                }
            }

            for (int i = _coroutines.Count - 1; i >= 0; i--)
            {
                var c = _coroutines[i];
                if (!c.MoveNext())
                    _coroutines.RemoveAt(i);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Random
    // -------------------------------------------------------------------------

    public static class Random
    {
        private static readonly System.Random _rng = new();

        public static float NextFloat() => (float)_rng.NextDouble();
        public static float NextFloat(float max) => NextFloat() * max;
        public static float NextFloat(float min, float max) => min + NextFloat() * (max - min);
        public static int NextInt() => _rng.Next();
        public static int NextInt(int max) => _rng.Next(max);
        public static int NextInt(int min, int max) => _rng.Next(min, max);
        public static float Range(float min, float max) => min + NextFloat() * (max - min);
        public static int Range(int min, int max) => _rng.Next(min, max);
        public static bool Chance(float probability) => NextFloat() < probability;

        public static T Choose<T>(params T[] items) => items[NextInt(items.Length)];

        public static Vector2 NextUnitVector()
        {
            float angle = NextFloat() * MathF.PI * 2f;
            return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        }

        public static float NextAngle() => NextFloat() * MathF.PI * 2f;
    }

    // -------------------------------------------------------------------------
    // Graphics / Batcher (no-op render stubs)
    // -------------------------------------------------------------------------

    public class Batcher
    {
        public void DrawRect(float x, float y, float w, float h, Color color) { }
        public void DrawRect(RectangleF rect, Color color) { }
        public void DrawRect(Rectangle rect, Color color) { }
        public void DrawLine(Vector2 a, Vector2 b, Color color) { }
        public void DrawLine(float x1, float y1, float x2, float y2, Color color) { }
        public void DrawCircle(Vector2 center, float radius, Color color) { }
        public void DrawTexture(Texture2D texture, Vector2 position, Color color) { }
    }

    public class Graphics
    {
        public static Graphics Instance { get; } = new();
        public Batcher Batcher { get; } = new();
        public static Microsoft.Xna.Framework.Graphics.GraphicsDevice? GraphicsDevice => Monocle.Engine.Graphics.GraphicsDevice;
    }

    // -------------------------------------------------------------------------
    // Input (delegates to Monocle.Input / MInput)
    // -------------------------------------------------------------------------

    public static class Input
    {
        public static bool IsKeyDown(Microsoft.Xna.Framework.Input.Keys key)
            => Monocle.MInput.Keyboard.Check(key);

        public static bool IsKeyPressed(Microsoft.Xna.Framework.Input.Keys key)
            => Monocle.MInput.Keyboard.Pressed(key);

        public static bool IsKeyReleased(Microsoft.Xna.Framework.Input.Keys key)
            => Monocle.MInput.Keyboard.Released(key);
    }

    // -------------------------------------------------------------------------
    // Vector2Ext
    // -------------------------------------------------------------------------

    public static class Vector2Ext
    {
        public static Vector2 Normalize(Vector2 value)
        {
            float len = value.Length();
            if (len > 0f)
                return value / len;
            return Vector2.Zero;
        }

        public static Vector2 Normalize(ref Vector2 value)
        {
            float len = value.Length();
            if (len > 0f)
                return value / len;
            return Vector2.Zero;
        }
    }

    // -------------------------------------------------------------------------
    // EaseType
    // -------------------------------------------------------------------------

    public enum EaseType
    {
        Linear,
        SineIn,
        SineOut,
        SineInOut,
        QuadIn,
        QuadOut,
        QuadInOut,
        CubicIn,
        CubicOut,
        CubicInOut,
        ExpoIn,
        ExpoOut,
        ExpoInOut,
        BackIn,
        BackOut,
        BackInOut,
    }
}

// -----------------------------------------------------------------------------
// DZ.Nez.Sprites
// -----------------------------------------------------------------------------

namespace DZ.Nez.Sprites
{
    public class SpriteRenderer : DZ.Nez.Component
    {
        public Texture2D? Texture { get; set; }
        public Vector2 LocalOffset { get; set; }
        public Vector2 Origin { get; set; } = new(0.5f, 0.5f);
        public Vector2 Scale { get; set; } = Vector2.One;
        public float Rotation { get; set; }
        public Color Color { get; set; } = Color.White;
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
        public int RenderLayer { get; set; }
        public RectangleF SourceRect { get; set; }

        public void SetSprite(Texture2D texture) { Texture = texture; }
    }

    public class SpriteAnimator : DZ.Nez.Component
    {
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
        public Color Color { get; set; } = Color.White;
        public Vector2 Scale { get; set; } = Vector2.One;
        public float Speed { get; set; } = 1f;
        public bool IsPlaying { get; private set; }
        public string? CurrentAnimation { get; private set; }

        public void Play(string name) { CurrentAnimation = name; IsPlaying = true; }
        public void Stop() { IsPlaying = false; }
    }

    public class Sprite : DZ.Nez.Component
    {
        public Texture2D? Texture { get; set; }
        public Vector2 Origin { get; set; } = new(0.5f, 0.5f);
        public Vector2 Scale { get; set; } = Vector2.One;
        public Color Color { get; set; } = Color.White;
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
        public int RenderLayer { get; set; }
        public RectangleF SourceRect { get; set; }

        public Sprite() { }
        public Sprite(Texture2D? texture) { Texture = texture; }
    }
}
