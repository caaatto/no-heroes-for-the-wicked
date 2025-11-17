using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class QuestSystem : Node
{
    private List<Quest> _activeQuests = new List<Quest>();
    private List<Quest> _completedQuests = new List<Quest>();
    private Dictionary<string, Quest> _questDatabase = new Dictionary<string, Quest>();

    public override void _Ready()
    {
        InitializeQuests();
        GD.Print("QuestSystem initialized");
    }

    private void InitializeQuests()
    {
        // Define the 3 MVP quests
        var quest1 = new Quest
        {
            Id = "defeat_troll",
            Title = "Der böse Troll",
            Description = "Ein gefährlicher Troll terrorisiert die Dorfbewohner. Besiege ihn!",
            Type = QuestType.Combat,
            Objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    Description = "Besiege den Troll",
                    TargetType = "enemy",
                    TargetId = "troll",
                    RequiredCount = 1,
                    CurrentCount = 0
                }
            },
            Rewards = new QuestRewards
            {
                Experience = 100,
                Gold = 50,
                Items = new List<string> { "rostiges_schwert" }
            }
        };

        var quest2 = new Quest
        {
            Id = "collect_weapons",
            Title = "Waffensammlung",
            Description = "Sammle 5 verschiedene Waffen aus der Taverne.",
            Type = QuestType.Collection,
            Objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    Description = "Sammle 5 Waffen",
                    TargetType = "item",
                    TargetId = "any_weapon",
                    RequiredCount = 5,
                    CurrentCount = 0
                }
            },
            Rewards = new QuestRewards
            {
                Experience = 50,
                Gold = 25
            }
        };

        var quest3 = new Quest
        {
            Id = "find_bartender",
            Title = "Der vermisste Wirt",
            Description = "Finde den Tavernenwirt im Hub-Level und sprich mit ihm.",
            Type = QuestType.Exploration,
            Objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    Description = "Finde den Wirt",
                    TargetType = "npc",
                    TargetId = "bartender",
                    RequiredCount = 1,
                    CurrentCount = 0
                },
                new QuestObjective
                {
                    Description = "Sprich mit dem Wirt",
                    TargetType = "dialogue",
                    TargetId = "bartender_talk",
                    RequiredCount = 1,
                    CurrentCount = 0
                }
            },
            Rewards = new QuestRewards
            {
                Experience = 75,
                Gold = 30,
                Items = new List<string> { "krug" }
            }
        };

        // Add to database
        _questDatabase[quest1.Id] = quest1;
        _questDatabase[quest2.Id] = quest2;
        _questDatabase[quest3.Id] = quest3;

        GD.Print($"Loaded {_questDatabase.Count} quests");
    }

    public void StartQuest(string questId)
    {
        if (!_questDatabase.ContainsKey(questId))
        {
            GD.PrintErr($"Quest '{questId}' not found");
            return;
        }

        var quest = _questDatabase[questId];

        if (_activeQuests.Contains(quest))
        {
            GD.Print($"Quest '{quest.Title}' is already active");
            return;
        }

        if (_completedQuests.Contains(quest))
        {
            GD.Print($"Quest '{quest.Title}' is already completed");
            return;
        }

        quest.Status = QuestStatus.Active;
        _activeQuests.Add(quest);
        GD.Print($"Started quest: {quest.Title}");
        EmitSignal(SignalName.QuestStarted, quest);
    }

    public void UpdateQuestProgress(string targetType, string targetId, int amount = 1)
    {
        foreach (var quest in _activeQuests.ToList())
        {
            bool questUpdated = false;

            foreach (var objective in quest.Objectives)
            {
                if (objective.IsCompleted)
                    continue;

                // Check if this objective matches the event
                bool matches = objective.TargetType == targetType &&
                              (objective.TargetId == targetId || objective.TargetId == "any_weapon" && targetType == "item");

                if (matches)
                {
                    objective.CurrentCount += amount;
                    if (objective.CurrentCount >= objective.RequiredCount)
                    {
                        objective.CurrentCount = objective.RequiredCount;
                        objective.IsCompleted = true;
                        GD.Print($"Objective completed: {objective.Description}");
                    }

                    questUpdated = true;
                    EmitSignal(SignalName.QuestProgressUpdated, quest, objective);
                }
            }

            if (questUpdated)
            {
                // Check if all objectives are complete
                if (quest.Objectives.All(obj => obj.IsCompleted))
                {
                    CompleteQuest(quest);
                }
            }
        }
    }

    private void CompleteQuest(Quest quest)
    {
        quest.Status = QuestStatus.Completed;
        _activeQuests.Remove(quest);
        _completedQuests.Add(quest);

        GD.Print($"Quest completed: {quest.Title}");
        EmitSignal(SignalName.QuestCompleted, quest);

        // Award rewards
        GiveRewards(quest.Rewards);
    }

    private void GiveRewards(QuestRewards rewards)
    {
        if (rewards == null)
            return;

        GD.Print($"Rewards: {rewards.Experience} XP, {rewards.Gold} Gold");

        // Emit signal for rewards (can be handled by player controller or game manager)
        EmitSignal(SignalName.RewardsGiven, rewards);

        // Items will be added to inventory by the handler
        if (rewards.Items != null && rewards.Items.Count > 0)
        {
            var inventory = GetNode<InventorySystem>("/root/InventorySystem");
            foreach (var itemId in rewards.Items)
            {
                var item = inventory.CreateWeaponFromId(itemId);
                if (item != null)
                {
                    inventory.AddItem(item);
                }
            }
        }
    }

    public List<Quest> GetActiveQuests()
    {
        return new List<Quest>(_activeQuests);
    }

    public List<Quest> GetCompletedQuests()
    {
        return new List<Quest>(_completedQuests);
    }

    public Quest GetQuest(string questId)
    {
        if (_questDatabase.ContainsKey(questId))
            return _questDatabase[questId];
        return null;
    }

    // Signals
    [Signal]
    public delegate void QuestStartedEventHandler(Quest quest);

    [Signal]
    public delegate void QuestProgressUpdatedEventHandler(Quest quest, QuestObjective objective);

    [Signal]
    public delegate void QuestCompletedEventHandler(Quest quest);

    [Signal]
    public delegate void RewardsGivenEventHandler(QuestRewards rewards);
}

// Quest classes
public class Quest
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public QuestType Type { get; set; }
    public QuestStatus Status { get; set; } = QuestStatus.NotStarted;
    public List<QuestObjective> Objectives { get; set; }
    public QuestRewards Rewards { get; set; }
}

public class QuestObjective
{
    public string Description { get; set; }
    public string TargetType { get; set; } // enemy, item, npc, dialogue, etc.
    public string TargetId { get; set; }
    public int RequiredCount { get; set; }
    public int CurrentCount { get; set; }
    public bool IsCompleted { get; set; }
}

public class QuestRewards
{
    public int Experience { get; set; }
    public int Gold { get; set; }
    public List<string> Items { get; set; }
}

public enum QuestType
{
    Combat,
    Collection,
    Exploration,
    Dialogue,
    Mixed
}

public enum QuestStatus
{
    NotStarted,
    Active,
    Completed,
    Failed
}
