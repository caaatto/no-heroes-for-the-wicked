using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Minimap system for displaying player position, enemies, and objectives.
/// Shows a top-down view of the game world in the corner of the screen.
/// </summary>
public partial class MinimapSystem : CanvasLayer
{
    [Signal]
    public delegate void MinimapToggledEventHandler(bool isVisible);

    // UI References
    private Control _minimapContainer;
    private SubViewportContainer _viewportContainer;
    private SubViewport _minimapViewport;
    private Camera2D _minimapCamera;
    private Panel _minimapBackground;

    // Minimap markers
    private Node2D _markersContainer;
    private Dictionary<Node2D, MinimapMarker> _entityMarkers = new Dictionary<Node2D, MinimapMarker>();

    // Player reference
    private Node2D _player;

    // Settings
    [Export] public Vector2 MinimapSize { get; set; } = new Vector2(200, 200);
    [Export] public Vector2 MinimapPosition { get; set; } = new Vector2(20, 20); // Top-right corner
    [Export] public float ZoomLevel { get; set; } = 2.0f;
    [Export] public bool IsVisible { get; set; } = true;
    [Export] public bool FollowPlayer { get; set; } = true;
    [Export] public string ToggleAction { get; set; } = "ui_focus_next"; // Tab key

    // Colors
    private Color _playerColor = new Color(0.2f, 1.0f, 0.2f);
    private Color _enemyColor = new Color(1.0f, 0.2f, 0.2f);
    private Color _bossColor = new Color(1.0f, 0.5f, 0.0f);
    private Color _objectiveColor = new Color(1.0f, 1.0f, 0.2f);
    private Color _npcColor = new Color(0.5f, 0.5f, 1.0f);

    public override void _Ready()
    {
        BuildMinimapUI();

        // Find player
        CallDeferred(nameof(FindPlayer));

        GD.Print("[MinimapSystem] Minimap system initialized");
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(ToggleAction))
        {
            ToggleMinimap();
        }

        if (_player != null && FollowPlayer && _minimapCamera != null)
        {
            // Center camera on player
            _minimapCamera.GlobalPosition = _player.GlobalPosition;
        }

