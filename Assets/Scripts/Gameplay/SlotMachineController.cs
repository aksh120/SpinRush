using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpinRush.Audio;
using SpinRush.Core;
using SpinRush.Effects;
using SpinRush.UI;

namespace SpinRush.Gameplay
{
    /// <summary>
    /// Central game coordinator and spin flow state machine for the slot machine.
    /// Coordinates wallet validation, bet deduction, RNG outcome determination,
    /// synchronized reel motion, win evaluation, audio cues, and visual fanfare.
    /// </summary>
    public class SlotMachineController : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private GameState currentState = GameState.Idle;

        [Header("Dependencies")]
        [Tooltip("Database of available slot symbols.")]
        [SerializeField] private SymbolDatabase symbolDatabase;

        [Tooltip("Independent RNG engine service.")]
        [SerializeField] private RandomNumberGenerator rng;

        [Tooltip("Player wallet and economy manager.")]
        [SerializeField] private WalletManager walletManager;

        [Tooltip("Modal celebration and low balance dialog controller.")]
        [SerializeField] private WinPopupController popupController;

        [Tooltip("Procedural audio controller.")]
        [SerializeField] private AudioController audioController;

        [Tooltip("Visual particles and screen-shake presenter.")]
        [SerializeField] private WinEffectsPresenter effectsPresenter;

        [Tooltip("Collection of the 3 active reels.")]
        [SerializeField] private List<SlotReel> reels = new List<SlotReel>();

        [Header("Spin Configuration")]
        [Tooltip("Base duration before the first reel begins deceleration (seconds).")]
        [SerializeField] private float baseSpinDuration = GameConstants.ReelBaseSpinDuration;

        [Tooltip("Stagger delay between consecutive reel stops (seconds).")]
        [SerializeField] private float reelStaggerDelay = GameConstants.ReelStaggerDelay;

        // Current spin target outcome & result
        private SymbolData[] _currentTargets;
        private SpinResult _lastResult;
        private Coroutine _spinCoroutine;

        // Public Events
        public event Action<GameState> OnStateChanged;
        public event Action<SymbolData[]> OnSpinStarted;
        public event Action<int, SymbolData> OnReelStopped;
        public event Action<SpinResult> OnSpinEvaluated;

        public GameState CurrentState => currentState;
        public bool IsSpinning => currentState != GameState.Idle && currentState != GameState.PresentingWin && currentState != GameState.PresentingLoss;
        public IReadOnlyList<SlotReel> Reels => reels;
        public SymbolData[] CurrentTargets => _currentTargets;
        public SpinResult LastResult => _lastResult;
        public WalletManager Wallet => walletManager;

        private void Awake()
        {
            if (rng == null)
            {
                rng = GetComponent<RandomNumberGenerator>();
                if (rng == null) rng = gameObject.AddComponent<RandomNumberGenerator>();
            }

            if (walletManager == null)
            {
                walletManager = GetComponent<WalletManager>();
                if (walletManager == null) walletManager = GetComponentInParent<WalletManager>();
                if (walletManager == null) walletManager = gameObject.AddComponent<WalletManager>();
            }

            if (popupController == null)
            {
                popupController = FindObjectOfType<WinPopupController>();
            }

            if (audioController == null)
            {
                audioController = FindObjectOfType<AudioController>();
                if (audioController == null) audioController = gameObject.AddComponent<AudioController>();
            }

            if (effectsPresenter == null)
            {
                effectsPresenter = FindObjectOfType<WinEffectsPresenter>();
                if (effectsPresenter == null) effectsPresenter = gameObject.AddComponent<WinEffectsPresenter>();
            }

            if (reels == null || reels.Count == 0)
            {
                GetComponentsInChildren(true, reels);
            }
        }

        private void Start()
        {
            if (walletManager != null)
            {
                walletManager.OnInsufficientFunds += HandleInsufficientFunds;
            }

            InitializeGame();
        }

        private void OnDestroy()
        {
            if (walletManager != null)
            {
                walletManager.OnInsufficientFunds -= HandleInsufficientFunds;
            }
        }

        /// <summary>
        /// Initializes reels and puts the machine in Idle state.
        /// </summary>
        public void InitializeGame()
        {
            if (symbolDatabase != null && reels != null)
            {
                for (int i = 0; i < reels.Count; i++)
                {
                    if (reels[i] != null)
                    {
                        reels[i].Initialize(i, symbolDatabase);
                    }
                }
            }

            ChangeState(GameState.Idle);
        }

        /// <summary>
        /// Void wrapper for UI button onclick events and lever triggers.
        /// </summary>
        public void OnSpinButtonClicked()
        {
            if (audioController != null) audioController.PlayButtonClick();
            RequestSpin();
        }

        /// <summary>
        /// Attempts to initiate a spin from player input (Spin button or Lever pull).
        /// Returns true if the spin request was accepted; false if rejected due to active spin or low funds.
        /// </summary>
        public bool RequestSpin()
        {
            // Input lock check: only accept spin in Idle or post-presentation states
            if (currentState != GameState.Idle && currentState != GameState.PresentingWin && currentState != GameState.PresentingLoss)
            {
                Debug.Log($"[SlotMachineController] Spin rejected: Current state is {currentState}.");
                return false;
            }

            if (symbolDatabase == null || symbolDatabase.Count == 0)
            {
                Debug.LogError("[SlotMachineController] Spin failed: SymbolDatabase is not assigned.");
                return false;
            }

            // Wallet check: validate and deduct bet in Rupees
            if (walletManager != null)
            {
                if (!walletManager.DeductBet())
                {
                    return false;
                }
            }

            StartSpinSequence();
            return true;
        }

