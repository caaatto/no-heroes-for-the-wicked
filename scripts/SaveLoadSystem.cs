using Godot;
using System;
using System.Text.Json;

public partial class SaveLoadSystem : Node
{
    private const string SAVE_FILE_PATH = "user://savegame.json";

    public override void _Ready()
    {
        GD.Print("SaveLoadSystem initialized");
    }

    public void SaveGame()
    {
        var saveData = new SaveData();

        // Get player data
        var player = GetTree().GetFirstNodeInGroup("player") as PlayerController;
        if (player != null)
        {
            saveData.PlayerData = new PlayerSaveData
            {
                Name = player.PlayerName,
                CurrentHP = player.CurrentLifePoints,
                MaxHP = player.MaxLifePoints,
                AttackValue = player.AttackValue,
                PositionX = player.GlobalPosition.X,
                PositionY = player.GlobalPosition.Y
            };
        }

        // Get inventory data
        var inventory = GetNodeOrNull<InventorySystem>("/root/InventorySystem");
        if (inventory != null)
        {
            saveData.InventoryData = new InventorySaveData
            {
                Items = inventory.GetAllItems(),
                EquippedWeaponId = inventory.GetEquippedWeapon()?.Id
            };
        }

        // Get quest data
        var questSystem = GetNodeOrNull<QuestSystem>("/root/QuestSystem");
        if (questSystem != null)
        {
            saveData.QuestData = new QuestSaveData
            {
                ActiveQuestIds = questSystem.GetActiveQuests().ConvertAll(q => q.Id),
                CompletedQuestIds = questSystem.GetCompletedQuests().ConvertAll(q => q.Id)
            };
        }

        // Save current scene
        saveData.CurrentScene = GetTree().CurrentScene.SceneFilePath;
        saveData.SaveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Serialize to JSON
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string jsonString = JsonSerializer.Serialize(saveData, options);

            // Write to file
            using var file = FileAccess.Open(SAVE_FILE_PATH, FileAccess.ModeFlags.Write);
            file.StoreString(jsonString);

            GD.Print($"Game saved successfully to {SAVE_FILE_PATH}");
            EmitSignal(SignalName.GameSaved);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save game: {e.Message}");
            EmitSignal(SignalName.SaveFailed, e.Message);
        }
    }

    public bool LoadGame()
    {
        if (!FileAccess.FileExists(SAVE_FILE_PATH))
        {
            GD.Print("No save file found");
            EmitSignal(SignalName.LoadFailed, "No save file found");
            return false;
        }

        try
        {
            // Read file
            using var file = FileAccess.Open(SAVE_FILE_PATH, FileAccess.ModeFlags.Read);
            string jsonString = file.GetAsText();

            // Deserialize
            var saveData = JsonSerializer.Deserialize<SaveData>(jsonString);

            if (saveData == null)
            {
                GD.PrintErr("Failed to deserialize save data");
                return false;
            }

            // Load scene first
            if (!string.IsNullOrEmpty(saveData.CurrentScene))
            {
                GetTree().ChangeSceneToFile(saveData.CurrentScene);
            }

            // Wait for scene to load, then restore data
            GetTree().CreateTimer(0.1).Timeout += () => RestoreSaveData(saveData);

            GD.Print($"Game loaded successfully from {SAVE_FILE_PATH}");
            EmitSignal(SignalName.GameLoaded);
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load game: {e.Message}");
            EmitSignal(SignalName.LoadFailed, e.Message);
            return false;
        }
    }

    private void RestoreSaveData(SaveData saveData)
    {
        // Restore player data
        if (saveData.PlayerData != null)
        {
            var player = GetTree().GetFirstNodeInGroup("player") as PlayerController;
            if (player != null)
            {
                player.PlayerName = saveData.PlayerData.Name;
                player.CurrentLifePoints = saveData.PlayerData.CurrentHP;
                player.MaxLifePoints = saveData.PlayerData.MaxHP;
                player.AttackValue = saveData.PlayerData.AttackValue;
                player.GlobalPosition = new Vector2(saveData.PlayerData.PositionX, saveData.PlayerData.PositionY);

                GD.Print("Player data restored");
            }
        }

        // Restore inventory
        if (saveData.InventoryData != null)
        {
            var inventory = GetNodeOrNull<InventorySystem>("/root/InventorySystem");
            if (inventory != null)
            {
                // Clear current inventory
                foreach (var item in inventory.GetAllItems())
                {
                    inventory.RemoveItem(item);
                }

                // Add saved items
                foreach (var item in saveData.InventoryData.Items)
                {
                    inventory.AddItem(item);
                }

                // Equip weapon
                if (!string.IsNullOrEmpty(saveData.InventoryData.EquippedWeaponId))
                {
                    var weapon = inventory.GetItem(saveData.InventoryData.EquippedWeaponId);
                    if (weapon != null)
                    {
                        inventory.EquipWeapon(weapon);
                    }
                }

                GD.Print("Inventory data restored");
            }
        }

        // Restore quests
        if (saveData.QuestData != null)
        {
            var questSystem = GetNodeOrNull<QuestSystem>("/root/QuestSystem");
            if (questSystem != null)
            {
                // Start active quests
                foreach (var questId in saveData.QuestData.ActiveQuestIds)
                {
                    questSystem.StartQuest(questId);
                }

                // Complete completed quests (simplified - in real implementation, preserve progress)
                // This would need more detailed objective tracking

                GD.Print("Quest data restored");
            }
        }
    }

    public bool SaveFileExists()
    {
        return FileAccess.FileExists(SAVE_FILE_PATH);
    }

    public void DeleteSaveFile()
    {
        if (FileAccess.FileExists(SAVE_FILE_PATH))
        {
            DirAccess.RemoveAbsolute(SAVE_FILE_PATH);
            GD.Print("Save file deleted");
        }
    }

    // Signals
    [Signal]
    public delegate void GameSavedEventHandler();

    [Signal]
    public delegate void GameLoadedEventHandler();

    [Signal]
    public delegate void SaveFailedEventHandler(string error);

    [Signal]
    public delegate void LoadFailedEventHandler(string error);
}

// Save data structures
public class SaveData
{
    public string SaveTimestamp { get; set; }
    public string CurrentScene { get; set; }
    public PlayerSaveData PlayerData { get; set; }
    public InventorySaveData InventoryData { get; set; }
    public QuestSaveData QuestData { get; set; }
}

public class PlayerSaveData
{
    public string Name { get; set; }
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    public int AttackValue { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
}

public class InventorySaveData
{
    public System.Collections.Generic.List<Item> Items { get; set; }
    public string EquippedWeaponId { get; set; }
}

public class QuestSaveData
{
    public System.Collections.Generic.List<string> ActiveQuestIds { get; set; }
    public System.Collections.Generic.List<string> CompletedQuestIds { get; set; }
}
