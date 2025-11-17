using Godot;
using System;
using System.Collections.Generic;

public partial class AttackHitbox : Area2D
{
    [Export] public int Damage { get; set; } = 10;
    [Export] public float Lifetime { get; set; } = 0.2f;
    [Export] public bool IsPlayerAttack { get; set; } = true;

    private Timer _lifetimeTimer;
    private HashSet<Node2D> _hitTargets = new HashSet<Node2D>();
    private CollisionShape2D _collisionShape;
    private Sprite2D _visualEffect;

    public override void _Ready()
    {
        // Setup collision layers
        if (IsPlayerAttack)
        {
            CollisionLayer = 0; // Don't collide with anything
            CollisionMask = 2;  // Only detect enemies (layer 2)
        }
        else
        {
            CollisionLayer = 0;
            CollisionMask = 1;  // Only detect player (layer 1)
        }

        _collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        _visualEffect = GetNodeOrNull<Sprite2D>("VisualEffect");

        // Connect signals
        AreaEntered += OnAreaEntered;
        BodyEntered += OnBodyEntered;

        // Setup lifetime timer
        _lifetimeTimer = new Timer();
        _lifetimeTimer.WaitTime = Lifetime;
        _lifetimeTimer.OneShot = true;
        _lifetimeTimer.Timeout += () => QueueFree();
        AddChild(_lifetimeTimer);
        _lifetimeTimer.Start();

        // Flash effect
        if (_visualEffect != null)
        {
            var tween = CreateTween();
            tween.TweenProperty(_visualEffect, "modulate:a", 0.0, Lifetime);
        }
    }

    private void OnAreaEntered(Area2D area)
    {
        // Handle area-based collision if needed
    }

    private void OnBodyEntered(Node2D body)
    {
        // Prevent hitting same target twice
        if (_hitTargets.Contains(body))
            return;

        _hitTargets.Add(body);

        if (IsPlayerAttack)
        {
            // Player attacking enemy
            if (body is EnemyController enemy && enemy.IsAlive)
            {
                enemy.TakeDamage(Damage);
                GD.Print($"Hitbox damaged enemy for {Damage}");

                // Create hit effect
                CreateHitEffect(enemy.GlobalPosition);

                // Update quest progress
                if (!enemy.IsAlive)
                {
                    var questSystem = GetNodeOrNull<QuestSystem>("/root/QuestSystem");
                    if (questSystem != null)
                    {
                        questSystem.UpdateQuestProgress("enemy", "troll");
                    }
                }
            }
        }
        else
        {
            // Enemy attacking player
            if (body is PlayerController player && player.IsAlive())
            {
                player.TakeDamage(Damage);
                GD.Print($"Hitbox damaged player for {Damage}");

                // Create hit effect
                CreateHitEffect(player.GlobalPosition);
            }
        }
    }

    private void CreateHitEffect(Vector2 position)
    {
        // Create a simple hit effect (will be enhanced with particles later)
        var hitMarker = new Node2D();
        hitMarker.Position = position;
        GetTree().Root.AddChild(hitMarker);

        var label = new Label();
        label.Text = $"-{Damage}";
        label.Position = Vector2.Zero;
        label.AddThemeColorOverride("font_color", IsPlayerAttack ? Colors.Red : Colors.Orange);
        label.AddThemeFontSizeOverride("font_size", 20);
        hitMarker.AddChild(label);

        // Animate damage number
        var tween = CreateTween();
        tween.TweenProperty(label, "position", new Vector2(0, -50), 1.0);
        tween.Parallel().TweenProperty(label, "modulate:a", 0.0, 1.0);
        tween.TweenCallback(Callable.From(() => hitMarker.QueueFree()));
    }
}
