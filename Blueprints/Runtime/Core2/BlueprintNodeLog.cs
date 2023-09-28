using System;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintNodeLogFactory : BlueprintFactory<BlueprintNodeLog.Data> {
        public override BlueprintNode2 CreateNode() => new BlueprintNodeLog();
    }

    [Serializable]
    public sealed class BlueprintNodeLog : BlueprintNode2, IBlueprintEnter2, IBlueprintOutput2<string> {

        [Serializable]
        public struct Data {
            public string text;
        }

        public void OnCreateNode(IBlueprint blueprint, long id) {
            ref var data = ref blueprint.GetData<Data>(id);

            data.text = "Default text";
        }

        public override Port[] CreatePorts(IBlueprint blueprint, long id) => new[] {
            Port.Enter(),
            Port.Exit(),
            Port.Input<string>(),
            Port.Output<string>(),
        };

        public void OnEnterPort(IBlueprint blueprint, long id, int port) {
            if (port != 0) return;

            ref var data = ref blueprint.GetData<Data>(id);
            Debug.Log(data.text);

            blueprint.Call(id, 1);
        }

        public string GetOutputPortValue(IBlueprint blueprint, long id, int port) {
            return blueprint.Read<string>(id, 2);
        }
    }

}
