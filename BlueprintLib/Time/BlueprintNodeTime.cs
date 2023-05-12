using System;
using MisterGames.Blueprints;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Time", Category = "Time", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeTime : BlueprintNode, IBlueprintOutput<float> {

        private float _startTime;
        
        public override Port[] CreatePorts() => new[] {
            Port.Output<float>("Time"),
            Port.Output<float>("Sine time"),
            Port.Output<float>("Cosine time"),
        };

        public override void OnInitialize(IBlueprintHost host) {
            _startTime = TimeSources.time;
        }

        public float GetOutputPortValue(int port) => port switch {
            0 => TimeSources.time - _startTime,
            1 => Mathf.Sin(TimeSources.time - _startTime),
            2 => Mathf.Cos(TimeSources.time - _startTime),
            _ => default,
        };
    }

}
