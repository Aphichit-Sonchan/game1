# Survival Arena MiniGame

## 🎮 Game Description
This is a survival-based mini-game where players must avoid hazardous sectors in a circular arena. The game consists of multiple rounds where sectors act as safe zones or danger zones. Players must identify safe sectors and move to them before the timer runs out.

## 🕹️ How to Play
- **Objective:** Survive all 5 rounds without getting eliminated.
- **Controls:** Use **WASD** or **Arrow Keys** to move.
- **Rules:**
  1. The game starts with a countdown.
  2. Each round, **2 random sectors** are marked as **Hazards** (Flashing Red).
  3. You have **5 seconds** to move out of the hazard zones.
  4. If you are on a hazard sector when the timer hits 0, you are **Eliminated**.
  5. The game ends after 5 rounds or if you are eliminated.

---

## 📅 Development History & Features
List of implemented features and mechanics currently in the project:

### ✅ Core Gameplay
- **Game Loop:** A structured loop managing Pre-Game, Rounds, and End-Game states.
- **Round System:** Currently set to **5 Rounds** per game.
- **Hazard Logic:** Randomly selects 2 sectors each round to become hazardous.
- **Player Elimination:** accurately detects if a player is standing on a hazard sector using angle-based math.

### ✅ Visuals & Feedback
- **Sector Coloring:**
  - **Normal:** Dark Blue-Gray
  - **Hazard:** Flashing Red (Pulsing effect)
- **UI HUD:**
  - **Countdown:** 3-2-1-GO! sequence.
  - **Round Counter:** Displays current round progress (e.g., "Round 1/5").
  - **Hazard Timer:** Countdown (5s) warning before hazard activation.

### ✅ Technical Implementation
- **Singleton Managers:** `MiniGameManager` and `UIManager` for easy global access.
- **Modular Design:**
  - `ArenaController`: Handles the physical arena and sector collection.
  - `SectorController`: Manages individual sector visuals and player detection.
  - `PlayerController`: Handles physics-based movement and inputs.

