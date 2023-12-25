using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.Character.Pose {

    public delegate void ProgressCallback(
        float progress,
        float totalDuration
    );

    public interface ICharacterCapsulePipeline : ICharacterPipeline {

        /// <summary>
        /// Notifies every frame while height is being changed.
        /// </summary>
        event ProgressCallback OnHeightChange;

        /// <summary>
        /// Notifies every frame while pose is being changed.
        /// </summary>
        event ProgressCallback OnPoseChange;

        /// <summary>
        /// Current character pose.
        /// When the property is set, any running pose change is cancelled.
        /// </summary>
        CharacterPoseType CurrentPose { get; set; }

        /// <summary>
        /// Target character pose, is set when pose is about to change.
        /// </summary>
        CharacterPoseType TargetPose { get; }

        /// <summary>
        /// Current character capsule height.
        /// When the property is set, any running pose change is cancelled.
        /// </summary>
        float CurrentHeight { get; set; }

        /// <summary>
        /// Target character capsule height, is set when height is about to change.
        /// </summary>
        float TargetHeight { get; }

        /// <summary>
        /// Current character capsule radius.
        /// When the property is set, any running pose change is cancelled.
        /// </summary>
        float CurrentRadius { get; set; }

        /// <summary>
        /// Target character capsule radius, is set when radius is about to change.
        /// </summary>
        float TargetRadius { get; }

        /// <summary>
        /// Top point of the character capsule collider.
        /// </summary>
        Vector3 ColliderTop { get; }

        /// <summary>
        /// Center point of the character capsule collider.
        /// </summary>
        Vector3 ColliderCenter { get; }

        /// <summary>
        /// Bottom point of the character capsule collider.
        /// </summary>
        Vector3 ColliderBottom { get; }

        /// <summary>
        /// Apply pose async with custom capsule size within duration.
        /// Target pose is set when operation progress >= changePoseAtProgress or 1.
        /// </summary>
        UniTask ChangePose(
            CharacterPoseType targetPose,
            CharacterCapsuleSize capsuleSize,
            float duration,
            float changePoseAtProgress,
            CancellationToken cancellationToken = default
        );
    }

}
