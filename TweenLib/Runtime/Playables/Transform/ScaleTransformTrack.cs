using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MisterGames.TweenLib.Playables {

    [TrackClipType(typeof(ScaleTransformPlayable))]
    [TrackBindingType(typeof(Transform))]
    public sealed class ScaleTransformTrack : TrackAsset {

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
            return ScriptPlayable<ScaleTransformBehaviourMixer>.Create(graph, inputCount);
        }
    }

}
