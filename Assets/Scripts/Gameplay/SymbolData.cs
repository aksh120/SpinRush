using UnityEngine;

namespace SpinRush.Gameplay
{
    /// <summary>
    /// Immutable ScriptableObject definition for an individual slot machine symbol.
    /// Defines identification, visual icon, payout multipliers, and Wild bonus properties.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSymbolData", menuName = "SpinRush/Symbol Data", order = 1)]
    public class SymbolData : ScriptableObject
    {
        [Header("Symbol Identity")]
        [Tooltip("Unique programmatic identifier for the symbol (e.g., SYM_01).")]
        [SerializeField] private string symbolId = "SYM_01";

        [Tooltip("Human-readable name of the symbol.")]
        [SerializeField] private string displayName = "Lucky Seven";

        [Header("Visuals")]
        [Tooltip("Sprite icon used to render the symbol on the reels.")]
        [SerializeField] private Sprite icon;

        [Header("Payout & Tier")]
        [Tooltip("Base payout multiplier for a 3-of-a-kind match (e.g., 50 means 50x current bet).")]
        [SerializeField] private int payoutMultiplier = 10;

        [Tooltip("Tier rating: 1 = Top Tier / Jackpot, 2 = High, 3 = Mid, 4 = Base.")]
        [SerializeField] private int tier = 4;

        [Header("Bonus / Wild Mechanics")]
        [Tooltip("If true, this symbol acts as a Wild and can substitute for other regular symbols.")]
        [SerializeField] private bool isWild = false;

        [Tooltip("Bonus multiplier applied when this Wild substitutes in a winning line.")]
        [SerializeField] private float wildMultiplier = 2.0f;

        // Public read-only property accessors
        public string SymbolId => symbolId;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public int PayoutMultiplier => payoutMultiplier;
        public int Tier => tier;
        public bool IsWild => isWild;
        public float WildMultiplier => wildMultiplier;

        /// <summary>
        /// Configures the symbol data in editor / initialization pipelines.
        /// </summary>
        public void Initialize(string id, string name, Sprite sprite, int payout, int symbolTier, bool wild = false, float wildMult = 2f)
        {
            symbolId = id;
            displayName = name;
            icon = sprite;
            payoutMultiplier = payout;
            tier = symbolTier;
            isWild = wild;
            wildMultiplier = wildMult;
        }
    }
}
