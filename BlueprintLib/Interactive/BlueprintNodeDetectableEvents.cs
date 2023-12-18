using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using MisterGames.Interact.Detectables;
using MisterGames.Interact.Interactives;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Detectable Events", Category = "Interactive", Color = BlueprintColors.Node.Events)]
    public sealed class BlueprintNodeDetectableEvents :
        IBlueprintNode,
        IBlueprintEnter,
        IBlueprintOutput<bool>,
        IBlueprintOutput<IDetector>,
        IBlueprintOutput<Transform>,
        IBlueprintOutput<GameObject>,
        IBlueprintStartCallback
    {
        [SerializeField] private bool _autoSetDetectableOnStart = true;

        private Detectable _detectable;
        private IDetector _lastDetector;
        private IBlueprint _blueprint;
        private NodeToken _token;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Set Detectable"));
            meta.AddPort(id, Port.Input<Detectable>());
            meta.AddPort(id, Port.Exit("On Detected"));
            meta.AddPort(id, Port.Exit("On Lost"));
            meta.AddPort(id, Port.Output<bool>("Is Detected"));
            meta.AddPort(id, Port.Output<IDetectable>("Last Detector"));
            meta.AddPort(id, Port.Output<Transform>("Last Detector Transform"));
            meta.AddPort(id, Port.Output<GameObject>("Last Detector Root"));
        }

        public void OnStart(IBlueprint blueprint, NodeToken token) {
            if (!_autoSetDetectableOnStart) return;

            _token = token;
            _blueprint = blueprint;

            PrepareDetectable();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            if (_detectable != null) {
                _detectable.OnDetectedBy -= OnDetectedBy;
                _detectable.OnLostBy -= OnLostBy;
            }

            _detectable = null;
            _lastDetector = null;
            _blueprint = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            _token = token;
            PrepareDetectable();
        }

        private void PrepareDetectable() {
            if (_detectable != null) {
                _detectable.OnDetectedBy -= OnDetectedBy;
                _detectable.OnLostBy -= OnLostBy;
            }

            _detectable = _blueprint.Read<Detectable>(_token, 1);

            if (_detectable != null) {
                _detectable.OnDetectedBy += OnDetectedBy;
                _detectable.OnLostBy += OnLostBy;
            }
        }

        private void OnDetectedBy(IDetector detector) {
            Debug.Log($"BlueprintNodeDetectableEvents.OnDetectedBy: detector.Transform {detector.Transform}");

            _lastDetector = detector;
            _blueprint.Call(_token, 2);
        }

        private void OnLostBy(IDetector detector) {
            Debug.Log($"BlueprintNodeDetectableEvents.OnLostBy: detector.Transform {detector.Transform}");

            _blueprint.Call(_token, 3);
        }

        bool IBlueprintOutput<bool>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            4 => _detectable != null && _detectable.IsDetectedBy(_lastDetector),
            _ => default,
        };

        IDetector IBlueprintOutput<IDetector>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            5 => _lastDetector,
            _ => default,
        };

        Transform IBlueprintOutput<Transform>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            6 => _lastDetector?.Transform,
            _ => default,
        };

        GameObject IBlueprintOutput<GameObject>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            7 => _lastDetector?.Root,
            _ => default,
        };
    }

}
