using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
        
        public static void SetDirty(this IReadOnlyList<GameObject> gameObjects) {
#if UNITY_EDITOR
            for (int i = 0; i < gameObjects.Count; i++) {
                if (gameObjects[i] != null) EditorUtility.SetDirty(gameObjects[i]);
            }      
#endif
        }
        
        public static void SetEnabled(this IReadOnlyList<Object> objects, bool active) {
            for (int i = 0; i < objects.Count; i++) {
                objects[i].SetEnabled(active);
            }
        }
        
        public static void SetEnabled(this HashSet<Object> objects, bool active) {
            foreach (var obj in objects) {
                obj.SetEnabled(active);
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
                
                case Renderer renderer:
                    renderer.enabled = enabled;
                    break;
            }
        }
        
        public static bool IsEnabled(this Object obj) {
            return obj switch {
                GameObject go => go.activeSelf && go.activeInHierarchy,
                Behaviour bhv => bhv.enabled,
                Collider collider => collider.enabled,
                Renderer renderer => renderer.enabled,
                _ => false
            };
        }

        public static string GetPathInScene(this GameObject gameObject, bool includeSceneName = true) {
            return gameObject.transform.GetPathInScene(includeSceneName);
        }

        public static string GetPathInScene(this Component component, bool includeSceneName = true) {
            if (component == null) return string.Empty;

            var transform = component.transform;
            
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