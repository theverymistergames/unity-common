using System;
using UnityEngine;

namespace MisterGames.Common.Data {
    
    [Serializable]
    public struct CompareInt {
        
        [SerializeField] private CompareMode _compareMode;
        [SerializeField] private int[] _values;
        [SerializeField] private int _value;
        
        public bool IsMatch(int value) {
            if (_compareMode is not (CompareMode.Equal or CompareMode.NotEqual)) {
                return _compareMode.IsMatch(value, value);
            }
            
            for (int i = 0; i < _values.Length; i++) {
                if (_compareMode.IsMatch(value, _values[i])) return true;
            }
            
            return false;
        }
    }
    
}