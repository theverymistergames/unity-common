using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Character.Phys {
    
    public abstract class MaterialDetectorBase : MonoBehaviour {

        public abstract IReadOnlyList<MaterialInfo> GetMaterials();
    }
    
}