using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Dialogue system for NPC conversations and story events.
/// Supports branching dialogues, choices, and character portraits.
/// </summary>
public partial class DialogueSystem : CanvasLayer
{
    [Signal]
    public delegate void DialogueStartedEventHandler(string dialogueId);

    [Signal]
    public delegate void DialogueEndedEventHandler();

    [Signal]
    public delegate void ChoiceSelectedEventHandler(int choiceIndex, string choiceText);

    // UI References
    private Control _dialogueContainer;
    private Panel _dialogueBox;
    private Label _characterNameLabel;
    private Label _dialogueTextLabel;
    private TextureRect _portraitTexture;
    private VBoxContainer _choicesContainer;
    private Button _continueButton;

    // Dialogue state
    private DialogueData _currentDialogue;
    private int _currentLineIndex = 0;
    private bool _isActive = false;
    private bool _isTyping = false;
    private float _typewriterSpeed = 0.05f;
    private string _currentFullText = "";
    private int _currentCharIndex = 0;
    private float _typewriterTimer = 0.0f;

    private AudioManager _audioManager;

    [Export] public bool UseTypewriterEffect { get; set; } = true;
    [Export] public float TypewriterSpeed { get; set; } = 0.05f;

    public override void _Ready()
    {
        _audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");

        BuildDialogueUI();

        // Start hidden
        _dialogueContainer.Visible = false;

        GD.Print("[DialogueSystem] Dialogue system initialized");
    }

    public override void _Process(double delta)
    {
        if (_isTyping)
        {
            ProcessTypewriter(delta);
        }

        // Skip typewriter with spacebar
        if (_isTyping && Input.IsActionJustPressed("ui_accept"))
        {
            SkipTypewriter();
        }
    }

    /// <summary>
    /// Build the dialogue UI
    /// </summary>
    private void BuildDialogueUI()
    {
        // Main container
        _dialogueContainer = new Control();
        _dialogueContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_dialogueContainer);

        // Position dialogue box at bottom
        _dialogueBox = new Panel();
        _dialogueBox.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
        _dialogueBox.OffsetTop = -200;
        _dialogueBox.CustomMinimumSize = new Vector2(0, 200);
        _dialogueContainer.AddChild(_dialogueBox);

        var boxStyle = new StyleBoxFlat();
        boxStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        boxStyle.BorderColor = new Color(0.5f, 0.5f, 0.6f);
        boxStyle.SetBorderWidthAll(3);
        boxStyle.SetCornerRadiusAll(8);
        _dialogueBox.AddThemeStyleboxOverride("panel", boxStyle);

