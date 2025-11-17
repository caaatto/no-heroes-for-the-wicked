using Godot;
using System;

/// <summary>
/// Berserker Boss: Scales in power as health decreases, unpredictable and aggressive
/// Strengths: Gets stronger as HP drops, lifesteal, area cleave attacks, unstoppable at low HP
/// Weaknesses: Vulnerable at full HP, predictable aggression, can be burst down early
/// </summary>
public partial class BerserkerBoss : BossController
{
    // Berserker-specific properties
    [Export] public float RageMultiplier { get; set; } = 0.5f; // Bonus per 20% HP lost
    [Export] public float LifestealPercent { get; set; } = 0.15f; // 15% lifesteal
    [Export] public int CleaveRadius { get; set; } = 80;

    private int _baseAttackValue;
    private float _baseMoveSpeed;
    private float _currentRageMultiplier = 0;

    // Execution threshold
    [Export] public float ExecuteThreshold { get; set; } = 0.15f; // When below 15% HP
    [Export] public bool IsExecuteMode { get; set; } = false;

    // Whirlwind attack
    private Timer _whirlwindTimer;
    private bool _canWhirlwind = true;
    private float _whirlwindCooldown = 8.0f;
    private bool _isWhirlwinding = false;
    private float _whirlwindDuration = 3.0f;
    private float _whirlwindTimeRemaining = 0;

    // Blood trail (leaves damaging zones when low HP)
    private bool _leavesBloodTrail = false;
    private float _bloodTrailTimer = 0;

    public override void _Ready()
    {
        // Berserker stats - starts weaker but scales
        BossTitle = "The Blood Reaver";
        EnemyName = "Berserker Boss";
        MaxLifePoints = 220;
        CurrentLifePoints = 220;
        AttackValue = 18; // Moderate starting damage
        MoveSpeed = 140.0f; // Moderate starting speed
        AttackRange = 55.0f;
        DetectionRange = 350.0f;
        PhaseCount = 4; // More phases due to HP-based scaling

        _baseAttackValue = AttackValue;
        _baseMoveSpeed = MoveSpeed;

        base._Ready();

        // Setup whirlwind timer
        _whirlwindTimer = new Timer();
        _whirlwindTimer.WaitTime = _whirlwindCooldown;
        _whirlwindTimer.OneShot = true;
        _whirlwindTimer.Timeout += OnWhirlwindCooldownTimeout;
        AddChild(_whirlwindTimer);

        GD.Print($"Berserker Boss: Scales with rage, Lifesteal: {LifestealPercent * 100}%");
        GD.Print($"Special: Gets stronger as HP drops, Whirlwind attack, Execute mode at low HP");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsAlive)
        {
            CurrentState = EnemyState.Dead;
            return;
        }

        // Update rage multiplier based on missing HP
        UpdateRageMultiplier();

        // Process whirlwind
        if (_isWhirlwinding)
        {
            ProcessWhirlwind(delta);
        }
        else
        {
            base._PhysicsProcess(delta);

            // Use whirlwind when surrounded or at low HP
            if (_canWhirlwind && ShouldUseWhirlwind())
            {
                StartWhirlwind();
            }
        }

