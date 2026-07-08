using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.NPCs;
using Microsoft.Xna.Framework;
using Monocle;
using NPC = Celeste.NPCs.NPC;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Base class for all lobby cutscenes (Chapter 10 Ruins Lobby, Chapter 11 Snowdin City Lobby, etc.)
    /// Provides standardized state checking, doorway unlocks, and player routing logic
    /// </summary>
    public abstract class LobbyCutsceneManager : CutsceneEntity
    {
        protected CelestePlayer player;
        protected Level currentLevel;
        protected float startLightAlpha;
        protected SoundSource sfx = new SoundSource();
        
        // Lobby configuration
        protected abstract string LobbyName { get; }
        protected abstract string LobbyFlag { get; }
        protected abstract string MusicEvent { get; }
        protected abstract string ExplorationMusicEvent { get; }
        
        // Common doorway management
        protected bool DoorsUnlocked { get; private set; } = false;
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected LobbyCutsceneManager(CelestePlayer player)
        {
            this.player = player;
            Add(sfx);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void OnBegin(Level level)
        {
            currentLevel = level;
            startLightAlpha = level.Lighting.Alpha;
            
            // Check if we should skip based on flag
            if (!ShouldRunLobbySequence())
            {
                EndCutscene(level);
                return;
            }
            
            Add(new Coroutine(LobbyCutsceneRoutine(level)));
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected virtual bool ShouldRunLobbySequence()
        {
            return !currentLevel.Session.GetFlag(LobbyFlag);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator LobbyCutsceneRoutine(Level level)
        {
            // Lock player movement
            player.StateMachine.State = Player.StDummy;
            player.StateMachine.Locked = true;
            
            yield return PreLobbySequence(level);
            
            // Set lobby music
            if (!string.IsNullOrEmpty(MusicEvent))
            {
                Audio.SetMusic(MusicEvent);
            }
            
            // Run main lobby sequence
            yield return MainLobbySequence(level);
            
            // Open all doors
            yield return UnlockLobbyDoors();
            
            // Post sequence cleanup
            yield return PostLobbySequence(level);
            
            EndCutscene(level);
        }
        
        /// <summary>
        /// Override to implement pre-lobby setup (lighting, camera movements, etc.)
        /// </summary>
        protected virtual IEnumerator PreLobbySequence(Level level)
        {
            yield return 0.5f;
        }
        
        /// <summary>
        /// Override to implement the main lobby cutscene sequence
        /// </summary>
        protected abstract IEnumerator MainLobbySequence(Level level);
        
        /// <summary>
        /// Override to implement post-lobby cleanup
        /// </summary>
        protected virtual IEnumerator PostLobbySequence(Level level)
        {
            yield return 1.5f;
        }
        
        /// <summary>
        /// Standardized door unlocking for lobbies
        /// </summary>
        protected virtual IEnumerator UnlockLobbyDoors()
        {
            if (DoorsUnlocked)
                yield break;
                
            // Find all door types that lobbies commonly use
            foreach (var door in currentLevel.Entities.FindAll<Door>())
            {
                door.Open(door.X);
            }
            
            foreach (var door in currentLevel.Entities.FindAll<MrOshiroDoor>())
            {
                door.Open();
            }
            
            // Add support for custom lobby doors if needed
            UnlockCustomDoors();
            
            DoorsUnlocked = true;
            yield return 0.25f;
        }
        
        /// <summary>
        /// Override to handle custom door types in specific lobbies
        /// </summary>
        protected virtual void UnlockCustomDoors()
        {
            // Override in derived classes for custom door types
        }
        
        /// <summary>
        /// Standard player routing helper
        /// </summary>
        protected IEnumerator RoutePlayerTo(Vector2 target, bool walkBackwards = false)
        {
            if (walkBackwards)
            {
                yield return player.DummyWalkToExact((int)target.X, walkBackwards: true);
            }
            else
            {
                yield return player.DummyWalkTo(target.X);
            }
            
            yield return 0.1f;
        }
        
        /// <summary>
        /// Standard NPC movement helper
        /// </summary>
        protected void MoveNPCAndRemove(NPC npc, Vector2 target)
        {
            npc.MoveToAndRemove(target);
            npc.Add(new SoundSource("event:/char/movement/exit"));
        }
        
        /// <summary>
        /// Standard zoom helper for lobby reveals
        /// </summary>
        protected IEnumerator ZoomToLobbyArea(Vector2 center, float zoom = 2f, float duration = 0.5f)
        {
            yield return currentLevel.ZoomTo(center, zoom, duration);
        }
        
        protected IEnumerator ZoomBack(float duration = 0.5f)
        {
            yield return 0.2f;
            yield return currentLevel.ZoomBack(duration);
            yield return 0.2f;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void OnEnd(Level level)
        {
            // Unlock player
            player.StateMachine.Locked = false;
            player.StateMachine.State = Player.StNormal;
            
            // Reset lighting
            level.Lighting.Alpha = startLightAlpha;
            level.Lighting.UnsetSpotlight();
            
            // Set flag
            level.Session.SetFlag(LobbyFlag);
            
            // Set exploration music
            if (!string.IsNullOrEmpty(ExplorationMusicEvent))
            {
                level.Session.Audio.Music.Event = ExplorationMusicEvent;
                level.Session.Audio.Music.Progress = 1;
                level.Session.Audio.Apply(forceSixteenthNoteHack: false);
            }
            
            // Handle skipped cutscenes
            if (WasSkipped)
            {
                HandleSkippedCutscene(level);
            }
            
            // Additional cleanup
            OnLobbyComplete(level);
        }
        
        /// <summary>
        /// Override for additional cleanup when lobby completes
        /// </summary>
        protected virtual void OnLobbyComplete(Level level)
        {
            // Override in derived classes
        }
        
        /// <summary>
        /// Handle what happens when the cutscene is skipped
        /// </summary>
        protected virtual void HandleSkippedCutscene(Level level)
        {
            // Instantly open all doors
            foreach (var door in level.Entities.FindAll<Door>())
            {
                door.Open(door.X);
            }
            
            foreach (var door in level.Entities.FindAll<MrOshiroDoor>())
            {
                door.Open();
            }
        }
    }
}
