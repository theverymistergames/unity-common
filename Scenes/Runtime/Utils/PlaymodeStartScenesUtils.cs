using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Scenes.Core;
using UnityEngine;

namespace MisterGames.Scenes.Utils {
    
    internal static class PlaymodeStartScenesUtils {
        
        [Serializable]
        private struct ScenesList {
            public List<string> sceneNames;
        }
        
        public static void SavePlaymodeStartScene(string sceneName) {
            SavePlaymodeStartScenes(new[] { sceneName });
        }
        
        public static void SavePlaymodeStartScenes(IEnumerable<string> sceneNames, string activeSceneName = null) {
            var list = sceneNames.Distinct().ToList();
            
            if (activeSceneName != null) {
                if (!list.Contains(activeSceneName)) list.Add(activeSceneName);
                
                for (int i = 0; i < list.Count; i++) {
                    if (list[i] != activeSceneName) continue;

                    list[i] = list[0];
                    list[0] = activeSceneName;
                    break;
                }
            }

            PlayerPrefs.SetString(GetPlaymodeStartSceneKey(), JsonUtility.ToJson(new ScenesList { sceneNames = list }));
            PlayerPrefs.Save();
        }
        
        public static void DeletePlaymodeStartScenes() {
            PlayerPrefs.DeleteKey(GetPlaymodeStartSceneKey());
        }

        /// <summary>
        /// Desired active scene will be first in a start scenes list. 
        /// </summary>
        public static bool IsPlaymodeStartScenesOverrideEnabled(out IReadOnlyList<string> startScenes) {
            startScenes = null;
            
            if (!Application.isPlaying) return false;
            
            var playmodeStartScenes = JsonUtility
                .FromJson<ScenesList>(PlayerPrefs.GetString(GetPlaymodeStartSceneKey()))
                .sceneNames;

            if (playmodeStartScenes is not { Count: > 0 } ||
                playmodeStartScenes.Contains(SceneLoader.RootScene)) 
            {
                return false;
            }
            
            startScenes = playmodeStartScenes;
            return true;
        }

        private static string GetPlaymodeStartSceneKey() {
            return $"{nameof(PlaymodeStartScenesUtils)}_playmodeStartScene";
        }
    }
    
}