using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Necromancer Boss: Summons minions, curses enemies, drains life
/// Strengths: Army of minions, life drain, curses, resurrects fallen minions
/// Weaknesses: Weak in direct combat, vulnerable when minions are defeated, long cast times
/// </summary>
public partial class NecromancerBoss : BossController
{
    // Necromancer-specific properties
    [Export] public int MaxMinions { get; set; } = 5;
    [Export] public float SummonCooldown { get; set; } = 6.0f;
    [Export] public float DrainRange { get; set; } = 150.0f;
    [Export] public int DrainDamage { get; set; } = 5;
    [Export] public float DrainTickRate { get; set; } = 1.0f;

    private Timer _summonTimer;
    private bool _canSummon = true;
    private List<MinionEnemy> _minions = new List<MinionEnemy>();
    private PackedScene _minionScene;

    // Life drain
    private Timer _drainTimer;
    private bool _isDraining = false;

    // Curse
    [Export] public float CurseDuration { get; set; } = 8.0f;
    [Export] public float CurseCooldown { get; set; } = 12.0f;
    private Timer _curseTimer;
    private bool _canCurse = true;

    // Soul harvest (resurrect minions)
    [Export] public float ResurrectCooldown { get; set; } = 15.0f;
    private Timer _resurrectTimer;
    private bool _canResurrect = false;
    private int _deadMinionCount = 0;

    // Death nova
    [Export] public float DeathNovaRadius { get; set; } = 200.0f;
    [Export] public int DeathNovaDamage { get; set; } = 30;
    private bool _deathNovaTriggered = false;

    public override void _Ready()
    {
        // Necromancer stats - weak individually, strong with army
        BossTitle = "The Undying Archlich";
        EnemyName = "Necromancer Boss";
        MaxLifePoints = 150; // Lower HP
        CurrentLifePoints = 150;
        AttackValue = 8; // Very weak melee
        MoveSpeed = 100.0f; // Slow
        AttackRange = 250.0f; // Prefers range
        DetectionRange = 400.0f;
        PhaseCount = 3;

        base._Ready();

        // Load minion scene
        _minionScene = GD.Load<PackedScene>("res://scenes/enemy_minion.tscn");
        if (_minionScene == null)
        {
            GD.PrintErr("[NecromancerBoss] Failed to load minion scene!");
        }

        // Setup summon timer
        _summonTimer = new Timer();
        _summonTimer.WaitTime = SummonCooldown;
        _summonTimer.OneShot = true;
        _summonTimer.Timeout += OnSummonCooldownTimeout;
        AddChild(_summonTimer);

        // Setup drain timer
        _drainTimer = new Timer();
        _drainTimer.WaitTime = DrainTickRate;
        _drainTimer.Timeout += OnDrainTick;
        AddChild(_drainTimer);

        // Setup curse timer
        _curseTimer = new Timer();
        _curseTimer.WaitTime = CurseCooldown;
        _curseTimer.OneShot = true;
        _curseTimer.Timeout += OnCurseCooldownTimeout;
        AddChild(_curseTimer);

        // Setup resurrect timer
        _resurrectTimer = new Timer();
        _resurrectTimer.WaitTime = ResurrectCooldown;
        _resurrectTimer.OneShot = true;
        _resurrectTimer.Timeout += OnResurrectCooldownTimeout;
        AddChild(_resurrectTimer);

        // Summon initial minions
        SummonMinion();
        SummonMinion();

        GD.Print($"Necromancer Boss: Weak alone, Strong with army");
        GD.Print($"Special: Summons minions (max {MaxMinions}), Life drain, Curses, Resurrection");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsAlive)
        {
            CurrentState = EnemyState.Dead;
            return;
        }

        // Clean up dead minions from list
        _minions.RemoveAll(m => !IsInstanceValid(m));

        // Custom necromancer behavior
        switch (CurrentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Chase:
                UpdateNecromancerChase();
                break;
            case EnemyState.Attack:
                UpdateNecromancerAttack();
                break;
        }

