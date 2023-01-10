using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core {

    public interface IBlueprintHost {

        event Action<BlueprintNode, BlueprintNode> OnFlow;
        Blueprint Instance { get; }
        Blueprint Source { get; }

        void OnCalled(BlueprintNode source, BlueprintNode target);

    }
    
    public sealed class BlueprintRunner : MonoBehaviour, IBlueprintHost {

        [SerializeField] private Blueprint _blueprint;
        Blueprint IBlueprintHost.Source => _blueprint;

        [SerializeField] private bool _launchOnStart = true;
        [SerializeField] private Map<string, GameObject> _sceneReferences;
        
        private event Action<BlueprintNode, BlueprintNode> _onFlow = delegate {  };
        event Action<BlueprintNode, BlueprintNode> IBlueprintHost.OnFlow {
            add => _onFlow += value;
            remove => _onFlow -= value;
        }

        private Blueprint _instance;
        Blueprint IBlueprintHost.Instance => _instance;
        
        private void Awake() {
            _instance = Instantiate(_blueprint);
            _instance.name = $"{_blueprint.name} (Runtime)";
            
            using (var resolver = new BlueprintResolver()) {
                resolver.Prepare(_blueprint);
                _instance.ResolveLinks(_blueprint, resolver);
            }

            InitBlackboard();
            _instance.Init(this, this);
        }

        private void OnDestroy() {
            _instance.Terminate();
            _instance = null;
        }

        private void Start() {
            if (_launchOnStart) Launch();
        }

        public void Launch() {
            _instance.Start();
        }
        
        private void InitBlackboard() {
            _instance.Blackboard.Init();
            
            for (int i = 0; i < _sceneReferences.Count; i++) {
                var property = _sceneReferences.GetKeyAt(i);
                var data = _sceneReferences[property];
                if (data == null) continue;
                
                int hash = Blackboard.StringToHash(property);
                _instance.Blackboard.Set(hash, data);
            }
        }

        void IBlueprintHost.OnCalled(BlueprintNode source, BlueprintNode target) {
            target.FlowCount++;
            _onFlow.Invoke(source, target);
        }

    }

}
