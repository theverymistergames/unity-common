using System;
using MisterGames.Blueprints;
using MisterGames.Common.Save;
using UnityEngine;

namespace MisterGames.BlueprintLib.Save {
    
    [Serializable]
    [BlueprintNode(Name = "Save Callbacks", Category = "Save")]
    public sealed class BlueprintNodeSaveCallbacks : IBlueprintNode, ISaveable {

        [SerializeField] private bool _notifyLoadAtStart;
        
        private IBlueprint _blueprint;
        private NodeToken _token;
        
        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Exit("On Save"));
            meta.AddPort(id, Port.Exit("On After Save"));
            meta.AddPort(id, Port.Exit("On Load"));
            meta.AddPort(id, Port.Exit("On After Load"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = blueprint;
            _token = token;
            
            SaveSystem.Instance.Register(this, _notifyLoadAtStart);
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = default;
            _token = default;
            
            SaveSystem.Instance.Unregister(this);
        }

        public void OnSaveData(ISaveSystem saveSystem) {
            _blueprint?.Call(_token, 0);
        }

        public void OnAfterSaveData(ISaveSystem saveSystem) {
            _blueprint?.Call(_token, 1);
        }

        public void OnLoadData(ISaveSystem saveSystem) {
            _blueprint?.Call(_token, 2);
        }

        public void OnAfterLoadData(ISaveSystem saveSystem) {
            _blueprint?.Call(_token, 3);
        }
    }
    
}