using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Data {
    
    public sealed class ScriptableObjectsStorage : ScriptableSingleton<ScriptableObjectsStorage> {

        [SerializeField] private SerializedDictionary<int, List<ScriptableObject>> _nameHashToListOfScriptableObjectMap;

        public static IReadOnlyList<T> FindAssetsByName<T>(string assetName) where T : ScriptableObject {
            return Instance.FindAssetsByNameHash<T>(assetName.GetHashCode());
        }

        private IReadOnlyList<T> FindAssetsByNameHash<T>(int nameHash) where T : ScriptableObject {
            if (!_nameHashToListOfScriptableObjectMap.TryGetValue(nameHash, out var scriptableObjects)) {
                return Array.Empty<T>();
            }

            var result = new List<T>();

            for (int i = 0; i < scriptableObjects.Count; i++) {
                if (scriptableObjects[i] is T t) result.Add(t);
            }

            return result;
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
            var scriptableObjects = GetAllScriptableObjectsExceptSingletons().ToArray();

            for (int i = 0; i < scriptableObjects.Length; i++) {
                var scriptableObject = scriptableObjects[i];
                int nameHash = scriptableObject.name.GetHashCode();

                if (_nameHashToListOfScriptableObjectMap.TryGetValue(nameHash, out var nameIndices)) {
                    nameIndices.Add(scriptableObject);
                    continue;
                }

                _nameHashToListOfScriptableObjectMap[nameHash] = new List<ScriptableObject> { scriptableObject };
            }

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
