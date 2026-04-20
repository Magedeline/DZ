namespace Celeste.Effects.ShaderEffects
{
    /// <summary>
    /// Simple per-frame LOD and budget manager for shader effects.
    /// Limits how many background/entity effects render and skips distant ones.
    /// </summary>
    public static class LODManager
    {
        public enum EffectCategory { Background, Entity }

        public static int MaxBackground = 5;  // recommended 3-5
        public static int MaxEntity = 15;     // recommended 10-15

        private static int frame = -1;
        private static int usedBackground = 0;
        private static int usedEntity = 0;

        public static bool ShouldRender(EffectCategory category, Vector2? worldPosition, Level level)
        {
            int f = (int)Engine.FrameCounter;
            if (f != frame)
            {
                frame = f;
                usedBackground = 0;
                usedEntity = 0;
            }

            // Distance gating: skip very distant effects to save fillrate
            if (worldPosition.HasValue && level != null)
            {
                Vector2 camCenter = level.Camera.Position + new Vector2(Engine.Width, Engine.Height) * 0.5f;
                float dist = Vector2.Distance(worldPosition.Value, camCenter);
                // Skip when far away from camera (threshold ~ 1.5x screen diagonal)
                float diag = (float)System.Math.Sqrt(Engine.Width * Engine.Width + Engine.Height * Engine.Height);
                if (dist > diag * 1.5f)
                    return false;
            }

            switch (category)
            {
                case EffectCategory.Background:
                    if (usedBackground >= MaxBackground)
                        return false;
                    usedBackground++;
                    return true;
                case EffectCategory.Entity:
                    if (usedEntity >= MaxEntity)
                        return false;
                    usedEntity++;
                    return true;
                default:
                    return true;
            }
        }
    }
}




