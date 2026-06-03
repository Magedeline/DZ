using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;

namespace Celeste.Mod.MaggyHelper.ProceduralGeneration.MarkovChain;

/// <summary>
/// Extension methods and utilities for Markov chains
/// </summary>
public static class MarkovChainExtensions
{
    /// <summary>
    /// Create a Markov chain from training data with automatic probability calculation
    /// </summary>
    public static MarkovChain<T> CreateFromTrainingData<T>(IEnumerable<T> trainingData, string name, int order = 1) where T : notnull
    {
        var chain = new MarkovChain<T>(name);
        chain.Train(trainingData, order);
        return chain;
    }
    
    /// <summary>
    /// Create a balanced Markov chain where all transitions have equal probability
    /// </summary>
    public static MarkovChain<T> CreateBalanced<T>(IEnumerable<T> states, string name) where T : notnull
    {
        var chain = new MarkovChain<T>(name);
        var stateList = states.ToList();
        float equalProbability = 1f / stateList.Count;
        
        foreach (var fromState in stateList)
        {
            foreach (var toState in stateList)
            {
                chain.AddTransition(fromState, toState, equalProbability);
            }
        }
        
        return chain;
    }
    
    /// <summary>
    /// Create a cyclic Markov chain (A -> B -> C -> A)
    /// </summary>
    public static MarkovChain<T> CreateCyclic<T>(IEnumerable<T> states, string name) where T : notnull
    {
        var chain = new MarkovChain<T>(name);
        var stateList = states.ToList();
        
        for (int i = 0; i < stateList.Count; i++)
        {
            var fromState = stateList[i];
            var toState = stateList[(i + 1) % stateList.Count];
            chain.AddTransition(fromState, toState, 1f);
        }
        
        return chain;
    }
    
    /// <summary>
    /// Add a self-loop transition (state can transition to itself)
    /// </summary>
    public static void AddSelfLoop<T>(this MarkovChain<T> chain, T state, float probability) where T : notnull
    {
        chain.AddTransition(state, state, probability);
    }
    
    /// <summary>
    /// Add a bidirectional transition (A -> B and B -> A with same probability)
    /// </summary>
    public static void AddBidirectional<T>(this MarkovChain<T> chain, T stateA, T stateB, float probability) where T : notnull
    {
        chain.AddTransition(stateA, stateB, probability);
        chain.AddTransition(stateB, stateA, probability);
    }
    
    /// <summary>
    /// Get the most likely next state
    /// </summary>
    public static T GetMostLikelyNextState<T>(this MarkovChain<T> chain) where T : notnull
    {
        var possibleStates = chain.GetPossibleNextStates();
        if (possibleStates.Count == 0)
            return default(T);
            
        var state = chain.CurrentState;
        if (state == null)
            return default(T);
            
        return possibleStates.OrderByDescending(s => chain.GetTransitionProbability(state, s)).FirstOrDefault();
    }
    
    /// <summary>
    /// Get the most likely next state from a specific state
    /// </summary>
    public static T GetMostLikelyNextState<T>(this MarkovChain<T> chain, T fromState) where T : notnull
    {
        var possibleStates = chain.GetPossibleNextStates(fromState);
        if (possibleStates.Count == 0)
            return default(T);
            
        return possibleStates.OrderByDescending(s => chain.GetTransitionProbability(fromState, s)).FirstOrDefault();
    }
    
    /// <summary>
    /// Get the least likely next state
    /// </summary>
    public static T GetLeastLikelyNextState<T>(this MarkovChain<T> chain) where T : notnull
    {
        var possibleStates = chain.GetPossibleNextStates();
        if (possibleStates.Count == 0)
            return default(T);
            
        var state = chain.CurrentState;
        if (state == null)
            return default(T);
            
        return possibleStates.OrderBy(s => chain.GetTransitionProbability(state, s)).FirstOrDefault();
    }
    
    /// <summary>
    /// Get the least likely next state from a specific state
    /// </summary>
    public static T GetLeastLikelyNextState<T>(this MarkovChain<T> chain, T fromState) where T : notnull
    {
        var possibleStates = chain.GetPossibleNextStates(fromState);
        if (possibleStates.Count == 0)
            return default(T);
            
        return possibleStates.OrderBy(s => chain.GetTransitionProbability(fromState, s)).FirstOrDefault();
    }
}

/// <summary>
/// Higher-order Markov chain that considers n previous states for transitions
/// </summary>
/// <typeparam name="T">The type of states in the Markov chain</typeparam>
public class HigherOrderMarkovChain<T> where T : notnull
{
    private readonly Dictionary<List<T>, Dictionary<T, float>> _transitionMatrix;
    private readonly Random _random;
    private readonly List<T> _stateHistory;
    private readonly int _order;
    private readonly int _seed;
    
