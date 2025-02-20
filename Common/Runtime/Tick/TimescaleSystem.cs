using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Tick {
    
    public static class TimescaleSystem {

        private sealed class Comparer : IComparer<int> {
            public int Compare(int x, int y) => y.CompareTo(x);
        }
        
        private static readonly SortedDictionary<int, float> _timeScaleMap = new(new Comparer()); 
        
        public static void SetTimeScale(int priority, float timeScale) {
            _timeScaleMap[priority] = timeScale;
            Time.timeScale = GetTimeScale();
        }

        public static void RemoveTimeScale(int priority) {
            _timeScaleMap.Remove(priority);
            Time.timeScale = GetTimeScale();
        }

        private static float GetTimeScale() {
            foreach (float timeScale in _timeScaleMap.Values) {
                return timeScale;
            }

            return 1f;
        }
    }
    
}