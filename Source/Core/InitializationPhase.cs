namespace DZ
{
    /// <summary>
    /// Defines the phases of module initialization to ensure proper loading order
    /// and provide clear structure for the initialization sequence.
    /// </summary>
    public enum InitializationPhase
    {
        /// <summary>
        /// Phase 1: Initialize audio directory structure to prevent audio errors
        /// Must be first to ensure audio banks can be loaded properly
        /// </summary>
        AudioDirectory = 1,
        
        /// <summary>
        /// Phase 2: Initialize core integrations with other mods
        /// Sets up FrostHelper integration and MonoMod exports
        /// </summary>
        CoreIntegrations = 2,
        
        /// <summary>
        /// Phase 3: Initialize metadata registries
        /// Loads all YAML metadata files for areas, submaps, cutscenes, models, etc.
        /// </summary>
        MetadataRegistries = 3,
        
        /// <summary>
        /// Phase 4: Register custom entities and triggers with Everest
        /// Ensures all gameplay elements are available before systems that use them
        /// </summary>
        EntityRegistration = 4,
        
        /// <summary>
        /// Phase 5: Initialize particle systems
        /// Sets up visual effects that may be used by entities
        /// </summary>
        ParticleSystems = 5,
        
        /// <summary>
        /// Phase 6: Register event hooks and callbacks
        /// Sets up the core event system for game state monitoring
        /// </summary>
        HookRegistration = 6,
        
        /// <summary>
        /// Phase 7: Initialize NPC systems
        /// Sets up non-player character interactions and dialogs
        /// </summary>
        NPCSystems = 7,
        
        /// <summary>
        /// Phase 8: Initialize custom cutscene systems
        /// Sets up PrismaticHelper integration and custom cutscene triggers
        /// </summary>
        CustomCutscenes = 8,
        
        /// <summary>
        /// Phase 9: Initialize cutscene management system
        /// Sets up the framework for story sequences and transitions
        /// </summary>
        CutsceneManager = 9,
        
        /// <summary>
        /// Phase 10: Initialize HLSL shader effect system
        /// Sets up the EffectManager and loads shader resources
        /// </summary>
        ShaderEffects = 10,
        
        /// <summary>
        /// Phase 11: Initialize companion character management
        /// Sets up Dream Friends and other companion systems
        /// </summary>
        CompanionManager = 11,
        
        /// <summary>
        /// Phase 12: Initialize player ability and state systems
        /// Sets up Kirby-style abilities and player extensions
        /// </summary>
        PlayerSystems = 12,
        
        /// <summary>
        /// Phase 13: Initialize strawberry collection hooks
        /// Sets up special collectible behavior and tracking
        /// </summary>
        StrawberryHooks = 13,
        
        /// <summary>
        /// Phase 14: Register console commands and debug tools
        /// Final phase - sets up development and debugging utilities
        /// </summary>
        ConsoleCommands = 14
    }
    
    /// <summary>
    /// Provides metadata and utilities for working with initialization phases
    /// </summary>
    public static class InitializationPhaseExtensions
    {
        /// <summary>
        /// Gets a human-readable description of the initialization phase
        /// </summary>
        /// <param name="phase">The initialization phase</param>
        /// <returns>A descriptive string for logging purposes</returns>
        public static string GetDescription(this InitializationPhase phase)
        {
            return phase switch
            {
                InitializationPhase.AudioDirectory => "Initializing audio directory structure",
                InitializationPhase.CoreIntegrations => "Setting up core mod integrations",
                InitializationPhase.MetadataRegistries => "Initializing metadata registries",
                InitializationPhase.EntityRegistration => "Registering custom entities and triggers",
                InitializationPhase.ParticleSystems => "Initializing particle systems",
                InitializationPhase.HookRegistration => "Registering event hooks",
                InitializationPhase.NPCSystems => "Initializing NPC systems",
                InitializationPhase.CustomCutscenes => "Initializing custom cutscene systems",
                InitializationPhase.CutsceneManager => "Initializing cutscene manager",
                InitializationPhase.ShaderEffects => "Initializing HLSL shader effects",
                InitializationPhase.CompanionManager => "Initializing companion manager",
                InitializationPhase.PlayerSystems => "Initializing player systems",
                InitializationPhase.StrawberryHooks => "Initializing strawberry hooks",
                InitializationPhase.ConsoleCommands => "Registering console commands",
                _ => "Unknown initialization phase"
            };
        }
        
        /// <summary>
        /// Gets the method name that should be called for this phase
        /// </summary>
        /// <param name="phase">The initialization phase</param>
        /// <returns>The corresponding method name</returns>
        public static string GetMethodName(this InitializationPhase phase)
        {
            return phase switch
            {
                InitializationPhase.AudioDirectory => "InitializeAudioDirectory",
                InitializationPhase.CoreIntegrations => "InitializeCoreIntegrations",
                InitializationPhase.MetadataRegistries => "InitializeMetadataRegistries",
                InitializationPhase.EntityRegistration => "RegisterCustomEntitiesAndTriggers",
                InitializationPhase.ParticleSystems => "InitializeParticles",
                InitializationPhase.HookRegistration => "RegisterHooks",
                InitializationPhase.NPCSystems => "InitializeNPCSystems",
                InitializationPhase.CustomCutscenes => "InitializeCustomCutscenes",
                InitializationPhase.CutsceneManager => "InitializeCutsceneManager",
                InitializationPhase.ShaderEffects => "InitializeShaderEffects",
                InitializationPhase.CompanionManager => "InitializeCompanionManager",
                InitializationPhase.PlayerSystems => "InitializePlayerSystems",
                InitializationPhase.StrawberryHooks => "InitializeStrawberryHooks",
                InitializationPhase.ConsoleCommands => "RegisterConsoleCommands",
                _ => "UnknownPhase"
            };
        }
        
        /// <summary>
        /// Checks if a phase is critical for basic mod functionality
        /// Critical phases will cause load failure if they fail
        /// </summary>
        /// <param name="phase">The initialization phase</param>
        /// <returns>True if the phase is critical</returns>
        public static bool IsCritical(this InitializationPhase phase)
        {
            return phase switch
            {
                InitializationPhase.AudioDirectory => true,
                InitializationPhase.CoreIntegrations => false,
                InitializationPhase.MetadataRegistries => false,
                InitializationPhase.EntityRegistration => true,
                InitializationPhase.ParticleSystems => false,
                InitializationPhase.HookRegistration => true,
                InitializationPhase.NPCSystems => false,
                InitializationPhase.CutsceneManager => false,
                InitializationPhase.ShaderEffects => false,
                InitializationPhase.CompanionManager => false,
                InitializationPhase.PlayerSystems => true,
                InitializationPhase.StrawberryHooks => false,
                InitializationPhase.ConsoleCommands => false,
                _ => false
            };
        }
    }
}