        // Update markers
        UpdateMarkers();
    }

    /// <summary>
    /// Build the minimap UI
    /// </summary>
    private void BuildMinimapUI()
    {
        // Main container
        _minimapContainer = new Control();
        _minimapContainer.SetAnchorsPreset(Control.LayoutPreset.TopRight);
        _minimapContainer.Position = MinimapPosition;
        _minimapContainer.CustomMinimumSize = MinimapSize;
        AddChild(_minimapContainer);

        // Background panel
        _minimapBackground = new Panel();
        _minimapBackground.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _minimapContainer.AddChild(_minimapBackground);

        var bgStyle = new StyleBoxFlat();
        bgStyle.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        bgStyle.BorderColor = new Color(0.5f, 0.5f, 0.5f);
        bgStyle.SetBorderWidthAll(2);
        _minimapBackground.AddThemeStyleboxOverride("panel", bgStyle);

        // SubViewport container
        _viewportContainer = new SubViewportContainer();
        _viewportContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _viewportContainer.StretchShrink = 1;
        _minimapContainer.AddChild(_viewportContainer);

        // Create SubViewport
        _minimapViewport = new SubViewport();
        _minimapViewport.Size = new Vector2I((int)MinimapSize.X, (int)MinimapSize.Y);
        _minimapViewport.TransparentBg = true;
        _viewportContainer.AddChild(_minimapViewport);

        // Create minimap camera
        _minimapCamera = new Camera2D();
        _minimapCamera.Zoom = new Vector2(ZoomLevel, ZoomLevel);
        _minimapCamera.Enabled = true;
        _minimapViewport.AddChild(_minimapCamera);

        // Create markers container
        _markersContainer = new Node2D();
        _minimapViewport.AddChild(_markersContainer);

        // Set visibility
        _minimapContainer.Visible = IsVisible;
    }

    /// <summary>
    /// Find the player in the scene
    /// </summary>
    private void FindPlayer()
    {
        // Try to find player node
        _player = GetTree().Root.FindChild("Player", true, false) as Node2D;

        if (_player != null)
        {
            RegisterEntity(_player, MinimapMarkerType.Player);
            GD.Print("[MinimapSystem] Player found and registered");
        }
        else
        {
            GD.PrintErr("[MinimapSystem] Player not found!");
        }
    }

    /// <summary>
    /// Register an entity to appear on minimap
    /// </summary>
    public void RegisterEntity(Node2D entity, MinimapMarkerType markerType)
    {
        if (entity == null || _entityMarkers.ContainsKey(entity))
            return;

        var marker = new MinimapMarker(entity, markerType, GetMarkerColor(markerType));
        _markersContainer.AddChild(marker);
        _entityMarkers[entity] = marker;

        GD.Print($"[MinimapSystem] Registered {markerType} entity");
    }

    /// <summary>
    /// Unregister an entity from minimap
    /// </summary>
    public void UnregisterEntity(Node2D entity)
    {
        if (!_entityMarkers.ContainsKey(entity))
            return;

        var marker = _entityMarkers[entity];
        marker.QueueFree();
        _entityMarkers.Remove(entity);
    }

    /// <summary>
    /// Update all markers
    /// </summary>
    private void UpdateMarkers()
    {
        // Clean up invalid markers
        var toRemove = new List<Node2D>();
        foreach (var kvp in _entityMarkers)
        {
            if (!IsInstanceValid(kvp.Key))
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var entity in toRemove)
        {
            UnregisterEntity(entity);
        }

        // Update marker positions
        foreach (var kvp in _entityMarkers)
        {
            if (IsInstanceValid(kvp.Key))
            {
                kvp.Value.UpdatePosition(kvp.Key.GlobalPosition);
            }
        }
    }

    /// <summary>
    /// Get marker color by type
    /// </summary>
    private Color GetMarkerColor(MinimapMarkerType type)
    {
        return type switch
        {
            MinimapMarkerType.Player => _playerColor,
            MinimapMarkerType.Enemy => _enemyColor,
            MinimapMarkerType.Boss => _bossColor,
            MinimapMarkerType.Objective => _objectiveColor,
            MinimapMarkerType.NPC => _npcColor,
            _ => Colors.White
        };
    }

    /// <summary>
    /// Toggle minimap visibility
    /// </summary>
    public void ToggleMinimap()
    {
        IsVisible = !IsVisible;
        _minimapContainer.Visible = IsVisible;
        EmitSignal(SignalName.MinimapToggled, IsVisible);
        GD.Print($"[MinimapSystem] Minimap toggled: {IsVisible}");
    }

    /// <summary>
    /// Set minimap visibility
    /// </summary>
    public void SetMinimapVisible(bool visible)
    {
        IsVisible = visible;
        _minimapContainer.Visible = visible;
        EmitSignal(SignalName.MinimapToggled, visible);
    }

    /// <summary>
    /// Set zoom level
    /// </summary>
    public void SetZoom(float zoom)
    {
        ZoomLevel = zoom;
        if (_minimapCamera != null)
        {
            _minimapCamera.Zoom = new Vector2(zoom, zoom);
        }
    }

    /// <summary>
    /// Auto-register enemies in the scene
    /// </summary>
    public void AutoRegisterEnemies()
    {
        var enemies = GetTree().GetNodesInGroup("enemies");
        foreach (var enemy in enemies)
        {
            if (enemy is Node2D enemy2D)
            {
                // Check if it's a boss
                bool isBoss = enemy.GetType().Name.Contains("Boss");
                var markerType = isBoss ? MinimapMarkerType.Boss : MinimapMarkerType.Enemy;
                RegisterEntity(enemy2D, markerType);
            }
        }

        GD.Print($"[MinimapSystem] Auto-registered {enemies.Count} enemies");
    }

    /// <summary>
    /// Auto-register NPCs in the scene
    /// </summary>
    public void AutoRegisterNPCs()
    {
        var npcs = GetTree().GetNodesInGroup("npcs");
        foreach (var npc in npcs)
        {
            if (npc is Node2D npc2D)
            {
                RegisterEntity(npc2D, MinimapMarkerType.NPC);
            }
        }

        GD.Print($"[MinimapSystem] Auto-registered {npcs.Count} NPCs");
    }
}

/// <summary>
/// Minimap marker types
/// </summary>
public enum MinimapMarkerType
{
    Player,
    Enemy,
    Boss,
    Objective,
    NPC,
    Item,
    Checkpoint
}

/// <summary>
/// Individual minimap marker
/// </summary>
public partial class MinimapMarker : Node2D
{
    private Node2D _trackedEntity;
    private MinimapMarkerType _markerType;
    private Color _markerColor;
    private ColorRect _markerRect;

    public MinimapMarker(Node2D entity, MinimapMarkerType markerType, Color color)
    {
        _trackedEntity = entity;
        _markerType = markerType;
        _markerColor = color;

        // Create visual marker
        _markerRect = new ColorRect();
        _markerRect.Color = color;

        // Size based on type
        float size = markerType switch
        {
            MinimapMarkerType.Player => 8.0f,
            MinimapMarkerType.Boss => 10.0f,
            MinimapMarkerType.Enemy => 6.0f,
            MinimapMarkerType.Objective => 7.0f,
            MinimapMarkerType.NPC => 5.0f,
            _ => 4.0f
        };

        _markerRect.Size = new Vector2(size, size);
        _markerRect.Position = new Vector2(-size / 2, -size / 2); // Center
        AddChild(_markerRect);

        // Player marker has a different shape (could be a triangle or arrow)
        if (markerType == MinimapMarkerType.Player)
        {
            _markerRect.Color = new Color(color, 0.9f);
        }
    }

    /// <summary>
    /// Update marker position
    /// </summary>
    public void UpdatePosition(Vector2 worldPosition)
    {
        GlobalPosition = worldPosition;
    }

    /// <summary>
    /// Get tracked entity
    /// </summary>
    public Node2D GetTrackedEntity()
    {
        return _trackedEntity;
    }
}
