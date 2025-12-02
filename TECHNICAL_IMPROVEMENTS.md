# Technical Improvements - No Heroes for the Wicked

## Übersicht

Dieses Dokument beschreibt die umfangreichen technischen Verbesserungen, die dem Projekt hinzugefügt wurden.

---

## 1. Lokalisierungssystem (LocalizationManager)

**Datei:** `scripts/LocalizationManager.cs`

### Features

- **Mehrsprachigkeit:** Unterstützung für Deutsch (DE) und Englisch (EN)
- **Dynamic Language Switching:** Sprache kann zur Laufzeit gewechselt werden
- **Centralized Translations:** Alle Texte an einem Ort verwaltet
- **Signal-basiert:** Benachrichtigt andere Systeme bei Sprachwechsel

### Unterstützte Sprachen

- Deutsch (de) - Standard
- English (en)

### Verwendung

```csharp
// Lokalisierungs-Manager holen
var localization = GetNode<LocalizationManager>("/root/LocalizationManager");

// Text abrufen
string text = localization.GetText("menu_new_game"); // "Neues Spiel"

// Sprache wechseln
localization.SetLanguage("en");

// Sprache abfragen
string currentLang = localization.GetCurrentLanguage();
```

### Übersetzungskategorien

- **Main Menu:** Hauptmenü-Texte
- **Settings:** Einstellungsmenü
- **Tutorial:** Tutorial-Anweisungen
- **HUD:** Heads-Up Display
- **Inventory:** Inventar-UI
- **Quests:** Quest-System
- **Pause Menu:** Pausenmenü
- **Dialogue:** Dialogsystem
- **Combat:** Kampfsystem
- **Items:** Item-Beschreibungen
- **General:** Allgemeine UI-Texte
- **Boss Names:** Boss-Namen
- **Credits:** Credits-Screen

### Signal

- `LanguageChanged(string languageCode)` - Wird ausgelöst, wenn die Sprache gewechselt wird

---

## 2. Erweitertes Main Menu

**Datei:** `scripts/MainMenu.cs`

### Neue Features

- **5 Hauptbuttons:**
  - Neues Spiel (startet Tutorial bei Erstspiel)
  - Fortsetzen (lädt gespeichertes Spiel)
  - Einstellungen
  - Credits
  - Beenden

- **Intelligentes Continue-Button:** Automatisch deaktiviert, wenn kein Savegame existiert
- **Lokalisierung:** Alle Texte werden automatisch übersetzt
- **Audio-Feedback:** Hover- und Click-Sounds für alle Buttons
- **Gamepad-Unterstützung:** ESC zum Zurückkehren aus Untermenüs

### Panel-System

Das Main Menu nutzt drei Panels:
1. `MainMenuPanel` - Hauptmenü
2. `SettingsPanel` - Einstellungen
3. `CreditsPanel` - Credits

### Integration mit Tutorial

Bei Erstspiel (kein Savegame vorhanden) startet "Neues Spiel" automatisch das Tutorial:
```csharp
bool isFirstPlay = _saveLoadSystem == null || !_saveLoadSystem.SaveFileExists();
if (isFirstPlay)
    GetTree().ChangeSceneToFile("res://scenes/tutorial.tscn");
```

---

## 3. Settings Menu

**Datei:** `scripts/SettingsMenu.cs`

### Tabs

#### Audio-Tab
- **Master Volume:** Gesamtlautstärke (0-100%)
- **Music Volume:** Musiklautstärke (0-100%)
- **SFX Volume:** Effektlautstärke (0-100%)
- Echtzeit-Anzeige der Prozentwerte

#### Graphics-Tab
- **Fullscreen:** Vollbild-Modus an/aus
- **VSync:** Vertikale Synchronisation
- **Pixel Perfect:** Pixel-genaues Rendering

#### Language-Tab
- **Sprachauswahl:** Dropdown mit Deutsch/English
- Sofortige UI-Aktualisierung bei Wechsel

### Buttons

- **Apply:** Einstellungen übernehmen und speichern
- **Reset:** Auf Standardwerte zurücksetzen
- **Back:** Zurück zum Hauptmenü

### Standardwerte

```csharp
Master Volume: 80%
Music Volume: 70%
SFX Volume: 90%
Fullscreen: Nein
VSync: Ja
Pixel Perfect: Ja
Language: Deutsch
```

---

## 4. Tutorial System

**Datei:** `scripts/TutorialSystem.cs`

### Features

- **7 Tutorial-Schritte:**
  1. Willkommen
  2. Bewegung (WASD/Pfeiltasten)
  3. Angriff (Leertaste)
  4. Interaktion (E)
  5. Inventar (I)
  6. Pause (ESC)
  7. Abschluss

