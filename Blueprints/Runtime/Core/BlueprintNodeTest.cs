using System;
using MisterGames.Blueprints;
using UnityEngine;

[Serializable]
[BlueprintNodeMeta(Name = "Test", Category = "Test Nodes")]
public class BlueprintNodeTest : BlueprintNode, IBlueprintEnter, IBlueprintOutput<string> {

    [SerializeField] private string _parameter;

    public override Port[] CreatePorts() => new [] {
        Port.Enter("Enter"),
        Port.Exit("Exit"),
        Port.Input<string>("Input String"),
        Port.Output<string>("Output String"),
    };

    public void OnEnterPort(int port) {
        // Enter port 0 called
        if (port == 0) {

            // Read input port 2
            string input = Ports[2].Get<string>();
            Debug.Log(input);

            // Call exit port 1
            Ports[1].Call();
        }
    }

    public string GetOutputPortValue(int port) {
        if (port == 3) {
            // Read input port 2
            string input = Ports[2].Get<string>();

            // Return string output when requested
            return $"Hello, {input}!";
        }

        return default;
    }
}
