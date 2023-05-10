using UnityEngine;
using UnityEngine.Playables;

namespace MisterGames.TweenLib.Playables {

    public sealed class MoveTransformPlayable : PlayableAsset {

        [SerializeField] private MoveTransformBehaviour _template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
            return ScriptPlayable<MoveTransformBehaviour>.Create(graph, _template);
        }
    }

}
