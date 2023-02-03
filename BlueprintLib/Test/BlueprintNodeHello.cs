using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib.Test {

    [Serializable]
    [BlueprintNodeMeta(Name = "Hello", Category = "Test")]
    public class BlueprintNodeHello : BlueprintNode, IBlueprintEnter {

        [SerializeField] private string _defaultText;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Print"),
            Port.Exit(),
            Port.Input<string>("Text"),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            Debug.Log($"Hello {ReadInputPort(2, _defaultText)}!");

            CallExitPort(1);
        }
    }

}
