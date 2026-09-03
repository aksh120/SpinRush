# SpinRush — Royal VIP Arcade Slot Machine

> A high-roller, Indian Rupee (INR) retro-arcade slot machine game crafted in **Unity 2022.3 LTS** featuring continuous physics-based reel strips, an interactive mechanical lever, mathematical symbol framing, and authentic casino game feel.

---

## 1. Game Overview

**SpinRush** is a 3-reel, single-payline video slot machine built around an authentic arcade casino economy. Players pull a realistic mechanical arcade lever (or use keyboard shortcuts) to spin the reels, matching iconic symbols along the central horizontal payline to claim tiered payouts up to the **100x Kohinoor Royal Dhamaka Jackpot**.

### Key Economy Specifications:
* **Starting Bankroll**: 5,00,000 INR (or 5,000 INR Arcade Challenge)
* **Default VIP Bet**: 250 INR
* **VIP Bet Ladder**: 100, 250, 500, 1,000, 2,500, 5,000 INR
* **Top Payout**: 5,00,000 INR (100x at max bet)
* **Hit Frequency**: Authentic 20% win distribution with 45% ragebait near-miss teases

---

## 2. Controls & Arcade Shortcuts

The game features an arcade shortcuts panel docked on the left side of the cabinet:

| Control | Action | Details |
|---|---|---|
| **Mouse Click & Drag** | **Pull Lever** | Drag the lever handle downward in real-time with physical 3D perspective tracking and ratchet audio. |
| **Mouse Click** | **Quick Lever Pull** | Click directly on the lever handle for a smooth weighted pull and damped spring release. |
| **`[SPACE]` / `[ENTER]`** | **Spin / Pull** | Instant keyboard shortcut that physically animates the lever arm and starts the reels. |
| **`[LEFT]` / `[DOWN]`** | **Decrease Bet** | Step down through the VIP Bet ladder (min 100 INR). |
| **`[RIGHT]` / `[UP]`** | **Increase Bet** | Step up through the VIP Bet ladder (max 5,000 INR). |
| **`[T]`** | **Turbo Mode** | Toggles 2x high-speed spin execution and instant stop reels. |
| **`[A]`** | **Auto-Spin** | Starts a continuous 10-spin automated session. |
| **`[H]`** | **How to Play** | Reopens the interactive spotlight tutorial anytime. |

---

## 3. Winning Combinations & Paytable

Wins are awarded when all 3 slots along the central horizontal payline match, or when substituted by the **Star Wildcard** symbol:

| Symbol | ID | Multiplier | Payout at Default 250 INR Bet | Description |
|:---:|:---:|:---:|:---:|---|
| DIAMOND | `DIAMOND` | **100x** | **25,000 INR** | **Kohinoor Jackpot** (Special Fanfare & Coin Shower) |
| SEVEN | `SEVEN` | **20x** | **5,000 INR** | Triple Lucky Sevens |
| BELL | `BELL` | **10x** | **2,500 INR** | Golden Liberty Bells |
| BAR | `BAR` | **5x** | **1,250 INR** | Classic Triple Bar |
| CHERRY | `CHERRY` | **3x** | **750 INR** | Double Cherries |
| WILD | `WILD` | — | — | **Star Wildcard**: Substitutes for any symbol on the payline |

---

## 4. Key Gameplay Features & Additions

### A. Royal Fever Frenzy (Bonus Round)
* A procedural neon progress gauge mounted directly above the reels.
* Charges +10% per spin, +15% per win, and +25% on heartbreaking near-misses.
* At 100%, triggers **5 Free Spins at a 3x Global Win Multiplier** with radiant rainbow particles and celebratory lighting.

### B. Double-or-Nothing Gamble Mini-Game
* Following any win, the player can choose between **COLLECT** or **DOUBLE (2X)**.
* Selecting Double launches a 50/50 card flip mini-game (Red vs Black) to double the win or risk losing it.

### C. Arcade Token Challenge & Game Over Leaderboard
* When bankroll falls below the minimum bet (100 INR), the machine locks into the **Game Over Screen**.
* Displays **Total Spins Survived**, **Highest Single Win**, and **Final Score (PTS)**.
* Features a persistent **Top 5 All-Time High Score Leaderboard** saved via `PlayerPrefs`.
* An emerald **INSERT COIN / PLAY AGAIN** arcade button resets bankroll back to starting credits for a new run.

