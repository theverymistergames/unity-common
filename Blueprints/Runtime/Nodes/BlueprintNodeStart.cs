using System;
using MisterGames.Blueprints.Core;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Start", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeStart :
        BlueprintNode,
        IBlueprintStartListener,
        IBlueprintPortLinker
    {
        public override Port[] CreatePorts() => new[] {
            Port.Enter("On Start").SetExternal(true),
            Port.Exit(),
        };

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
