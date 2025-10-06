using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;

namespace MisterGames.Common.Labels {
    
    [Serializable]
    public struct LabelValueMap<TValue> {
        
        public LabelArray labelArray;
        public Value[] values;

        [Serializable]
        public struct Value {
            [LabelValueVisibility(lib: false, array: false)]
            [ReadOnly] public LabelValue label;
            public Optional<TValue> value;
        }
    }
    
    [Serializable]
    public struct LabelValueMap<TLibData, TValue> {
        
        public LabelArray<TLibData> labelArray;
        public Value[] values;

        [Serializable]
        public struct Value {
            [LabelValueVisibility(lib: false, array: false)]
            [ReadOnly] public LabelValue<TLibData> label;
            public Optional<TValue> value;
        }
    }
    
}