using Godot;
using System;

/// <summary>
/// Speed Boss: Low HP, extremely fast, rapid attacks with dash ability
/// Strengths: High mobility, rapid attack speed, difficult to hit, dash ability
/// Weaknesses: Low health pool, vulnerable when attack patterns are learned, predictable dashes
/// </summary>
public partial class SpeedBoss : BossController
{
    // Speed-specific properties
    [Export] public float DashSpeed { get; set; } = 400.0f;
    [Export] public float DashDistance { get; set; } = 200.0f;
    [Export] public float DashCooldown { get; set; } = 2.0f;

    private Timer _dashTimer;
    private bool _canDash = true;
    private bool _isDashing = false;
    private Vector2 _dashDirection;
    private float _dashTimeRemaining = 0;

    // Combo attack
    private int _comboCount = 0;
    private const int MaxComboHits = 3;
    private Timer _comboResetTimer;

    // Afterimage effect
    private int _afterimageCount = 0;
    private const int MaxAfterimages = 5;

    public override void _Ready()
    {
        // Speed stats
        BossTitle = "The Shadow Blade";
        EnemyName = "Speed Boss";
        MaxLifePoints = 120; // Low HP
        CurrentLifePoints = 120;
        AttackValue = 12; // Lower damage per hit
        MoveSpeed = 180.0f; // Very fast
        AttackRange = 45.0f;
        DetectionRange = 300.0f;
        PhaseCount = 2;

        base._Ready();

        // Setup dash timer
        _dashTimer = new Timer();
        _dashTimer.WaitTime = DashCooldown;
        _dashTimer.OneShot = true;
        _dashTimer.Timeout += OnDashCooldownTimeout;
        AddChild(_dashTimer);

        // Setup combo reset timer
        _comboResetTimer = new Timer();
        _comboResetTimer.WaitTime = 2.0f;
        _comboResetTimer.OneShot = true;
        _comboResetTimer.Timeout += OnComboResetTimeout;
        AddChild(_comboResetTimer);

        // Faster attack speed
        _attackCooldown = 0.5f;

        GD.Print($"Speed Boss: Low HP ({MaxLifePoints}), Very Fast ({MoveSpeed}), Rapid Attacks");
        GD.Print($"Special: Dash ability, Combo attacks, Afterimages");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isDashing)
        {
            ProcessDash(delta);
        }
        else
        {
            base._PhysicsProcess(delta);

            // Try to dash when chasing
            if (CurrentState == EnemyState.Chase && _canDash && ShouldDash())
            {
                PerformDash();
            }
        }
    }

    protected override void OnPhaseChange(int newPhase)
    {
        base.OnPhaseChange(newPhase);

        if (newPhase == 2)
        {
            GD.Print($"{BossTitle}: \"You're too slow! Can you keep up?\"");
            // Phase 2: Even faster, more frequent dashes
            MoveSpeed = 220.0f;
            DashCooldown = 1.5f;
            _dashTimer.WaitTime = DashCooldown;
            _attackCooldown = 0.4f;

            CreateAfterimage();
        }
    }

    protected override void OnEnrage()
    {
        base.OnEnrage();
        GD.Print($"{BossTitle}: \"I am speed incarnate!\"");

        // Enrage: Maximum speed, constant afterimages
        MoveSpeed = 250.0f;
        DashSpeed = 500.0f;
        DashCooldown = 1.0f;
        _dashTimer.WaitTime = DashCooldown;
        _attackCooldown = 0.3f;

        // Create multiple afterimages
        for (int i = 0; i < MaxAfterimages; i++)
        {
            CreateAfterimage();
        }
    }

    private bool ShouldDash()
    {
        if (_player == null) return false;

        float distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);

        // Dash when player is at medium range
        return distanceToPlayer > 100.0f && distanceToPlayer < 250.0f;
    }

    private void PerformDash()
    {
        if (_player == null) return;

        _canDash = false;
        _isDashing = true;
        _dashTimer.Start();

        // Dash towards player's position
        _dashDirection = GlobalPosition.DirectionTo(_player.GlobalPosition);
        _dashTimeRemaining = DashDistance / DashSpeed;

        GD.Print($"{BossTitle} dashes!");
        CreateAfterimage();

        // TODO: Play dash sound/animation
    }

    private void ProcessDash(double delta)
    {
        _dashTimeRemaining -= (float)delta;

        if (_dashTimeRemaining <= 0)
        {
            _isDashing = false;
            Velocity = Vector2.Zero;
            return;
        }

        Velocity = _dashDirection * DashSpeed;
        MoveAndSlide();

        // Check for collision with player during dash
        if (IsPlayerInRange(AttackRange))
        {
            // Dash attack!
            if (_player != null && _canAttack)
            {
                int dashDamage = AttackValue + 5; // Bonus damage on dash attack
                GD.Print($"{BossTitle} hits with DASH ATTACK for {dashDamage} damage!");
                _player.TakeDamage(dashDamage);
                _canAttack = false;
            }
        }
    }

    private void OnDashCooldownTimeout()
    {
        _canDash = true;
    }

    public new void Attack(PlayerController player)
    {
        if (player == null || !player.IsAlive())
            return;

        _canAttack = false;
        _attackTimer.Start();

        // Build combo
        _comboCount++;
        _comboResetTimer.Start();

        int damage = AttackValue;
        string attackType = "slashes";

        if (_comboCount >= MaxComboHits)
        {
            // Finisher attack
            damage = (int)(AttackValue * 2.0f);
            attackType = "executes a COMBO FINISHER on";
            _comboCount = 0;
            GD.Print($"{BossTitle} completes a {MaxComboHits}-hit combo!");
        }

        GD.Print($"{BossTitle} {attackType} {player.PlayerName} for {damage} damage. (Combo: {_comboCount})");
        player.TakeDamage(damage);
    }

    private void OnComboResetTimeout()
    {
        if (_comboCount > 0)
        {
            GD.Print($"{BossTitle}'s combo resets.");
            _comboCount = 0;
        }
    }

    private void CreateAfterimage()
    {
        if (_afterimageCount >= MaxAfterimages) return;

        _afterimageCount++;
        GD.Print($"{BossTitle} creates an afterimage! ({_afterimageCount}/{MaxAfterimages})");

        // TODO: Spawn visual afterimage effect that fades out
        // This would be a sprite that appears at current position and fades
    }

    public new void TakeDamage(int damage)
    {
        // Chance to dodge attacks when not dashing
        if (!_isDashing && GD.Randf() < 0.15f) // 15% dodge chance
        {
            GD.Print($"{BossTitle} dodges the attack with incredible speed!");

            // Counter-dash towards attacker
            if (_canDash)
            {
                PerformDash();
            }
            return;
        }

        base.TakeDamage(damage);

        // Create afterimage when hit
        if (IsAlive && _afterimageCount < MaxAfterimages)
        {
            CreateAfterimage();
        }
    }
}
