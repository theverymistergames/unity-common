﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Labels {
    
    [CreateAssetMenu(fileName = nameof(LabelLibrary), menuName = "MisterGames/Libs/" + nameof(LabelLibrary))]
    public sealed class LabelLibrary : LabelLibraryBase {
        
        internal const int Null = -2;
        internal const int None = -1;
        internal const string NoneLabel = "None";
        
        [SerializeField] private LabelArray[] _labelArrays;

        [Serializable]
        private struct LabelArray {
            
            public string name;
            public LabelArrayUsage usage;
            
            [TextArea]
            public string comment;
            
            [Space(10f)]
            public bool none;
            public string[] labels;
        }
        
        private readonly Dictionary<(int, int), int> _indexMap = new();

        public override string GetLabel(int array, int value) {
            int address = GetIndex(array, value);
            
            switch (address) {
                case Null:
                    return null;
                
                case None:
                    return NoneLabel;
                
                default:
                    ref var arr = ref _labelArrays[array];
                    return arr.labels[address];
            }
        }

        public override bool ContainsLabel(int array, int value) {
            return GetIndex(array, value) > Null;
        }

        public override Type GetDataType() {
            return null;
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
            
            return arr.labels[index];
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
            if (array < 0 || array >= arrays) return Null;
            
            ref var labelArray = ref _labelArrays![array];
            if (labelArray.none && value == 0) {
                _indexMap[(array, value)] = None;
                return None;
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