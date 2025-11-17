using Godot;
using System;

public partial class EnemyController : CharacterBody2D
{
    // Enemy Stats
    [Export] public string EnemyName { get; set; } = "Troll";
    [Export] public int MaxLifePoints { get; set; } = 50;
    [Export] public int CurrentLifePoints { get; set; } = 50;
    [Export] public int AttackValue { get; set; } = 8;
    [Export] public float MoveSpeed { get; set; } = 100.0f;
    [Export] public float DetectionRange { get; set; } = 200.0f;
    [Export] public float AttackRange { get; set; } = 50.0f;

    // AI Behavior
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Dead
    }

    [Export] public EnemyState CurrentState { get; set; } = EnemyState.Idle;

    // Components
    private AnimatedSprite2D _animatedSprite;
    private Timer _attackTimer;
    private PlayerController _player;

    // Combat
    private bool _canAttack = true;
    private float _attackCooldown = 1.0f;

    // Patrol (optional)
    private Vector2 _patrolStartPosition;
    private float _patrolRadius = 100.0f;
    private Vector2 _patrolTarget;

    public bool IsAlive => CurrentLifePoints > 0;

    public override void _Ready()
    {
        AddToGroup("enemies");

        _animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        _patrolStartPosition = GlobalPosition;
        _patrolTarget = GlobalPosition;

        // Setup attack timer
        _attackTimer = new Timer();
        _attackTimer.WaitTime = _attackCooldown;
        _attackTimer.OneShot = true;
        _attackTimer.Timeout += OnAttackTimerTimeout;
        AddChild(_attackTimer);

        // Find player
        var playerNode = GetTree().GetFirstNodeInGroup("player");
        if (playerNode is PlayerController player)
        {
            _player = player;
        }

        GD.Print($"Enemy '{EnemyName}' spawned with {CurrentLifePoints} HP");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsAlive)
        {
            CurrentState = EnemyState.Dead;
            return;
        }

        // Update AI based on state
        switch (CurrentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
        }

        MoveAndSlide();
        UpdateAnimation();
    }

    private void UpdateIdle()
    {
        Velocity = Vector2.Zero;

        // Check for player in detection range
        if (IsPlayerInRange(DetectionRange))
        {
            CurrentState = EnemyState.Chase;
            GD.Print($"{EnemyName} detected player!");
        }
    }

    private void UpdatePatrol()
    {
        // Simple patrol behavior
        if (GlobalPosition.DistanceTo(_patrolTarget) < 10.0f)
        {
            // Reached patrol target, pick new one
            var randomOffset = new Vector2(
                (float)GD.RandRange(-_patrolRadius, _patrolRadius),
                (float)GD.RandRange(-_patrolRadius, _patrolRadius)
            );
            _patrolTarget = _patrolStartPosition + randomOffset;
        }

        // Move towards patrol target
        var direction = (GlobalPosition.DirectionTo(_patrolTarget));
        Velocity = direction * MoveSpeed * 0.5f; // Slower when patrolling

        // Check for player
        if (IsPlayerInRange(DetectionRange))
        {
            CurrentState = EnemyState.Chase;
        }
    }

    private void UpdateChase()
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
            GD.Print($"{EnemyName} lost sight of player");
            return;
        }

        // In attack range
        if (distanceToPlayer <= AttackRange)
        {
            CurrentState = EnemyState.Attack;
            return;
        }

        // Chase player
        var direction = GlobalPosition.DirectionTo(_player.GlobalPosition);
        Velocity = direction * MoveSpeed;
    }

    private void UpdateAttack()
    {
        if (_player == null || !_player.IsAlive())
        {
            CurrentState = EnemyState.Idle;
            return;
        }

        Velocity = Vector2.Zero;

        float distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);

        // Player moved out of range
        if (distanceToPlayer > AttackRange * 1.2f)
        {
            CurrentState = EnemyState.Chase;
            return;
        }

        // Attack
        if (_canAttack)
        {
            Attack(_player);
        }
    }

    private bool IsPlayerInRange(float range)
    {
        if (_player == null)
            return false;

        return GlobalPosition.DistanceTo(_player.GlobalPosition) <= range;
    }

    public void Attack(PlayerController player)
    {
        if (player == null || !player.IsAlive())
            return;

        _canAttack = false;
        _attackTimer.Start();

        GD.Print($"{EnemyName} attacks {player.PlayerName} for {AttackValue} damage.");
        player.TakeDamage(AttackValue);

        // Play attack animation
        if (_animatedSprite != null)
        {
            // Animation will be added when sprites are available
        }
    }

    public void TakeDamage(int damage)
    {
        CurrentLifePoints -= damage;
        GD.Print($"{EnemyName} takes {damage} damage. Remaining HP: {CurrentLifePoints}");

        // Interrupt current action and chase attacker
        if (CurrentState != EnemyState.Attack && CurrentState != EnemyState.Chase)
        {
            CurrentState = EnemyState.Chase;
        }

        if (!IsAlive)
        {
            Die();
        }
    }

    private void Die()
    {
        GD.Print($"{EnemyName} has been defeated!");
        CurrentState = EnemyState.Dead;

        // Emit signal for quest/score tracking
        EmitSignal(SignalName.EnemyDied, this);

        // TODO: Implement death sequence
        // - Play death animation
        // - Drop loot
        // - Award experience
        // - Remove from scene

        // Temporary: Remove after delay
        var deathTimer = GetTree().CreateTimer(2.0);
        deathTimer.Timeout += () => QueueFree();
    }

    private void OnAttackTimerTimeout()
    {
        _canAttack = true;
    }

    private void UpdateAnimation()
    {
        if (_animatedSprite == null || !IsAlive)
            return;

        // Animation logic - will be implemented when sprites are available
        if (Velocity.Length() > 0)
        {
            // Play walk animation based on direction
            _animatedSprite.FlipH = Velocity.X < 0;
        }
    }

    // Signals
    [Signal]
    public delegate void EnemyDiedEventHandler(EnemyController enemy);
}
