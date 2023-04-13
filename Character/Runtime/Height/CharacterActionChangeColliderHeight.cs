using System;
using MisterGames.Character.Access;
using MisterGames.Character.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Character.Height {

    [Serializable]
    public sealed class CharacterActionChangeColliderHeight : ICharacterAction {

        [Header("Parameters to change")]
        public Optional<float> targetHeight;
        public Optional<float> targetRadius;

        [Header("Change pattern")]
        [Min(0f)] public float duration;
        public bool scaleDuration;
        [SerializeReference] [SubclassSelector] public ICharacterHeightChangePattern pattern;

        public void Apply(object source, ICharacterAccess characterAccess) {
            float height = targetHeight.HasValue ? targetHeight.Value : characterAccess.HeightPipeline.TargetHeight;
            float radius = targetRadius.HasValue ? targetRadius.Value : characterAccess.HeightPipeline.TargetRadius;

            characterAccess.HeightPipeline.ApplyHeightChange(height, radius, duration, scaleDuration, pattern);
        }
    }

}
