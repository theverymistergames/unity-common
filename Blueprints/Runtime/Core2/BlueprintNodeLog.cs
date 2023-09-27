using System;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintNodeLogFactory : BlueprintFactory<BlueprintNodeLog.Data> {
        public override IBlueprintNode CreateNode() => new BlueprintNodeLog();
    }

    [Serializable]
    public sealed class BlueprintNodeLog : IBlueprintNode {

        [Serializable]
        public struct Data {
            public string text;
        }

        public void OnCreateNode(IBlueprint blueprint, long id) {

        }

        public Port[] CreatePorts(IBlueprint blueprint, long id) {
            ref var data = ref blueprint.GetData<Data>(id);
            return Array.Empty<Port>();
        }
    }

}
