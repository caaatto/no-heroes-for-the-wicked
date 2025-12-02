using Godot;
using System;

/// <summary>
/// Assassin enemy - stealthy, high damage, low health
/// Can teleport behind player
/// </summary>
public partial class AssassinEnemy : EnemyController
{
    [Export] public float TeleportCooldown { get; set; } = 8.0f;
    [Export] public float TeleportRange { get; set; } = 300.0f;
    [Export] public float BackstabMultiplier { get; set; } = 2.0f;
    [Export] public float InvisibilityDuration { get; set; } = 2.0f;

    private float _teleportTimer = 0.0f;
    private bool _isInvisible = false;
    private float _invisibilityTimer = 0.0f;

    public override void _Ready()
    {
        // Assassin stats - glass cannon
        MaxLifePoints = 35;
        CurrentLifePoints = MaxLifePoints;
        AttackValue = 20; // High damage
        MoveSpeed = 150.0f;
        DetectionRange = 220.0f;
        AttackRange = 50.0f;
        AttackCooldown = 1.0f;

        base._Ready();

        _teleportTimer = TeleportCooldown; // Start ready to teleport

        GD.Print("[AssassinEnemy] Assassin initialized");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        // Update timers
        if (_teleportTimer > 0)
        {
            _teleportTimer -= (float)delta;
        }

        if (_invisibilityTimer > 0)
        {
            _invisibilityTimer -= (float)delta;
            if (_invisibilityTimer <= 0)
            {
                EndInvisibility();
            }
        }

        // Try to teleport when in chase state
        if (CurrentState == EnemyState.Chase && _teleportTimer <= 0 && !_isInvisible)
        {
            if (_player != null && GlobalPosition.DistanceTo(_player.GlobalPosition) <= TeleportRange)
            {
                if (GD.Randf() < 0.4f) // 40% chance to teleport
                {
                    TeleportBehindPlayer();
                }
            }
        }
    }

    /// <summary>
    /// Teleport behind the player
    /// </summary>
    private void TeleportBehindPlayer()
    {
        if (_player == null) return;

        _teleportTimer = TeleportCooldown;

        // Calculate position behind player
        // Assume player is facing right by default, teleport behind
        Vector2 behindOffset = new Vector2(-60, 0); // 60 units behind
        Vector2 teleportPosition = _player.GlobalPosition + behindOffset;

        GlobalPosition = teleportPosition;

        // Enter stealth briefly
        StartInvisibility();

        GD.Print("[AssassinEnemy] Teleported behind player!");
    }

    /// <summary>
    /// Start invisibility
    /// </summary>
    private void StartInvisibility()
    {
        _isInvisible = true;
        _invisibilityTimer = InvisibilityDuration;

        // Reduce opacity (visual feedback)
        Modulate = new Color(1, 1, 1, 0.3f);

        GD.Print("[AssassinEnemy] Entered stealth");
    }

    /// <summary>
    /// End invisibility
    /// </summary>
    private void EndInvisibility()
    {
        _isInvisible = false;
        Modulate = new Color(1, 1, 1, 1.0f);

        GD.Print("[AssassinEnemy] Stealth ended");
    }

    protected override void AttackPlayer()
    {
        if (_player == null || !_canAttack) return;

        _canAttack = false;
        _attackTimer.Start();

        // Check if attacking from behind (backstab)
        bool isBackstab = IsAttackingFromBehind();
        int damage = AttackValue;

        if (isBackstab)
        {
            damage = (int)(AttackValue * BackstabMultiplier);
            GD.Print("[AssassinEnemy] BACKSTAB!");
        }

        // Deal damage to player
        GD.Print($"[AssassinEnemy] Attacks player for {damage} damage");

        // Exit invisibility after attack
        if (_isInvisible)
        {
            EndInvisibility();
        }

        // EmitSignal(SignalName.EnemyAttacked, damage); // Signal not defined in base class
    }

    /// <summary>
    /// Check if attacking from behind
    /// </summary>
    private bool IsAttackingFromBehind()
    {
        if (_player == null) return false;

        Vector2 toEnemy = (GlobalPosition - _player.GlobalPosition).Normalized();
        // Simplified: assume player faces right, check if enemy is to the left
        return toEnemy.X < 0;
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        // Exit invisibility when damaged
        if (_isInvisible)
        {
            EndInvisibility();
        }

        // 25% chance to teleport away when hit
        if (_teleportTimer <= 0 && GD.Randf() < 0.25f)
        {
            TeleportToSafety();
        }
    }

    /// <summary>
    /// Teleport to safety when damaged
    /// </summary>
    private void TeleportToSafety()
    {
        if (_player == null) return;

        _teleportTimer = TeleportCooldown;

        // Teleport away from player
        Vector2 awayDirection = (GlobalPosition - _player.GlobalPosition).Normalized();
        Vector2 safePosition = GlobalPosition + awayDirection * 150.0f;

        GlobalPosition = safePosition;

        StartInvisibility();

        GD.Print("[AssassinEnemy] Teleported to safety!");
    }
}
