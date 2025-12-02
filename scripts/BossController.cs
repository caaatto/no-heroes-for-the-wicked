using Godot;
using System;

/// <summary>
/// Base class for all boss enemies with enhanced abilities and unique mechanics
/// </summary>
public partial class BossController : EnemyController
{
    // Boss-specific properties
    [Export] public string BossTitle { get; set; } = "The Unnamed";
    [Export] public bool IsBoss { get; set; } = true;
    [Export] public int PhaseCount { get; set; } = 1;
    [Export] public int CurrentPhase { get; set; } = 1;

    // Boss mechanics
    [Export] public float EnrageThreshold { get; set; } = 0.3f; // HP% to trigger enrage
    [Export] public bool IsEnraged { get; set; } = false;

    // Loot
    [Export] public int GoldDrop { get; set; } = 100;
    [Export] public string[] LootTable { get; set; } = Array.Empty<string>();

    // UI Elements
    private BossHealthUI _bossHealthUI;

    public override void _Ready()
    {
        base._Ready();

        // Set up boss-specific initialization
        AddToGroup("bosses");

        GD.Print($"=== BOSS SPAWNED: {BossTitle} ===");
        GD.Print($"Type: {EnemyName}");
        GD.Print($"Max HP: {MaxLifePoints}");
        GD.Print($"Attack: {AttackValue}");
        GD.Print($"Speed: {MoveSpeed}");

        // Defer UI setup to ensure scene is ready
        CallDeferred(MethodName.ShowBossUI);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        // Check for phase transitions
        CheckPhaseTransition();

        // Check for enrage
        if (!IsEnraged && GetHealthPercentage() <= EnrageThreshold)
        {
            TriggerEnrage();
        }
    }

    protected virtual void CheckPhaseTransition()
    {
        // Override in derived classes for multi-phase bosses
        float healthPercent = GetHealthPercentage();
        int newPhase = 1 + (int)((1.0f - healthPercent) * PhaseCount);

        if (newPhase > CurrentPhase && newPhase <= PhaseCount)
        {
            CurrentPhase = newPhase;
            OnPhaseChange(CurrentPhase);
        }
    }

    protected virtual void OnPhaseChange(int newPhase)
    {
        GD.Print($"{BossTitle} enters Phase {newPhase}!");

        // Notify UI
        if (_bossHealthUI != null)
        {
            _bossHealthUI.OnPhaseChanged(newPhase, PhaseCount);
        }

        // Override in derived classes for phase-specific behavior
    }

    protected virtual void TriggerEnrage()
    {
        IsEnraged = true;
        GD.Print($"{BossTitle} is ENRAGED!");

        // Base enrage effects
        MoveSpeed *= 1.3f;
        AttackValue = (int)(AttackValue * 1.2f);

        // Override in derived classes for boss-specific enrage
        OnEnrage();
    }

    protected virtual void OnEnrage()
    {
        // Override in derived classes
    }

    public float GetHealthPercentage()
    {
        return (float)CurrentLifePoints / MaxLifePoints;
    }

    private void ShowBossUI()
    {
        // Find or create boss health UI
        _bossHealthUI = GetTree().Root.GetNodeOrNull<BossHealthUI>("BossHealthUI");

        if (_bossHealthUI == null)
        {
            // Create new boss health UI
            _bossHealthUI = new BossHealthUI();
            _bossHealthUI.Name = "BossHealthUI";
            GetTree().Root.AddChild(_bossHealthUI);
            GD.Print("[BossController] Created new BossHealthUI");
        }

        // Show this boss
        _bossHealthUI.ShowBoss(this);
        GD.Print($"[BossController] Displaying boss UI for: {BossTitle}");
    }

    protected override void Die()
    {
        // Hide boss UI when boss dies
        if (_bossHealthUI != null)
        {
            _bossHealthUI.Hide();
        }

        base.Die();
    }

    protected void SpawnMinion(PackedScene minionScene, Vector2 offset)
    {
        if (minionScene == null) return;

        var minion = minionScene.Instantiate();
        if (minion is Node2D node2D)
        {
            node2D.GlobalPosition = GlobalPosition + offset;
            GetParent().AddChild(node2D);
            GD.Print($"{BossTitle} spawned a minion!");
        }
    }
}
