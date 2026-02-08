# Simple Call

MelonLoader mod for **Schedule I** that lets you summon dealers to your location with one click. Dealers pathfind to you, wait a configurable time, and return to their routine. Supports indoor fallback and dealer messages.

## Features

- One-button summon from the Dealer Management phone app
- Pathfinding-based movement (walks or runs)
- Indoor fallback: dealer waits at your last outdoor position if you go inside; optional max wait timeout
- Configurable meet delay; early departure when you interact with the dealer
- Dealer messages via in-game message app (On my way, At door, Leaving)
- Block re-calling the same dealer while they're en route
- Multiple dealers can be in transit at once

## Requirements

- [MelonLoader](https://melonwiki.xyz/)

## Installation

1. Install [MelonLoader](https://melonwiki.xyz/)
2. Place `SimpleCall.dll` in the game's `Mods/` folder
3. Launch the game

The "Call Dealer" button appears in the Dealer Management app.

## Settings

### Basic
| Setting | Default | Range | Description |
|---------|---------|-------|-------------|
| Meet Delay | 10s | 1–120s | Time dealer stays at your location before returning |
| Allow Dealer Run | ✓ | - | Dealer runs when on, walks when off |
| No Signal Notification | ✓ | - | Notification when you're indoors (no pathfinding) |
| Dealer Messages | ✓ | - | Dealer sends status messages via message app |

### Advanced
| Setting | Default | Range | Description |
|---------|---------|-------|-------------|
| Max Fallback Attempts | 8 | 1–20 | Indoor fallback: how many past positions to try |
| Repath Interval | 2.5s | 0.25–10s | Path recalc frequency; lower = responsive, may lag |
| Max Wait At Door | 120s | 30–600s | Max time dealer waits outside when you're indoors |

### Debug
| Setting | Default | Description |
|---------|---------|-------------|
| Debug Logging | ✗ | Verbose logs for bug reports |

## Project Structure

```
SimpleCall/
├── Core.cs              # MelonMod entry, scene handling
├── ModSettings.cs       # MelonPreferences, value clamping
├── Controllers/         # DealerCallController
├── Models/              # DealerCallService, DealerMessages
├── Views/               # CallDealerButtonView
└── Utils/               # SpriteLoader
```

## Links

- [GitHub](https://github.com/tiagovitorino97/SimpleCall)
- [Nexus Mods](https://www.nexusmods.com/schedule1/mods/996)

## Version

**1.2.0**: Rewrite with MVC structure, pathfinding, indoor fallback, dealer messages, and full settings.
