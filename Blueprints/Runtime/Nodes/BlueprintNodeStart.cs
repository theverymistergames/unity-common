using System;
using MisterGames.Blueprints.Core;

namespace MisterGames.Blueprints.Nodes {

#if UNITY_EDITOR
    [BlueprintNodeMeta(Name = "Start", Category = "External", Color = BlueprintColors.Node.External)]
#endif

    [Serializable]
    public sealed class BlueprintNodeStart : BlueprintNode, IBlueprintStart, IBlueprintPortLinker {

#if UNITY_EDITOR
        public override Port[] CreatePorts() => new[] {
            Port.Exit(),
            Port.Enter("On Start").SetExternal(true),
        };
#else
        public override Port[] CreatePorts() => null;
#endif

        public void OnStart() {
            CallExitPort(0);
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
