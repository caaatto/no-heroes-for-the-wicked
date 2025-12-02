using Godot;
using System;

/// <summary>
/// Enhanced main menu with full navigation:
/// New Game, Continue, Settings, Credits, Quit
/// Supports localization and gamepad input
/// </summary>
public partial class MainMenu : Control
{
    // Button references
    private Button _newGameButton;
    private Button _continueButton;
    private Button _settingsButton;
    private Button _creditsButton;
    private Button _quitButton;

    // System references
    private SaveLoadSystem _saveLoadSystem;
    private LocalizationManager _localization;
    private AudioManager _audioManager;

    // Menu state
    private Control _mainMenuPanel;
    private Control _settingsPanel;
    private Control _creditsPanel;

    public override void _Ready()
    {
        // Get system references
        _saveLoadSystem = GetNodeOrNull<SaveLoadSystem>("/root/SaveLoadSystem");
        _localization = GetNodeOrNull<LocalizationManager>("/root/LocalizationManager");
        _audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");

        // Get panel references (not needed for simple main menu)
        _mainMenuPanel = GetNodeOrNull<Control>("MainMenuPanel");
        _settingsPanel = GetNodeOrNull<Control>("SettingsPanel");
        _creditsPanel = GetNodeOrNull<Control>("CreditsPanel");

        // Get button references from the actual scene structure
        _newGameButton = GetNodeOrNull<Button>("MenuContainer/NewGameButton");
        _continueButton = GetNodeOrNull<Button>("MenuContainer/LoadGameButton");
        _settingsButton = GetNodeOrNull<Button>("MenuContainer/SettingsButton");
        _creditsButton = GetNodeOrNull<Button>("MenuContainer/CreditsButton");
        _quitButton = GetNodeOrNull<Button>("MenuContainer/QuitButton");

        // Connect button signals
        ConnectButtons();

        // Update UI with localization
        UpdateLocalization();

        // Subscribe to language changes
        if (_localization != null)
        {
            _localization.LanguageChanged += OnLanguageChanged;
        }

        // Show main menu panel by default (if panels exist)
        if (_mainMenuPanel != null)
        {
            ShowMainMenu();
        }

        // Play menu music
        if (_audioManager != null)
        {
            _audioManager.PlayMusic("main_menu", loop: true);
        }

        // Check for save file and enable/disable continue button
        UpdateContinueButton();

        GD.Print("Main Menu ready - Buttons: NewGame=" + (_newGameButton != null) +
                 ", Continue=" + (_continueButton != null) +
                 ", Quit=" + (_quitButton != null));
    }

    private void ConnectButtons()
    {
        if (_newGameButton != null)
        {
            _newGameButton.Pressed += OnNewGamePressed;
            _newGameButton.MouseEntered += () => PlayHoverSound();
        }

        if (_continueButton != null)
        {
            _continueButton.Pressed += OnContinuePressed;
            _continueButton.MouseEntered += () => PlayHoverSound();
        }

        if (_settingsButton != null)
        {
            _settingsButton.Pressed += OnSettingsPressed;
            _settingsButton.MouseEntered += () => PlayHoverSound();
        }

        if (_creditsButton != null)
        {
            _creditsButton.Pressed += OnCreditsPressed;
            _creditsButton.MouseEntered += () => PlayHoverSound();
        }

        if (_quitButton != null)
        {
            _quitButton.Pressed += OnQuitPressed;
            _quitButton.MouseEntered += () => PlayHoverSound();
        }
    }

    private void UpdateContinueButton()
    {
        if (_continueButton == null) return;

        bool hasSave = _saveLoadSystem != null && _saveLoadSystem.SaveFileExists();
        _continueButton.Disabled = !hasSave;
    }

    private void UpdateLocalization()
    {
        // Localization is optional - buttons have default German text in scene
        if (_localization == null)
        {
            GD.Print("Localization not available, using default button text");
            return;
        }

        if (_newGameButton != null)
            _newGameButton.Text = _localization.GetText("menu_new_game");

        if (_continueButton != null)
            _continueButton.Text = _localization.GetText("menu_continue");

        if (_settingsButton != null)
            _settingsButton.Text = _localization.GetText("menu_settings");

        if (_creditsButton != null)
            _creditsButton.Text = _localization.GetText("menu_credits");

        if (_quitButton != null)
            _quitButton.Text = _localization.GetText("menu_quit");
    }

    private void OnLanguageChanged(string languageCode)
    {
        UpdateLocalization();
    }

    private void ShowMainMenu()
    {
        if (_mainMenuPanel != null) _mainMenuPanel.Visible = true;
        if (_settingsPanel != null) _settingsPanel.Visible = false;
        if (_creditsPanel != null) _creditsPanel.Visible = false;
    }

    private void ShowSettings()
    {
        if (_mainMenuPanel != null) _mainMenuPanel.Visible = false;
        if (_settingsPanel != null) _settingsPanel.Visible = true;
        if (_creditsPanel != null) _creditsPanel.Visible = false;
    }

    private void ShowCredits()
    {
        if (_mainMenuPanel != null) _mainMenuPanel.Visible = false;
        if (_settingsPanel != null) _settingsPanel.Visible = false;
        if (_creditsPanel != null) _creditsPanel.Visible = true;
    }

    // Button callbacks
    private void OnNewGamePressed()
    {
        PlayClickSound();
        GD.Print("Starting new game...");

        // TODO: Add tutorial scene later
        // For now, go directly to hub
        GetTree().ChangeSceneToFile("res://scenes/hub.tscn");
    }

    private void OnContinuePressed()
    {
        PlayClickSound();

        if (_saveLoadSystem != null && _saveLoadSystem.SaveFileExists())
        {
            GD.Print("Loading saved game...");
            _saveLoadSystem.LoadGame();
        }
        else
        {
            GD.PrintErr("No save file found!");
        }
    }

    private void OnSettingsPressed()
    {
        PlayClickSound();
        ShowSettings();
    }

    private void OnCreditsPressed()
    {
        PlayClickSound();
        ShowCredits();
    }

    private void OnQuitPressed()
    {
        PlayClickSound();
        GD.Print("Quitting game...");
        GetTree().Quit();
    }

    // Called from Settings/Credits panels
    public void OnBackToMainMenu()
    {
        PlayClickSound();
        ShowMainMenu();
        UpdateContinueButton(); // Update in case settings changed
    }

    private void PlayClickSound()
    {
        if (_audioManager != null)
        {
            _audioManager.PlaySfx("button_click");
        }
    }

    private void PlayHoverSound()
    {
        if (_audioManager != null)
        {
            _audioManager.PlaySfx("button_hover");
        }
    }

    public override void _Input(InputEvent @event)
    {
        // ESC to go back from sub-menus
        if (@event.IsActionPressed("ui_cancel"))
        {
            if (_settingsPanel != null && _settingsPanel.Visible)
            {
                OnBackToMainMenu();
                GetViewport().SetInputAsHandled();
            }
            else if (_creditsPanel != null && _creditsPanel.Visible)
            {
                OnBackToMainMenu();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    public override void _ExitTree()
    {
        // Unsubscribe from events
        if (_localization != null)
        {
            _localization.LanguageChanged -= OnLanguageChanged;
        }
    }
}
