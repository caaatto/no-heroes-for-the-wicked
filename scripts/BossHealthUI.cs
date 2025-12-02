using Godot;
using System;

/// <summary>
/// Boss health bar UI displayed at the top of the screen during boss fights
/// </summary>
public partial class BossHealthUI : CanvasLayer
{
    // UI Components
    private Control _container;
    private Panel _backgroundPanel;
    private Label _bossNameLabel;
    private Label _bossTitleLabel;
    private ProgressBar _healthBar;
    private Label _healthLabel;
    private HBoxContainer _phaseContainer;
    private Label _enrageLabel;

    // Boss reference
    private BossController _currentBoss;

    // Styling
    private readonly Color _healthColor = new Color(0.8f, 0.2f, 0.2f);
    private readonly Color _healthLowColor = new Color(1.0f, 0.0f, 0.0f);
    private readonly Color _enrageColor = new Color(1.0f, 0.3f, 0.0f);
    private readonly Color _phaseActiveColor = new Color(0.9f, 0.7f, 0.2f);
    private readonly Color _phaseInactiveColor = new Color(0.3f, 0.3f, 0.3f);

    public override void _Ready()
    {
        Layer = 100; // Render above everything else
        BuildUI();
        Hide();

        GD.Print("[BossHealthUI] Initialized");
    }

    /// <summary>
    /// Build the boss health bar UI
    /// </summary>
    private void BuildUI()
    {
        // Main container
        _container = new Control();
        _container.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        _container.OffsetBottom = 150;
        _container.MouseFilter = Control.MouseFilterEnum.Ignore;
        AddChild(_container);

        // Center container
        var centerContainer = new CenterContainer();
        centerContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _container.AddChild(centerContainer);

        // Main VBox
        var mainVBox = new VBoxContainer();
        mainVBox.CustomMinimumSize = new Vector2(800, 0);
        mainVBox.AddThemeConstantOverride("separation", 8);
        centerContainer.AddChild(mainVBox);

        // Add top spacing
        var topSpacer = new Control();
        topSpacer.CustomMinimumSize = new Vector2(0, 20);
        mainVBox.AddChild(topSpacer);

        // Background panel for boss info
        _backgroundPanel = new Panel();
        mainVBox.AddChild(_backgroundPanel);

        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        panelStyle.BorderColor = new Color(0.6f, 0.3f, 0.1f);
        panelStyle.SetBorderWidthAll(3);
        panelStyle.SetCornerRadiusAll(8);
        _backgroundPanel.AddThemeStyleboxOverride("panel", panelStyle);

        // Content VBox inside panel
        var contentVBox = new VBoxContainer();
        contentVBox.AddThemeConstantOverride("separation", 5);
        _backgroundPanel.AddChild(contentVBox);

        // Add margin
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 20);
        margin.AddThemeConstantOverride("margin_right", 20);
        margin.AddThemeConstantOverride("margin_top", 15);
        margin.AddThemeConstantOverride("margin_bottom", 15);
        contentVBox.AddChild(margin);

        var innerVBox = new VBoxContainer();
        innerVBox.AddThemeConstantOverride("separation", 5);
        margin.AddChild(innerVBox);

        // Boss name
        _bossNameLabel = new Label();
        _bossNameLabel.Text = "BOSS NAME";
        _bossNameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _bossNameLabel.AddThemeFontSizeOverride("font_size", 20);
        _bossNameLabel.AddThemeColorOverride("font_color", new Color(1.0f, 0.9f, 0.7f));
        _bossNameLabel.AddThemeColorOverride("font_shadow_color", Colors.Black);
        _bossNameLabel.AddThemeConstantOverride("shadow_offset_x", 2);
        _bossNameLabel.AddThemeConstantOverride("shadow_offset_y", 2);
        innerVBox.AddChild(_bossNameLabel);

        // Boss title
        _bossTitleLabel = new Label();
        _bossTitleLabel.Text = "The Unnamed";
        _bossTitleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _bossTitleLabel.AddThemeFontSizeOverride("font_size", 28);
        _bossTitleLabel.AddThemeColorOverride("font_color", new Color(1.0f, 0.7f, 0.2f));
        _bossTitleLabel.AddThemeColorOverride("font_shadow_color", Colors.Black);
        _bossTitleLabel.AddThemeConstantOverride("shadow_offset_x", 3);
        _bossTitleLabel.AddThemeConstantOverride("shadow_offset_y", 3);
        innerVBox.AddChild(_bossTitleLabel);

        // Health bar container
        var healthContainer = new VBoxContainer();
        healthContainer.AddThemeConstantOverride("separation", 3);
        innerVBox.AddChild(healthContainer);

        // Health bar
        _healthBar = new ProgressBar();
        _healthBar.CustomMinimumSize = new Vector2(760, 40);
        _healthBar.MaxValue = 100;
        _healthBar.Value = 100;
        _healthBar.ShowPercentage = false;
        healthContainer.AddChild(_healthBar);

        // Style health bar
        var healthBarStyle = new StyleBoxFlat();
        healthBarStyle.BgColor = new Color(0.2f, 0.2f, 0.2f);
        healthBarStyle.BorderColor = new Color(0.4f, 0.4f, 0.4f);
        healthBarStyle.SetBorderWidthAll(2);
        healthBarStyle.SetCornerRadiusAll(4);
        _healthBar.AddThemeStyleboxOverride("background", healthBarStyle);

        var healthBarFill = new StyleBoxFlat();
        healthBarFill.BgColor = _healthColor;
        healthBarFill.SetCornerRadiusAll(4);
        _healthBar.AddThemeStyleboxOverride("fill", healthBarFill);

