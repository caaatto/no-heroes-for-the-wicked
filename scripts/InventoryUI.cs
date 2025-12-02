using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Visual inventory grid UI system.
/// Displays items in a grid layout with drag-and-drop support.
/// </summary>
public partial class InventoryUI : CanvasLayer
{
    [Signal]
    public delegate void ItemSlotClickedEventHandler(int slotIndex);

    [Signal]
    public delegate void ItemEquippedEventHandler(string itemName);

    [Signal]
    public delegate void ItemDroppedEventHandler(int slotIndex);

    // UI References
    private Control _inventoryContainer;
    private Panel _backgroundPanel;
    private GridContainer _itemGrid;
    private Label _titleLabel;
    private Label _descriptionLabel;
    private Label _statsLabel;
    private Panel _itemPreviewPanel;
    private Button _closeButton;
    private Button _dropButton;
    private Button _equipButton;

    // Inventory data
    private InventorySystem _inventorySystem;
    private AudioManager _audioManager;
    private List<InventorySlot> _itemSlots = new List<InventorySlot>();

    [Export] public int GridColumns { get; set; } = 5;
    [Export] public int GridRows { get; set; } = 4;
    [Export] public int SlotSize { get; set; } = 80;
    [Export] public string ToggleAction { get; set; } = "inventory"; // 'I' key

    private int _selectedSlotIndex = -1;
    private bool _isVisible = false;

    public override void _Ready()
    {
        // Get system references
        _inventorySystem = GetNodeOrNull<InventorySystem>("/root/InventorySystem");
        _audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");

        BuildInventoryUI();

        // Start hidden
        _inventoryContainer.Visible = false;

        // Connect to inventory system signals
        if (_inventorySystem != null)
        {
            _inventorySystem.ItemAdded += OnInventoryChanged;
            _inventorySystem.ItemRemoved += OnInventoryChanged;
            _inventorySystem.WeaponEquipped += OnWeaponEquipped;
        }
    }

    public override void _Input(InputEvent @event)
    {
        // Toggle inventory with 'I' key
        if (@event.IsActionPressed(ToggleAction))
        {
            ToggleInventory();
            GetViewport().SetInputAsHandled();
        }
    }

    /// <summary>
    /// Build the inventory UI
    /// </summary>
    private void BuildInventoryUI()
    {
        // Main container
        _inventoryContainer = new Control();
        _inventoryContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_inventoryContainer);

        // Semi-transparent background
        _backgroundPanel = new Panel();
        _backgroundPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _inventoryContainer.AddChild(_backgroundPanel);

        var bgStyle = new StyleBoxFlat();
        bgStyle.BgColor = new Color(0, 0, 0, 0.8f);
        _backgroundPanel.AddThemeStyleboxOverride("panel", bgStyle);

        // Center container
        var centerContainer = new CenterContainer();
        centerContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _inventoryContainer.AddChild(centerContainer);

        // Main inventory panel
        var mainPanel = new Panel();
        mainPanel.CustomMinimumSize = new Vector2(700, 600);
        centerContainer.AddChild(mainPanel);

        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        panelStyle.BorderColor = new Color(0.4f, 0.4f, 0.5f);
        panelStyle.SetBorderWidthAll(3);
        panelStyle.SetCornerRadiusAll(8);
        mainPanel.AddThemeStyleboxOverride("panel", panelStyle);

        // Main VBox
        var mainVBox = new VBoxContainer();
        mainVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        mainPanel.AddChild(mainVBox);

