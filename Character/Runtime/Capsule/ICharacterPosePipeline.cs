using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Conditions;
using MisterGames.Character.Core;

namespace MisterGames.Character.Capsule {

    public delegate void OnPoseChanged(CharacterPoseType newPose, CharacterPoseType oldPose);

    public interface ICharacterPosePipeline : ICharacterPipeline {

        /// <summary>
        /// Called during pose change or when current pose is set directly.
        /// </summary>
        event OnPoseChanged OnPoseChanged;

        /// <summary>
        /// Current character pose.
        /// When set, pose is changed immediately, capsule size does not change.
        /// </summary>
        CharacterPoseType CurrentPose { get; set; }

        /// <summary>
        /// Target character pose is set when pose change begins.
        /// </summary>
        CharacterPoseType TargetPose { get; }

        /// <summary>
        /// Current character height and radius.
        /// When set, size is changed immediately, character pose does not change.
        /// </summary>
        CharacterCapsuleSize CurrentCapsuleSize { get; set; }

        /// <summary>
        /// Current character height and radius.
        /// When set, size is changed immediately, character pose does not change.
        /// </summary>
        CharacterCapsuleSize TargetCapsuleSize { get; }

        /// <summary>
        /// Change character pose and capsule size linearly within duration.
        /// Returns true if size change is done, otherwise false.
        /// </summary>
        UniTask<bool> TryChangePose(
            CharacterPoseType targetPose,
            CharacterCapsuleSize capsuleSize,
            float duration,
            float setTargetPoseAt = 0f,
            ICharacterCondition canContinue = null,
            CancellationToken cancellationToken = default
        );

        void StopPoseChange();
    }

}
