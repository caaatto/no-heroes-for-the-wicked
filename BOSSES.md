# Boss Enemy System

## Overview

Das Spiel verfügt über ein vielfältiges Boss-System mit 5 unterschiedlichen Boss-Typen, die jeweils einzigartige Stärken, Schwächen und Kampfmechaniken besitzen.

## Boss-Typen

### 1. Tank Boss - "The Iron Colossus"

**Konzept:** Ein massiver, gepanzerter Gegner der durch schiere Kraft und Ausdauer dominiert.

**Stats:**
- HP: 300 (Höchste)
- Angriff: 25 (Sehr hoch)
- Geschwindigkeit: 60 (Sehr langsam)
- Range: 60 (Nahkampf)

**Stärken:**
- Massiver HP-Pool
- 20% Schadensreduktion
- Kann bis zu 5 Rüstungsstacks aufbauen (jeweils +5% Reduktion)
- Ground Pound AOE-Angriff (Radius: 150, Schaden: 25)
- Wird in Phase 2 noch stärker

**Schwächen:**
- Sehr langsame Bewegung
- Anfällig für Hit-and-Run-Taktiken
- Lange Angriffs-Cooldowns (Ground Pound: 5 Sekunden)
- Vorhersehbare Angriffsmuster

**Besondere Fähigkeiten:**
- **Ground Pound:** Flächenschaden der mit Distanz abnimmt
- **Armor Stacks:** Gewinnt Rüstung im Kampf
- **Phase 2 (50% HP):** Schnellere Angriffe, mehr Ground Pounds
- **Enrage (30% HP):** Maximale Rüstung, noch mehr Schaden

**Strategie:**
- Halte Distanz und nutze Fernkampf wenn möglich
- Weiche Ground Pound-Angriffen aus
- Chip-Damage über Zeit
- Nutze die langsame Geschwindigkeit für Hit-and-Run

---

### 2. Speed Boss - "The Shadow Blade"

**Konzept:** Ein blitzschneller Assassine der durch Geschwindigkeit und Präzision tötet.

**Stats:**
- HP: 120 (Niedrig)
- Angriff: 12 (Mittel, aber schnelle Combos)
- Geschwindigkeit: 180 (Sehr schnell)
- Range: 45 (Nahkampf)

**Stärken:**
- Extrem hohe Mobilität
- Dash-Fähigkeit (400 Speed, 200 Distance)
- 3-Hit Combo-System mit Finisher (2x Schaden)
- 15% Dodge-Chance
- Afterimage-Effekte erschweren das Zielen

**Schwächen:**
- Niedriger HP-Pool
- Stirbt schnell bei getroffenen Angriffen
- Vorhersehbare Dash-Muster
- Muss nah rankommen für Schaden

**Besondere Fähigkeiten:**
- **Dash Attack:** Schneller Vorstoß mit Bonus-Schaden
- **Combo System:** Baut bis zu 3 Hits auf, Finisher macht 2x Schaden
- **Afterimages:** Erzeugt Nachbilder die verwirren
- **Phase 2 (50% HP):** Noch schneller, kürzere Dash-Cooldowns
- **Enrage (30% HP):** Maximum Speed (250), konstante Afterimages

**Strategie:**
- Timing ist alles - lerne die Angriffsmuster
- Blockiere oder dodgede nach dem 2. Combo-Hit
- Nutze AOE-Angriffe da er schwer zu treffen ist
- Counter-Angriffe wenn der Dash vorbei ist

---

### 3. Ranged Boss - "The Arcane Sorcerer"

**Konzept:** Ein mächtiger Magier der durch Fernkampf und Gebietskontrolle dominiert.

**Stats:**
- HP: 180 (Mittel)
- Angriff: 10 (Niedrig im Nahkampf), 15 (Projektile)
- Geschwindigkeit: 120 (Mittel)
- Range: 300 (Sehr weit)

**Stärken:**
- Lange Angriffsreichweite
- Vielfältige Projektilmuster (Single, Spread, Circle)
- Teleport-Fähigkeit (250 Range, 8s Cooldown)
- Area Denial durch Hazard Zones
- Hält automatisch Distanz

