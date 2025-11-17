using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Global audio manager for music and sound effects.
/// Handles audio playback, volume control, and audio pooling.
/// </summary>
public partial class AudioManager : Node
{
    [Signal]
    public delegate void MusicChangedEventHandler(string trackName);

    [Signal]
    public delegate void VolumeChangedEventHandler(string busName, float volume);

    // Audio bus indices
    private const string MASTER_BUS = "Master";
    private const string MUSIC_BUS = "Music";
    private const string SFX_BUS = "SFX";

    // Volume settings (0.0 to 1.0)
    [Export] public float MasterVolume { get; set; } = 0.8f;
    [Export] public float MusicVolume { get; set; } = 0.7f;
    [Export] public float SfxVolume { get; set; } = 0.8f;

    // Music players
    private AudioStreamPlayer _musicPlayer;
    private AudioStreamPlayer _musicPlayerFade;
    private bool _isCrossfading = false;
    private float _crossfadeDuration = 1.5f;

    // SFX pooling
    private const int SFX_POOL_SIZE = 20;
    private List<AudioStreamPlayer> _sfxPool = new List<AudioStreamPlayer>();
    private int _currentSfxIndex = 0;

    // Audio library paths (configurable)
    private Dictionary<string, string> _musicTracks = new Dictionary<string, string>()
    {
        { "main_menu", "res://audio/music/main_menu.ogg" },
        { "hub", "res://audio/music/hub_theme.ogg" },
        { "combat", "res://audio/music/combat_theme.ogg" },
        { "boss_battle", "res://audio/music/boss_battle.ogg" },
        { "victory", "res://audio/music/victory.ogg" },
        { "game_over", "res://audio/music/game_over.ogg" }
    };

    private Dictionary<string, string> _sfxSounds = new Dictionary<string, string>()
    {
        // Player sounds
        { "player_attack", "res://audio/sfx/player_attack.wav" },
        { "player_hit", "res://audio/sfx/player_hit.wav" },
        { "player_death", "res://audio/sfx/player_death.wav" },
        { "player_heal", "res://audio/sfx/player_heal.wav" },
        { "footstep", "res://audio/sfx/footstep.wav" },

        // Enemy sounds
        { "enemy_hit", "res://audio/sfx/enemy_hit.wav" },
        { "enemy_death", "res://audio/sfx/enemy_death.wav" },
        { "enemy_attack", "res://audio/sfx/enemy_attack.wav" },

        // Boss sounds
        { "boss_roar", "res://audio/sfx/boss_roar.wav" },
        { "boss_attack", "res://audio/sfx/boss_attack.wav" },
        { "boss_phase_transition", "res://audio/sfx/boss_phase_change.wav" },
        { "boss_enrage", "res://audio/sfx/boss_enrage.wav" },
        { "boss_death", "res://audio/sfx/boss_death.wav" },

        // UI sounds
        { "button_click", "res://audio/sfx/button_click.wav" },
        { "button_hover", "res://audio/sfx/button_hover.wav" },
        { "menu_open", "res://audio/sfx/menu_open.wav" },
        { "menu_close", "res://audio/sfx/menu_close.wav" },
        { "inventory_open", "res://audio/sfx/inventory_open.wav" },
        { "item_pickup", "res://audio/sfx/item_pickup.wav" },
        { "item_equip", "res://audio/sfx/item_equip.wav" },
        { "quest_complete", "res://audio/sfx/quest_complete.wav" },

        // Combat sounds
        { "sword_slash", "res://audio/sfx/sword_slash.wav" },
        { "heavy_impact", "res://audio/sfx/heavy_impact.wav" },
        { "projectile_launch", "res://audio/sfx/projectile_launch.wav" },
        { "explosion", "res://audio/sfx/explosion.wav" },
        { "shield_block", "res://audio/sfx/shield_block.wav" },

        // Magic sounds
        { "spell_cast", "res://audio/sfx/spell_cast.wav" },
        { "teleport", "res://audio/sfx/teleport.wav" },
        { "summon", "res://audio/sfx/summon.wav" },
        { "curse", "res://audio/sfx/curse.wav" },
        { "life_drain", "res://audio/sfx/life_drain.wav" }
    };

    private string _currentMusicTrack = "";

