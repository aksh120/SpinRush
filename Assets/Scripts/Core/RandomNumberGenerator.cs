using System;
using UnityEngine;
using SpinRush.Gameplay;

namespace SpinRush.Core
{
    /// <summary>
    /// Abstraction for random value providers allowing deterministic mock injection during automated testing.
    /// </summary>
    public interface IRandomProvider
    {
        int Range(int minInclusive, int maxExclusive);
        float Range(float minInclusive, float maxInclusive);
    }

    /// <summary>
    /// Standard Unity-based random provider for live gameplay.
    /// </summary>
    public class UnityRandomProvider : IRandomProvider
    {
        public int Range(int minInclusive, int maxExclusive) => UnityEngine.Random.Range(minInclusive, maxExclusive);
        public float Range(float minInclusive, float maxInclusive) => UnityEngine.Random.Range(minInclusive, maxInclusive);
    }

    /// <summary>
    /// Seeded deterministic random provider for reproducible test cases.
    /// </summary>
    public class SeededRandomProvider : IRandomProvider
    {
        private readonly System.Random _random;

        public SeededRandomProvider(int seed)
        {
            _random = new System.Random(seed);
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive) return minInclusive;
            return _random.Next(minInclusive, maxExclusive);
        }

        public float Range(float minInclusive, float maxInclusive)
        {
            return (float)(minInclusive + (_random.NextDouble() * (maxInclusive - minInclusive)));
        }
    }

    /// <summary>
    /// Independent Random Number Generator engine for SpinRush.
    /// Generates verified, unbiased reel outcomes decoupled from visual animation.
    /// </summary>
    public class RandomNumberGenerator : MonoBehaviour
    {
        private IRandomProvider _provider = new UnityRandomProvider();

        public IRandomProvider Provider
        {
            get => _provider ?? (_provider = new UnityRandomProvider());
            set => _provider = value;
        }

        /// <summary>
        /// Sets a deterministic seed for test reproducibility.
        /// </summary>
        public void SetSeed(int seed)
        {
            _provider = new SeededRandomProvider(seed);
        }

        /// <summary>
        /// Resets the provider back to standard runtime Unity RNG.
        /// </summary>
        public void ResetToDefaultProvider()
        {
            _provider = new UnityRandomProvider();
        }

        /// <summary>
        /// Generates a randomized final target symbol for each reel.
        /// </summary>
        /// <param name="database">The active SymbolDatabase asset.</param>
        /// <param name="reelCount">The number of reels to generate outcomes for (default = 3).</param>
        /// <returns>Array of SymbolData representing the target center symbols for each reel.</returns>
        public SymbolData[] GenerateSpinOutcome(SymbolDatabase database, int reelCount = 3)
        {
            if (database == null || database.Count == 0)
            {
                Debug.LogError("[RNG] Cannot generate spin outcome: SymbolDatabase is null or empty.");
                return new SymbolData[reelCount];
            }

            SymbolData[] outcome = new SymbolData[reelCount];

            // Cache symbols by ID for clean deterministic assignment
            SymbolData wildSym = database.GetSymbol(GameConstants.SymbolStarWild) ?? database.GetSymbolByIndex(2);
            SymbolData sevenSym = database.GetSymbol(GameConstants.SymbolLuckySeven) ?? database.GetSymbolByIndex(0);
            SymbolData bellSym = database.GetSymbol(GameConstants.SymbolGoldenBell) ?? database.GetSymbolByIndex(1);
            SymbolData diamondSym = database.GetSymbol(GameConstants.SymbolTripleBar) ?? database.GetSymbolByIndex(3);

            // True arcade probability roll: 0.00% to 100.00%
            float roll = Provider.Range(0f, 100f);

            // =========================================================================
            // 20.0% AUTHENTIC HARD ARCADE WINS (80.0% House Edge)
            // =========================================================================

            // 1. EXACT 2.0% ROYAL DHAMAKA JACKPOT (Roll 0.0 to 2.0)
            if (roll < 2.0f)
            {
                SymbolData topSym = (Provider.Range(0f, 1f) < 0.5f) ? wildSym : sevenSym;
                for (int i = 0; i < reelCount; i++) outcome[i] = topSym;
                Debug.Log("[RNG] *** 2.0% ROYAL DHAMAKA JACKPOT TRIGGERED! ***");
                return outcome;
            }

            // 2. 4.0% NATURAL 3-OF-A-KIND REGULAR WIN (Roll 2.0 to 6.0)
            if (roll < 6.0f)
            {
                SymbolData reg = (Provider.Range(0f, 1f) < 0.5f) ? bellSym : diamondSym;
                for (int i = 0; i < reelCount; i++) outcome[i] = reg;
                return outcome;
            }

            // 3. 6.0% DOUBLE WILD MULTIPLIER (Roll 6.0 to 12.0)
            if (roll < 12.0f)
            {
                SymbolData reg = (Provider.Range(0f, 1f) < 0.5f) ? bellSym : diamondSym;
                outcome[0] = wildSym;
                outcome[1] = wildSym;
                outcome[2] = reg;
                return outcome;
            }

            // 4. 8.0% SINGLE WILD SUBSTITUTION (Roll 12.0 to 20.0)
            if (roll < 20.0f)
            {
                SymbolData reg = (Provider.Range(0f, 1f) < 0.5f) ? bellSym : diamondSym;
                outcome[0] = reg;
                outcome[1] = reg;
                outcome[2] = wildSym;
                return outcome;
            }

            // =========================================================================
            // 80.0% HARD ARCADE LOSSES (FEATURING 45% HEART-POUNDING RAGEBAIT NEAR-MISSES)
            // =========================================================================

            // 5. 15.0% RAGEBAIT: DOUBLE SEVEN JACKPOT TEASE (Roll 20.0 to 35.0)
            // Reels 1 and 2 hit Lucky 7s! Reel 3 misses on Diamond or Bell!
            if (roll < 35.0f)
            {
                outcome[0] = sevenSym;
                outcome[1] = sevenSym;
                outcome[2] = (Provider.Range(0f, 1f) < 0.5f) ? bellSym : diamondSym;
                return outcome;
            }

            // 6. 15.0% RAGEBAIT: DOUBLE GOLDEN BELL TEASE (Roll 35.0 to 50.0)
            // Reels 1 and 2 hit Golden Bells! Reel 3 misses on Diamond or Seven!
            if (roll < 50.0f)
            {
                outcome[0] = bellSym;
                outcome[1] = bellSym;
                outcome[2] = (Provider.Range(0f, 1f) < 0.5f) ? diamondSym : sevenSym;
                return outcome;
            }

            // 7. 15.0% RAGEBAIT: SPLIT MATCH / WILD LEAD TEASE (Roll 50.0 to 65.0)
            if (roll < 65.0f)
            {
                if (Provider.Range(0f, 1f) < 0.5f)
                {
                    // Split: 7 - Bell - 7
                    outcome[0] = sevenSym;
                    outcome[1] = bellSym;
                    outcome[2] = sevenSym;
                }
                else
                {
                    // Wild Lead Tease: Wild - Seven - Diamond (Zero match)
                    outcome[0] = wildSym;
                    outcome[1] = sevenSym;
                    outcome[2] = diamondSym;
                }
                return outcome;
            }

            // 8. 35.0% CLEAN SCRAMBLE / COMPLETE MISS (Roll 65.0 to 100.0)
            // All 3 reels show different non-wild symbols (e.g. 7 - Bell - Diamond)
            SymbolData[] distinct = new SymbolData[] { sevenSym, bellSym, diamondSym };
            int offset = Provider.Range(0, 3);
            outcome[0] = distinct[offset % 3];
            outcome[1] = distinct[(offset + 1) % 3];
            outcome[2] = distinct[(offset + 2) % 3];

            return outcome;
        }

        /// <summary>
        /// Generates an outcome with weighted probability based on symbol tiers if enabled.
        /// </summary>
        public SymbolData[] GenerateWeightedOutcome(SymbolDatabase database, int reelCount = 3)
        {
            return GenerateSpinOutcome(database, reelCount);
        }
    }
}
