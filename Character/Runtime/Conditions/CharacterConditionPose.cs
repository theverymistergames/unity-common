using System;
using MisterGames.Character.Core;
using MisterGames.Character.Capsule;
using MisterGames.Common.Data;

namespace MisterGames.Character.Conditions {

    [Serializable]
    public sealed class CharacterConditionPose : ICharacterCondition {

        public Optional<CharacterPoseType> equalsCurrentPose;
        public Optional<CharacterPoseType> equalsTargetPose;

        public bool IsMatch(ICharacterAccess context) {
            var pose = context.GetPipeline<ICharacterPosePipeline>();
            return equalsCurrentPose.IsEmptyOrEquals(pose.CurrentPose) &&
                   equalsTargetPose.IsEmptyOrEquals(pose.TargetPose);
        }
    }

}
