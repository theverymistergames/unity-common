using System;
using MisterGames.Blueprints;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceTime :
        BlueprintSource<BlueprintNodeTime>,
        BlueprintSources.IOutput<BlueprintNodeTime, float>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Time", Category = "Time", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeTime : IBlueprintNode, IBlueprintOutput<float> {

        private float _startTime;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<float>("Time"));
            meta.AddPort(id, Port.Output<float>("Sine time"));
            meta.AddPort(id, Port.Output<float>("Cosine time"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _startTime = TimeSources.scaledTime;
        }

        public float GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            0 => TimeSources.scaledTime - _startTime,
            1 => Mathf.Sin(TimeSources.scaledTime - _startTime),
            2 => Mathf.Cos(TimeSources.scaledTime - _startTime),
            _ => default,
        };
    }

}
