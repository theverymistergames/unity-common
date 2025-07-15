using System.Collections.Generic;
using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Collisions.Detectors {
    
    public sealed class MaterialDetectorGroup : MaterialDetectorBase {

        [SerializeField] private MaterialDetectorBase[] _detectorsPrimary;
        [SerializeField] private MaterialDetectorBase[] _detectorsSecondary;

        private readonly List<MaterialInfo> _materialList = new(); 
        
        public override IReadOnlyList<MaterialInfo> GetMaterials() {
            _materialList.Clear();
            
            for (int i = _detectorsPrimary.Length - 1; i >= 0; i--) {
                var detector = _detectorsPrimary[i];
                
                var materials = detector.GetMaterials();
                if (materials.Count == 0) continue;

                _materialList.AddRange(materials);
                break;
            }
            
            for (int i = _detectorsSecondary.Length - 1; i >= 0; i--) {
                var detector = _detectorsSecondary[i];
                
                var materials = detector.GetMaterials();
                if (materials.Count == 0) continue;

                _materialList.AddRange(materials);
                break;
            }

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