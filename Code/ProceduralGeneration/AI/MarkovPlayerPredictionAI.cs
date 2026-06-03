using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.MarkovChain;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.AI;

/// <summary>
/// Enhanced AI system using Markov chains for player pattern prediction
/// </summary>
public class MarkovPlayerPredictionAI
{
    private const string LogTag = "MarkovPlayerPredictionAI";
    
    private readonly HigherOrderMarkovChain<PlayerAction> _playerActionChain;
    private readonly MarkovChain<string> _enemyResponseChain;
    private readonly Dictionary<string, float> _actionProbabilities;
    private readonly List<PlayerAction> _playerHistory;
    private readonly int _maxHistoryLength;
    private readonly int _seed;
    private readonly Random _random;
    
    public MarkovPlayerPredictionAI(int order = 2, int seed = 0)
    {
        _seed = seed;
        _random = seed == 0 ? new Random() : new Random(seed);
        
        _playerActionChain = new HigherOrderMarkovChain<PlayerAction>("PlayerActions", order, seed);
        _enemyResponseChain = new MarkovChain<string>("EnemyResponses", seed);
        _actionProbabilities = new Dictionary<string, float>();
        _playerHistory = new List<PlayerAction>();
        _maxHistoryLength = order * 3;
        
        InitializeChains();
    }
    
    private void InitializeChains()
    {
        var enemyResponses = new[] { "Attack", "Defend", "Retreat", "Ambush", "Predict" };
        
        foreach (var from in enemyResponses)
            foreach (var to in enemyResponses)
                _enemyResponseChain.AddTransition(from, to, CalculateResponseTransition(from, to));
        
        _enemyResponseChain.Initialize("Predict");
        
        Logger.Log(LogLevel.Info, LogTag, "Initialized AI prediction chains");
    }
    
    private float CalculateResponseTransition(string from, string to)
    {
        if (from == to) return 0.05f;
        if (from == "Predict" && to == "Attack") return 0.3f;
        if (from == "Predict" && to == "Defend") return 0.25f;
        if (to == "Retreat") return 0.15f;
        return 0.12f;
    }
    
    public void RecordPlayerAction(PlayerAction action)
    {
        _playerHistory.Add(action);
        
        if (_playerHistory.Count > _maxHistoryLength)
            _playerHistory.RemoveAt(0);
        
        // Update action probabilities
        string actionKey = action.Type.ToString();
        if (!_actionProbabilities.ContainsKey(actionKey))
            _actionProbabilities[actionKey] = 0f;
        _actionProbabilities[actionKey] += 0.1f;
        
        // Train Markov chain when we have enough history
        if (_playerHistory.Count >= _playerActionChain.Order + 1)
        {
            _playerActionChain.Train(_playerHistory);
        }
    }
    
    public PlayerAction PredictNextPlayerAction()
    {
        if (_playerHistory.Count < _playerActionChain.Order)
            return new PlayerAction { Type = ActionType.Unknown };
        
        var recentActions = _playerHistory.TakeLast(_playerActionChain.Order).ToList();
        _playerActionChain.Initialize(recentActions);
        
        var predictedAction = _playerActionChain.GetNextState();
        
        if (predictedAction == null)
            return new PlayerAction { Type = ActionType.Unknown };
        
        return predictedAction;
    }
    
    public string GenerateEnemyResponse(PlayerAction predictedAction)
    {
        var response = _enemyResponseChain.GetNextState() ?? "Predict";
        
        // Adjust response based on predicted action
        if (predictedAction.Type == ActionType.Dash)
            response = "Attack"; // Punish dashes
        else if (predictedAction.Type == ActionType.Jump)
            response = "Ambush"; // Ambush jumping players
        else if (predictedAction.Type == ActionType.Attack)
            response = "Defend"; // Defend against attacks
        
        return response;
    }
    
    public float CalculateActionProbability(string actionType)
    {
        return _actionProbabilities.TryGetValue(actionType, out var prob) ? prob : 0f;
    }
    
    public void Reset()
    {
        _playerHistory.Clear();
        _actionProbabilities.Clear();
        Logger.Log(LogLevel.Info, LogTag, "Reset AI prediction system");
    }
    
    public AIStats GetStats()
    {
        return new AIStats
        {
            HistoryLength = _playerHistory.Count,
            ActionProbabilities = new Dictionary<string, float>(_actionProbabilities),
            EnemyResponseChainStats = _enemyResponseChain.GetStats()
        };
    }
}

public class PlayerAction
{
    public ActionType Type { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Direction { get; set; }
    public float Timestamp { get; set; }
}

public enum ActionType
{
    Idle,
    Move,
    Jump,
    Dash,
    Attack,
    Unknown
}

public class AIStats
{
    public int HistoryLength { get; set; }
    public Dictionary<string, float> ActionProbabilities { get; set; } = new();
    public MarkovChainStats EnemyResponseChainStats { get; set; } = new();
    
    public override string ToString()
    {
        return $"History: {HistoryLength}, Probabilities: {string.Join(", ", ActionProbabilities.Select(kvp => $"{kvp.Key}={kvp.Value:F2}"))}";
    }
}