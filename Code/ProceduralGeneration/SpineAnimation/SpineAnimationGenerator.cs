using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.MarkovChain;
using Microsoft.Xna.Framework;
using Monocle;
using Spine;

namespace Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.SpineAnimation;

/// <summary>
/// Procedural Spine animation variant generator using Markov chains
/// </summary>
public class SpineAnimationGenerator
{
    private const string LogTag = "SpineAnimationGenerator";
    
    private readonly MarkovChain<string> _animationChain;
    private readonly Dictionary<string, AnimationData> _baseAnimations;
    private readonly Random _random;
    private readonly string _skeletonName;
    
    public string SkeletonName => _skeletonName;
    
    public SpineAnimationGenerator(string skeletonName, int seed = 0)
    {
        _skeletonName = skeletonName;
        _random = seed == 0 ? new Random() : new Random(seed);
        _animationChain = new MarkovChain<string>($"{skeletonName}_Animations", seed);
        _baseAnimations = new Dictionary<string, AnimationData>();
    }
    
    /// <summary>
    /// Register a base animation for variation generation
    /// </summary>
    public void RegisterAnimation(string animationName, AnimationData animationData)
    {
        _baseAnimations[animationName] = animationData;
        Logger.Log(LogLevel.Verbose, LogTag, 
            $"Registered animation: {animationName} for {_skeletonName}");
    }
    
    /// <summary>
    /// Build Markov chain transitions between animations
    /// </summary>
    public void BuildAnimationTransitions(Dictionary<string, List<string>> transitionRules)
    {
        foreach (var rule in transitionRules)
        {
            var fromAnimation = rule.Key;
            var possibleNext = rule.Value;
            
            // Distribute probability evenly among possible next animations
            float probability = 1f / possibleNext.Count;
            
            foreach (var toAnimation in possibleNext)
            {
                _animationChain.AddTransition(fromAnimation, toAnimation, probability);
            }
        }
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Built animation transitions for {_skeletonName}");
    }
    
    /// <summary>
    /// Generate a variant of an animation with procedural modifications
    /// </summary>
    public AnimationVariant GenerateVariant(string baseAnimation, float mutationAmount = 0.2f)
    {
        if (!_baseAnimations.ContainsKey(baseAnimation))
        {
            Logger.Log(LogLevel.Warn, LogTag, 
                $"Base animation {baseAnimation} not found for {_skeletonName}");
            return new AnimationVariant(baseAnimation, new Dictionary<string, BoneTransform>());
        }
        
        var baseData = _baseAnimations[baseAnimation];
        var transforms = new Dictionary<string, BoneTransform>();
        
        // Apply procedural mutations to bones
        foreach (var boneName in baseData.BoneNames)
        {
            if (_random.NextDouble() < mutationAmount)
            {
                var transform = GenerateBoneTransform(mutationAmount);
                transforms[boneName] = transform;
            }
        }
        
        // Generate timing variations
        var timingVariation = GenerateTimingVariation(mutationAmount);
        
        return new AnimationVariant(baseAnimation, transforms)
        {
            TimingScale = timingVariation,
            LoopVariation = GenerateLoopVariation(mutationAmount)
        };
    }
    
    /// <summary>
    /// Generate a sequence of animation variants using Markov chain
    /// </summary>
    public List<AnimationVariant> GenerateSequence(string startAnimation, int length)
    {
        _animationChain.Initialize(startAnimation);
        var sequence = new List<AnimationVariant>();
        
        var currentAnimation = startAnimation;
        
        for (int i = 0; i < length; i++)
        {
            var variant = GenerateVariant(currentAnimation, 0.15f);
            sequence.Add(variant);
            
            // Get next animation from Markov chain
            var nextAnimation = _animationChain.GetNextState();
            if (nextAnimation != null)
            {
                currentAnimation = nextAnimation;
            }
            else
            {
                break;
            }
        }
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generated animation sequence of length {sequence.Count} for {_skeletonName}");
        
        return sequence;
    }
    
