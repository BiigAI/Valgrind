# Valgrind

> **Valgrind is a lightweight mod designed to scale death penalty skill loss dynamically based on character progression.**

### About the Name: *Valgrind*
In Norse mythology, **Valgrind** (*"The Gate of the Slain"*) is the sacred outer gate of Valhalla through which fallen warriors pass into the afterlife. In this mod, *Valgrind* acts as the gatekeeper of your mortality—fairly weighing and scaling your skill loss upon death rather than inflicting an indiscriminate flat penalty.

---

## Features
- **Progressive Skill Protection**: Scales death penalty skill loss based on progression, protecting high-level masteries while keeping early-game deaths consequential.
- **Multiple Calculation Modes**: Supports Tiered Brackets, Continuous Curve scaling, and independent Per-Skill tier evaluation.
- **Server-Synchronized Rules**: Synchronizes and locks server rules across all connected clients using Jotunn.

---

### Installation Type
- **Location:** Must be installed on both the Server and the Client.
- **Enforcement:** Client versions must match the server version.

### Manual Install
1. Ensure BepInEx and Jotunn are installed.
2. Extract the downloaded `.zip` archive.
3. Copy `Valgrind.dll` into your `Valheim/BepInEx/plugins/` folder.
4. Launch the game once to generate the default configuration file.

---

## Configuration
The configuration file is automatically created at `BepInEx/config/com.bigai.valgrind.cfg` after running the game once.

| Section | Setting | Default | Description |
| :--- | :--- | :--- | :--- |
| `1 - General` | `CalculationMode` | `TieredBrackets` | Method used to calculate dynamic skill loss (`TieredBrackets`, `ContinuousCurve`, `PerSkill`). |
| `1 - General` | `UseTopNSkillsOnly` | `false` | If true, average skill level is computed using only the player's top N highest skills. |
| `1 - General` | `TopNSkillsCount` | `5` | Number of top skills to factor into the average when `UseTopNSkillsOnly` is enabled. |
| `1 - General` | `ResetAccumulatorOnDeath` | `true` | If true, partial XP progress toward the next level is wiped on death (vanilla behavior). |
| `1 - General` | `EnableDebugLogging` | `false` | Enables verbose calculation logging in the BepInEx console. |
| `2 - Tiered Brackets` | `EarlyGameLossPercent` | `8.0` | Skill loss % for skill/average levels < 25. |
| `2 - Tiered Brackets` | `MidGameLossPercent` | `5.0` | Skill loss % for skill/average levels between 25 and 50. |
| `2 - Tiered Brackets` | `LateGameLossPercent` | `2.5` | Skill loss % for skill/average levels between 50 and 75. |
| `2 - Tiered Brackets` | `EndgameLossPercent` | `1.0` | Skill loss % for skill/average levels > 75. |
| `3 - Continuous Curve` | `CurveMaxLossPercent` | `8.0` | Maximum skill loss % applied at skill level 0. |
| `3 - Continuous Curve` | `CurveMinLossPercent` | `1.0` | Minimum skill loss % applied at skill level 100. |

---

## Controls & Commands
- **Keybinds:** None.
- **Admin Commands:** None.

---

## Compatibility & Safe Removal
- **Multiplayer:** Must be installed on both server and clients with Jotunn.
- **Save Integrity:** Safe to add or remove mid-playthrough. Valgrind modifies only the skill loss calculation upon death.

### AI Disclosure 

I made this mod using AI. Most of the code in this mod was AI generated. If you have an issue with this, I completely understand and urge you to not use this mod. This mod ("Valgrind") is meant as a lightweight mod for small servers that don't need all the bells and whistles of a more complex mod.
