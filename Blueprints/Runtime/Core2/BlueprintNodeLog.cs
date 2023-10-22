﻿using System;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintNodeLogSource :
        BlueprintSource<BlueprintNodeLog>,
        BlueprintSources.IEnter<BlueprintNodeLog>,
        BlueprintSources.IOutput<BlueprintNodeLog, string> { }

    [Serializable]
    public struct BlueprintNodeLog : IBlueprintNode, IBlueprintEnter2, IBlueprintOutput2<string> {

        public string text;

        public void SetDefaultValues(IBlueprintMeta meta, NodeId id) {
            text = "Default text";
        }

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Exit());
            meta.AddPort(id, Port.Input<string>());
            meta.AddPort(id, Port.Output<string>());
        }

        public void OnEnterPort(IBlueprint blueprint, NodeId id, int port) {
            if (port != 0) return;

            Debug.Log(text);

            blueprint.Call(id, 1);
        }

        public string GetPortValue(IBlueprint blueprint, NodeId id, int port) {
            return blueprint.Read<string>(id, 2);
        }
    }

}