        // Blood trail at low HP
        if (_leavesBloodTrail)
        {
            _bloodTrailTimer += (float)delta;
            if (_bloodTrailTimer >= 0.5f)
            {
                CreateBloodTrail();
                _bloodTrailTimer = 0;
            }
        }
    }

    private void UpdateRageMultiplier()
    {
        float healthPercent = GetHealthPercentage();
        float hpLost = 1.0f - healthPercent;

        // Exponential scaling: gets stronger faster as HP drops
        _currentRageMultiplier = hpLost * hpLost * 2.0f;

        // Apply rage bonuses
        AttackValue = (int)(_baseAttackValue * (1 + _currentRageMultiplier));
        MoveSpeed = _baseMoveSpeed * (1 + _currentRageMultiplier * 0.5f);

        // Check for execute mode
        if (!IsExecuteMode && healthPercent <= ExecuteThreshold)
        {
            EnterExecuteMode();
        }
    }

    protected override void OnPhaseChange(int newPhase)
    {
        base.OnPhaseChange(newPhase);

        string[] taunts = {
            "First blood!",
            "BLOOD FOR BLOOD!",
            "I WILL FEAST ON YOUR BONES!",
            "NOTHING CAN STOP ME NOW!"
        };

        if (newPhase <= taunts.Length)
        {
            GD.Print($"{BossTitle}: \"{taunts[newPhase - 1]}\"");
        }

        switch (newPhase)
        {
            case 2: // 75% HP
                LifestealPercent = 0.20f;
                _whirlwindCooldown = 7.0f;
                _whirlwindTimer.WaitTime = _whirlwindCooldown;
                break;

            case 3: // 50% HP
                LifestealPercent = 0.25f;
                _whirlwindCooldown = 6.0f;
                _whirlwindTimer.WaitTime = _whirlwindCooldown;
                _leavesBloodTrail = true;
                break;

            case 4: // 25% HP
                LifestealPercent = 0.30f;
                _whirlwindCooldown = 5.0f;
                _whirlwindTimer.WaitTime = _whirlwindCooldown;
                break;
        }

        GD.Print($"{BossTitle} grows more powerful! Rage multiplier: {_currentRageMultiplier:F2}x");
        GD.Print($"Current stats - ATK: {AttackValue}, SPD: {MoveSpeed:F0}, Lifesteal: {LifestealPercent * 100}%");
    }

    protected override void OnEnrage()
    {
        base.OnEnrage();
        GD.Print($"{BossTitle}: \"RAAAAAAAGH! I AM UNSTOPPABLE!\"");

        // Maximum rage bonuses
        _currentRageMultiplier += 1.0f;
        LifestealPercent = 0.40f;
        _whirlwindCooldown = 3.0f;
        _whirlwindTimer.WaitTime = _whirlwindCooldown;

        // Permanent blood trail
        _leavesBloodTrail = true;

        // Immediately use whirlwind
        if (_canWhirlwind)
        {
            StartWhirlwind();
        }
    }

    private void EnterExecuteMode()
    {
        IsExecuteMode = true;
        GD.Print($"{BossTitle} enters EXECUTE MODE!");
        GD.Print($"{BossTitle}: \"Death comes for us all... starting with YOU!\"");

        // Massive bonuses
        AttackValue += 15;
        MoveSpeed *= 1.5f;
        LifestealPercent = 0.50f; // 50% lifesteal!
        _whirlwindDuration = 5.0f; // Longer whirlwinds

        // Visual effect
        // TODO: Red glow, particle effects
    }

    public new void Attack(PlayerController player)
    {
        if (player == null || !player.IsAlive())
            return;

        _canAttack = false;
        _attackTimer.Start();

        int damage = AttackValue;

        // Cleave attack - hits in an area
        GD.Print($"{BossTitle} performs a CLEAVING strike for {damage} damage!");
        player.TakeDamage(damage);

        // Lifesteal
        int healAmount = (int)(damage * LifestealPercent);
        Heal(healAmount);

        // Execute bonus
        if (IsExecuteMode && player.GetHealthPercentage() < 0.30f)
        {
            int executeDamage = damage / 2;
            GD.Print($"{BossTitle} deals {executeDamage} EXECUTE damage to wounded prey!");
            player.TakeDamage(executeDamage);
        }
    }

    private void Heal(int amount)
    {
        CurrentLifePoints = Math.Min(CurrentLifePoints + amount, MaxLifePoints);
        GD.Print($"{BossTitle} lifesteals {amount} HP! Current HP: {CurrentLifePoints}/{MaxLifePoints}");

        // TODO: Visual healing effect
    }

    private bool ShouldUseWhirlwind()
    {
        // Use whirlwind at low HP or when player is close
        if (IsExecuteMode) return true;

        if (_player != null)
        {
            float distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
            if (distance < 100.0f) return true;
        }

        return false;
    }

    private void StartWhirlwind()
    {
        _canWhirlwind = false;
        _isWhirlwinding = true;
        _whirlwindTimeRemaining = _whirlwindDuration;
        _whirlwindTimer.Start();

        GD.Print($"{BossTitle} starts WHIRLWIND attack!");

        // TODO: Play whirlwind animation and sound
    }

    private void ProcessWhirlwind(double delta)
    {
        _whirlwindTimeRemaining -= (float)delta;

        if (_whirlwindTimeRemaining <= 0)
        {
            _isWhirlwinding = false;
            GD.Print($"{BossTitle} ends whirlwind.");
            return;
        }

        // Spin and move
        // TODO: Rotate sprite
        if (_player != null)
        {
            var direction = GlobalPosition.DirectionTo(_player.GlobalPosition);
            Velocity = direction * MoveSpeed * 1.2f;
        }

        MoveAndSlide();

        // Deal damage to nearby players
        if (_player != null)
        {
            float distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
            if (distance <= CleaveRadius)
            {
                int whirlwindDamage = AttackValue / 3; // Lower damage but continuous
                _player.TakeDamage(whirlwindDamage);

                // Lifesteal from whirlwind
                int healAmount = (int)(whirlwindDamage * LifestealPercent);
                Heal(healAmount);
            }
        }
    }

    private void OnWhirlwindCooldownTimeout()
    {
        _canWhirlwind = true;
    }

    private void CreateBloodTrail()
    {
        GD.Print($"{BossTitle} leaves a blood trail!");

        // TODO: Create Area2D that damages players who step in it
        // Blood trail would last for 5 seconds and deal damage over time
    }

    public new void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        // Gain rage when hit
        if (IsAlive)
        {
            float rageGain = 0.05f;
            _currentRageMultiplier += rageGain;
            GD.Print($"{BossTitle} gains rage from pain! (+{rageGain:F2}x)");
        }
    }
}
