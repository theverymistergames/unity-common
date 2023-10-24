using System;
using MisterGames.Blueprints.Core;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    [BlueprintNodeMeta(Name = "Start", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeStart : BlueprintNode, IBlueprintStart, IBlueprintPortLinker {

        public override Port[] CreatePorts() => new[] {
            Port.Exit(),
            Port.Enter("On Start").External(true).Hidden(true),
        };

        public void OnStart() {
            Ports[0].Call();
        }

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
