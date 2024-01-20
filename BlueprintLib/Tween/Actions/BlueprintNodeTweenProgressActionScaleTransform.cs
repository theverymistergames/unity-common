using System;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceTweenProgressActionScaleTransform :
        BlueprintSource<BlueprintNodeTweenProgressActionScaleTransform>,
        BlueprintSources.IOutput<BlueprintNodeTweenProgressActionScaleTransform, ITweenProgressCallback>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Tween Action Scale Transform", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeTweenProgressActionScaleTransform : IBlueprintNode, IBlueprintOutput<ITweenProgressCallback>  {

        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _startLocalScale;
        [SerializeField] private Vector3 _endLocalScale;

        private TweenProgressActionScaleTransform _action;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<Transform>());
            meta.AddPort(id, Port.Input<Vector3>("Start Scale"));
            meta.AddPort(id, Port.Input<Vector3>("End Scale"));
            meta.AddPort(id, Port.Output<ITweenProgressCallback>());
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _action = new TweenProgressActionScaleTransform();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _action = null;
        }

        public ITweenProgressCallback GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 3) return null;

            _action.transform = blueprint.Read(token, 0, _transform);
            _action.startLocalScale = blueprint.Read(token, 1, _startLocalScale);
            _action.endLocalScale = blueprint.Read(token, 2, _endLocalScale);

            return _action;
        }
    }

}
