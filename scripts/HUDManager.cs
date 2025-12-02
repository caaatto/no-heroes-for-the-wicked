using Godot;
using System;

public partial class HUDManager : CanvasLayer
{
    // Health Bar
    private ProgressBar _healthBar;
    private Label _healthLabel;

    // Stats Display
    private Label _levelLabel;
    private Label _experienceLabel;
    private Label _goldLabel;

    // Weapon Display
    private Label _weaponNameLabel;
    private Label _weaponDamageLabel;

    // Quest Display
    private VBoxContainer _questContainer;
    private Label _activeQuestsLabel;

    // Notification System
    private Label _notificationLabel;
    private Timer _notificationTimer;

    // References
    private PlayerController _player;
    private GameManager _gameManager;
    private InventorySystem _inventory;
    private QuestSystem _questSystem;

    public override void _Ready()
    {
        // Get UI node references
        _healthBar = GetNodeOrNull<ProgressBar>("HUD/TopLeft/HealthBar");
        _healthLabel = GetNodeOrNull<Label>("HUD/TopLeft/HealthLabel");
        _levelLabel = GetNodeOrNull<Label>("HUD/TopLeft/LevelLabel");
        _experienceLabel = GetNodeOrNull<Label>("HUD/TopLeft/ExperienceLabel");
        _goldLabel = GetNodeOrNull<Label>("HUD/TopLeft/GoldLabel");
        _weaponNameLabel = GetNodeOrNull<Label>("HUD/TopRight/WeaponName");
        _weaponDamageLabel = GetNodeOrNull<Label>("HUD/TopRight/WeaponDamage");
        _questContainer = GetNodeOrNull<VBoxContainer>("HUD/BottomLeft/QuestContainer");
        _activeQuestsLabel = GetNodeOrNull<Label>("HUD/BottomLeft/ActiveQuestsLabel");
        _notificationLabel = GetNodeOrNull<Label>("HUD/Center/NotificationLabel");

        // Setup notification timer
        _notificationTimer = new Timer();
        _notificationTimer.WaitTime = 3.0;
        _notificationTimer.OneShot = true;
        _notificationTimer.Timeout += HideNotification;
        AddChild(_notificationTimer);

        if (_notificationLabel != null)
            _notificationLabel.Visible = false;

        // Get system references
        _gameManager = GameManager.Instance;
        _inventory = GetNodeOrNull<InventorySystem>("/root/InventorySystem");
        _questSystem = GetNodeOrNull<QuestSystem>("/root/QuestSystem");

        // Wait for scene to be ready, then find player
        CallDeferred(MethodName.SetupPlayer);
        CallDeferred(MethodName.ConnectSignals);

        GD.Print("HUDManager initialized");
    }

    private void SetupPlayer()
    {
        var playerNode = GetTree().GetFirstNodeInGroup("player");
        if (playerNode is PlayerController player)
        {
            _player = player;
            UpdateHealth(_player.CurrentLifePoints, _player.MaxLifePoints);
        }
    }

    private void ConnectSignals()
    {
        // Connect to player signals
        if (_player != null)
        {
            _player.HealthChanged += UpdateHealth;
        }

        // Connect to game manager signals
        if (_gameManager != null)
        {
            _gameManager.ExperienceGained += OnExperienceGained;
            _gameManager.GoldGained += OnGoldGained;
            _gameManager.PlayerLeveledUp += OnPlayerLevelUp;
            _gameManager.NotificationShown += ShowNotification;
        }

        // Connect to inventory signals
        if (_inventory != null)
        {
            _inventory.WeaponEquipped += OnWeaponEquipped;
            _inventory.WeaponUnequipped += OnWeaponUnequipped;
        }

        // Connect to quest system signals
        if (_questSystem != null)
        {
            _questSystem.QuestStarted += OnQuestStarted;
            _questSystem.QuestCompleted += OnQuestCompleted;
            _questSystem.QuestProgressUpdated += OnQuestProgressUpdated;
        }

        UpdateAllUI();
    }

    private void UpdateHealth(int current, int max)
    {
        if (_healthBar != null)
        {
            _healthBar.MaxValue = max;
            _healthBar.Value = current;
        }

        if (_healthLabel != null)
        {
            _healthLabel.Text = $"HP: {current}/{max}";
        }

        // Change color based on health percentage
        if (_healthBar != null)
        {
            float percentage = (float)current / max;
            if (percentage > 0.5f)
                _healthBar.Modulate = Colors.Green;
            else if (percentage > 0.25f)
                _healthBar.Modulate = Colors.Yellow;
            else
                _healthBar.Modulate = Colors.Red;
        }
    }

