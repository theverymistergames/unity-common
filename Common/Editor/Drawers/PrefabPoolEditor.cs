using System;
using System.Collections.Generic;
using MisterGames.Common.Pooling;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    [CustomEditor(typeof(PrefabPool))]
    public class PrefabPoolEditor : UnityEditor.Editor {
        
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            if (target is not PrefabPool pool) return;

            if (GUILayout.Button("Refresh pools")) {
                var prefabs = GetAllPrefabs(pool.SearchPrefabsInFolders);
                pool.Refresh(prefabs);

                EditorUtility.SetDirty(pool);
            }
        }

        private static GameObject[] GetAllPrefabs(string[] searchInFolders) {
            if (searchInFolders.Length == 0) return Array.Empty<GameObject>();

            string[] guids = AssetDatabase.FindAssets($"t:Prefab", searchInFolders);
            var assetNames = new HashSet<string>();
            var gameObjects = new List<GameObject>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null)
                {
                    continue;
                }

                if (assetNames.Contains(asset.name))
                {
                    Debug.LogError($"{nameof(PrefabPool)}: found multiple prefabs with name {asset.name}, "
                                   + $"consider giving different name to be able to instantiate them");
                    continue;
                }

                assetNames.Add(asset.name);
                gameObjects.Add(asset);
            }

            return gameObjects.ToArray();
        }
    }
    
}