        // Main HBox for portrait and text
        var mainHBox = new HBoxContainer();
        mainHBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _dialogueBox.AddChild(mainHBox);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 20);
        margin.AddThemeConstantOverride("margin_right", 20);
        margin.AddThemeConstantOverride("margin_top", 15);
        margin.AddThemeConstantOverride("margin_bottom", 15);
        mainHBox.AddChild(margin);

        var contentHBox = new HBoxContainer();
        contentHBox.AddThemeConstantOverride("separation", 20);
        margin.AddChild(contentHBox);

        // Portrait
        _portraitTexture = new TextureRect();
        _portraitTexture.CustomMinimumSize = new Vector2(120, 120);
        _portraitTexture.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        contentHBox.AddChild(_portraitTexture);

        // Text VBox
        var textVBox = new VBoxContainer();
        textVBox.AddThemeConstantOverride("separation", 10);
        textVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        contentHBox.AddChild(textVBox);

        // Character name
        _characterNameLabel = new Label();
        _characterNameLabel.Text = "Character";
        _characterNameLabel.AddThemeFontSizeOverride("font_size", 20);
        _characterNameLabel.Modulate = new Color(1, 0.9f, 0.6f);
        textVBox.AddChild(_characterNameLabel);

        // Dialogue text
        _dialogueTextLabel = new Label();
        _dialogueTextLabel.Text = "Dialogue text goes here...";
        _dialogueTextLabel.AddThemeFontSizeOverride("font_size", 16);
        _dialogueTextLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        _dialogueTextLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        textVBox.AddChild(_dialogueTextLabel);

        // Choices container
        _choicesContainer = new VBoxContainer();
        _choicesContainer.AddThemeConstantOverride("separation", 8);
        textVBox.AddChild(_choicesContainer);

        // Continue button
        var buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.AlignmentMode.End;
        textVBox.AddChild(buttonContainer);

        _continueButton = new Button();
        _continueButton.Text = "Continue >";
        _continueButton.CustomMinimumSize = new Vector2(120, 40);
        _continueButton.Pressed += OnContinuePressed;
        buttonContainer.AddChild(_continueButton);
    }

    /// <summary>
    /// Start a dialogue
    /// </summary>
    public void StartDialogue(DialogueData dialogueData)
    {
        if (dialogueData == null || dialogueData.Lines.Count == 0)
        {
            GD.PrintErr("[DialogueSystem] Invalid dialogue data");
            return;
        }

        _currentDialogue = dialogueData;
        _currentLineIndex = 0;
        _isActive = true;

        _dialogueContainer.Visible = true;
        _audioManager?.PlaySfx("menu_open");

        ShowCurrentLine();

        EmitSignal(SignalName.DialogueStarted, dialogueData.Id);
        GD.Print($"[DialogueSystem] Started dialogue: {dialogueData.Id}");
    }

    /// <summary>
    /// Show the current dialogue line
    /// </summary>
    private void ShowCurrentLine()
    {
        if (_currentDialogue == null || _currentLineIndex >= _currentDialogue.Lines.Count)
        {
            EndDialogue();
            return;
        }

        var line = _currentDialogue.Lines[_currentLineIndex];

        // Set character name
        _characterNameLabel.Text = line.CharacterName;

        // Set portrait (if exists)
        // _portraitTexture.Texture = line.Portrait; // Would load portrait texture

        // Display text
        _currentFullText = line.Text;

        if (UseTypewriterEffect)
        {
            StartTypewriter();
        }
        else
        {
            _dialogueTextLabel.Text = _currentFullText;
        }

        // Handle choices
        ClearChoices();

        if (line.Choices != null && line.Choices.Count > 0)
        {
            _continueButton.Visible = false;
            ShowChoices(line.Choices);
        }
        else
        {
            _continueButton.Visible = true;
        }
    }

    /// <summary>
    /// Start typewriter effect
    /// </summary>
    private void StartTypewriter()
    {
        _isTyping = true;
        _currentCharIndex = 0;
        _typewriterTimer = 0.0f;
        _dialogueTextLabel.Text = "";
    }

    /// <summary>
    /// Process typewriter effect
    /// </summary>
    private void ProcessTypewriter(double delta)
    {
        _typewriterTimer -= (float)delta;

        if (_typewriterTimer <= 0)
        {
            _typewriterTimer = TypewriterSpeed;

            if (_currentCharIndex < _currentFullText.Length)
            {
                _currentCharIndex++;
                _dialogueTextLabel.Text = _currentFullText.Substring(0, _currentCharIndex);

                // Play typing sound
                if (_currentCharIndex % 3 == 0) // Every 3rd character
                {
                    _audioManager?.PlaySfx("button_hover", 0.2f, -10.0f);
                }
            }
            else
            {
                _isTyping = false;
            }
        }
    }

    /// <summary>
    /// Skip typewriter effect
    /// </summary>
    private void SkipTypewriter()
    {
        _isTyping = false;
        _dialogueTextLabel.Text = _currentFullText;
    }

    /// <summary>
    /// Show dialogue choices
    /// </summary>
    private void ShowChoices(List<DialogueChoice> choices)
    {
        for (int i = 0; i < choices.Count; i++)
        {
            var choice = choices[i];
            var choiceButton = new Button();
            choiceButton.Text = choice.Text;
            choiceButton.CustomMinimumSize = new Vector2(0, 40);
            choiceButton.AddThemeFontSizeOverride("font_size", 14);

            int choiceIndex = i; // Capture for lambda
            choiceButton.Pressed += () => OnChoiceSelected(choiceIndex, choice);

            _choicesContainer.AddChild(choiceButton);
        }
    }

    /// <summary>
    /// Clear dialogue choices
    /// </summary>
    private void ClearChoices()
    {
        foreach (var child in _choicesContainer.GetChildren())
        {
            child.QueueFree();
        }
    }

    /// <summary>
    /// Handle continue button pressed
    /// </summary>
    private void OnContinuePressed()
    {
        if (_isTyping)
        {
            SkipTypewriter();
            return;
        }

        _audioManager?.PlaySfx("button_click");

        _currentLineIndex++;
        ShowCurrentLine();
    }

    /// <summary>
    /// Handle choice selected
    /// </summary>
    private void OnChoiceSelected(int choiceIndex, DialogueChoice choice)
    {
        _audioManager?.PlaySfx("button_click");

        EmitSignal(SignalName.ChoiceSelected, choiceIndex, choice.Text);

        // Navigate to next dialogue or end
        if (!string.IsNullOrEmpty(choice.NextDialogueId))
        {
            // Would load and start new dialogue
            GD.Print($"[DialogueSystem] Choice leads to dialogue: {choice.NextDialogueId}");
            EndDialogue();
        }
        else
        {
            _currentLineIndex++;
            ShowCurrentLine();
        }
    }

    /// <summary>
    /// End dialogue
    /// </summary>
    public void EndDialogue()
    {
        _isActive = false;
        _dialogueContainer.Visible = false;
        _currentDialogue = null;
        _currentLineIndex = 0;

        _audioManager?.PlaySfx("menu_close");

        EmitSignal(SignalName.DialogueEnded);
        GD.Print("[DialogueSystem] Dialogue ended");
    }

    /// <summary>
    /// Check if dialogue is active
    /// </summary>
    public bool IsDialogueActive()
    {
        return _isActive;
    }
}

/// <summary>
/// Dialogue data container
/// </summary>
public class DialogueData
{
    public string Id { get; set; }
    public List<DialogueLine> Lines { get; set; } = new List<DialogueLine>();
}

/// <summary>
/// Single dialogue line
/// </summary>
public class DialogueLine
{
    public string CharacterName { get; set; }
    public string Text { get; set; }
    public Texture2D Portrait { get; set; }
    public List<DialogueChoice> Choices { get; set; }
}

/// <summary>
/// Dialogue choice option
/// </summary>
public class DialogueChoice
{
    public string Text { get; set; }
    public string NextDialogueId { get; set; }
}
