public class GameManager
{
    private Player player;
    private Enemy troll;

    public GameManager()
    {
        player = new Player(100);  // Spieler mit 100 Lebenspunkten initialisieren
        troll = new Enemy(50);     // Troll mit 50 Lebenspunkten initialisieren
    }

    public void StartGame()
    {
        // Zeige den Willkommensbildschirm
        ShowWelcomeScreen();

        // Starte das Spiel nach der Willkommensnachricht
        while (player.IsAlive() && troll.IsAlive)
        {
            PlayTurn();
        }

        // Spiel beenden
        if (!player.IsAlive())
        {
            Console.WriteLine("The player has been defeated!");
        }
        else if (!troll.IsAlive)
        {
            Console.WriteLine("The troll has been defeated!");
        }
    }

    // Willkommensbildschirm
    public void ShowWelcomeScreen()
    {
        Console.Clear();
        Console.WriteLine("**************************************");
        Console.WriteLine("*       WELCOME TO THE BATTLE        *");
        Console.WriteLine("*    No Heroes for the Wicked Game!   *");
        Console.WriteLine("**************************************");
        Console.WriteLine("\nGet ready for an epic battle!");
        Console.WriteLine("Press any key to start...");
        Console.ReadKey();  // Warte, bis der Spieler eine Taste drückt
        Console.Clear();    // Bereinige den Bildschirm, bevor das Spiel startet
    }

    public void PlayTurn()
    {
        // Spielablauf wie oben beschrieben
    }
}
