using System;
using MisterGames.Blueprints;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "InstantTween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeInstantTween : BlueprintNode, IBlueprintOutput<ITween> {

        private readonly InstantTween _tween = new InstantTween();

        public override Port[] CreatePorts() => new[] {
            Port.Input<ITweenInstantAction>("Tween Instant Action"),
            Port.Output<ITween>("Tween"),
        };

        public ITween GetOutputPortValue(int port) {
            if (port != 1) return null;

            _tween.action = ReadInputPort<ITweenInstantAction>(0);

            return _tween;
        }
    }

}