        // Add margin
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 20);
        margin.AddThemeConstantOverride("margin_right", 20);
        margin.AddThemeConstantOverride("margin_top", 20);
        margin.AddThemeConstantOverride("margin_bottom", 20);
        mainVBox.AddChild(margin);

        var contentVBox = new VBoxContainer();
        contentVBox.AddThemeConstantOverride("separation", 15);
        margin.AddChild(contentVBox);

        // Title bar
        var titleHBox = new HBoxContainer();
        contentVBox.AddChild(titleHBox);

        _titleLabel = new Label();
        _titleLabel.Text = "INVENTORY";
        _titleLabel.AddThemeFontSizeOverride("font_size", 28);
        _titleLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        titleHBox.AddChild(_titleLabel);

        _closeButton = new Button();
        _closeButton.Text = "X";
        _closeButton.CustomMinimumSize = new Vector2(40, 40);
        _closeButton.Pressed += OnClosePressed;
        titleHBox.AddChild(_closeButton);

        // Item grid
        _itemGrid = new GridContainer();
        _itemGrid.Columns = GridColumns;
        _itemGrid.AddThemeConstantOverride("h_separation", 5);
        _itemGrid.AddThemeConstantOverride("v_separation", 5);
        contentVBox.AddChild(_itemGrid);

        // Create inventory slots
        int totalSlots = GridColumns * GridRows;
        for (int i = 0; i < totalSlots; i++)
        {
            var slot = new InventorySlot(i, SlotSize);
            slot.SlotClicked += OnSlotClicked;
            _itemGrid.AddChild(slot);
            _itemSlots.Add(slot);
        }

        // Item details panel
        var detailsPanel = new Panel();
        detailsPanel.CustomMinimumSize = new Vector2(0, 150);
        contentVBox.AddChild(detailsPanel);

        var detailsStyle = new StyleBoxFlat();
        detailsStyle.BgColor = new Color(0.15f, 0.15f, 0.2f);
        detailsStyle.SetCornerRadiusAll(4);
        detailsPanel.AddThemeStyleboxOverride("panel", detailsStyle);

        var detailsMargin = new MarginContainer();
        detailsMargin.AddThemeConstantOverride("margin_left", 15);
        detailsMargin.AddThemeConstantOverride("margin_right", 15);
        detailsMargin.AddThemeConstantOverride("margin_top", 15);
        detailsMargin.AddThemeConstantOverride("margin_bottom", 15);
        detailsPanel.AddChild(detailsMargin);

        var detailsVBox = new VBoxContainer();
        detailsVBox.AddThemeConstantOverride("separation", 5);
        detailsMargin.AddChild(detailsVBox);

        _descriptionLabel = new Label();
        _descriptionLabel.Text = "Select an item to view details";
        _descriptionLabel.AddThemeFontSizeOverride("font_size", 14);
        _descriptionLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        detailsVBox.AddChild(_descriptionLabel);

        _statsLabel = new Label();
        _statsLabel.Text = "";
        _statsLabel.AddThemeFontSizeOverride("font_size", 12);
        _statsLabel.Modulate = new Color(0.7f, 0.9f, 1.0f);
        detailsVBox.AddChild(_statsLabel);

        // Action buttons
        var buttonHBox = new HBoxContainer();
        buttonHBox.AddThemeConstantOverride("separation", 10);
        contentVBox.AddChild(buttonHBox);

        _equipButton = new Button();
        _equipButton.Text = "Equip";
        _equipButton.CustomMinimumSize = new Vector2(120, 40);
        _equipButton.Disabled = true;
        _equipButton.Pressed += OnEquipPressed;
        buttonHBox.AddChild(_equipButton);

        _dropButton = new Button();
        _dropButton.Text = "Drop";
        _dropButton.CustomMinimumSize = new Vector2(120, 40);
        _dropButton.Disabled = true;
        _dropButton.Pressed += OnDropPressed;
        buttonHBox.AddChild(_dropButton);
    }

    /// <summary>
    /// Toggle inventory visibility
    /// </summary>
    public void ToggleInventory()
    {
        SetInventoryVisible(!_isVisible);
    }

    /// <summary>
    /// Set inventory visibility
    /// </summary>
    public void SetInventoryVisible(bool visible)
    {
        _isVisible = visible;
        _inventoryContainer.Visible = visible;

        if (visible)
        {
            _audioManager?.PlaySfx("inventory_open");
            RefreshInventory();
        }
        else
        {
            _audioManager?.PlaySfx("menu_close");
            DeselectSlot();
        }
    }

    /// <summary>
    /// Refresh all inventory slots
    /// </summary>
    private void RefreshInventory()
    {
        if (_inventorySystem == null) return;

        var items = _inventorySystem.GetAllItems();

        for (int i = 0; i < _itemSlots.Count; i++)
        {
            if (i < items.Count && items[i] != null)
            {
                _itemSlots[i].SetItem(items[i]);
            }
            else
            {
                _itemSlots[i].ClearSlot();
            }
        }

        GD.Print($"[InventoryUI] Refreshed {items.Count} items");
    }

    /// <summary>
    /// Handle slot clicked
    /// </summary>
    private void OnSlotClicked(int slotIndex)
    {
        _audioManager?.PlaySfx("button_click");

        if (_selectedSlotIndex == slotIndex)
        {
            // Deselect if clicking same slot
            DeselectSlot();
        }
        else
        {
            SelectSlot(slotIndex);
        }

        EmitSignal(SignalName.ItemSlotClicked, slotIndex);
    }

    /// <summary>
    /// Select an inventory slot
    /// </summary>
    private void SelectSlot(int slotIndex)
    {
        // Deselect previous slot
        if (_selectedSlotIndex >= 0 && _selectedSlotIndex < _itemSlots.Count)
        {
            _itemSlots[_selectedSlotIndex].SetSelected(false);
        }

        _selectedSlotIndex = slotIndex;

        if (slotIndex >= 0 && slotIndex < _itemSlots.Count)
        {
            _itemSlots[slotIndex].SetSelected(true);

            // Update item details
            var item = _itemSlots[slotIndex].GetItem();
            if (item != null)
            {
                UpdateItemDetails(item);
                _equipButton.Disabled = item.Type != ItemType.Weapon;
                _dropButton.Disabled = false;
            }
        }
    }

    /// <summary>
    /// Deselect current slot
    /// </summary>
    private void DeselectSlot()
    {
        if (_selectedSlotIndex >= 0 && _selectedSlotIndex < _itemSlots.Count)
        {
            _itemSlots[_selectedSlotIndex].SetSelected(false);
        }

        _selectedSlotIndex = -1;
        _descriptionLabel.Text = "Select an item to view details";
        _statsLabel.Text = "";
        _equipButton.Disabled = true;
        _dropButton.Disabled = true;
    }

    /// <summary>
    /// Update item details display
    /// </summary>
    private void UpdateItemDetails(Item item)
    {
        _descriptionLabel.Text = $"{item.Name}\n{item.Description}\n{item.Type}";

        if (item.Type == ItemType.Weapon)
        {
            string stats = $"Damage: {item.DamageDice}\n";
            stats += $"Stat: {item.DamageStat}\n";
            stats += $"Rarity: {item.Rarity}";

            _statsLabel.Text = stats;
        }
        else
        {
            _statsLabel.Text = "";
        }
    }

    #region Button Handlers

    private void OnClosePressed()
    {
        _audioManager?.PlaySfx("button_click");
        SetInventoryVisible(false);
    }

    private void OnEquipPressed()
    {
        if (_selectedSlotIndex < 0 || _inventorySystem == null) return;

        var item = _itemSlots[_selectedSlotIndex].GetItem();
        if (item != null && item.Type == ItemType.Weapon)
        {
            _inventorySystem.EquipWeapon(item);
            _audioManager?.PlaySfx("item_equip");
            EmitSignal(SignalName.ItemEquipped, item.Name);
        }
    }

    private void OnDropPressed()
    {
        if (_selectedSlotIndex < 0 || _inventorySystem == null) return;

        var item = _itemSlots[_selectedSlotIndex].GetItem();
        if (item != null)
        {
            _inventorySystem.RemoveItem(item);
            _audioManager?.PlaySfx("button_click");
            EmitSignal(SignalName.ItemDropped, _selectedSlotIndex);
            RefreshInventory();
            DeselectSlot();
        }
    }

    #endregion

    #region Signal Handlers

    private void OnInventoryChanged(Item item)
    {
        RefreshInventory();
    }

    private void OnWeaponEquipped(Item weapon)
    {
        RefreshInventory();
        if (weapon != null)
        {
            GD.Print($"[InventoryUI] Weapon equipped: {weapon.Name}");
        }
    }

    #endregion
}

