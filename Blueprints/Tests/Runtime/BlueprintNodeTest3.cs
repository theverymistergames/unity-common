﻿using System;
using MisterGames.Blueprints.Core2;

namespace Core {

    [Serializable]
    public sealed class BlueprintSourceTest3 : BlueprintSource<BlueprintNodeTest3>,
        BlueprintSources.IEnter<BlueprintNodeTest3>,
        BlueprintSources.IOutput<BlueprintNodeTest3, int> { }

    [Serializable]
    public struct BlueprintNodeTest3 : IBlueprintNode, IBlueprintEnter2, IBlueprintOutput2<int> {

        public int pickedPort;

        public void SetDefaultValues(long id) {
            pickedPort = -1;
        }

        public void CreatePorts(IBlueprintMeta meta, long id) { }

        public void OnEnterPort(IBlueprint blueprint, long id, int port) {
            pickedPort = port;
        }

        public int GetPortValue(IBlueprint blueprint, long id, int port) {
            return pickedPort;
        }
    }

}
