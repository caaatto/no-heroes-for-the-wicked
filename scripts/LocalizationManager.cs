using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages game localization (translation) system
/// Supports multiple languages and provides easy text lookup
/// </summary>
public partial class LocalizationManager : Node
{
    [Signal]
    public delegate void LanguageChangedEventHandler(string languageCode);

    private string _currentLanguage = "de"; // Default to German
    private Dictionary<string, Dictionary<string, string>> _translations;

    public override void _Ready()
    {
        InitializeTranslations();
        GD.Print($"LocalizationManager ready. Current language: {_currentLanguage}");
    }

    private void InitializeTranslations()
    {
        _translations = new Dictionary<string, Dictionary<string, string>>
        {
            // German translations
            ["de"] = new Dictionary<string, string>
            {
                // Main Menu
                ["menu_new_game"] = "Neues Spiel",
                ["menu_continue"] = "Fortsetzen",
                ["menu_settings"] = "Einstellungen",
                ["menu_credits"] = "Credits",
                ["menu_quit"] = "Beenden",
                ["menu_title"] = "No Heroes for the Wicked",

                // Settings Menu
                ["settings_audio"] = "Audio",
                ["settings_controls"] = "Steuerung",
                ["settings_graphics"] = "Grafik",
                ["settings_language"] = "Sprache",
                ["settings_master_volume"] = "Gesamtlautstärke",
                ["settings_music_volume"] = "Musiklautstärke",
                ["settings_sfx_volume"] = "Effektlautstärke",
                ["settings_fullscreen"] = "Vollbild",
                ["settings_vsync"] = "VSync",
                ["settings_pixel_perfect"] = "Pixel Perfect",
                ["settings_apply"] = "Übernehmen",
                ["settings_back"] = "Zurück",
                ["settings_reset"] = "Zurücksetzen",

                // Tutorial
                ["tutorial_welcome"] = "Willkommen in der Taverne!",
                ["tutorial_movement"] = "Benutze WASD oder die Pfeiltasten zum Bewegen",
                ["tutorial_attack"] = "Drücke LEERTASTE zum Angreifen",
                ["tutorial_interact"] = "Drücke E zum Interagieren",
                ["tutorial_inventory"] = "Drücke I um dein Inventar zu öffnen",
                ["tutorial_pause"] = "Drücke ESC um das Pausenmenü zu öffnen",
                ["tutorial_complete"] = "Tutorial abgeschlossen!",
                ["tutorial_skip"] = "Tutorial überspringen",

                // HUD
                ["hud_health"] = "Leben",
                ["hud_level"] = "Level",
                ["hud_gold"] = "Gold",

                // Inventory
                ["inventory_title"] = "Inventar",
                ["inventory_equipped"] = "Ausgerüstet",
                ["inventory_empty"] = "Leer",
                ["inventory_equip"] = "Ausrüsten",
                ["inventory_drop"] = "Wegwerfen",
                ["inventory_use"] = "Benutzen",

                // Quests
                ["quest_log"] = "Questlog",
                ["quest_active"] = "Aktive Quests",
                ["quest_completed"] = "Abgeschlossen",
                ["quest_failed"] = "Fehlgeschlagen",

                // Pause Menu
                ["pause_resume"] = "Fortsetzen",
                ["pause_settings"] = "Einstellungen",
                ["pause_save"] = "Speichern",
                ["pause_load"] = "Laden",
                ["pause_main_menu"] = "Hauptmenü",
                ["pause_quit"] = "Beenden",

                // Dialogue
                ["dialogue_continue"] = "Weiter (Leertaste)",
                ["dialogue_skip"] = "Überspringen",

                // Combat
                ["combat_damage"] = "Schaden",
                ["combat_critical"] = "Kritisch!",
                ["combat_dodge"] = "Ausweichen!",
                ["combat_blocked"] = "Geblockt",

                // Items
                ["item_weapon"] = "Waffe",
                ["item_consumable"] = "Verbrauchsgegenstand",
                ["item_key_item"] = "Schlüsselgegenstand",
                ["item_rarity_common"] = "Gewöhnlich",
                ["item_rarity_uncommon"] = "Ungewöhnlich",
                ["item_rarity_rare"] = "Selten",
                ["item_rarity_legendary"] = "Legendär",

                // General
                ["yes"] = "Ja",
                ["no"] = "Nein",
                ["ok"] = "OK",
                ["cancel"] = "Abbrechen",
                ["confirm"] = "Bestätigen",
                ["back"] = "Zurück",
                ["next"] = "Weiter",
                ["loading"] = "Lädt...",
                ["saved"] = "Gespeichert!",
                ["save_failed"] = "Speichern fehlgeschlagen!",

                // Boss Names
                ["boss_tank"] = "Der Eiserne Koloss",
                ["boss_berserker"] = "Der Wütende Barbar",
                ["boss_speed"] = "Der Schattenläufer",
                ["boss_ranged"] = "Der Dunkle Schütze",
                ["boss_necromancer"] = "Der Totenbeschörer",

                // Credits
                ["credits_title"] = "Credits",
                ["credits_developed_by"] = "Entwickelt von",
                ["credits_programming"] = "Programmierung",
                ["credits_art"] = "Grafik",
                ["credits_music"] = "Musik",
                ["credits_special_thanks"] = "Besonderer Dank an",
            },

            // English translations
            ["en"] = new Dictionary<string, string>
            {
                // Main Menu
                ["menu_new_game"] = "New Game",
                ["menu_continue"] = "Continue",
                ["menu_settings"] = "Settings",
                ["menu_credits"] = "Credits",
                ["menu_quit"] = "Quit",
                ["menu_title"] = "No Heroes for the Wicked",

                // Settings Menu
                ["settings_audio"] = "Audio",
                ["settings_controls"] = "Controls",
                ["settings_graphics"] = "Graphics",
                ["settings_language"] = "Language",
                ["settings_master_volume"] = "Master Volume",
                ["settings_music_volume"] = "Music Volume",
                ["settings_sfx_volume"] = "SFX Volume",
                ["settings_fullscreen"] = "Fullscreen",
                ["settings_vsync"] = "VSync",
                ["settings_pixel_perfect"] = "Pixel Perfect",
                ["settings_apply"] = "Apply",
                ["settings_back"] = "Back",
                ["settings_reset"] = "Reset",

                // Tutorial
                ["tutorial_welcome"] = "Welcome to the Tavern!",
                ["tutorial_movement"] = "Use WASD or Arrow Keys to move",
                ["tutorial_attack"] = "Press SPACE to attack",
                ["tutorial_interact"] = "Press E to interact",
                ["tutorial_inventory"] = "Press I to open your inventory",
                ["tutorial_pause"] = "Press ESC to open the pause menu",
                ["tutorial_complete"] = "Tutorial complete!",
                ["tutorial_skip"] = "Skip Tutorial",

                // HUD
                ["hud_health"] = "Health",
                ["hud_level"] = "Level",
                ["hud_gold"] = "Gold",

                // Inventory
                ["inventory_title"] = "Inventory",
                ["inventory_equipped"] = "Equipped",
                ["inventory_empty"] = "Empty",
                ["inventory_equip"] = "Equip",
                ["inventory_drop"] = "Drop",
                ["inventory_use"] = "Use",

                // Quests
                ["quest_log"] = "Quest Log",
                ["quest_active"] = "Active Quests",
                ["quest_completed"] = "Completed",
                ["quest_failed"] = "Failed",

                // Pause Menu
                ["pause_resume"] = "Resume",
                ["pause_settings"] = "Settings",
                ["pause_save"] = "Save",
                ["pause_load"] = "Load",
                ["pause_main_menu"] = "Main Menu",
                ["pause_quit"] = "Quit",

                // Dialogue
                ["dialogue_continue"] = "Continue (Space)",
                ["dialogue_skip"] = "Skip",

                // Combat
                ["combat_damage"] = "Damage",
                ["combat_critical"] = "Critical!",
                ["combat_dodge"] = "Dodge!",
                ["combat_blocked"] = "Blocked",

                // Items
                ["item_weapon"] = "Weapon",
                ["item_consumable"] = "Consumable",
                ["item_key_item"] = "Key Item",
                ["item_rarity_common"] = "Common",
                ["item_rarity_uncommon"] = "Uncommon",
                ["item_rarity_rare"] = "Rare",
                ["item_rarity_legendary"] = "Legendary",

                // General
                ["yes"] = "Yes",
                ["no"] = "No",
                ["ok"] = "OK",
                ["cancel"] = "Cancel",
                ["confirm"] = "Confirm",
                ["back"] = "Back",
                ["next"] = "Next",
                ["loading"] = "Loading...",
                ["saved"] = "Saved!",
                ["save_failed"] = "Save failed!",

                // Boss Names
                ["boss_tank"] = "The Iron Colossus",
                ["boss_berserker"] = "The Raging Barbarian",
                ["boss_speed"] = "The Shadow Runner",
                ["boss_ranged"] = "The Dark Archer",
                ["boss_necromancer"] = "The Necromancer",

                // Credits
                ["credits_title"] = "Credits",
                ["credits_developed_by"] = "Developed by",
                ["credits_programming"] = "Programming",
                ["credits_art"] = "Art",
                ["credits_music"] = "Music",
                ["credits_special_thanks"] = "Special Thanks to",
            }
        };
    }

