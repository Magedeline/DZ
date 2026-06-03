using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.MarkovChain;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.BackgroundGeneration;

/// <summary>
/// Dynamic background generator using Nez effects and Markov chains
/// </summary>
public class DynamicBackgroundGenerator
{
    private const string LogTag = "DynamicBackgroundGenerator";
    
    private readonly MarkovChain<string> _effectChain;
    private readonly MarkovChain<string> _colorChain;
    private readonly MarkovChain<string> _animationChain;
    private readonly Random _random;
    private readonly int _seed;
    
    private readonly Dictionary<string, GeneratedBackground> _generatedBackgrounds;
    private int _backgroundCounter;
    
    public DynamicBackgroundGenerator(int seed = 0)
    {
        _seed = seed;
        _random = seed == 0 ? new Random() : new Random(seed);
        
        _effectChain = new MarkovChain<string>("BackgroundEffects", seed);
        _colorChain = new MarkovChain<string>("BackgroundColors", seed);
        _animationChain = new MarkovChain<string>("BackgroundAnimations", seed);
        
        _generatedBackgrounds = new Dictionary<string, GeneratedBackground>();
        _backgroundCounter = 0;
        
        InitializeChains();
    }
    
    /// <summary>
    /// Initialize Markov chains for background generation
    /// </summary>
    private void InitializeChains()
    {
        var effects = new[]
        {
            "Bloom", "Vignette", "Scanlines", "Grayscale", "Sepia",
            "Noise", "Glitch", "Distortion", "Reflection", "Letterbox",
            "PaletteCycler", "HeatDistortion", "PixelGlitch", "Dissolve"
        };
        
        var colors = new[]
        {
            "Sunset", "Night", "Dawn", "Dusk", "Midnight",
            "Aurora", "Storm", "Cosmic", "Underwater", "Volcanic"
        };
        
        var animations = new[]
        {
            "Parallax", "Pulse", "Wave", "Drift", "Rotate",
            "Zoom", "Shake", "Scroll", "Fade", "Breathe"
        };
        
        // Initialize effect chain
        foreach (var fromEffect in effects)
        {
            foreach (var toEffect in effects)
            {
                float probability = CalculateEffectTransition(fromEffect, toEffect);
                _effectChain.AddTransition(fromEffect, toEffect, probability);
            }
        }
        
        // Initialize color chain
        foreach (var fromColor in colors)
        {
            foreach (var toColor in colors)
            {
                float probability = CalculateColorTransition(fromColor, toColor);
                _colorChain.AddTransition(fromColor, toColor, probability);
            }
        }
        
        // Initialize animation chain
        foreach (var fromAnimation in animations)
        {
            foreach (var toAnimation in animations)
            {
                float probability = CalculateAnimationTransition(fromAnimation, toAnimation);
                _animationChain.AddTransition(fromAnimation, toAnimation, probability);
            }
        }
        
        _effectChain.Initialize("Bloom");
        _colorChain.Initialize("Sunset");
        _animationChain.Initialize("Parallax");
        
        Logger.Log(LogLevel.Info, LogTag, "Initialized background generation chains");
    }
    
    /// <summary>
    /// Calculate effect transition probabilities
    /// </summary>
    private float CalculateEffectTransition(string from, string to)
    {
        // Similar effects often transition
        if ((from.Contains("Glitch") || from.Contains("Distortion")) && 
            (to.Contains("Glitch") || to.Contains("Distortion")))
            return 0.25f;
        
        // Heavy effects are rare
        if (to == "Distortion" || to == "HeatDistortion" || to == "PixelGlitch")
            return 0.08f;
        
        // Common effects like bloom and vignette transition well
        if ((from == "Bloom" || from == "Vignette") && (to == "Bloom" || to == "Vignette"))
            return 0.2f;
        
        // Prevent immediate repetition
        if (from == to)
            return 0.05f;
        
        return 0.12f;
    }
    
    /// <summary>
    /// Calculate color transition probabilities
    /// </summary>
    private float CalculateColorTransition(string from, string to)
    {
        // Time-based color transitions
        if ((from == "Dawn" || from == "Sunset") && (to == "Night" || to == "Dusk"))
            return 0.3f;
        
        // Extreme colors are rare
        if (to == "Cosmic" || to == "Volcanic" || to == "Aurora")
            return 0.08f;
        
        // Natural transitions
        if ((from == "Night" && to == "Dawn") || (from == "Dawn" && to == "Sunset"))
            return 0.25f;
        
        // Prevent immediate repetition
        if (from == to)
            return 0.05f;
        
        return 0.1f;
    }
    
