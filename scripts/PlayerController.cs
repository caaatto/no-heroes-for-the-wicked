using Godot;
using System;

public partial class PlayerController : CharacterBody2D
{
    // Player Stats
    [Export] public string PlayerName { get; set; } = "Hero";
    [Export] public int MaxLifePoints { get; set; } = 100;
    [Export] public int CurrentLifePoints { get; set; } = 100;
    [Export] public int AttackValue { get; set; } = 10;
    [Export] public float MoveSpeed { get; set; } = 200.0f;

    // Components
    private AnimatedSprite2D _animatedSprite;
    private InventorySystem _inventory;

    // Movement
    private Vector2 _velocity = Vector2.Zero;

    // Combat
    private bool _canAttack = true;
    private float _attackCooldown = 0.5f;
    private Timer _attackTimer;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _inventory = GetNode<InventorySystem>("/root/InventorySystem");

        // Setup attack timer
        _attackTimer = new Timer();
        _attackTimer.WaitTime = _attackCooldown;
        _attackTimer.OneShot = true;
        _attackTimer.Timeout += OnAttackTimerTimeout;
        AddChild(_attackTimer);

        GD.Print($"Player '{PlayerName}' initialized with {CurrentLifePoints} HP");
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleMovement();
        HandleCombat();
        MoveAndSlide();
        UpdateAnimation();
    }

    private void HandleMovement()
    {
        Vector2 inputDirection = Vector2.Zero;

        // Get input
        if (Input.IsActionPressed("move_right"))
            inputDirection.X += 1;
        if (Input.IsActionPressed("move_left"))
            inputDirection.X -= 1;
        if (Input.IsActionPressed("move_down"))
            inputDirection.Y += 1;
        if (Input.IsActionPressed("move_up"))
            inputDirection.Y -= 1;

        // Normalize and apply speed
        inputDirection = inputDirection.Normalized();
        Velocity = inputDirection * MoveSpeed;
    }

    private void HandleCombat()
    {
        if (Input.IsActionJustPressed("attack") && _canAttack)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        _canAttack = false;
        _attackTimer.Start();

        // Play attack animation
        if (_animatedSprite != null)
        {
            // Animation will be added when sprites are available
            GD.Print($"{PlayerName} attacks!");
        }

        // Check for enemies in attack range
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsShapeQueryParameters2D.Create();

        // Create attack hitbox
        var attackRange = 50.0f;
        var attackDirection = GetFacingDirection();
        var attackPosition = GlobalPosition + attackDirection * attackRange;

        // Simplified attack detection - will be enhanced with Area2D
        CheckForEnemiesInRange(attackPosition, attackRange);
    }

    private Vector2 GetFacingDirection()
    {
        // Return the last movement direction or default to right
        if (Velocity.Length() > 0)
            return Velocity.Normalized();
        return Vector2.Right;
    }

    private void CheckForEnemiesInRange(Vector2 position, float range)
    {
        // This will be enhanced with proper collision detection
        // For now, we'll use a simple distance check
        var enemies = GetTree().GetNodesInGroup("enemies");
        foreach (Node enemy in enemies)
        {
            if (enemy is EnemyController enemyController)
            {
                var distance = GlobalPosition.DistanceTo(enemyController.GlobalPosition);
                if (distance <= range)
                {
                    Attack(enemyController);
                }
            }
        }
    }

    public void Attack(EnemyController enemy)
    {
        if (enemy != null && enemy.IsAlive)
        {
            int damage = AttackValue;

            // Apply weapon damage if equipped
            if (_inventory != null && _inventory.HasEquippedWeapon())
            {
                damage += _inventory.GetEquippedWeaponDamage();
            }

            GD.Print($"{PlayerName} attacks {enemy.EnemyName} for {damage} damage.");
            enemy.TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage)
    {
        CurrentLifePoints -= damage;
        GD.Print($"{PlayerName} takes {damage} damage. Remaining HP: {CurrentLifePoints}");

        // Emit signal for UI update
        EmitSignal(SignalName.HealthChanged, CurrentLifePoints, MaxLifePoints);

        if (!IsAlive())
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        CurrentLifePoints = Mathf.Min(CurrentLifePoints + amount, MaxLifePoints);
        EmitSignal(SignalName.HealthChanged, CurrentLifePoints, MaxLifePoints);
        GD.Print($"{PlayerName} healed for {amount}. Current HP: {CurrentLifePoints}");
    }

    public bool IsAlive()
    {
        return CurrentLifePoints > 0;
    }

    private void Die()
    {
        GD.Print($"{PlayerName} has been defeated!");
        EmitSignal(SignalName.PlayerDied);
        // TODO: Implement death sequence
        // - Play death animation
        // - Show game over screen
        // - Load last save
    }

    private void OnAttackTimerTimeout()
    {
        _canAttack = true;
    }

    private void UpdateAnimation()
    {
        if (_animatedSprite == null)
            return;

        // Animation logic - will be implemented when sprites are available
        if (Velocity.Length() > 0)
        {
            // Play walk animation based on direction
            if (Mathf.Abs(Velocity.X) > Mathf.Abs(Velocity.Y))
            {
                // Horizontal movement
                _animatedSprite.FlipH = Velocity.X < 0;
                // _animatedSprite.Play("walk_side");
            }
            else
            {
                // Vertical movement
                // if (Velocity.Y < 0)
                //     _animatedSprite.Play("walk_up");
                // else
                //     _animatedSprite.Play("walk_down");
            }
        }
        else
        {
            // Play idle animation
            // _animatedSprite.Play("idle");
        }
    }

    // Signals
    [Signal]
    public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

    [Signal]
    public delegate void PlayerDiedEventHandler();
}
