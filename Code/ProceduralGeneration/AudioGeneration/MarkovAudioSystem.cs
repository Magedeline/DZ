using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.MarkovChain;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.AudioGeneration;

/// <summary>
/// Markov-based audio transition system for dynamic music and sound effects
/// </summary>
public class MarkovAudioSystem
{
    private const string LogTag = "MarkovAudioSystem";
    
    private readonly MarkovChain<string> _musicChain;
    private readonly MarkovChain<string> _sfxChain;
    private readonly MarkovChain<string> _intensityChain;
    private readonly Random _random;
    private readonly int _seed;
    
    private readonly Dictionary<string, GeneratedAudioSegment> _generatedSegments;
    private int _segmentCounter;
    
    public MarkovAudioSystem(int seed = 0)
    {
        _seed = seed;
        _random = seed == 0 ? new Random() : new Random(seed);
        
        _musicChain = new MarkovChain<string>("MusicTransitions", seed);
        _sfxChain = new MarkovChain<string>("SFXTransitions", seed);
        _intensityChain = new MarkovChain<string>("IntensityLevels", seed);
        
        _generatedSegments = new Dictionary<string, GeneratedAudioSegment>();
        _segmentCounter = 0;
        
        InitializeChains();
    }
    
    private void InitializeChains()
    {
        var musicTracks = new[] { "Calm", "Tense", "Epic", "Mysterious", "Triumphant", "Dark", "Peaceful" };
        var sfxTypes = new[] { "Impact", "Whoosh", "Chime", "Explosion", "Magic", "Ambient", "Warning" };
        var intensityLevels = new[] { "Low", "Medium", "High", "Extreme" };
        
        foreach (var from in musicTracks)
            foreach (var to in musicTracks)
                _musicChain.AddTransition(from, to, CalculateTransitionProbability(from, to));
        
        foreach (var from in sfxTypes)
            foreach (var to in sfxTypes)
                _sfxChain.AddTransition(from, to, 0.12f);
        
        foreach (var from in intensityLevels)
            foreach (var to in intensityLevels)
                _intensityChain.AddTransition(from, to, CalculateIntensityTransition(from, to));
        
        _musicChain.Initialize("Calm");
        _sfxChain.Initialize("Impact");
        _intensityChain.Initialize("Medium");
        
        Logger.Log(LogLevel.Info, LogTag, "Initialized audio transition chains");
    }
    
    private float CalculateTransitionProbability(string from, string to)
    {
        if (from == to) return 0.05f;
        if ((from == "Calm" || from == "Peaceful") && to == "Tense") return 0.25f;
        if ((from == "Tense" || from == "Dark") && to == "Epic") return 0.3f;
        if (to == "Dark") return 0.08f;
        return 0.12f;
    }
    
    private float CalculateIntensityTransition(string from, string to)
    {
        int fromLevel = from switch { "Low" => 1, "Medium" => 2, "High" => 3, "Extreme" => 4, _ => 2 };
        int toLevel = to switch { "Low" => 1, "Medium" => 2, "High" => 3, "Extreme" => 4, _ => 2 };
        int diff = Math.Abs(toLevel - fromLevel);
        
        if (diff == 0) return 0.4f;
        if (diff == 1) return 0.3f;
        if (diff == 2) return 0.2f;
        return 0.1f;
    }
    
    public GeneratedAudioSegment GenerateMusicSegment(string currentMood, DifficultyTier difficulty)
    {
        var nextMood = _musicChain.GetNextState() ?? "Calm";
        var intensity = _intensityChain.GetNextState() ?? "Medium";
        
        return new GeneratedAudioSegment
        {
            Id = $"Audio_{_segmentCounter++}",
            Type = "Music",
            Mood = nextMood,
            Intensity = intensity,
            Duration = 10f + (float)_random.NextDouble() * 20f,
            TransitionTime = 2f + (float)_random.NextDouble() * 2f,
            Parameters = GenerateMusicParameters(nextMood, intensity, difficulty)
        };
    }
    
    private AudioParameters GenerateMusicParameters(string mood, string intensity, DifficultyTier difficulty)
    {
        float intensityMultiplier = difficulty switch
        {
            DifficultyTier.Easy => 0.7f,
            DifficultyTier.Normal => 1.0f,
            DifficultyTier.Hard => 1.3f,
            DifficultyTier.Expert => 1.6f,
            DifficultyTier.Master => 2.0f,
            _ => 1.0f
        };
        
        return new AudioParameters
        {
            Volume = 0.8f * intensityMultiplier,
            Tempo = mood switch { "Epic" => 140f, "Tense" => 120f, "Calm" => 80f, _ => 100f },
            LayerCount = intensity switch { "Low" => 2, "Medium" => 4, "High" => 6, "Extreme" => 8, _ => 4 }
        };
    }
    
    public AudioStats GetStats()
    {
        return new AudioStats
        {
            TotalSegmentsGenerated = _generatedSegments.Count,
            MusicChainStats = _musicChain.GetStats(),
            SFXChainStats = _sfxChain.GetStats(),
            IntensityChainStats = _intensityChain.GetStats()
        };
    }
}

public class GeneratedAudioSegment
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Mood { get; set; } = string.Empty;
    public string Intensity { get; set; } = string.Empty;
    public float Duration { get; set; }
    public float TransitionTime { get; set; }
    public AudioParameters Parameters { get; set; } = new();
}

public class AudioParameters
{
    public float Volume { get; set; }
    public float Tempo { get; set; }
    public int LayerCount { get; set; }
}

public class AudioStats
{
    public int TotalSegmentsGenerated { get; set; }
    public MarkovChainStats MusicChainStats { get; set; } = new();
    public MarkovChainStats SFXChainStats { get; set; } = new();
    public MarkovChainStats IntensityChainStats { get; set; } = new();
}
