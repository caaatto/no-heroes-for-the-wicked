using Godot;
using System;

/// <summary>
/// Generic projectile that can be shot by enemies or the player
/// Handles movement, collision, and damage
/// </summary>
public partial class Projectile : Area2D
{
    [Export] public float Speed { get; set; } = 300.0f;
    [Export] public int Damage { get; set; } = 10;
    [Export] public float Lifetime { get; set; } = 5.0f;
    [Export] public bool IsPlayerProjectile { get; set; } = false;
    [Export] public Color ProjectileColor { get; set; } = new Color(1.0f, 0.5f, 0.0f);

    private Vector2 _direction;
    private Timer _lifetimeTimer;
    private Sprite2D _sprite;
    private CollisionShape2D _collisionShape;

    public override void _Ready()
    {
        // Setup collision layers
        if (IsPlayerProjectile)
        {
            CollisionLayer = 4; // Player projectile layer
            CollisionMask = 2;  // Hit enemies
        }
        else
        {
            CollisionLayer = 8; // Enemy projectile layer
            CollisionMask = 1;  // Hit player
        }

        // Create visual representation
        CreateVisuals();

        // Setup lifetime timer
        _lifetimeTimer = new Timer();
        _lifetimeTimer.WaitTime = Lifetime;
        _lifetimeTimer.OneShot = true;
        _lifetimeTimer.Timeout += OnLifetimeExpired;
        AddChild(_lifetimeTimer);
        _lifetimeTimer.Start();

        // Connect collision signal
        AreaEntered += OnAreaEntered;
        BodyEntered += OnBodyEntered;

        GD.Print($"[Projectile] Created at {GlobalPosition} with direction {_direction}");
    }

    /// <summary>
    /// Create visual representation of the projectile
    /// </summary>
    private void CreateVisuals()
    {
        // Create sprite
        _sprite = new Sprite2D();
        _sprite.Modulate = ProjectileColor;
        AddChild(_sprite);

        // Create a simple circular texture programmatically
        var image = Image.CreateEmpty(16, 16, false, Image.Format.Rgba8);
        image.Fill(Colors.Transparent);

        // Draw a circle
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float dx = x - 8;
                float dy = y - 8;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                if (distance <= 6.0f)
                {
                    // Gradient from center
                    float alpha = 1.0f - (distance / 6.0f) * 0.3f;
                    var color = ProjectileColor;
                    color.A = alpha;
                    image.SetPixel(x, y, color);
                }
            }
        }

        var texture = ImageTexture.CreateFromImage(image);
        _sprite.Texture = texture;
        _sprite.Scale = new Vector2(1.5f, 1.5f);

        // Create collision shape
        _collisionShape = new CollisionShape2D();
        var circleShape = new CircleShape2D();
        circleShape.Radius = 8.0f;
        _collisionShape.Shape = circleShape;
        AddChild(_collisionShape);
    }

    /// <summary>
    /// Set the direction for the projectile to travel
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        _direction = direction.Normalized();
        Rotation = direction.Angle();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Move projectile
        Position += _direction * Speed * (float)delta;
    }

    /// <summary>
    /// Called when projectile collides with an area
    /// </summary>
    private void OnAreaEntered(Area2D area)
    {
        // Check if it's an attack hitbox or other relevant area
        GD.Print($"[Projectile] Area entered: {area.Name}");
    }

    /// <summary>
    /// Called when projectile collides with a body
    /// </summary>
    private void OnBodyEntered(Node2D body)
    {
        GD.Print($"[Projectile] Hit {body.Name}");

        if (IsPlayerProjectile)
        {
            // Player projectile hitting enemy
            if (body is EnemyController enemy && enemy.IsAlive)
            {
                enemy.TakeDamage(Damage);
                GD.Print($"[Projectile] Player projectile hit {enemy.EnemyName} for {Damage} damage");
                Explode();
            }
        }
        else
        {
            // Enemy projectile hitting player
            if (body is PlayerController player && player.IsAlive())
            {
                player.TakeDamage(Damage);
                GD.Print($"[Projectile] Enemy projectile hit {player.PlayerName} for {Damage} damage");
                Explode();
            }
        }

        // Hit wall or obstacle
        if (body is StaticBody2D)
        {
            GD.Print("[Projectile] Hit wall");
            Explode();
        }
        // Check for TileMapLayer (Godot 4.x)
        else if (body.GetType().Name.Contains("TileMap"))
        {
            GD.Print("[Projectile] Hit tilemap");
            Explode();
        }
    }

    /// <summary>
    /// Called when lifetime expires
    /// </summary>
    private void OnLifetimeExpired()
    {
        GD.Print("[Projectile] Lifetime expired");
        QueueFree();
    }

    /// <summary>
    /// Explode and destroy the projectile
    /// </summary>
    private void Explode()
    {
        // TODO: Add explosion effect/particles
        QueueFree();
    }
}
