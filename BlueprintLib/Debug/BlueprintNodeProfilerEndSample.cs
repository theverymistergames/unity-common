using System;
using MisterGames.Blueprints;
using UnityEngine;
using UnityEngine.Profiling;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Profiler.EndSample", Category = "Debug", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeProfilerEndSample : BlueprintNode, IBlueprintEnter {

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            Profiler.EndSample();
            CallPort(1);
        }
    }

}
