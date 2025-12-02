using Godot;
using System;

/// <summary>
/// Ogre enemy - slow, tanky, heavy hitter
/// Has ground slam AoE attack
/// </summary>
public partial class OgreEnemy : EnemyController
{
    [Export] public float SlamRange { get; set; } = 100.0f;
    [Export] public float SlamCooldown { get; set; } = 5.0f;
    [Export] public int SlamDamage { get; set; } = 20;

    private float _slamTimer = 0.0f;
    private bool _isSlam = false;

    public override void _Ready()
    {
        // Ogre stats - tanky and slow
        MaxLifePoints = 120;
        CurrentLifePoints = MaxLifePoints;
        AttackValue = 15;
        MoveSpeed = 70.0f;
        DetectionRange = 180.0f;
        AttackRange = 60.0f;
        AttackCooldown = 2.0f;

        base._Ready();

        GD.Print("[OgreEnemy] Ogre initialized");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        // Update slam cooldown
        if (_slamTimer > 0)
        {
            _slamTimer -= (float)delta;
        }

        // Check for slam opportunity
        if (CurrentState == EnemyState.Attack && _slamTimer <= 0 && !_isSlam)
        {
            // 30% chance to use slam instead of normal attack
            if (GD.Randf() < 0.3f)
            {
                PerformGroundSlam();
            }
        }
    }

    /// <summary>
    /// Perform ground slam AoE attack
    /// </summary>
    private void PerformGroundSlam()
    {
        _isSlam = true;
        _slamTimer = SlamCooldown;

        GD.Print("[OgreEnemy] Performing ground slam!");

        // Find all enemies in radius (would need physics query in real implementation)
        if (_player != null && GlobalPosition.DistanceTo(_player.GlobalPosition) <= SlamRange)
        {
            // Calculate distance-based damage
            float distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
            float damageMultiplier = 1.0f - (distance / SlamRange);
            int damage = (int)(SlamDamage * damageMultiplier);

            // Damage player (would need to call player's TakeDamage method)
            GD.Print($"[OgreEnemy] Slam hits player for {damage} damage!");
        }

        _isSlam = false;
    }

    protected override void AttackPlayer()
    {
        // Normal attack with longer cooldown
        base.AttackPlayer();
    }
}
