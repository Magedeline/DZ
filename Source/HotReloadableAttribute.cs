using System;

namespace Celeste
{
    /// <summary>
    /// Marks a class or method as hot-reloadable during development.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class HotReloadableAttribute : Attribute
    {
        public HotReloadableAttribute()
        {
        }
    }
}