    /// <summary>
    /// Get translated text for the current language
    /// </summary>
    public string GetText(string key)
    {
        if (_translations.ContainsKey(_currentLanguage) &&
            _translations[_currentLanguage].ContainsKey(key))
        {
            return _translations[_currentLanguage][key];
        }

        // Fallback to German if translation not found
        if (_currentLanguage != "de" && _translations["de"].ContainsKey(key))
        {
            GD.PushWarning($"Translation key '{key}' not found for language '{_currentLanguage}', using German fallback");
            return _translations["de"][key];
        }

        GD.PushWarning($"Translation key '{key}' not found!");
        return $"[{key}]";
    }

    /// <summary>
    /// Set the current language
    /// </summary>
    public void SetLanguage(string languageCode)
    {
        if (!_translations.ContainsKey(languageCode))
        {
            GD.PrintErr($"Language '{languageCode}' not supported!");
            return;
        }

        _currentLanguage = languageCode;
        EmitSignal(SignalName.LanguageChanged, languageCode);
        GD.Print($"Language changed to: {languageCode}");
    }

    /// <summary>
    /// Get the current language code
    /// </summary>
    public string GetCurrentLanguage()
    {
        return _currentLanguage;
    }

    /// <summary>
    /// Get all supported language codes
    /// </summary>
    public string[] GetSupportedLanguages()
    {
        return new string[] { "de", "en" };
    }

    /// <summary>
    /// Get display name for a language code
    /// </summary>
    public string GetLanguageDisplayName(string languageCode)
    {
        return languageCode switch
        {
            "de" => "Deutsch",
            "en" => "English",
            _ => languageCode
        };
    }

    /// <summary>
    /// Helper method to translate text (shorthand for GetText)
    /// </summary>
    public string Tr(string key) => GetText(key);
}
