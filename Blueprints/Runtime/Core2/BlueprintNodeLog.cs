using System;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintNodeLogFactory :
        BlueprintFactory<BlueprintNodeLog>,
        BlueprintFactories.Enter<BlueprintNodeLog>,
        BlueprintFactories.Output<BlueprintNodeLog, string> { }

    [Serializable]
    public struct BlueprintNodeLog : IBlueprintNode, IBlueprintEnter2, IBlueprintOutput2<string> {

        public string text;

        public void SetDefaultValues(IBlueprintMeta blueprintMeta, long id) {
            text = "Default text";
        }

        public Port[] CreatePorts(IBlueprintMeta blueprintMeta, long id) => new[] {
            Port.Enter(),
            Port.Exit(),
            Port.Input<string>(),
            Port.Output<string>(),
        };

        public void OnEnterPort(IBlueprint blueprint, long id, int port) {
            if (port != 0) return;

            Debug.Log(text);

            blueprint.Call(id, 1);
        }

        public string GetOutputPortValue(IBlueprint blueprint, long id, int port) {
            return blueprint.Read<string>(id, 2);
        }
    }

}
