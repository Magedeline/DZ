using System;

namespace MaggyHelper.HotReload
{
    /// <summary>
    /// Marks an entity class as supporting hot reload during development.
    /// This is a marker attribute with no runtime behavior.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class HotReloadableAttribute : Attribute
    {
    }
}
