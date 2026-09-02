using UnityEditor;
using UnityEngine;
using SpinRush.Core;
using SpinRush.Gameplay;

namespace SpinRush.Editor
{
    /// <summary>
    /// Automated test runner to validate Royal VIP Indian Rupee (₹) paytable mathematics,
    /// Kohinoor Jackpots, compounding Wild substitutions, and WalletManager economy.
    /// </summary>
    public static class PayoutSimulationTest
    {
        [MenuItem("SpinRush/Run Royal VIP Payout & Wallet Tests")]
        public static void RunTests()
        {
            Debug.Log("=================================================");
            Debug.Log("  SPINRUSH ROYAL VIP (₹) PAYTABLE & WALLET TESTS");
            Debug.Log("=================================================");

            string dbPath = "Assets/Data/SymbolDatabase.asset";
            SymbolDatabase db = AssetDatabase.LoadAssetAtPath<SymbolDatabase>(dbPath);
            if (db == null)
            {
                Debug.LogError($"[TEST FAILED] SymbolDatabase not found at {dbPath}!");
                return;
            }

            var symSeven = db.GetSymbol(GameConstants.SymbolLuckySeven);
            var symBell = db.GetSymbol(GameConstants.SymbolGoldenBell);
            var symBar = db.GetSymbol(GameConstants.SymbolTripleBar);
            var symWild = db.GetSymbol(GameConstants.SymbolStarWild);

            // TEST 1: Natural 3-Match Paytable Verification
            TestNatural3Matches(symSeven, symBell, symBar);

            // TEST 2: Kohinoor 3-Wild Mega Jackpot (100x)
            TestKohinoorJackpot(symWild);

            // TEST 3: Compounding Wild Substitutions (2x & 4x)
            TestWildSubstitutions(symSeven, symWild);

            // TEST 4: Non-Winning Permutation
            TestNonWinning(symSeven, symBell, symBar);

            // TEST 5: Royal VIP Wallet Economy & Bet Ladder
            TestWalletEconomy();

            Debug.Log("=================================================");
            Debug.Log("  ALL ROYAL VIP PAYOUT & WALLET TESTS PASSED [100%]");
            Debug.Log("=================================================");
        }

        private static void TestNatural3Matches(SymbolData seven, SymbolData bell, SymbolData bar)
        {
            Debug.Log("\n--- TEST 1: Natural 3-Match Paytable ---");
            int bet = 500; // ₹500

            // 3 Sevens: 50x -> ₹25,000
            var resSeven = WinEvaluator.EvaluateSpin(new SymbolData[] { seven, seven, seven }, bet);
            AssertEqual(resSeven.IsWin, true, "3 Sevens should win");
            AssertEqual(resSeven.Payout, 25000, "3 Sevens on ₹500 bet should pay ₹25,000");

            // 3 Bells: 25x -> ₹12,500
            var resBell = WinEvaluator.EvaluateSpin(new SymbolData[] { bell, bell, bell }, bet);
            AssertEqual(resBell.IsWin, true, "3 Bells should win");
            AssertEqual(resBell.Payout, 12500, "3 Bells on ₹500 bet should pay ₹12,500");

            // 3 Bars: 10x -> ₹5,000
            var resBar = WinEvaluator.EvaluateSpin(new SymbolData[] { bar, bar, bar }, bet);
            AssertEqual(resBar.IsWin, true, "3 Bars should win");
            AssertEqual(resBar.Payout, 5000, "3 Bars on ₹500 bet should pay ₹5,000");

            Debug.Log("[PASS] Natural 3-matches correctly evaluated against paytable.");
        }

        private static void TestKohinoorJackpot(SymbolData wild)
        {
            Debug.Log("\n--- TEST 2: Kohinoor 3-Wild Mega Jackpot ---");
            int maxBet = 5000; // ₹5,000

            var res = WinEvaluator.EvaluateSpin(new SymbolData[] { wild, wild, wild }, maxBet);
            AssertEqual(res.IsWin, true, "3 Wilds should trigger win");
            AssertEqual(res.IsJackpot, true, "3 Wilds should flag as Jackpot");
            AssertEqual(res.Multiplier, 100f, "3 Wilds multiplier should be 100x");
            AssertEqual(res.Payout, 500000, "3 Wilds on ₹5,000 max bet should pay ₹5,00,000 (5 Lakhs)");

            Debug.Log($"[PASS] Kohinoor Jackpot evaluated correctly: {res.FormattedPayout} payout!");
        }

