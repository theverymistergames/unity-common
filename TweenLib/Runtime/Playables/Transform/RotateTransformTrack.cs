using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MisterGames.TweenLib.Playables {

    [TrackClipType(typeof(RotateTransformPlayable))]
    [TrackBindingType(typeof(Transform))]
    public sealed class RotateTransformTrack : TrackAsset {

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
            return ScriptPlayable<RotateTransformBehaviourMixer>.Create(graph, inputCount);
        }
    }

}
