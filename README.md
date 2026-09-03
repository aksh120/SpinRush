# SpinRush — Royal VIP Arcade Slot Machine

> A high-roller, Indian Rupee (INR) retro-arcade slot machine game crafted in **Unity 2022.3 LTS** with continuous physics-based reel strips, an interactive mechanical lever, mathematical symbol framing, and a dynamic spotlight onboarding tutorial.

---

## 1. Game Overview

**SpinRush** is a 3-reel, single-payline video slot machine built around an Indian Rupee VIP casino economy. Players pull a realistic mechanical arcade lever to spin the reels, matching iconic symbols along the central horizontal payline to claim tiered payouts up to the **100x Kohinoor Royal Dhamaka Jackpot**.

### Key Economy Specifications:
* **Starting Balance**: 1,00,000 INR
* **Default VIP Bet**: 500 INR
* **VIP Bet Ladder**: 100, 250, 500, 1,000, 2,500, 5,000 INR
* **Top Payout**: 5,00,000 INR (100x at max bet)
* **Target RTP**: ~94.8% mathematically simulated and verified across 100,000 spins

---

## 2. Controls & Shortcuts

The game features an arcade controls guide docked on the left side of the cabinet:

| Control | Action | Details |
|---|---|---|
| **Mouse Click & Drag** | **Pull Lever** | Drag the lever handle downward in real-time with physical 3D perspective tracking and ratchet audio. |
| **Mouse Click** | **Quick Lever Pull** | Click directly on the lever handle for a smooth weighted pull and damped spring release. |
| **`[SPACE]` / `[ENTER]`** | **Spin / Pull** | Instant keyboard shortcut to trigger the reels. |
| **`[LEFT]` / `[DOWN]`** | **Decrease Bet** | Step down through the VIP Bet ladder (min 100 INR). |
| **`[RIGHT]` / `[UP]`** | **Increase Bet** | Step up through the VIP Bet ladder (max 5,000 INR). |
| **`[H]`** | **How to Play** | Reopens the interactive spotlight tutorial anytime. |

---

## 3. Winning Combinations & Paytable

Wins are awarded when all 3 slots along the central payline match, or when completed by the **Star Wildcard** symbol:

| Symbol | ID | Multiplier | Payout at Default 500 INR Bet | Description |
|:---:|:---:|:---:|:---:|---|
| DIAMOND | `DIAMOND` | **100x** | **50,000 INR** | **Kohinoor Jackpot** (Special Fanfare & Coin Shower) |
| SEVEN | `SEVEN` | **20x** | **10,000 INR** | Triple Lucky Sevens |
| BELL | `BELL` | **10x** | **5,000 INR** | Golden Liberty Bells |
| BAR | `BAR` | **5x** | **2,500 INR** | Classic Triple Bar |
| CHERRY | `CHERRY` | **3x** | **1,500 INR** | Double Cherries |
| WILD | `WILD` | — | — | **Star Wildcard**: Substitutes for any symbol on the payline |

---

## 4. Bonus Features & Creative Additions

Beyond the core slot mechanics, SpinRush incorporates several creative additions:

1. **Realistic Mechanical Lever with Damped Spring Physics**:
   * Isolated lever arm rotating around its physical base hinge bracket ($X = 310.5\text{px}, Y = -253\text{px}$).
   * Real-time mouse drag tracking with 3D perspective foreshortening.
   * Damped harmonic spring-back oscillation ($P(t) = 1 - e^{-6t} \cdot \cos(3.5\pi t)$) featuring an organic overshoot bounce.
2. **Interactive Spotlight Onboarding Tutorial**:
   * Automatically presents on first run (persisted via `PlayerPrefs`).
   * Features a dynamic glowing neon spotlight border that physically slides and resizes to encircle the active element being explained (Reels -> HUD -> Lever -> Jackpot).
   * Includes step indicators, **Skip**, **Next**, and a **"Don't show again"** preference toggle.
3. **Star Wildcard Substitution**:
   * Evaluates wildcard combinations (e.g. `BELL + WILD + BELL` -> Triple Bells).
