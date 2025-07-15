using System;
using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.Collisions.Detectors {
    
    public sealed class TerrainMaterialDetector : MaterialDetectorBase {

        [SerializeField] private CapsuleCollider _capsuleCollider;
        [SerializeField] private CollisionDetectorBase _groundDetector;
        [SerializeField] private LabelValue _defaultMaterial;
        [SerializeField] [Min(0f)] private float _weight = 1f;
        [SerializeField] private MaterialData[] _materials;
        
        [Serializable]
        private struct MaterialData {
            [Min(0)] public int textureIndex;
            public LabelValue material;
        }
        
        private readonly Dictionary<int, int> _textureIndexToMaterialIdMap = new();
        private readonly List<MaterialInfo> _materialList = new();
        private Transform _transform;
        private Terrain _terrain;
        private TerrainData _terrainData;
        private int _alphaMapWidth;
        private int _alphaMapHeight;
        private float[,,] _splatMapData;
        private int _numTextures;
        private int _lastContactHash;

        private void Awake() {
            _transform = _capsuleCollider.transform;

            FetchTextureIndexToMaterialIdMap();
        }

        private void OnEnable() {
            _groundDetector.OnContact += OnContact;
            _groundDetector.OnLostContact += OnLostContact;
        }

        private void OnDisable() {
            _groundDetector.OnContact -= OnContact;
            _groundDetector.OnLostContact -= OnLostContact;
            
            ResetTerrainData();
        }

        private void FetchTextureIndexToMaterialIdMap() {
            for (int i = 0; i < _materials.Length; i++) {
                ref var materialData = ref _materials[i];
                _textureIndexToMaterialIdMap[materialData.textureIndex] = materialData.material.GetValue();
            }
        }

        public override IReadOnlyList<MaterialInfo> GetMaterials() {
            _materialList.Clear();

            if (_terrain == null) {
                return _materialList;
            }
            
            var up = _transform.up;
            var point = _transform.TransformPoint(_capsuleCollider.center) - _capsuleCollider.height * 0.5f * up;
            var terrainCoord = ConvertToSplatMapCoordinate(_terrain, point);

            for (int i = 0; i < _numTextures; i++) 
            {
                float opacity = _splatMapData[(int) terrainCoord.z, (int) terrainCoord.x, i];
                if (opacity <= 0f) continue;
                
                int materialId = _textureIndexToMaterialIdMap.GetValueOrDefault(i, _defaultMaterial.GetValue());
                _materialList.Add(new MaterialInfo(materialId, opacity));
            }

            return _materialList;
        }

        private void OnContact() {
            var info = _groundDetector.CollisionInfo;
            int hash = info.collider.GetInstanceID();
            
            if (hash == _lastContactHash) return;

            _lastContactHash = hash;

            if (info.collider.TryGetComponent(out TerrainCollider terrainCollider)) {
                FetchTerrainData(terrainCollider);
                return;
            }
            
            ResetTerrainData();
        }

        private void OnLostContact() {
            _lastContactHash = 0;
            ResetTerrainData();
        }

        private void FetchTerrainData(TerrainCollider terrainCollider) {
            _terrain = terrainCollider.GetComponent<Terrain>();
            _terrainData = terrainCollider.terrainData;
            _alphaMapWidth = _terrainData.alphamapWidth;
            _alphaMapHeight = _terrainData.alphamapHeight;

            _splatMapData = _terrainData.GetAlphamaps(0, 0, _alphaMapWidth, _alphaMapHeight);
            _numTextures = _splatMapData.Length / (_alphaMapWidth * _alphaMapHeight);
        }

        private void ResetTerrainData() {
            _terrain = null;
            _terrainData = null;
        }

        private static Vector3 ConvertToSplatMapCoordinate(Terrain terrain, Vector3 worldPosition) {
            var terPosition = terrain.transform.position;

            return new Vector3(
                (worldPosition.x - terPosition.x) / terrain.terrainData.size.x * terrain.terrainData.alphamapWidth,
                0f,
                (worldPosition.z - terPosition.z) / terrain.terrainData.size.z * terrain.terrainData.alphamapHeight
            );
        }

#if UNITY_EDITOR
        private void OnValidate() {
            FetchTextureIndexToMaterialIdMap();
        }
#endif
        
    }
}