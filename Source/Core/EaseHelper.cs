using System;

namespace DZ.Core;

/// <summary>
/// Stub easing helper for the standalone fangame code.
/// Provides a simple linear/sine interpolation; full Celeste easing is not needed
/// since these entities are not instantiated at runtime in the DZ mod.
/// </summary>
public static class EaseHelper
{
    public static float Ease(DZ.Nez.EaseType type, float elapsed, float duration)
    {
        if (duration <= 0f) return 1f;
        float t = Math.Clamp(elapsed / duration, 0f, 1f);
        return type switch
        {
            DZ.Nez.EaseType.Linear      => t,
            DZ.Nez.EaseType.SineIn      => 1f - MathF.Cos(t * MathF.PI * 0.5f),
            DZ.Nez.EaseType.SineOut     => MathF.Sin(t * MathF.PI * 0.5f),
            DZ.Nez.EaseType.SineInOut   => 0.5f * (1f - MathF.Cos(t * MathF.PI)),
            DZ.Nez.EaseType.QuadIn      => t * t,
            DZ.Nez.EaseType.QuadOut     => 1f - (1f - t) * (1f - t),
            DZ.Nez.EaseType.QuadInOut   => t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) * 0.5f,
            DZ.Nez.EaseType.CubicIn     => t * t * t,
            DZ.Nez.EaseType.CubicOut    => 1f - MathF.Pow(1f - t, 3f),
            DZ.Nez.EaseType.CubicInOut  => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) * 0.5f,
            DZ.Nez.EaseType.ExpoIn      => t == 0f ? 0f : MathF.Pow(2f, 10f * t - 10f),
            DZ.Nez.EaseType.ExpoOut     => t == 1f ? 1f : 1f - MathF.Pow(2f, -10f * t),
            DZ.Nez.EaseType.ExpoInOut   => t == 0f ? 0f : t == 1f ? 1f : t < 0.5f ? MathF.Pow(2f, 20f * t - 10f) * 0.5f : (2f - MathF.Pow(2f, -20f * t + 10f)) * 0.5f,
            DZ.Nez.EaseType.BackIn      => 2.70158f * t * t * t - 1.70158f * t * t,
            DZ.Nez.EaseType.BackOut     => 1f + 2.70158f * MathF.Pow(t - 1f, 3f) + 1.70158f * MathF.Pow(t - 1f, 2f),
            DZ.Nez.EaseType.BackInOut   => t < 0.5f ? MathF.Pow(2f * t, 2f) * (7.5325f * 2f * t - 6.5325f) * 0.5f : (MathF.Pow(2f * t - 2f, 2f) * (7.5325f * (2f * t - 2f) + 6.5325f) + 2f) * 0.5f,
            _ => t,
        };
    }
}
