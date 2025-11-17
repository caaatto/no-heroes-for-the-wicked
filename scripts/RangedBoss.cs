using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Ranged Boss: Medium HP, maintains distance, projectile attacks with area denial
/// Strengths: Long range attacks, area control, projectile patterns, kiting ability
/// Weaknesses: Vulnerable in close combat, lower melee damage, relies on positioning
/// </summary>
public partial class RangedBoss : BossController
{
    // Ranged-specific properties
    [Export] public float PreferredDistance { get; set; } = 200.0f;
    [Export] public float ProjectileSpeed { get; set; } = 300.0f;
    [Export] public int ProjectileDamage { get; set; } = 15;
    [Export] public float ProjectileCooldown { get; set; } = 1.5f;

    private Timer _projectileTimer;
    private bool _canShootProjectile = true;

    // Pattern attack
    private int _projectilePattern = 0; // 0 = single, 1 = spread, 2 = circle
    private Timer _patternCycleTimer;

    // Teleport ability
    [Export] public float TeleportRange { get; set; } = 250.0f;
    [Export] public float TeleportCooldown { get; set; } = 8.0f;
    private Timer _teleportTimer;
    private bool _canTeleport = true;

    // Area denial
    private List<Vector2> _hazardZones = new List<Vector2>();
    private const int MaxHazardZones = 5;

    public override void _Ready()
    {
        // Ranged stats
        BossTitle = "The Arcane Sorcerer";
        EnemyName = "Ranged Boss";
        MaxLifePoints = 180; // Medium HP
        CurrentLifePoints = 180;
        AttackValue = 10; // Lower melee damage
        MoveSpeed = 120.0f; // Medium speed
        AttackRange = 300.0f; // Long range
        DetectionRange = 400.0f;
        PhaseCount = 3;

        base._Ready();

        // Setup projectile timer
        _projectileTimer = new Timer();
        _projectileTimer.WaitTime = ProjectileCooldown;
        _projectileTimer.OneShot = true;
        _projectileTimer.Timeout += OnProjectileCooldownTimeout;
        AddChild(_projectileTimer);

        // Setup pattern cycle timer
        _patternCycleTimer = new Timer();
        _patternCycleTimer.WaitTime = 5.0f;
        _patternCycleTimer.Timeout += OnPatternCycleTimeout;
        AddChild(_patternCycleTimer);
        _patternCycleTimer.Start();

        // Setup teleport timer
        _teleportTimer = new Timer();
        _teleportTimer.WaitTime = TeleportCooldown;
        _teleportTimer.OneShot = true;
        _teleportTimer.Timeout += OnTeleportCooldownTimeout;
        AddChild(_teleportTimer);

        GD.Print($"Ranged Boss: Medium HP ({MaxLifePoints}), Long Range ({AttackRange}), Projectile Master");
        GD.Print($"Special: Projectile patterns, Teleport, Area denial zones");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsAlive)
        {
            CurrentState = EnemyState.Dead;
            return;
        }

        // Custom behavior for ranged boss
        switch (CurrentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Chase:
                UpdateRangedChase();
                break;
            case EnemyState.Attack:
                UpdateRangedAttack();
                break;
        }

