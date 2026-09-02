# SpinRush Implementation Tasks

## 1. Project Setup & Asset Analysis
- [x] Create Unity LTS project using Unity 2022.3.62f2.
- [x] Configure base folder layout (`Assets/Scripts/`, `Prefabs/`, `Scenes/`, `Art/`, `Sounds/`, `UI/`, `Animations/`, `Build/WebGL/`).
- [x] Import all provided graphics (`slot-machine1..5.png`, `slot-symbol1..4.png`, `slot_machine_buttons-02..04.png`, `slot_machine_Middle_box.png`, `Yes_No_Btn.png`, `popup.png`, `bg_gradient.png`, `4tXlXs.gif`).
- [x] Analyze image geometry and layer transparency:
  - [x] `slot-machine4.png` window cutout bounds ($X: 232..593$, $Y: 250..451$).
  - [x] 3-Reel column centers at $X = 283$, $412$, $541$.
  - [x] `slot-machine5.png` glass frame and column divider lines ($X = 347, 477$).
  - [x] `slot_machine_buttons-02..04.png` 4-state vertical cells ($256\times 256$ each).
  - [x] `Yes_No_Btn.png` 2-column $\times$ 3-row button state layout.
  - [x] Lever arm states (`slot-machine2.png` up vs `slot-machine3.png` down).

## 2. Sprite Processing & Editor Tooling
- [x] Create `Editor/AutoSpriteSlicer.cs` to automate texture import settings and sprite slicing:
  - [x] Set all art assets to `TextureImporterType.Sprite`.
  - [x] Slice `slot_machine_buttons-02.png` into 4 states (Spin: Normal, Highlighted, Pressed, Disabled).
  - [x] Slice `slot_machine_buttons-03.png` into 4 states (Bet -: Normal, Highlighted, Pressed, Disabled).
  - [x] Slice `slot_machine_buttons-04.png` into 4 states (Bet +: Normal, Highlighted, Pressed, Disabled).
  - [x] Slice `Yes_No_Btn.png` into 6 states (Yes & No buttons: Normal, Hover, Pressed).
  - [x] Configure `slot-symbol1.png` through `slot-symbol4.png` as individual 2D sprites.
  - [x] Configure `slot-machine2.png` and `slot-machine3.png` as lever sprites.

## 3. Data Architecture & ScriptableObjects
- [x] Create `SymbolData.cs` ScriptableObject:
  - [x] Symbol ID (string/enum).
  - [x] Display Name (string).
  - [x] Icon (Sprite reference).
  - [x] Payout Multiplier (int).
  - [x] IsWild (bool).
- [x] Create `SymbolDatabase.cs` ScriptableObject containing:
  - [x] `SYM_01`: Lucky Seven (Red) — `slot-symbol1.png` ($50\times$).
  - [x] `SYM_02`: Golden Bell — `slot-symbol2.png` ($25\times$).
  - [x] `SYM_04`: Triple Bar (Blue) — `slot-symbol4.png` ($10\times$).
  - [x] `SYM_03`: Golden Star / Wild — `slot-symbol3.png` ($100\times$ 3-of-a-kind, or $2\times$ wild substitution).
- [x] Create scriptable asset instances in `Assets/Data/Symbols/`.

## 4. Reel Hierarchy & Cabinet Presentation
- [x] Create `SlotSymbol.cs` component for symbol instances:
  - [x] SpriteRenderer / Image component reference.
  - [x] SetSymbol(SymbolData) method.
  - [x] Glow/pulse highlight animation trigger.
- [x] Create `SlotReel.cs` reel controller component:
  - [x] Manage circular pool of visible and buffer symbols.
  - [x] Maintain vertical spacing matching window height ($200\text{px}$).
- [x] Create `ReelPrefab` with `RectMask2D` clipping boundary.
- [x] Construct the Slot Machine Canvas hierarchy:
  - [x] Background Image (`bg_gradient.png`).
  - [x] Machine Base (`slot-machine4.png`).
  - [x] Reels container positioned behind the machine window cutout.
  - [x] 3 Reel columns placed at $X = 283, 412, 541$.
  - [x] Glass Frame overlay (`slot-machine5.png`) in front of reels.
  - [x] Interactive Lever (`slot-machine2.png`/`3.png`).

## 5. RNG Engine & Game Flow State Machine
- [x] Create `RandomNumberGenerator.cs`:
  - [x] Independent RNG service for outcome generation.
  - [x] Generate 3 final target symbols per spin.
  - [x] Seeded / deterministic mode support for automated testing.
- [x] Create `SlotMachineController.cs`:
  - [x] Manage game states (`Idle`, `BetValidation`, `Spinning`, `Evaluating`, `PresentingWin`, `PresentingLoss`).
  - [x] Coordinate spin start across all 3 reels.
  - [x] Enforce input blocking while a spin is in progress.

## 6. Reel Motion Physics & Lever Interaction
- [x] Implement smooth continuous scrolling in `SlotReel.cs`:
  - [x] Ease-in acceleration on spin start.
  - [x] High-speed spin phase with random cycling symbols.
  - [x] Ease-out deceleration curves.
  - [x] Precise target snapping with slight bounce-back.