    /// <summary>
    /// Generate a procedural attack pattern for bosses
    /// </summary>
    public BossAttackPattern GenerateAttackPattern(List<string> attackAnimations)
    {
        var pattern = new BossAttackPattern();
        
        // Build Markov chain for attack animations
        foreach (var fromAttack in attackAnimations)
        {
            foreach (var toAttack in attackAnimations)
            {
                if (fromAttack == toAttack) continue; // Prevent immediate repetition
                
                float probability = CalculateAttackTransitionProbability(fromAttack, toAttack);
                _animationChain.AddTransition(fromAttack, toAttack, probability);
            }
        }
        
        // Generate attack sequence
        var firstAttack = attackAnimations[_random.Next(attackAnimations.Count)];
        pattern.AttackSequence = GenerateSequence(firstAttack, attackAnimations.Count);
        
        // Generate timing variations for attacks
        pattern.TimingVariations = GenerateAttackTiming(pattern.AttackSequence);
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generated attack pattern for {_skeletonName}");
        
        return pattern;
    }
    
    /// <summary>
    /// Calculate intelligent attack transition probabilities
    /// </summary>
    private float CalculateAttackTransitionProbability(string from, string to)
    {
        // Opening moves have high priority
        if (from.Contains("Idle") && to.Contains("Attack"))
            return 0.3f;
        
        // Combo moves
        if (from.Contains("Attack") && to.Contains("Attack"))
            return 0.2f;
        
        // Return to idle after special attacks
        if (from.Contains("Special") && to.Contains("Idle"))
            return 0.4f;
        
        // Special attacks are rare
        if (to.Contains("Special"))
            return 0.1f;
        
        return 0.15f;
    }
    
    /// <summary>
    /// Generate timing variations for attack patterns
    /// </summary>
    private Dictionary<string, float> GenerateAttackTiming(List<AnimationVariant> sequence)
    {
        var timings = new Dictionary<string, float>();
        
        foreach (var variant in sequence)
        {
            float timingModifier = 0.8f + (float)_random.NextDouble() * 0.4f; // 0.8-1.2x
            timings[variant.BaseAnimation] = timingModifier;
        }
        
        return timings;
    }
    
    /// <summary>
    /// Generate a procedural bone transform
    /// </summary>
    private BoneTransform GenerateBoneTransform(float mutationAmount)
    {
        return new BoneTransform
        {
            PositionOffset = new Vector3(
                (float)(_random.NextDouble() - 0.5) * 10 * mutationAmount,
                (float)(_random.NextDouble() - 0.5) * 10 * mutationAmount,
                (float)(_random.NextDouble() - 0.5) * 5 * mutationAmount
            ),
            RotationOffset = (float)(_random.NextDouble() - 0.5) * 30 * mutationAmount,
            ScaleOffset = new Vector2(
                1f + (float)((_random.NextDouble() - 0.5) * 0.3 * mutationAmount),
                1f + (float)((_random.NextDouble() - 0.5) * 0.3 * mutationAmount)
            )
        };
    }
    
    /// <summary>
    /// Generate timing variation
    /// </summary>
    private float GenerateTimingVariation(float mutationAmount)
    {
        return 1f + (float)((_random.NextDouble() - 0.5) * 0.5 * mutationAmount);
    }
    
    /// <summary>
    /// Generate loop variation
    /// </summary>
    private bool GenerateLoopVariation(float mutationAmount)
    {
        // Slight chance to change looping behavior
        return _random.NextDouble() > (0.1 * mutationAmount);
    }
    
    /// <summary>
    /// Apply variant to a Spine skeleton
    /// </summary>
    public void ApplyVariant(Skeleton skeleton, AnimationVariant variant)
    {
        foreach (var boneTransform in variant.BoneTransforms)
        {
            var bone = skeleton.FindBone(boneTransform.Key);
            if (bone != null)
            {
                ApplyBoneTransform(bone, boneTransform.Value);
            }
        }
    }
    
