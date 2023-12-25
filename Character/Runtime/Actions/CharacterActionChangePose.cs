using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Pose;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Character.Actions {

    [Serializable]
    public sealed class CharacterActionChangePose : ICharacterAction {

        public Optional<CharacterPoseType> overridePose;
        [Range(0f, 1f)] public float changePoseAt;
        public Optional<float> overrideDuration;
        public Optional<CharacterCapsuleSize> overrideCapsuleSize;

        public UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            var capsule = characterAccess.GetPipeline<ICharacterCapsulePipeline>();
            ICharacterPoseGraphPipeline poseGraph = null;

            var targetPose = overridePose.GetOrDefault(capsule.CurrentPose);
            var capsuleSize = overrideCapsuleSize.Value;
            float duration = overrideDuration.Value;

            if (!overrideCapsuleSize.HasValue) {
                poseGraph = characterAccess.GetPipeline<ICharacterPoseGraphPipeline>();
                capsuleSize = poseGraph.GetCapsuleSize(targetPose);
            }

            if (!overrideDuration.HasValue) {
                poseGraph ??= characterAccess.GetPipeline<ICharacterPoseGraphPipeline>();
                duration = poseGraph.GetDefaultTransitionDuration(targetPose);
            }

            return capsule.ChangePose(targetPose, capsuleSize, duration, changePoseAt, cancellationToken);
        }
    }

}
