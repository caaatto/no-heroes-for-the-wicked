using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages particle effects and visual effects throughout the game.
/// Provides pooling and easy spawning of various effect types.
/// </summary>
public partial class ParticleEffectManager : Node2D
{
    [Signal]
    public delegate void EffectSpawnedEventHandler(string effectName, Vector2 position);

    // Effect pool
    private Dictionary<string, List<GpuParticles2D>> _effectPools = new Dictionary<string, List<GpuParticles2D>>();
    private const int POOL_SIZE_PER_EFFECT = 10;

    // Effect definitions
    private Dictionary<string, Func<GpuParticles2D>> _effectTemplates = new Dictionary<string, Func<GpuParticles2D>>();

    public override void _Ready()
    {
        // Register all effect templates
        RegisterEffectTemplates();

        // Pre-warm effect pools
        PrewarmPools();

        GD.Print("[ParticleEffectManager] Initialized particle system");
    }

    /// <summary>
    /// Register all available particle effect templates
    /// </summary>
    private void RegisterEffectTemplates()
    {
        _effectTemplates["hit_impact"] = CreateHitImpactEffect;
        _effectTemplates["blood_splatter"] = CreateBloodSplatterEffect;
        _effectTemplates["death_explosion"] = CreateDeathExplosionEffect;
        _effectTemplates["heal"] = CreateHealEffect;
        _effectTemplates["level_up"] = CreateLevelUpEffect;
        _effectTemplates["dash_trail"] = CreateDashTrailEffect;
        // _effectTemplates["explosion"] = CreateExplosionEffect; // Method not implemented yet
        _effectTemplates["magic_cast"] = CreateMagicCastEffect;
        _effectTemplates["teleport"] = CreateTeleportEffect;
        _effectTemplates["power_up"] = CreatePowerUpEffect;
        _effectTemplates["smoke"] = CreateSmokeEffect;
        _effectTemplates["sparkles"] = CreateSparklesEffect;
        _effectTemplates["fire"] = CreateFireEffect;
        _effectTemplates["poison_cloud"] = CreatePoisonCloudEffect;
        _effectTemplates["lightning"] = CreateLightningEffect;
        _effectTemplates["shield_break"] = CreateShieldBreakEffect;
    }

    /// <summary>
    /// Pre-warm effect pools
    /// </summary>
    private void PrewarmPools()
    {
        foreach (var effectName in _effectTemplates.Keys)
        {
            _effectPools[effectName] = new List<GpuParticles2D>();

            for (int i = 0; i < POOL_SIZE_PER_EFFECT; i++)
            {
                var effect = _effectTemplates[effectName]();
                effect.Emitting = false;
                effect.Visible = false;
                AddChild(effect);
                _effectPools[effectName].Add(effect);
            }
        }
    }

    /// <summary>
    /// Spawn a particle effect at a position
    /// </summary>
    public void SpawnEffect(string effectName, Vector2 position, float scale = 1.0f, Color? color = null)
    {
        if (!_effectPools.ContainsKey(effectName))
        {
            GD.PrintErr($"[ParticleEffectManager] Effect '{effectName}' not found!");
            return;
        }

        var effect = GetAvailableEffect(effectName);
        if (effect == null)
        {
            GD.PrintErr($"[ParticleEffectManager] No available effect instance for '{effectName}'");
            return;
        }

        effect.GlobalPosition = position;
        effect.Scale = Vector2.One * scale;
        effect.Visible = true;

        if (color.HasValue)
        {
            effect.Modulate = color.Value;
        }
        else
        {
            effect.Modulate = Colors.White;
        }

        effect.Restart();
        effect.Emitting = true;

        // Auto-disable after lifetime
        var timer = GetTree().CreateTimer(effect.Lifetime);
        timer.Timeout += () =>
        {
            effect.Emitting = false;
            effect.Visible = false;
        };

        EmitSignal(SignalName.EffectSpawned, effectName, position);
    }

    /// <summary>
    /// Get an available effect from the pool
    /// </summary>
    private GpuParticles2D GetAvailableEffect(string effectName)
    {
        var pool = _effectPools[effectName];

        foreach (var effect in pool)
        {
            if (!effect.Emitting)
            {
                return effect;
            }
        }

        // All effects in use, create a new one
        var newEffect = _effectTemplates[effectName]();
        newEffect.Emitting = false;
        newEffect.Visible = false;
        AddChild(newEffect);
        pool.Add(newEffect);

        return newEffect;
    }