- **Interaktiv:** Wartet auf tatsächliche Spielereingaben
- **Überspringbar:** "Tutorial überspringen"-Button
- **Progress Bar:** Visueller Fortschritt (z.B. 3/7)
- **Lokalisiert:** Alle Texte in DE/EN verfügbar

### Tutorial-Ablauf

```
Willkommen → Bewegung → Angriff → Interaktion → Inventar → Pause → Fertig
     ↓           ↓          ↓           ↓           ↓         ↓        ↓
  [Weiter]   [Input]    [Input]     [Input]     [Input]   [Input]  [Hub]
```

### Signals

- `TutorialCompleted()` - Tutorial erfolgreich abgeschlossen
- `TutorialSkipped()` - Tutorial übersprungen

### Verwendung

```csharp
var tutorial = GetNode<TutorialSystem>("TutorialSystem");

// Tutorial starten
tutorial.StartTutorial();

// Tutorial stoppen
tutorial.StopTutorial();

// Fortschritt prüfen
float progress = tutorial.GetProgress(); // 0.0 - 1.0

// Status prüfen
bool active = tutorial.IsTutorialActive();
```

---

## 5. Credits Screen

**Datei:** `scripts/CreditsScreen.cs`

### Features

- **Auto-Scrolling:** Credits scrollen automatisch
- **Manuelles Scrollen:** Mit Mausrad oder Pfeiltasten
- **Sections:**
  - Titel
  - Team-Info
  - Programmierung
  - Grafik
  - Musik
  - Game Design
  - Special Thanks
  - Tools & Technologies
  - Copyright

### Auto-Scroll Verhalten

- Scrollt mit 30 Pixel/Sekunde
- Pausiert 3 Sekunden am Ende
- Resetted und startet von vorne
- Pausiert bei manueller Interaktion für 2 Sekunden

### Anpassung

Credits können einfach in der `BuildCredits()` Methode angepasst werden:
```csharp
AddSection("Neue Sektion");
AddText("Text hier");
AddSpacing(30);
```

---

## 6. Gamepad-Unterstützung

**Datei:** `project.godot` (Input-Mapping aktualisiert)

### Controller-Mappings

| Aktion | Tastatur | Gamepad |
|--------|----------|---------|
| Bewegung Links | A / ← | D-Pad Links / L-Stick Links |
| Bewegung Rechts | D / → | D-Pad Rechts / L-Stick Rechts |
| Bewegung Hoch | W / ↑ | D-Pad Hoch / L-Stick Hoch |
| Bewegung Runter | S / ↓ | D-Pad Runter / L-Stick Runter |
| Angriff | Leertaste | A-Button (Xbox) / X-Button (PS) |
| Interaktion | E | B-Button (Xbox) / O-Button (PS) |
| Inventar | I | X-Button (Xbox) / □-Button (PS) |

### Analog-Stick Support

Beide Analog-Sticks (Achsen 0 und 1) werden für Bewegung unterstützt:
- **Deadzone:** 0.5 (konfigurierbar)
- **Links/Rechts:** Axis 0 (-1.0 / +1.0)
- **Hoch/Runter:** Axis 1 (-1.0 / +1.0)

### D-Pad Support

Alle vier D-Pad-Richtungen sind ebenfalls gemappt:
- D-Pad Links: Button 13
- D-Pad Rechts: Button 14
- D-Pad Hoch: Button 11
- D-Pad Runter: Button 12

---

## 7. Autoload-System aktualisiert

**Datei:** `project.godot`

### Registrierte Autoloads

```gdscript
[autoload]
LocalizationManager="*res://scripts/LocalizationManager.cs"
AudioManager="*res://scripts/AudioManager.cs"
InventorySystem="*res://scripts/InventorySystem.cs"
QuestSystem="*res://scripts/QuestSystem.cs"
SaveLoadSystem="*res://scripts/SaveLoadSystem.cs"
```

Alle Systeme sind global verfügbar über:
```csharp
GetNode<SystemName>("/root/SystemName")
```

---

## Integration Guide

### 1. Main Menu Scene Setup

```
MainMenu (Control)
├── MainMenuPanel (Control)
│   └── MenuContainer (VBoxContainer)
│       ├── NewGameButton
│       ├── ContinueButton
│       ├── SettingsButton
│       ├── CreditsButton
│       └── QuitButton
├── SettingsPanel (Control) - SettingsMenu.cs
│   ├── TabContainer
│   │   ├── Audio (Tab)
│   │   ├── Graphics (Tab)
│   │   └── Language (Tab)
│   └── ButtonContainer
│       ├── ApplyButton
│       ├── ResetButton
│       └── BackButton
└── CreditsPanel (Control) - CreditsScreen.cs
    ├── ScrollContainer
    │   └── CreditsContainer (VBoxContainer)
    └── BackButton
```

