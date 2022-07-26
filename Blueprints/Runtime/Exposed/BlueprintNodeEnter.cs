using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Enter", Category = "Exposed", Color = BlueprintColors.Node.Exposed)]
    public sealed class BlueprintNodeEnter : BlueprintNode, IBlueprintEnter {

        [SerializeField] private string _parameter;

        internal override bool HasExposedPorts => true;

        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter(_parameter).Exposed(),
            Port.Exit()
        };

        void IBlueprintEnter.Enter(int port) {
            if (port == 0) Call(port: 1);
        }

    }

}