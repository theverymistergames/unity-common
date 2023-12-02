using System;
using MisterGames.Blueprints;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceInputActionFilter :
        BlueprintSource<BlueprintNodeInputActionFilter>,
        BlueprintSources.IEnter<BlueprintNodeInputActionFilter> {}

    [Serializable]
    [BlueprintNode(Name = "Input Action Filter", Category = "Input", Color = BlueprintLibColors.Node.Input)]
    public struct BlueprintNodeInputActionFilter : IBlueprintNode, IBlueprintEnter {

        [SerializeField] private InputActionFilter _filter;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Apply"));
            meta.AddPort(id, Port.Enter("Release"));
            meta.AddPort(id, Port.Exit());
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            switch (port) {
                case 0:
                    _filter.Apply();
                    break;

                case 1:
                    _filter.Release();
                    break;
            }

            blueprint.Call(token, 2);
        }
    }

}
