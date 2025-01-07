using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Capsule;
using MisterGames.Common.Data;

namespace MisterGames.ActionLib.Character {

    [Serializable]
    public sealed class CharacterConditionPose : IActorCondition {

        public Optional<CharacterPose> equalsCurrentPose;
        public Optional<CharacterPose> equalsTargetPose;

        public bool IsMatch(IActor context, float startTime) {
            var pose = context.GetComponent<CharacterPosePipeline>();
            return equalsCurrentPose.IsEmptyOrEquals(pose.CurrentPose) &&
                   equalsTargetPose.IsEmptyOrEquals(pose.TargetPose);
        }
    }

}
