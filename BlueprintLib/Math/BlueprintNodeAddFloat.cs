﻿using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceAddFloat :
        BlueprintSource<BlueprintNodeAddFloat2>,
        BlueprintSources.IOutput<BlueprintNodeAddFloat2, float>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Add Float", Category = "Math", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeAddFloat2 : IBlueprintNode, IBlueprintOutput2<float> {

        [SerializeField] private float _a;
        [SerializeField] private float _b;
        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<float>("A"));
            meta.AddPort(id, Port.Input<float>("B"));
            meta.AddPort(id, Port.Output<float>());
        }

        public float GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            2 => blueprint.Read(token, 0, _a) + blueprint.Read(token, 1, _b),
            _ => default,
        };
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Add Float", Category = "Math", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeAddFloat : BlueprintNode, IBlueprintOutput<float> {

        [SerializeField] private float _a;
        [SerializeField] private float _b;

        public override Port[] CreatePorts() => new[] {
            Port.Input<float>("A"),
            Port.Input<float>("B"),
            Port.Output<float>()
        };

        public float GetOutputPortValue(int port) => port switch {
            2 => Ports[0].Get(_a) + Ports[1].Get(_b),
            _ => default,
        };
    }

}
