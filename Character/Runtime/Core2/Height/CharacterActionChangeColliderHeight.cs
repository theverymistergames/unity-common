using System;
using MisterGames.Character.Core2.Access;
using MisterGames.Character.Core2.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Character.Core2.Height {

    [Serializable]
    public sealed class CharacterActionChangeColliderHeight : ICharacterAction {

        [Header("Parameters to change")]
        [Min(0f)] public Optional<float> targetHeight;
        [Min(0f)] public Optional<float> targetRadius;

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
