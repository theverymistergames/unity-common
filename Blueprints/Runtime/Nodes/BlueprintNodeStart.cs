using System;
using MisterGames.Blueprints.Core;

namespace MisterGames.Blueprints.Nodes {

#if UNITY_EDITOR
    [BlueprintNodeMeta(Name = "Start", Category = "External", Color = BlueprintColors.Node.External)]
#endif

    [Serializable]
    public sealed class BlueprintNodeStart : BlueprintNode, IBlueprintStartListener, IBlueprintPortLinker {

#if UNITY_EDITOR
        public override Port[] CreatePorts() => new[] {
            Port.Exit(),
            Port.Enter("On Start").SetExternal(true),
        };
#else
        public override Port[] CreatePorts() => null;
#endif

        public void OnStart() {
            CallExitPort(1);
        }

        public int GetLinkedPort(int port) => port switch {
            0 => 1,
            1 => 0,
            _ => -1,
        };
    }

}