        MoveAndSlide();
        UpdateAnimation();
    }

    private void UpdateRangedChase()
    {
        if (_player == null || !_player.IsAlive())
        {
            CurrentState = EnemyState.Idle;
            return;
        }

        float distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);

        // Lost player
        if (distanceToPlayer > DetectionRange * 1.5f)
        {
            CurrentState = EnemyState.Idle;
            return;
        }

        // Maintain preferred distance
        if (distanceToPlayer <= PreferredDistance)
        {
            // Too close - back away
            var direction = _player.GlobalPosition.DirectionTo(GlobalPosition);
            Velocity = direction * MoveSpeed;

            // Teleport if player gets too close and teleport is ready
            if (distanceToPlayer < 100.0f && _canTeleport)
            {
                PerformTeleport();
            }
        }
        else if (distanceToPlayer > AttackRange)
        {
            // Too far - move closer
            var direction = GlobalPosition.DirectionTo(_player.GlobalPosition);
            Velocity = direction * MoveSpeed;
        }
        else
        {
            // Perfect range - attack
            CurrentState = EnemyState.Attack;
        }
    }

    private void UpdateRangedAttack()
    {
        if (_player == null || !_player.IsAlive())
        {
            CurrentState = EnemyState.Idle;
            return;
        }

        float distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);

        // Maintain distance while attacking
        if (distanceToPlayer < PreferredDistance * 0.7f)
        {
            // Back away while shooting
            var direction = _player.GlobalPosition.DirectionTo(GlobalPosition);
            Velocity = direction * MoveSpeed * 0.5f;
        }
        else
        {
            Velocity = Vector2.Zero;
        }

        // Out of range
        if (distanceToPlayer > AttackRange * 1.2f)
        {
            CurrentState = EnemyState.Chase;
            return;
        }

        // Shoot projectiles
        if (_canShootProjectile)
        {
            ShootProjectilePattern();
        }
    }

    protected override void OnPhaseChange(int newPhase)
    {
        base.OnPhaseChange(newPhase);

        if (newPhase == 2)
        {
            GD.Print($"{BossTitle}: \"Witness the power of true magic!\"");
            // Phase 2: Faster projectiles, shorter cooldowns
            ProjectileSpeed = 350.0f;
            ProjectileCooldown = 1.2f;
            _projectileTimer.WaitTime = ProjectileCooldown;

            // Create initial hazard zone
            CreateHazardZone(GlobalPosition);
        }
        else if (newPhase == 3)
        {
            GD.Print($"{BossTitle}: \"You face annihilation!\"");
            // Phase 3: Multiple projectiles, even faster
            ProjectileSpeed = 400.0f;
            ProjectileCooldown = 1.0f;
            _projectileTimer.WaitTime = ProjectileCooldown;
            ProjectileDamage += 5;

            // Shorter teleport cooldown
            TeleportCooldown = 5.0f;
            _teleportTimer.WaitTime = TeleportCooldown;
        }
    }

    protected override void OnEnrage()
    {
        base.OnEnrage();
        GD.Print($"{BossTitle}: \"I will reduce you to ash!\"");

        // Enrage: Rapid fire, maximum hazard zones
        ProjectileCooldown = 0.7f;
        _projectileTimer.WaitTime = ProjectileCooldown;
        ProjectileDamage += 10;

        // Create maximum hazard zones
        while (_hazardZones.Count < MaxHazardZones)
        {
            Vector2 randomPos = GlobalPosition + new Vector2(
                (float)GD.RandRange(-300, 300),
                (float)GD.RandRange(-300, 300)
            );
            CreateHazardZone(randomPos);
        }
    }

    private void ShootProjectilePattern()
    {
        if (_player == null) return;

        _canShootProjectile = false;
        _projectileTimer.Start();

        Vector2 directionToPlayer = GlobalPosition.DirectionTo(_player.GlobalPosition);

        switch (_projectilePattern)
        {
            case 0: // Single projectile
                ShootProjectile(directionToPlayer);
                break;

            case 1: // Spread shot (3 projectiles)
                float spreadAngle = 20.0f * Mathf.Pi / 180.0f;
                ShootProjectile(directionToPlayer);
                ShootProjectile(directionToPlayer.Rotated(spreadAngle));
                ShootProjectile(directionToPlayer.Rotated(-spreadAngle));
                GD.Print($"{BossTitle} fires a SPREAD SHOT!");
                break;

            case 2: // Circle pattern (8 directions)
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * (2 * Mathf.Pi / 8);
                    Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    ShootProjectile(direction);
                }
                GD.Print($"{BossTitle} fires a CIRCLE PATTERN!");
                break;
        }
    }

    private void ShootProjectile(Vector2 direction)
    {
        GD.Print($"{BossTitle} shoots a projectile!");

        // TODO: Instantiate actual projectile scene
        // For now, do instant raycast-style damage
        // In full implementation, this would spawn a projectile node

        // Simulate projectile with raycast
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(
            GlobalPosition,
            GlobalPosition + direction * AttackRange
        );
        query.CollisionMask = 1; // Player layer

        var result = spaceState.IntersectRay(query);
        if (result.Count > 0 && result["collider"].Obj is PlayerController player)
        {
            player.TakeDamage(ProjectileDamage);
        }
    }

    private void OnProjectileCooldownTimeout()
    {
        _canShootProjectile = true;
    }

    private void OnPatternCycleTimeout()
    {
        // Cycle through patterns
        _projectilePattern = (_projectilePattern + 1) % 3;
        GD.Print($"{BossTitle} changes attack pattern to: {GetPatternName()}");
    }

    private string GetPatternName()
    {
        return _projectilePattern switch
        {
            0 => "Single Shot",
            1 => "Spread Shot",
            2 => "Circle Pattern",
            _ => "Unknown"
        };
    }

    private void PerformTeleport()
    {
        if (_player == null) return;

        _canTeleport = false;
        _teleportTimer.Start();

        // Teleport away from player
        Vector2 awayFromPlayer = _player.GlobalPosition.DirectionTo(GlobalPosition);
        Vector2 teleportPosition = GlobalPosition + awayFromPlayer * TeleportRange;

        // TODO: Check if position is valid (not in walls)
        GlobalPosition = teleportPosition;

        GD.Print($"{BossTitle} teleports away!");

        // Create hazard zone at old position
        CreateHazardZone(GlobalPosition);

        // TODO: Play teleport effects (particles, sound)
    }

    private void OnTeleportCooldownTimeout()
    {
        _canTeleport = true;
    }

    private void CreateHazardZone(Vector2 position)
    {
        if (_hazardZones.Count >= MaxHazardZones)
        {
            _hazardZones.RemoveAt(0); // Remove oldest
        }

        _hazardZones.Add(position);
        GD.Print($"{BossTitle} creates a hazard zone! Total: {_hazardZones.Count}");

        // TODO: Spawn visual hazard effect that damages player over time
        // Would be an Area2D that deals periodic damage
    }

    public new void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        // Chance to teleport when hit
        if (IsAlive && _canTeleport && GD.Randf() < 0.25f) // 25% chance
        {
            GD.Print($"{BossTitle} teleports to safety!");
            PerformTeleport();
        }
    }
}