    /// <summary>
    /// Calculate animation transition probabilities
    /// </summary>
    private float CalculateAnimationTransition(string from, string to)
    {
        // Movement animations often transition
        if ((from == "Parallax" || from == "Drift") && (to == "Parallax" || to == "Drift"))
            return 0.25f;
        
        // Intense animations are rare
        if (to == "Shake" || to == "Zoom")
            return 0.08f;
        
        // Subtle animations are common
        if ((from == "Pulse" || from == "Breathe") && (to == "Pulse" || to == "Breathe"))
            return 0.2f;
        
        // Prevent immediate repetition
        if (from == to)
            return 0.05f;
        
        return 0.12f;
    }
    
    /// <summary>
    /// Generate a dynamic background for a room type
    /// </summary>
    public GeneratedBackground GenerateBackground(string roomType, DifficultyTier difficulty)
    {
        var effect = _effectChain.GetNextState() ?? "Bloom";
        var colorScheme = _colorChain.GetNextState() ?? "Sunset";
        var animation = _animationChain.GetNextState() ?? "Parallax";
        
        // Adjust based on room type
        if (roomType == "Boss")
        {
            effect = "Bloom"; // Boss rooms have bloom
            colorScheme = "Cosmic"; // Dramatic colors
            animation = "Pulse"; // Pulsing effect
        }
        
        if (roomType == "Rest")
        {
            effect = "Vignette"; // Calm effect
            colorScheme = "Dawn"; // Peaceful colors
            animation = "Drift"; // Slow movement
        }
        
        if (roomType == "Challenge")
        {
            effect = "Glitch"; // Intense effect
            colorScheme = "Storm"; // Dramatic colors
            animation = "Shake"; // Intense animation
        }
        
        var background = new GeneratedBackground
        {
            Id = $"Background_{_backgroundCounter++}",
            Name = $"{roomType}_Background",
            RoomType = roomType,
            Effect = effect,
            ColorScheme = colorScheme,
            Animation = animation,
            Difficulty = difficulty,
            Layers = GenerateBackgroundLayers(roomType, colorScheme),
            EffectParameters = GenerateEffectParameters(effect, difficulty),
            AnimationParameters = GenerateAnimationParameters(animation, difficulty)
        };
        
        _generatedBackgrounds[background.Id] = background;
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generated background: {background.Name} ({effect}, {colorScheme}, {animation})");
        
        return background;
    }
    
    /// <summary>
    /// Generate background layers
    /// </summary>
    private List<BackgroundLayer> GenerateBackgroundLayers(string roomType, string colorScheme)
    {
        var layers = new List<BackgroundLayer>();
        
        int layerCount = roomType switch
        {
            "Boss" => 4,
            "Challenge" => 3,
            "Cinematic" => 5,
            _ => 2
        };
        
        for (int i = 0; i < layerCount; i++)
        {
            var layer = new BackgroundLayer
            {
                Index = i,
                Depth = i * 0.2f,
                ParallaxFactor = 1f - (i * 0.15f),
                Color = GetLayerColor(colorScheme, i, layerCount),
                Opacity = 1f - (i * 0.2f),
                Texture = GenerateLayerTexture(roomType, i)
            };
            
            layers.Add(layer);
        }
        
        return layers;
    }
    
    /// <summary>
    /// Get layer color based on color scheme
    /// </summary>
    private Color GetLayerColor(string colorScheme, int layerIndex, int totalLayers)
    {
        var baseColor = GetBaseColor(colorScheme);
        
        // Adjust brightness based on layer depth
        float brightness = 1f - (layerIndex / (float)totalLayers) * 0.5f;
        
        return new Color(
            (byte)(baseColor.R * brightness),
            (byte)(baseColor.G * brightness),
            (byte)(baseColor.B * brightness)
        );
    }
    
    /// <summary>
    /// Get base color for color scheme
    /// </summary>
    private Color GetBaseColor(string colorScheme)
    {
        return colorScheme switch
        {
            "Sunset" => new Color(255, 150, 100),
            "Night" => new Color(20, 20, 60),
            "Dawn" => new Color(255, 200, 150),
            "Dusk" => new Color(200, 100, 150),
            "Midnight" => new Color(10, 10, 40),
            "Aurora" => new Color(100, 255, 200),
            "Storm" => new Color(80, 80, 120),
            "Cosmic" => new Color(100, 50, 150),
            "Underwater" => new Color(50, 100, 150),
            "Volcanic" => new Color(200, 80, 50),
            _ => Color.White
        };
    }
    
