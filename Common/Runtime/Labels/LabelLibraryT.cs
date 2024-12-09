using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Labels {
    
    public abstract class LabelLibrary<T> : LabelLibraryBaseT<T> {

        [SerializeField] private LabelArray[] _labelArrays;

        [Serializable]
        private struct LabelArray {
            
            public string name;
            public LabelArrayUsage usage;
            
            [TextArea]
            public string comment;
            
            [Space(10f)]
            public bool none;
            public LabelData[] labels;
        }

        [Serializable]
        private struct LabelData {
            public string name;
            public T data;
        }
        
        private readonly Dictionary<(int, int), int> _indexMap = new();

        public override string GetLabel(int array, int value) {
            int address = GetIndex(array, value);
            
            switch (address) {
                case LabelLibrary.Null:
                    return null;
                
                case LabelLibrary.None:
                    return LabelLibrary.NoneLabel;
                
                default:
                    ref var arr = ref _labelArrays[array];
                    return arr.labels[address].name;
            }
        }

        public override bool ContainsLabel(int array, int value) {
            return GetIndex(array, value) > LabelLibrary.Null;
        }
        
        public override bool TryGetData(int array, int value, out T data) {
            int address = GetIndex(array, value);

            if (address < 0) {
                data = default;
                return false;
            }
            
            ref var arr = ref _labelArrays[array];
            data = arr.labels[address].data;
            return true;
        }
        
        public override int GetArraysCount() {
            return _labelArrays?.Length ?? 0;
        }

        public override int GetLabelsCount(int array) {
            if (_labelArrays == null || _labelArrays.Length <= array) return 0;

            ref var arr = ref _labelArrays[array];
            return arr.labels?.Length ?? 0;
        }

        public override string GetLabelByIndex(int array, int index) {
            if (_labelArrays == null || _labelArrays.Length <= array) return default;

            ref var arr = ref _labelArrays[array];
            if (arr.labels == null || arr.labels.Length <= index) return default;
            
            return arr.labels[index].name;
        }

        public override string GetArrayName(int array) {
            if (_labelArrays == null || _labelArrays.Length <= array) return default;

            ref var arr = ref _labelArrays[array];
            return arr.name;
        }

        public override bool GetArrayNoneLabel(int array) {
            if (_labelArrays == null || _labelArrays.Length <= array) return default;

            ref var arr = ref _labelArrays[array];
            return arr.none;
        }

        public override LabelArrayUsage GetArrayUsage(int array) {
            if (_labelArrays == null || _labelArrays.Length <= array) return default;

            ref var arr = ref _labelArrays[array];
            return arr.usage;
        }

        private int GetIndex(int array, int value) {
#if UNITY_EDITOR
            if (_isInvalid) {
                _indexMap.Clear();
                _isInvalid = false;
            }
#endif
            
            if (_indexMap.TryGetValue((array, value), out int index)) return index;

            int arrays = _labelArrays?.Length ?? 0;
            if (array < 0 || array >= arrays) return LabelLibrary.Null;
            
            ref var labelArray = ref _labelArrays![array];
            if (labelArray.none && value == 0) {
                _indexMap[(array, value)] = LabelLibrary.None;
                return LabelLibrary.None;
            }

            switch (labelArray.usage) {
                case LabelArrayUsage.ByIndex:
                    int offset = labelArray.none ? 1 : 0;
                    if (value >= offset && value - offset < labelArray.labels?.Length) {
                        _indexMap[(array, value)] = value - offset;
                        return value - offset;
                    }
                    break;
                
                case LabelArrayUsage.ByHash:
                    for (int i = 0; i < labelArray.labels?.Length; i++) {
                        if (Animator.StringToHash(labelArray.labels[i].name) == value) return i;
                    }
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return LabelLibrary.Null;
        }
        
#if UNITY_EDITOR
        private bool _isInvalid;
        private void OnValidate() => _isInvalid = true;
#endif
    }
    
}