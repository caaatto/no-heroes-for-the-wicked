# 📘 README.md — No Heroes for the Wicked

## 🧩 Projektübersicht

**No Heroes for the Wicked** ist ein storygetriebenes 2D-Action-Adventure (Pixelart).

Der MVP umfasst:

- **Player Movement**
- **Basis-Kampf**
- **1 Hub-Level + 1 Boss-Arena**
- **Inventar**
- **Quest-System (3 Quests)**
- **Save/Load**
- **UI (HUD, Inventory, Pause)**
- **Audio (Music + SFX)**

Das Projekt ist für ein kleines Team (3 Personen) ausgelegt.

---

## 🛠️ Voraussetzungen

Installiert werden sollte:

- **Godot 4.x** (mit C# / .NET-Unterstützung)
- **.NET SDK** (6.x/7.x) – für C#-Entwicklung
- **Git**
- **Aseprite** (optional für Pixelart)
- **Audacity / Reaper** (für Audio)

---

## 🚀 Schnellstart (Development)

### 1. Repository klonen

```bash
git clone https://github.com/caaatto/no-heroes-for-the-wicked.git
cd no-heroes-for-the-wicked
```

### 2. Projekt in Godot öffnen

- Godot starten
- `project.godot` im Repository öffnen
- Prüfen, ob C# korrekt initialisiert ist (falls genutzt)

### 3. Abhängigkeiten

Falls C# genutzt wird:

```bash
dotnet restore
```

### 4. Spiel starten

Im Godot Editor:

- `main.tscn` oder `hub.tscn` öffnen
- **Play** drücken

---

## 🧱 Projektstruktur

```
/project-root
├─ /assets
│  ├─ /art
│  ├─ /audio
│  └─ /ui
├─ /scenes
│  ├─ main.tscn
│  ├─ hub.tscn
│  ├─ boss_arena.tscn
│  └─ player.tscn
├─ /scripts
│  ├─ PlayerController.cs
│  ├─ EnemyController.cs
│  ├─ InventorySystem.cs
│  ├─ QuestSystem.cs
│  └─ SaveLoad.cs
├─ /data
│  ├─ items.json
│  ├─ quests.json
│  └─ levels.json
├─ project.godot
├─ README.md
└─ /docs
   └─ design_notes.md
```

### Module-Übersicht

- **PlayerController** – Movement, Combat Hooks
- **EnemyController** – einfache Gegner-KI
- **InventorySystem** – Items, Pickup, UI-Verknüpfung
- **QuestSystem** – Questlogik & Fortschritt
- **SaveLoad** – JSON-Saves

---

## 🗺️ Milestones & Roadmap

### M1 — Setup & Movement (Woche 0–1)
Repo, Godot-Projekt, Player Movement, Kamera

### M2 — Combat (Woche 2–3)
Nahkampf, 2 Gegnertypen, HP-System

### M3 — Items & Save/Load (Woche 4)
Inventar, Items, JSON-Speicherung

### M4 — Quests & Levels (Woche 5–6)
Quest-System, Hub-Level, Boss-Arena

### M5 — Audio & Polish (Woche 7–9)
Musik, SFX, Feinschliff, finaler Build

---

## 🧪 Testing & QA

### Unit Tests

- Save/Load
- Inventory
- Queststatus

### Integration Tests

- Level Load/Save
- Quest-Trigger

### Playtests

- Wöchentliche Playtests
- Issues im Tracker dokumentieren

### Abnahmekriterien (MVP)

- Stabiler Spielablauf
- 3 Quests spielbar
- Keine Crashes
- Save/Load vollständig funktionsfähig

---

## 🤝 Contributing

### Branching

- `main` → release
- `develop` → aktueller Entwicklungsstand
- Feature-Branches: `feature/<name>`

### Commit-Style

```
feat(player): add dash ability
fix(quest): trigger edge-case
```

### Pull Requests

- Beschreibung + Screenshots
- Mindestens 1 Review

---

## ⚠️ Known Limitations & Offene Fragen

Einige Designentscheidungen sind Platzhalter (Spritegröße, Enginepräferenzen).

Bestätigung benötigt:

- Zielplattform(en)
- Engine (Godot vs Unity)
- Artstil (16px vs 32px)
- Vorhandene Assets

---

## 📄 License & Credits

- **Lizenz**: MIT
- Credits für externe Assets in `CREDITS.md`

---

## 📬 Kontakt

**Projektmaintainer:**
caaatto • [GitHub](https://github.com/caaatto/no-heroes-for-the-wicked)

---

## 📝 Annahmen & Aktueller Status

**Aktueller Stand:**
- Das Projekt befindet sich in früher Entwicklung
- Prototyp als C#-Konsolenanwendung implementiert (siehe `Base.cs`)
- Waffensystem in Entwicklung (siehe `Waffen.txt`)
- Migration zu Godot 4 geplant

**Annahmen:**
- Teamgröße: 3 Personen
- Engine: Godot 4 (C#)
- Pixelart-Adventure als Basis
