﻿using System;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public class BlueprintSourceStart :
        BlueprintSource<BlueprintNodeStart2>,
        BlueprintSources.IStartCallback<BlueprintNodeStart2> { }

    [Serializable]
    [BlueprintNode(Name = "Start", Category = "External", Color = BlueprintColors.Node.External)]
    public struct BlueprintNodeStart2 : IBlueprintNode, IBlueprintStartCallback {

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Exit());
        }

        public void OnStart(IBlueprint blueprint, NodeId id) {
            blueprint.Call(id, 0);
        }
    }

}
