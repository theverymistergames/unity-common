using System.Linq;
using System.Text;
using MisterGames.Blueprints;
using MisterGames.Common.GameObjects;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Editor.Utils {
    
    public static class FindBlueprintUsagesMenu {
        
        [MenuItem("Assets/Find blueprint usages on all scenes")]
        private static void FindBlueprintUsagesOnAllScenes() {
            if (Application.isPlaying) {
                Debug.LogWarning($"Finding blueprint usages on all scenes is not allowed in playmode.");
                return;
            }
            
            var blueprintAsset = Selection.activeObject as BlueprintAsset;
            
            if (blueprintAsset == null) {
                Debug.LogWarning($"Trying to find usages of object <color=yellow>{Selection.activeObject}</color> that is not a {nameof(BlueprintAsset)}.");
                return;
            }
            
            string[] openedScenes = SceneUtils.GetOpenedScenes().Select(s => s.path).ToArray();
            var sceneAssets = SceneLoaderSettings.GetAllSceneAssets().ToArray();
            int totalCount = 0;
            int containingScenesCount = 0;
            
            for (int i = 0; i < sceneAssets.Length; i++) {
                var scene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneAssets[i]), OpenSceneMode.Single);
                int countOnScene = FindBlueprintUsagesOnScene(blueprintAsset, scene);
                
                totalCount += countOnScene;
                containingScenesCount += countOnScene > 0 ? 1 : 0;
            }
            
            Debug.Log($"Found total <color=yellow>{totalCount}</color> usages " +
                      $"of blueprint <color=yellow>{blueprintAsset.name}</color> " +
                      $"on <color=white>{containingScenesCount}/{sceneAssets.Length}</color> scenes");

            for (int i = 0; i < openedScenes.Length; i++) {
                EditorSceneManager.OpenScene(openedScenes[i], i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive);
            }
        }

        [MenuItem("Assets/Find blueprint usages on opened scenes")]
        private static void FindBlueprintUsagesOnOpenedScenes() {
            var blueprintAsset = Selection.activeObject as BlueprintAsset;
            
            if (blueprintAsset == null) {
                Debug.LogWarning($"Trying to find usages of object <color=yellow>{Selection.activeObject}</color> that is not a {nameof(BlueprintAsset)}.");
                return;
            }
            
            var openedScenes = SceneUtils.GetOpenedScenes();
            int totalCount = 0;
            int containingScenesCount = 0;
            
            for (int i = 0; i < openedScenes.Count; i++) {
                int countOnScene = FindBlueprintUsagesOnScene(blueprintAsset, openedScenes[i]);
                
                totalCount += countOnScene;
                containingScenesCount += countOnScene > 0 ? 1 : 0;
            }
            
            Debug.Log($"Found total <color=yellow>{totalCount}</color> usages " +
                      $"of blueprint <color=yellow>{blueprintAsset.name}</color> " +
                      $"on <color=white>{containingScenesCount}/{openedScenes.Count}</color> scenes.");
        }
        
        [MenuItem("Assets/Find blueprint usages in all prefabs")]
        private static void FindBlueprintUsagesInAllPrefabs() {
            var blueprintAsset = Selection.activeObject as BlueprintAsset;
            
            if (blueprintAsset == null) {
                Debug.LogWarning($"Trying to find usages of object <color=yellow>{Selection.activeObject}</color> that is not a {nameof(BlueprintAsset)}.");
                return;
            }

            var allPrefabs = AssetDatabase.FindAssets("a:assets t:Prefab")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<GameObject>).ToArray();
                
            int totalCount = 0;
            int containingPrefabsCount = 0;
            
            for (int i = 0; i < allPrefabs.Length; i++) {
                int countInPrefab = FindBlueprintUsagesInPrefab(blueprintAsset, allPrefabs[i]);
                
                totalCount += countInPrefab;
                containingPrefabsCount += countInPrefab > 0 ? 1 : 0;
            }
            
            Debug.Log($"Found total <color=yellow>{totalCount}</color> usages " +
                      $"of blueprint <color=yellow>{blueprintAsset.name}</color> " +
                      $"in <color=white>{containingPrefabsCount}/{allPrefabs.Length}</color> prefabs.");
        }

        [MenuItem("Assets/Find blueprint usages on all scenes", isValidateFunction: true)]
        [MenuItem("Assets/Find blueprint usages on opened scenes", isValidateFunction: true)]
        [MenuItem("Assets/Find blueprint usages in all prefabs", isValidateFunction: true)]
        private static bool IsBlueprintSelectedValidation() {
            return Selection.activeObject is BlueprintAsset;
        }
        
        private static int FindBlueprintUsagesOnScene(BlueprintAsset blueprintAsset, Scene scene)
        {
            var rootObjects = scene.GetRootGameObjects();
            int count = 0;
            StringBuilder sb = null;

            for (int i = 0; i < rootObjects.Length; i++)
            {
                var obj = rootObjects[i];
                var children = obj.GetComponentsInChildren<BlueprintRunner>(true);

                for (int j = 0; j < children.Length; j++) {
                    var runner = children[j];
                    if (runner.BlueprintAsset != blueprintAsset) continue;

                    sb ??= new StringBuilder();
                    sb.AppendLine($" + {runner.transform.GetPathInScene()}");
                    
                    count++;
                }
            }
            
            if (count > 0)
            {
                Debug.Log($"Found <color=yellow>{count}</color> usages " +
                          $"of blueprint <color=yellow>{blueprintAsset.name}</color> " +
                          $"on scene <color=white>{scene.name}</color>:\n{sb}");
            }
            
            return count;
        }
        
        private static int FindBlueprintUsagesInPrefab(BlueprintAsset blueprintAsset, GameObject prefabToSearch)
        {
            int count = 0;
            StringBuilder sb = null;

            var children = prefabToSearch.GetComponentsInChildren<BlueprintRunner>(true);

            for (int j = 0; j < children.Length; j++) {
                var runner = children[j];
                if (runner.BlueprintAsset != blueprintAsset) continue;

                sb ??= new StringBuilder();
                sb.AppendLine($" + {runner.transform.GetPathInScene()}");
                    
                count++;
            }
            
            
            if (count > 0)
            {
                Debug.Log($"Found <color=yellow>{count}</color> usages " +
                          $"of blueprint <color=yellow>{blueprintAsset.name}</color> " +
                          $"in prefab <color=white>{AssetDatabase.GetAssetPath(prefabToSearch)}</color>:\n{sb}");
            }
            
            return count;
        }
    }
    
}