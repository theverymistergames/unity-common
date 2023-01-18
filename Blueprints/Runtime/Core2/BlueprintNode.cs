using System;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public abstract class BlueprintNode {

        internal RuntimePort[] RuntimePorts;

        public abstract Port[] CreatePorts();

        public virtual void OnInitialize(BlueprintRunner runner) {}
        public virtual void OnDeInitialize() {}
        public virtual void OnValidate() {}

        protected void CallPort(int portIndex) {
            var links = RuntimePorts[portIndex].links;
            for (int i = 0; i < links.Length; i++) {
                var link = links[i];
                if (link.node is IBlueprintEnter enter) enter.OnEnterPort(link.port);
            }
        }

        protected T ReadPort<T>(int portIndex, T defaultValue = default) {
            var links = RuntimePorts[portIndex].links;
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