        // Health label (centered on health bar)
        _healthLabel = new Label();
        _healthLabel.Text = "100 / 100";
        _healthLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _healthLabel.VerticalAlignment = VerticalAlignment.Center;
        _healthLabel.AddThemeFontSizeOverride("font_size", 18);
        _healthLabel.AddThemeColorOverride("font_color", Colors.White);
        _healthLabel.AddThemeColorOverride("font_shadow_color", Colors.Black);
        _healthLabel.AddThemeConstantOverride("shadow_offset_x", 2);
        _healthLabel.AddThemeConstantOverride("shadow_offset_y", 2);
        _healthLabel.Position = new Vector2(0, -32); // Overlay on health bar
        healthContainer.AddChild(_healthLabel);

        // Phase indicator container
        _phaseContainer = new HBoxContainer();
        _phaseContainer.AddThemeConstantOverride("separation", 10);
        _phaseContainer.Alignment = BoxContainer.AlignmentMode.Center;
        innerVBox.AddChild(_phaseContainer);

        // Enrage label
        _enrageLabel = new Label();
        _enrageLabel.Text = "⚠ ENRAGED ⚠";
        _enrageLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _enrageLabel.AddThemeFontSizeOverride("font_size", 24);
        _enrageLabel.AddThemeColorOverride("font_color", _enrageColor);
        _enrageLabel.AddThemeColorOverride("font_shadow_color", Colors.Black);
        _enrageLabel.AddThemeConstantOverride("shadow_offset_x", 2);
        _enrageLabel.AddThemeConstantOverride("shadow_offset_y", 2);
        _enrageLabel.Visible = false;
        innerVBox.AddChild(_enrageLabel);
    }

    /// <summary>
    /// Show the boss health UI for a specific boss
    /// </summary>
    public void ShowBoss(BossController boss)
    {
        if (boss == null)
        {
            GD.PrintErr("[BossHealthUI] Cannot show boss: boss is null");
            return;
        }

        _currentBoss = boss;
        _bossNameLabel.Text = boss.EnemyName.ToUpper();
        _bossTitleLabel.Text = boss.BossTitle;

        // Create phase indicators
        UpdatePhaseIndicators(boss.CurrentPhase, boss.PhaseCount);

        // Update health
        UpdateHealth();

        Show();
        GD.Print($"[BossHealthUI] Showing boss: {boss.BossTitle}");
    }

    /// <summary>
    /// Hide the boss health UI
    /// </summary>
    public new void Hide()
    {
        _currentBoss = null;
        _container.Visible = false;
        GD.Print("[BossHealthUI] Hidden");
    }

    /// <summary>
    /// Show the UI
    /// </summary>
    public new void Show()
    {
        _container.Visible = true;
    }

    /// <summary>
    /// Update health bar display
    /// </summary>
    private void UpdateHealth()
    {
        if (_currentBoss == null) return;

        float healthPercent = _currentBoss.GetHealthPercentage();
        _healthBar.Value = healthPercent * 100;
        _healthLabel.Text = $"{_currentBoss.CurrentLifePoints} / {_currentBoss.MaxLifePoints}";

        // Change color based on health
        var healthBarFill = new StyleBoxFlat();
        if (healthPercent <= 0.3f)
        {
            healthBarFill.BgColor = _healthLowColor;
        }
        else if (_currentBoss.IsEnraged)
        {
            healthBarFill.BgColor = _enrageColor;
        }
        else
        {
            healthBarFill.BgColor = _healthColor;
        }
        healthBarFill.SetCornerRadiusAll(4);
        _healthBar.AddThemeStyleboxOverride("fill", healthBarFill);

        // Show enrage indicator
        _enrageLabel.Visible = _currentBoss.IsEnraged;
    }

    /// <summary>
    /// Update phase indicator display
    /// </summary>
    private void UpdatePhaseIndicators(int currentPhase, int totalPhases)
    {
        // Clear existing indicators
        foreach (Node child in _phaseContainer.GetChildren())
        {
            child.QueueFree();
        }

        // Create phase indicators
        for (int i = 1; i <= totalPhases; i++)
        {
            var phasePanel = new Panel();
            phasePanel.CustomMinimumSize = new Vector2(60, 10);

            var phaseStyle = new StyleBoxFlat();
            phaseStyle.BgColor = i <= currentPhase ? _phaseActiveColor : _phaseInactiveColor;
            phaseStyle.SetCornerRadiusAll(3);
            phasePanel.AddThemeStyleboxOverride("panel", phaseStyle);

            _phaseContainer.AddChild(phasePanel);

            // Add phase label
            var phaseLabel = new Label();
            phaseLabel.Text = $"Phase {i}";
            phaseLabel.HorizontalAlignment = HorizontalAlignment.Center;
            phaseLabel.VerticalAlignment = VerticalAlignment.Center;
            phaseLabel.AddThemeFontSizeOverride("font_size", 10);
            phaseLabel.AddThemeColorOverride("font_color", i <= currentPhase ? Colors.Black : new Color(0.6f, 0.6f, 0.6f));
            phaseLabel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            phasePanel.AddChild(phaseLabel);
        }
    }

    /// <summary>
    /// Update display when boss phase changes
    /// </summary>
    public void OnPhaseChanged(int newPhase, int totalPhases)
    {
        UpdatePhaseIndicators(newPhase, totalPhases);
        GD.Print($"[BossHealthUI] Phase changed to {newPhase}");
    }

    public override void _Process(double delta)
    {
        if (_currentBoss != null && _container.Visible)
        {
            UpdateHealth();

            // Hide if boss is dead
            if (!_currentBoss.IsAlive)
            {
                Hide();
            }
        }
    }
}
