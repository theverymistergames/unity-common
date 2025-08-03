using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Collisions.Detectors {
    
    public sealed class MaterialDetectorRegisterer : MonoBehaviour {

        [SerializeField] private MaterialDetectorBase _materialDetector;
        [SerializeField] private LabelValue _materialDetectorGroup;

        private void OnEnable() {
            var group = Services.Get<MaterialDetectorGroup>(_materialDetectorGroup.GetValue());
            if (group == null) return;
            
            group.AddDetector(_materialDetector);
        }

        private void OnDisable() {
            var group = Services.Get<MaterialDetectorGroup>(_materialDetectorGroup.GetValue());
            if (group == null) return;
            
            group.RemoveDetector(_materialDetector);
        }
    }
    
}