using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Labels {
    
    [CreateAssetMenu(fileName = nameof(LabelLibrary), menuName = "MisterGames/" + nameof(LabelLibrary))]
    public sealed class LabelLibrary : ScriptableObject {
        
        private const int Null = -2;
        private const int None = -1;
        private const string NoneLabel = "None";
        
        public LabelArray[] labelArrays;

        public enum Usage {
            ByIndex,
            ByHash,
        }
        
        [Serializable]
        public struct LabelArray {
            
            public string name;
            public Usage usage;
            
            [TextArea]
            public string comment;
            
            [Space(10f)]
            public bool none;
            public string[] labels;
        }
        
        private readonly Dictionary<(int, int), int> _indexMap = new();

        public string GetLabel(int array, int value) {
            int address = GetIndex(array, value);
            
            switch (address) {
                case Null:
                    return null;
                
                case None:
                    return NoneLabel;
                
                default:
                    ref var arr = ref labelArrays[array];
                    return arr.labels[address];
            }
        }

        public bool ContainsLabel(int array, int value) {
            return GetIndex(array, value) > Null;
        }
        
        private int GetIndex(int array, int value) {
#if UNITY_EDITOR
            if (_isInvalid) {
                _indexMap.Clear();
                _isInvalid = false;
            }
#endif
            
            if (_indexMap.TryGetValue((array, value), out int index)) return index;

            int arrays = labelArrays?.Length ?? 0;
            if (array < 0 || array >= arrays) return Null;
            
            ref var labelArray = ref labelArrays![array];
            if (labelArray.none && value == 0) {
                _indexMap[(array, value)] = None;
                return None;
            }

            switch (labelArray.usage) {
                case Usage.ByIndex:
                    int offset = labelArray.none ? 1 : 0;
                    if (value >= offset && value - offset < labelArray.labels?.Length) {
                        _indexMap[(array, value)] = value - offset;
                        return value - offset;
                    }
                    break;
                
                case Usage.ByHash:
                    for (int i = 0; i < labelArray.labels?.Length; i++) {
                        if (Animator.StringToHash(labelArray.labels[i]) == value) return i;
                    }
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Null;
        }
        
#if UNITY_EDITOR
        private bool _isInvalid;
        private void OnValidate() => _isInvalid = true;
#endif
    }
    
}