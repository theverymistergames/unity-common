using System.Linq;
using System.Text;
using MisterGames.Common.GameObjects;
using MisterGames.Scenes.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Editor.Utils {
    
    public static class FindPrefabUsagesMenu {
        
        [MenuItem("Assets/Find prefab usages on all scenes")]
        private static void FindPrefabUsagesOnAllScenes() {
            if (Application.isPlaying) {
                Debug.LogWarning($"Finding prefab usages on all scenes is not allowed in playmode.");
                return;
            }
            
            var prefab = Selection.activeObject as GameObject;
            
            if (prefab == null) {
                Debug.LogWarning($"Trying to find usages of object <color=yellow>{Selection.activeObject}</color> that is not a prefab.");
                return;
            }
            
            string[] openedScenes = SceneUtils.GetOpenedScenes().Select(s => s.path).ToArray();
            var sceneAssets = SceneLoaderSettings.GetAllSceneAssets().ToArray();
            int totalCount = 0;
            int containingScenesCount = 0;
            
            for (int i = 0; i < sceneAssets.Length; i++) {
                var scene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneAssets[i]), OpenSceneMode.Single);
                int countOnScene = FindPrefabUsagesOnScene(prefab, scene);
                
                totalCount += countOnScene;
                containingScenesCount += countOnScene > 0 ? 1 : 0;
            }
            
            Debug.Log($"Found total <color=yellow>{totalCount}</color> usages " +
                      $"of prefab <color=yellow>{prefab.name}</color> " +
                      $"on <color=white>{containingScenesCount}/{sceneAssets.Length}</color> scenes");

            for (int i = 0; i < openedScenes.Length; i++) {
                EditorSceneManager.OpenScene(openedScenes[i], i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive);
            }
        }

        [MenuItem("Assets/Find prefab usages on opened scenes")]
        private static void FindPrefabUsagesOnOpenedScenes() {
            var prefab = Selection.activeObject as GameObject;
            
            if (prefab == null) {
                Debug.LogWarning($"Trying to find usages of object <color=yellow>{Selection.activeObject}</color> that is not a prefab.");
                return;
            }
            
            var openedScenes = SceneUtils.GetOpenedScenes();
            int totalCount = 0;
            int containingScenesCount = 0;
            
            for (int i = 0; i < openedScenes.Count; i++) {
                int countOnScene = FindPrefabUsagesOnScene(prefab, openedScenes[i]);
                
                totalCount += countOnScene;
                containingScenesCount += countOnScene > 0 ? 1 : 0;
            }
            
            Debug.Log($"Found total <color=yellow>{totalCount}</color> usages " +
                      $"of prefab <color=yellow>{prefab.name}</color> " +
                      $"on <color=white>{containingScenesCount}/{openedScenes.Count}</color> scenes.");
        }
        
        [MenuItem("Assets/Find prefab usages in all prefabs")]
        private static void FindPrefabUsagesInAllPrefabs() {
            var prefab = Selection.activeObject as GameObject;
            
            if (prefab == null) {
                Debug.LogWarning($"Trying to find usages of object <color=yellow>{Selection.activeObject}</color> that is not a prefab.");
                return;
            }

            var allPrefabs = AssetDatabase.FindAssets("a:assets t:Prefab")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<GameObject>).ToArray();
                
            int totalCount = 0;
            int containingPrefabsCount = 0;
            
            for (int i = 0; i < allPrefabs.Length; i++) {
                int countInPrefab = FindPrefabUsagesInPrefab(prefab, allPrefabs[i]);
                
                totalCount += countInPrefab;
                containingPrefabsCount += countInPrefab > 0 ? 1 : 0;
            }
            
            Debug.Log($"Found total <color=yellow>{totalCount}</color> usages " +
                      $"of prefab <color=yellow>{prefab.name}</color> " +
                      $"in <color=white>{containingPrefabsCount}/{allPrefabs.Length}</color> prefabs.");
        }

        [MenuItem("Assets/Find prefab usages on all scenes", isValidateFunction: true)]
        private static bool FindPrefabUsagesOnScenesValidation() {
            return PrefabUtility.IsPartOfAnyPrefab(Selection.activeObject);
        }

        [MenuItem("Assets/Find prefab usages on opened scenes", isValidateFunction: true)]
        private static bool FindPrefabUsagesOnOpenedScenesValidation() {
            return PrefabUtility.IsPartOfAnyPrefab(Selection.activeObject);
        }
        
        [MenuItem("Assets/Find prefab usages in all prefabs", isValidateFunction: true)]
        private static bool FindPrefabUsagesInAllPrefabsValidation() {
            return PrefabUtility.IsPartOfAnyPrefab(Selection.activeObject);
        }
        
        private static int FindPrefabUsagesOnScene(GameObject prefab, Scene scene)
        {
            var rootObjects = scene.GetRootGameObjects();
            int count = 0;
            StringBuilder sb = null;

            for (int i = 0; i < rootObjects.Length; i++)
            {
                var obj = rootObjects[i];
                var children = obj.GetComponentsInChildren<Transform>(true);

                for (int j = 0; j < children.Length; j++) {
                    var t = children[j];

                    if (PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject) != prefab) continue;

                    sb ??= new StringBuilder();
                    sb.AppendLine($" + {GameObjectExtensions.GetPathInScene(t)}");
                    
                    count++;
                }
            }
            
            if (count > 0)
            {
                Debug.Log($"Found <color=yellow>{count}</color> usages " +
                          $"of prefab <color=yellow>{prefab.name}</color> " +
                          $"on scene <color=white>{scene.name}</color>:\n{sb}");
            }
            
            return count;
        }
        
        private static int FindPrefabUsagesInPrefab(GameObject prefab, GameObject prefabToSearch)
        {
            int count = 0;
            StringBuilder sb = null;

            var children = prefabToSearch.GetComponentsInChildren<Transform>(true);

            for (int j = 0; j < children.Length; j++) {
                var t = children[j];

                if (PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject) != prefab) continue;

                sb ??= new StringBuilder();
                sb.AppendLine($" + {GameObjectExtensions.GetPathInScene(t)}");
                    
                count++;
            }
            
            
            if (count > 0)
            {
                Debug.Log($"Found <color=yellow>{count}</color> usages " +
                          $"of prefab <color=yellow>{prefab.name}</color> " +
                          $"in prefab <color=white>{AssetDatabase.GetAssetPath(prefabToSearch)}</color>:\n{sb}");
            }
            
            return count;
        }
    }
    
}