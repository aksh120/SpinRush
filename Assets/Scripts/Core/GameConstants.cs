namespace SpinRush.Core
{
    /// <summary>
    /// Lifecycle states for the main slot machine game flow.
    /// </summary>
    public enum GameState
    {
        /// <summary>Game is idle and ready to accept a spin request.</summary>
        Idle,

        /// <summary>Validating player credits against the current bet amount.</summary>
        ValidatingBet,

        /// <summary>Reels are currently in motion.</summary>
        Spinning,

        /// <summary>All reels have stopped; evaluating combinations and payouts.</summary>
        Evaluating,

        /// <summary>Presenting winning lines, particle effects, and tallying payouts.</summary>
        PresentingWin,

        /// <summary>Presenting non-winning spin outcome.</summary>
        PresentingLoss
    }

    /// <summary>
    /// Motion states for an individual slot reel.
    /// </summary>
    public enum ReelSpinState
    {
        Stopped,
        Accelerating,
        Spinning,
        Decelerating,
        Snapping
    }

    /// <summary>
    /// Symbol hierarchy tier rating.
    /// </summary>
    public enum SymbolTier
    {
        Jackpot = 1,
        High = 2,
        Mid = 3,
        Base = 4
    }

    /// <summary>
    /// Global gameplay and Royal VIP Indian Rupee economy configuration constants.
    /// </summary>
    public static class GameConstants
    {
        // Currency & Localization
        public const string CurrencySymbol = "₹";
        public const string CurrencyCode = "INR";

        // Royal VIP Economy & Betting (Rupees)
        public const int DefaultStartingBalance = 100000; // ₹1,00,000 (1 Lakh Rupees)
        public const int DefaultBet = 500;               // ₹500
        public const int MinBet = 100;                   // ₹100
        public const int MaxBet = 5000;                  // ₹5,000

        // VIP Bet Ladder steps
        public static readonly int[] VIPBetLadder = new int[] { 100, 250, 500, 1000, 2500, 5000 };

        // Timing & Animation Parameters (Seconds)
        public const float ReelBaseSpinDuration = 1.0f;
        public const float ReelStaggerDelay = 0.35f;
        public const float SymbolHeight = 100f;
        public const float SpinScrollSpeed = 1200f;
        public const float DecelerationDuration = 0.45f;

        // Symbol IDs
        public const string SymbolLuckySeven = "SYM_01";
        public const string SymbolGoldenBell = "SYM_02";
        public const string SymbolTripleBar = "SYM_04";
        public const string SymbolStarWild = "SYM_03";

        // Payout Multipliers
        public const int MultiplierLuckySeven = 50;   // Royal 7s: 50x Bet
        public const int MultiplierGoldenBell = 25;   // Golden Bell: 25x Bet
        public const int MultiplierTripleBar = 10;    // Diamond Bar: 10x Bet
        public const int MultiplierKohinoorWild = 100; // Kohinoor 3-Wild Jackpot: 100x Bet
        public const float WildSubstitutionMultiplier = 2.0f; // 2x per Wild in winning line
    }
}
