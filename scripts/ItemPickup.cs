using Godot;
using System;

public partial class ItemPickup : Area2D
{
    [Export] public string ItemId { get; set; } = "stuhlbein";
    [Export] public bool AutoPickup { get; set; } = false;
    [Export] public float BobSpeed { get; set; } = 2.0f;
    [Export] public float BobAmount { get; set; } = 5.0f;

    private InventorySystem _inventory;
    private Sprite2D _sprite;
    private Label _label;
    private bool _playerNearby = false;
    private PlayerController _nearbyPlayer;
    private float _time = 0.0f;
    private Vector2 _originalPosition;

    public override void _Ready()
    {
        AddToGroup("items");

        _inventory = GetNodeOrNull<InventorySystem>("/root/InventorySystem");
        _sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        _label = GetNodeOrNull<Label>("Label");

        _originalPosition = Position;

        // Connect area signals
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        // Set label text if available
        if (_label != null && _inventory != null)
        {
            var weaponData = _inventory.GetWeaponData(ItemId);
            if (weaponData != null)
            {
                _label.Text = weaponData.name;
            }
        }

        GD.Print($"ItemPickup '{ItemId}' ready");
    }

    public override void _Process(double delta)
    {
        // Bob up and down animation
        _time += (float)delta;
        Position = _originalPosition + new Vector2(0, Mathf.Sin(_time * BobSpeed) * BobAmount);

        // Check for interaction
        if (_playerNearby && Input.IsActionJustPressed("interact"))
        {
            PickupItem();
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is PlayerController player)
        {
            _playerNearby = true;
            _nearbyPlayer = player;

            // Show interaction hint
            if (_label != null)
            {
                _label.Visible = true;
            }

            // Auto pickup if enabled
            if (AutoPickup)
            {
                PickupItem();
            }
            else
            {
                GD.Print($"Press E to pickup {ItemId}");
            }
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is PlayerController)
        {
            _playerNearby = false;
            _nearbyPlayer = null;

            // Hide interaction hint
            if (_label != null)
            {
                _label.Visible = false;
            }
        }
    }

    private void PickupItem()
    {
        if (_inventory == null)
        {
            GD.PrintErr("InventorySystem not found!");
            return;
        }

        // Create item from ID
        var item = _inventory.CreateWeaponFromId(ItemId);
        if (item == null)
        {
            GD.PrintErr($"Failed to create item '{ItemId}'");
            return;
        }

        // Add to inventory
        if (_inventory.AddItem(item))
        {
            GD.Print($"Picked up {item.Name}");

            // Update quest progress
            var questSystem = GetNodeOrNull<QuestSystem>("/root/QuestSystem");
            if (questSystem != null)
            {
                questSystem.UpdateQuestProgress("item", ItemId);
            }

            // Remove from scene
            QueueFree();
        }
        else
        {
            GD.Print("Inventory is full!");
        }
    }
}
