using System;
using MisterGames.Character.Core;
using MisterGames.Character.Pose;
using MisterGames.Common.Data;

namespace MisterGames.Character.Conditions {

    [Serializable]
    public sealed class CharacterConditionPose : ICharacterCondition {

        public Optional<CharacterPoseType> equalsCurrentPose;
        public Optional<CharacterPoseType> equalsTargetPose;

        public bool IsMatch(ICharacterAccess context) {
            var capsule = context.GetPipeline<ICharacterCapsulePipeline>();
            return equalsCurrentPose.IsEmptyOrEquals(capsule.CurrentPose) &&
                   equalsTargetPose.IsEmptyOrEquals(capsule.TargetPose);
        }
    }

}
