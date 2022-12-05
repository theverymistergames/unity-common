using System;
using MisterGames.Common.Data;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Blueprints.Core {

    public interface IBlueprintHost {

        event Action<BlueprintNode, BlueprintNode> OnFlow;
        Blueprint Instance { get; }
        Blueprint Source { get; }

        void OnCalled(BlueprintNode source, BlueprintNode target);

    }
    
    public sealed class BlueprintRunner : MonoBehaviour, IBlueprintHost {

        [SerializeField] private PlayerLoopStage _timeSourceStage;
        public ITimeSource TimeSource => TimeSources.Get(_timeSourceStage);

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
            
            var properties = _sceneReferences.GetKeys();
            for (int i = 0; i < properties.Count; i++) {
                string property = properties[i];
                var data = _sceneReferences.Get(property);
                if (!data.HasValue) continue;
                
                int hash = Blackboard.StringToHash(property);
                _instance.Blackboard.Set(hash, data.Value);
            }
        }

        void IBlueprintHost.OnCalled(BlueprintNode source, BlueprintNode target) {
            target.FlowCount++;
            _onFlow.Invoke(source, target);
        }

    }

}
