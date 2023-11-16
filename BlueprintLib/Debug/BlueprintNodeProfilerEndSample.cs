using System;
using MisterGames.Blueprints;
using UnityEngine.Profiling;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceProfilerEndSample :
        BlueprintSource<BlueprintNodeProfilerEndSample2>,
        BlueprintSources.IEnter<BlueprintNodeProfilerBeginSample2>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Profiler.EndSample", Category = "Debug", Color = BlueprintColors.Node.Debug)]
    public struct BlueprintNodeProfilerEndSample2 : IBlueprintNode, IBlueprintEnter2 {

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Exit());
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            Profiler.EndSample();
            blueprint.Call(token, 1);
        }
    }

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
