using System;
using MisterGames.Blueprints;
using UnityEngine;
using UnityEngine.Profiling;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Profiler.BeginSample", Category = "Debug", Color = BlueprintColors.Node.Debug)]
    public sealed class BlueprintNodeProfilerBeginSample : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private string _sampleName;

        public override Port[] CreatePorts() => new[] {
            Port.Action(PortDirection.Input),
            Port.Action(PortDirection.Output),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            Profiler.BeginSample(_sampleName);
            Ports[1].Call();
        }
    }

}
