using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Routines;
using MisterGames.Scenes.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Load Scene", Category = "Scenes", Color = BlueprintLibColors.Node.Scenes)]
    public sealed class BlueprintNodeLoadScene : BlueprintNode, IBlueprintEnter, IBlueprintGetter<float> {
        
        [SerializeField] private SceneReference _scene;

        private readonly SingleJobHandler _handler = new SingleJobHandler();
        private TimeDomain _timeDomain;
        private float _progress;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter(),
            Port.Exit(),
            Port.Output<float>("Progress"),
        };

        protected override void OnInit() {
            _handler.Stop();
            _timeDomain = runner.TimeDomain;
        }

        protected override void OnTerminate() {
            _handler.Stop();
        }

        void IBlueprintEnter.Enter(int port) {
            if (port != 0) return;

            var loadingTask = SceneLoader.LoadScene(_scene.scene);

            Jobs.Do(_timeDomain.Process(() => loadingTask.Progress, OnProgress))
                .Then(OnFinish)
                .StartFrom(_handler);
        }

        float IBlueprintGetter<float>.Get(int port) {
            return port == 2 ? _progress : 0f;
        }

        private void OnProgress(float progress) {
            _progress = progress;
        }
        
        private void OnFinish() {
            Call(1);
        }
    }

}