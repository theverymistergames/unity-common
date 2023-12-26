using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Conditions;
using MisterGames.Character.Core;
using MisterGames.Character.Capsule;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Character.Actions {

    [Serializable]
    public sealed class CharacterActionChangePose : ICharacterAction {

        public Optional<CharacterPoseType> overridePose;
        [Range(0f, 1f)] public float changePoseAt;
        public Optional<float> overrideDuration;
        public Optional<CharacterCapsuleSize> overrideCapsuleSize;
        [SerializeReference] [SubclassSelector] public ICharacterCondition canTransit;

        public async UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            var pose = characterAccess.GetPipeline<ICharacterPosePipeline>();
            ICharacterPoseGraphPipeline poseGraph = null;

            var targetPose = overridePose.GetOrDefault(pose.TargetPose);
            var targetCapsuleSize = overrideCapsuleSize.Value;
            float duration = overrideDuration.Value;

            if (!overrideCapsuleSize.HasValue) {
                poseGraph = characterAccess.GetPipeline<ICharacterPoseGraphPipeline>();
                targetCapsuleSize = poseGraph.GetDefaultCapsuleSize(targetPose);
            }

            if (!overrideDuration.HasValue) {
                poseGraph ??= characterAccess.GetPipeline<ICharacterPoseGraphPipeline>();
                duration = poseGraph.GetDefaultTransitionDuration(targetPose);
            }

            await pose.TryChangePose(
                targetPose,
                targetCapsuleSize,
                duration,
                changePoseAt,
                canTransit,
                cancellationToken
            );
        }
    }

}
