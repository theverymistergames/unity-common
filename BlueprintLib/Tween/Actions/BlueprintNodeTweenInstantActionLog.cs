using System;
using MisterGames.Blueprints;
using MisterGames.TweenLib;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Tween Instant Action Log", Category = "Tweens/Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenInstantActionLog : BlueprintNode, IBlueprintOutput<ITweenInstantAction> {

        [SerializeField] private string _text;

        private readonly TweenInstantActionLog _action = new TweenInstantActionLog();

        public override Port[] CreatePorts() => new[] {
            Port.Input<string>("Text"),
            Port.Output<ITweenInstantAction>("Action"),
        };

        public ITweenInstantAction GetOutputPortValue(int port) {
            if (port != 1) return null;

            _action.text = ReadInputPort(0, _text);

            return _action;
        }
    }

}