        private void StartSpinSequence()
        {
            ChangeState(GameState.Spinning);

            if (audioController != null) audioController.StartReelSpinLoop();

            // Clear previous winning highlights
            for (int i = 0; i < reels.Count; i++)
            {
                if (reels[i] != null) reels[i].SetWinningHighlight(false);
            }

            // Generate fair RNG outcome BEFORE animating
            int reelCount = reels.Count > 0 ? reels.Count : 3;
            _currentTargets = rng.GenerateSpinOutcome(symbolDatabase, reelCount);

            OnSpinStarted?.Invoke(_currentTargets);

            // Start all reels spinning simultaneously
            for (int i = 0; i < reels.Count; i++)
            {
                if (reels[i] != null)
                {
                    reels[i].StartSpin();
                }
            }

            if (_spinCoroutine != null) StopCoroutine(_spinCoroutine);
            _spinCoroutine = StartCoroutine(ExecuteSpinFlow());
        }

        private IEnumerator ExecuteSpinFlow()
        {
            int reelCount = reels.Count;

            // Wait for base spin duration
            yield return new WaitForSeconds(baseSpinDuration);

            // Staggered stop sequence
            for (int i = 0; i < reelCount; i++)
            {
                if (i < _currentTargets.Length && _currentTargets[i] != null)
                {
                    if (reels[i] != null)
                    {
                        reels[i].StopAtTarget(_currentTargets[i]);
                    }

                    if (audioController != null) audioController.PlayReelStop(i);
                    if (effectsPresenter != null) effectsPresenter.TriggerReelStopShake();

                    OnReelStopped?.Invoke(i, _currentTargets[i]);
                }

                if (i < reelCount - 1)
                {
                    yield return new WaitForSeconds(reelStaggerDelay);
                }
            }

            // Wait for last reel deceleration to settle
            yield return new WaitForSeconds(0.55f);

            if (audioController != null) audioController.StopReelSpinLoop();

            // All reels locked and snapped -> Evaluate outcome
            ChangeState(GameState.Evaluating);

            int activeBet = walletManager != null ? walletManager.CurrentBet : GameConstants.DefaultBet;
            _lastResult = WinEvaluator.EvaluateSpin(_currentTargets, activeBet);

            OnSpinEvaluated?.Invoke(_lastResult);

            if (_lastResult.IsWin)
            {
                ChangeState(GameState.PresentingWin);

                // Play audio celebration
                if (audioController != null)
                {
                    audioController.PlayWinCelebration(_lastResult.Multiplier, _lastResult.IsJackpot);
                }

                // Trigger visual particles and screen shake
                if (effectsPresenter != null)
                {
                    effectsPresenter.TriggerWinCelebration(_lastResult);
                }

                // Highlight winning symbols on reels
                for (int i = 0; i < reels.Count; i++)
                {
                    if (reels[i] != null) reels[i].SetWinningHighlight(true);
                }

                // Award payout in Rupees
                if (walletManager != null)
                {
                    walletManager.AwardPayout(_lastResult.Payout);
                }

                // Show modal popup for big wins (25x or higher) and Kohinoor Jackpots
                if (popupController != null && (_lastResult.IsJackpot || _lastResult.Multiplier >= 25f))
                {
                    popupController.ShowWinPopup(_lastResult);
                }

                // Win presentation duration (longer for big jackpot)
                float presentationTime = _lastResult.IsJackpot ? 2.5f : 1.2f;
                yield return new WaitForSeconds(presentationTime);
            }
            else
            {
                ChangeState(GameState.PresentingLoss);
                yield return new WaitForSeconds(0.3f);
            }

            // Reset back to Idle ready for next spin
            ChangeState(GameState.Idle);
            _spinCoroutine = null;
        }

        private void HandleInsufficientFunds()
        {
            if (audioController != null) audioController.PlayLowBalanceAlert();

            if (popupController != null)
            {
                popupController.ShowInsufficientFundsPopup(
                    onResetConfirmed: () => walletManager.ResetBalance(GameConstants.DefaultStartingBalance),
                    onCancelled: null
                );
            }
        }

        /// <summary>
        /// Updates the state machine and invokes the state change event.
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            Debug.Log($"[SlotMachineController] State changed -> {currentState}");
            OnStateChanged?.Invoke(currentState);
        }

        /// <summary>
        /// Sets dependencies manually for editor tests and scene builders.
        /// </summary>
        public void Configure(SymbolDatabase db, RandomNumberGenerator rngService, List<SlotReel> reelList)
        {
            symbolDatabase = db;
            rng = rngService;
            if (walletManager == null) walletManager = GetComponent<WalletManager>();
            reels = reelList;
        }

        /// <summary>
        /// Sets dependencies manually for editor tests and scene builders.
        /// </summary>
        public void Configure(SymbolDatabase db, RandomNumberGenerator rngService, WalletManager wallet, List<SlotReel> reelList, WinPopupController popup = null, AudioController audio = null, WinEffectsPresenter fx = null)
        {
            symbolDatabase = db;
            rng = rngService;
            walletManager = wallet;
            reels = reelList;
            popupController = popup;
            audioController = audio;
            effectsPresenter = fx;
        }
    }
}
