using System.Collections.Generic;
using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Collisions.Detectors {
    
    public abstract class MaterialDetectorBase : MonoBehaviour {

        public abstract IReadOnlyList<MaterialInfo> GetMaterials(Vector3 point);
    }
    
}