using UnityEngine;

namespace MisterGames.Character.Phys {
    
    public sealed class MaterialDetectorGroup : MaterialDetectorBase {

        [SerializeField] private MaterialDetectorBase[] _detectors;

        public override bool TryGetMaterial(out int materialId, out int priority) {
            int mat = 0;
            int topPriority = -1;
            
            for (int i = 0; i < _detectors.Length; i++) {
                if (!_detectors[i].TryGetMaterial(out int matId, out int prior) ||
                    topPriority >= 0 && prior < topPriority) 
                { 
                    continue;    
                }

                topPriority = prior;
                mat = matId;
            }

            materialId = mat;
            priority = topPriority;
            
            return topPriority >= 0;
        }
    }
    
}