using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Set Material Emission", Category = "Material", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeSetMaterialEmission : BlueprintNode, IBlueprintEnter, IBlueprintStart {

        private static readonly int EmissiveColor = Shader.PropertyToID("_EmissiveColor");
        private static readonly int EmissiveIntensity = Shader.PropertyToID("_EmissiveIntensity");

        [SerializeField] private bool _autoSetRendererAtStart;
        [SerializeField] private Color _color;
        [SerializeField] private float _intensity;

        private Renderer _renderer;
        private Color _currentColor;
        private float _currentIntensity;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Set Renderer"),
            Port.Input<Renderer>(),
            Port.Enter("Set Color"),
            Port.Input<Color>(),
            Port.Enter("Set Intensity"),
            Port.Input<float>("Intensity"),
        };

        public void OnStart() {
            if (!_autoSetRendererAtStart) return;

            _renderer = Ports[1].Get<Renderer>();
            var material = _renderer.material;

            _currentIntensity = material.GetFloat(EmissiveIntensity);
            _currentColor = material.GetColor(EmissiveColor) /
                            (_currentIntensity.IsNearlyZero() ? 1f : _currentIntensity);
        }

        public void OnEnterPort(int port) {
            switch (port) {
                case 0:
                    _renderer = Ports[1].Get<Renderer>();
                    var material = _renderer.material;

                    _currentIntensity = material.GetFloat(EmissiveIntensity);
                    _currentColor = material.GetColor(EmissiveColor) /
                                    (_currentIntensity.IsNearlyZero() ? 1f : _currentIntensity);
                    break;

                case 2:
                    _currentColor = Ports[3].Get(_color);
                    _renderer.material.SetColor(EmissiveColor, _currentColor * _currentIntensity);
                    break;

                case 4:
                    _currentIntensity = Ports[5].Get(_intensity);
                    _renderer.material.SetColor(EmissiveColor, _currentColor * _currentIntensity);
                    break;
            }
        }
    }

}
