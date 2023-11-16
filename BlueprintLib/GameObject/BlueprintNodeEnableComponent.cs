using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceEnableComponent :
        BlueprintSource<BlueprintNodeEnableComponent>,
        BlueprintSources.IEnter<BlueprintNodeEnableComponent>,
        BlueprintSources.ICloneable {}
    
    [Serializable]
    [BlueprintNode(Name = "Enable Component", Category = "GameObject", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeEnableComponent : IBlueprintNode, IBlueprintEnter2 {

        [SerializeField] private bool _isEnabled;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Apply"));
            meta.AddPort(id, Port.Input<Behaviour>());
            meta.AddPort(id, Port.Input<bool>("Is Enabled"));
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            bool isEnabled = blueprint.Read(token, 2, _isEnabled);
            var behaviour = blueprint.Read<Behaviour>(token, 1);

            behaviour.enabled = isEnabled;
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            meta.InvalidateNode(id, invalidateLinks: true);
        }
    }

}
