using UnityEngine;
using UnityEngine.Playables;

namespace MisterGames.TweenLib.Playables {

    public sealed class RotateTransformBehaviourMixer : PlayableBehaviour {

        public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
            if (playerData is not Transform transform) return;

            var rotationAccumulator = Quaternion.identity;

            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++) {
                float inputWeight = playable.GetInputWeight(i);

                var clip = (ScriptPlayable<RotateTransformBehaviour>) playable.GetInput(i);
                var bhv = clip.GetBehaviour();

                double duration = clip.GetDuration();
                float progress = duration > 0d ? (float) (clip.GetTime() / duration) : 0f;
                var rotation = bhv.GetRotation(transform, progress);

                rotationAccumulator *= Quaternion.Slerp(Quaternion.identity, rotation, inputWeight);
            }

            transform.localRotation = rotationAccumulator;
        }
    }

}
