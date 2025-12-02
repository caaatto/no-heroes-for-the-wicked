using Godot;
using System;

/// <summary>
/// Credits screen with scrolling text
/// Shows team members, contributors, and special thanks
/// </summary>
public partial class CreditsScreen : Control
{
    private ScrollContainer _scrollContainer;
    private VBoxContainer _creditsContainer;
    private Button _backButton;

    private LocalizationManager _localization;
    private AudioManager _audioManager;
    private MainMenu _mainMenu;

    private bool _autoScroll = true;
    private float _scrollSpeed = 30.0f;

    public override void _Ready()
    {
        // Get system references
        _localization = GetNodeOrNull<LocalizationManager>("/root/LocalizationManager");
        _audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
        _mainMenu = GetNodeOrNull<MainMenu>("..");

        // Get UI references
        _scrollContainer = GetNodeOrNull<ScrollContainer>("ScrollContainer");
        _creditsContainer = GetNodeOrNull<VBoxContainer>("ScrollContainer/CreditsContainer");
        _backButton = GetNodeOrNull<Button>("BackButton");

        // Connect signals
        if (_backButton != null)
            _backButton.Pressed += OnBackPressed;

        // Build credits content
        BuildCredits();

        GD.Print("CreditsScreen ready");
    }

    private void BuildCredits()
    {
        if (_creditsContainer == null || _localization == null)
            return;

        // Clear existing content
        foreach (Node child in _creditsContainer.GetChildren())
        {
            child.QueueFree();
        }

        // Title
        AddTitle(_localization.GetText("credits_title"));
        AddSpacing(40);

        // Game Info
        AddSection(_localization.GetText("menu_title"));
        AddText("Ein storygetriebenes 2D-Action-Adventure");
        AddSpacing(40);

        // Team
        AddSection(_localization.GetText("credits_developed_by"));
        AddText("caaatto");
        AddSpacing(30);

        // Programming
        AddSection(_localization.GetText("credits_programming"));
        AddText("caaatto");
        AddText("Claude Code (AI Assistant)");
        AddSpacing(30);

        // Art
        AddSection(_localization.GetText("credits_art"));
        AddText("Placeholder Assets");
        AddText("(To be replaced with custom pixel art)");
        AddSpacing(30);

        // Music & Sound
        AddSection(_localization.GetText("credits_music"));
        AddText("Placeholder Audio");
        AddText("(To be replaced with custom soundtrack)");
        AddSpacing(30);

        // Game Design
        AddSection("Game Design");
        AddText("caaatto");
        AddSpacing(30);

        // Special Thanks
        AddSection(_localization.GetText("credits_special_thanks"));
        AddText("Godot Engine Community");
        AddText("All Playtesters");
        AddText("Open Source Contributors");
        AddSpacing(40);

        // Tools & Technologies
        AddSection("Tools & Technologies");
        AddText("Godot Engine 4.x");
        AddText("C# / .NET");
        AddText("Aseprite (Pixel Art)");
        AddText("Git / GitHub");
        AddSpacing(40);

        // Footer
        AddSection("© 2025");
        AddText("Made with ❤ and Godot");
        AddSpacing(60);

        // Update back button text
        if (_backButton != null)
            _backButton.Text = _localization.GetText("settings_back");
    }

    private void AddTitle(string text)
    {
        var label = new Label();
        label.Text = text;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.AddThemeColorOverride("font_color", Colors.Gold);
        label.AddThemeFontSizeOverride("font_size", 48);
        _creditsContainer.AddChild(label);
    }

    private void AddSection(string text)
    {
        var label = new Label();
        label.Text = text;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.AddThemeColorOverride("font_color", Colors.LightCyan);
        label.AddThemeFontSizeOverride("font_size", 32);
        _creditsContainer.AddChild(label);
    }

    private void AddText(string text)
    {
        var label = new Label();
        label.Text = text;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.AddThemeColorOverride("font_color", Colors.White);
        label.AddThemeFontSizeOverride("font_size", 24);
        _creditsContainer.AddChild(label);
    }

    private void AddSpacing(float height)
    {
        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(0, height);
        _creditsContainer.AddChild(spacer);
    }

    public override void _Process(double delta)
    {
        if (!_autoScroll || _scrollContainer == null)
            return;

        // Auto-scroll credits
        float currentScroll = _scrollContainer.ScrollVertical;
        float maxScroll = _scrollContainer.GetVScrollBar().MaxValue;

        if (currentScroll < maxScroll)
        {
            _scrollContainer.ScrollVertical = (int)(currentScroll + _scrollSpeed * delta);
        }
        else
        {
            // Reached the end, reset after a delay
            _autoScroll = false;
            GetTree().CreateTimer(3.0).Timeout += ResetScroll;
        }
    }

    private void ResetScroll()
    {
        if (_scrollContainer != null)
        {
            _scrollContainer.ScrollVertical = 0;
            _autoScroll = true;
        }
    }

    public override void _Input(InputEvent @event)
    {
        // Manual scroll with mouse wheel or arrow keys disables auto-scroll temporarily
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.WheelUp ||
                mouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                _autoScroll = false;
                GetTree().CreateTimer(2.0).Timeout += () => _autoScroll = true;
            }
        }

        if (@event.IsActionPressed("ui_up") || @event.IsActionPressed("ui_down"))
        {
            _autoScroll = false;
            GetTree().CreateTimer(2.0).Timeout += () => _autoScroll = true;
        }

        // ESC or Back action to exit
        if (@event.IsActionPressed("ui_cancel"))
        {
            OnBackPressed();
            GetViewport().SetInputAsHandled();
        }
    }

    private void OnBackPressed()
    {
        if (_audioManager != null)
            _audioManager.PlaySfx("button_click");

        if (_mainMenu != null)
            _mainMenu.OnBackToMainMenu();
    }
}