    public override void _Ready()
    {
        // Create music players
        _musicPlayer = new AudioStreamPlayer();
        _musicPlayer.Bus = MUSIC_BUS;
        AddChild(_musicPlayer);

        _musicPlayerFade = new AudioStreamPlayer();
        _musicPlayerFade.Bus = MUSIC_BUS;
        AddChild(_musicPlayerFade);

        // Create SFX pool
        for (int i = 0; i < SFX_POOL_SIZE; i++)
        {
            var sfxPlayer = new AudioStreamPlayer();
            sfxPlayer.Bus = SFX_BUS;
            AddChild(sfxPlayer);
            _sfxPool.Add(sfxPlayer);
        }

        // Apply initial volumes
        SetMasterVolume(MasterVolume);
        SetMusicVolume(MusicVolume);
        SetSfxVolume(SfxVolume);

        GD.Print("[AudioManager] Initialized with ", SFX_POOL_SIZE, " SFX channels");
    }

    public override void _Process(double delta)
    {
        // Handle crossfade
        if (_isCrossfading)
        {
            // This is handled by tweens, but we track the state
        }
    }

    #region Music Control

    /// <summary>
    /// Play a music track by name
    /// </summary>
    public void PlayMusic(string trackName, bool loop = true, bool crossfade = true)
    {
        if (!_musicTracks.ContainsKey(trackName))
        {
            GD.PrintErr($"[AudioManager] Music track '{trackName}' not found!");
            return;
        }

        if (_currentMusicTrack == trackName && _musicPlayer.Playing)
        {
            return; // Already playing this track
        }

        string trackPath = _musicTracks[trackName];

        if (!ResourceLoader.Exists(trackPath))
        {
            GD.Print($"[AudioManager] Music file not found at '{trackPath}', skipping playback");
            _currentMusicTrack = trackName;
            return;
        }

        var stream = ResourceLoader.Load<AudioStream>(trackPath);
        if (stream == null)
        {
            GD.PrintErr($"[AudioManager] Failed to load music: {trackPath}");
            return;
        }

        if (crossfade && _musicPlayer.Playing)
        {
            CrossfadeMusic(stream, loop);
        }
        else
        {
            _musicPlayer.Stream = stream;
            _musicPlayer.Play();
            _musicPlayer.VolumeDb = 0;
        }

        _currentMusicTrack = trackName;
        EmitSignal(SignalName.MusicChanged, trackName);
        GD.Print($"[AudioManager] Playing music: {trackName}");
    }

    /// <summary>
    /// Crossfade between current music and new track
    /// </summary>
    private void CrossfadeMusic(AudioStream newStream, bool loop)
    {
        if (_isCrossfading) return;

        _isCrossfading = true;

        // Setup new track on fade player
        _musicPlayerFade.Stream = newStream;
        _musicPlayerFade.VolumeDb = -80; // Start silent
        _musicPlayerFade.Play();

        // Create tweens for crossfade
        var tween = CreateTween();
        tween.SetParallel(true);

        // Fade out current music
        tween.TweenProperty(_musicPlayer, "volume_db", -80, _crossfadeDuration);

        // Fade in new music
        tween.TweenProperty(_musicPlayerFade, "volume_db", 0, _crossfadeDuration);

        tween.SetParallel(false);
        tween.TweenCallback(Callable.From(() =>
        {
            _musicPlayer.Stop();

            // Swap players
            var temp = _musicPlayer;
            _musicPlayer = _musicPlayerFade;
            _musicPlayerFade = temp;

            _isCrossfading = false;
        }));
    }

    /// <summary>
    /// Stop currently playing music
    /// </summary>
    public void StopMusic(bool fade = true)
    {
        if (!_musicPlayer.Playing) return;

        if (fade)
        {
            var tween = CreateTween();
            tween.TweenProperty(_musicPlayer, "volume_db", -80, _crossfadeDuration);
            tween.TweenCallback(Callable.From(() => _musicPlayer.Stop()));
        }
        else
        {
            _musicPlayer.Stop();
        }

        _currentMusicTrack = "";
    }

    /// <summary>
    /// Pause/Resume music
    /// </summary>
    public void PauseMusic(bool paused)
    {
        _musicPlayer.StreamPaused = paused;
    }

    #endregion

    #region SFX Control

