using MisterGames.Character.Core;

namespace MisterGames.Character.Pose {

    public interface ICharacterPoseGraphPipeline : ICharacterPipeline {
        CharacterCapsuleSize GetCapsuleSize(CharacterPoseType pose);

        float GetDefaultTransitionDuration(CharacterPoseType targetPose);
    }

}
