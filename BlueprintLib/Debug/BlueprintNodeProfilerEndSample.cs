using System;
using MisterGames.Blueprints;
using UnityEngine.Profiling;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Profiler.EndSample", Category = "Debug", Color = BlueprintColors.Node.Debug)]
    public sealed class BlueprintNodeProfilerEndSample : BlueprintNode, IBlueprintEnter {

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            Profiler.EndSample();
            Ports[1].Call();
        }
    }

}
