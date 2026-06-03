using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.MaggyHelper.ProceduralGeneration.MarkovChain;

namespace Celeste.Mod.MaggyHelper.ProceduralGeneration.Dialogue;

/// <summary>
/// Markov chain-based dialogue generator for character speech patterns
/// </summary>
public class MarkovDialogueGenerator
{
    private const string LogTag = "MarkovDialogueGenerator";
    
    private readonly Dictionary<string, HigherOrderMarkovChain<string>> _characterChains;
    private readonly Dictionary<string, DialogueProfile> _characterProfiles;
    private readonly Random _random;
    private readonly int _seed;
    
    private readonly Dictionary<string, GeneratedDialogue> _generatedDialogues;
    private int _dialogueCounter;
    
    public MarkovDialogueGenerator(int seed = 0)
    {
        _seed = seed;
        _random = seed == 0 ? new Random() : new Random(seed);
        
        _characterChains = new Dictionary<string, HigherOrderMarkovChain<string>>();
        _characterProfiles = new Dictionary<string, DialogueProfile>();
        _generatedDialogues = new Dictionary<string, GeneratedDialogue>();
        _dialogueCounter = 0;
    }
    
    /// <summary>
    /// Register a character with training dialogue
    /// </summary>
    public void RegisterCharacter(string characterName, DialogueProfile profile, List<string> trainingDialogue)
    {
        _characterProfiles[characterName] = profile;
        
        var chain = new HigherOrderMarkovChain<string>($"{characterName}_Dialogue", profile.Order, _seed + characterName.GetHashCode());
        chain.Train(trainingDialogue);
        _characterChains[characterName] = chain;
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Registered character: {characterName} with {trainingDialogue.Count} training lines");
    }
    
    /// <summary>
    /// Generate dialogue for a character
    /// </summary>
    public GeneratedDialogue GenerateDialogue(string characterName, DialogueContext context)
    {
        if (!_characterChains.ContainsKey(characterName))
        {
            Logger.Log(LogLevel.Warn, LogTag, $"Character {characterName} not registered");
            return new GeneratedDialogue { CharacterName = characterName, Text = "..." };
        }
        
        var profile = _characterProfiles[characterName];
        var chain = _characterChains[characterName];
        
        // Generate words using Markov chain
        var words = GenerateWords(chain, context.WordCount);
        
        // Apply character personality modifiers
        ApplyPersonalityModifiers(ref words, profile);
        
        var text = string.Join(" ", words);
        
        var dialogue = new GeneratedDialogue
        {
            Id = $"Dialogue_{_dialogueCounter++}",
            CharacterName = characterName,
            Text = text,
            Context = context,
            Emotion = DetermineEmotion(profile, context),
            Duration = CalculateDialogueDuration(text, profile.SpeakingSpeed)
        };
        
        _generatedDialogues[dialogue.Id] = dialogue;
        
        Logger.Log(LogLevel.Verbose, LogTag, 
            $"Generated dialogue for {characterName}: \"{text}\"");
        
        return dialogue;
    }
    
    /// <summary>
    /// Generate words using higher-order Markov chain
    /// </summary>
    private List<string> GenerateWords(HigherOrderMarkovChain<string> chain, int wordCount)
    {
        var words = new List<string>();
        var seedWords = new List<string> { "I", "am" };
        chain.Initialize(seedWords);
        
        for (int i = 0; i < wordCount; i++)
        {
            var word = chain.GetNextState();
            if (word == null) break;
            words.Add(word);
        }
        
        return words;
    }
    
    /// <summary>
    /// Apply personality modifiers to dialogue
    /// </summary>
    private void ApplyPersonalityModifiers(ref List<string> words, DialogueProfile profile)
    {
        if (profile.Personality.Contains("Aggressive"))
        {
            // Add exclamation marks and aggressive words
            if (words.Count > 0)
                words[words.Count - 1] = words[words.Count - 1].ToUpper() + "!";
        }
        
        if (profile.Personality.Contains("Calm"))
        {
            // Add softer language
            if (words.Count > 0 && words[^1].EndsWith("!"))
                words[^1] = words[^1].Replace("!", ".");
        }
        
        if (profile.Personality.Contains("Formal"))
        {
            // Add formal words
            if (words.Count > 0)
                words[0] = char.ToUpper(words[0][0]) + words[0].Substring(1);
        }
    }
    
    /// <summary>
    /// Determine emotion based on profile and context
    /// </summary>
    private string DetermineEmotion(DialogueProfile profile, DialogueContext context)
    {
        if (context.ContextType == "Combat")
            return profile.Personality.Contains("Aggressive") ? "Angry" : "Determined";
        
        if (context.ContextType == "Victory")
            return "Happy";
        
        if (context.ContextType == "Defeat")
            return "Sad";
        
        return "Neutral";
    }
    
    /// <summary>
    /// Calculate dialogue duration based on text and speaking speed
    /// </summary>
    private float CalculateDialogueDuration(string text, float speakingSpeed)
    {
        int wordCount = text.Split(' ').Length;
        return wordCount / speakingSpeed;
    }
    
    /// <summary>
    /// Generate a dialogue sequence (conversation)
    /// </summary>
    public List<GeneratedDialogue> GenerateConversation(List<string> participants, DialogueContext context)
    {
        var conversation = new List<GeneratedDialogue>();
        
        foreach (var participant in participants)
        {
            if (_characterChains.ContainsKey(participant))
            {
                var dialogue = GenerateDialogue(participant, context);
                conversation.Add(dialogue);
            }
        }
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generated conversation with {conversation.Count} lines");
        
        return conversation;
    }
    
    /// <summary>
    /// Get statistics about generated dialogues
    /// </summary>
    public DialogueGenerationStats GetStats()
    {
        return new DialogueGenerationStats
        {
            TotalDialoguesGenerated = _generatedDialogues.Count,
            CharactersRegistered = _characterChains.Count,
            DialoguesByCharacter = _generatedDialogues.GroupBy(d => d.Value.CharacterName)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}

/// <summary>
/// Character dialogue profile
/// </summary>
public class DialogueProfile
{
    public int Order { get; set; } = 2;
    public List<string> Personality { get; set; } = new();
    public float SpeakingSpeed { get; set; } = 2f; // Words per second
    public List<string> Catchphrases { get; set; } = new();
}

/// <summary>
/// Dialogue context
/// </summary>
public class DialogueContext
{
    public string ContextType { get; set; } = "General"; // Combat, Victory, Defeat, General
    public int WordCount { get; set; } = 10;
    public Dictionary<string, string> Variables { get; set; } = new();
}

/// <summary>
/// Generated dialogue
/// </summary>
public class GeneratedDialogue
{
    public string Id { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DialogueContext Context { get; set; } = new();
    public string Emotion { get; set; } = string.Empty;
    public float Duration { get; set; }
}

/// <summary>
/// Statistics for dialogue generation
/// </summary>
public class DialogueGenerationStats
{
    public int TotalDialoguesGenerated { get; set; }
    public int CharactersRegistered { get; set; }
    public Dictionary<string, int> DialoguesByCharacter { get; set; } = new();
    
    public override string ToString()
    {
        return $"Total Dialogues: {TotalDialoguesGenerated}, Characters: {CharactersRegistered}";
    }
}