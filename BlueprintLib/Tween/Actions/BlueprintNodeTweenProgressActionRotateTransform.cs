using System;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceTweenProgressActionRotateTransform :
        BlueprintSource<BlueprintNodeTweenProgressActionRotateTransform2>,
        BlueprintSources.IOutput<BlueprintNodeTweenProgressActionRotateTransform2, ITweenProgressAction>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Tween Action Rotate Transform", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeTweenProgressActionRotateTransform2 : IBlueprintNode, IBlueprintOutput2<ITweenProgressAction>  {

        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _startEulerAngles;
        [SerializeField] private Vector3 _endEulerAngles;
        [SerializeField] private bool _useLocal;

        private TweenProgressActionRotateTransform _action;

        public void OnSetDefaults(IBlueprintMeta meta, NodeId id) {
            _useLocal = true;
        }

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<Transform>());
            meta.AddPort(id, Port.Input<Vector3>("Start Euler Angles"));
            meta.AddPort(id, Port.Input<Vector3>("End Euler Angles"));
            meta.AddPort(id, Port.Output<ITweenProgressAction>());
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token) {
            _action = new TweenProgressActionRotateTransform();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token) {
            _action = null;
        }

        public ITweenProgressAction GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 3) return null;

            _action.transform = blueprint.Read(token, 0, _transform);
            _action.startEulerAngles = blueprint.Read(token, 1, _startEulerAngles);
            _action.endEulerAngles = blueprint.Read(token, 2, _endEulerAngles);
            _action.useLocal = _useLocal;

            return _action;
        }
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Tween Action Rotate Transform", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenProgressActionRotateTransform : BlueprintNode, IBlueprintOutput<ITweenProgressAction>  {

        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _startEulerAngles;
        [SerializeField] private Vector3 _endEulerAngles;
        [SerializeField] private bool _useLocal = true;

        private readonly TweenProgressActionRotateTransform _action = new TweenProgressActionRotateTransform();

        public override Port[] CreatePorts() => new[] {
            Port.Input<Transform>(),
            Port.Input<Vector3>("Start Euler Angles"),
            Port.Input<Vector3>("End Euler Angles"),
            Port.Output<ITweenProgressAction>(),
        };

        public ITweenProgressAction GetOutputPortValue(int port) {
            if (port != 3) return null;

            _action.transform = Ports[0].Get(_transform);
            _action.startEulerAngles = Ports[1].Get(_startEulerAngles);
            _action.endEulerAngles = Ports[2].Get(_endEulerAngles);
            _action.useLocal = _useLocal;

            return _action;
        }
    }

}