    /// <summary>
    /// Generate layer texture name
    /// </summary>
    private string GenerateLayerTexture(string roomType, int layerIndex)
    {
        return roomType switch
        {
            "Boss" => layerIndex == 0 ? "boss_bg_near" : $"boss_bg_{layerIndex}",
            "Platforming" => layerIndex == 0 ? "platforming_bg_near" : $"platforming_bg_{layerIndex}",
            "Combat" => layerIndex == 0 ? "combat_bg_near" : $"combat_bg_{layerIndex}",
            _ => $"generic_bg_{layerIndex}"
        };
    }
    
    /// <summary>
    /// Generate effect parameters
    /// </summary>
    private EffectParameters GenerateEffectParameters(string effect, DifficultyTier difficulty)
    {
        float intensity = difficulty switch
        {
            DifficultyTier.Easy => 0.5f,
            DifficultyTier.Normal => 0.7f,
            DifficultyTier.Hard => 1.0f,
            DifficultyTier.Expert => 1.3f,
            DifficultyTier.Master => 1.6f,
            _ => 1.0f
        };
        
        return effect switch
        {
            "Bloom" => new EffectParameters
            {
                BloomThreshold = 0.5f,
                BloomIntensity = 0.8f * intensity,
                BloomBlurAmount = 4f
            },
            "Vignette" => new EffectParameters
            {
                VignetteIntensity = 0.6f * intensity,
                VignetteSize = 0.8f
            },
            "Scanlines" => new EffectParameters
            {
                ScanlineIntensity = 0.3f * intensity,
                ScanlineCount = 200
            },
            "Grayscale" => new EffectParameters
            {
                GrayscaleAmount = 0.8f * intensity
            },
            "Noise" => new EffectParameters
            {
                NoiseIntensity = 0.4f * intensity,
                NoiseScale = 1f
            },
            "Glitch" => new EffectParameters
            {
                GlitchIntensity = 0.6f * intensity,
                GlitchFrequency = 10f
            },
            "Distortion" => new EffectParameters
            {
                DistortionAmount = 0.5f * intensity,
                DistortionSpeed = 2f
            },
            _ => new EffectParameters()
        };
    }
    
    /// <summary>
    /// Generate animation parameters
    /// </summary>
    private AnimationParameters GenerateAnimationParameters(string animation, DifficultyTier difficulty)
    {
        float speedMultiplier = difficulty switch
        {
            DifficultyTier.Easy => 0.7f,
            DifficultyTier.Normal => 1.0f,
            DifficultyTier.Hard => 1.3f,
            DifficultyTier.Expert => 1.6f,
            DifficultyTier.Master => 2.0f,
            _ => 1.0f
        };
        
        return animation switch
        {
            "Parallax" => new AnimationParameters
            {
                Speed = 0.5f * speedMultiplier,
                Amplitude = 10f
            },
            "Pulse" => new AnimationParameters
            {
                Speed = 1f * speedMultiplier,
                Amplitude = 0.2f,
                Duration = 2f
            },
            "Wave" => new AnimationParameters
            {
                Speed = 2f * speedMultiplier,
                Amplitude = 20f,
                Frequency = 1f
            },
            "Drift" => new AnimationParameters
            {
                Speed = 0.3f * speedMultiplier,
                Amplitude = 50f
            },
            "Rotate" => new AnimationParameters
            {
                Speed = 0.5f * speedMultiplier,
                Amplitude = 5f
            },
            "Zoom" => new AnimationParameters
            {
                Speed = 0.2f * speedMultiplier,
                Amplitude = 0.1f
            },
            "Shake" => new AnimationParameters
            {
                Speed = 10f * speedMultiplier,
                Amplitude = 5f
            },
            "Scroll" => new AnimationParameters
            {
                Speed = 1f * speedMultiplier,
                Amplitude = 100f
            },
            "Fade" => new AnimationParameters
            {
                Speed = 0.5f * speedMultiplier,
                Amplitude = 0.5f,
                Duration = 3f
            },
            "Breathe" => new AnimationParameters
            {
                Speed = 0.8f * speedMultiplier,
                Amplitude = 0.15f,
                Duration = 4f
            },
            _ => new AnimationParameters()
        };
    }
    
    /// <summary>
    /// Generate a background sequence for level transitions
    /// </summary>
    public List<GeneratedBackground> GenerateBackgroundSequence(string startEffect, int count)
    {
        _effectChain.Initialize(startEffect);
        var sequence = new List<GeneratedBackground>();
        
        for (int i = 0; i < count; i++)
        {
            var effect = _effectChain.GetNextState() ?? "Bloom";
            var color = _colorChain.GetNextState() ?? "Sunset";
            var animation = _animationChain.GetNextState() ?? "Parallax";
            
            var background = new GeneratedBackground
            {
                Id = $"Background_{_backgroundCounter++}",
                Name = $"Sequence_Background_{i}",
                Effect = effect,
                ColorScheme = color,
                Animation = animation,
                Layers = GenerateBackgroundLayers("Platforming", color),
                EffectParameters = GenerateEffectParameters(effect, DifficultyTier.Normal),
                AnimationParameters = GenerateAnimationParameters(animation, DifficultyTier.Normal)
            };
            
            sequence.Add(background);
        }
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generated background sequence of {count} backgrounds");
        
        return sequence;
    }
    
