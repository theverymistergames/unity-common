using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Routines;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Delay", Category = "Time", Color = BlueprintColors.Node.Time)]
    public sealed class BlueprintNodeDelay : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private float _defaultDuration;

        private readonly SingleJobHandler _handler = new SingleJobHandler();
        private TimeDomain _timeDomain;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter("Start"),
            Port.Enter("Cancel"),
            Port.Input<float>("Duration"),
            Port.Exit(),
        };

        protected override void OnInit() {
            _handler.Stop();
            _timeDomain = runner.TimeDomain;
        }

        protected override void OnTerminate() {
            _handler.Stop();
        }

        void IBlueprintEnter.Enter(int port) {
            if (port == 0) {
                float duration = Read(2, _defaultDuration);
                Jobs.Do(_timeDomain.Delay(duration))
                    .Then(OnDelayFinished)
                    .StartFrom(_handler);

                return;
            }

            if (port == 1) {
                _handler.Stop();
            }
        }

        private void OnDelayFinished() {
            Call(port: 3);
        }
    }

}