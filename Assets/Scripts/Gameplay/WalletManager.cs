using System;
using System.Globalization;
using UnityEngine;
using SpinRush.Core;

namespace SpinRush.Gameplay
{
    /// <summary>
    /// Manages the player's high-roller Indian Rupee (₹) wallet economy,
    /// VIP bet ladder stepping, credit deductions, and payout awards.
    /// </summary>
    public class WalletManager : MonoBehaviour
    {
        [Header("Royal VIP Balance (Rupees)")]
        [Tooltip("Current total player credit balance in Indian Rupees (₹).")]
        [SerializeField] private int balance = GameConstants.DefaultStartingBalance;

        [Header("VIP Bet Ladder")]
        [Tooltip("Active index within the VIP bet ladder.")]
        [SerializeField] private int currentBetIndex = 2; // Default ₹500 (index 2: 100, 250, 500, 1000, 2500, 5000)

        [Header("Stats")]
        [SerializeField] private int lastWin = 0;

        // Events
        public event Action<int, int> OnBalanceChanged; // (newBalance, delta)
        public event Action<int> OnBetChanged;          // (newBet)
        public event Action<int> OnWinAwarded;         // (winAmount)
        public event Action OnInsufficientFunds;

        public int Balance => balance;
        public int CurrentBet => GameConstants.VIPBetLadder[Mathf.Clamp(currentBetIndex, 0, GameConstants.VIPBetLadder.Length - 1)];
        public int LastWin => lastWin;
        public int BetIndex => currentBetIndex;

        private void Awake()
        {
            // Enforce authentic bankroll of ₹5,000
            balance = GameConstants.DefaultStartingBalance;
        }

        private void Start()
        {
            // Initial broadcast of wallet state
            OnBalanceChanged?.Invoke(balance, 0);
            OnBetChanged?.Invoke(CurrentBet);
        }

        /// <summary>
        /// Steps up to the next higher VIP bet amount.
        /// </summary>
        public void IncreaseBet()
        {
            if (currentBetIndex < GameConstants.VIPBetLadder.Length - 1)
            {
                currentBetIndex++;
                Debug.Log($"[WalletManager] Bet increased to: {GetFormattedBet()}");
                OnBetChanged?.Invoke(CurrentBet);
            }
        }

        /// <summary>
        /// Steps down to the next lower VIP bet amount.
        /// </summary>
        public void DecreaseBet()
        {
            if (currentBetIndex > 0)
            {
                currentBetIndex--;
                Debug.Log($"[WalletManager] Bet decreased to: {GetFormattedBet()}");
                OnBetChanged?.Invoke(CurrentBet);
            }
        }

        /// <summary>
        /// Jumps immediately to the highest VIP bet tier (₹5,000).
        /// </summary>
        public void SetMaxBet()
        {
            currentBetIndex = GameConstants.VIPBetLadder.Length - 1;
            Debug.Log($"[WalletManager] Max Bet selected: {GetFormattedBet()}");
            OnBetChanged?.Invoke(CurrentBet);
        }

        /// <summary>
        /// Checks if the player has enough Rupees to afford the current bet.
        /// </summary>
        public bool CanAffordSpin()
        {
            return balance >= CurrentBet;
        }

        /// <summary>
        /// Deducts the active bet from the wallet balance upon spin launch.
        /// Returns true if successfully deducted, or false if insufficient funds.
        /// </summary>
        public bool DeductBet()
        {
            if (!CanAffordSpin())
            {
                Debug.LogWarning($"[WalletManager] Insufficient funds: Balance ({GetFormattedBalance()}) < Bet ({GetFormattedBet()})");
                OnInsufficientFunds?.Invoke();
                return false;
            }

            int betAmount = CurrentBet;
            balance -= betAmount;
            lastWin = 0;

            Debug.Log($"[WalletManager] Deducted bet: -{FormatRupees(betAmount)}. New Balance: {GetFormattedBalance()}");
            OnBalanceChanged?.Invoke(balance, -betAmount);
            return true;
        }

        /// <summary>
        /// Credits a winning payout in Rupees to the player's balance.
        /// </summary>
        public void AwardPayout(int payoutAmount)
        {
            if (payoutAmount <= 0) return;

            balance += payoutAmount;
            lastWin = payoutAmount;

            Debug.Log($"[WalletManager] Awarded Payout: +{FormatRupees(payoutAmount)}! New Balance: {GetFormattedBalance()}");
            OnBalanceChanged?.Invoke(balance, payoutAmount);
            OnWinAwarded?.Invoke(payoutAmount);
        }

        /// <summary>
        /// Resets the wallet balance (e.g. from popup prompt or debug reset).
        /// </summary>
        public void ResetBalance(int newAmount = GameConstants.DefaultStartingBalance)
        {
            int delta = newAmount - balance;
            balance = newAmount;
            lastWin = 0;
            Debug.Log($"[WalletManager] Wallet balance reset to: {GetFormattedBalance()}");
            OnBalanceChanged?.Invoke(balance, delta);
        }

        /// <summary>
        /// Returns the formatted Rupee balance string (e.g., "₹1,00,000").
        /// </summary>
        public string GetFormattedBalance() => FormatRupees(balance);

        /// <summary>
        /// Returns the formatted Rupee bet string (e.g., "₹500").
        /// </summary>
        public string GetFormattedBet() => FormatRupees(CurrentBet);

        /// <summary>
        /// Returns the formatted Rupee win string (e.g., "₹50,000").
        /// </summary>
        public string GetFormattedWin() => FormatRupees(lastWin);

        /// <summary>
        /// Static formatter converting raw numeric integer to Indian Rupee notation (e.g. ₹1,00,000).
        /// </summary>
        public static string FormatRupees(int amount)
        {
            // Standard Indian/Global currency grouping
            return $"{GameConstants.CurrencySymbol}{amount:N0}";
        }
    }
}
