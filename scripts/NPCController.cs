using Godot;
using System;
using System.Collections.Generic;

public partial class NPCController : CharacterBody2D
{
    [Export] public string NpcName { get; set; } = "Bartender";
    [Export] public string NpcId { get; set; } = "bartender";
    [Export] public string[] DialogueLines { get; set; }

    private Label _nameLabel;
    private Label _interactionHint;
    private bool _playerNearby = false;
    private PlayerController _nearbyPlayer;
    private Area2D _interactionArea;
    private DialogueSystem _dialogueSystem;
    private QuestSystem _questSystem;

    private int _currentDialogueIndex = 0;
    private bool _hasInteracted = false;

    public override void _Ready()
    {
        AddToGroup("npcs");

        _nameLabel = GetNodeOrNull<Label>("NameLabel");
        _interactionHint = GetNodeOrNull<Label>("InteractionHint");
        _interactionArea = GetNodeOrNull<Area2D>("InteractionArea");

        if (_nameLabel != null)
            _nameLabel.Text = NpcName;

        if (_interactionHint != null)
            _interactionHint.Visible = false;

        // Setup default dialogue if none provided
        if (DialogueLines == null || DialogueLines.Length == 0)
        {
            DialogueLines = new string[]
            {
                $"Hallo! Ich bin {NpcName}.",
                "Willkommen in No Heroes for the Wicked!",
                "Viel Glück auf deiner Reise!"
            };
        }

        // Connect interaction area
        if (_interactionArea != null)
        {
            _interactionArea.BodyEntered += OnPlayerEntered;
            _interactionArea.BodyExited += OnPlayerExited;
        }

        _questSystem = GetNodeOrNull<QuestSystem>("/root/QuestSystem");

        GD.Print($"NPC '{NpcName}' ready");
    }

    public override void _Process(double delta)
    {
        if (_playerNearby && Input.IsActionJustPressed("interact"))
        {
            Interact();
        }
    }

    private void OnPlayerEntered(Node2D body)
    {
        if (body is PlayerController player)
        {
            _playerNearby = true;
            _nearbyPlayer = player;

            if (_interactionHint != null)
                _interactionHint.Visible = true;

            GD.Print($"Player near {NpcName}");
        }
    }

    private void OnPlayerExited(Node2D body)
    {
        if (body is PlayerController)
        {
            _playerNearby = false;
            _nearbyPlayer = null;

            if (_interactionHint != null)
                _interactionHint.Visible = false;
        }
    }

    private void Interact()
    {
        if (!_hasInteracted)
        {
            // First interaction - update quest
            if (_questSystem != null)
            {
                _questSystem.UpdateQuestProgress("npc", NpcId);
            }
            _hasInteracted = true;
        }

        // Show current dialogue line
        if (_currentDialogueIndex < DialogueLines.Length)
        {
            string dialogue = DialogueLines[_currentDialogueIndex];
            ShowDialogue(dialogue);

            _currentDialogueIndex++;

            // Update quest progress for dialogue
            if (_questSystem != null)
            {
                _questSystem.UpdateQuestProgress("dialogue", $"{NpcId}_talk");
            }
        }
        else
        {
            // Loop back to start
            _currentDialogueIndex = 0;
            ShowDialogue(DialogueLines[0]);
        }
    }

    private void ShowDialogue(string text)
    {
        GD.Print($"[{NpcName}]: {text}");

        // Show dialogue in game
        var gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.ShowNotification($"{NpcName}: {text}");
        }
    }
}
