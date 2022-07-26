using System;
using MisterGames.Common.Lists;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Common.Data {
    
    public abstract class ScriptableSingletonBase : ScriptableObject {}
    
    public abstract class ScriptableSingleton<T> : ScriptableSingletonBase where T : ScriptableSingleton<T> {
        
        private static T _instance;
        public static T Instance {
            get {
                if (_instance != null) return _instance;

                var instances = Resources.LoadAll<T>("");
                if (instances == null || instances.IsEmpty()) {
                    if (Application.isPlaying) {
                        throw new Exception(
                            $"ScriptableSingleton instance of type [{typeof(T).Name}] is not found. " +
                            $"Can not instantiate new instance at runtime, it should be created in Unity Editor."
                        );
                    }
#if UNITY_EDITOR
                    _instance = CreateNewSingleton();
#endif
                }
                else if (instances.Length > 1) {
                    throw new Exception(
                        $"Found multiple ScriptableSingleton instances of type [{typeof(T).Name}]. " +
                        $"You should delete them all or leave one that will be used."
                    );
                }
                else {
                    _instance = instances[0];
                }
                
                if (!Application.isPlaying) _instance.OnSingletonInstanceLoaded();
                return _instance;
            }
        }

        protected virtual void OnSingletonInstanceLoaded() { }

#if UNITY_EDITOR
        private const string DataFolderPath = "Assets/Data";

        private static T CreateNewSingleton() {
            var singleton = CreateInstance<T>();
                
            if (!AssetDatabase.IsValidFolder($"{DataFolderPath}/Resources")) {
                AssetDatabase.CreateFolder(DataFolderPath, "Resources");
            }

            string path = GetDefaultAssetPath();
            
            AssetDatabase.CreateAsset(singleton, path);
            AssetDatabase.SaveAssets();

            singleton.hideFlags = HideFlags.DontUnloadUnusedAsset;
            
            Debug.Log($"Created new ScriptableSingleton instance of type [{typeof(T).Name}] at [{path}]");
            
            return singleton;
        }

        private static string GetDefaultAssetPath() {
            return $"{DataFolderPath}/Resources/{typeof(T).Name}.asset";
        }
#endif
    }
}