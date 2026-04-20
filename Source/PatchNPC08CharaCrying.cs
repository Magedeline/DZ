#nullable disable
using System.Diagnostics;

namespace Celeste
{
    /// <summary>
    /// Modifies the method to remove the hardcoded FMOD event string used for playing the footstep sound effect.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal class PatchNpcSetupCharaSpriteSoundsAttribute : Attribute
    {
        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}



