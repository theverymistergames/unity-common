using MisterGames.Common.Editor.Windows;
using MisterGames.Scenes.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Editor.Windows {

	[InitializeOnLoad]
	public static class SceneShortcutEditor {

		static SceneShortcutEditor() {
			ToolbarExtender.OnRightToolbarGUI(OnGUI);
			
			EditorSceneManager.sceneOpened -= OnSceneOpened;
			EditorSceneManager.sceneOpened += OnSceneOpened;
		}

		private static void OnSceneOpened(Scene scene, OpenSceneMode mode) {
			ScenesStorage.Instance.SceneStart = scene.name;
			EditorUtility.SetDirty(ScenesStorage.Instance);
		}

		private static void OnGUI() {
			using var scope = new EditorGUI.DisabledScope(Application.isPlaying);

			var activeScene = SceneManager.GetActiveScene();
			string activeSceneName = activeScene.name;
			
			var sceneNames = ScenesStorage.Instance.SceneNames;
			int activeSceneIndex = -1;

			for (int i = 0; i < sceneNames.Length; i++) {
				if (activeSceneName != sceneNames[i]) continue;
				activeSceneIndex = i;
				break;
			}

			int newSceneIndex = EditorGUILayout.Popup(
				activeSceneIndex, sceneNames, 
				EditorStyles.toolbarPopup, GUILayout.Width(250f)
			);

			if (newSceneIndex == activeSceneIndex) return;

			if (activeScene.isDirty) {
				int dialogResult = EditorUtility.DisplayDialogComplex(
					"Scene Have Been Modified",
					$"Do you want to save the changes you made in the scene:"
					+ $"\n{activeScene.path}"
					+ "\nYour changes will be lost if you don't save them.",
					"Save", "Cancel", "Don't Save"
				);

				switch (dialogResult) {
					case 0: // Save
						EditorSceneManager.SaveScene(activeScene);
						break;
					case 1: // Cancel
						break;
					case 2: // Don't Save
						break;
				}
				return;
			}

			string targetSceneName = sceneNames[newSceneIndex];
			var guids = AssetDatabase.FindAssets($"t:{nameof(SceneAsset)}");
			
			for (int i = 0; i < guids.Length; i++) {
				string guid = guids[i];
				string path = AssetDatabase.GUIDToAssetPath(guid);
				
				if (string.IsNullOrEmpty(path)) continue;

				var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
				if (asset == null) continue;

				if (asset.name != targetSceneName) continue;
				
				EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
			}
		}
	}
}

