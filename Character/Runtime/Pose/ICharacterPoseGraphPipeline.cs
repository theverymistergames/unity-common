using MisterGames.Character.Core;

namespace MisterGames.Character.Pose {

    public interface ICharacterPoseGraphPipeline : ICharacterPipeline {

        CharacterCapsuleSize GetDefaultCapsuleSize(CharacterPoseType pose);

        float GetDefaultTransitionDuration(CharacterPoseType targetPose);
    }

}