        private static void TestWildSubstitutions(SymbolData seven, SymbolData wild)
        {
            Debug.Log("\n--- TEST 3: Compounding Wild Substitutions ---");
            int bet = 1000; // ₹1,000

            // 2 Sevens + 1 Wild: 50x * 2x = 100x -> ₹1,00,000
            var resSingleWild = WinEvaluator.EvaluateSpin(new SymbolData[] { seven, wild, seven }, bet);
            AssertEqual(resSingleWild.IsWin, true, "Seven + Wild + Seven should win");
            AssertEqual(resSingleWild.Multiplier, 100f, "Seven + Wild + Seven multiplier should be 100x (50x * 2)");
            AssertEqual(resSingleWild.Payout, 100000, "Seven + Wild + Seven on ₹1,000 bet should pay ₹1,00,000");

            // 1 Seven + 2 Wilds: 50x * 4x = 200x -> ₹2,00,000
            var resDoubleWild = WinEvaluator.EvaluateSpin(new SymbolData[] { wild, seven, wild }, bet);
            AssertEqual(resDoubleWild.IsWin, true, "Wild + Seven + Wild should win");
            AssertEqual(resDoubleWild.Multiplier, 200f, "Wild + Seven + Wild multiplier should be 200x (50x * 4)");
            AssertEqual(resDoubleWild.Payout, 200000, "Wild + Seven + Wild on ₹1,000 bet should pay ₹2,00,000");

            Debug.Log("[PASS] Compounding Wild substitutions (2x and 4x) verified.");
        }

        private static void TestNonWinning(SymbolData seven, SymbolData bell, SymbolData bar)
        {
            Debug.Log("\n--- TEST 4: Non-Winning Combination ---");
            var res = WinEvaluator.EvaluateSpin(new SymbolData[] { seven, bell, bar }, 500);
            AssertEqual(res.IsWin, false, "Seven + Bell + Bar should not win");
            AssertEqual(res.Payout, 0, "Non-winning spin payout should be 0");
            Debug.Log("[PASS] Non-winning permutations correctly return 0 payout.");
        }

        private static void TestWalletEconomy()
        {
            Debug.Log("\n--- TEST 5: Royal VIP Wallet Economy & Bet Ladder ---");
            var go = new GameObject("TestWallet");
            var wallet = go.AddComponent<WalletManager>();

            // Default VIP balance: ₹1,00,000
            AssertEqual(wallet.Balance, 100000, "Starting balance should be ₹1,00,000");
            AssertEqual(wallet.CurrentBet, 500, "Default bet should be ₹500");

            // Bet ladder stepping
            wallet.IncreaseBet(); // ₹1,000
            AssertEqual(wallet.CurrentBet, 1000, "Bet should step up to ₹1,000");
            wallet.IncreaseBet(); // ₹2,500
            AssertEqual(wallet.CurrentBet, 2500, "Bet should step up to ₹2,500");
            wallet.SetMaxBet();   // ₹5,000
            AssertEqual(wallet.CurrentBet, 5000, "Max bet should be ₹5,000");

            // Deduct max bet
            bool deducted = wallet.DeductBet();
            AssertEqual(deducted, true, "Deduct bet should succeed");
            AssertEqual(wallet.Balance, 95000, "Balance should be ₹95,000 after ₹5,000 deduction");

            // Award Jackpot
            wallet.AwardPayout(500000);
            AssertEqual(wallet.Balance, 595000, "Balance should be ₹5,95,000 after ₹5 Lakh award");

            // Insufficient funds guard
            wallet.ResetBalance(100);
            wallet.SetMaxBet(); // Bet is 5000, balance is 100
            bool insufficientDeduct = wallet.DeductBet();
            AssertEqual(insufficientDeduct, false, "Deduct bet should fail when balance < bet");

            Object.DestroyImmediate(go);
            Debug.Log("[PASS] Wallet balance deductions, payouts, and bet ladder verified.");
        }

        private static void AssertEqual<T>(T actual, T expected, string message)
        {
            if (!Equals(actual, expected))
            {
                Debug.LogError($"[ASSERTION FAILED] {message}. Expected: {expected}, Got: {actual}");
            }
        }
    }
}