    #region Effect Templates

    /// <summary>
    /// Create hit impact effect
    /// </summary>
    private GpuParticles2D CreateHitImpactEffect()
    {
        var particles = new GpuParticles2D();
        particles.Amount = 20;
        particles.Lifetime = 0.5f;
        particles.OneShot = true;
        particles.Explosiveness = 1.0f;
        particles.ProcessMaterial = CreateImpactMaterial();
        return particles;
    }

    private ParticleProcessMaterial CreateImpactMaterial()
    {
        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(0, -1, 0);
        material.Spread = 180;
        material.InitialVelocityMin = 50;
        material.InitialVelocityMax = 150;
        material.Gravity = new Vector3(0, 200, 0);
        material.ScaleMin = 1.0f;
        material.ScaleMax = 3.0f;
        material.Color = new Color(1, 0.8f, 0.3f);
        return material;
    }

    /// <summary>
    /// Create blood splatter effect
    /// </summary>
    private GpuParticles2D CreateBloodSplatterEffect()
    {
        var particles = new GpuParticles2D();
        particles.Amount = 30;
        particles.Lifetime = 0.8f;
        particles.OneShot = true;
        particles.Explosiveness = 0.9f;
        particles.ProcessMaterial = CreateBloodMaterial();
        return particles;
    }

    private ParticleProcessMaterial CreateBloodMaterial()
    {
        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(1, 0, 0);
        material.Spread = 180;
        material.InitialVelocityMin = 80;
        material.InitialVelocityMax = 200;
        material.Gravity = new Vector3(0, 400, 0);
        material.ScaleMin = 2.0f;
        material.ScaleMax = 5.0f;
        material.Color = new Color(0.8f, 0.1f, 0.1f);
        return material;
    }

    /// <summary>
    /// Create death explosion effect
    /// </summary>
    private GpuParticles2D CreateDeathExplosionEffect()
    {
        var particles = new GpuParticles2D();
        particles.Amount = 50;
        particles.Lifetime = 1.0f;
        particles.OneShot = true;
        particles.Explosiveness = 1.0f;
        particles.ProcessMaterial = CreateExplosionMaterial();
        return particles;
    }

    private ParticleProcessMaterial CreateExplosionMaterial()
    {
        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(0, -1, 0);
        material.Spread = 180;
        material.InitialVelocityMin = 100;
        material.InitialVelocityMax = 300;
        material.Gravity = new Vector3(0, 300, 0);
        material.ScaleMin = 2.0f;
        material.ScaleMax = 6.0f;
        material.Color = new Color(1, 0.5f, 0);
        return material;
    }

    /// <summary>
    /// Create heal effect
    /// </summary>
    private GpuParticles2D CreateHealEffect()
    {
        var particles = new GpuParticles2D();
        particles.Amount = 40;
        particles.Lifetime = 1.5f;
        particles.OneShot = true;
        particles.Explosiveness = 0.5f;
        particles.ProcessMaterial = CreateHealMaterial();
        return particles;
    }

    private ParticleProcessMaterial CreateHealMaterial()
    {
        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(0, -1, 0);
        material.Spread = 45;
        material.InitialVelocityMin = 40;
        material.InitialVelocityMax = 80;
        material.Gravity = new Vector3(0, -150, 0); // Float upward
        material.ScaleMin = 2.0f;
        material.ScaleMax = 4.0f;
        material.Color = new Color(0.2f, 1, 0.2f);
        return material;
    }

    /// <summary>
    /// Create level up effect
    /// </summary>
    private GpuParticles2D CreateLevelUpEffect()
    {
        var particles = new GpuParticles2D();
        particles.Amount = 60;
        particles.Lifetime = 2.0f;
        particles.OneShot = true;
        particles.Explosiveness = 0.3f;
        particles.ProcessMaterial = CreateLevelUpMaterial();
        return particles;
    }

    private ParticleProcessMaterial CreateLevelUpMaterial()
    {
        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(0, -1, 0);
        material.Spread = 30;
        material.InitialVelocityMin = 50;
        material.InitialVelocityMax = 100;
        material.Gravity = new Vector3(0, -100, 0);
        material.ScaleMin = 3.0f;
        material.ScaleMax = 6.0f;
        material.Color = new Color(1, 1, 0);
        return material;
    }

