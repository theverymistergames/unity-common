using System;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Exit", Category = "Flow", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeExit : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private string _parameter;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Exit(_parameter).SetExternal(true)
        };

        public void OnEnterPort(int port) {
            if (port == 0) CallPort(1);
        }
    }

}
