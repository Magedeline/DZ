using System;

namespace DZ
{
    /// <summary>
    /// Stub attribute for [HotReloadable] marking.
    /// The actual hot-reload system has been removed to a separate project.
    /// This attribute is kept for backward compatibility with existing code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HotReloadableAttribute : Attribute
    {
    }
}
