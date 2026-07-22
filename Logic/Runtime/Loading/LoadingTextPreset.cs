using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Localization;
using UnityEngine;

namespace MisterGames.Logic.Loading {

    [CreateAssetMenu(fileName = nameof(LoadingTextPreset), menuName = "MisterGames/Dialogues/" + nameof(LoadingTextPreset))]
    public sealed class LoadingTextPreset : ScriptableObject {
        
        public LocalizationKey dialogueId;
        public LocalizationKey roleId;
        public LocalizationKey branchId;
        
        [Space]
        public LocalizationKey[] elements;
        [Space]
        public Arguments[] args;
        
        [Serializable]
        public struct Arguments {
            public LocalizationKey key;
            [SerializeReference] [SubclassSelector] public IArgumentResolver resolver;
        }
    }
    
}