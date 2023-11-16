﻿using System;
using MisterGames.Blueprints;
using UnityEngine;
using UnityEngine.Profiling;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceProfilerBeginSample :
        BlueprintSource<BlueprintNodeProfilerBeginSample2>,
        BlueprintSources.IEnter<BlueprintNodeProfilerBeginSample2>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Profiler.BeginSample", Category = "Debug", Color = BlueprintColors.Node.Debug)]
    public struct BlueprintNodeProfilerBeginSample2 : IBlueprintNode, IBlueprintEnter2 {

        [SerializeField] private string _sampleName;

        private IBlueprint _blueprint;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Exit());
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token) {
            _blueprint = blueprint;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token) {
            _blueprint = null;
        }

        public void OnEnterPort(NodeToken token, int port) {
            if (port != 0) return;

            Profiler.BeginSample(_sampleName);
            _blueprint.Call(token, 1);
        }
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Profiler.BeginSample", Category = "Debug", Color = BlueprintColors.Node.Debug)]
    public sealed class BlueprintNodeProfilerBeginSample : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private string _sampleName;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            Profiler.BeginSample(_sampleName);
            Ports[1].Call();
        }
    }

}
