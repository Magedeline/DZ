using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace DZ.Core;

/// <summary>
/// Standalone FMOD Audio Manager.
/// Replaces: GlobalModAudioLoader, PusheenAudioHooks, KirbyAudioHooks, and Celeste's Audio system.
///
/// Key differences from Everest mod approach:
/// - No hooking needed - you control ALL audio calls
/// - Direct FMOD Studio API access
/// - Your banks are loaded directly, no "mod content" scanning
/// - Event paths work exactly as in your FMOD project
/// </summary>
public class AudioManager
{
    // FMOD Studio System
    private FMOD.Studio.System _studioSystem;
    private FMOD.System _coreSystem;

    // Loaded banks
    private readonly Dictionary<string, FMOD.Studio.Bank> _banks = new();
    private readonly Dictionary<string, FMOD.Studio.EventDescription> _eventCache = new();
    private readonly Dictionary<string, FMOD.Studio.Bus> _busCache = new();

    // Active instances for management
    private readonly List<FMOD.Studio.EventInstance> _activeInstances = new();

    // Master bus
    private FMOD.Studio.Bus _masterBus;
    private FMOD.Studio.Bus _musicBus;
    private FMOD.Studio.Bus _sfxBus;

    // Configuration
    public float MasterVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 1.0f;
    public float SfxVolume { get; set; } = 1.0f;

