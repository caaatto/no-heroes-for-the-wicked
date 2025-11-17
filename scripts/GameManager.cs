using Godot;
using System;

public partial class GameManager : Node
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Game State
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    [Export] public GameState CurrentState { get; private set; } = GameState.MainMenu;

    // Player Stats
    [Export] public int PlayerExperience { get; set; } = 0;
    [Export] public int PlayerLevel { get; set; } = 1;
    [Export] public int PlayerGold { get; set; } = 0;

    // Systems
    private InventorySystem _inventory;
    private QuestSystem _questSystem;
    private SaveLoadSystem _saveLoadSystem;
    private PlayerController _player;

    // UI References
    private CanvasLayer _hudLayer;
    private Control _pauseMenu;
    private Control _inventoryUI;
    private Control _gameOverScreen;

    public override void _Ready()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }
        Instance = this;

        // Get system references
        _inventory = GetNodeOrNull<InventorySystem>("/root/InventorySystem");
        _questSystem = GetNodeOrNull<QuestSystem>("/root/QuestSystem");
        _saveLoadSystem = GetNodeOrNull<SaveLoadSystem>("/root/SaveLoadSystem");

        // Connect to key events
        ConnectSignals();

        GD.Print("GameManager initialized");
    }

    public override void _Process(double delta)
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Pause/Unpause
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            if (CurrentState == GameState.Playing)
                PauseGame();
            else if (CurrentState == GameState.Paused)
                ResumeGame();
        }

        // Quick Save
        if (Input.IsActionJustPressed("quick_save"))
        {
            if (CurrentState == GameState.Playing && _saveLoadSystem != null)
            {
                _saveLoadSystem.SaveGame();
                ShowNotification("Game Saved!");
            }
        }

        // Quick Load
        if (Input.IsActionJustPressed("quick_load"))
        {
            if (_saveLoadSystem != null)
            {
                _saveLoadSystem.LoadGame();
                ShowNotification("Game Loaded!");
            }
        }

        // Toggle Inventory
        if (Input.IsActionJustPressed("inventory"))
        {
            ToggleInventory();
        }
    }

    private void ConnectSignals()
    {
        // Connect to quest system
        if (_questSystem != null)
        {
            _questSystem.QuestCompleted += OnQuestCompleted;
            _questSystem.RewardsGiven += OnRewardsGiven;
        }
    }

    public void SetPlayer(PlayerController player)
    {
        _player = player;
        if (_player != null)
        {
            _player.PlayerDied += OnPlayerDied;
            GD.Print("Player connected to GameManager");
        }
    }

    public PlayerController GetPlayer()
    {
        if (_player == null)
        {
            var playerNode = GetTree().GetFirstNodeInGroup("player");
            if (playerNode is PlayerController player)
            {
                SetPlayer(player);
            }
        }
        return _player;
    }

    public void ChangeState(GameState newState)
    {
        var oldState = CurrentState;
        CurrentState = newState;
        GD.Print($"Game State: {oldState} -> {newState}");

        switch (newState)
        {
            case GameState.MainMenu:
                GetTree().Paused = false;
                break;
            case GameState.Playing:
                GetTree().Paused = false;
                break;
            case GameState.Paused:
                GetTree().Paused = true;
                break;
            case GameState.GameOver:
                GetTree().Paused = true;
                ShowGameOver();
                break;
            case GameState.Victory:
                GetTree().Paused = true;
                ShowVictory();
                break;
        }

        EmitSignal(SignalName.GameStateChanged, (int)newState);
    }

    public void PauseGame()
    {
        ChangeState(GameState.Paused);
        ShowPauseMenu();
    }

    public void ResumeGame()
    {
        ChangeState(GameState.Playing);
        HidePauseMenu();
    }

    public void StartNewGame()
    {
        // Reset player stats
        PlayerExperience = 0;
        PlayerLevel = 1;
        PlayerGold = 0;

        // Load hub scene
        GetTree().ChangeSceneToFile("res://scenes/hub.tscn");
        GetTree().CreateTimer(0.1).Timeout += () =>
        {
            ChangeState(GameState.Playing);

            // Start first quest
            if (_questSystem != null)
            {
                _questSystem.StartQuest("find_bartender");
            }
        };
    }

    public void LoadGame()
    {
        if (_saveLoadSystem != null && _saveLoadSystem.SaveFileExists())
        {
            _saveLoadSystem.LoadGame();
            ChangeState(GameState.Playing);
        }
        else
        {
            ShowNotification("No save file found!");
        }
    }

    public void SaveGame()
    {
        if (_saveLoadSystem != null)
        {
            _saveLoadSystem.SaveGame();
            ShowNotification("Game Saved!");
        }
    }

    public void QuitToMenu()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://scenes/main.tscn");
        ChangeState(GameState.MainMenu);
    }

    public void QuitGame()
    {
        GetTree().Quit();
    }

    // Reward and progression
    public void AddExperience(int amount)
    {
        PlayerExperience += amount;
        GD.Print($"Gained {amount} XP. Total: {PlayerExperience}");

        // Check for level up (simple: every 100 XP)
        int requiredXP = PlayerLevel * 100;
        if (PlayerExperience >= requiredXP)
        {
            LevelUp();
        }

        EmitSignal(SignalName.ExperienceGained, amount, PlayerExperience);
    }

    public void AddGold(int amount)
    {
        PlayerGold += amount;
        GD.Print($"Gained {amount} Gold. Total: {PlayerGold}");
        EmitSignal(SignalName.GoldGained, amount, PlayerGold);
    }

    private void LevelUp()
    {
        PlayerLevel++;
        PlayerExperience = 0;
        GD.Print($"Level Up! Now Level {PlayerLevel}");

        // Boost player stats
        if (_player != null)
        {
            _player.MaxLifePoints += 10;
            _player.CurrentLifePoints = _player.MaxLifePoints;
            _player.AttackValue += 2;
        }

        ShowNotification($"LEVEL UP! Now Level {PlayerLevel}!");
        EmitSignal(SignalName.PlayerLeveledUp, PlayerLevel);
    }

    // Event handlers
    private void OnPlayerDied()
    {
        GD.Print("Player died - Game Over");
        GetTree().CreateTimer(2.0).Timeout += () => ChangeState(GameState.GameOver);
    }

    private void OnQuestCompleted(Quest quest)
    {
        ShowNotification($"Quest Completed: {quest.Title}!");
    }

    private void OnRewardsGiven(QuestRewards rewards)
    {
        if (rewards.Experience > 0)
            AddExperience(rewards.Experience);
        if (rewards.Gold > 0)
            AddGold(rewards.Gold);
    }

    // UI Management
    private void ShowPauseMenu()
    {
        // Will be implemented with UI system
        GD.Print("Pause Menu Shown");
    }

    private void HidePauseMenu()
    {
        GD.Print("Pause Menu Hidden");
    }

    private void ToggleInventory()
    {
        // Will be implemented with UI system
        GD.Print("Inventory Toggled");
    }

    private void ShowGameOver()
    {
        GD.Print("=== GAME OVER ===");
        ShowNotification("GAME OVER - Press R to Restart");
    }

    private void ShowVictory()
    {
        GD.Print("=== VICTORY ===");
        ShowNotification("VICTORY! You defeated the Troll!");
    }

    public void ShowNotification(string message)
    {
        GD.Print($"[NOTIFICATION] {message}");
        EmitSignal(SignalName.NotificationShown, message);
    }

    // Signals
    [Signal]
    public delegate void GameStateChangedEventHandler(int newState);

    [Signal]
    public delegate void ExperienceGainedEventHandler(int amount, int total);

    [Signal]
    public delegate void GoldGainedEventHandler(int amount, int total);

    [Signal]
    public delegate void PlayerLeveledUpEventHandler(int newLevel);

    [Signal]
    public delegate void NotificationShownEventHandler(string message);
}
