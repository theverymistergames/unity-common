﻿using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Compile;

namespace MisterGames.Blueprints {

    [Serializable]
    public abstract class BlueprintNode {

        internal RuntimePort[] RuntimePorts;

        public abstract Port[] CreatePorts();

        public virtual void OnInitialize(IBlueprintHost host) {}
        public virtual void OnDeInitialize() {}

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
            if (link.node is IBlueprintOutput<T> output) return output.GetOutputPortValue(link.port);

            return defaultValue;
        }

        protected IReadOnlyList<T> ReadInputArrayPort<T>(int port) {
            var links = RuntimePorts[port].links;
            if (links.Count == 0) return Array.Empty<T>();

            if (links.Count == 1) {
                var link = links[0];

                if (link.node is IBlueprintOutput<T> output) {
                    return new []{ output.GetOutputPortValue(link.port) };
                }

                if (link.node is IBlueprintOutputArray<T> outputArray) {
                    return outputArray.GetOutputArrayPortValues(link.port);
                }

                return Array.Empty<T>();
            }

            var values = new List<T>();

            for (int i = 0; i < links.Count; i++) {
                var link = links[i];

                if (link.node is IBlueprintOutput<T> output) {
                    values.Add(output.GetOutputPortValue(link.port));
                    continue;
                }

                if (link.node is IBlueprintOutputArray<T> outputArray) {
                    values.AddRange(outputArray.GetOutputArrayPortValues(link.port));
                }
            }

            return values;
        }

#if UNITY_EDITOR
        public virtual void OnValidate() {}
#endif
    }

}
