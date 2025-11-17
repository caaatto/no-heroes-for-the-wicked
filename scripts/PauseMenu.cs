using Godot;
using System;

/// <summary>
/// Pause menu system with game pause functionality.
/// Handles pause state, UI display, and user interactions.
/// </summary>
public partial class PauseMenu : CanvasLayer
{
    [Signal]
    public delegate void GamePausedEventHandler(bool isPaused);

    [Signal]
    public delegate void ResumeRequestedEventHandler();

    [Signal]
    public delegate void RestartRequestedEventHandler();

    [Signal]
    public delegate void MainMenuRequestedEventHandler();

    [Signal]
    public delegate void SettingsRequestedEventHandler();

    // UI References
    private Control _pauseMenuContainer;
    private Panel _backgroundPanel;
    private VBoxContainer _buttonContainer;
    private Label _titleLabel;

    private Button _resumeButton;
    private Button _settingsButton;
    private Button _saveButton;
    private Button _loadButton;
    private Button _restartButton;
    private Button _mainMenuButton;
    private Button _quitButton;

    // Settings panel
    private Panel _settingsPanel;
    private VBoxContainer _settingsContainer;
    private HSlider _masterVolumeSlider;
    private HSlider _musicVolumeSlider;
    private HSlider _sfxVolumeSlider;
    private Button _settingsBackButton;

    private bool _isPaused = false;
    private AudioManager _audioManager;

    [Export] public string PauseAction { get; set; } = "ui_cancel"; // ESC key by default

    public override void _Ready()
    {
        // Get AudioManager reference
        _audioManager = GetNode<AudioManager>("/root/AudioManager");

        BuildPauseMenuUI();
        BuildSettingsUI();

        // Start hidden
        _pauseMenuContainer.Visible = false;
        _settingsPanel.Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        // Toggle pause with ESC key
        if (@event.IsActionPressed(PauseAction))
        {
            TogglePause();
            GetViewport().SetInputAsHandled();
        }
    }

    /// <summary>
    /// Build the main pause menu UI
    /// </summary>
    private void BuildPauseMenuUI()
    {
        // Main container
        _pauseMenuContainer = new Control();
        _pauseMenuContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_pauseMenuContainer);

        // Semi-transparent background
        _backgroundPanel = new Panel();
        _backgroundPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _pauseMenuContainer.AddChild(_backgroundPanel);