- [x] Implement staggered reel stopping:
  - [x] Reel 1 stops at $T = 1.0\text{s}$.
  - [x] Reel 2 stops at $T = 1.35\text{s}$.
  - [x] Reel 3 stops at $T = 1.70\text{s}$.
- [x] Create `LeverController.cs`:
  - [x] Support click and drag interaction on the lever arm.
  - [x] Animate lever from upright (`slot-machine2.png`) to pulled (`slot-machine3.png`).
  - [x] Trigger spin when pulled past threshold and spring back to upright.

## 7. Wallet, Betting & Win Evaluation
- [x] Create `WalletManager.cs`:
  - [x] Starting balance: ₹1,00,000 Royal VIP Credits.
  - [x] Bet values: ₹100 to ₹5,000 VIP ladder.
  - [x] Validate balance $\ge$ bet before allowing spin.
  - [x] Deduct bet at spin launch.
- [x] Create `WinEvaluator.cs`:
  - [x] Evaluate 3-reel stop outcomes.
  - [x] Exact match-3 evaluation ($BaseBet \times Multiplier$).
  - [x] Wild symbol (`SYM_03`) substitution: substitutes for regular symbols and applies $2\times$ / $4\times$ multiplier.
  - [x] 3 Wild symbols awards $100\times$ Kohinoor Mega Jackpot.
  - [x] Calculate total payout in Rupees and return structured `SpinResult`.
- [x] Update wallet balance with payout and trigger rolling score animation in `MiddleBoxHUD.cs`.

## 8. UI System, Middle Box HUD & Modal Popups
- [x] Create `MiddleBoxHUD.cs`:
  - [x] Mount inside `slot_machine_Middle_box.png`.
  - [x] Display formatted Credit Balance in Rupees.
  - [x] Display Current Bet with +/- interactive controls.
  - [x] Display Last Win amount with count-up animation.
- [x] Sliced 4-state sprite swaps (`Normal`, `Highlighted`, `Pressed`, `Disabled`):
  - [x] Spin button (`slot_machine_buttons-02.png`).
  - [x] Bet - button (`slot_machine_buttons-03.png`).
  - [x] Bet + button (`slot_machine_buttons-04.png`).
- [x] Create `WinPopupController.cs`:
  - [x] Modal dialog using `popup.png`.
  - [x] Yes and No buttons using sliced `Yes_No_Btn.png`.
  - [x] Big Win / Jackpot modal with payout breakdown.
  - [x] Insufficient Funds modal with "Reset Balance to ₹1,00,000?" option.
- [x] Configure `CanvasScaler` for $1920\times 1080$ resolution with responsive UI anchors.

## 9. Bonus Features, Audio & Visual Polish
- [ ] Add winning symbol pulse / glow animation during win presentation.
- [ ] Add celebration particle burst for high payouts ($> 20\times$).
- [ ] Create `AudioController.cs`:
  - [ ] Procedural audio / sound clips for:
    - [ ] Spin start / lever pull.
    - [ ] Reel spinning loop / reel click stop.
    - [ ] Standard win chime.
    - [ ] Big win / Jackpot fanfare.
    - [ ] Button click feedback.
- [ ] Add subtle cabinet shake on big win outcomes.

## 10. WebGL Build Pipeline & Optimization
- [ ] Configure WebGL PlayerSettings (Linear color space, WebGL 2.0 with fallback, Gzip/Brotli disabled for local file previews).
- [ ] Create `Editor/BuildScript.cs` with `BuildWebGL()` static method.
- [ ] Execute automated WebGL build to `Build/WebGL/`.
- [ ] Verify WebGL build execution using local HTTP server.

## 11. Testing & Edge Cases
- [x] Test balance deduction and payout arithmetic across all symbol combinations (`PayoutSimulationTest.cs`).
- [x] Test Wild symbol substitution with different regular symbols (`PayoutSimulationTest.cs`).
- [x] Test 3-Wild jackpot condition (`PayoutSimulationTest.cs`).
- [x] Test insufficient balance (balance $<$ bet) prevents spin and triggers modal.
- [x] Test rapid button clicking during spin to verify input locking (`RNGSimulationTest.cs`).
- [x] Test consecutive spins to confirm zero symbol vertical drift or alignment error.
- [ ] Test UI scaling across 16:9, 16:10, 4:3, and mobile portrait/landscape aspect ratios.

## 12. Documentation & Submission
- [ ] Create comprehensive `README.md`:
  - [ ] Game Overview and Core Features.
  - [ ] WebGL build instructions (how to run locally).
  - [ ] Bonus Features (Wild substitution, $2\times$ multiplier, Jackpot, interactive lever).
  - [ ] Architecture & OOP Design Decisions.
  - [ ] Folder Structure Breakdown.
- [ ] Review `.gitignore` to ensure `Library/`, `Temp/`, `Logs/` are excluded.
- [ ] Create clear, descriptive Git commit milestones.
- [ ] Verify repository completeness for public submission.
