using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Ports;
using MisterGames.Blueprints.Utils2;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public abstract class BlueprintNode {

        [SerializeField] [HideInInspector] private List<Port> _ports;
        public IReadOnlyList<Port> Ports => _ports;

        private RuntimePort[] _runtimePorts;

        public abstract void CreatePorts();

        public void ClearPorts() {
            _ports.Clear();
        }

        public void AddPort(Port port) {
            _ports.Add(port);
        }

        public void SetPort(int portIndex, Port value) {
            _ports[portIndex] = value;
        }

        public bool HasPort(int portIndex) {
            return 0 <= portIndex && portIndex < _ports.Count;
        }

        public void ResolveLinks(IBlueprintRouter router) {
            _runtimePorts = _ports.Count > 0 ? new RuntimePort[_ports.Count] : Array.Empty<RuntimePort>();

            for (int p = 0; p < _ports.Count; p++) {
                var port = _ports[p];

#if DEBUG
                if (!BlueprintValidationUtils.ValidatePort(this, p)) continue;
#endif

                var links = port.Links;
                var runtimeLinks = links.Count > 0 ? new RuntimeLink[links.Count] : Array.Empty<RuntimeLink>();

                for (int l = 0; l < links.Count; l++) {
                    var link = links[l];
                    var linkedNode = router.GetNode(link.nodeId);

#if DEBUG
                    if (!BlueprintValidationUtils.ValidateLink(this, p, linkedNode, link.port)) continue;
#endif

                    runtimeLinks[l] = new RuntimeLink(linkedNode, link.port);
                }

                _runtimePorts[p] = new RuntimePort(runtimeLinks);
            }
        }

        public void CallPort(int portIndex) {
#if DEBUG
            if (!BlueprintValidationUtils.ValidateExitPort(this, portIndex)) return;
#endif

            var links = _runtimePorts[portIndex].links;
            for (int i = 0; i < links.Length; i++) {
                var link = links[i];
                if (link.node is IBlueprintEnter enter) enter.OnEnterPort(link.port);
            }
        }

        public T ReadPort<T>(int portIndex, T defaultValue = default) {
#if DEBUG
            if (!BlueprintValidationUtils.ValidateInputPort<T>(this, portIndex)) return defaultValue;
#endif

            var links = _runtimePorts[portIndex].links;
            if (links.Length == 0) return defaultValue;

            var link = links[0];

            if (link.node is IBlueprintOutput<T> outputT) return outputT.GetPortValue(link.port);
            if (link.node is IBlueprintOutput output) return output.GetPortValue<T>(link.port);

            return defaultValue;
        }

        public override string ToString() {
            return $"BlueprintNode(type {GetType().Name}";
        }
    }

}
