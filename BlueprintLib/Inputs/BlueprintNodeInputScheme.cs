using System;
using MisterGames.Blueprints;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceInputScheme :
        BlueprintSource<BlueprintNodeInputScheme2>,
        BlueprintSources.IEnter<BlueprintNodeInputScheme2>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Input Scheme", Category = "Input", Color = BlueprintLibColors.Node.Input)]
    public struct BlueprintNodeInputScheme2 : IBlueprintNode, IBlueprintEnter2 {

        [SerializeField] private InputChannel _channel;
        [SerializeField] private InputScheme _scheme;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Activate"));
            meta.AddPort(id, Port.Enter("Deactivate"));
            meta.AddPort(id, Port.Exit());
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            switch (port) {
                case 0:
                    _channel.ActivateInputScheme(_scheme);
                    break;

                case 1:
                    _channel.DeactivateInputScheme(_scheme);
                    break;
            }

            blueprint.Call(token, 2);
        }
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Input Scheme", Category = "Input", Color = BlueprintLibColors.Node.Input)]
    public sealed class BlueprintNodeInputScheme : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private InputChannel _channel;
        [SerializeField] private InputScheme _scheme;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Activate"),
            Port.Enter("Deactivate"),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            switch (port) {
                case 0:
                    _channel.ActivateInputScheme(_scheme);
                    break;

                case 1:
                    _channel.DeactivateInputScheme(_scheme);
                    break;
            }

            Ports[2].Call();
        }
    }

}