    // Current music instance for crossfading
    private FMOD.Studio.EventInstance _currentMusic;

    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Initialize FMOD Studio system
    /// </summary>
    public void Initialize()
    {
        if (IsInitialized) return;

        try
        {
            // Initialize studio system (it creates the core system internally)
            FMOD.Studio.System.create(out _studioSystem);
            _studioSystem.initialize(512, FMOD.Studio.INITFLAGS.NORMAL, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);

            // Get the core system from studio
            _studioSystem.getCoreSystem(out _coreSystem);

            // Get master bus — always present once a bank with a master bus is loaded
            var masterResult = _studioSystem.getBus("bus:/", out _masterBus);
            if (masterResult != FMOD.RESULT.OK)
                Console.WriteLine($"[AudioManager] Warning: could not obtain master bus (result={masterResult}). Audio volume control may be unavailable.");

            // Retrieve optional buses — these only exist if the FMOD project defines them.
            // Failures here are non-fatal; volume methods guard against invalid handles.
            var musicResult = _studioSystem.getBus("bus:/music", out _musicBus);
            if (musicResult != FMOD.RESULT.OK)
                Console.WriteLine($"[AudioManager] Warning: 'bus:/music' not found in loaded banks (result={musicResult}). Music bus volume control unavailable.");

            var sfxResult = _studioSystem.getBus("bus:/sfx", out _sfxBus);
            if (sfxResult != FMOD.RESULT.OK)
                Console.WriteLine($"[AudioManager] Warning: 'bus:/sfx' not found in loaded banks (result={sfxResult}). SFX bus volume control unavailable.");

            IsInitialized = true;
            Console.WriteLine("[AudioManager] FMOD Studio initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AudioManager] FMOD initialization failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Load an FMOD bank file.
    /// This replaces the complex bank discovery from Everest.
    /// </summary>
    public void LoadBank(string bankName)
    {
        if (!IsInitialized)
            throw new InvalidOperationException("AudioManager not initialized. Call Initialize() first.");

        if (_banks.ContainsKey(bankName))
        {
            Console.WriteLine($"[AudioManager] Bank '{bankName}' already loaded");
            return;
        }

        string bankPath = Path.Combine(DZGame.AudioBankPath, $"{bankName}.bank");
        string stringsPath = Path.Combine(DZGame.AudioBankPath, $"{bankName}.strings.bank");

        if (!File.Exists(bankPath))
        {
            Console.WriteLine($"[AudioManager] Bank file not found: {bankPath}");
            return;
        }

        try
        {
            FMOD.Studio.Bank mainBank;
            FMOD.Studio.Bank stringsBank = default;

            // Load strings bank first (if it exists)
            if (File.Exists(stringsPath))
            {
                var result = _studioSystem.loadBankFile(stringsPath,
                    FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out stringsBank);
                if (result != FMOD.RESULT.OK)
                {
                    Console.WriteLine($"[AudioManager] Failed to load strings bank: {result}");
                }
            }

            // Load main bank
            var mainResult = _studioSystem.loadBankFile(bankPath,
                FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out mainBank);

            if (mainResult != FMOD.RESULT.OK)
            {
                Console.WriteLine($"[AudioManager] Failed to load bank '{bankName}': {mainResult}");
                return;
            }

            _banks[bankName] = mainBank;
            Console.WriteLine($"[AudioManager] Loaded bank: {bankName}");

            // Pre-cache events from this bank for faster lookups
            CacheBankEvents(mainBank);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AudioManager] Exception loading bank '{bankName}': {ex.Message}");
        }
    }

    /// <summary>
    /// Pre-cache all events from a bank for faster runtime lookups
    /// </summary>
    private void CacheBankEvents(FMOD.Studio.Bank bank)
    {
        bank.getEventCount(out int eventCount);
        if (eventCount == 0) return;

        bank.getEventList(out FMOD.Studio.EventDescription[] eventDescriptions);

        foreach (var eventDesc in eventDescriptions)
        {
            eventDesc.getPath(out string path);
            if (!string.IsNullOrEmpty(path))
            {
                _eventCache[path] = eventDesc;
            }
        }

        Console.WriteLine($"[AudioManager] Cached {eventCount} events from bank");
    }

    /// <summary>
    /// Play a one-shot SFX event
    /// </summary>
    public void PlaySfx(string eventPath)
    {
        PlayOneShot(eventPath, _sfxBus);
    }

    /// <summary>
    /// Play a one-shot SFX at a 2D position
    /// </summary>
    public void PlaySfx(string eventPath, Microsoft.Xna.Framework.Vector2 position)
    {
        PlayOneShot3D(eventPath, position);
    }

    /// <summary>
    /// Play music - this handles crossfading from any currently playing music
    /// </summary>
    public void PlayMusic(string eventPath, bool fadeIn = true, float fadeDuration = 2.0f)
    {
        if (!IsInitialized) return;

        // Stop current music with fade out
        if (_currentMusic.hasHandle())
        {
            if (fadeIn)
            {
                _currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
            else
            {
                _currentMusic.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }

        // Create new music instance
        if (!GetEventInstance(eventPath, out FMOD.Studio.EventInstance musicInstance))
        {
            Console.WriteLine($"[AudioManager] Could not find music event: {eventPath}");
            return;
        }

        musicInstance.start();
        _currentMusic = musicInstance;
        _activeInstances.Add(musicInstance);

        Console.WriteLine($"[AudioManager] Playing music: {eventPath}");
    }

    /// <summary>
    /// Set a parameter on the currently playing music
    /// </summary>
    public void SetMusicParameter(string paramName, float value)
    {
        if (_currentMusic.hasHandle())
        {
            _currentMusic.setParameterByName(paramName, value);
        }
    }

    /// <summary>
    /// Stop music with optional fade
    /// </summary>
    public void StopMusic(bool fadeOut = true)
    {
        if (_currentMusic.hasHandle())
        {
            var mode = fadeOut ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE;
            _currentMusic.stop(mode);
        }
    }

    /// <summary>
    /// Play a one-shot event (no instance tracking)
    /// </summary>
    private void PlayOneShot(string eventPath, FMOD.Studio.Bus targetBus)
    {
        if (!IsInitialized) return;

        if (!GetEventInstance(eventPath, out FMOD.Studio.EventInstance instance))
        {
            // Event not found - this is normal if you reference a vanilla Celeste path
            // that doesn't exist in your banks
            Console.WriteLine($"[AudioManager] Event not found: {eventPath}");
            return;
        }

        instance.start();
        instance.release(); // Let FMOD clean up when done
    }

    private void PlayOneShot3D(string eventPath, Microsoft.Xna.Framework.Vector2 position)
    {
        if (!IsInitialized) return;

        if (!GetEventInstance(eventPath, out FMOD.Studio.EventInstance instance))
            return;

        // Set 3D attributes
        var attributes = new FMOD.Studio._3D_ATTRIBUTES
        {
            position = new FMOD.VECTOR { x = position.X, y = position.Y, z = 0 },
            forward = new FMOD.VECTOR { x = 0, y = 0, z = 1 },
            up = new FMOD.VECTOR { x = 0, y = 1, z = 0 }
        };

        instance.set3DAttributes(attributes);
        instance.start();
        instance.release();
    }

    /// <summary>
    /// Get or create an event instance
    /// </summary>
    private bool GetEventInstance(string eventPath, out FMOD.Studio.EventInstance instance)
    {
        instance = default;

        // Try cache first
        if (_eventCache.TryGetValue(eventPath, out FMOD.Studio.EventDescription description))
        {
            description.createInstance(out instance);
            return true;
        }

        // Try to get from system (might be from a bank loaded after cache)
        var result = _studioSystem.getEvent(eventPath, out description);
        if (result == FMOD.RESULT.OK)
        {
            description.createInstance(out instance);
            _eventCache[eventPath] = description; // Cache for next time
            return true;
        }

        return false;
    }

    /// <summary>
    /// Set master volume (0.0 to 1.0)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        MasterVolume = Math.Clamp(volume, 0.0f, 1.0f);
        if (_masterBus.isValid())
            _masterBus.setVolume(MasterVolume);
    }

    /// <summary>
    /// Set music volume (0.0 to 1.0)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        MusicVolume = Math.Clamp(volume, 0.0f, 1.0f);
        if (_musicBus.isValid())
            _musicBus.setVolume(MusicVolume);
    }

    /// <summary>
    /// Set SFX volume (0.0 to 1.0)
    /// </summary>
    public void SetSfxVolume(float volume)
    {
        SfxVolume = Math.Clamp(volume, 0.0f, 1.0f);
        if (_sfxBus.isValid())
            _sfxBus.setVolume(SfxVolume);
    }

    /// <summary>
    /// Mute/unmute all audio
    /// </summary>
    public void SetMuted(bool muted)
    {
        if (_masterBus.isValid())
            _masterBus.setMute(muted);
    }

    /// <summary>
    /// Update audio system - call once per frame
    /// </summary>
    public void Update()
    {
        if (!IsInitialized) return;

        // Update FMOD system
        _studioSystem.update();

        // Clean up stopped instances
        CleanupStoppedInstances();
    }

    private void CleanupStoppedInstances()
    {
        for (int i = _activeInstances.Count - 1; i >= 0; i--)
        {
            var instance = _activeInstances[i];
            instance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE state);

            if (state == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                instance.release();
                _activeInstances.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Shutdown audio system and release all resources
    /// </summary>
    public void Shutdown()
    {
        if (!IsInitialized) return;

        Console.WriteLine("[AudioManager] Shutting down...");

        // Stop all instances
        foreach (var instance in _activeInstances)
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
        }
        _activeInstances.Clear();

        // Unload banks
        foreach (var bank in _banks.Values)
        {
            bank.unload();
        }
        _banks.Clear();
        _eventCache.Clear();

        // Release systems
        _studioSystem.release();
        _coreSystem.close();
        _coreSystem.release();

        IsInitialized = false;
        Console.WriteLine("[AudioManager] Shutdown complete");
    }

    /// <summary>
    /// Get list of loaded bank names for debugging
    /// </summary>
    public IEnumerable<string> GetLoadedBanks()
    {
        return _banks.Keys;
    }
}
