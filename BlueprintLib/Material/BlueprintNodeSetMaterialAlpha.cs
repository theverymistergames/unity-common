using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class  BlueprintSourceSetMaterialAlpha :
        BlueprintSource<BlueprintNodeSetMaterialAlpha>,
        BlueprintSources.IEnter<BlueprintNodeSetMaterialAlpha>,
        BlueprintSources.IStartCallback<BlueprintNodeSetMaterialAlpha>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Set Material Alpha", Category = "Material", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeSetMaterialAlpha : IBlueprintNode, IBlueprintEnter, IBlueprintStartCallback {

        [SerializeField] private bool _autoSetRendererAtStart;
        [SerializeField] private float _alpha;

        private static readonly int Alpha = Shader.PropertyToID("_alpha");

        private Renderer _renderer;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Set Renderer"));
            meta.AddPort(id, Port.Input<Renderer>());
            meta.AddPort(id, Port.Enter("Set Alpha"));
            meta.AddPort(id, Port.Input<float>("Alpha"));
        }

        public void OnStart(IBlueprint blueprint, NodeToken token) {
            if (!_autoSetRendererAtStart) return;

            _renderer = blueprint.Read<Renderer>(token, 1);
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _renderer = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            switch (port) {
                case 0:
                    _renderer = blueprint.Read<Renderer>(token, 1);
                    break;

                case 2:
                    float alpha = blueprint.Read(token, 3, _alpha);
                    _renderer.material.SetFloat(Alpha, alpha);
                    break;
            }
        }
    }

}
