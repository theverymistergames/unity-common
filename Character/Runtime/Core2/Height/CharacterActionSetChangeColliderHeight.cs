using System;
using MisterGames.Character.Core2.Modifiers;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Core2.Height {

    [Serializable]
    public sealed class CharacterActionSetChangeColliderHeight : ICharacterAction {

        [Min(0f)] public float targetHeight;
        [Min(0f)] public float duration;
        public bool scaleDuration;
        [SerializeReference] [SubclassSelector] public ICharacterHeightChangePattern pattern;

        public void Apply(object source, ICharacterAccess characterAccess) {
            characterAccess.HeightPipeline.ApplyHeightChange(targetHeight, duration, scaleDuration, pattern);
        }
    }

}