### D. Dynamic Floating Delta & Rolling Balance HUD
* **Floating Delta Labels**: Spawns a floating red label (e.g. `-250 INR`) on bets, or an emerald green label (e.g. `+5,000 INR`) on wins that rises upward and fades out.
* **Rolling Number Counter**: Rapidly rolls balance numbers using cubic ease-out interpolation instead of abrupt text changes.
* **Punch Bounce Animation**: Performs an elastic pop-in / pop-out punch scale bounce (`1.0 -> 1.30 -> 1.0`) when the final balance lands.

### E. Psychological Ragebait Near-Miss Teases
* 45% of losing spins generate high-tension near-miss combinations (Double 7s, Double Bells, Split matches).
* **Reel 3 Suspense Delay**: When reels 1 and 2 land matching high-value symbols, Reel 3 slows down with an extra +0.85s delay accompanied by an audio tension riser.
* On missing the third symbol, the middle HUD flashes **"SO CLOSE!"** alongside a crowd sigh audio cue.

### F. Seamless Infinite Reel Strip with Wrap Buffers
* 20-symbol strip with wrap-around clone buffer symbols at `-100px`, `-200px`, `+2000px`, and `+2100px`.
* Reel target calculations select interior indices, ensuring the top row, center payline, and bottom row are always completely filled with symbols at every stop.

### G. Cabinet Marquee Crown ("SPINRUSH")
* Centered directly above the middle reel on the cabinet crown ($380x56px$).
* Crafted with obsidian glass, radiant gold neon bevels, and animated neon breathing shimmer.

---

## 5. Technical Architecture

### Clean Modular Architecture
SpinRush follows strict separation of concerns with decoupled subsystems:
```
Assets/
├── Scripts/
│   ├── Core/           # Enums, GameConstants, RandomNumberGenerator
│   ├── Gameplay/       # SlotMachineController, SlotReel, SlotSymbol, LeverController,
│   │                   # WalletManager, WinEvaluator, FeverModeController, SymbolDatabase
│   ├── UI/             # MiddleBoxHUD, WinPopupController, ShortcutsPanel,
│   │                   # GambleMiniGameController, GameOverModalController, CabinetMarqueeHeader
│   ├── Audio/          # AudioController (PCM Synthesizer & Master Audio)
│   ├── Effects/        # WinEffectsPresenter (Camera Shake & Particle Systems)
│   └── Editor/         # SceneSetupEditor, AutoSpriteSlicer, BuildScript
├── Prefabs/            # 20-Symbol Continuous ReelPrefab
├── Sounds/             # Master 44.1kHz WAV sound clips
├── Data/               # ScriptableObject symbol databases
└── Scenes/             # MainGameScene.unity
```

### Mathematical Pixel Alignment
The slot machine viewport coordinates align with the cabinet texture:
* **Reel Column 1**: Center = -130px relative to viewport center
* **Reel Column 2**: Center = 0px (middle reel)
* **Reel Column 3**: Center = +130px relative to viewport center
* Individual `RectMask2D` clipping per column eliminates symbol bleeding across cabinet dividers.

---

## 6. How to Run

### Option A: Running in Unity Editor (Recommended)
1. Open the project in **Unity 2022.3 LTS** (or compatible Unity 2022+ version).
2. Open the scene: `Assets/Scenes/MainGameScene.unity`.
3. Click the **Play** button at the top of the editor.
4. Interact using the mouse (click or drag the Lever) or keyboard shortcuts (`[SPACE]`, `[ENTER]`, `[T]`, `[A]`, Arrow keys).

### Option B: Running the WebGL Player
A pre-compiled production WebGL build is included in `Build/WebGL/`:
1. Open a terminal in the repository root directory and run:
   ```bash
   npx -y http-server "Build/WebGL" -p 8080 -c-1
   ```
2. Navigate in any browser to:
   ```
   http://localhost:8080/
   ```

---

## 7. Repository Best Practices

* **`Assets/`**, **`Packages/`**, and **`ProjectSettings/`** are fully committed to ensure immediate, zero-configuration setup for anyone cloning the repository.
* **`Library/`** is excluded from Git according to standard Unity practices, as it consists of auto-generated local caches that Unity recreates automatically on first launch.
* **`Build/WebGL/`** is tracked so the game can be hosted or played directly without requiring Unity Editor installation.