**Schwächen:**
- Verwundbar im Nahkampf (nur 10 ATK)
- Abhängig von Positionierung
- Lange Projektil-Cooldowns (1.5s)
- Teleport hat lange Abklingzeit

**Besondere Fähigkeiten:**
- **Projektilmuster:**
  - Single Shot: Gezielter Schuss
  - Spread Shot: 3 Projektile in Fächer
  - Circle Pattern: 8 Richtungen gleichzeitig
- **Teleport:** Springt weg wenn bedroht (25% Chance bei Treffer)
- **Hazard Zones:** Hinterlässt gefährliche Bereiche (max 5)
- **3 Phasen:** Wird mit jeder Phase schneller und gefährlicher
- **Enrage (30% HP):** Rapid Fire (0.7s Cooldown), maximale Hazards

**Strategie:**
- Aggressiv in den Nahkampf gehen
- Projektilmuster lernen und ausweichen
- Vermeide Hazard Zones
- Unterbreche Teleport-Versuche
- Nutze Deckung wenn verfügbar

---

### 4. Berserker Boss - "The Blood Reaver"

**Konzept:** Ein Berserker der mit jedem verlorenen HP stärker und gefährlicher wird.

**Stats (Initial):**
- HP: 220 (Mittel-Hoch)
- Angriff: 18 (Steigt mit Rage)
- Geschwindigkeit: 140 (Steigt mit Rage)
- Range: 55 (Nahkampf mit Cleave)

**Stärken:**
- Exponentielles Scaling (wird stärker je weniger HP)
- 15-30% Lifesteal (steigt mit Phasen)
- Whirlwind AOE-Angriff
- Execute-Mode bei <15% HP
- Gewinnt Rage bei erlittenem Schaden
- Blood Trail (Damage over Time zones)

**Schwächen:**
- Relativ schwach bei voller HP
- Vorhersehbare Aggression
- Kann durch Burst-Damage früh besiegt werden
- Kein Ranged-Angriff

**Besondere Fähigkeiten:**
- **Rage System:**
  - Schaden = Base × (1 + HP_Lost²× 2)
  - Speed = Base × (1 + HP_Lost² × 0.5)
- **Lifesteal:** 15-50% je nach HP
- **Whirlwind:** Dreht und verfolgt Spieler (3-5s Duration)
- **Execute Mode (<15% HP):**
  - +15 ATK, +50% Speed, 50% Lifesteal
  - Bonus-Schaden gegen verwundete Feinde
- **Blood Trail:** Hinterlässt schädigende Zonen bei niedrigem HP
- **4 Phasen:** Jede Phase macht ihn aggressiver

**Strategie:**
- FRÜH maximalen Schaden machen
- Vermeide verlängerte Kämpfe
- Dodge Whirlwind-Angriffe
- Bei Execute-Mode: Defensive spielen oder schnell beenden
- Kite wenn möglich

---

### 5. Necromancer Boss - "The Undying Archlich"

**Konzept:** Ein Nekromant der eine Armee von Untoten befehligt und durch sie stark wird.

**Stats:**
- HP: 150 (Niedrig)
- Angriff: 8 (Sehr niedrig allein)
- Geschwindigkeit: 100 (Langsam)
- Range: 250 (Bevorzugt Distanz)

**Stärken:**
- Beschwört bis zu 15 Minions (Phase-abhängig)
- Life Drain (5-15 Schaden pro Sekunde, 150-200 Range)
- Curse-Debuff (-30% Speed, -20% ATK, DOT)
- Kann gefallene Minions wiederbeleben
- Damage Shield durch Minions (bis zu 30% Reduktion)
- Death Nova beim Tod (200 Radius, 30 Schaden)

**Schwächen:**
- Sehr schwach im direkten Kampf
- Niedrige HP
- Abhängig von Minions
- Lange Cast-Zeiten
- Verwundbar wenn Minions tot sind

**Besondere Fähigkeiten:**
- **Summon Minions:**
  - Phase 1: Max 5
  - Phase 2: Max 7
  - Phase 3: Max 10
  - Enrage: Max 15
