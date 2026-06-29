using System;
using System.Collections.Generic;
using System.Linq;

namespace DZ.Nez;

/// <summary>
/// Extension methods that add missing Celeste/Monocle-style APIs to Nez types.
/// </summary>
public static class NezEntityExtensions
{
    // -------------------------------------------------------------------------
    // EntityList extensions
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the first entity of type <typeparamref name="T"/> in the list, or null.
    /// </summary>
    public static T? FindEntityOfType<T>(this EntityList list) where T : class
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] is T t)
                return t;
        }
        return null;
    }

    /// <summary>
    /// Returns all entities of type <typeparamref name="T"/>.
    /// </summary>
    public static List<T> FindEntitiesOfType<T>(this EntityList list) where T : Entity
    {
        var result = new List<T>();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] is T t)
                result.Add(t);
        }
        return result;
    }

    /// <summary>
    /// Returns entities with the given tag as an IEnumerable so foreach works.
    /// </summary>
    public static IEnumerable<Entity> GetEntitiesWithTag(this EntityList list, int tag)
    {
        var entities = list.EntitiesWithTag(tag);
        foreach (var e in entities)
            yield return e;
    }

    // -------------------------------------------------------------------------
    // Scene extensions
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the first entity of type <typeparamref name="T"/> in the scene, or null.
    /// </summary>
    public static T? FindEntityOfType<T>(this Scene scene) where T : Entity
        => scene.Entities.FindEntityOfType<T>();

    /// <summary>
    /// Returns all entities of type <typeparamref name="T"/> in the scene.
    /// </summary>
    public static List<T> FindEntitiesOfType<T>(this Scene scene) where T : Entity
        => scene.Entities.FindEntitiesOfType<T>();

    /// <summary>
    /// Returns the first component of type <typeparamref name="T"/> in the scene, or null.
    /// </summary>
    public static T? FindComponentOfType<T>(this Scene scene) where T : Component
    {
        for (int i = 0; i < scene.Entities.Count; i++)
        {
            var comp = scene.Entities[i].GetComponent<T>();
            if (comp != null)
                return comp;
        }
        return null;
    }

    /// <summary>
    /// Returns all components of type <typeparamref name="T"/> in the scene.
    /// </summary>
    public static IEnumerable<T> FindComponentsOfType<T>(this Scene scene) where T : Component
    {
        for (int i = 0; i < scene.Entities.Count; i++)
        {
            var comp = scene.Entities[i].GetComponent<T>();
            if (comp != null)
                yield return comp;
        }
    }

    // -------------------------------------------------------------------------
    // Component extensions
    // -------------------------------------------------------------------------

    /// <summary>
    /// Shorthand for <c>Component.Entity.Scene</c>.
    /// </summary>
    public static Scene Scene(this Component component)
        => component.Entity.Scene;

    // -------------------------------------------------------------------------
    // Collider helpers (Monocle API parity)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns true if this collider overlaps the other collider.
    /// Mirrors Monocle's <c>Collider.Collides(Collider)</c>.
    /// </summary>
    public static bool Collides(this Collider self, Collider? other)
    {
        if (other == null) return false;
        return self.Overlaps(other);
    }

    // -------------------------------------------------------------------------
    // Entity removal helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Removes this entity from its scene without destroying it.
    /// </summary>
    public static void RemoveFromScene(this Entity entity)
    {
        if (entity.Scene != null)
            entity.Scene.Entities.Remove(entity);
    }
}
