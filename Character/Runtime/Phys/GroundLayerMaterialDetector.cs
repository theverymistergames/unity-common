using System;
using MisterGames.Collisions.Core;
using MisterGames.Common.Data;
using MisterGames.Common.Labels;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Character.Phys {
    
    public sealed class GroundLayerMaterialDetector : MaterialDetectorBase {

        [SerializeField] private CollisionDetectorBase _collisionDetector;
        [SerializeField] private LabelValue _priority;
        [SerializeField] private LabelValue _defaultMaterial;
        [SerializeField] private Optional<LabelValue> _noContactMaterial;
        [SerializeField] private MaterialData[] _materials;
        
        [Serializable]
        private struct MaterialData {
            public LayerMask layerMask;
            public LabelValue material;
        }

        private int _lastContactHash;
        private int _lastContactMaterialId;
        
        public override bool TryGetMaterial(out int materialId, out int priority) {
            priority = _priority.GetValue();
            
            if (!_collisionDetector.HasContact) {
                _lastContactHash = 0;
                materialId = _noContactMaterial.Value.GetValue();
                return _noContactMaterial.HasValue;
            }

            var info = _collisionDetector.CollisionInfo;
            int hash = info.transform.GetHashCode();
            
            if (hash == _lastContactHash) {
                materialId = _lastContactMaterialId;
                return true;
            }

            _lastContactHash = hash;

            if (info.transform.TryGetComponent(out SurfaceMaterial surfaceMaterial)) {
                _lastContactMaterialId = surfaceMaterial.MaterialId;
                materialId = _lastContactMaterialId;
                return true;
            }
            
            int layer = info.transform.gameObject.layer;
            
            for (int i = 0; i < _materials.Length; i++) {
                ref var data = ref _materials[i];
                if (!data.layerMask.Contains(layer)) continue;

                _lastContactMaterialId = data.material.GetValue();
                materialId = _lastContactMaterialId;
                return true;
            }
            
            _lastContactMaterialId = _defaultMaterial.GetValue();
            materialId = _lastContactMaterialId;
            return true;
        }
    }
    
}