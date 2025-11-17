using Godot;
using System;

/// <summary>
/// Tank Boss: High HP, slow movement, devastating attacks
/// Strengths: Massive health pool, high damage resistance, powerful melee attacks
/// Weaknesses: Slow movement speed, vulnerable to hit-and-run tactics, long attack cooldowns
/// </summary>
public partial class TankBoss : BossController
{
    // Tank-specific properties
    [Export] public float DamageReduction { get; set; } = 0.2f; // 20% damage reduction
    [Export] public float GroundPoundRadius { get; set; } = 150.0f;
    [Export] public int GroundPoundDamage { get; set; } = 25;

    private Timer _groundPoundTimer;
    private bool _canGroundPound = true;
    private float _groundPoundCooldown = 5.0f;

    // Armor stacks
    private int _armorStacks = 0;
    private const int MaxArmorStacks = 5;

    public override void _Ready()
    {
        // Tank stats
        BossTitle = "The Iron Colossus";
        EnemyName = "Tank Boss";
        MaxLifePoints = 300;
        CurrentLifePoints = 300;
        AttackValue = 25;
        MoveSpeed = 60.0f; // Slow
        AttackRange = 60.0f;
        DetectionRange = 250.0f;
        PhaseCount = 2;

        base._Ready();

        // Setup ground pound timer
        _groundPoundTimer = new Timer();
        _groundPoundTimer.WaitTime = _groundPoundCooldown;
        _groundPoundTimer.OneShot = true;
        _groundPoundTimer.Timeout += OnGroundPoundCooldownTimeout;
        AddChild(_groundPoundTimer);

        GD.Print($"Tank Boss: High HP ({MaxLifePoints}), Slow ({MoveSpeed}), High Damage ({AttackValue})");
        GD.Print($"Special: Ground Pound AOE attack, Damage Reduction: {DamageReduction * 100}%");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        // Use Ground Pound when player is in range
        if (CurrentState == EnemyState.Attack && _canGroundPound && IsPlayerInGroundPoundRange())
        {
            PerformGroundPound();
        }
    }

    protected override void OnPhaseChange(int newPhase)
    {
        base.OnPhaseChange(newPhase);

        if (newPhase == 2)
        {
            GD.Print($"{BossTitle}: \"You dare challenge my might? Feel the earth tremble!\"");
            // Phase 2: Faster attacks, more frequent ground pounds
            _groundPoundCooldown = 3.0f;
            _groundPoundTimer.WaitTime = _groundPoundCooldown;
            AttackValue += 5;
            GainArmorStack();
        }
    }

    protected override void OnEnrage()
    {
        base.OnEnrage();
        GD.Print($"{BossTitle}: \"ENOUGH! I will crush you all!\"");

        // Enrage: Even more damage, faster ground pounds
        GroundPoundDamage += 10;
        _groundPoundCooldown = 2.0f;
        _groundPoundTimer.WaitTime = _groundPoundCooldown;

        // Gain maximum armor stacks
        while (_armorStacks < MaxArmorStacks)
        {
            GainArmorStack();
        }
    }

    private bool IsPlayerInGroundPoundRange()
    {
        if (_player == null) return false;
        return GlobalPosition.DistanceTo(_player.GlobalPosition) <= GroundPoundRadius;
    }

    private void PerformGroundPound()
    {
        _canGroundPound = false;
        _groundPoundTimer.Start();

        GD.Print($"{BossTitle} performs GROUND POUND!");
        Velocity = Vector2.Zero; // Stop moving during ground pound

        // Deal AOE damage
        var enemies = GetTree().GetNodesInGroup("player");
        foreach (var node in enemies)
        {
            if (node is PlayerController player)
            {
                float distance = GlobalPosition.DistanceTo(player.GlobalPosition);
                if (distance <= GroundPoundRadius)
                {
                    // Damage falls off with distance
                    float damageMultiplier = 1.0f - (distance / GroundPoundRadius) * 0.5f;
                    int damage = (int)(GroundPoundDamage * damageMultiplier);

                    GD.Print($"Ground Pound hits {player.PlayerName} for {damage} damage!");
                    player.TakeDamage(damage);

                    // TODO: Apply knockback
                }
            }
        }

        // Visual effect would go here
        // TODO: Spawn ground pound particles/animation
    }

    private void OnGroundPoundCooldownTimeout()
    {
        _canGroundPound = true;
    }

    public new void TakeDamage(int damage)
    {
        // Apply damage reduction
        int reducedDamage = (int)(damage * (1.0f - DamageReduction - (_armorStacks * 0.05f)));
        reducedDamage = Math.Max(1, reducedDamage); // Minimum 1 damage

        if (reducedDamage < damage)
        {
            GD.Print($"{BossTitle}'s armor absorbs {damage - reducedDamage} damage!");
        }

        base.TakeDamage(reducedDamage);
    }

    private void GainArmorStack()
    {
        if (_armorStacks < MaxArmorStacks)
        {
            _armorStacks++;
            GD.Print($"{BossTitle} gains armor! Stacks: {_armorStacks}/{MaxArmorStacks}");
        }
    }
}
