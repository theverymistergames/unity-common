using System;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    [BlueprintNodeMeta(Name = "Core2.Start", Category = "Core2.External", Color = BlueprintColors.Node.External)]
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
