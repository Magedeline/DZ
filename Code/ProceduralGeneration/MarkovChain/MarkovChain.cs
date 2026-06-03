using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.MarkovChain;

/// <summary>
/// Core Markov Chain implementation for procedural generation across all systems
/// </summary>
/// <typeparam name="T">The type of states in the Markov chain</typeparam>
public class MarkovChain<T> where T : notnull
{
    private readonly Dictionary<T, Dictionary<T, float>> _transitionMatrix;
    private readonly Random _random;
    private T? _currentState;
    private readonly int _seed;
    
    public string Name { get; set; }
    public bool IsInitialized => _currentState != null;
    public T? CurrentState => _currentState;
    
    /// <summary>
    /// Event fired when state transitions occur
    /// </summary>
    public event Action<T, T>? OnStateTransition;
    
    public MarkovChain(string name, int seed = 0)
    {
        Name = name;
        _seed = seed;
        _random = seed == 0 ? new Random() : new Random(seed);
        _transitionMatrix = new Dictionary<T, Dictionary<T, float>>();
    }
    
    /// <summary>
    /// Add a transition between states with a given probability
    /// </summary>
    public void AddTransition(T fromState, T toState, float probability)
    {
        if (!_transitionMatrix.ContainsKey(fromState))
        {
            _transitionMatrix[fromState] = new Dictionary<T, float>();
        }
        
        _transitionMatrix[fromState][toState] = probability;
        Logger.Log(LogLevel.Verbose, "MarkovChain", $"{Name}: Added transition {fromState} -> {toState} (p={probability:F2})");
    }
    
    /// <summary>
    /// Add multiple transitions from a state (probabilities will be normalized)
    /// </summary>
    public void AddTransitions(T fromState, Dictionary<T, float> transitions)
    {
        if (!_transitionMatrix.ContainsKey(fromState))
        {
            _transitionMatrix[fromState] = new Dictionary<T, float>();
        }
        
        foreach (var transition in transitions)
        {
            _transitionMatrix[fromState][transition.Key] = transition.Value;
        }
        
        NormalizeTransitions(fromState);
    }
    
    /// <summary>
    /// Normalize transition probabilities for a state to sum to 1.0
    /// </summary>
    private void NormalizeTransitions(T fromState)
    {
        if (!_transitionMatrix.ContainsKey(fromState))
            return;
            
        var transitions = _transitionMatrix[fromState];
        float total = transitions.Values.Sum();
        
        if (total > 0)
        {
            var keys = transitions.Keys.ToList();
            foreach (var key in keys)
            {
                transitions[key] /= total;
            }
        }
    }
    
    /// <summary>
    /// Initialize the chain with a starting state
    /// </summary>
    public void Initialize(T initialState)
    {
        _currentState = initialState;
        Logger.Log(LogLevel.Info, "MarkovChain", $"{Name}: Initialized with state {initialState}");
    }
    
    /// <summary>
    /// Get the next state based on current state and transition probabilities
    /// </summary>
    public T? GetNextState()
    {
        if (_currentState == null)
        {
            Logger.Log(LogLevel.Warn, "MarkovChain", $"{Name}: Cannot get next state - chain not initialized");
            return default;
        }
        
        if (!_transitionMatrix.ContainsKey(_currentState))
        {
            Logger.Log(LogLevel.Warn, "MarkovChain", $"{Name}: No transitions defined for state {_currentState}");
            return _currentState;
        }
        
        var transitions = _transitionMatrix[_currentState];
        float roll = (float)_random.NextDouble();
        float cumulative = 0f;
        
        foreach (var transition in transitions)
        {
            cumulative += transition.Value;
            if (roll <= cumulative)
            {
                T previousState = _currentState;
                _currentState = transition.Key;
                
                OnStateTransition?.Invoke(previousState, _currentState);
                Logger.Log(LogLevel.Verbose, "MarkovChain", $"{Name}: Transition {previousState} -> {_currentState}");
                
                return _currentState;
            }
        }
        
        return _currentState;
    }
    
