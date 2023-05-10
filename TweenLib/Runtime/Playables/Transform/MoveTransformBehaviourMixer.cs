using UnityEngine;
using UnityEngine.Playables;

namespace MisterGames.TweenLib.Playables {

    public sealed class MoveTransformBehaviourMixer : PlayableBehaviour {

        public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
            if (playerData is not Transform transform) return;

            var positionAccumulator = Vector3.zero;

            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++) {
                float inputWeight = playable.GetInputWeight(i);

                var clip = (ScriptPlayable<MoveTransformBehaviour>) playable.GetInput(i);
                var bhv = clip.GetBehaviour();

                double duration = clip.GetDuration();
                float progress = duration > 0d ? (float) (clip.GetTime() / duration) : 0f;
                var position = bhv.GetPosition(transform, progress);

                positionAccumulator += position * inputWeight;
            }

            transform.localPosition = positionAccumulator;
        }
    }

}
