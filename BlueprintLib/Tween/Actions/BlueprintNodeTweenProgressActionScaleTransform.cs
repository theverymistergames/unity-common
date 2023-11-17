using System;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceTweenProgressActionScaleTransform :
        BlueprintSource<BlueprintNodeTweenProgressActionScaleTransform2>,
        BlueprintSources.IOutput<BlueprintNodeTweenProgressActionScaleTransform2, ITweenProgressAction>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Tween Action Scale Transform", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeTweenProgressActionScaleTransform2 : IBlueprintNode, IBlueprintOutput2<ITweenProgressAction>  {

        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _startLocalScale;
        [SerializeField] private Vector3 _endLocalScale;

        private TweenProgressActionScaleTransform _action;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<Transform>());
            meta.AddPort(id, Port.Input<Vector3>("Start Scale"));
            meta.AddPort(id, Port.Input<Vector3>("End Scale"));
            meta.AddPort(id, Port.Output<ITweenProgressAction>());
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token) {
            _action = new TweenProgressActionScaleTransform();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token) {
            _action = null;
        }

        public ITweenProgressAction GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 3) return null;

            _action.transform = blueprint.Read(token, 0, _transform);
            _action.startLocalScale = blueprint.Read(token, 1, _startLocalScale);
            _action.endLocalScale = blueprint.Read(token, 2, _endLocalScale);

            return _action;
        }
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Tween Action Scale Transform", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenProgressActionScaleTransform : BlueprintNode, IBlueprintOutput<ITweenProgressAction>  {

        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _startLocalScale;
        [SerializeField] private Vector3 _endLocalScale;

        private readonly TweenProgressActionScaleTransform  _action = new TweenProgressActionScaleTransform ();

        public override Port[] CreatePorts() => new[] {
            Port.Input<Transform>(),
            Port.Input<Vector3>("Start Scale"),
            Port.Input<Vector3>("End Scale"),
            Port.Output<ITweenProgressAction>(),
        };

        public ITweenProgressAction GetOutputPortValue(int port) {
            if (port != 3) return null;

            _action.transform = Ports[0].Get(_transform);
            _action.startLocalScale = Ports[1].Get(_startLocalScale);
            _action.endLocalScale = Ports[2].Get(_endLocalScale);

            return _action;
        }
    }

}
