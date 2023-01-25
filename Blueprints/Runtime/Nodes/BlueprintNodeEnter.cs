using System;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Enter", Category = "Flow", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeEnter : BlueprintNode, IBlueprintEnter {

        [SerializeField] private string _parameter;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(_parameter).SetExternal(true),
            Port.Exit()
        };

        public void OnEnterPort(int port) {
            if (port == 0) CallPort(1);
        }
    }

}