    /// <summary>
    /// Apply transform to a single bone
    /// </summary>
    private void ApplyBoneTransform(Bone bone, BoneTransform transform)
    {
        bone.X += transform.PositionOffset.X;
        bone.Y += transform.PositionOffset.Y;
        bone.Rotation += transform.RotationOffset;
        bone.ScaleX *= transform.ScaleOffset.X;
        bone.ScaleY *= transform.ScaleOffset.Y;
    }
    
    /// <summary>
    /// Create a morph target blend between two animations
    /// </summary>
    public AnimationBlend CreateMorphBlend(string animationA, string animationB, float blendAmount)
    {
        return new AnimationBlend
        {
            AnimationA = animationA,
            AnimationB = animationB,
            BlendAmount = blendAmount,
            BlendDuration = 0.5f + (float)_random.NextDouble() * 0.5f
        };
    }
    
    /// <summary>
    /// Generate procedural idle animations with subtle variations
    /// </summary>
    public List<AnimationVariant> GenerateIdleVariants(string baseIdle, int count)
    {
        var variants = new List<AnimationVariant>();
        
        for (int i = 0; i < count; i++)
        {
            // Idle animations should have subtle variations
            var variant = GenerateVariant(baseIdle, 0.05f);
            variants.Add(variant);
        }
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generated {count} idle variants for {_skeletonName}");
        
        return variants;
    }
    
    /// <summary>
    /// Generate procedural death animations
    /// </summary>
    public AnimationVariant GenerateDeathVariant(string baseDeath)
    {
        // Death animations can have more dramatic variations
        return GenerateVariant(baseDeath, 0.3f);
    }
    
    /// <summary>
    /// Get statistics about generated animations
    /// </summary>
    public SpineAnimationStats GetStats()
    {
        return new SpineAnimationStats
        {
            SkeletonName = _skeletonName,
            BaseAnimationCount = _baseAnimations.Count,
            MarkovStats = _animationChain.GetStats()
        };
    }
}

/// <summary>
/// Data structure for animation variants
/// </summary>
public class AnimationVariant
{
    public string BaseAnimation { get; set; }
    public Dictionary<string, BoneTransform> BoneTransforms { get; set; }
    public float TimingScale { get; set; } = 1f;
    public bool LoopVariation { get; set; } = true;
    
    public AnimationVariant(string baseAnimation, Dictionary<string, BoneTransform> boneTransforms)
    {
        BaseAnimation = baseAnimation;
        BoneTransforms = boneTransforms;
    }
}

/// <summary>
/// Bone transform data
/// </summary>
public class BoneTransform
{
    public Vector3 PositionOffset { get; set; }
    public float RotationOffset { get; set; }
    public Vector2 ScaleOffset { get; set; }
    
    public BoneTransform()
    {
        PositionOffset = Vector3.Zero;
        RotationOffset = 0f;
        ScaleOffset = Vector2.One;
    }
}

/// <summary>
/// Animation blend data
/// </summary>
public class AnimationBlend
{
    public string AnimationA { get; set; } = string.Empty;
    public string AnimationB { get; set; } = string.Empty;
    public float BlendAmount { get; set; } = 0.5f;
    public float BlendDuration { get; set; } = 1f;
}

/// <summary>
/// Boss attack pattern data
/// </summary>
public class BossAttackPattern
{
    public List<AnimationVariant> AttackSequence { get; set; } = new();
    public Dictionary<string, float> TimingVariations { get; set; } = new();
    public float IntensityMultiplier { get; set; } = 1f;
}

/// <summary>
/// Base animation data
/// </summary>
public class AnimationData
{
    public string Name { get; set; } = string.Empty;
    public List<string> BoneNames { get; set; } = new();
    public float Duration { get; set; }
    public bool Loops { get; set; }
}

/// <summary>
/// Statistics for Spine animation generator
/// </summary>
public class SpineAnimationStats
{
    public string SkeletonName { get; set; } = string.Empty;
    public int BaseAnimationCount { get; set; }
    public MarkovChainStats MarkovStats { get; set; } = new();
    
    public override string ToString()
    {
        return $"Skeleton: {SkeletonName}, Animations: {BaseAnimationCount}, Markov: {MarkovStats}";
    }
}