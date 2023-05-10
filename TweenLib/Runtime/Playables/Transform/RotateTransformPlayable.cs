using UnityEngine;
using UnityEngine.Playables;

namespace MisterGames.TweenLib.Playables {

    public sealed class RotateTransformPlayable : PlayableAsset {

        [SerializeField] private RotateTransformBehaviour _template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
            return ScriptPlayable<RotateTransformBehaviour>.Create(graph, _template);
        }
    }

}