    public string Name { get; set; }
    public int Order => _order;
    public bool IsInitialized => _stateHistory.Count >= _order;
    
    public HigherOrderMarkovChain(string name, int order = 2, int seed = 0)
    {
        Name = name;
        _order = Math.Max(1, order);
        _seed = seed;
        _random = seed == 0 ? new Random() : new Random(seed);
        _transitionMatrix = new Dictionary<List<T>, Dictionary<T, float>>(new ListEqualityComparer<T>());
        _stateHistory = new List<T>();
    }
    
    /// <summary>
    /// Add a transition from a sequence of states to the next state
    /// </summary>
    public void AddTransition(List<T> fromStates, T toState, float probability)
    {
        if (fromStates.Count != _order)
        {
            Logger.Log(LogLevel.Warn, "HigherOrderMarkovChain", 
                $"{Name}: Expected {_order} states, got {fromStates.Count}");
            return;
        }
        
        var key = new List<T>(fromStates);
        if (!_transitionMatrix.ContainsKey(key))
        {
            _transitionMatrix[key] = new Dictionary<T, float>();
        }
        
        _transitionMatrix[key][toState] = probability;
    }
    
    /// <summary>
    /// Initialize with a sequence of states
    /// </summary>
    public void Initialize(List<T> initialStates)
    {
        if (initialStates.Count < _order)
        {
            Logger.Log(LogLevel.Warn, "HigherOrderMarkovChain", 
                $"{Name}: Need at least {_order} states to initialize");
            return;
        }
        
        _stateHistory.Clear();
        _stateHistory.AddRange(initialStates.Take(_order));
        Logger.Log(LogLevel.Info, "HigherOrderMarkovChain", $"{Name}: Initialized with {_order} states");
    }
    
    /// <summary>
    /// Get the next state based on the history
    /// </summary>
    public T? GetNextState()
    {
        if (!IsInitialized)
        {
            Logger.Log(LogLevel.Warn, "HigherOrderMarkovChain", $"{Name}: Chain not initialized");
            return default;
        }
        
        var history = _stateHistory.Take(_order).ToList();
        
        if (!_transitionMatrix.ContainsKey(history))
        {
            Logger.Log(LogLevel.Warn, "HigherOrderMarkovChain", 
                $"{Name}: No transitions for current history");
            return _stateHistory.Last();
        }
        
        var transitions = _transitionMatrix[history];
        float roll = (float)_random.NextDouble();
        float cumulative = 0f;
        
        foreach (var transition in transitions)
        {
            cumulative += transition.Value;
            if (roll <= cumulative)
            {
                _stateHistory.Add(transition.Key);
                if (_stateHistory.Count > _order * 2)
                {
                    _stateHistory.RemoveAt(0);
                }
                return transition.Key;
            }
        }
        
        return _stateHistory.Last();
    }
    
    /// <summary>
    /// Train the higher-order Markov chain from a sequence
    /// </summary>
    public void Train(IEnumerable<T> sequence)
    {
        var sequenceList = sequence.ToList();
        
        for (int i = 0; i < sequenceList.Count - _order; i++)
        {
            var fromStates = sequenceList.Skip(i).Take(_order).ToList();
            var toState = sequenceList[i + _order];
            
            if (!_transitionMatrix.ContainsKey(fromStates))
            {
                _transitionMatrix[fromStates] = new Dictionary<T, float>();
            }
            
            if (!_transitionMatrix[fromStates].ContainsKey(toState))
            {
                _transitionMatrix[fromStates][toState] = 0f;
            }
            
            _transitionMatrix[fromStates][toState] += 1f;
        }
        
        // Normalize all transitions
        foreach (var history in _transitionMatrix.Keys.ToList())
        {
            var transitions = _transitionMatrix[history];
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
        
        Logger.Log(LogLevel.Info, "HigherOrderMarkovChain", 
            $"{Name}: Trained on {sequenceList.Count} states with order {_order}");
    }
}

/// <summary>
/// Equality comparer for lists to use as dictionary keys
/// </summary>
internal class ListEqualityComparer<T> : IEqualityComparer<List<T>>
{
    public bool Equals(List<T>? x, List<T>? y)
    {
        if (x == null || y == null)
            return x == y;
            
        if (x.Count != y.Count)
            return false;
            
        for (int i = 0; i < x.Count; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(x[i], y[i]))
                return false;
        }
        
        return true;
    }
    
    public int GetHashCode(List<T> obj)
    {
        unchecked
        {
            int hash = 17;
            foreach (var item in obj)
            {
                hash = hash * 31 + (item?.GetHashCode() ?? 0);
            }
            return hash;
        }
    }
}