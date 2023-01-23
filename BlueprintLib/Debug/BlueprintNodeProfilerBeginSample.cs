using System;
using MisterGames.Blueprints;
using UnityEngine;
using UnityEngine.Profiling;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Profiler.BeginSample", Category = "Debug", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeProfilerBeginSample : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private string _sampleName;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            Profiler.BeginSample(_sampleName);
            CallPort(1);
        }
    }

}
