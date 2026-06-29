using System;
using System.Runtime.InteropServices;

namespace DZ.Core
{
    /// <summary>FMOD Studio API compatibility extensions.
    /// Bridges method names from the local FMOD bindings to Celeste's FMOD bindings.
    /// Placed in the same namespace as AudioManager so extension methods are in scope.</summary>
    public static class FmodStudioCompat
    {
        /// <summary>Checks whether the instance has a valid handle.</summary>
        public static bool hasHandle(this FMOD.Studio.EventInstance instance)
            => instance.isValid();

        /// <summary>Sets a parameter by name (maps to Celeste's setParameterValue).</summary>
        public static FMOD.RESULT setParameterByName(this FMOD.Studio.EventInstance instance, string name, float value)
            => instance.setParameterValue(name, value);

        /// <summary>Gets the core/low-level system (maps to Celeste's getLowLevelSystem).</summary>
        public static FMOD.RESULT getCoreSystem(this FMOD.Studio.System system, out FMOD.System coreSystem)
            => system.getLowLevelSystem(out coreSystem);
    }
}
