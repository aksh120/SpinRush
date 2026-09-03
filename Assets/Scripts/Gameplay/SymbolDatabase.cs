using System.Collections.Generic;
using UnityEngine;

namespace SpinRush.Gameplay
{
    /// <summary>
    /// Centralized ScriptableObject database containing all active symbols in the game.
    /// Provides fast dictionary-based lookups and random symbol sampling.
    /// </summary>
    [CreateAssetMenu(fileName = "SymbolDatabase", menuName = "SpinRush/Symbol Database", order = 2)]
    public class SymbolDatabase : ScriptableObject
    {
        [Tooltip("Collection of all active symbols available in the slot machine.")]
        [SerializeField] private List<SymbolData> symbols = new List<SymbolData>();

        private Dictionary<string, SymbolData> _lookupCache;

        public IReadOnlyList<SymbolData> Symbols => symbols;
        public int Count => symbols != null ? symbols.Count : 0;
        public SymbolData this[int index] => (symbols != null && index >= 0 && index < symbols.Count) ? symbols[index] : null;

        private void OnEnable()
        {
            BuildLookupCache();
        }

        private void BuildLookupCache()
        {
            if (_lookupCache == null)
            {
                _lookupCache = new Dictionary<string, SymbolData>();
            }
            else
            {
                _lookupCache.Clear();
            }

            if (symbols == null) return;

            foreach (var sym in symbols)
            {
                if (sym != null && !string.IsNullOrEmpty(sym.SymbolId))
                {
                    _lookupCache[sym.SymbolId] = sym;
                }
            }
        }

        /// <summary>
        /// Retrieves a SymbolData by its unique ID.
        /// </summary>
        public SymbolData GetSymbol(string symbolId)
        {
            if (string.IsNullOrEmpty(symbolId)) return null;

            if (_lookupCache == null || _lookupCache.Count == 0)
            {
                BuildLookupCache();
            }

            if (_lookupCache != null && _lookupCache.TryGetValue(symbolId, out var foundSymbol))
            {
                return foundSymbol;
            }

            // Fallback scan
            for (int i = 0; i < symbols.Count; i++)
            {
                if (symbols[i] != null && symbols[i].SymbolId == symbolId)
                {
                    return symbols[i];
                }
            }

            Debug.LogWarning($"[SymbolDatabase] Symbol ID '{symbolId}' not found in database.");
            return null;
        }

        /// <summary>
        /// Gets a symbol by index.
        /// </summary>
        public SymbolData GetSymbolByIndex(int index)
        {
            if (symbols == null || index < 0 || index >= symbols.Count) return null;
            return symbols[index];
        }

        /// <summary>
        /// Returns a uniformly random symbol from the database.
        /// </summary>
        public SymbolData GetRandomSymbol()
        {
            if (symbols == null || symbols.Count == 0) return null;
            int randomIndex = Random.Range(0, symbols.Count);
            return symbols[randomIndex];
        }

        /// <summary>
        /// Helper method for editor scripts to assign the symbol list.
        /// </summary>
        public void SetSymbols(IEnumerable<SymbolData> newSymbols)
        {
            if (symbols == null) symbols = new List<SymbolData>();
            symbols.Clear();
            if (newSymbols != null)
            {
                symbols.AddRange(newSymbols);
            }
            BuildLookupCache();
        }
    }
}
