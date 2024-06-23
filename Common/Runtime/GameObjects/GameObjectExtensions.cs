using UnityEngine;

namespace MisterGames.Common.GameObjects {
    
    public static class GameObjectExtensions {

        public static void SetupUniqueMaterial(this Renderer renderer) {
            if (renderer.material == renderer.sharedMaterial) renderer.material = new Material(renderer.sharedMaterial);
        }

        public static void SetEnabled(this Object obj, bool enabled) {
            switch (obj) {
                case GameObject go:
                    go.SetActive(enabled);
                    break;
                
                case Behaviour bhv:
                    bhv.enabled = enabled;
                    break;
                
                case Collider collider:
                    collider.enabled = enabled;
                    break;
            }
        }
    }
    
}