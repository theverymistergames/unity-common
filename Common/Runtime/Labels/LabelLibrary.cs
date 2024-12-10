using System;
using System.Collections.Generic;
using MisterGames.Common.Labels.Base;
using UnityEngine;

namespace MisterGames.Common.Labels {
    
    [CreateAssetMenu(fileName = nameof(LabelLibrary), menuName = "MisterGames/Libs/" + nameof(LabelLibrary))]
    public sealed class LabelLibrary : LabelLibraryBase {
        
        internal const int Null = -2;
        internal const int None = -1;
        internal const string NoneLabel = "None";
        
        [SerializeField] private LabelArray[] _labelArrays;
        
        [HideInInspector]
        [SerializeField] private int _lastId;
        
        [Serializable]
        private struct LabelArray {

            [HideInInspector] public int id;
            public string name;
            public LabelArrayUsage usage;
            
            [TextArea]
            public string comment;
            
            [Space(10f)]
            public bool none;
            public LabelData[] labels;
        }

        [Serializable]
        internal struct LabelData {
            [HideInInspector] public int id;
            public string name;
        }
        
        private readonly Dictionary<int, (int, int)> _addressMap = new();
        private readonly Dictionary<int, int> _valueMap = new();

        public override Type GetDataType() {
            return null;
        }

        public override bool ContainsLabel(int id) {
            return GetAddress(id).index > Null;
        }

        public override string GetLabel(int id) {
            (int array, int index) = GetAddress(id);
            
            switch (index) {
                case Null:
                    return null;
                
                case None:
                    return NoneLabel;
                
                default:
                    ref var arr = ref _labelArrays[array];
                    return arr.labels[index].name;
            }
        }

        public override int GetValue(int id) {
#if UNITY_EDITOR
            if (_invalidateFlag) {
                _addressMap.Clear();
                _valueMap.Clear();
                _invalidateFlag = false;
            }
#endif

            if (_valueMap.TryGetValue(id, out int value)) return value;
            
            (int array, int index) = GetAddress(id);
            if (index < 0) return 0;
            
            ref var arr = ref _labelArrays[array];
            value = arr.usage switch {
                LabelArrayUsage.ByIndex => index + (arr.none ? 1 : 0),
                LabelArrayUsage.ByHash => Animator.StringToHash(arr.labels[index].name),
                _ => throw new ArgumentOutOfRangeException(),
            };
                    
            _valueMap[id] = value;
            return value;
        }

        public override int GetArraysCount() {
            return _labelArrays?.Length ?? 0;
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

        public override int GetArrayId(int array) {
            if (_labelArrays == null || _labelArrays.Length <= array) return default;

            ref var arr = ref _labelArrays[array];
            return arr.id;
        }

        public override int GetArrayIndex(int labelId) {
            return GetAddress(labelId).array;
        }

        public override int GetLabelsCount(int array) {
            if (_labelArrays == null || _labelArrays.Length <= array) return 0;

            ref var arr = ref _labelArrays[array];
            return arr.labels?.Length ?? 0;
        }

        public override int GetLabelId(int array, int index) {
            if (_labelArrays == null || _labelArrays.Length <= array) return default;

            ref var arr = ref _labelArrays[array];
            if (arr.labels == null || arr.labels.Length <= index) return default;
            
            return arr.labels[index].id;
        }

        public override int GetLabelIndex(int labelId) {
            return GetAddress(labelId).index;
        }

        private (int array, int index) GetAddress(int id) {
#if UNITY_EDITOR
            if (_invalidateFlag) {
                _addressMap.Clear();
                _valueMap.Clear();
                _invalidateFlag = false;
            }
#endif
            
            if (_addressMap.TryGetValue(id, out (int array, int index) address)) return address;

            address = (0, Null);
            if (id == 0) return address;
            
            int arrays = _labelArrays?.Length ?? 0;
            
            for (int i = 0; i < arrays; i++) {
                ref var array = ref _labelArrays![i];

                if (array.none) {
                    _addressMap[id] = (i, None); 
                    if (array.id == id) address = (i, None);
                }

                int labels = array.labels?.Length ?? 0;
                
                for (int j = 0; j < labels; j++) {
                    ref var label = ref array.labels![j];
                    
                    _addressMap[label.id] = (i, j);
                    if (label.id == id) address = (i, j);
                }
            }

            return address;
        }
        
#if UNITY_EDITOR
        private readonly HashSet<int> _occupiedIdsCache = new();
        private bool _invalidateFlag;
        
        private void OnValidate() {
            _invalidateFlag = true;
            _occupiedIdsCache.Clear();
            
            int arrays = _labelArrays?.Length ?? 0;

            for (int i = 0; i < arrays; i++) {
                ref var array = ref _labelArrays![i];
                
                if (array.id == 0 || _occupiedIdsCache.Contains(array.id)) array.id = GetNextId();
                _occupiedIdsCache.Add(array.id);
                
                int labels = array.labels?.Length ?? 0;
                
                for (int j = 0; j < labels; j++) {
                    ref var label = ref array.labels![j];

                    if (label.id == 0 || _occupiedIdsCache.Contains(label.id)) label.id = GetNextId();
                    _occupiedIdsCache.Add(label.id);
                }
            }
        }

        private int GetNextId() {
            if (_lastId == 0) _lastId++;
            return _lastId++;
        }
#endif
    }
    
}