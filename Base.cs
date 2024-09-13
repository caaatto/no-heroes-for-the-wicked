using System;

class Program
{
    static void Main(string[] args)
    {
        // Start game by initializing the player and troll
        Player player = new Player("Hero", 100);
        Enemy troll = new Enemy("Troll", 50);

        Console.WriteLine("Welcome to 'No Heroes for the Wicked'!");
        Console.WriteLine("You are " + player.Name + " with " + player.LifePoints + " life points.");
        Console.WriteLine("You face a " + troll.Name + " with " + troll.LifePoints + " life points.");
        Console.WriteLine();

        // Simple loop to demonstrate the interaction
        while (player.IsAlive() && troll.IsAlive)
        {
            Console.WriteLine("Do you want to attack the Troll? (yes/no)");
            string input = Console.ReadLine();

            if (input.ToLower() == "yes")
            {
                // Attack logic (basic for now)
                player.Attack(troll);

                // Check if troll is dead
                if (!troll.IsAlive)
                {
                    Console.WriteLine("You have defeated the Troll!");
                    break;
                }

                // Troll counter-attacks
                troll.Attack(player);

                // Check if player is dead
                if (!player.IsAlive())
                {
                    Console.WriteLine("The Troll has defeated you!");
                    break;
                }

                Console.WriteLine();
            }
            else if (input.ToLower() == "no")
            {
                Console.WriteLine("You decided not to attack. The Troll stares at you menacingly...");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Invalid input. Please type 'yes' or 'no'.");
            }
        }

        Console.WriteLine("Game over.");
    }
}

// Simple Player class
class Player
{
    public string Name { get; }
    public int LifePoints { get; set; }
    public int AttackValue { get; set; }

    public Player(string name, int lifePoints)
    {
        Name = name;
        LifePoints = lifePoints;
        AttackValue = 10;  // Default attack value
    }

    public bool IsAlive()
    {
        return LifePoints > 0;
    }

    public void Attack(Enemy enemy)
    {
        Console.WriteLine(Name + " attacks the Troll for " + AttackValue + " damage.");
        enemy.TakeDamage(AttackValue);
    }
}

// Simple Enemy class (Troll)
class Enemy
{
    public string Name { get; }
    public int LifePoints { get; set; }

    public Enemy(string name, int lifePoints)
    {
        Name = name;
        LifePoints = lifePoints;
    }

    public bool IsAlive
    {
        get { return LifePoints > 0; }
    }

    public void TakeDamage(int damage)
    {
        LifePoints -= damage;
        Console.WriteLine(Name + " takes " + damage + " damage. Remaining life: " + LifePoints);
    }

    public void Attack(Player player)
    {
        int trollAttack = 8;  // Troll's attack value
        Console.WriteLine(Name + " attacks " + player.Name + " for " + trollAttack + " damage.");
        player.LifePoints -= trollAttack;
        Console.WriteLine(player.Name + " has " + player.LifePoints + " life points remaining.");
    }
}
