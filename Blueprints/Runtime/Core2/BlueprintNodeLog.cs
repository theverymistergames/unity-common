using System;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintNodeLogFactory : BlueprintFactory<BlueprintNodeLog.Data> {
        public override IBlueprintNode CreateNode() => new BlueprintNodeLog();
    }

    [Serializable]
    public sealed class BlueprintNodeLog : IBlueprintNode, IBlueprintEnter2, IBlueprintOutput2<string> {

        [Serializable]
        public struct Data {
            public string text;
        }

        public void OnCreateNode(IBlueprint blueprint, long id) {
            ref var data = ref blueprint.GetData<Data>(id);

            data.text = "Default text";
        }

        public Port[] CreatePorts(IBlueprint blueprint, long id) => new[] {
            Port.Enter(),
            Port.Exit(),
            Port.Input<string>(),
            Port.Output<string>(),
        };

        public void OnEnterPort(int port, IBlueprint blueprint, long id) {
            if (port != 0) return;

            ref var data = ref blueprint.GetData<Data>(id);
            Debug.Log(data.text);

            blueprint.Call(id, 1);
        }

        public string GetOutputPortValue(int port, IBlueprint blueprint, long id) {
            return blueprint.Read<string>(id, 2);
        }
    }

}
