﻿namespace MisterGames.Blueprints.Compile {

    public sealed class RuntimeBlueprint {

        private readonly BlueprintNode[] _nodes;

        public RuntimeBlueprint(BlueprintNode[] nodes) {
            _nodes = nodes;
        }

        public void Initialize(IBlueprintHost host) {
            for (int i = 0; i < _nodes.Length; i++) {
                _nodes[i].OnInitialize(host);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < _nodes.Length; i++) {
                _nodes[i].OnDeInitialize();
            }
        }

        public void Start() {
            for (int i = 0; i < _nodes.Length; i++) {
                if (_nodes[i] is IBlueprintStart start) start.OnStart();
            }
        }
    }

}