    /// <summary>
    /// Play a sound effect by name
    /// </summary>
    public void PlaySfx(string sfxName, float pitchVariation = 0.0f, float volumeDb = 0.0f)
    {
        if (!_sfxSounds.ContainsKey(sfxName))
        {
            GD.PrintErr($"[AudioManager] SFX '{sfxName}' not found!");
            return;
        }

        string sfxPath = _sfxSounds[sfxName];

        if (!ResourceLoader.Exists(sfxPath))
        {
            // Silently skip if audio file doesn't exist (allows game to run without audio assets)
            return;
        }

        var stream = ResourceLoader.Load<AudioStream>(sfxPath);
        if (stream == null)
        {
            GD.PrintErr($"[AudioManager] Failed to load SFX: {sfxPath}");
            return;
        }

        // Get next available SFX player from pool
        var sfxPlayer = _sfxPool[_currentSfxIndex];
        _currentSfxIndex = (_currentSfxIndex + 1) % SFX_POOL_SIZE;

        sfxPlayer.Stream = stream;
        sfxPlayer.VolumeDb = volumeDb;
        sfxPlayer.PitchScale = 1.0f + (float)GD.RandRange(-pitchVariation, pitchVariation);
        sfxPlayer.Play();
    }

    /// <summary>
    /// Play a sound effect at a specific 2D position
    /// </summary>
    public void PlaySfxAtPosition(string sfxName, Vector2 position, float maxDistance = 500.0f)
    {
        // For now, just play the sound (spatial audio can be added later)
        // Calculate volume based on distance to camera/player
        PlaySfx(sfxName);
    }

    /// <summary>
    /// Play a random sound from a list of variants
    /// </summary>
    public void PlayRandomSfx(string[] sfxNames, float pitchVariation = 0.0f)
    {
        if (sfxNames.Length == 0) return;

        int randomIndex = GD.RandRange(0, sfxNames.Length - 1);
        PlaySfx(sfxNames[randomIndex], pitchVariation);
    }

    #endregion

    #region Volume Control

    /// <summary>
    /// Set master volume (0.0 to 1.0)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        MasterVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
        int busIndex = AudioServer.GetBusIndex(MASTER_BUS);
        AudioServer.SetBusVolumeDb(busIndex, LinearToDb(MasterVolume));
        EmitSignal(SignalName.VolumeChanged, MASTER_BUS, MasterVolume);
    }

    /// <summary>
    /// Set music volume (0.0 to 1.0)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
        int busIndex = AudioServer.GetBusIndex(MUSIC_BUS);
        AudioServer.SetBusVolumeDb(busIndex, LinearToDb(MusicVolume));
        EmitSignal(SignalName.VolumeChanged, MUSIC_BUS, MusicVolume);
    }

    /// <summary>
    /// Set SFX volume (0.0 to 1.0)
    /// </summary>
    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
        int busIndex = AudioServer.GetBusIndex(SFX_BUS);
        AudioServer.SetBusVolumeDb(busIndex, LinearToDb(SfxVolume));
        EmitSignal(SignalName.VolumeChanged, SFX_BUS, SfxVolume);
    }

    /// <summary>
    /// Convert linear volume (0-1) to decibels
    /// </summary>
    private float LinearToDb(float linear)
    {
        if (linear <= 0.0f)
            return -80.0f; // Mute

        return Mathf.LinearToDb(linear);
    }

    #endregion

    #region Audio Library Management

    /// <summary>
    /// Register a custom music track
    /// </summary>
    public void RegisterMusic(string trackName, string filePath)
    {
        _musicTracks[trackName] = filePath;
        GD.Print($"[AudioManager] Registered music track: {trackName}");
    }

    /// <summary>
    /// Register a custom SFX
    /// </summary>
    public void RegisterSfx(string sfxName, string filePath)
    {
        _sfxSounds[sfxName] = filePath;
        GD.Print($"[AudioManager] Registered SFX: {sfxName}");
    }

    /// <summary>
    /// Get current music track name
    /// </summary>
    public string GetCurrentMusicTrack()
    {
        return _currentMusicTrack;
    }

    /// <summary>
    /// Check if music is currently playing
    /// </summary>
    public bool IsMusicPlaying()
    {
        return _musicPlayer.Playing;
    }

    #endregion
}
