using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceEnableGameObject :
        BlueprintSource<BlueprintNodeEnableGameObject>,
        BlueprintSources.IEnter<BlueprintNodeEnableGameObject>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Enable GameObject", Category = "GameObject", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeEnableGameObject : IBlueprintNode, IBlueprintEnter2 {

        [SerializeField] private bool _isEnabled;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Apply"));
            meta.AddPort(id, Port.Input<GameObject>());
            meta.AddPort(id, Port.Input<bool>("Is Enabled"));
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            bool isEnabled = blueprint.Read(token, 2, _isEnabled);
            var gameObject = blueprint.Read<GameObject>(token, 1);

            gameObject.SetActive(isEnabled);
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            meta.InvalidateNode(id, invalidateLinks: true);
        }
    }

}