        // Always try to maintain minion count
        if (_canSummon && _minions.Count < MaxMinions)
        {
            SummonMinion();
        }

        // Try to curse player
        if (_canCurse && _player != null && IsPlayerInRange(AttackRange))
        {
            CastCurse();
        }

        // Resurrect minions if possible
        if (_canResurrect && _deadMinionCount > 0)
        {
            ResurrectMinions();
        }

        MoveAndSlide();
        UpdateAnimation();
    }

    private void UpdateNecromancerChase()
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

        // Keep distance (necromancer prefers range)
        if (distanceToPlayer < DrainRange)
        {
            // Too close - back away
            var direction = _player.GlobalPosition.DirectionTo(GlobalPosition);
            Velocity = direction * MoveSpeed;
        }
        else if (distanceToPlayer > AttackRange)
        {
            // Too far - move closer
            var direction = GlobalPosition.DirectionTo(_player.GlobalPosition);
            Velocity = direction * MoveSpeed * 0.7f; // Move slowly
        }
        else
        {
            // Good range - attack
            CurrentState = EnemyState.Attack;
        }
    }

    private void UpdateNecromancerAttack()
    {
        if (_player == null || !_player.IsAlive())
        {
            CurrentState = EnemyState.Idle;
            return;
        }

        float distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);
        Velocity = Vector2.Zero;

        // Start draining if in range
        if (distanceToPlayer <= DrainRange && !_isDraining)
        {
            StartLifeDrain();
        }
        else if (distanceToPlayer > DrainRange && _isDraining)
        {
            StopLifeDrain();
        }

        // Out of attack range
        if (distanceToPlayer > AttackRange)
        {
            CurrentState = EnemyState.Chase;
            StopLifeDrain();
        }
    }

    protected override void OnPhaseChange(int newPhase)
    {
        base.OnPhaseChange(newPhase);

        if (newPhase == 2)
        {
            GD.Print($"{BossTitle}: \"Rise, my servants! Consume the living!\"");
            // Phase 2: More minions, faster summons
            MaxMinions = 7;
            SummonCooldown = 5.0f;
            _summonTimer.WaitTime = SummonCooldown;
            DrainDamage = 7;

            // Summon extra minions immediately
            SummonMinion();
            SummonMinion();
        }
        else if (newPhase == 3)
        {
            GD.Print($"{BossTitle}: \"You cannot kill what is already dead!\"");
            // Phase 3: Maximum army, powerful drain
            MaxMinions = 10;
            SummonCooldown = 4.0f;
            _summonTimer.WaitTime = SummonCooldown;
            DrainDamage = 10;
            DrainRange = 200.0f;

            // Enable resurrection
            _canResurrect = true;
        }
    }

    protected override void OnEnrage()
    {
        base.OnEnrage();
        GD.Print($"{BossTitle}: \"I am eternal! Death is my domain!\"");

        // Enrage: Massive army, constant drain, rapid summons
        MaxMinions = 15;
        SummonCooldown = 3.0f;
        _summonTimer.WaitTime = SummonCooldown;
        DrainDamage = 15;
        DrainTickRate = 0.5f;
        _drainTimer.WaitTime = DrainTickRate;

        // Summon extra minions
        for (int i = 0; i < 5; i++)
        {
            SummonMinion();
        }
    }

    private void SummonMinion()
    {
        if (_minions.Count >= MaxMinions) return;

        if (_minionScene == null)
        {
            GD.PrintErr("[NecromancerBoss] Cannot summon minion: scene not loaded!");
            return;
        }

        _canSummon = false;
        _summonTimer.Start();

        // Random offset around boss
        Vector2 offset = new Vector2(
            (float)GD.RandRange(-100, 100),
            (float)GD.RandRange(-100, 100)
        );

        GD.Print($"{BossTitle} summons a minion! ({_minions.Count + 1}/{MaxMinions})");

        // Instantiate minion
        var minion = _minionScene.Instantiate<MinionEnemy>();
        minion.GlobalPosition = GlobalPosition + offset;
        minion.Master = this;
        minion.Name = $"Minion_{_minions.Count}";

        // Add to scene tree
        GetParent().AddChild(minion);
        _minions.Add(minion);

        GD.Print($"[NecromancerBoss] Spawned minion at {minion.GlobalPosition}");
    }

    private void OnSummonCooldownTimeout()
    {
        _canSummon = true;
    }

    private void StartLifeDrain()
    {
        if (_isDraining) return;

        _isDraining = true;
        _drainTimer.Start();

        GD.Print($"{BossTitle} begins draining life force!");

        // TODO: Visual beam effect from boss to player
    }

    private void StopLifeDrain()
    {
        if (!_isDraining) return;

        _isDraining = false;
        _drainTimer.Stop();

        GD.Print($"{BossTitle} stops draining.");
    }

    private void OnDrainTick()
    {
        if (!_isDraining || _player == null) return;

        float distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);

        if (distanceToPlayer <= DrainRange)
        {
            GD.Print($"{BossTitle} drains {DrainDamage} HP!");
            _player.TakeDamage(DrainDamage);

            // Heal self
            CurrentLifePoints = Math.Min(CurrentLifePoints + DrainDamage, MaxLifePoints);
        }
    }

    private void CastCurse()
    {
        if (_player == null) return;

        _canCurse = false;
        _curseTimer.Start();

        GD.Print($"{BossTitle} casts a CURSE on {_player.PlayerName}!");

        // TODO: Apply curse debuff to player
        // Curse effects:
        // - Reduced movement speed (-30%)
        // - Reduced attack damage (-20%)
        // - Take damage over time (3 per second)
        // Duration: CurseDuration seconds

        // For now, just deal immediate damage
        int curseDamage = 15;
        _player.TakeDamage(curseDamage);
    }

    private void OnCurseCooldownTimeout()
    {
        _canCurse = true;
    }

    private void ResurrectMinions()
    {
        if (_deadMinionCount <= 0) return;

        _canResurrect = false;
        _resurrectTimer.Start();

        int minionsToResurrect = Math.Min(_deadMinionCount, 3);

        GD.Print($"{BossTitle} resurrects {minionsToResurrect} fallen minions!");

        for (int i = 0; i < minionsToResurrect; i++)
        {
            SummonMinion();
            _deadMinionCount--;
        }

        // TODO: Special resurrection animation/effect
    }

    private void OnResurrectCooldownTimeout()
    {
        _canResurrect = true;
    }

    public void OnMinionDeath()
    {
        _deadMinionCount++;
        GD.Print($"{BossTitle}'s minion falls! Dead minions: {_deadMinionCount}");

        // Gain power from minion death
        int powerGain = 2;
        AttackValue += powerGain;
        DrainDamage += 1;
    }

    public new void TakeDamage(int damage)
    {
        // Necromancer is frail - takes full damage
        // But has damage shield from minions
        if (_minions.Count > 0)
        {
            float damageReduction = Math.Min(_minions.Count * 0.05f, 0.3f);
            int reducedDamage = (int)(damage * (1.0f - damageReduction));
            GD.Print($"{BossTitle}'s minions shield {damage - reducedDamage} damage!");
            damage = reducedDamage;
        }

        base.TakeDamage(damage);

        // Trigger death nova when killed
        if (!IsAlive && !_deathNovaTriggered)
        {
            TriggerDeathNova();
        }
    }

    private void TriggerDeathNova()
    {
        _deathNovaTriggered = true;

        GD.Print($"{BossTitle} releases a DEATH NOVA in final moments!");

        // Deal massive AOE damage
        if (_player != null)
        {
            float distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
            if (distance <= DeathNovaRadius)
            {
                float damageMultiplier = 1.0f - (distance / DeathNovaRadius) * 0.5f;
                int damage = (int)(DeathNovaDamage * damageMultiplier);

                GD.Print($"Death Nova hits {_player.PlayerName} for {damage} damage!");
                _player.TakeDamage(damage);
            }
        }

        // TODO: Massive particle explosion effect
    }
}
