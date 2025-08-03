using System.Collections.Generic;
using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Collisions.Detectors {
    
    public sealed class MaterialDetectorGroup : MaterialDetectorBase {

        [SerializeField] private MaterialDetectorBase[] _predefinedDetectors;

        private readonly Dictionary<int, int> _detectorIdToIndexMap = new();
        private readonly List<MaterialDetectorBase> _detectorList = new();
        private readonly List<MaterialInfo> _materialList = new();

        private void Awake() {
            for (int i = 0; i < _predefinedDetectors.Length; i++) {
                AddDetector(_predefinedDetectors[i]);
            }
        }

        public void AddDetector(MaterialDetectorBase detector) {
            int id = detector.GetHashCode();
            if (!_detectorIdToIndexMap.TryAdd(id, _detectorList.Count)) return;
            
            _detectorList.Add(detector);
        }

        public void RemoveDetector(MaterialDetectorBase detector) {
            int id = detector.GetHashCode();
            if (!_detectorIdToIndexMap.Remove(id, out int index)) return;
            
            _detectorList[index] = null;
        }
        
        public override IReadOnlyList<MaterialInfo> GetMaterials(Vector3 point, Vector3 normal) {
            _materialList.Clear();

            int count = _detectorList.Count;
            int validCount = count;
            
            for (int i = count - 1; i >= 0; i--) {
                var detector = _detectorList[i];
                
                if (detector != null) {
                    var materials = detector.GetMaterials(point, normal);
                    if (materials.Count == 0) continue;

                    _materialList.AddRange(materials);
                    continue;
                }
                
                if (_detectorList[--validCount] is var swap && swap != null) 
                {
                    _detectorList[i] = swap;
                    _detectorIdToIndexMap[swap.GetHashCode()] = i;
                }
            }
            
            _detectorList.RemoveRange(validCount, count - validCount);

            float invertedWeightSum = 0f;
            for (int i = 0; i < _materialList.Count; i++) {
                invertedWeightSum += _materialList[i].weight;
            }
            
            invertedWeightSum = invertedWeightSum > 0f ? 1f / invertedWeightSum : 0f;

            for (int i = 0; i < _materialList.Count; i++) {
                var material = _materialList[i];
                _materialList[i] = new MaterialInfo(material.materialId, material.weight * invertedWeightSum);
            }
            
            return _materialList;
        }
    }
    
}