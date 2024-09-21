using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.GameObjects {
    
    public static class GameObjectExtensions {

        public static T GetComponentFromCollider<T>(this Collider collider) {
            return collider.attachedRigidbody != null 
                ? collider.attachedRigidbody.GetComponent<T>() 
                : collider.GetComponent<T>();
        }
        
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
    }
    
}