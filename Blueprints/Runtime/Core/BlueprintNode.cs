using System;
using MisterGames.Blueprints.Compile;

namespace MisterGames.Blueprints {

    [Serializable]
    public abstract class BlueprintNode {

        internal RuntimePort[] RuntimePorts;

        public abstract Port[] CreatePorts();

        public virtual void OnInitialize(IBlueprintHost host) {}
        public virtual void OnDeInitialize() {}

        protected void CallPort(int port) {
            var links = RuntimePorts[port].links;
            for (int i = 0; i < links.Count; i++) {
                var link = links[i];
                if (link.node is IBlueprintEnter enter) enter.OnEnterPort(link.port);
            }
        }

        protected T ReadPort<T>(int port, T defaultValue = default) {
            var links = RuntimePorts[port].links;
            if (links.Count == 0) return defaultValue;

            var link = links[0];
            if (link.node is IBlueprintOutput<T> outputT) return outputT.GetPortValue(link.port);

            return defaultValue;
        }

#if UNITY_EDITOR
        public virtual void OnValidate() {}
#endif
    }

}
