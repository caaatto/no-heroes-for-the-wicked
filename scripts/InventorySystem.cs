using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class InventorySystem : Node
{
    // Inventory capacity
    [Export] public int MaxSlots { get; set; } = 20;

    // Current inventory
    private List<Item> _items = new List<Item>();
    private Item _equippedWeapon = null;

    // Weapon database
    private Dictionary<string, WeaponData> _weaponDatabase = new Dictionary<string, WeaponData>();

    public override void _Ready()
    {
        LoadWeaponDatabase();
        GD.Print("InventorySystem initialized");
    }

    private void LoadWeaponDatabase()
    {
        // Load weapons.json
        string path = "res://data/weapons.json";

        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr($"Weapon database not found at {path}");
            return;
        }

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string jsonContent = file.GetAsText();

        try
        {
            var weaponList = JsonSerializer.Deserialize<WeaponDatabase>(jsonContent);
            if (weaponList != null && weaponList.weapons != null)
            {
                foreach (var weapon in weaponList.weapons)
                {
                    _weaponDatabase[weapon.id] = weapon;
                }
                GD.Print($"Loaded {_weaponDatabase.Count} weapons from database");
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to parse weapon database: {e.Message}");
        }
    }

    public bool AddItem(Item item)
    {
        if (_items.Count >= MaxSlots)
        {
            GD.Print("Inventory is full!");
            EmitSignal(SignalName.InventoryFull);
            return false;
        }

        _items.Add(item);
        GD.Print($"Added {item.Name} to inventory");
        EmitSignal(SignalName.ItemAdded, item);
        return true;
    }

    public bool RemoveItem(Item item)
    {
        if (_items.Remove(item))
        {
            GD.Print($"Removed {item.Name} from inventory");
            EmitSignal(SignalName.ItemRemoved, item);
            return true;
        }
        return false;
    }

    public bool HasItem(string itemId)
    {
        return _items.Exists(item => item.Id == itemId);
    }

    public Item GetItem(string itemId)
    {
        return _items.Find(item => item.Id == itemId);
    }

    public List<Item> GetAllItems()
    {
        return new List<Item>(_items);
    }

    public void EquipWeapon(Item weapon)
    {
        if (weapon == null || weapon.Type != ItemType.Weapon)
        {
            GD.PrintErr("Cannot equip: not a weapon");
            return;
        }

        _equippedWeapon = weapon;
        GD.Print($"Equipped weapon: {weapon.Name}");
        EmitSignal(SignalName.WeaponEquipped, weapon);
    }

    public void UnequipWeapon()
    {
        if (_equippedWeapon != null)
        {
            GD.Print($"Unequipped weapon: {_equippedWeapon.Name}");
            _equippedWeapon = null;
            EmitSignal(SignalName.WeaponUnequipped);
        }
    }

    public bool HasEquippedWeapon()
    {
        return _equippedWeapon != null;
    }

    public Item GetEquippedWeapon()
    {
        return _equippedWeapon;
    }

    public int GetEquippedWeaponDamage()
    {
        if (_equippedWeapon == null)
            return 0;

        // Parse damage dice (e.g., "1d6")
        return RollDice(_equippedWeapon.DamageDice);
    }

    private int RollDice(string diceNotation)
    {
        // Parse dice notation like "1d6", "2d4", etc.
        try
        {
            var parts = diceNotation.ToLower().Split('d');
            if (parts.Length != 2)
                return 0;

            int numDice = int.Parse(parts[0]);
            int diceSides = int.Parse(parts[1]);

            int total = 0;
            for (int i = 0; i < numDice; i++)
            {
                total += GD.RandRange(1, diceSides);
            }

            return total;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to parse dice notation '{diceNotation}': {e.Message}");
            return 0;
        }
    }

    public WeaponData GetWeaponData(string weaponId)
    {
        if (_weaponDatabase.ContainsKey(weaponId))
            return _weaponDatabase[weaponId];
        return null;
    }

    public Item CreateWeaponFromId(string weaponId)
    {
        var weaponData = GetWeaponData(weaponId);
        if (weaponData == null)
        {
            GD.PrintErr($"Weapon '{weaponId}' not found in database");
            return null;
        }

        return new Item
        {
            Id = weaponData.id,
            Name = weaponData.name,
            Description = weaponData.description,
            Type = ItemType.Weapon,
            DamageDice = weaponData.damage_dice,
            DamageStat = weaponData.damage_stat,
            Rarity = weaponData.rarity
        };
    }

    // Signals
    [Signal]
    public delegate void ItemAddedEventHandler(Item item);

    [Signal]
    public delegate void ItemRemovedEventHandler(Item item);

    [Signal]
    public delegate void InventoryFullEventHandler();

    [Signal]
    public delegate void WeaponEquippedEventHandler(Item weapon);

    [Signal]
    public delegate void WeaponUnequippedEventHandler();
}

// Item class
public partial class Item : Resource
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ItemType Type { get; set; }
    public string DamageDice { get; set; }
    public string DamageStat { get; set; }
    public string Rarity { get; set; }
    public int Quantity { get; set; } = 1;
}

public enum ItemType
{
    Weapon,
    Consumable,
    QuestItem,
    Misc
}

// Weapon data structure for JSON deserialization
public class WeaponData
{
    public string id { get; set; }
    public string name { get; set; }
    public string damage_dice { get; set; }
    public string damage_stat { get; set; }
    public string description { get; set; }
    public string rarity { get; set; }
    public string element { get; set; }
    public string damage_type { get; set; }
    public string special { get; set; }
    public int defense_bonus { get; set; }
}

public class WeaponDatabase
{
    public List<WeaponData> weapons { get; set; }
}
