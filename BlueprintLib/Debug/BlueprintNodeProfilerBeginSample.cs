using System;
using MisterGames.Blueprints;
using UnityEngine;
using UnityEngine.Profiling;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceProfilerBeginSample :
        BlueprintSource<BlueprintNodeProfilerBeginSample>,
        BlueprintSources.IEnter<BlueprintNodeProfilerBeginSample>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Profiler.BeginSample", Category = "Debug", Color = BlueprintColors.Node.Debug)]
    public struct BlueprintNodeProfilerBeginSample : IBlueprintNode, IBlueprintEnter {

        [SerializeField] private string _sampleName;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Exit());
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            Profiler.BeginSample(_sampleName);
            blueprint.Call(token, 1);
        }
    }

}
