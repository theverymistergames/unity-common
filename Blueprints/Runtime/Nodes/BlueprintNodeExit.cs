using System;
using MisterGames.Blueprints.Core;

#if UNITY_EDITOR
using MisterGames.Blueprints.Meta;
using UnityEngine;
#endif

namespace MisterGames.Blueprints.Nodes {

#if UNITY_EDITOR
    [BlueprintNodeMeta(Name = "Exit", Category = "External", Color = BlueprintColors.Node.External)]
#endif

    [Serializable]
    public sealed class BlueprintNodeExit : BlueprintNode, IBlueprintPortLinker

#if UNITY_EDITOR
        , IBlueprintAssetValidator
#endif

    {

#if UNITY_EDITOR
        [SerializeField] private string _port;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Exit(_port).SetExternal(true),
        };

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: false, notify: false);
        }
#else
        public override Port[] CreatePorts() => null;
#endif

        public int GetLinkedPort(int port) => port switch {
            0 => 1,
            1 => 0,
            _ => -1,
        };
    }

}