    /// <summary>
    /// Generate a sequence of states
    /// </summary>
    public List<T> GenerateSequence(int length)
    {
        var sequence = new List<T>();
        
        if (!IsInitialized)
        {
            Logger.Log(LogLevel.Warn, "MarkovChain", $"{Name}: Cannot generate sequence - chain not initialized");
            return sequence;
        }
        
        sequence.Add(_currentState!);
        
        for (int i = 1; i < length; i++)
        {
            var nextState = GetNextState();
            if (nextState != null)
            {
                sequence.Add(nextState);
            }
            else
            {
                break;
            }
        }
        
        Logger.Log(LogLevel.Info, "MarkovChain", $"{Name}: Generated sequence of length {sequence.Count}");
        return sequence;
    }
    
    /// <summary>
    /// Generate a sequence of states starting from a specific state
    /// </summary>
    public List<T> GenerateSequence(int length, T startState)
    {
        Initialize(startState);
        return GenerateSequence(length);
    }
    
    /// <summary>
    /// Get transition probability from one state to another
    /// </summary>
    public float GetTransitionProbability(T fromState, T toState)
    {
        if (_transitionMatrix.ContainsKey(fromState) && _transitionMatrix[fromState].ContainsKey(toState))
        {
            return _transitionMatrix[fromState][toState];
        }
        return 0f;
    }
    
    /// <summary>
    /// Get all possible next states from current state
    /// </summary>
    public List<T> GetPossibleNextStates()
    {
        if (_currentState == null || !_transitionMatrix.ContainsKey(_currentState))
        {
            return new List<T>();
        }
        
        return _transitionMatrix[_currentState].Keys.ToList();
    }
    
    /// <summary>
    /// Get all possible next states from a specific state
    /// </summary>
    public List<T> GetPossibleNextStates(T fromState)
    {
        if (!_transitionMatrix.ContainsKey(fromState))
        {
            return new List<T>();
        }
        
        return _transitionMatrix[fromState].Keys.ToList();
    }
    
    /// <summary>
    /// Reset the chain to a specific state
    /// </summary>
    public void Reset(T newState)
    {
        _currentState = newState;
        Logger.Log(LogLevel.Info, "MarkovChain", $"{Name}: Reset to state {newState}");
    }
    
    /// <summary>
    /// Clear all transitions
    /// </summary>
    public void Clear()
    {
        _transitionMatrix.Clear();
        _currentState = default;
        Logger.Log(LogLevel.Info, "MarkovChain", $"{Name}: Cleared all transitions");
    }
    
    /// <summary>
    /// Train the Markov chain from a sequence of observed transitions
    /// </summary>
    public void Train(IEnumerable<T> sequence, int order = 1)
    {
        var sequenceList = sequence.ToList();
        
        for (int i = 0; i < sequenceList.Count - order; i++)
        {
            var fromState = sequenceList[i];
            var toState = sequenceList[i + order];
            
            if (!_transitionMatrix.ContainsKey(fromState))
            {
                _transitionMatrix[fromState] = new Dictionary<T, float>();
            }
            
            if (!_transitionMatrix[fromState].ContainsKey(toState))
            {
                _transitionMatrix[fromState][toState] = 0f;
            }
            
            _transitionMatrix[fromState][toState] += 1f;
        }
        
        // Normalize all transitions
        foreach (var state in _transitionMatrix.Keys.ToList())
        {
            NormalizeTransitions(state);
        }
        
        Logger.Log(LogLevel.Info, "MarkovChain", $"{Name}: Trained on {sequenceList.Count} states with order {order}");
    }
    
    /// <summary>
    /// Get statistics about the chain
    /// </summary>
    public MarkovChainStats GetStats()
    {
        return new MarkovChainStats
        {
            StateCount = _transitionMatrix.Count,
            TotalTransitions = _transitionMatrix.Sum(kvp => kvp.Value.Count),
            CurrentState = _currentState?.ToString() ?? "None",
            IsInitialized = IsInitialized
        };
    }
}

/// <summary>
/// Statistics about a Markov chain
/// </summary>
public class MarkovChainStats
{
    public int StateCount { get; set; }
    public int TotalTransitions { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public bool IsInitialized { get; set; }
    
    public override string ToString()
    {
        return $"States: {StateCount}, Transitions: {TotalTransitions}, Current: {CurrentState}, Initialized: {IsInitialized}";
    }
}