## 📂 Project Structure
- **Assets/Scripts/MiniGame/**
  - `MiniGameManager.cs`: The brain of the game, handling the state machine.
  - `ArenaController.cs`: Manages the collection of sectors.
  - `SectorController.cs`: Helper script on each sector piece.
  - `PlayerController.cs`: Player movement logic.
  - `UIManager.cs`: Updates the canvas text elements.

---

# Rotating Platform MiniGame

## 🎮 Game Description
A multiplayer elimination game where 6 players compete on a rotating platform divided by danger zones. The platform randomly rotates, and players must move away from the divider lines before time runs out or be eliminated.

## 🕹️ How to Play
- **Objective:** Be the last player standing across multiple rounds.
- **Controls:** **Click on your player** to move to a new position around the platform.
- **Rules:**
  1. The game starts when you click "เริ่มเกม" (Start Game).
  2. The platform rotates randomly by 90°, 180°, 270°, or 360°.
  3. You have **5 seconds** to click your player and move to a safe position.
  4. Players standing on or near the **divider lines** (danger zones) when time runs out are **eliminated**.
  5. The last player remaining wins!

---

## 📅 Development History & Features

### ✅ Completed (Automated Setup)
- **Platform Structure:**
  - Main platform mesh (blue cylinder)
  - Decorative outer ring (gold cylinder)
  - Center circle (red cylinder)
  - 4 divider barriers in cross pattern (orange cubes)
  
- **Materials System:**
  - 11 materials created with proper colors
  - 6 player colors (red, blue, green, yellow, purple, orange)
  - Platform materials (blue, gold, red, orange)
  - Transparent selection ring material

- **Player Prefab:**
  - Ready-to-use player prefab with sphere body
  - Selection ring for visual feedback
  - Collider for mouse click detection
  - Saved at `Assets/Prefabs/RotatingPlatform/Player.prefab`

- **Game Manager:**
  - `RotatingPlatformManager` GameObject created
  - Ready for `PlatformRotationManager` component

### ⏳ Pending (Manual Setup Required)
See [Setup Guide](https://github.com/yourusername/yourrepo) for detailed instructions:
- Add `PlatformRotationManager` component to manager
- Create UI elements (timer, status text, buttons)
- Use `PlayerSpawner` to generate 6 players
- Configure camera position and lighting
- Link all component references

### 🎯 Core Features (Scripts Already Implemented)
- **Smart Rotation System:** Random rotation (90°/180°/270°/360°)
- **Circular Movement:** Players move along the platform's edge
- **Danger Zone Detection:** Angle-based math to detect divider collisions
- **Smooth Animations:** Platform rotation and player fall effects
- **Visual Feedback:** Selection ring on hover, color-coded players
- **Win Detection:** Automatic winner announcement

---

## 📂 Project Structure

### Rotating Platform Game Files
- **Assets/Materials/RotatingPlatform/**
  - Platform materials: `Platform_Blue.mat`, `Platform_Gold.mat`, `Center_Red.mat`, `Divider_Orange.mat`
  - Player materials: `Player_Red.mat`, `Player_Blue.mat`, `Player_Green.mat`, `Player_Yellow.mat`, `Player_Purple.mat`, `Player_Orange.mat`
  - Special: `SelectionRing_Transparent.mat`

- **Assets/Prefabs/RotatingPlatform/**
  - `Player.prefab`: Reusable player with body and selection ring

- **Assets/Scripts/MiniGame/**
  - `PlatformRotationManager.cs`: Game controller with rotation logic
  - `PlayerController.cs` (PlayerController1.cs): Player movement and elimination
  - `PlayerSpawner.cs`: Editor utility for spawning 6 players

- **Assets/Scenes/**
  - `MiniGame.unity`: Contains both Survival Arena and Rotating Platform setups

### Survival Arena Game Files
- **Assets/Scripts/MiniGame/**
  - `MiniGameManager.cs`: The brain of the game, handling the state machine.
  - `ArenaController.cs`: Manages the collection of sectors.
  - `SectorController.cs`: Helper script on each sector piece.
  - `PlayerController.cs`: Player movement logic (arena version).
  - `UIManager.cs`: Updates the canvas text elements.

---

## 🎨 Visual Design

### Rotating Platform Color Scheme
- **Platform:** Dark blue (#2a3f5f) with gold accents (#f4d03f)
- **Danger Zones:** Orange dividers (#f39c12)
- **Center:** Red circle (#e74c3c)
- **Players:** Red, Blue, Green, Yellow, Purple, Orange

### Scene Layout
- Platform centered at world origin (0, 0, 0)
- Camera positioned at (0, 15, -8) with 60° downward angle
- Directional light + Point light for optimal visibility

---

## 🚀 Quick Start (Rotating Platform)

### Prerequisites
1. Unity 2020.3 or later
2. TextMeshPro package imported

### Setup Steps
1. Open `Assets/Scenes/MiniGame.unity`
2. Follow the [Final Setup Guide](path/to/guide) to:
   - Add components to GameObjects
   - Create UI elements
   - Spawn 6 players using PlayerSpawner
   - Link references in Inspector
3. Press Play and click "เริ่มเกม"!

---

## 📊 Game Statistics

### Rotating Platform Game
- **Players:** 6 (AI or manual control)
- **Rounds:** Until 1 winner remains
- **Round Duration:** ~8-10 seconds (rotation + movement phase)
- **Danger Zones:** 4 dividers at 0°, 90°, 180°, 270°
- **Danger Zone Width:** ±22.5° from each divider

### Survival Arena Game
- **Rounds:** 5
- **Hazard Sectors:** 2 random sectors per round
- **Warning Time:** 5 seconds
- **Sectors:** Multiple divided sections

---

## 🔧 Customization

### Rotating Platform Tweaks
Edit values in `PlatformRotationManager`:
- `rotationDuration`: Speed of platform rotation (default: 2 seconds)
- `countdownTime`: Movement phase duration (default: 5 seconds)
- `checkDelay`: Delay before checking eliminations (default: 1.5 seconds)

Edit values in `PlayerController`:
- `moveSpeed`: Player movement speed (default: 5)
- `distanceFromCenter`: Radius from platform center (default: 3.5)
- `fallDuration`: Elimination animation time (default: 0.8 seconds)

---

## 📝 Notes

- Both minigames exist in the same scene (`MiniGame.unity`)
- Scripts are shared in `Assets/Scripts/MiniGame/`
- Materials are organized by game type in subfolders
- `PlayerSpawner` is an Editor tool (only works in Unity Editor)

---

## 🔨 Code Quality Improvements

### Recent Fixes (2026-02-09)

#### Compiler Warning Fixes
Fixed all Unity compiler warnings to ensure clean compilation:

1. **Deprecated API Updates:**
   - `FindObjectOfType<T>()` → `FindFirstObjectByType<T>()` in `MiniGameManager.cs`
   - `FindObjectsOfType<T>()` → `FindObjectsByType<T>(FindObjectsSortMode.None)` in `ArenaController.cs`
   - These changes use the newer, more explicit Unity API that allows better performance control

2. **Removed Unused Fields:**
   - Removed `arenaRadius` from `ArenaController.cs` (was declared but never used)
   - Removed `canPlayersMove` from `PlatformRotationManager.cs` (redundant with direct `SetCanMove()` calls)
   - Cleaned up all references to removed fields

3. **Script Restoration:**
   - Uncommented `UIManager.cs`, `MiniGameManager.cs`, and `PlayerController.cs` to fix "missing script" references
   - All Survival Arena game scripts are now active and functional

**Result:** Zero compiler warnings ✅ - Clean, maintainable codebase

---

*Generated by Antigravity*

