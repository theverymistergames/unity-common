using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Goto (exit)", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeGotoExit : BlueprintNode, IBlueprintPortLinker, IBlueprintNodeLinker {
        
        [SerializeField] private string _label;

        public int LinkerNodeHash => string.IsNullOrWhiteSpace(_label) ? 0 : _label.GetHashCode();
        public int LinkerNodePort => 1;

        public override Port[] CreatePorts() => new[] {
            Port.Exit(),
            Port.Enter().Hidden(true),
        };

        public int GetLinkedPorts(int port, out int count) {
            if (port == 0) {
                count = 1;
                return 1;
            }

            if (port == 1) {
                count = 1;
                return 0;
            }

            count = 0;
            return -1;
        }
    }

}
