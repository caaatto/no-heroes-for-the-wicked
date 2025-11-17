using Godot;
using System;

/// <summary>
/// Berserker enemy - gets stronger when damaged
/// Enrages at low health
/// </summary>
public partial class BerserkerEnemy : EnemyController
{
    [Export] public float EnrageThreshold { get; set; } = 0.4f; // Enrage at 40% HP
    [Export] public float EnrageDamageMultiplier { get; set; } = 1.5f;
    [Export] public float EnrageSpeedMultiplier { get; set; } = 1.3f;

    private bool _isEnraged = false;
    private int _baseAttackValue;
    private float _baseMoveSpeed;

    public override void _Ready()
    {
        // Berserker stats - balanced
        MaxLifePoints = 80;
        CurrentLifePoints = MaxLifePoints;
        AttackValue = 12;
        MoveSpeed = 100.0f;
        DetectionRange = 200.0f;
        AttackRange = 50.0f;
        AttackCooldown = 1.2f;

        _baseAttackValue = AttackValue;
        _baseMoveSpeed = MoveSpeed;

        base._Ready();

        GD.Print("[BerserkerEnemy] Berserker initialized");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        // Check for enrage
        float healthPercent = (float)CurrentLifePoints / MaxLifePoints;
        if (healthPercent <= EnrageThreshold && !_isEnraged)
        {
            EnterEnrage();
        }
    }

    /// <summary>
    /// Enter enraged state
    /// </summary>
    private void EnterEnrage()
    {
        _isEnraged = true;

        // Boost stats
        AttackValue = (int)(_baseAttackValue * EnrageDamageMultiplier);
        MoveSpeed = _baseMoveSpeed * EnrageSpeedMultiplier;

        // Visual feedback - red tint
        Modulate = new Color(1.5f, 0.5f, 0.5f);

        GD.Print("[BerserkerEnemy] ENRAGED! Attack and speed increased!");
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        // Get angrier with each hit (small attack boost)
        if (!_isEnraged)
        {
            AttackValue += 1;
            GD.Print($"[BerserkerEnemy] Growing angrier! Attack: {AttackValue}");
        }
    }

    protected override void AttackPlayer()
    {
        base.AttackPlayer();

        // Enraged attacks are faster
        if (_isEnraged)
        {
            _attackTimer *= 0.8f; // 20% faster attacks
        }
    }
}