    /// <summary>
    /// Create dash trail effect
    /// </summary>
    private GpuParticles2D CreateDashTrailEffect()
    {
        var particles = new GpuParticles2D();
        particles.Amount = 30;
        particles.Lifetime = 0.4f;
        particles.OneShot = true;
        particles.Explosiveness = 0.1f;
        particles.ProcessMaterial = CreateDashMaterial();
        return particles;
    }

    private ParticleProcessMaterial CreateDashMaterial()
    {
        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(0, 0, 0);
        material.Spread = 180;
        material.InitialVelocityMin = 10;
        material.InitialVelocityMax = 30;
        material.Gravity = new Vector3(0, 0, 0);
        material.ScaleMin = 1.0f;
        material.ScaleMax = 3.0f;
        material.Color = new Color(0.5f, 0.5f, 1, 0.6f);
        return material;
    }

    /// <summary>
    /// Create magic cast effect
    /// </summary>
    private GpuParticles2D CreateMagicCastEffect()
    {
        var particles = new GpuParticles2D();
        particles.Amount = 50;
        particles.Lifetime = 1.0f;
        particles.OneShot = true;
        particles.Explosiveness = 0.4f;
        particles.ProcessMaterial = CreateMagicMaterial();
        return particles;
    }

    private ParticleProcessMaterial CreateMagicMaterial()
    {
        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(0, -1, 0);
        material.Spread = 180;
        material.InitialVelocityMin = 60;
        material.InitialVelocityMax = 120;
        material.Gravity = new Vector3(0, -50, 0);
        material.ScaleMin = 2.0f;
        material.ScaleMax = 5.0f;
        material.Color = new Color(0.5f, 0.2f, 1);
        return material;
    }

    /// <summary>
    /// Create teleport effect
    /// </summary>
    private GpuParticles2D CreateTeleportEffect()
    {
        var particles = new GpuParticles2D();
        particles.Amount = 80;
        particles.Lifetime = 0.8f;
        particles.OneShot = true;
        particles.Explosiveness = 0.8f;
        particles.ProcessMaterial = CreateTeleportMaterial();
        return particles;
    }

    private ParticleProcessMaterial CreateTeleportMaterial()
    {
        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(0, -1, 0);
        material.Spread = 180;
        material.InitialVelocityMin = 100;
        material.InitialVelocityMax = 200;
        material.Gravity = new Vector3(0, 0, 0);
        material.ScaleMin = 3.0f;
        material.ScaleMax = 6.0f;
        material.Color = new Color(0, 1, 1);
        return material;
    }

    /// <summary>
    /// Create power up effect
    /// </summary>
    private GpuParticles2D CreatePowerUpEffect()
    {
        var particles = new GpuParticles2D();
        particles.Amount = 40;
        particles.Lifetime = 1.5f;
        particles.OneShot = true;
        particles.Explosiveness = 0.2f;
        particles.ProcessMaterial = CreatePowerUpMaterial();
        return particles;
    }

    private ParticleProcessMaterial CreatePowerUpMaterial()
    {
        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(0, -1, 0);
        material.Spread = 45;
        material.InitialVelocityMin = 30;
        material.InitialVelocityMax = 70;
        material.Gravity = new Vector3(0, -80, 0);
        material.ScaleMin = 2.5f;
        material.ScaleMax = 5.0f;
        material.Color = new Color(1, 0.3f, 0.3f);
        return material;
    }

    /// <summary>
    /// Create smoke effect
    /// </summary>
    private GpuParticles2D CreateSmokeEffect()
    {
        var particles = new GpuParticles2D();
        particles.Amount = 25;
        particles.Lifetime = 2.0f;
        particles.OneShot = true;
        particles.Explosiveness = 0.1f;
        particles.ProcessMaterial = CreateSmokeMaterial();
        return particles;
    }

    private ParticleProcessMaterial CreateSmokeMaterial()
    {
        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(0, -1, 0);
        material.Spread = 30;
        material.InitialVelocityMin = 20;
        material.InitialVelocityMax = 50;
        material.Gravity = new Vector3(0, -30, 0);
        material.ScaleMin = 4.0f;
        material.ScaleMax = 8.0f;
        material.Color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        return material;
    }

    /// <summary>
    /// Create sparkles effect
    /// </summary>
    private GpuParticles2D CreateSparklesEffect()
    {
        var particles = new GpuParticles2D();
        particles.Amount = 35;
        particles.Lifetime = 1.2f;
        particles.OneShot = true;
        particles.Explosiveness = 0.3f;
        particles.ProcessMaterial = CreateSparklesMaterial();
        return particles;
    }

