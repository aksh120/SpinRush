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
            for (int i = 0; i < reelCount; i++)
            {
                int randomIndex = Provider.Range(0, database.Count);
                outcome[i] = database.GetSymbolByIndex(randomIndex);
            }

            return outcome;
        }

        /// <summary>
        /// Generates an outcome with weighted probability based on symbol tiers if enabled.
        /// </summary>
        public SymbolData[] GenerateWeightedOutcome(SymbolDatabase database, int reelCount = 3)
        {
            if (database == null || database.Count == 0)
            {
                Debug.LogError("[RNG] Cannot generate weighted outcome: SymbolDatabase is null or empty.");
                return new SymbolData[reelCount];
            }

            // Weights inversely proportional to payout / tier:
            // Base Tier (SYM_04: 10x) -> weight 40
            // Mid Tier (SYM_03: Wild) -> weight 25
            // High Tier (SYM_02: 25x) -> weight 25
            // Jackpot Tier (SYM_01: 50x) -> weight 10
            int[] weights = new int[database.Count];
            int totalWeight = 0;

            for (int i = 0; i < database.Count; i++)
            {
                var sym = database.GetSymbolByIndex(i);
                int w = 25; // default
                if (sym != null)
                {
                    switch (sym.Tier)
                    {
                        case 1: w = 10; break;
                        case 2: w = 25; break;
                        case 3: w = 25; break;
                        case 4: w = 40; break;
                    }
                }
                weights[i] = w;
                totalWeight += w;
            }

            SymbolData[] outcome = new SymbolData[reelCount];
            for (int r = 0; r < reelCount; r++)
            {
                int roll = Provider.Range(0, totalWeight);
                int accum = 0;
                SymbolData selected = database.GetSymbolByIndex(0);

                for (int i = 0; i < database.Count; i++)
                {
                    accum += weights[i];
                    if (roll < accum)
                    {
                        selected = database.GetSymbolByIndex(i);
                        break;
                    }
                }
                outcome[r] = selected;
            }

            return outcome;
        }
    }
}
