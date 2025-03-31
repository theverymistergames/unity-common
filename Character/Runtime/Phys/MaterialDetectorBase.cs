using UnityEngine;

namespace MisterGames.Character.Phys {
    
    public abstract class MaterialDetectorBase : MonoBehaviour {

        public abstract bool TryGetMaterial(out int materialId, out int priority);
    }
    
}