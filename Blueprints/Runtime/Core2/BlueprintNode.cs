﻿using System;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public abstract class BlueprintNode {

        private RuntimePort[] _runtimePorts;

        public abstract Port[] CreatePorts();

        internal void InjectRuntimePorts(RuntimePort[] runtimePorts) {
            _runtimePorts = runtimePorts;
        }

        protected void CallPort(int portIndex) {
            var links = _runtimePorts[portIndex].links;
            for (int i = 0; i < links.Length; i++) {
                var link = links[i];
                if (link.node is IBlueprintEnter enter) enter.OnEnterPort(link.port);
            }
        }

        protected T ReadPort<T>(int portIndex, T defaultValue = default) {
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