- **Life Drain:** Kontinuierlicher Schaden + Heilung
- **Curse:** 8s Debuff, 12s Cooldown
- **Resurrection:** Belebt bis zu 3 Minions wieder (15s Cooldown)
- **Minion Shield:** Jeder Minion reduziert Schaden um 5% (max 30%)
- **Death Nova:** Finale AOE-Explosion beim Tod
- **Power Gain:** +2 ATK, +1 Drain pro gefallenem Minion

**Strategie:**
- PRIORITÄT: Töte Minions zuerst
- Nutze AOE-Angriffe gegen Minion-Gruppen
- Unterbreche Life Drain durch Distanz
- Vermeide Curse wenn möglich
- Burst-Damage wenn Minions tot sind
- Bereite dich auf Death Nova vor

---

## Boss Mechanics - Allgemein

### Phase System

Alle Bosse haben mehrere Phasen (2-4) basierend auf HP:
- Phase-Übergänge bei 75%, 50%, 25% HP
- Jede Phase macht den Boss stärker/gefährlicher
- Neue Fähigkeiten oder verbesserte Stats

### Enrage System

Bei 30% HP triggern alle Bosse "Enrage":
- +30% Movement Speed (base)
- +20% Attack (base)
- Boss-spezifische Enrage-Effekte
- Visueller Indikator (sollte implementiert werden)

### Boss UI

Jeder Boss sollte haben:
- Großer HP-Balken am oberen Bildschirmrand
- Boss-Name und Titel
- Phase-Indikator
- Status-Effekte (Enrage, etc.)

---

## Implementation Details

### File Structure
```
/scripts
├── BossController.cs          # Base-Klasse für alle Bosse
├── TankBoss.cs               # Iron Colossus
├── SpeedBoss.cs              # Shadow Blade
├── RangedBoss.cs             # Arcane Sorcerer
├── BerserkerBoss.cs          # Blood Reaver
└── NecromancerBoss.cs        # Undying Archlich

/scenes
├── boss_tank.tscn
├── boss_speed.tscn
├── boss_ranged.tscn
├── boss_berserker.tscn
└── boss_necromancer.tscn
```

### Verwendung

In Godot Editor:
1. Scene öffnen (z.B. `boss_arena.tscn`)
2. Boss-Scene als Child-Node hinzufügen
3. Position setzen
4. Optional: Stats in Inspector anpassen

Im Code:
```csharp
// Boss spawnen
var bossScene = GD.Load<PackedScene>("res://scenes/boss_tank.tscn");
var boss = bossScene.Instantiate<TankBoss>();
boss.GlobalPosition = new Vector2(640, 360);
AddChild(boss);

// Boss Events
boss.EnemyDied += OnBossDefeated;
```

### TODO / Future Improvements

- [ ] Projektil-System für Ranged Boss implementieren
- [ ] Minion-Enemies für Necromancer erstellen
- [ ] Visuelle Effekte (Particles, Shaders)
- [ ] Boss-Musik Tracks
- [ ] Loot-System und Drop-Tables
- [ ] Achievements für Boss-Kills
- [ ] Boss Rush Mode
- [ ] Difficulty Modifiers

---

## Balancing Notes

Aktuelle Werte sind erste Implementierung. Anpassungen basierend auf Playtesting:

**Tank Boss:** Eventuell zu langsam? Ground Pound-Range testen.
**Speed Boss:** Dodge-Chance von 15% könnte frustrierend sein.
**Ranged Boss:** Teleport-Cooldown balancen.
**Berserker Boss:** Execute-Mode könnte zu stark sein.
**Necromancer Boss:** Minion-Count und deren Stats anpassen.

Empfohlene Boss-Reihenfolge für Spieler:
1. Tank Boss (Tutorial-Boss, vorhersehbar)
2. Speed Boss (Einführung in Timing)
3. Ranged Boss (Positionierung lernen)
4. Berserker Boss (DPS-Check)
5. Necromancer Boss (Final Boss, alle Skills nötig)
