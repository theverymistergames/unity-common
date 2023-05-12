using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Set Material Emission", Category = "Material", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeSetMaterialEmission : BlueprintNode, IBlueprintEnter {

        private static readonly int EmissiveColor = Shader.PropertyToID("_EmissiveColor");

        [SerializeField] private Color _color;
        [SerializeField] private float _intensity;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Input<Renderer>(),
            Port.Input<Color>(),
            Port.Input<float>("Intensity"),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            var renderer = Ports[1].Get<Renderer>();

            var color = Ports[2].Get(_color);
            float intensity = Ports[3].Get(_intensity);

            renderer.material.SetColor(EmissiveColor, color * intensity);

            Ports[4].Call();
        }
    }

}