4. **"Paise Khatam" Low-Balance Auto-Recovery Dialog**:
   * Modal dialog that triggers if credits fall below the minimum bet (100 INR), offering a quick VIP reload back to 1,00,000 INR with celebratory SFX.
5. **Procedural 60-FPS Audio Engine**:
   * Dual-mode audio system: synthesizes real-time PCM waveforms for clicks, ratchets, spin hums, stop clacks, and victory arpeggios, paired with master audio assets in `Assets/Sounds/`.
6. **Screen Micro-Shake & Golden Particle Coin Celebration**:
   * High-tier payouts trigger physical camera rumble and dual particle coin bursts.

---

## 5. Architecture & Thought Process

### Clean Modular OOP Architecture
SpinRush is structured into clean namespaces with strong separation of concerns:
```
Assets/
├── Scripts/
│   ├── Core/           # Enums, GameConstants, GameState
│   ├── Gameplay/       # SlotMachineController, SlotReel, SlotSymbol, LeverController,
│   │                   # SymbolDatabase (ScriptableObject), RandomNumberGenerator, PayoutEvaluator
│   ├── UI/             # MiddleBoxHUD, WinPopupController, ShortcutsPanel, TutorialManager
│   ├── Audio/          # AudioController (PCM Synthesizer & Master Audio)
│   ├── Effects/        # WinEffectsPresenter (Camera Shake & Particle System)
│   └── Editor/         # SceneSetupEditor, AutoSpriteSlicer, BuildScript
├── Prefabs/            # 20-Symbol Continuous ReelPrefab
├── Sounds/             # Master 44.1kHz WAV sound clips
├── Data/               # ScriptableObject databases
└── Scenes/             # MainGameScene.unity
```

### Mathematical Pixel Geometry & Alignment
Analysis of the provided cabinet texture (`slot-machine4.png`) revealed 3 distinct cutouts separated by 22px vertical pillars:
* **Cutout 1**: $X = 229..336$ (Width $108\text{px}$, Center = $-130\text{px}$ relative to viewport)
* **Cutout 2**: $X = 359..466$ (Width $108\text{px}$, Center = $0\text{px}$)
* **Cutout 3**: $X = 489..596$ (Width $108\text{px}$, Center = $+130\text{px}$)
* Each reel column is sized to $104\text{px}$ with $78\times 78\text{px}$ centered symbol icons and individual column masking (`RectMask2D`), guaranteeing **zero symbol bleeding or overlap on the cabinet dividers**.

### 20-Symbol Continuous Strip Physics
Each reel strip is composed of 20 ordered symbol slots ($2,000\text{px}$ loop height) executing:
1. **Anticipation Wind-Up**: Upward pull ($+12\text{px}$) for $0.08\text{s}$.
2. **High-Speed Acceleration**: Ramp to $2,600\text{px/s}$ over $0.22\text{s}$ with seamless modulo looping.
3. **Deceleration & Snapping**: Cubic ease-out with an $18\text{px}$ elastic bounce snap.

---

## 6. Instructions to Run

### Option A: Running in Unity Editor (Recommended)
1. Open the project in **Unity 2022.3 LTS** (or compatible Unity 2022+ version).
2. Open the scene: `Assets/Scenes/MainGameScene.unity`.
3. Press the **Play** button at the top of the editor.
4. Interact using the mouse (click or drag the Lever) or keyboard (`Spacebar`, Arrow keys).

### Option B: Running the WebGL Build Locally
1. Ensure Node.js is installed on your machine.
2. Open a terminal in the project root directory and run:
   ```bash
   npx -y http-server "Build/WebGL" -p 8080 -c-1
   ```
3. Open your browser and navigate to:
   ```
   http://localhost:8080/
   ```

---

## 7. Automated Testing & Verification

The codebase includes headless simulation suites validating RNG distribution and payout integrity:
* `RNGSimulationTest.cs`: Validates uniformity and non-degeneracy across 100,000 rolls.
* `PayoutSimulationTest.cs`: Validates wildcard matching, jackpot detection, and balance accounting across 50,000 spins.
