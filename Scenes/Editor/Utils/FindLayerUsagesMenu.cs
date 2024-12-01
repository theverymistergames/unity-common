using System.Linq;
using System.Text;
using MisterGames.Scenes.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Editor.Utils {
    
    public sealed class FindLayerUsagesMenu : EditorWindow {

        [SerializeField] private int _layer;

        [MenuItem("MisterGames/Tools/Find layer usages")]
        private static void OpenWindow() {
            var window = GetWindow<FindLayerUsagesMenu>("Find layer usages", focus: true);
            window.minSize = new Vector2(230, 65);
        }
        
        private void OnGUI()
        {
            _layer = EditorGUILayout.LayerField("Layer", _layer);
            if (GUILayout.Button("Find on opened scenes")) FindLayerUsagesOnOpenedScenes(_layer);
            if (GUILayout.Button("Find on all scenes")) FindLayerUsagesOnAllScenes(_layer);
            if (GUILayout.Button("Find in all prefabs")) FindLayerUsagesInAllPrefabs(_layer);
        }

        private static void FindLayerUsagesOnOpenedScenes(int layer) {
            var openedScenes = SceneUtils.GetOpenedScenes();
            int totalCount = 0;
            int containingScenesCount = 0;
            
            for (int i = 0; i < openedScenes.Count; i++) {
                int countOnScene = FindLayerUsageOnScene(layer, openedScenes[i]);
                
                totalCount += countOnScene;
                containingScenesCount += countOnScene > 0 ? 1 : 0;
            }
            
            Debug.Log($"Found total <color=yellow>{totalCount}</color> usages " +
                      $"of layer <color=yellow>{LayerMask.LayerToName(layer)}</color> " +
                      $"on <color=white>{containingScenesCount}/{openedScenes.Count}</color> scenes.");
        }

        private static void FindLayerUsagesOnAllScenes(int layer) {
            if (Application.isPlaying) {
                Debug.LogWarning($"Finding layer usages on all scenes is not allowed in playmode.");
                return;
            }
            
            string[] openedScenes = SceneUtils.GetOpenedScenes().Select(s => s.path).ToArray();
            var sceneAssets = SceneLoaderSettings.GetAllSceneAssets().ToArray();
            int totalCount = 0;
            int containingScenesCount = 0;
            
            for (int i = 0; i < sceneAssets.Length; i++) {
                var scene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneAssets[i]), OpenSceneMode.Single);
                int countOnScene = FindLayerUsageOnScene(layer, scene);
                
                totalCount += countOnScene;
                containingScenesCount += countOnScene > 0 ? 1 : 0;
            }
            
            Debug.Log($"Found total <color=yellow>{totalCount}</color> usages " +
                      $"of layer <color=yellow>{LayerMask.LayerToName(layer)}</color> " +
                      $"on <color=white>{containingScenesCount}/{sceneAssets.Length}</color> scenes.");

            for (int i = 0; i < openedScenes.Length; i++) {
                EditorSceneManager.OpenScene(openedScenes[i], i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive);
            }
        }
        
        private static void FindLayerUsagesInAllPrefabs(int layer) {
            if (Application.isPlaying) {
                Debug.LogWarning($"Finding layer usages on all scenes is not allowed in playmode.");
                return;
            }
            
            var allPrefabs = AssetDatabase.FindAssets("a:assets t:Prefab")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<GameObject>).ToArray();
            
            int totalCount = 0;
            int containingPrefabsCount = 0;
            
            for (int i = 0; i < allPrefabs.Length; i++) {
                int countInPrefab = FindLayerUsageInPrefab(layer, allPrefabs[i]);
                
                totalCount += countInPrefab;
                containingPrefabsCount += countInPrefab > 0 ? 1 : 0;
            }
            
            Debug.Log($"Found total <color=yellow>{totalCount}</color> usages " +
                      $"of layer <color=yellow>{LayerMask.LayerToName(layer)}</color> " +
                      $"in <color=white>{containingPrefabsCount}/{allPrefabs.Length}</color> prefabs.");
        }

        private static int FindLayerUsageOnScene(int layer, Scene scene)
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
                    if (layer != t.gameObject.layer) continue;

                    sb ??= new StringBuilder();
                    sb.AppendLine($" + {SceneUtils.GetGameObjectPathInScene(t)}");
                    
                    count++;
                }
            }
            
            if (count > 0)
            {
                Debug.Log($"Found <color=yellow>{count}</color> usages " +
                          $"of layer <color=yellow>{LayerMask.LayerToName(layer)}</color> " +
                          $"on scene <color=white>{scene.name}</color>:\n{sb}");
            }
            
            return count;
        }
        
        private static int FindLayerUsageInPrefab(int layer, GameObject prefabToSearch)
        {
            int count = 0;
            StringBuilder sb = null;

            var children = prefabToSearch.GetComponentsInChildren<Transform>(true);

            for (int j = 0; j < children.Length; j++) {
                var t = children[j];
                if (layer != t.gameObject.layer) continue;

                sb ??= new StringBuilder();
                sb.AppendLine($" + {SceneUtils.GetGameObjectPathInScene(t)}");
                    
                count++;
            }
            
            if (count > 0)
            {
                Debug.Log($"Found <color=yellow>{count}</color> usages " +
                          $"of layer <color=yellow>{LayerMask.LayerToName(layer)}</color> " +
                          $"in prefab <color=white>{AssetDatabase.GetAssetPath(prefabToSearch)}</color>:\n{sb}");
            }
            
            return count;
        }
    }
    
}