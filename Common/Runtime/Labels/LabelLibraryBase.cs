using System;
using UnityEngine;

namespace MisterGames.Common.Labels.Base {
    
    public abstract class LabelLibraryBase : ScriptableObject {
        
        public abstract Type GetDataType();

        public abstract bool ContainsLabel(int id);
        public abstract string GetLabel(int id);
        public abstract int GetValue(int id);

        public abstract int GetArraysCount();
        public abstract string GetArrayName(int array);
        public abstract bool GetArrayNoneLabel(int array);
        public abstract LabelArrayUsage GetArrayUsage(int array);
        public abstract int GetArrayId(int array);
        public abstract int GetArrayIndex(int labelId);

        public abstract int GetLabelsCount(int array);
        public abstract int GetLabelId(int array, int index);
        public abstract int GetLabelIndex(int labelId);
    }
    
    public abstract class LabelLibraryBase<T> : LabelLibraryBase {

        public override Type GetDataType() {
            return typeof(T);
        }
        
        public abstract bool TryGetData(int id, out T data);
    }
    
}