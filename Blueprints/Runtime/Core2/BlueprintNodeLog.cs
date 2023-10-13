using System;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintNodeLogSource :
        BlueprintSource<BlueprintNodeLog>,
        BlueprintSources.Enter<BlueprintNodeLog>,
        BlueprintSources.Output<BlueprintNodeLog, string> { }

    [Serializable]
    public struct BlueprintNodeLog : IBlueprintNode, IBlueprintEnter2, IBlueprintOutput2<string> {

        public string text;

        public void SetDefaultValues() {
            text = "Default text";
        }

        public void CreatePorts(IBlueprintMeta meta, long id) {
            meta.AddPort(id, 0, Port.Enter());
            meta.AddPort(id, 1, Port.Exit());
            meta.AddPort(id, 2, Port.Input<string>());
            meta.AddPort(id, 3, Port.Output<string>());
        }

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