    private ParticleProcessMaterial CreateSparklesMaterial()
    {
        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(0, 0, 0);
        material.Spread = 180;
        material.InitialVelocityMin = 40;
        material.InitialVelocityMax = 80;
        material.Gravity = new Vector3(0, -50, 0);
        material.ScaleMin = 1.5f;
        material.ScaleMax = 3.5f;
        material.Color = new Color(1, 1, 0.5f);
        return material;
    }

    /// <summary>
    /// Create fire effect
    /// </summary>
    private GpuParticles2D CreateFireEffect()
    {
        var particles = new GpuParticles2D();
        particles.Amount = 45;
        particles.Lifetime = 1.0f;
        particles.OneShot = true;
        particles.Explosiveness = 0.5f;
        particles.ProcessMaterial = CreateFireMaterial();
        return particles;
    }

    private ParticleProcessMaterial CreateFireMaterial()
    {
        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(0, -1, 0);
        material.Spread = 45;
        material.InitialVelocityMin = 60;
        material.InitialVelocityMax = 120;
        material.Gravity = new Vector3(0, -100, 0);
        material.ScaleMin = 2.0f;
        material.ScaleMax = 6.0f;
        material.Color = new Color(1, 0.3f, 0);
        return material;
    }

    /// <summary>
    /// Create poison cloud effect
    /// </summary>
    private GpuParticles2D CreatePoisonCloudEffect()
    {
        var particles = new GpuParticles2D();
        particles.Amount = 30;
        particles.Lifetime = 2.5f;
        particles.OneShot = true;
        particles.Explosiveness = 0.1f;
        particles.ProcessMaterial = CreatePoisonMaterial();
        return particles;
    }

    private ParticleProcessMaterial CreatePoisonMaterial()
    {
        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(0, 0, 0);
        material.Spread = 180;
        material.InitialVelocityMin = 15;
        material.InitialVelocityMax = 40;
        material.Gravity = new Vector3(0, -10, 0);
        material.ScaleMin = 5.0f;
        material.ScaleMax = 9.0f;
        material.Color = new Color(0.3f, 0.8f, 0.2f, 0.6f);
        return material;
    }

    /// <summary>
    /// Create lightning effect
    /// </summary>
    private GpuParticles2D CreateLightningEffect()
    {
        var particles = new GpuParticles2D();
        particles.Amount = 60;
        particles.Lifetime = 0.3f;
        particles.OneShot = true;
        particles.Explosiveness = 1.0f;
        particles.ProcessMaterial = CreateLightningMaterial();
        return particles;
    }

    private ParticleProcessMaterial CreateLightningMaterial()
    {
        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(0, -1, 0);
        material.Spread = 180;
        material.InitialVelocityMin = 150;
        material.InitialVelocityMax = 300;
        material.Gravity = new Vector3(0, 0, 0);
        material.ScaleMin = 2.0f;
        material.ScaleMax = 5.0f;
        material.Color = new Color(0.7f, 0.7f, 1);
        return material;
    }

    /// <summary>
    /// Create shield break effect
    /// </summary>
    private GpuParticles2D CreateShieldBreakEffect()
    {
        var particles = new GpuParticles2D();
        particles.Amount = 40;
        particles.Lifetime = 0.8f;
        particles.OneShot = true;
        particles.Explosiveness = 0.9f;
        particles.ProcessMaterial = CreateShieldBreakMaterial();
        return particles;
    }

    private ParticleProcessMaterial CreateShieldBreakMaterial()
    {
        var material = new ParticleProcessMaterial();
        material.Direction = new Vector3(0, 0, 0);
        material.Spread = 180;
        material.InitialVelocityMin = 80;
        material.InitialVelocityMax = 160;
        material.Gravity = new Vector3(0, 200, 0);
        material.ScaleMin = 3.0f;
        material.ScaleMax = 6.0f;
        material.Color = new Color(0.5f, 0.8f, 1);
        return material;
    }

    #endregion

    /// <summary>
    /// Cleanup method
    /// </summary>
    public void ClearAllEffects()
    {
        foreach (var pool in _effectPools.Values)
        {
            foreach (var effect in pool)
            {
                effect.Emitting = false;
                effect.Visible = false;
            }
        }
    }
}
