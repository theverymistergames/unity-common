using System;
using MisterGames.Blueprints;
using UnityEngine.Profiling;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceProfilerEndSample :
        BlueprintSource<BlueprintNodeProfilerEndSample>,
        BlueprintSources.IEnter<BlueprintNodeProfilerBeginSample>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Profiler.EndSample", Category = "Debug", Color = BlueprintColors.Node.Debug)]
    public struct BlueprintNodeProfilerEndSample : IBlueprintNode, IBlueprintEnter {

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

}
