using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MisterGames.TweenLib.Playables {

    [TrackClipType(typeof(MoveTransformPlayable))]
    [TrackBindingType(typeof(Transform))]
    public sealed class MoveTransformTrack : TrackAsset {

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
            return ScriptPlayable<MoveTransformBehaviourMixer>.Create(graph, inputCount);
        }
    }

}
