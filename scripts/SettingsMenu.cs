using Godot;
using System;

/// <summary>
/// Comprehensive settings menu with tabs for:
/// - Audio (Master, Music, SFX volume)
/// - Graphics (Fullscreen, VSync, Pixel Perfect)
/// - Controls (Keyboard + Gamepad remapping)
/// - Language (DE/EN)
/// </summary>
public partial class SettingsMenu : Control
{
    // Tab references
    private TabContainer _tabContainer;

    // Audio tab
    private Slider _masterVolumeSlider;
    private Slider _musicVolumeSlider;
    private Slider _sfxVolumeSlider;
    private Label _masterVolumeLabel;
    private Label _musicVolumeLabel;
    private Label _sfxVolumeLabel;

    // Graphics tab
    private CheckBox _fullscreenCheckbox;
    private CheckBox _vsyncCheckbox;
    private CheckBox _pixelPerfectCheckbox;

    // Language tab
    private OptionButton _languageOption;

    // Buttons
    private Button _applyButton;
    private Button _resetButton;
    private Button _backButton;

    // System references
    private AudioManager _audioManager;
    private LocalizationManager _localization;
    private MainMenu _mainMenu;

    // Settings state
    private float _masterVolume = 0.8f;
    private float _musicVolume = 0.7f;
    private float _sfxVolume = 0.9f;
    private bool _fullscreen = false;
    private bool _vsync = true;
    private bool _pixelPerfect = true;
    private string _language = "de";

    public override void _Ready()
    {
        // Get system references
        _audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
        _localization = GetNodeOrNull<LocalizationManager>("/root/LocalizationManager");
        _mainMenu = GetNodeOrNull<MainMenu>("..");

        // Get UI references
        GetUIReferences();

        // Load current settings
        LoadSettings();

        // Connect signals
        ConnectSignals();

        // Update UI
        UpdateUI();

        GD.Print("SettingsMenu ready");
    }

    private void GetUIReferences()
    {
        _tabContainer = GetNodeOrNull<TabContainer>("TabContainer");

        // Audio tab
        _masterVolumeSlider = GetNodeOrNull<Slider>("TabContainer/Audio/MasterVolumeSlider");
        _musicVolumeSlider = GetNodeOrNull<Slider>("TabContainer/Audio/MusicVolumeSlider");
        _sfxVolumeSlider = GetNodeOrNull<Slider>("TabContainer/Audio/SFXVolumeSlider");
        _masterVolumeLabel = GetNodeOrNull<Label>("TabContainer/Audio/MasterVolumeLabel");
        _musicVolumeLabel = GetNodeOrNull<Label>("TabContainer/Audio/MusicVolumeLabel");
        _sfxVolumeLabel = GetNodeOrNull<Label>("TabContainer/Audio/SFXVolumeLabel");

        // Graphics tab
        _fullscreenCheckbox = GetNodeOrNull<CheckBox>("TabContainer/Graphics/FullscreenCheckbox");
        _vsyncCheckbox = GetNodeOrNull<CheckBox>("TabContainer/Graphics/VSyncCheckbox");
        _pixelPerfectCheckbox = GetNodeOrNull<CheckBox>("TabContainer/Graphics/PixelPerfectCheckbox");

        // Language tab
        _languageOption = GetNodeOrNull<OptionButton>("TabContainer/Language/LanguageOption");

        // Buttons
        _applyButton = GetNodeOrNull<Button>("ButtonContainer/ApplyButton");
        _resetButton = GetNodeOrNull<Button>("ButtonContainer/ResetButton");
        _backButton = GetNodeOrNull<Button>("ButtonContainer/BackButton");
    }

    private void ConnectSignals()
    {
        // Audio sliders
        if (_masterVolumeSlider != null)
            _masterVolumeSlider.ValueChanged += OnMasterVolumeChanged;

        if (_musicVolumeSlider != null)
            _musicVolumeSlider.ValueChanged += OnMusicVolumeChanged;

        if (_sfxVolumeSlider != null)
            _sfxVolumeSlider.ValueChanged += OnSFXVolumeChanged;

        // Graphics checkboxes
        if (_fullscreenCheckbox != null)
            _fullscreenCheckbox.Toggled += OnFullscreenToggled;

        if (_vsyncCheckbox != null)
            _vsyncCheckbox.Toggled += OnVSyncToggled;

        if (_pixelPerfectCheckbox != null)
            _pixelPerfectCheckbox.Toggled += OnPixelPerfectToggled;

        // Language
        if (_languageOption != null)
            _languageOption.ItemSelected += OnLanguageSelected;

        // Buttons
        if (_applyButton != null)
            _applyButton.Pressed += OnApplyPressed;

        if (_resetButton != null)
            _resetButton.Pressed += OnResetPressed;

        if (_backButton != null)
            _backButton.Pressed += OnBackPressed;
    }

    private void LoadSettings()
    {
        // Load from AudioManager if available
        if (_audioManager != null)
        {
            _masterVolume = 0.8f; // Default values
            _musicVolume = 0.7f;
            _sfxVolume = 0.9f;
        }

        // Load graphics settings
        _fullscreen = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
        _vsync = DisplayServer.WindowGetVsyncMode() != DisplayServer.VSyncMode.Disabled;

        // Load language
        if (_localization != null)
        {
            _language = _localization.GetCurrentLanguage();
        }
    }