### 2. Tutorial Scene Setup

```
Tutorial (Node)
└── TutorialSystem (CanvasLayer)
    └── TutorialPanel (Control)
        ├── TitleLabel
        ├── InstructionLabel
        ├── ProgressBar
        ├── SkipButton
        └── NextButton
```

### 3. Required Input Actions

Stelle sicher, dass folgende Input Actions in `project.godot` definiert sind:
- `ui_cancel` (ESC)
- `ui_up` / `ui_down` (Pfeiltasten)
- `move_left` / `move_right` / `move_up` / `move_down`
- `attack`
- `interact`
- `inventory`

---

## Best Practices

### Lokalisierung

1. **Neue Texte hinzufügen:**
   - Füge Keys in beiden Sprachen (DE + EN) hinzu
   - Nutze aussagekräftige Key-Namen (z.B. `menu_new_game`)
   - Gruppiere Keys nach Feature (`menu_`, `settings_`, etc.)

2. **Texte abrufen:**
   ```csharp
   var loc = GetNode<LocalizationManager>("/root/LocalizationManager");
   buttonText = loc.GetText("menu_new_game");
   ```

3. **Auf Sprachwechsel reagieren:**
   ```csharp
   _localization.LanguageChanged += OnLanguageChanged;

   private void OnLanguageChanged(string languageCode)
   {
       UpdateAllUIText();
   }
   ```

### Settings Persistence

Die Settings werden aktuell in Echtzeit angewendet. Für Persistence:
1. Erweitere `SaveLoadSystem` um Settings-Speicherung
2. Nutze JSON oder Godot's ConfigFile
3. Lade Settings in `SettingsMenu._Ready()`

### Tutorial Customization

Neue Tutorial-Schritte hinzufügen:
```csharp
_tutorialSteps.Add(new TutorialStep
{
    TitleKey = "tutorial_new_mechanic",
    InstructionKey = "tutorial_new_mechanic",
    RequiredAction = "special_move",
    CanSkip = false
});
```

---

## Testing Checklist

### Main Menu
- [ ] Alle Buttons funktionieren
- [ ] Continue-Button disabled bei fehlendem Save
- [ ] Sprachwechsel funktioniert
- [ ] Audio-Feedback bei Hover/Click
- [ ] ESC geht zurück aus Untermenüs

### Settings
- [ ] Lautstärkeregler funktionieren in Echtzeit
- [ ] Fullscreen Toggle funktioniert
- [ ] VSync kann umgeschaltet werden
- [ ] Sprachwechsel aktualisiert UI sofort
- [ ] Reset-Button stellt Defaults wieder her

### Tutorial
- [ ] Tutorial startet bei erstem "Neues Spiel"
- [ ] Alle Schritte erkennen Input korrekt
- [ ] Skip-Button funktioniert
- [ ] Progress Bar zeigt korrekten Fortschritt
- [ ] Tutorial führt nach Abschluss zum Hub

### Gamepad
- [ ] Bewegung mit D-Pad funktioniert
- [ ] Bewegung mit L-Stick funktioniert
- [ ] Alle Action-Buttons reagieren
- [ ] Deadzone verhindert Drift
- [ ] UI-Navigation mit Gamepad möglich

---

## Bekannte Limitierungen

1. **Settings Persistence:** Settings werden noch nicht dauerhaft gespeichert
2. **Controller Remapping:** Kein UI für Button-Remapping vorhanden
3. **Tutorial Scene:** Muss noch als `.tscn` Datei erstellt werden
4. **Audio Files:** Placeholder - echte Audio-Assets fehlen noch
5. **Gamepad Vibration:** Noch nicht implementiert

---

## Zukünftige Erweiterungen

- [ ] Settings in Config-File speichern
- [ ] Controller-Remapping UI
- [ ] Mehr Sprachen (FR, ES, IT, etc.)
- [ ] Accessibility-Optionen (Textgröße, Farbblindmodus)
- [ ] Cloud-Saves
- [ ] Achievements-Integration
- [ ] Steam/Epic/GOG-Integration

---

## Credits für technische Verbesserungen

**Implementiert von:** Claude Code + caaatto
**Datum:** 2025-12-02
**Version:** 1.0
**Engine:** Godot 4.x mit C#

---

## Kontakt & Support

Bei Fragen oder Problemen:
- **GitHub Issues:** https://github.com/caaatto/no-heroes-for-the-wicked/issues
- **Maintainer:** caaatto

---

**Happy Coding! 🎮**
