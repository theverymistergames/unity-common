using System;
using System.Collections.Generic;
using MisterGames.Common.Labels.Base;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Common.Labels {
    
    public abstract class LabelLibrary<T> : LabelLibraryBase<T> {

        [SerializeField] private LabelArray[] _labelArrays;

        [HideInInspector]
        [SerializeField] private int _lastId;
        
        [Serializable]
        private struct LabelArray {

            [HideInInspector] public int id;
            
            public string name;
            [TextArea]
            public string comment;
            
            [Space(10f)]
            public LabelArrayUsage usage;
            public bool none;
            public LabelData[] labels;
        }

        [Serializable]
        private struct LabelData {
            [HideInInspector] public int id;
            public string name;
            public T data;
        }

        private readonly Dictionary<int, (int arrayIndex, int labelIndex)> _labelIdToAddressMap = new();
        private readonly Dictionary<int, int> _arrayIdToIndexMap = new();
        private readonly Dictionary<int, int> _valueMap = new();

        public override bool ContainsLabel(int labelId) {
            return GetLabelAddress(labelId).labelIndex > LabelLibrary.Null;
        }

        public override string GetLabel(int labelId) {
            (int array, int index) = GetLabelAddress(labelId);
            
            switch (index) {
                case LabelLibrary.Null:
                    return null;
                
                case LabelLibrary.None:
                    return LabelLibrary.NoneLabel;
                
                default:
                    ref var arr = ref _labelArrays[array];
                    return arr.labels[index].name;
            }
        }

        public override int GetValue(int labelId) {
#if UNITY_EDITOR
            if (_invalidateFlag) {
                _arrayIdToIndexMap.Clear();
                _valueMap.Clear();
                _invalidateFlag = false;
            }
#endif

            if (_valueMap.TryGetValue(labelId, out int value)) return value;
            
            (int array, int index) = GetLabelAddress(labelId);
            if (index < 0) return 0;
            
            ref var arr = ref _labelArrays[array];
            value = arr.usage switch {
                LabelArrayUsage.ByIndex => index + (arr.none ? 1 : 0),
                LabelArrayUsage.ByHash => Animator.StringToHash(arr.labels[index].name),
                _ => throw new ArgumentOutOfRangeException(),
            };
                    
            _valueMap[labelId] = value;
            return value;
        }

        public override bool TryGetData(int id, out T data) {
            (int array, int index) = GetLabelAddress(id);
            
            if (index < 0) {
                data = default;
                return false;
            }

            ref var arr = ref _labelArrays[array];
            data = arr.labels[index].data;

            return true;
        }
        
        public override bool TrySetData(int id, T data) {
            return false;
        }

        public override bool ClearData(int id) {
            return false;
        }
        
        public override int GetArraysCount() {
            return _labelArrays?.Length ?? 0;
        }

        public override string GetArrayName(int arrayIndex) {
            if (_labelArrays == null || _labelArrays.Length <= arrayIndex) return default;

            ref var arr = ref _labelArrays[arrayIndex];
            return arr.name;
        }

        public override bool GetArrayNoneLabel(int arrayIndex) {
            if (_labelArrays == null || _labelArrays.Length <= arrayIndex) return default;

            ref var arr = ref _labelArrays[arrayIndex];
            return arr.none;
        }

        public override int GetArrayId(int arrayIndex) {
            if (_labelArrays == null || _labelArrays.Length <= arrayIndex) return default;

            ref var arr = ref _labelArrays[arrayIndex];
            return arr.id;
        }
        
        public override int GetArrayIndex(int arrayId) {
            return GetArrayIndexById(arrayId);
        }
        
        public override LabelArrayUsage GetArrayUsage(int arrayIndex) {
            if (_labelArrays == null || _labelArrays.Length <= arrayIndex) return default;

            ref var arr = ref _labelArrays[arrayIndex];
            return arr.usage;
        }

        public override int GetLabelArrayIndex(int labelId) {
            return GetLabelAddress(labelId).arrayIndex;
        }

        public override int GetArrayLabelsCount(int arrayIndex) {
            if (_labelArrays == null || _labelArrays.Length <= arrayIndex) return 0;

            ref var arr = ref _labelArrays[arrayIndex];
            return arr.labels?.Length ?? 0;
        }

        public override int GetLabelId(int arrayIndex, int labelIndex) {
            if (_labelArrays == null || _labelArrays.Length <= arrayIndex) return default;

            ref var arr = ref _labelArrays[arrayIndex];
            if (arr.labels == null || arr.labels.Length <= labelIndex) return default;
            
            return arr.labels[labelIndex].id;
        }
        
        public override int GetLabelIndex(int labelId) {
            return GetLabelAddress(labelId).labelIndex;
        }

        private (int arrayIndex, int labelIndex) GetLabelAddress(int labelId) {
#if UNITY_EDITOR
            ClearMapsIfInvalid();
#endif
            
            if (_labelIdToAddressMap.TryGetValue(labelId, out var address)) return address;

            address = (0, LabelLibrary.Null);
            if (labelId == 0) return address;
            
            FetchAddresses();

            return _labelIdToAddressMap.GetValueOrDefault(labelId, address);
        }
        
        private int GetArrayIndexById(int arrayId) {
#if UNITY_EDITOR
            ClearMapsIfInvalid();
#endif
            
            if (_arrayIdToIndexMap.TryGetValue(arrayId, out int index)) return index;

            index = -1;
            if (arrayId == 0) return index;
            
            FetchAddresses();

            return _arrayIdToIndexMap.GetValueOrDefault(arrayId, -1);
        }

        private void FetchAddresses() {
            int arrays = _labelArrays?.Length ?? 0;
            
            for (int i = 0; i < arrays; i++) {
                ref var array = ref _labelArrays![i];
                _arrayIdToIndexMap[array.id] = i;
                
                if (array.none) {
                    _labelIdToAddressMap[array.id] = (i, LabelLibrary.None); 
                }

                int labels = array.labels?.Length ?? 0;
                
                for (int j = 0; j < labels; j++) {
                    ref var label = ref array.labels![j];
                    _labelIdToAddressMap[label.id] = (i, j);
                }
            }
        }
        
#if UNITY_EDITOR
        private readonly HashSet<int> _occupiedIdsCache = new();
        private bool _invalidateFlag;
        
        private void ClearMapsIfInvalid() {
            if (!_invalidateFlag) return;
            
            _labelIdToAddressMap.Clear();
            _arrayIdToIndexMap.Clear();
            _valueMap.Clear();
            _invalidateFlag = false;
        }
        
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
            if (_lastId == 0) _lastId.IncrementUncheckedRef();
            return _lastId.IncrementUncheckedRef();
        }
#endif
    }
    
}