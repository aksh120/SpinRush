using System;
using System.Collections.Generic;
using UnityEngine;
using SpinRush.Core;

namespace SpinRush.Gameplay
{
    /// <summary>
    /// Immutable value object containing the full payout breakdown and metadata for a spin.
    /// </summary>
    [System.Serializable]
    public struct SpinResult
    {
        public bool IsWin;
        public int Payout;
        public float Multiplier;
        public SymbolData WinningSymbol;
        public int WildCount;
        public bool IsJackpot;
        public bool IsRoyalJackpot;
        public string RarityBadge;
        public string WinTitle;
        public string Description;
        public string FormattedPayout;

        public static SpinResult CreateLoss()
        {
            return new SpinResult
            {
                IsWin = false,
                Payout = 0,
                Multiplier = 0f,
                WinningSymbol = null,
                WildCount = 0,
                IsJackpot = false,
                IsRoyalJackpot = false,
                RarityBadge = "",
                WinTitle = "TRY AGAIN",
                Description = "No winning payline.",
                FormattedPayout = WalletManager.FormatRupees(0)
            };
        }
    }

    /// <summary>
    /// Pure mathematical win evaluation engine for SpinRush.
    /// Evaluates 3-reel stop permutations, natural 3-of-a-kind matches,
    /// Kohinoor 3-Wild Jackpots, and compounding Wild multiplier substitutions.
    /// </summary>
    public static class WinEvaluator
    {
        /// <summary>
        /// Evaluates a 3-reel outcome against the active bet amount.
        /// </summary>
        /// <param name="symbols">Array of the 3 visible center symbols.</param>
        /// <param name="betAmount">Current active bet in Indian Rupees (₹).</param>
        /// <returns>Structured SpinResult detailing payout and celebration metadata.</returns>
        public static SpinResult EvaluateSpin(SymbolData[] symbols, int betAmount)
        {
            if (symbols == null || symbols.Length < 3)
            {
                return SpinResult.CreateLoss();
            }

            SymbolData s1 = symbols[0];
            SymbolData s2 = symbols[1];
            SymbolData s3 = symbols[2];

            if (s1 == null || s2 == null || s3 == null)
            {
                return SpinResult.CreateLoss();
            }

            // Count Wilds
            int wildCount = 0;
            if (s1.IsWild) wildCount++;
            if (s2.IsWild) wildCount++;
            if (s3.IsWild) wildCount++;

            // CASE 1: 3-Wilds -> ROYAL KOHINOOR MEGA JACKPOT (100x)
            if (wildCount == 3)
            {
                float mult = GameConstants.MultiplierKohinoorWild;
                int payout = Mathf.RoundToInt(mult * betAmount);
                return new SpinResult
                {
                    IsWin = true,
                    Payout = payout,
                    Multiplier = mult,
                    WinningSymbol = s1,
                    WildCount = 3,
                    IsJackpot = true,
                    IsRoyalJackpot = true,
                    RarityBadge = "ULTRA RARE: TOP 2% CHANCE (1 IN 50 SPINS)!",
                    WinTitle = "ROYAL DHAMAKA JACKPOT!",
                    Description = "COLOSSAL 100x MULTIPLIER!\nAll 3 reels locked onto the legendary Kohinoor Star Wild!",
                    FormattedPayout = WalletManager.FormatRupees(payout)
                };
            }

            // CASE 2: Natural 3-of-a-Kind (3 identical regular symbols)
            if (s1.SymbolId == s2.SymbolId && s2.SymbolId == s3.SymbolId)
            {
                float mult = s1.PayoutMultiplier;
                int payout = Mathf.RoundToInt(mult * betAmount);
                bool isJackpot = s1.SymbolId == GameConstants.SymbolLuckySeven;

                string title = isJackpot ? "ROYAL 7s JACKPOT!" :
                              (s1.SymbolId == GameConstants.SymbolGoldenBell ? "GRAND GOLDEN WIN!" : "DIAMOND STRIKE!");

                string badge = isJackpot ? "ULTRA RARE: TOP 2% CHANCE (1 IN 50 SPINS)!" : "ALL 3 REELS MATCHED!";

                return new SpinResult
                {
                    IsWin = true,
                    Payout = payout,
                    Multiplier = mult,
                    WinningSymbol = s1,
                    WildCount = 0,
                    IsJackpot = isJackpot,
                    IsRoyalJackpot = isJackpot,
                    RarityBadge = badge,
                    WinTitle = title,
                    Description = $"All 3 {s1.DisplayName}s Matched! {mult}x Bet Payout!",
                    FormattedPayout = WalletManager.FormatRupees(payout)
                };
            }

            // CASE 3: 2 Wilds + 1 Regular Symbol -> Double Wild Multiplier (4x)
            if (wildCount == 2)
            {
                SymbolData regularSym = !s1.IsWild ? s1 : (!s2.IsWild ? s2 : s3);
                float mult = regularSym.PayoutMultiplier * 4.0f; // 2x * 2x
                int payout = Mathf.RoundToInt(mult * betAmount);

                return new SpinResult
                {
                    IsWin = true,
                    Payout = payout,
                    Multiplier = mult,
                    WinningSymbol = regularSym,
                    WildCount = 2,
                    IsJackpot = regularSym.SymbolId == GameConstants.SymbolLuckySeven,
                    WinTitle = "DOUBLE WILD MULTIPLIER (4X)!",
                    Description = $"{regularSym.DisplayName} + 2 Wilds! {mult}x Payout!",
                    FormattedPayout = WalletManager.FormatRupees(payout)
                };
            }

            // CASE 4: 1 Wild + 2 Matching Regular Symbols -> Single Wild Multiplier (2x)
            if (wildCount == 1)
            {
                // Identify the 2 regular symbols
                SymbolData rA = null, rB = null;
                if (!s1.IsWild) { rA = s1; }
                if (!s2.IsWild) { if (rA == null) rA = s2; else rB = s2; }
                if (!s3.IsWild) { if (rA == null) rA = s3; else rB = s3; }

                if (rA != null && rB != null && rA.SymbolId == rB.SymbolId)
                {
                    float mult = rA.PayoutMultiplier * GameConstants.WildSubstitutionMultiplier; // 2x
                    int payout = Mathf.RoundToInt(mult * betAmount);

                    return new SpinResult
                    {
                        IsWin = true,
                        Payout = payout,
                        Multiplier = mult,
                        WinningSymbol = rA,
                        WildCount = 1,
                        IsJackpot = rA.SymbolId == GameConstants.SymbolLuckySeven,
                        WinTitle = "ROYAL WILD MULTIPLIER (2X)!",
                        Description = $"2 {rA.DisplayName}s + 1 Wild! {mult}x Payout!",
                        FormattedPayout = WalletManager.FormatRupees(payout)
                    };
                }
            }

            // CASE 5: No winning payline
            return SpinResult.CreateLoss();
        }
    }
}
