using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NoHeroesForTheWicked
{
    #region Configuration
    /// <summary>
    /// Game configuration constants
    /// </summary>
    public static class GameConfig
    {
        public const int DefaultPlayerHealth = 100;
        public const int DefaultPlayerAttack = 10;
        public const int DefaultEnemyHealth = 50;
        public const int DefaultEnemyAttack = 8;
        public const string WeaponsDataFile = "Waffen.txt";
    }
    #endregion

    #region Weapon System
    /// <summary>
    /// Represents a weapon with dice-based damage calculation
    /// </summary>
    public class Weapon
    {
        public string Name { get; }
        public string DiceNotation { get; }
        public string Attribute { get; }
        private Random _random = new Random();

        public Weapon(string name, string diceNotation, string attribute)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DiceNotation = diceNotation ?? throw new ArgumentNullException(nameof(diceNotation));
            Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
        }

        /// <summary>
        /// Calculate damage based on dice notation (e.g., "1d6", "2d4")
        /// </summary>
        public int RollDamage()
        {
            try
            {
                var parts = DiceNotation.ToLower().Split('d');
                if (parts.Length != 2)
                    return 5; // Default damage if parsing fails

                int numDice = int.Parse(parts[0]);
                int diceSize = int.Parse(parts[1]);

                int totalDamage = 0;
                for (int i = 0; i < numDice; i++)
                {
                    totalDamage += _random.Next(1, diceSize + 1);
                }

                return totalDamage;
            }
            catch
            {
                return 5; // Default damage if parsing fails
            }
        }

        public override string ToString()
        {
            return $"{Name} ({DiceNotation} {Attribute})";
        }
    }

    /// <summary>
    /// Loads weapons from the data file
    /// </summary>
    public static class WeaponLoader
    {
        public static List<Weapon> LoadWeapons(string filePath)
        {
            var weapons = new List<Weapon>();

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Warning: Weapons file '{filePath}' not found. Using default weapons.");
                    return GetDefaultWeapons();
                }

                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        string name = parts[0].Replace('_', ' ');
                        string dice = parts[1];
                        string attribute = parts[2];
                        weapons.Add(new Weapon(name, dice, attribute));
                    }
                }

                return weapons.Count > 0 ? weapons : GetDefaultWeapons();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading weapons: {ex.Message}");
                return GetDefaultWeapons();
            }
        }

        private static List<Weapon> GetDefaultWeapons()
        {
            return new List<Weapon>
            {
                new Weapon("Fist", "1d4", "Str"),
                new Weapon("Rusty Sword", "1d6", "Str"),
                new Weapon("Club", "1d6", "Str")
            };
        }
    }
    #endregion

    #region Character System
    /// <summary>
    /// Common interface for all combat characters
    /// </summary>
    public interface ICharacter
    {
        string Name { get; }
        int MaxHealth { get; }
        int CurrentHealth { get; }
        int BaseAttack { get; }
        bool IsAlive { get; }
        Weapon EquippedWeapon { get; }

        void TakeDamage(int damage);
        int CalculateAttackDamage();
    }

    /// <summary>
    /// Base class for all characters (Player and Enemy)
    /// </summary>
    public abstract class Character : ICharacter
    {
        public string Name { get; protected set; }
        public int MaxHealth { get; protected set; }
        public int BaseAttack { get; protected set; }
        public Weapon EquippedWeapon { get; protected set; }

        private int _currentHealth;
        public int CurrentHealth
        {
            get => _currentHealth;
            protected set => _currentHealth = Math.Max(0, value);
        }

        public bool IsAlive => CurrentHealth > 0;

        protected Character(string name, int maxHealth, int baseAttack)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));
            if (maxHealth <= 0)
                throw new ArgumentException("Max health must be positive", nameof(maxHealth));
            if (baseAttack < 0)
                throw new ArgumentException("Base attack cannot be negative", nameof(baseAttack));

            Name = name;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            BaseAttack = baseAttack;
        }

        public virtual void TakeDamage(int damage)
        {
            if (damage < 0)
                throw new ArgumentException("Damage cannot be negative", nameof(damage));

            CurrentHealth -= damage;
        }

        public virtual int CalculateAttackDamage()
        {
            if (EquippedWeapon != null)
            {
                return BaseAttack + EquippedWeapon.RollDamage();
            }
            return BaseAttack;
        }

        public void EquipWeapon(Weapon weapon)
        {
            EquippedWeapon = weapon;
        }
    }

    /// <summary>
    /// Player character class
    /// </summary>
    public class Player : Character
    {
        public Player(string name, int maxHealth = GameConfig.DefaultPlayerHealth, int baseAttack = GameConfig.DefaultPlayerAttack)
            : base(name, maxHealth, baseAttack)
        {
        }

        public Player(string name, Weapon weapon, int maxHealth = GameConfig.DefaultPlayerHealth, int baseAttack = GameConfig.DefaultPlayerAttack)
            : base(name, maxHealth, baseAttack)
        {
            EquippedWeapon = weapon;
        }
    }

    /// <summary>
    /// Enemy character class
    /// </summary>
    public class Enemy : Character
    {
        public string Type { get; }

        public Enemy(string name, string type, int maxHealth = GameConfig.DefaultEnemyHealth, int baseAttack = GameConfig.DefaultEnemyAttack)
            : base(name, maxHealth, baseAttack)
        {
            Type = type ?? "Monster";
        }

        public Enemy(string name, string type, Weapon weapon, int maxHealth = GameConfig.DefaultEnemyHealth, int baseAttack = GameConfig.DefaultEnemyAttack)
            : base(name, maxHealth, baseAttack)
        {
            Type = type ?? "Monster";
            EquippedWeapon = weapon;
        }
    }
    #endregion

    #region Combat System
    /// <summary>
    /// Handles combat logic separated from presentation
    /// </summary>
    public class CombatSystem
    {
        public delegate void CombatEventHandler(string message);
        public event CombatEventHandler OnCombatMessage;

        public CombatResult ProcessAttack(ICharacter attacker, ICharacter defender)
        {
            if (attacker == null)
                throw new ArgumentNullException(nameof(attacker));
            if (defender == null)
                throw new ArgumentNullException(nameof(defender));

            if (!attacker.IsAlive)
                return new CombatResult(false, 0, "Attacker is not alive");

            if (!defender.IsAlive)
                return new CombatResult(false, 0, "Defender is already defeated");

            int damage = attacker.CalculateAttackDamage();
            int healthBefore = defender.CurrentHealth;
            defender.TakeDamage(damage);
            int actualDamage = healthBefore - defender.CurrentHealth;

            string weaponInfo = attacker.EquippedWeapon != null
                ? $" with {attacker.EquippedWeapon.Name}"
                : "";

            OnCombatMessage?.Invoke($"{attacker.Name} attacks {defender.Name}{weaponInfo} for {actualDamage} damage!");
            OnCombatMessage?.Invoke($"{defender.Name} has {defender.CurrentHealth}/{defender.MaxHealth} HP remaining.");

            bool isDefenderDefeated = !defender.IsAlive;
            return new CombatResult(true, actualDamage, null, isDefenderDefeated);
        }
    }

    /// <summary>
    /// Result of a combat action
    /// </summary>
    public class CombatResult
    {
        public bool Success { get; }
        public int Damage { get; }
        public string ErrorMessage { get; }
        public bool TargetDefeated { get; }

        public CombatResult(bool success, int damage, string errorMessage = null, bool targetDefeated = false)
        {
            Success = success;
            Damage = damage;
            ErrorMessage = errorMessage;
            TargetDefeated = targetDefeated;
        }
    }
    #endregion

    #region UI System
    /// <summary>
    /// Handles all user interface and presentation logic
    /// </summary>
    public static class GameUI
    {
        public static void ShowWelcome()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("   Welcome to 'No Heroes for the Wicked'!");
            Console.WriteLine("========================================");
            Console.WriteLine();
        }

        public static void ShowCharacterInfo(ICharacter character)
        {
            string weaponInfo = character.EquippedWeapon != null
                ? $", wielding {character.EquippedWeapon}"
                : "";
            Console.WriteLine($"{character.Name}: {character.CurrentHealth}/{character.MaxHealth} HP{weaponInfo}");
        }

        public static void ShowCombatStart(ICharacter player, ICharacter enemy)
        {
            ShowCharacterInfo(player);
            ShowCharacterInfo(enemy);
            Console.WriteLine();
        }

        public static string GetPlayerInput(string prompt)
        {
            Console.WriteLine(prompt);
            string input = Console.ReadLine();
            return input?.Trim() ?? string.Empty;
        }

        public static bool AskYesNo(string question)
        {
            while (true)
            {
                string input = GetPlayerInput($"{question} (yes/no)");
                string normalized = input.ToLower();

                if (normalized == "yes" || normalized == "y")
                    return true;
                if (normalized == "no" || normalized == "n")
                    return false;

                Console.WriteLine("Invalid input. Please type 'yes' or 'no'.");
                Console.WriteLine();
            }
        }

        public static void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }

        public static void ShowVictory(ICharacter victor, ICharacter defeated)
        {
            Console.WriteLine();
            Console.WriteLine($"*** {victor.Name} has defeated {defeated.Name}! ***");
        }

        public static void ShowGameOver()
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("           Game Over");
            Console.WriteLine("========================================");
        }

        public static int ShowWeaponSelection(List<Weapon> weapons)
        {
            Console.WriteLine("\nAvailable weapons:");
            for (int i = 0; i < weapons.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {weapons[i]}");
            }

            while (true)
            {
                Console.Write($"\nSelect a weapon (1-{weapons.Count}): ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int choice) && choice >= 1 && choice <= weapons.Count)
                {
                    return choice - 1;
                }

                Console.WriteLine("Invalid selection. Please try again.");
            }
        }
    }
    #endregion

    #region Game Controller
    /// <summary>
    /// Main game controller
    /// </summary>
    public class Game
    {
        private Player _player;
        private Enemy _enemy;
        private CombatSystem _combatSystem;
        private List<Weapon> _availableWeapons;

        public Game()
        {
            _combatSystem = new CombatSystem();
            _combatSystem.OnCombatMessage += GameUI.ShowMessage;
            _availableWeapons = WeaponLoader.LoadWeapons(GameConfig.WeaponsDataFile);
        }

        public void Start()
        {
            GameUI.ShowWelcome();

            // Setup player
            string playerName = GameUI.GetPlayerInput("Enter your hero's name:");
            if (string.IsNullOrWhiteSpace(playerName))
                playerName = "Hero";

            // Let player choose weapon
            int weaponIndex = GameUI.ShowWeaponSelection(_availableWeapons);
            Weapon playerWeapon = _availableWeapons[weaponIndex];

            _player = new Player(playerName, playerWeapon);

            // Setup enemy with random weapon
            Random random = new Random();
            Weapon enemyWeapon = _availableWeapons[random.Next(_availableWeapons.Count)];
            _enemy = new Enemy("Troll", "Troll", enemyWeapon);

            Console.WriteLine();
            GameUI.ShowCombatStart(_player, _enemy);

            RunGameLoop();

            GameUI.ShowGameOver();
        }

        private void RunGameLoop()
        {
            while (_player.IsAlive && _enemy.IsAlive)
            {
                bool playerWantsToAttack = GameUI.AskYesNo($"Do you want to attack the {_enemy.Name}?");

                if (playerWantsToAttack)
                {
                    // Player attacks
                    var result = _combatSystem.ProcessAttack(_player, _enemy);

                    if (result.TargetDefeated)
                    {
                        GameUI.ShowVictory(_player, _enemy);
                        break;
                    }

                    // Enemy counter-attacks
                    var counterResult = _combatSystem.ProcessAttack(_enemy, _player);

                    if (counterResult.TargetDefeated)
                    {
                        GameUI.ShowVictory(_enemy, _player);
                        break;
                    }

                    Console.WriteLine();
                }
                else
                {
                    GameUI.ShowMessage($"You decided not to attack. The {_enemy.Name} stares at you menacingly...");
                    Console.WriteLine();
                }
            }
        }
    }
    #endregion

    #region Program Entry Point
    /// <summary>
    /// Main program entry point
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var game = new Game();
                game.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
    #endregion
}
