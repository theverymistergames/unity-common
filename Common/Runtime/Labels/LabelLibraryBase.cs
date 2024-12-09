using System;
using UnityEngine;

namespace MisterGames.Common.Labels {
    
    public abstract class LabelLibraryBase : ScriptableObject {

        public abstract string GetLabel(int array, int value);
        public abstract bool ContainsLabel(int array, int value);

        public abstract Type GetDataType();
        public abstract int GetArraysCount();
        public abstract int GetLabelsCount(int array);
        public abstract string GetLabelByIndex(int array, int index);
        public abstract string GetArrayName(int array);
        public abstract bool GetArrayNoneLabel(int array);
        public abstract LabelArrayUsage GetArrayUsage(int array);
    }
    
}