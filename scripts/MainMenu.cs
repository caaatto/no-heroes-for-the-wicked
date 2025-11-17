using Godot;
using System;

public partial class MainMenu : Control
{
    private Button _newGameButton;
    private Button _loadGameButton;
    private Button _quitButton;
    private SaveLoadSystem _saveLoadSystem;

    public override void _Ready()
    {
        // Get button references
        _newGameButton = GetNodeOrNull<Button>("MenuContainer/NewGameButton");
        _loadGameButton = GetNodeOrNull<Button>("MenuContainer/LoadGameButton");
        _quitButton = GetNodeOrNull<Button>("MenuContainer/QuitButton");

        _saveLoadSystem = GetNodeOrNull<SaveLoadSystem>("/root/SaveLoadSystem");

        // Connect button signals
        if (_newGameButton != null)
            _newGameButton.Pressed += OnNewGamePressed;

        if (_loadGameButton != null)
        {
            _loadGameButton.Pressed += OnLoadGamePressed;

            // Disable load button if no save exists
            if (_saveLoadSystem != null && !_saveLoadSystem.SaveFileExists())
            {
                _loadGameButton.Disabled = true;
            }
        }

        if (_quitButton != null)
            _quitButton.Pressed += OnQuitPressed;

        GD.Print("Main Menu ready");
    }

    private void OnNewGamePressed()
    {
        GD.Print("Starting new game...");
        GetTree().ChangeSceneToFile("res://scenes/hub.tscn");
    }

    private void OnLoadGamePressed()
    {
        if (_saveLoadSystem != null && _saveLoadSystem.SaveFileExists())
        {
            GD.Print("Loading game...");
            _saveLoadSystem.LoadGame();
        }
        else
        {
            GD.Print("No save file found!");
        }
    }

    private void OnQuitPressed()
    {
        GD.Print("Quitting game...");
        GetTree().Quit();
    }
}