    /// <summary>
    /// Get a previously generated background
    /// </summary>
    public GeneratedBackground? GetBackground(string backgroundId)
    {
        return _generatedBackgrounds.TryGetValue(backgroundId, out var background) ? background : null;
    }
    
    /// <summary>
    /// Get all generated backgrounds
    /// </summary>
    public List<GeneratedBackground> GetAllBackgrounds()
    {
        return _generatedBackgrounds.Values.ToList();
    }
    
    /// <summary>
    /// Get statistics about generated backgrounds
    /// </summary>
    public BackgroundGenerationStats GetStats()
    {
        return new BackgroundGenerationStats
        {
            TotalBackgroundsGenerated = _generatedBackgrounds.Count,
            BackgroundsByEffect = _generatedBackgrounds.GroupBy(b => b.Value.Effect)
                .ToDictionary(g => g.Key, g => g.Count()),
            BackgroundsByColor = _generatedBackgrounds.GroupBy(b => b.Value.ColorScheme)
                .ToDictionary(g => g.Key, g => g.Count()),
            BackgroundsByAnimation = _generatedBackgrounds.GroupBy(b => b.Value.Animation)
                .ToDictionary(g => g.Key, g => g.Count()),
            EffectChainStats = _effectChain.GetStats(),
            ColorChainStats = _colorChain.GetStats(),
            AnimationChainStats = _animationChain.GetStats()
        };
    }
}

/// <summary>
/// Generated background
/// </summary>
public class GeneratedBackground
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public string Effect { get; set; } = string.Empty;
    public string ColorScheme { get; set; } = string.Empty;
    public string Animation { get; set; } = string.Empty;
    public DifficultyTier Difficulty { get; set; }
    public List<BackgroundLayer> Layers { get; set; } = new();
    public EffectParameters EffectParameters { get; set; } = new();
    public AnimationParameters AnimationParameters { get; set; } = new();
}

/// <summary>
/// Background layer
/// </summary>
public class BackgroundLayer
{
    public int Index { get; set; }
    public float Depth { get; set; }
    public float ParallaxFactor { get; set; }
    public Color Color { get; set; }
    public float Opacity { get; set; }
    public string Texture { get; set; } = string.Empty;
}

/// <summary>
/// Effect parameters (using Nez effects)
/// </summary>
public class EffectParameters
{
    // Bloom
    public float BloomThreshold { get; set; }
    public float BloomIntensity { get; set; }
    public float BloomBlurAmount { get; set; }
    
    // Vignette
    public float VignetteIntensity { get; set; }
    public float VignetteSize { get; set; }
    
    // Scanlines
    public float ScanlineIntensity { get; set; }
    public int ScanlineCount { get; set; }
    
    // Grayscale
    public float GrayscaleAmount { get; set; }
    
    // Noise
    public float NoiseIntensity { get; set; }
    public float NoiseScale { get; set; }
    
    // Glitch
    public float GlitchIntensity { get; set; }
    public float GlitchFrequency { get; set; }
    
    // Distortion
    public float DistortionAmount { get; set; }
    public float DistortionSpeed { get; set; }
}

/// <summary>
/// Animation parameters
/// </summary>
public class AnimationParameters
{
    public float Speed { get; set; }
    public float Amplitude { get; set; }
    public float Frequency { get; set; }
    public float Duration { get; set; }
}

/// <summary>
/// Statistics for background generation
/// </summary>
public class BackgroundGenerationStats
{
    public int TotalBackgroundsGenerated { get; set; }
    public Dictionary<string, int> BackgroundsByEffect { get; set; } = new();
    public Dictionary<string, int> BackgroundsByColor { get; set; } = new();
    public Dictionary<string, int> BackgroundsByAnimation { get; set; } = new();
    public MarkovChainStats EffectChainStats { get; set; } = new();
    public MarkovChainStats ColorChainStats { get; set; } = new();
    public MarkovChainStats AnimationChainStats { get; set; } = new();
    
    public override string ToString()
    {
        return $"Total Backgrounds: {TotalBackgroundsGenerated}, " +
               $"Effects: {string.Join(", ", BackgroundsByEffect.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
    }
}
