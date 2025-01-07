using System;
using UnityEngine;

namespace MisterGames.Common.Data {
    
    public static class CompareModeExtensions {

        public static bool IsMatch(this CompareMode compareMode, int a, int b) {
            return compareMode switch {
                CompareMode.Equal => a == b,
                CompareMode.NotEqual => a != b,
                CompareMode.Less => a < b,
                CompareMode.Greater => a > b,
                CompareMode.LessOrEqual => a <= b,
                CompareMode.GreaterOrEqual => a >= b,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public static bool IsMatch(this CompareMode compareMode, float a, float b) {
            return compareMode switch {
                CompareMode.Equal => Mathf.Approximately(a, b),
                CompareMode.NotEqual => !Mathf.Approximately(a, b),
                CompareMode.Less => a < b,
                CompareMode.Greater => a > b,
                CompareMode.LessOrEqual => a <= b,
                CompareMode.GreaterOrEqual => a >= b,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

}