/// <summary>
/// Individual inventory slot UI element
/// </summary>
public partial class InventorySlot : Panel
{
    [Signal]
    public delegate void SlotClickedEventHandler(int slotIndex);

    private int _slotIndex;
    private Item _item;
    private Label _itemLabel;
    private Panel _highlightPanel;
    private bool _isSelected = false;

    public InventorySlot(int slotIndex, int size)
    {
        _slotIndex = slotIndex;
        CustomMinimumSize = new Vector2(size, size);

        // Slot background
        var slotStyle = new StyleBoxFlat();
        slotStyle.BgColor = new Color(0.2f, 0.2f, 0.25f);
        slotStyle.BorderColor = new Color(0.3f, 0.3f, 0.35f);
        slotStyle.SetBorderWidthAll(2);
        slotStyle.SetCornerRadiusAll(4);
        AddThemeStyleboxOverride("panel", slotStyle);

        // Item label
        _itemLabel = new Label();
        _itemLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _itemLabel.VerticalAlignment = VerticalAlignment.Center;
        _itemLabel.AddThemeFontSizeOverride("font_size", 12);
        _itemLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        _itemLabel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_itemLabel);

        // Highlight overlay
        _highlightPanel = new Panel();
        _highlightPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _highlightPanel.MouseFilter = MouseFilterEnum.Ignore;
        _highlightPanel.Visible = false;
        AddChild(_highlightPanel);

        var highlightStyle = new StyleBoxFlat();
        highlightStyle.BgColor = new Color(1, 1, 0, 0.3f);
        highlightStyle.BorderColor = new Color(1, 1, 0);
        highlightStyle.SetBorderWidthAll(3);
        highlightStyle.SetCornerRadiusAll(4);
        _highlightPanel.AddThemeStyleboxOverride("panel", highlightStyle);

        // Make clickable
        MouseFilter = MouseFilterEnum.Stop;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            EmitSignal(SignalName.SlotClicked, _slotIndex);
        }
    }

    public void SetItem(Item item)
    {
        _item = item;
        if (item != null)
        {
            _itemLabel.Text = item.Name;

            // Color code by rarity
            Color textColor = item.Rarity switch
            {
                "common" => new Color(0.8f, 0.8f, 0.8f),
                "uncommon" => new Color(0.3f, 1.0f, 0.3f),
                "rare" => new Color(0.3f, 0.5f, 1.0f),
                "legendary" => new Color(1.0f, 0.6f, 0.0f),
                _ => Colors.White
            };
            _itemLabel.Modulate = textColor;
        }
    }

    public void ClearSlot()
    {
        _item = null;
        _itemLabel.Text = "";
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        _highlightPanel.Visible = selected;
    }

    public Item GetItem()
    {
        return _item;
    }
}
