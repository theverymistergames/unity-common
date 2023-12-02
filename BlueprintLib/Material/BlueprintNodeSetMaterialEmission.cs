using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class  BlueprintSourceSetMaterialEmission :
        BlueprintSource<BlueprintNodeSetMaterialEmission>,
        BlueprintSources.IEnter<BlueprintNodeSetMaterialEmission>,
        BlueprintSources.IStartCallback<BlueprintNodeSetMaterialEmission>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Set Material Emission", Category = "Material", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeSetMaterialEmission : IBlueprintNode, IBlueprintEnter, IBlueprintStartCallback {

        [SerializeField] private bool _autoSetRendererAtStart;
        [SerializeField] private Color _color;
        [SerializeField] private float _intensity;

        private static readonly int EmissiveColor = Shader.PropertyToID("_EmissiveColor");
        private static readonly int EmissiveIntensity = Shader.PropertyToID("_EmissiveIntensity");

        private Renderer _renderer;
        private Color _currentColor;
        private float _currentIntensity;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Set Renderer"));
            meta.AddPort(id, Port.Input<Renderer>());
            meta.AddPort(id, Port.Enter("Set Color"));
            meta.AddPort(id, Port.Input<Color>());
            meta.AddPort(id, Port.Enter("Set Intensity"));
            meta.AddPort(id, Port.Input<float>("Intensity"));
        }

        public void OnStart(IBlueprint blueprint, NodeToken token) {
            if (!_autoSetRendererAtStart) return;

            _renderer = blueprint.Read<Renderer>(token, 1);
            var material = _renderer.material;

            _currentIntensity = material.GetFloat(EmissiveIntensity);
            _currentColor = material.GetColor(EmissiveColor) /
                            (_currentIntensity.IsNearlyZero() ? 1f : _currentIntensity);
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _renderer = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            switch (port) {
                case 0:
                    _renderer = blueprint.Read<Renderer>(token, 1);
                    var material = _renderer.material;

                    _currentIntensity = material.GetFloat(EmissiveIntensity);
                    _currentColor = material.GetColor(EmissiveColor) /
                                    (_currentIntensity.IsNearlyZero() ? 1f : _currentIntensity);
                    break;

                case 2:
                    _currentColor = blueprint.Read(token, 3, _color);
                    _renderer.material.SetColor(EmissiveColor, _currentColor * _currentIntensity);
                    break;

                case 4:
                    _currentIntensity = blueprint.Read(token, 5, _intensity);
                    _renderer.material.SetColor(EmissiveColor, _currentColor * _currentIntensity);
                    break;
            }
        }
    }

}
