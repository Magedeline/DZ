namespace Celeste.Effects.ShaderEffects
{
    /// <summary>
    /// Lightweight pool for VirtualRenderTarget to minimize allocations for multi-pass effects.
    /// Tracks approximate GPU memory usage and enforces a soft budget.
    /// </summary>
    public static class RenderTargetPool
    {
        private struct Key : IEquatable<Key>
        {
            public readonly int Width;
            public readonly int Height;
            public readonly SurfaceFormat Format;
            public Key(int w, int h, SurfaceFormat f) { Width = w; Height = h; Format = f; }
            public bool Equals(Key other) => Width == other.Width && Height == other.Height && Format == other.Format;
            public override bool Equals(object obj) => obj is Key k && Equals(k);
            public override int GetHashCode() => (Width * 397) ^ Height ^ (int)Format;
            public override string ToString() => string.Format("{0}x{1} {2}", Width, Height, Format);
        }

        private static readonly Dictionary<Key, Stack<VirtualRenderTarget>> pool = new Dictionary<Key, Stack<VirtualRenderTarget>>();
        private static readonly HashSet<VirtualRenderTarget> checkedOut = new HashSet<VirtualRenderTarget>();
        private static long totalAllocatedBytes = 0;
        private const long SoftBudgetBytes = 16L * 1024L * 1024L; // 16MB budget
        private static bool warnedBudget = false;

        public static VirtualRenderTarget Get(int width, int height, SurfaceFormat format = SurfaceFormat.Color)
        {
            var key = new Key(width, height, format);
            if (!pool.TryGetValue(key, out var stack) || stack.Count == 0)
            {
                // Create new render target
                string name = string.Format("rtp_{0}_{1}", key, Guid.NewGuid().ToString("N").Substring(0, 6));
                var rt = VirtualContent.CreateRenderTarget(name, width, height, false, true, 0);
                checkedOut.Add(rt);
                totalAllocatedBytes += EstimateBytes(width, height, format);
                CheckBudget();
                return rt;
            }

            var target = stack.Pop();
            checkedOut.Add(target);
            return target;
        }

        public static void Return(VirtualRenderTarget target)
        {
            if (target == null) return;
            if (!checkedOut.Remove(target)) return; // Already returned or unknown

            var key = new Key(target.Width, target.Height, SurfaceFormat.Color);
            if (!pool.TryGetValue(key, out var stack))
            {
                stack = new Stack<VirtualRenderTarget>();
                pool[key] = stack;
            }
            stack.Push(target);
        }

        public static void Reset()
        {
            // Dispose everything
            foreach (var kv in pool)
            {
                foreach (var rt in kv.Value)
                {
                    try { rt.Dispose(); } catch (Exception ex) { Logger.Log(LogLevel.Warn, "RenderTargetPool", $"Failed to dispose render target: {ex.Message}"); }
                }
            }

            foreach (var rt in checkedOut)
            {
                try { rt.Dispose(); } catch (Exception ex) { Logger.Log(LogLevel.Warn, "RenderTargetPool", $"Failed to dispose render target: {ex.Message}"); }
            }

            pool.Clear();
            checkedOut.Clear();
            totalAllocatedBytes = 0;
            warnedBudget = false;
        }

        private static void CheckBudget()
        {
            if (!warnedBudget && totalAllocatedBytes > SoftBudgetBytes)
            {
                warnedBudget = true;
                IngesteLogger.Warn(string.Format("RenderTargetPool exceeded soft budget: {0:0.0} MB > {1} MB", totalAllocatedBytes / 1048576.0, SoftBudgetBytes / 1048576));
            }
        }

        private static long EstimateBytes(int w, int h, SurfaceFormat format)
        {
            int bpp = 4; // default for Color
            switch (format)
            {
                case SurfaceFormat.Color: bpp = 4; break;
                case SurfaceFormat.Bgra5551:
                case SurfaceFormat.Bgr565:
                case SurfaceFormat.Bgra4444: bpp = 2; break;
                case SurfaceFormat.Alpha8: bpp = 1; break;
                default: bpp = 4; break;
            }
            return (long)w * h * bpp;
        }

        public static string GetStats()
        {
            return string.Format("Pool keys: {0}, CheckedOut: {1}, Approx GPU: {2:0.0} MB", pool.Count, checkedOut.Count, totalAllocatedBytes / 1048576.0);
        }
    }
}