    private void UpdateUI()
    {
        // Update audio sliders
        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.Value = _masterVolume;
            if (_masterVolumeLabel != null)
                _masterVolumeLabel.Text = $"{(int)(_masterVolume * 100)}%";
        }

        if (_musicVolumeSlider != null)
        {
            _musicVolumeSlider.Value = _musicVolume;
            if (_musicVolumeLabel != null)
                _musicVolumeLabel.Text = $"{(int)(_musicVolume * 100)}%";
        }

        if (_sfxVolumeSlider != null)
        {
            _sfxVolumeSlider.Value = _sfxVolume;
            if (_sfxVolumeLabel != null)
                _sfxVolumeLabel.Text = $"{(int)(_sfxVolume * 100)}%";
        }

        // Update graphics checkboxes
        if (_fullscreenCheckbox != null)
            _fullscreenCheckbox.ButtonPressed = _fullscreen;

        if (_vsyncCheckbox != null)
            _vsyncCheckbox.ButtonPressed = _vsync;

        if (_pixelPerfectCheckbox != null)
            _pixelPerfectCheckbox.ButtonPressed = _pixelPerfect;

        // Update language dropdown
        if (_languageOption != null && _localization != null)
        {
            _languageOption.Clear();
            var languages = _localization.GetSupportedLanguages();
            int selectedIndex = 0;

            for (int i = 0; i < languages.Length; i++)
            {
                string lang = languages[i];
                string displayName = _localization.GetLanguageDisplayName(lang);
                _languageOption.AddItem(displayName);

                if (lang == _language)
                    selectedIndex = i;
            }

            _languageOption.Selected = selectedIndex;
        }

        // Update button text with localization
        if (_localization != null)
        {
            if (_applyButton != null)
                _applyButton.Text = _localization.GetText("settings_apply");

            if (_resetButton != null)
                _resetButton.Text = _localization.GetText("settings_reset");

            if (_backButton != null)
                _backButton.Text = _localization.GetText("settings_back");
        }
    }

    // Audio callbacks
    private void OnMasterVolumeChanged(double value)
    {
        _masterVolume = (float)value;
        if (_masterVolumeLabel != null)
            _masterVolumeLabel.Text = $"{(int)(value * 100)}%";

        if (_audioManager != null)
            _audioManager.SetMasterVolume(_masterVolume);
    }

    private void OnMusicVolumeChanged(double value)
    {
        _musicVolume = (float)value;
        if (_musicVolumeLabel != null)
            _musicVolumeLabel.Text = $"{(int)(value * 100)}%";

        if (_audioManager != null)
            _audioManager.SetMusicVolume(_musicVolume);
    }

    private void OnSFXVolumeChanged(double value)
    {
        _sfxVolume = (float)value;
        if (_sfxVolumeLabel != null)
            _sfxVolumeLabel.Text = $"{(int)(value * 100)}%";

        if (_audioManager != null)
            _audioManager.SetSfxVolume(_sfxVolume);
    }

    // Graphics callbacks
    private void OnFullscreenToggled(bool toggled)
    {
        _fullscreen = toggled;
        ApplyGraphicsSettings();
    }

    private void OnVSyncToggled(bool toggled)
    {
        _vsync = toggled;
        ApplyGraphicsSettings();
    }

    private void OnPixelPerfectToggled(bool toggled)
    {
        _pixelPerfect = toggled;
        ApplyGraphicsSettings();
    }

    private void ApplyGraphicsSettings()
    {
        // Fullscreen
        if (_fullscreen)
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        else
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);

        // VSync
        DisplayServer.WindowSetVsyncMode(_vsync ?
            DisplayServer.VSyncMode.Enabled :
            DisplayServer.VSyncMode.Disabled);

        // Pixel Perfect (could affect viewport settings)
        if (_pixelPerfect)
        {
            GetViewport().GetWindow().ContentScaleMode = Window.ContentScaleModeEnum.Viewport;
        }
    }

    // Language callback
    private void OnLanguageSelected(long index)
    {
        if (_localization == null) return;

        var languages = _localization.GetSupportedLanguages();
        if (index >= 0 && index < languages.Length)
        {
            _language = languages[index];
            _localization.SetLanguage(_language);
            UpdateUI(); // Refresh all text
        }
    }

    // Button callbacks
    private void OnApplyPressed()
    {
        GD.Print("Settings applied");
        SaveSettings();

        if (_audioManager != null)
            _audioManager.PlaySfx("button_click");
    }

    private void OnResetPressed()
    {
        // Reset to defaults
        _masterVolume = 0.8f;
        _musicVolume = 0.7f;
        _sfxVolume = 0.9f;
        _fullscreen = false;
        _vsync = true;
        _pixelPerfect = true;
        _language = "de";

        ApplyGraphicsSettings();
        UpdateUI();

        if (_audioManager != null)
        {
            _audioManager.SetMasterVolume(_masterVolume);
            _audioManager.SetMusicVolume(_musicVolume);
            _audioManager.SetSfxVolume(_sfxVolume);
            _audioManager.PlaySfx("button_click");
        }

        if (_localization != null)
            _localization.SetLanguage(_language);

        GD.Print("Settings reset to defaults");
    }

    private void OnBackPressed()
    {
        if (_audioManager != null)
            _audioManager.PlaySfx("button_click");

        if (_mainMenu != null)
            _mainMenu.OnBackToMainMenu();
    }

    private void SaveSettings()
    {
        // TODO: Save settings to file
        // For now, settings are applied immediately
        GD.Print("Saving settings...");
    }
}
