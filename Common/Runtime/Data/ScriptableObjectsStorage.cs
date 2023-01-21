using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Data {
    
    public sealed class ScriptableObjectsStorage : ScriptableSingleton<ScriptableObjectsStorage> {

        [SerializeField] private ScriptableObject[] _scriptableObjects;

        public static T[] FindAssetsByName<T>(string objectName) where T : ScriptableObject {
            return Instance.FindByName<T>(objectName);
        }
        
        public static T[] FindAssetsByType<T>() where T : ScriptableObject {
            return Instance.FindByType<T>();
        }

        private T[] FindByName<T>(string objectName) where T : ScriptableObject {
            var result = new List<T>();
            
            for (int i = 0; i < _scriptableObjects.Length; i++) {
                var asset = _scriptableObjects[i];
                if (asset.name == objectName && asset is T t) result.Add(t);
            }

            return result.ToArray();
        }
        
        private T[] FindByType<T>() where T : ScriptableObject {
            var result = new List<T>();
            
            for (int i = 0; i < _scriptableObjects.Length; i++) {
                var asset = _scriptableObjects[i];
                if (asset is T t) result.Add(t);
            }

            return result.ToArray();
        }

        protected override void OnSingletonInstanceLoaded() {
#if UNITY_EDITOR
            Refresh();
#endif
        }
        
#if UNITY_EDITOR
        private static readonly string[] SearchScriptableObjectsInFolders = {
            "Assets/Data",
            //"Assets/Temp",
        };

        public void Refresh() {
            _scriptableObjects = GetAllScriptableObjectsExceptSingletons().ToArray();
            EditorUtility.SetDirty(this);
        }

        private static IEnumerable<ScriptableObject> GetAllScriptableObjectsExceptSingletons() {
            return AssetDatabase
                .FindAssets($"a:assets t:{nameof(ScriptableObject)}", SearchScriptableObjectsInFolders)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !string.IsNullOrEmpty(path))
                .Select(AssetDatabase.LoadAssetAtPath<ScriptableObject>)
                .Where(asset => asset != null);
        }
#endif
    }
}
