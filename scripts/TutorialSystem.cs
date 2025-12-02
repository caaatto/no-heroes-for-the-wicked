using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Interactive tutorial system for new players
/// Shows step-by-step instructions for movement, combat, inventory, etc.
/// Can be skipped at any time
/// </summary>
public partial class TutorialSystem : CanvasLayer
{
    [Signal]
    public delegate void TutorialCompletedEventHandler();

    [Signal]
    public delegate void TutorialSkippedEventHandler();

    // UI Elements
    private Control _tutorialPanel;
    private Label _titleLabel;
    private Label _instructionLabel;
    private Button _skipButton;
    private Button _nextButton;
    private ProgressBar _progressBar;

    // System references
    private LocalizationManager _localization;
    private AudioManager _audioManager;

    // Tutorial state
    private int _currentStep = 0;
    private List<TutorialStep> _tutorialSteps;
    private bool _tutorialActive = false;

    public override void _Ready()
    {
        // Get system references
        _localization = GetNodeOrNull<LocalizationManager>("/root/LocalizationManager");
        _audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");

        // Get UI references
        _tutorialPanel = GetNodeOrNull<Control>("TutorialPanel");
        _titleLabel = GetNodeOrNull<Label>("TutorialPanel/TitleLabel");
        _instructionLabel = GetNodeOrNull<Label>("TutorialPanel/InstructionLabel");
        _skipButton = GetNodeOrNull<Button>("TutorialPanel/SkipButton");
        _nextButton = GetNodeOrNull<Button>("TutorialPanel/NextButton");
        _progressBar = GetNodeOrNull<ProgressBar>("TutorialPanel/ProgressBar");

        // Connect signals
        if (_skipButton != null)
            _skipButton.Pressed += OnSkipPressed;

        if (_nextButton != null)
            _nextButton.Pressed += OnNextPressed;

        // Initialize tutorial steps
        InitializeTutorialSteps();

        // Hide by default
        if (_tutorialPanel != null)
            _tutorialPanel.Visible = false;

        GD.Print("TutorialSystem ready");
    }

    private void InitializeTutorialSteps()
    {
        _tutorialSteps = new List<TutorialStep>
        {
            new TutorialStep
            {
                TitleKey = "tutorial_welcome",
                InstructionKey = "tutorial_welcome",
                RequiredAction = "",
                CanSkip = true
            },
            new TutorialStep
            {
                TitleKey = "tutorial_movement",
                InstructionKey = "tutorial_movement",
                RequiredAction = "move",
                CanSkip = false
            },
            new TutorialStep
            {
                TitleKey = "tutorial_attack",
                InstructionKey = "tutorial_attack",
                RequiredAction = "attack",
                CanSkip = false
            },
            new TutorialStep
            {
                TitleKey = "tutorial_interact",
                InstructionKey = "tutorial_interact",
                RequiredAction = "interact",
                CanSkip = false
            },
            new TutorialStep
            {
                TitleKey = "tutorial_inventory",
                InstructionKey = "tutorial_inventory",
                RequiredAction = "inventory",
                CanSkip = false
            },
            new TutorialStep
            {
                TitleKey = "tutorial_pause",
                InstructionKey = "tutorial_pause",
                RequiredAction = "ui_cancel",
                CanSkip = false
            },
            new TutorialStep
            {
                TitleKey = "tutorial_complete",
                InstructionKey = "tutorial_complete",
                RequiredAction = "",
                CanSkip = true
            }
        };
    }

    /// <summary>
    /// Start the tutorial from the beginning
    /// </summary>
    public void StartTutorial()
    {
        _currentStep = 0;
        _tutorialActive = true;

        if (_tutorialPanel != null)
            _tutorialPanel.Visible = true;

        ShowCurrentStep();
        GD.Print("Tutorial started");
    }

    /// <summary>
    /// Stop and hide the tutorial
    /// </summary>
    public void StopTutorial()
    {
        _tutorialActive = false;

        if (_tutorialPanel != null)
            _tutorialPanel.Visible = false;

        GD.Print("Tutorial stopped");
    }

    private void ShowCurrentStep()
    {
        if (_currentStep >= _tutorialSteps.Count)
        {
            CompleteTutorial();
            return;
        }

        var step = _tutorialSteps[_currentStep];

        // Update UI
        if (_titleLabel != null && _localization != null)
            _titleLabel.Text = _localization.GetText(step.TitleKey);

        if (_instructionLabel != null && _localization != null)
            _instructionLabel.Text = _localization.GetText(step.InstructionKey);

        if (_progressBar != null)
        {
            _progressBar.MaxValue = _tutorialSteps.Count;
            _progressBar.Value = _currentStep + 1;
        }

        // Show/hide next button based on step
        if (_nextButton != null)
        {
            _nextButton.Visible = string.IsNullOrEmpty(step.RequiredAction) || step.CanSkip;
        }

        // Update skip button text
        if (_skipButton != null && _localization != null)
        {
            _skipButton.Text = _localization.GetText("tutorial_skip");
        }

        GD.Print($"Tutorial step {_currentStep + 1}/{_tutorialSteps.Count}: {step.TitleKey}");
    }

    private void NextStep()
    {
        _currentStep++;

        if (_currentStep >= _tutorialSteps.Count)
        {
            CompleteTutorial();
        }
        else
        {
            ShowCurrentStep();
        }
    }

    private void CompleteTutorial()
    {
        GD.Print("Tutorial completed!");
        _tutorialActive = false;

        EmitSignal(SignalName.TutorialCompleted);

        if (_audioManager != null)
            _audioManager.PlaySfx("quest_complete");

        StopTutorial();

        // Load hub scene
        CallDeferred(MethodName.LoadHubScene);
    }

    private void LoadHubScene()
    {
        GetTree().ChangeSceneToFile("res://scenes/hub.tscn");
    }

    public override void _Input(InputEvent @event)
    {
        if (!_tutorialActive || _currentStep >= _tutorialSteps.Count)
            return;

        var step = _tutorialSteps[_currentStep];

        // Check if required action is performed
        if (!string.IsNullOrEmpty(step.RequiredAction))
        {
            if (@event.IsActionPressed(step.RequiredAction))
            {
                GD.Print($"Tutorial action '{step.RequiredAction}' performed");
                NextStep();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    private void OnSkipPressed()
    {
        GD.Print("Tutorial skipped by player");

        if (_audioManager != null)
            _audioManager.PlaySfx("button_click");

        _tutorialActive = false;
        EmitSignal(SignalName.TutorialSkipped);
        StopTutorial();

        // Load hub scene
        CallDeferred(MethodName.LoadHubScene);
    }

    private void OnNextPressed()
    {
        if (_audioManager != null)
            _audioManager.PlaySfx("button_click");

        NextStep();
    }

    /// <summary>
    /// Check if a specific step is completed
    /// </summary>
    public bool IsStepCompleted(int stepIndex)
    {
        return stepIndex < _currentStep;
    }

    /// <summary>
    /// Get current tutorial progress (0-1)
    /// </summary>
    public float GetProgress()
    {
        if (_tutorialSteps.Count == 0)
            return 1.0f;

        return (float)_currentStep / _tutorialSteps.Count;
    }

    /// <summary>
    /// Check if tutorial is currently active
    /// </summary>
    public bool IsTutorialActive()
    {
        return _tutorialActive;
    }
}

/// <summary>
/// Data structure for a single tutorial step
/// </summary>
public class TutorialStep
{
    public string TitleKey { get; set; }
    public string InstructionKey { get; set; }
    public string RequiredAction { get; set; } // Empty if no action required
    public bool CanSkip { get; set; }
}
