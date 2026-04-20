namespace Celeste.Effects.ShaderEffects
{
    /// <summary>
    /// Manages loading, caching, and lifecycle of HLSL shader effects.
    /// Provides centralized access to compiled shader effects with automatic disposal.
    /// </summary>
    [HotReloadable]
    public static class EffectManager
    {
        private static Dictionary<string, Effect> loadedEffects = new Dictionary<string, Effect>();
        private static bool initialized = false;
        private static bool warnedMissingEffects = false;

        /// <summary>
        /// Initialize the effect manager. Called during module load.
        /// </summary>
        public static void Initialize()
        {
            if (initialized)
            {
                IngesteLogger.Warn("EffectManager already initialized");
                return;
            }

            IngesteLogger.Info("Initializing EffectManager for HLSL shader effects");
            initialized = true;
        }

        /// <summary>
        /// Load an effect by name from the Graphics/Effects directory.
        /// Effects are cached after first load for performance.
        /// </summary>
        /// <param name="effectName">Name of the effect file (without extension)</param>
        /// <returns>Loaded Effect instance</returns>
        public static Effect LoadEffect(string effectName)
        {
            if (!initialized)
            {
                IngesteLogger.Error("EffectManager not initialized. Call Initialize() first.");
                return null;
            }

            if (loadedEffects.TryGetValue(effectName, out Effect cachedEffect))
            {
                IngesteLogger.Debug($"Using cached effect: {effectName}");
                return cachedEffect;
            }

            try
            {
                // Try loading from mod content directory (expects compiled bytecode under Graphics/Effects/Compiled)
                string effectPath = string.Format("Effects/{0}", effectName);
                IngesteLogger.Debug(string.Format("Loading effect from path: {0}", effectPath));

                Effect effect = null;

                // Attempt to load through Everest mod content system
                if (Everest.Content.TryGet(effectPath, out ModAsset asset))
                {
                    using (var stream = asset.Stream)
                    {
                        byte[] effectCode = new byte[stream.Length];
                        stream.Read(effectCode, 0, effectCode.Length);
                        effect = new Effect(Engine.Graphics.GraphicsDevice, effectCode);
                    }
                }
                else
                {
                    // Fallback: Try vanilla content loading
                    IngesteLogger.Warn(string.Format("Effect {0} not found in mod content, trying vanilla content", effectName));
                    effect = global::Celeste.Celeste.Instance.Content.Load<Effect>(effectPath);
                }

                if (effect != null)
                {
                    loadedEffects[effectName] = effect;
                    IngesteLogger.Info(string.Format("Successfully loaded effect: {0}", effectName));
                    return effect;
                }
                else if (!warnedMissingEffects)
                {
                    warnedMissingEffects = true;
                    IngesteLogger.Warn("No compiled shader effects were found. Did you run Tools/CompileShaders.ps1? Falling back to no-op rendering for effects.");
                }
            }
            catch (Exception ex)
            {
                IngesteLogger.Error(string.Format("Failed to load effect '{0}': {1}", effectName, ex.Message));
                IngesteLogger.Error(string.Format("Stack trace: {0}", ex.StackTrace));
            }

            return null;
        }

        /// <summary>
        /// Check if an effect is loaded and cached.
        /// </summary>
        public static bool IsEffectLoaded(string effectName)
        {
            return loadedEffects.ContainsKey(effectName);
        }

        /// <summary>
        /// Get a loaded effect without triggering a load operation.
        /// </summary>
        public static Effect GetEffect(string effectName)
        {
            loadedEffects.TryGetValue(effectName, out Effect effect);
            return effect;
        }

        /// <summary>
        /// Unload a specific effect and free its resources.
        /// </summary>
        public static void UnloadEffect(string effectName)
        {
            if (loadedEffects.TryGetValue(effectName, out Effect effect))
            {
                if (effect != null)
                {
                    effect.Dispose();
                }
                loadedEffects.Remove(effectName);
                IngesteLogger.Info(string.Format("Unloaded effect: {0}", effectName));
            }
        }

        /// <summary>
        /// Unload all effects and free resources. Called during module unload.
        /// </summary>
        public static void UnloadAll()
        {
            IngesteLogger.Info(string.Format("Unloading {0} cached effects", loadedEffects.Count));

            foreach (var kvp in loadedEffects)
            {
                try
                {
                    if (kvp.Value != null)
                    {
                        kvp.Value.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    IngesteLogger.Error(string.Format("Error disposing effect {0}: {1}", kvp.Key, ex.Message));
                }
            }

            loadedEffects.Clear();
            initialized = false;
            warnedMissingEffects = false;
            // Reset pooled render targets to free VRAM
            try { RenderTargetPool.Reset(); } catch { }
            IngesteLogger.Info("EffectManager unloaded");
        }

        /// <summary>
        /// Get count of currently loaded effects.
        /// </summary>
        public static int LoadedEffectCount => loadedEffects.Count;

        /// <summary>
        /// Get names of all loaded effects.
        /// </summary>
        public static IEnumerable<string> GetLoadedEffectNames()
        {
            return loadedEffects.Keys;
        }
    }
}




