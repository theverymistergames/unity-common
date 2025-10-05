using System;
using UnityEngine;

namespace MisterGames.Common.Labels.Base {
    
    public abstract class LabelLibraryBase : ScriptableObject {
        
        public abstract Type GetDataType();

        public abstract bool ContainsLabel(int labelId);
        public abstract string GetLabel(int labelId);
        public abstract int GetValue(int labelId);

        public abstract int GetArraysCount();
        public abstract string GetArrayName(int arrayIndex);
        public abstract bool GetArrayNoneLabel(int arrayIndex);
        public abstract LabelArrayUsage GetArrayUsage(int arrayIndex);
        public abstract int GetArrayId(int arrayIndex);
        public abstract int GetArrayIndex(int arrayId);
        public abstract int GetArrayLabelsCount(int arrayIndex);
        
        public abstract int GetLabelId(int arrayIndex, int labelIndex);
        public abstract int GetLabelIndex(int labelId);
        public abstract int GetLabelArrayIndex(int labelId);
    }
    
    public abstract class LabelLibraryBase<T> : LabelLibraryBase {

        public sealed override Type GetDataType() => typeof(T);
        
        public abstract bool TryGetData(int id, out T data);
        public abstract bool TrySetData(int id, T data);
        public abstract bool ClearData(int id);
    }
    
}