using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Bezier.Utility {
    
    public static class UoUtility {
        
        public static GameObject Create(string name, GameObject parent, params Type[] components) {
            var res = new GameObject(name, components);
            res.transform.parent = parent.transform;
            res.transform.localPosition = Vector3.zero;
            res.transform.localScale = Vector3.one;
            res.transform.localRotation = Quaternion.identity;
            return res;
        }

        public static GameObject Instantiate(GameObject prefab, Transform parent) {
            var res = Object.Instantiate(prefab, parent);
            res.transform.localPosition = Vector3.zero;
            res.transform.localRotation = Quaternion.identity;
            res.transform.localScale = Vector3.one;
            return res;
        }

        public static void Destroy(GameObject go) {
            if (Application.isPlaying) {
                Object.Destroy(go);
            } else {
                Object.DestroyImmediate(go);
            }
        }

        public static void Destroy(Component comp) {
            if (Application.isPlaying) {
                Object.Destroy(comp);
            } else {
                Object.DestroyImmediate(comp);
            }
        }

        public static void DestroyChildren(GameObject go) {
            var childList = go.transform.Cast<Transform>().ToList();
            foreach (var childTransform in childList) {
                Destroy(childTransform.gameObject);
            }
        }
        
    }
}
