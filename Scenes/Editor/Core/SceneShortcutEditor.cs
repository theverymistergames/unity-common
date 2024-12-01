using MisterGames.Common.Editor.Menu;
using MisterGames.Common.Editor.Views;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Editor.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Editor.Core {

	[InitializeOnLoad]
	public static class SceneShortcutEditor {

		static SceneShortcutEditor() {
			ToolbarExtender.OnRightToolbarGUI(OnGUI);
		}

		private static void OnGUI(Rect rect) {
			EditorGUI.BeginDisabledGroup(Application.isPlaying);

			string activeSceneName = SceneManager.GetActiveScene().name;

			if (EditorGUILayout.DropdownButton(new GUIContent(activeSceneName), FocusType.Keyboard, GUILayout.MinWidth(222))) {
				var scenesDropdown = new AdvancedDropdown<SceneAsset>(
					"Select scene",
					SceneLoaderSettings.GetAllSceneAssets(),
					sceneAsset => SceneUtils.RemoveSceneAssetFileFormat(AssetDatabase.GetAssetPath(sceneAsset)),
					OnSceneSelected
				);

				var dropdownRect = new Rect(rect);
				dropdownRect.y += EditorGUIUtility.singleLineHeight;

				scenesDropdown.Show(dropdownRect);
			}

			EditorGUI.EndDisabledGroup();
		}

		private static void OnSceneSelected(SceneAsset sceneAsset) {
			var activeScene = SceneManager.GetActiveScene();

			if (activeScene.isDirty) {
				int dialogResult = EditorUtility.DisplayDialogComplex(
					"Scene have been modified",
					$"Do you want to save the changes in the scene:\n{activeScene.path}",
					"Save", "Cancel", "Discard"
				);

				switch (dialogResult) {
					// Save
					case 0:
						EditorSceneManager.SaveScene(activeScene);
						break;

					// Cancel
					case 1:
						return;

					// Don't Save
					case 2:
						break;
				}
			}

			EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneAsset), OpenSceneMode.Single);
		}
	}

}
