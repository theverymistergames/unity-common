using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Localization;
using UnityEngine;

namespace MisterGames.Logic.UI {

    [CreateAssetMenu(fileName = nameof(PlainTextPreset), menuName = "MisterGames/Dialogues/" + nameof(PlainTextPreset))]
    public sealed class PlainTextPreset : ScriptableObject {
        
        [Header("Elements")]
        public bool waitSkipInputAfterBlock;
        [SerializeReference] [SubclassSelector] public ILocalizedStringProvider[] blocks;
        public Arguments[] args;
        
        [Serializable]
        public struct Arguments {
            public LocalizationKey[] keys;
            [SerializeReference] [SubclassSelector] public IArgumentResolver resolver;
        }
    }
    
}