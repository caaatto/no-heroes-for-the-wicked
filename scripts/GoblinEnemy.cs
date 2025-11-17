using Godot;
using System;

/// <summary>
/// Goblin enemy - fast, weak, cowardly
/// Runs away when low on health
/// </summary>
public partial class GoblinEnemy : EnemyController
{
    [Export] public float FleeHealthThreshold { get; set; } = 0.3f; // Flee at 30% HP
    [Export] public float FleeSpeedMultiplier { get; set; } = 1.5f;

    private bool _isFleeing = false;
    private Vector2 _fleeDirection;

    public override void _Ready()
    {
        // Goblin stats
        MaxLifePoints = 30;
        CurrentLifePoints = MaxLifePoints;
        AttackValue = 6;
        MoveSpeed = 130.0f;
        DetectionRange = 250.0f;
        AttackRange = 40.0f;
        AttackCooldown = 0.8f;

        base._Ready();

        GD.Print("[GoblinEnemy] Goblin initialized");
    }

    public override void _PhysicsProcess(double delta)
    {
        // Check if should flee
        float healthPercent = (float)CurrentLifePoints / MaxLifePoints;
        if (healthPercent <= FleeHealthThreshold && !_isFleeing)
        {
            StartFleeing();
        }

        if (_isFleeing)
        {
            ProcessFleeing(delta);
        }
        else
        {
            base._PhysicsProcess(delta);
        }
    }

    /// <summary>
    /// Start fleeing behavior
    /// </summary>
    private void StartFleeing()
    {
        _isFleeing = true;
        CurrentState = AIState.Chase; // Use chase state but flee

        // Calculate flee direction (away from player)
        if (_player != null)
        {
            _fleeDirection = (GlobalPosition - _player.GlobalPosition).Normalized();
        }

        GD.Print("[GoblinEnemy] Fleeing in panic!");
    }

    /// <summary>
    /// Process fleeing behavior
    /// </summary>
    private void ProcessFleeing(double delta)
    {
        if (_player == null) return;

        // Update flee direction
        _fleeDirection = (GlobalPosition - _player.GlobalPosition).Normalized();

        // Move away from player at increased speed
        Vector2 velocity = _fleeDirection * MoveSpeed * FleeSpeedMultiplier;
        Velocity = velocity;
        MoveAndSlide();
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        // Chance to flee even before threshold when hit
        if (!_isFleeing && GD.Randf() < 0.2f) // 20% chance to panic
        {
            StartFleeing();
        }
    }
}
