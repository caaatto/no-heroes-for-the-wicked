using Godot;
using System;

/// <summary>
/// Archer enemy - ranged attacker
/// Keeps distance from player and shoots projectiles
/// </summary>
public partial class ArcherEnemy : EnemyController
{
    [Export] public float PreferredRange { get; set; } = 200.0f;
    [Export] public float MinRange { get; set; } = 100.0f;
    [Export] public float ProjectileSpeed { get; set; } = 300.0f;
    [Export] public int ProjectileDamage { get; set; } = 12;

    private PackedScene _projectileScene;

    public override void _Ready()
    {
        // Archer stats - medium health, ranged
        MaxLifePoints = 40;
        CurrentLifePoints = MaxLifePoints;
        AttackValue = 10;
        MoveSpeed = 110.0f;
        DetectionRange = 300.0f;
        AttackRange = 250.0f; // Long range
        AttackCooldown = 1.5f;

        base._Ready();

        GD.Print("[ArcherEnemy] Archer initialized");
    }

    public override void _PhysicsProcess(double delta)
    {
        // Maintain distance from player
        if (_player != null && CurrentState == AIState.Chase)
        {
            float distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);

            // Too close? Back away
            if (distanceToPlayer < MinRange)
            {
                Vector2 awayDirection = (GlobalPosition - _player.GlobalPosition).Normalized();
                Velocity = awayDirection * MoveSpeed;
                MoveAndSlide();
                return;
            }
            // In preferred range? Stop and shoot
            else if (distanceToPlayer <= PreferredRange)
            {
                CurrentState = AIState.Attack;
                Velocity = Vector2.Zero;
            }
        }

        base._PhysicsProcess(delta);
    }

    protected override void AttackPlayer()
    {
        if (_player == null || !_canAttack) return;

        _canAttack = false;
        _attackTimer = AttackCooldown;

        // Shoot projectile
        ShootProjectile();

        GD.Print("[ArcherEnemy] Shooting arrow!");
    }

    /// <summary>
    /// Shoot a projectile at the player
    /// </summary>
    private void ShootProjectile()
    {
        if (_player == null) return;

        // Calculate direction to player
        Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();

        // Create projectile (simplified - would spawn actual projectile node)
        GD.Print($"[ArcherEnemy] Arrow fired towards player at direction {direction}");

        // In a full implementation, would spawn a Projectile scene here
        // var projectile = _projectileScene.Instantiate<Projectile>();
        // projectile.Initialize(GlobalPosition, direction, ProjectileSpeed, ProjectileDamage);
        // GetParent().AddChild(projectile);
    }

    protected override void ChasePlayer()
    {
        if (_player == null) return;

        float distance = GlobalPosition.DistanceTo(_player.GlobalPosition);

        // If in attack range, attack instead
        if (distance <= AttackRange && distance >= MinRange)
        {
            CurrentState = AIState.Attack;
            return;
        }

        // If too far, chase
        if (distance > PreferredRange)
        {
            base.ChasePlayer();
        }
    }
}
