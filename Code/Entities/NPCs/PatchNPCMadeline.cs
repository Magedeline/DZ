#nullable disable
using System.Diagnostics;

namespace Celeste
{
    /// <summary>
    /// Modifies the method to remove the hardcoded FMOD event string used for playing the footstep sound effect.
    /// </summary>
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal class PatchNpcSetupMadelineSpriteSoundsAttribute : Attribute
    {
        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
