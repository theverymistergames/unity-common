using System;
using MisterGames.Common.Attributes;
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
        [SerializeReference] [SubclassSelector] public ILocalizedStringProvider[] blocks;
        public Arguments[] args;

        [Header("Loading")]
        public LocalizationKey loadingProgressKey;
        [Min(0)] public int loadProgressCharsCount = 20;
        public char loadProgressEmptyChar = '░';
        public char loadProgressFullChar = '█';
        
        [Serializable]
        public struct Arguments {
            public LocalizationKey key;
            [SerializeReference] [SubclassSelector] public IArgumentResolver resolver;
        }
    }
    
}