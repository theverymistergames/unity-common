using System;
using MisterGames.Common.Localization;
using UnityEngine;

namespace MisterGames.Logic.Loading {

    [CreateAssetMenu(fileName = nameof(LoadingTextPreset), menuName = "MisterGames/Dialogues/" + nameof(LoadingTextPreset))]
    public sealed class LoadingTextPreset : ScriptableObject {
        
        [Header("Meta")]
        public LocalizationKey dialogueId;
        public LocalizationKey roleId;
        public LocalizationKey branchId;
        
        [Header("Elements")]
        public Group[] groups;
        public Variable<LocalizationKey>[] localizedVariables;
        public Variable<string>[] stringVariables;
        
        [Serializable]
        public struct Group {
            [Tooltip("Random variant from this list will be selected")]
            public LocalizationKey[] variants;
            [Tooltip("Format selected variant with one or more variables")]
            public LocalizationKey[] variables;
        }
        
        [Serializable]
        public struct Variable<T> {
            public LocalizationKey key;
            public T[] values;
        }
    }
    
}