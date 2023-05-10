using UnityEngine;
using UnityEngine.Playables;

namespace MisterGames.TweenLib.Playables {

    public sealed class ScaleTransformPlayable : PlayableAsset {

        [SerializeField] private ScaleTransformBehaviour _template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
            return ScriptPlayable<ScaleTransformBehaviour>.Create(graph, _template);
        }
    }

}
