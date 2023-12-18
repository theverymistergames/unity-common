using System;
using MisterGames.Blueprints;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceFloatCurve :
        BlueprintSource<BlueprintNodeFloatCurve>,
        BlueprintSources.IOutput<BlueprintNodeFloatCurve, float>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Float Curve", Category = "Math", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeFloatCurve : IBlueprintNode, IBlueprintOutput<float> {

        [SerializeField] private Vector2 _inputBounds;
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private float _outputMultiplier;
        [SerializeField] private float _outputOffset;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<float>("Value"));
            meta.AddPort(id, Port.Input<Vector2>("Input Bounds"));
            meta.AddPort(id, Port.Input<AnimationCurve>("Curve"));
            meta.AddPort(id, Port.Input<float>("Output Multiplier"));
            meta.AddPort(id, Port.Input<float>("Output Offset"));
            meta.AddPort(id, Port.Output<float>("Result"));
        }

        public float GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 5) return default;

            float input = blueprint.Read<float>(token, 0);
            var inputBounds = blueprint.Read(token, 1, _inputBounds);
            var curve = blueprint.Read(token, 2, _curve);
            float outputMultiplier = blueprint.Read(token, 3, _outputMultiplier);
            float outputOffset = blueprint.Read(token, 4, _outputOffset);

            if (inputBounds.y.IsNearlyEqual(inputBounds.x)) {
                input = inputBounds.x;
            }
            else {
                input = (input - inputBounds.x) / (inputBounds.y - inputBounds.x);
            }

            return curve.Evaluate(input) * outputMultiplier + outputOffset;
        }
    }

}
