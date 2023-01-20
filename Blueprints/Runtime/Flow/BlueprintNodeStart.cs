using System;

namespace MisterGames.Blueprints.External {

    [Serializable]
    [BlueprintNodeMeta(Name = "Start", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeStart : BlueprintNode, IBlueprintEnter, IBlueprintStart {

        public override Port[] CreatePorts() => new[] {
            Port.Enter("On Start").SetExternal(true),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            if (port == 0) CallPort(1);
        }

        public void OnStart() {
            CallPort(1);
        }
    }

}
