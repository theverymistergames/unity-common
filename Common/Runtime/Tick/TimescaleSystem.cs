using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Tick {
    
    public static class TimescaleSystem {

        private sealed class Comparer : IComparer<Key> {
            public int Compare(Key x, Key y) => y.priority.CompareTo(x.priority);
        }

        private readonly struct Key : IEquatable<Key> {
            
            public readonly int hash;
            public readonly int priority;
            
            public Key(int hash, int priority) {
                this.hash = hash;
                this.priority = priority;
            }
            
            public bool Equals(Key other) => hash == other.hash;
            public override bool Equals(object obj) => obj is Key other && Equals(other);
            public override int GetHashCode() => hash;
            public static bool operator ==(Key left, Key right) => left.Equals(right);
            public static bool operator !=(Key left, Key right) => !left.Equals(right);
        }
        
        private static readonly SortedDictionary<Key, float> _timeScaleMap = new(new Comparer()); 
        private static readonly Dictionary<int, int> _priorityMap = new(); 
        
        public static void SetTimeScale(object source, int priority, float timeScale) {
            int hash = source.GetHashCode();

            if (_priorityMap.TryGetValue(hash, out int oldPriority)) {
                _timeScaleMap.Remove(new Key(hash, oldPriority));
            }
            
            _priorityMap[hash] = priority;
            _timeScaleMap[new Key(hash, priority)] = timeScale;
            
            Time.timeScale = GetTimeScale();
        }

        public static void RemoveTimeScale(object source) {
            int hash = source.GetHashCode();
            if (!_priorityMap.Remove(hash, out int priority)) return;

            _timeScaleMap.Remove(new Key(hash, priority));
            
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