using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MisterGames.Common.GameObjects {
    
    public static class GameObjectExtensions {
        
        public static void SetupUniqueMaterial(this Renderer renderer) {
            if (renderer.material == renderer.sharedMaterial) renderer.material = new Material(renderer.sharedMaterial);
        }

        public static void SetActive(this IReadOnlyList<GameObject> gameObjects, bool active) {
            for (int i = 0; i < gameObjects.Count; i++) {
                gameObjects[i].SetActive(active);
            }
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

        public static string GetPathInScene(this GameObject gameObject, bool includeSceneName = false) {
            return gameObject.transform.GetPathInScene(includeSceneName);
        }

        public static string GetPathInScene(this Transform transform, bool includeSceneName = false) {
            if (transform == null) return string.Empty;
            
            var sb = new StringBuilder(transform.name);
            var original = transform;
            
            while (transform.parent != null) {
                sb.Insert(0, '/');
                sb.Insert(0, transform.parent.name);
                transform = transform.parent;
            }
            
            if (includeSceneName) {
                sb.Insert(0, '/');
                sb.Insert(0, original.gameObject.scene.name);
            }

            return sb.ToString();
        }
    }
    
}