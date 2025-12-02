using Godot;
using System;

/// <summary>
/// Minion enemy spawned by the Necromancer boss
/// Weak individually but dangerous in groups
/// </summary>
public partial class MinionEnemy : EnemyController
{
    [Export] public NecromancerBoss Master { get; set; }

    public override void _Ready()
    {
        // Minion stats - weak but fast
        EnemyName = "Undead Minion";
        MaxLifePoints = 20;
        CurrentLifePoints = 20;
        AttackValue = 5;
        MoveSpeed = 100.0f;
        DetectionRange = 300.0f;
        AttackRange = 40.0f;
        AttackCooldown = 1.2f;

        base._Ready();

        GD.Print($"[MinionEnemy] Spawned by {Master?.BossTitle ?? "Unknown Master"}");
    }

    protected override void Die()
    {
        // Notify master of death
        if (Master != null && IsInstanceValid(Master))
        {
            Master.OnMinionDeath();
        }

        GD.Print($"[MinionEnemy] Defeated!");
        base.Die();
    }

    public override void _PhysicsProcess(double delta)
    {
        // If master is dead or invalid, die
        if (Master == null || !IsInstanceValid(Master) || !Master.IsAlive)
        {
            GD.Print("[MinionEnemy] Master is dead, dying...");
            CurrentLifePoints = 0;
            Die();
            return;
        }

        base._PhysicsProcess(delta);
    }
}
