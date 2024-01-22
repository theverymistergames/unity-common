using System;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceTweenProgressActionMoveTransform :
        BlueprintSource<BlueprintNodeTweenProgressActionMoveTransform>,
        BlueprintSources.IOutput<BlueprintNodeTweenProgressActionMoveTransform, ITweenProgressAction>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Tween Action Move Transform", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeTweenProgressActionMoveTransform : IBlueprintNode, IBlueprintOutput<ITweenProgressAction>  {

        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _startPosition;
        [SerializeField] private Vector3 _endPosition;
        [SerializeField] private bool _useLocal;

        private TweenProgressActionMoveTransform _action;

        public void OnSetDefaults(IBlueprintMeta meta, NodeId id) {
            _useLocal = true;
        }

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<Transform>());
            meta.AddPort(id, Port.Input<Vector3>("Start Position"));
            meta.AddPort(id, Port.Input<Vector3>("End Position"));
            meta.AddPort(id, Port.Output<ITweenProgressAction>());
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _action = new TweenProgressActionMoveTransform();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _action = null;
        }

        public ITweenProgressAction GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 3) return null;

            _action.transform = blueprint.Read(token, 0, _transform);
            _action.startPosition = blueprint.Read(token, 1, _startPosition);
            _action.endPosition = blueprint.Read(token, 2, _endPosition);
            _action.useLocal = _useLocal;

            return _action;
        }
    }

}