        // Apply dark overlay style
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0, 0, 0, 0.7f); // 70% transparent black
        _backgroundPanel.AddThemeStyleboxOverride("panel", styleBox);

        // Center container
        var centerContainer = new CenterContainer();
        centerContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _pauseMenuContainer.AddChild(centerContainer);

        // Menu panel
        var menuPanel = new Panel();
        menuPanel.CustomMinimumSize = new Vector2(400, 500);
        centerContainer.AddChild(menuPanel);

        var menuPanelStyle = new StyleBoxFlat();
        menuPanelStyle.BgColor = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        menuPanelStyle.BorderColor = new Color(0.4f, 0.4f, 0.5f);
        menuPanelStyle.SetBorderWidthAll(2);
        menuPanelStyle.SetCornerRadiusAll(8);
        menuPanel.AddThemeStyleboxOverride("panel", menuPanelStyle);

        // Menu content
        var menuVBox = new VBoxContainer();
        menuVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        menuVBox.AddThemeConstantOverride("separation", 10);
        menuPanel.AddChild(menuVBox);

        // Add margin
        var marginContainer = new MarginContainer();
        marginContainer.AddThemeConstantOverride("margin_left", 40);
        marginContainer.AddThemeConstantOverride("margin_right", 40);
        marginContainer.AddThemeConstantOverride("margin_top", 40);
        marginContainer.AddThemeConstantOverride("margin_bottom", 40);
        menuVBox.AddChild(marginContainer);

        _buttonContainer = new VBoxContainer();
        _buttonContainer.AddThemeConstantOverride("separation", 15);
        marginContainer.AddChild(_buttonContainer);

        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "GAME PAUSED";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 32);
        _buttonContainer.AddChild(_titleLabel);

        // Spacer
        var spacer1 = new Control();
        spacer1.CustomMinimumSize = new Vector2(0, 20);
        _buttonContainer.AddChild(spacer1);

        // Resume button
        _resumeButton = CreateMenuButton("Resume Game");
        _resumeButton.Pressed += OnResumePressed;
        _buttonContainer.AddChild(_resumeButton);

        // Settings button
        _settingsButton = CreateMenuButton("Settings");
        _settingsButton.Pressed += OnSettingsPressed;
        _buttonContainer.AddChild(_settingsButton);

        // Save button
        _saveButton = CreateMenuButton("Save Game");
        _saveButton.Pressed += OnSavePressed;
        _buttonContainer.AddChild(_saveButton);

        // Load button
        _loadButton = CreateMenuButton("Load Game");
        _loadButton.Pressed += OnLoadPressed;
        _buttonContainer.AddChild(_loadButton);

        // Restart button
        _restartButton = CreateMenuButton("Restart");
        _restartButton.Pressed += OnRestartPressed;
        _buttonContainer.AddChild(_restartButton);

        // Main Menu button
        _mainMenuButton = CreateMenuButton("Main Menu");
        _mainMenuButton.Pressed += OnMainMenuPressed;
        _buttonContainer.AddChild(_mainMenuButton);

        // Quit button
        _quitButton = CreateMenuButton("Quit Game");
        _quitButton.Pressed += OnQuitPressed;
        _buttonContainer.AddChild(_quitButton);
    }

    /// <summary>
    /// Build the settings UI panel
    /// </summary>
    private void BuildSettingsUI()
    {
        // Settings panel (overlays pause menu)
        _settingsPanel = new Panel();
        _settingsPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _pauseMenuContainer.AddChild(_settingsPanel);

        var settingsStyle = new StyleBoxFlat();
        settingsStyle.BgColor = new Color(0, 0, 0, 0.8f);
        _settingsPanel.AddThemeStyleboxOverride("panel", settingsStyle);

        // Center container
        var settingsCenterContainer = new CenterContainer();
        settingsCenterContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _settingsPanel.AddChild(settingsCenterContainer);

        // Settings content panel
        var settingsContentPanel = new Panel();
        settingsContentPanel.CustomMinimumSize = new Vector2(500, 400);
        settingsCenterContainer.AddChild(settingsContentPanel);

        var contentPanelStyle = new StyleBoxFlat();
        contentPanelStyle.BgColor = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        contentPanelStyle.BorderColor = new Color(0.4f, 0.4f, 0.5f);
        contentPanelStyle.SetBorderWidthAll(2);
        contentPanelStyle.SetCornerRadiusAll(8);
        settingsContentPanel.AddThemeStyleboxOverride("panel", contentPanelStyle);

        // Settings VBox
        var settingsVBox = new VBoxContainer();
        settingsVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        settingsContentPanel.AddChild(settingsVBox);

        // Margin
        var settingsMargin = new MarginContainer();
        settingsMargin.AddThemeConstantOverride("margin_left", 40);
        settingsMargin.AddThemeConstantOverride("margin_right", 40);
        settingsMargin.AddThemeConstantOverride("margin_top", 40);
        settingsMargin.AddThemeConstantOverride("margin_bottom", 40);
        settingsVBox.AddChild(settingsMargin);

        _settingsContainer = new VBoxContainer();
        _settingsContainer.AddThemeConstantOverride("separation", 20);
        settingsMargin.AddChild(_settingsContainer);

        // Title
        var settingsTitle = new Label();
        settingsTitle.Text = "SETTINGS";
        settingsTitle.HorizontalAlignment = HorizontalAlignment.Center;
        settingsTitle.AddThemeFontSizeOverride("font_size", 28);
        _settingsContainer.AddChild(settingsTitle);

        // Spacer
        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(0, 20);
        _settingsContainer.AddChild(spacer);

        // Master Volume
        _masterVolumeSlider = CreateVolumeSlider("Master Volume", _audioManager?.MasterVolume ?? 0.8f);
        _masterVolumeSlider.ValueChanged += OnMasterVolumeChanged;

        // Music Volume
        _musicVolumeSlider = CreateVolumeSlider("Music Volume", _audioManager?.MusicVolume ?? 0.7f);
        _musicVolumeSlider.ValueChanged += OnMusicVolumeChanged;

        // SFX Volume
        _sfxVolumeSlider = CreateVolumeSlider("SFX Volume", _audioManager?.SfxVolume ?? 0.8f);
        _sfxVolumeSlider.ValueChanged += OnSfxVolumeChanged;

        // Back button
        var spacer2 = new Control();
        spacer2.CustomMinimumSize = new Vector2(0, 20);
        _settingsContainer.AddChild(spacer2);

        _settingsBackButton = CreateMenuButton("Back");
        _settingsBackButton.Pressed += OnSettingsBackPressed;
        _settingsContainer.AddChild(_settingsBackButton);
    }

    /// <summary>
    /// Create a styled menu button
    /// </summary>
    private Button CreateMenuButton(string text)
    {
        var button = new Button();
        button.Text = text;
        button.CustomMinimumSize = new Vector2(300, 50);
        button.AddThemeFontSizeOverride("font_size", 18);

        // Normal style
        var normalStyle = new StyleBoxFlat();
        normalStyle.BgColor = new Color(0.2f, 0.25f, 0.3f);
        normalStyle.SetCornerRadiusAll(4);
        button.AddThemeStyleboxOverride("normal", normalStyle);

        // Hover style
        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = new Color(0.3f, 0.35f, 0.4f);
        hoverStyle.SetCornerRadiusAll(4);
        button.AddThemeStyleboxOverride("hover", hoverStyle);

        // Pressed style
        var pressedStyle = new StyleBoxFlat();
        pressedStyle.BgColor = new Color(0.15f, 0.2f, 0.25f);
        pressedStyle.SetCornerRadiusAll(4);
        button.AddThemeStyleboxOverride("pressed", pressedStyle);

        return button;
    }

    /// <summary>
    /// Create a volume slider with label
    /// </summary>
    private HSlider CreateVolumeSlider(string labelText, float initialValue)
    {
        var container = new VBoxContainer();
        container.AddThemeConstantOverride("separation", 5);
        _settingsContainer.AddChild(container);

        var label = new Label();
        label.Text = labelText;
        label.AddThemeFontSizeOverride("font_size", 16);
        container.AddChild(label);

        var slider = new HSlider();
        slider.MinValue = 0.0f;
        slider.MaxValue = 1.0f;
        slider.Step = 0.01f;
        slider.Value = initialValue;
        slider.CustomMinimumSize = new Vector2(400, 30);
        container.AddChild(slider);

        return slider;
    }

    /// <summary>
    /// Toggle pause state
    /// </summary>
    public void TogglePause()
    {
        SetPaused(!_isPaused);
    }

    /// <summary>
    /// Set pause state
    /// </summary>
    public void SetPaused(bool paused)
    {
        _isPaused = paused;
        _pauseMenuContainer.Visible = paused;
        _settingsPanel.Visible = false; // Hide settings when opening pause menu
        GetTree().Paused = paused;

        if (paused)
        {
            _audioManager?.PauseMusic(true);
            _audioManager?.PlaySfx("menu_open");
        }
        else
        {
            _audioManager?.PauseMusic(false);
            _audioManager?.PlaySfx("menu_close");
        }

        EmitSignal(SignalName.GamePaused, paused);
        GD.Print($"[PauseMenu] Game paused: {paused}");
    }

    /// <summary>
    /// Check if game is currently paused
    /// </summary>
    public bool IsPaused()
    {
        return _isPaused;
    }

    #region Button Handlers

    private void OnResumePressed()
    {
        _audioManager?.PlaySfx("button_click");
        SetPaused(false);
        EmitSignal(SignalName.ResumeRequested);
    }

    private void OnSettingsPressed()
    {
        _audioManager?.PlaySfx("button_click");
        _settingsPanel.Visible = true;
    }

    private void OnSavePressed()
    {
        _audioManager?.PlaySfx("button_click");
        var saveSystem = GetNodeOrNull<SaveLoadSystem>("/root/SaveLoadSystem");
        saveSystem?.SaveGame();
        GD.Print("[PauseMenu] Game saved");
    }

    private void OnLoadPressed()
    {
        _audioManager?.PlaySfx("button_click");
        var saveSystem = GetNodeOrNull<SaveLoadSystem>("/root/SaveLoadSystem");
        if (saveSystem != null && saveSystem.SaveFileExists())
        {
            saveSystem.LoadGame();
            SetPaused(false);
            GD.Print("[PauseMenu] Game loaded");
        }
        else
        {
            GD.Print("[PauseMenu] No save file found");
        }
    }

    private void OnRestartPressed()
    {
        _audioManager?.PlaySfx("button_click");
        SetPaused(false);
        EmitSignal(SignalName.RestartRequested);
        GetTree().ReloadCurrentScene();
    }

    private void OnMainMenuPressed()
    {
        _audioManager?.PlaySfx("button_click");
        SetPaused(false);
        EmitSignal(SignalName.MainMenuRequested);
        // Navigate to main menu scene (implement based on your project structure)
        // GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
    }

    private void OnQuitPressed()
    {
        _audioManager?.PlaySfx("button_click");
        GD.Print("[PauseMenu] Quitting game");
        GetTree().Quit();
    }

    private void OnSettingsBackPressed()
    {
        _audioManager?.PlaySfx("button_click");
        _settingsPanel.Visible = false;
    }

    #endregion

    #region Volume Handlers

    private void OnMasterVolumeChanged(double value)
    {
        _audioManager?.SetMasterVolume((float)value);
    }

    private void OnMusicVolumeChanged(double value)
    {
        _audioManager?.SetMusicVolume((float)value);
    }

    private void OnSfxVolumeChanged(double value)
    {
        _audioManager?.SetSfxVolume((float)value);
        _audioManager?.PlaySfx("button_click"); // Test SFX volume
    }

    #endregion
}
