using System;
using UnityEngine;

namespace StrategyGame.Utils {
    public class DeterministicRNG {
        private System.Random _rng;
        private int _rollCount;
        
        public int RollCount => _rollCount;
        
        public DeterministicRNG(int seed) {
            _rng = new System.Random(seed);
        }

        /// <summary>
        /// Returns [0,1)
        /// </summary>
        public float Value() {
            _rollCount++;
            return (float)_rng.NextDouble();
        }

        /// <summary>
        /// Returns [min,max)
        /// </summary>
        public int Range(int min, int max) {
            if (max < min) {
                throw new ArgumentException("DeterministicRNG.Range(int): max must be greater than or equal to min");
            }
            
            _rollCount++;
            return _rng.Next(min, max);
        }

        /// <summary>
        /// Returns [min,max)
        /// </summary>
        public float Range(float min, float max) {
            if (max < min) {
                throw new ArgumentException("DeterministicRNG.Range(float): max must be greater than or equal to than min");
            }
            
            return min + Value() * (max - min);
        }

        /// <summary>
        /// Does a roll, returning if an event of the given probability will occur.
        /// </summary>
        /// <param name="probability">Chance for event to occur.</param>
        /// <returns>If event will occur, true. Else, false.</returns>
        public bool Chance(float probability) {
            return Value() < Mathf.Clamp01(probability);
        }
    }
}
