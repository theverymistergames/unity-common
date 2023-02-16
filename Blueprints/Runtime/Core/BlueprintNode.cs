using System;
using MisterGames.Blueprints.Compile;

namespace MisterGames.Blueprints {

    [Serializable]
    public abstract class BlueprintNode {

        internal RuntimePort[] RuntimePorts;

        public abstract Port[] CreatePorts();

        public virtual void OnInitialize(IBlueprintHost host) {}
        public virtual void OnDeInitialize() {}

        public virtual void OnValidate() {}

        protected void CallExitPort(int port) {
            var links = RuntimePorts[port].links;
            for (int i = 0; i < links.Count; i++) {
                var link = links[i];
                if (link.node is IBlueprintEnter enter) enter.OnEnterPort(link.port);
            }
        }

        protected T ReadInputPort<T>(int port, T defaultValue = default) {
            var links = RuntimePorts[port].links;
            if (links.Count == 0) return defaultValue;

            var link = links[0];
            if (link.node is IBlueprintOutput<T> outputT) return outputT.GetOutputPortValue(link.port);
            if (link.node is IBlueprintOutput output) return output.GetOutputPortValue<T>(link.port);

            return defaultValue;
        }

        protected T[] ReadInputArrayPort<T>(int port, T[] defaultArray = null) {
            var links = RuntimePorts[port].links;
            if (links.Count == 0) return defaultArray ?? Array.Empty<T>();

            var array = new T[links.Count];

            for (int i = 0; i < links.Count; i++) {
                var link = links[i];

                if (link.node is IBlueprintOutput<T> outputT) {
                    array[i] = outputT.GetOutputPortValue(link.port);
                    continue;
                }

                if (link.node is IBlueprintOutput output) {
                    array[i] = output.GetOutputPortValue<T>(link.port);
                }
            }

            return array;
        }
    }

}
