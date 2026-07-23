using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Localization;
using UnityEngine;

namespace MisterGames.Logic.Loading {

    [CreateAssetMenu(fileName = nameof(LoadingTextPreset), menuName = "MisterGames/Dialogues/" + nameof(LoadingTextPreset))]
    public sealed class LoadingTextPreset : ScriptableObject {
        
        [Header("Elements")]
        [SerializeReference] [SubclassSelector] public ILocalizedStringProvider[] blocks;
        public Arguments[] args;
        [Min(0f)] public float printElementDelay = 0.3f;
        [Min(0f)] public float afterLoadDelay = 0.5f;
        [Min(0f)] public float finishDelay = 0.5f;
        
        [Header("Loading Progress")]
        public bool showProgress = true;
        public LocalizationKey loadingProgressKey;
        [Min(0)] public float progressSmoothing = 7f;
        [Min(0)] public int loadProgressCharsCount = 20;
        public char loadProgressEmptyChar = '░';
        public char loadProgressFullChar = '█';
        
        [Header("Print After Loading")]
        [SerializeReference] [SubclassSelector] public ILocalizedStringProvider[] afterLoading;

        [Header("Await Input")]
        public bool awaitInput = true; 
        public LocalizationKey awaitInputKey;
        [Min(0f)] public float dotPrintDelay = 0.15f;
        [Min(0f)] public float dotPrintRestartDelay = 0.7f;
        [Min(0f)] public int dotsCount = 3;
        public char dotChar = '.';
        
        [Serializable]
        public struct Arguments {
            public LocalizationKey key;
            [SerializeReference] [SubclassSelector] public IArgumentResolver resolver;
        }
    }
    
}