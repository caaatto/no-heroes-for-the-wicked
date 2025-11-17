# Game Systems Documentation

This document describes all the game systems implemented in "No Heroes for the Wicked".

## Table of Contents

1. [Audio System](#audio-system)
2. [Pause Menu UI](#pause-menu-ui)
3. [Inventory Grid UI](#inventory-grid-ui)
4. [Particle Effects System](#particle-effects-system)
5. [Dialogue System](#dialogue-system)
6. [Minimap System](#minimap-system)
7. [Enemy Types](#enemy-types)
8. [Weapons](#weapons)

---

## Audio System

**File:** `scripts/AudioManager.cs`

### Features

- **Music Playback:** Crossfade support, loop control
- **Sound Effects:** Pooled SFX players (20 channels)
- **Volume Control:** Separate Master, Music, and SFX volume
- **Audio Library:** Configurable music tracks and sound effects

### Setup

1. Add `AudioManager` as an autoload singleton named `AudioManager`
2. Create audio directories:
   - `res://audio/music/` - Music tracks
   - `res://audio/sfx/` - Sound effects

### Usage

```csharp
// Get the audio manager
var audioManager = GetNode<AudioManager>("/root/AudioManager");

// Play music
audioManager.PlayMusic("combat", loop: true, crossfade: true);

// Play sound effect
audioManager.PlaySfx("player_attack");

// Play with pitch variation
audioManager.PlaySfx("footstep", pitchVariation: 0.1f);

// Set volumes (0.0 to 1.0)
audioManager.SetMasterVolume(0.8f);
audioManager.SetMusicVolume(0.7f);
audioManager.SetSfxVolume(0.9f);
```

### Predefined Music Tracks

- `main_menu` - Main menu theme
- `hub` - Safe area theme
- `combat` - Combat theme
- `boss_battle` - Boss battle theme
- `victory` - Victory fanfare
- `game_over` - Game over music

### Predefined SFX

**Player:**
- `player_attack`, `player_hit`, `player_death`, `player_heal`, `footstep`

**Enemies:**
- `enemy_hit`, `enemy_death`, `enemy_attack`

**Bosses:**
- `boss_roar`, `boss_attack`, `boss_phase_transition`, `boss_enrage`, `boss_death`

**UI:**
- `button_click`, `button_hover`, `menu_open`, `menu_close`
- `inventory_open`, `item_pickup`, `item_equip`, `quest_complete`

**Combat:**
- `sword_slash`, `heavy_impact`, `projectile_launch`, `explosion`, `shield_block`

**Magic:**
- `spell_cast`, `teleport`, `summon`, `curse`, `life_drain`

---

## Pause Menu UI

**File:** `scripts/PauseMenu.cs`

### Features

- Pause/resume game
- Settings panel with volume sliders
- Save/Load game integration
- Restart and quit options
- ESC key to toggle

### Setup

Add `PauseMenu` node to your main scene as a `CanvasLayer`.

### Usage

```csharp
// Get pause menu
var pauseMenu = GetNode<PauseMenu>("PauseMenu");

// Toggle pause
pauseMenu.TogglePause();

// Set pause state
pauseMenu.SetPaused(true);

// Check if paused
bool isPaused = pauseMenu.IsPaused();
```

### Signals

- `GamePaused(bool isPaused)` - Emitted when pause state changes
- `ResumeRequested()` - Resume button pressed
- `RestartRequested()` - Restart button pressed
- `MainMenuRequested()` - Main menu button pressed
- `SettingsRequested()` - Settings button pressed

---

## Inventory Grid UI

**File:** `scripts/InventoryUI.cs`

### Features

- Grid-based inventory display (5x4 by default)
- Item rarity color coding
- Equip/drop items
- Item details panel
- 'I' key to toggle

### Setup

1. Add `InventoryUI` as a `CanvasLayer` node
2. Ensure `InventorySystem` is autoloaded as `/root/InventorySystem`

### Usage

```csharp
// Get inventory UI
var inventoryUI = GetNode<InventoryUI>("InventoryUI");

// Toggle inventory
inventoryUI.ToggleInventory();

// Show/hide inventory
inventoryUI.SetInventoryVisible(true);
```

### Item Rarity Colors

- **Common:** Gray/White
- **Uncommon:** Green
- **Rare:** Blue
- **Legendary:** Orange

### Signals

- `ItemSlotClicked(int slotIndex)` - Slot clicked
- `ItemEquipped(string itemName)` - Item equipped
- `ItemDropped(int slotIndex)` - Item dropped

---

## Particle Effects System

**File:** `scripts/ParticleEffectManager.cs`

### Features

- Pooled particle system (10 instances per effect)
- 16 predefined particle effects
- Color and scale customization
- Auto-cleanup after lifetime

### Setup

Add `ParticleEffectManager` as an autoload singleton or add to main scene.

### Usage

```csharp
// Get particle manager
var particles = GetNode<ParticleEffectManager>("/root/ParticleEffectManager");

// Spawn effect
particles.SpawnEffect("hit_impact", position);

// Spawn with scale
particles.SpawnEffect("explosion", position, scale: 2.0f);

// Spawn with color
particles.SpawnEffect("heal", position, color: Colors.Green);
```

### Available Effects

**Combat:**
- `hit_impact` - Yellow impact sparks
- `blood_splatter` - Red blood particles
- `death_explosion` - Orange explosion
- `explosion` - Large orange explosion
- `shield_break` - Blue shield fragments

**Magic:**
- `magic_cast` - Purple magic particles
- `teleport` - Cyan teleport flash
- `heal` - Green upward floating particles
- `level_up` - Yellow upward particles
- `power_up` - Red power aura

**Environmental:**
- `smoke` - Gray smoke clouds
- `sparkles` - Yellow sparkles
- `fire` - Orange/red fire
- `poison_cloud` - Green poison fog
- `lightning` - White/blue lightning
- `dash_trail` - Blue motion trail

---

## Dialogue System

**File:** `scripts/DialogueSystem.cs`

### Features

- Typewriter text effect
- Character portraits
- Branching dialogue choices
- Skip typewriter with spacebar

### Setup

Add `DialogueSystem` as a `CanvasLayer` node.

### Usage

```csharp
// Get dialogue system
var dialogue = GetNode<DialogueSystem>("DialogueSystem");

// Create dialogue data
var dialogueData = new DialogueData
{
    Id = "tavern_keeper_1",
    Lines = new List<DialogueLine>
    {
        new DialogueLine
        {
            CharacterName = "Tavern Keeper",
            Text = "Welcome to the Rusty Mug! What can I get you?",
            Choices = new List<DialogueChoice>
            {
                new DialogueChoice { Text = "A drink, please.", NextDialogueId = "order_drink" },
                new DialogueChoice { Text = "Just information.", NextDialogueId = "ask_info" }
            }
        }
    }
};

// Start dialogue
dialogue.StartDialogue(dialogueData);

// End dialogue
dialogue.EndDialogue();
```

### Signals

- `DialogueStarted(string dialogueId)` - Dialogue started
- `DialogueEnded()` - Dialogue ended
- `ChoiceSelected(int choiceIndex, string choiceText)` - Choice selected

---

## Minimap System

**File:** `scripts/MinimapSystem.cs`

### Features

- Top-down minimap view
- Player, enemy, boss, NPC, and objective markers
- Configurable zoom and size
- Tab key to toggle

### Setup

Add `MinimapSystem` as a `CanvasLayer` node.

### Usage

```csharp
// Get minimap
var minimap = GetNode<MinimapSystem>("MinimapSystem");

// Register entities
minimap.RegisterEntity(playerNode, MinimapMarkerType.Player);
minimap.RegisterEntity(enemyNode, MinimapMarkerType.Enemy);
minimap.RegisterEntity(bossNode, MinimapMarkerType.Boss);
minimap.RegisterEntity(npcNode, MinimapMarkerType.NPC);

// Auto-register all enemies
minimap.AutoRegisterEnemies();

// Toggle minimap
minimap.ToggleMinimap();

// Set zoom
minimap.SetZoom(3.0f);
```

### Marker Types and Colors

- **Player:** Green (8px)
- **Enemy:** Red (6px)
- **Boss:** Orange (10px)
- **Objective:** Yellow (7px)
- **NPC:** Blue (5px)

---

## Enemy Types

### 1. **Troll (Basic)**
**File:** `scripts/EnemyController.cs`

- **HP:** 50
- **Attack:** 8
- **Speed:** 100
- **Behavior:** Basic patrol, chase, attack

### 2. **Goblin**
**File:** `scripts/GoblinEnemy.cs`

- **HP:** 30
- **Attack:** 6
- **Speed:** 130
- **Special:** Flees at 30% HP, 20% panic chance when hit

### 3. **Ogre**
**File:** `scripts/OgreEnemy.cs`

- **HP:** 120
- **Attack:** 15
- **Speed:** 70
- **Special:** Ground Slam AoE (100 range, 20 damage)

### 4. **Archer**
**File:** `scripts/ArcherEnemy.cs`

- **HP:** 40
- **Attack:** 10
- **Speed:** 110
- **Special:** Ranged attacks, maintains 200 unit distance

### 5. **Assassin**
**File:** `scripts/AssassinEnemy.cs`

- **HP:** 35
- **Attack:** 20
- **Speed:** 150
- **Special:** Teleport behind player, backstab (2x damage), invisibility

### 6. **Berserker**
**File:** `scripts/BerserkerEnemy.cs`

- **HP:** 80
- **Attack:** 12 (increases with damage)
- **Speed:** 100
- **Special:** Enrage at 40% HP (1.5x damage, 1.3x speed)

### 7. **Necromancer Minion**
**File:** `scripts/NecromancerMinion.cs`

- **HP:** 20
- **Attack:** 5
- **Speed:** 80
- **Special:** 30s lifetime, can be resurrected

---

## Weapons

**File:** `data/weapons.json`

### Total Weapons: 45

#### Common (15 weapons)
- Stuhlbein, Krug, Tischbein, Glasflasche, Hufeisen, Hammer
- Laute Stimme, Bartschlüssel, Fassdeckel, Eiseneimer
- Heugabel, Mistgabel, Besen, Sichel, Fackel, Eisenstange, Armbrustbolzen

#### Uncommon (16 weapons)
- Kaputte Türe, Kettenpeitsche, Kerze, Rostiges Schwert
- Sense, Öllampe, Pfanne, Axt, Spitzhacke, Geweih
- Knochen, Brandeisen, Fleischerhaken, Wurfstern, Peitsche

#### Rare (11 weapons)
- Morgenstern, Bibel, Zweihänder, Rapier, Streitkolben
- Giftdolch, Dreschflegel, Zauberstab, Kriegshammer

#### Legendary (3 weapons)
- Kleines Kind am Fuß gepackt, Schrottflinte, Grimoire der Schatten

### Weapon Properties

**Damage Stats:**
- Str (Strength), Dex (Dexterity), Wis (Wisdom), Chr (Charisma)

**Elements:**
- Fire, Holy, Arcane, Shadow, Poison

**Special Properties:**
- Stun, Critical Chance, Armor Pierce, Knockback
- Bleed, Burn, Poison DoT, Fear, Screaming Debuff
- Pull Effect, Scatter Damage, Mana Drain

---

## Integration Guide

### Adding to Main Scene

```gdscript
# In your main scene tree:
Main (Node2D)
├── AudioManager (autoload)
├── PauseMenu (CanvasLayer)
├── InventoryUI (CanvasLayer)
├── DialogueSystem (CanvasLayer)
├── MinimapSystem (CanvasLayer)
└── ParticleEffectManager (Node2D or autoload)
```

### Input Actions Required

Add these to Project Settings → Input Map:

- `ui_cancel` - ESC (Pause menu)
- `inventory` - I key (Inventory)
- `ui_focus_next` - Tab (Minimap toggle)
- `ui_accept` - Space/Enter (Dialogue continue)
- `interact` - E key (Interactions)

### Audio Bus Setup

Project Settings → Audio → Buses:

1. Master
   - Music
   - SFX

---

## Tips and Best Practices

### Audio

- Place music files in `res://audio/music/` as `.ogg` files
- Place SFX in `res://audio/sfx/` as `.wav` files
- Keep SFX short (< 1 second) for better performance
- Use pitch variation for repeated sounds

### Particles

- Spawn particles sparingly to avoid performance issues
- Use the pool system (auto-managed)
- Customize colors for different effect contexts

### Inventory

- Connect to `InventorySystem` signals for item changes
- Update UI when items are added/removed

### Dialogue

- Create modular dialogue chunks
- Use branching for player choices
- Keep lines under 200 characters for readability

### Minimap

- Register enemies when spawned
- Unregister when destroyed
- Use groups ("enemies", "npcs") for auto-registration

---

## Future Enhancements

- [ ] Add achievement system
- [ ] Implement crafting system
- [ ] Add quest journal UI
- [ ] Create save slots UI
- [ ] Add gamepad support
- [ ] Implement localization
- [ ] Add accessibility options
- [ ] Create tutorial system

---

**Last Updated:** 2025-11-17
**Version:** 1.0
