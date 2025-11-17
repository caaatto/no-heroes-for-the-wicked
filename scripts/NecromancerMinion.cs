using Godot;
using System;

/// <summary>
/// Necromancer minion - weak undead servant
/// Spawned by Necromancer boss
/// </summary>
public partial class NecromancerMinion : EnemyController
{
    [Export] public float LifeTime { get; set; } = 30.0f; // Auto-die after 30 seconds
    [Export] public bool IsResurrected { get; set; } = false;

    private float _lifeTimer = 0.0f;
    private Node _master; // Reference to Necromancer

    public override void _Ready()
    {
        // Minion stats - very weak
        MaxLifePoints = 20;
        CurrentLifePoints = MaxLifePoints;
        AttackValue = 5;
        MoveSpeed = 80.0f;
        DetectionRange = 150.0f;
        AttackRange = 40.0f;
        AttackCooldown = 1.2f;

        base._Ready();

        _lifeTimer = LifeTime;

        GD.Print("[NecromancerMinion] Undead minion spawned");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        // Count down lifetime
        _lifeTimer -= (float)delta;
        if (_lifeTimer <= 0)
        {
            // Auto-die when lifetime expires
            Die();
        }
    }

    /// <summary>
    /// Set the master (Necromancer)
    /// </summary>
    public void SetMaster(Node master)
    {
        _master = master;
    }

    /// <summary>
    /// Get the master
    /// </summary>
    public Node GetMaster()
    {
        return _master;
    }

    protected override void Die()
    {
        // Notify master of death
        if (_master != null && IsInstanceValid(_master))
        {
            // Master can track minion deaths
            GD.Print("[NecromancerMinion] Reporting death to master");
        }

        base.Die();
    }

    /// <summary>
    /// Resurrect the minion
    /// </summary>
    public void Resurrect()
    {
        CurrentLifePoints = MaxLifePoints;
        IsResurrected = true;
        _lifeTimer = LifeTime;

        // Reset state
        CurrentState = AIState.Idle;

        // Visual effect (reduced opacity for undead)
        Modulate = new Color(0.8f, 0.8f, 1.0f, 0.9f);

        GD.Print("[NecromancerMinion] Resurrected!");
    }
}
