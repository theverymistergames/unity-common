using System;
using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Common.Data;
using MisterGames.Common.Labels;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Detectors {
    
    public sealed class GroundLayerMaterialDetector : MaterialDetectorBase {

        [SerializeField] private CollisionDetectorBase _collisionDetector;
        [SerializeField] private LabelValue _defaultMaterial;
        [SerializeField] private Optional<LabelValue> _noContactMaterial;
        [SerializeField] [Min(0f)] private float _weight = 1f;
        [SerializeField] private MaterialData[] _materials;
        
        [Serializable]
        private struct MaterialData {
            public LayerMask layerMask;
            public LabelValue material;
        }

        private readonly List<MaterialInfo> _materialList = new();
        
        private int _lastContactHash;
        private int _lastContactMaterialId;
        
        public override IReadOnlyList<MaterialInfo> GetMaterials(Vector3 point, Vector3 normal) {
            _materialList.Clear();
            
            if (!_collisionDetector.HasContact) {
                _lastContactHash = 0;

                if (_noContactMaterial.HasValue) {
                    _materialList.Add(new MaterialInfo(_noContactMaterial.Value.GetValue(), _weight));
                }
                
                return _materialList;
            }

            var info = _collisionDetector.CollisionInfo;
            int hash = info.collider.GetInstanceID();
            
            if (hash == _lastContactHash) {
                _materialList.Add(new MaterialInfo(_lastContactMaterialId, _weight));
                return _materialList;
            }

            _lastContactHash = hash;

            if (info.collider.TryGetComponent(out SurfaceMaterial surfaceMaterial)) {
                _lastContactMaterialId = surfaceMaterial.MaterialId;
                _materialList.Add(new MaterialInfo(_lastContactMaterialId, _weight));
                return _materialList;
            }
            
            int layer = info.collider.gameObject.layer;
            
            for (int i = 0; i < _materials.Length; i++) {
                ref var data = ref _materials[i];
                if (!data.layerMask.Contains(layer)) continue;

                _lastContactMaterialId = data.material.GetValue();
                _materialList.Add(new MaterialInfo(_lastContactMaterialId, _weight));
                return _materialList;
            }
            
            _lastContactMaterialId = _defaultMaterial.GetValue();
            _materialList.Add(new MaterialInfo(_lastContactMaterialId, _weight));
            return _materialList;
        }
    }
    
}