    private void OnExperienceGained(int amount, int total)
    {
        UpdateExperience(total);
    }

    private void OnGoldGained(int amount, int total)
    {
        UpdateGold(total);
    }

    private void OnPlayerLevelUp(int newLevel)
    {
        UpdateLevel(newLevel);
    }

    private void UpdateLevel(int level)
    {
        if (_levelLabel != null)
        {
            _levelLabel.Text = $"Level: {level}";
        }
    }

    private void UpdateExperience(int experience)
    {
        if (_experienceLabel != null)
        {
            int requiredXP = _gameManager != null ? _gameManager.PlayerLevel * 100 : 100;
            _experienceLabel.Text = $"XP: {experience}/{requiredXP}";
        }
    }

    private void UpdateGold(int gold)
    {
        if (_goldLabel != null)
        {
            _goldLabel.Text = $"Gold: {gold}";
        }
    }

    private void OnWeaponEquipped(Item weapon)
    {
        if (_weaponNameLabel != null)
        {
            _weaponNameLabel.Text = $"Weapon: {weapon.Name}";
        }

        if (_weaponDamageLabel != null)
        {
            _weaponDamageLabel.Text = $"Damage: {weapon.DamageDice} ({weapon.DamageStat})";
        }
    }

    private void OnWeaponUnequipped()
    {
        if (_weaponNameLabel != null)
        {
            _weaponNameLabel.Text = "Weapon: None";
        }

        if (_weaponDamageLabel != null)
        {
            _weaponDamageLabel.Text = "Damage: -";
        }
    }

    private void OnQuestStarted(string questId)
    {
        UpdateQuestDisplay();
        var quest = _questSystem?.GetQuest(questId);
        if (quest != null)
        {
            ShowNotification($"New Quest: {quest.Title}");
        }
    }

    private void OnQuestCompleted(string questId)
    {
        UpdateQuestDisplay();
    }

    private void OnQuestProgressUpdated(string questId, string objectiveDescription)
    {
        UpdateQuestDisplay();
    }

    private void UpdateQuestDisplay()
    {
        if (_activeQuestsLabel != null && _questSystem != null)
        {
            var activeQuests = _questSystem.GetActiveQuests();
            var completedQuests = _questSystem.GetCompletedQuests();

            string questText = $"Quests: {activeQuests.Count} active, {completedQuests.Count} completed\n";

            foreach (var quest in activeQuests)
            {
                questText += $"• {quest.Title}\n";
                foreach (var objective in quest.Objectives)
                {
                    string status = objective.IsCompleted ? "[✓]" : $"[{objective.CurrentCount}/{objective.RequiredCount}]";
                    questText += $"  {status} {objective.Description}\n";
                }
            }

            _activeQuestsLabel.Text = questText;
        }
    }

    public void ShowNotification(string message)
    {
        if (_notificationLabel != null)
        {
            _notificationLabel.Text = message;
            _notificationLabel.Visible = true;
            _notificationTimer.Start();
        }
    }

    private void HideNotification()
    {
        if (_notificationLabel != null)
        {
            _notificationLabel.Visible = false;
        }
    }

    private void UpdateAllUI()
    {
        if (_player != null)
        {
            UpdateHealth(_player.CurrentLifePoints, _player.MaxLifePoints);
        }

        if (_gameManager != null)
        {
            UpdateLevel(_gameManager.PlayerLevel);
            UpdateExperience(_gameManager.PlayerExperience);
            UpdateGold(_gameManager.PlayerGold);
        }

        if (_inventory != null)
        {
            var weapon = _inventory.GetEquippedWeapon();
            if (weapon != null)
                OnWeaponEquipped(weapon);
            else
                OnWeaponUnequipped();
        }

        UpdateQuestDisplay();
    }

    public override void _Process(double delta)
    {
        // Update HUD every frame (for debug purposes - can be optimized later)
        if (_player != null && _healthBar != null)
        {
            if (_healthBar.Value != _player.CurrentLifePoints)
            {
                UpdateHealth(_player.CurrentLifePoints, _player.MaxLifePoints);
            }
        }
    }
}
