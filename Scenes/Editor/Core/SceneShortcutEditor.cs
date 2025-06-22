using System;
using MisterGames.Common.Editor.Menu;
using MisterGames.Common.Editor.Views;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Editor.Utils;
using MisterGames.Scenes.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Editor.Core {

	[InitializeOnLoad]
	public static class SceneShortcutEditor {

		private static Texture2D _iconPlus;
		private static Texture2D _iconMinus;
		
		static SceneShortcutEditor() {
			ToolbarExtender.OnRightToolbarGUI(OnGUI);
		}

		private static void OnGUI(Rect rect) {
			EditorGUI.BeginDisabledGroup(Application.isPlaying);

			string activeSceneName = SceneManager.GetActiveScene().name;
			
			if (EditorGUILayout.DropdownButton(new GUIContent(activeSceneName), FocusType.Keyboard, GUILayout.MinWidth(222))) {
				LoadIcons();
				
				var scenesDropdown = new AdvancedDropdown<SceneAsset>(
					"Select scene",
					SceneLoaderSettings.GetAllSceneAssets(),
					sceneAsset => SceneUtils.RemoveSceneAssetFileFormat(AssetDatabase.GetAssetPath(sceneAsset)),
					OnSceneSelected,
					getIcon: GetIcon
				);

				var dropdownRect = new Rect(rect);
				dropdownRect.y += EditorGUIUtility.singleLineHeight;

				scenesDropdown.Show(dropdownRect);
			}

			EditorGUI.EndDisabledGroup();
		}

		private static void LoadIcons() {
			_iconPlus = Resources.Load<Texture2D>("SceneShortcut_IconPlus");
			_iconMinus = Resources.Load<Texture2D>("SceneShortcut_IconMinus");
		}
		
		private static Texture2D GetIcon(SceneAsset sceneAsset) {
			return sceneAsset == null
				? null
				: SceneManager.GetSceneByName(sceneAsset.name).isLoaded 
					? SceneManager.loadedSceneCount == 1 ? null : _iconMinus 
					: _iconPlus;
		}
		
		private static void OnSceneSelected(SceneAsset sceneAsset, AdvancedDropdownSelectType selectType) {
			int loadedCount = SceneManager.loadedSceneCount;
			string sceneName = sceneAsset.name;
			bool isRequestedSceneLoaded = SceneManager.GetSceneByName(sceneName).isLoaded;
			
			for (int i = 0; i < loadedCount; i++) {
				var scene = SceneManager.GetSceneAt(i);

				bool needUnload = selectType switch {
					AdvancedDropdownSelectType.Item => scene.name != sceneName,
					AdvancedDropdownSelectType.ItemIcon => scene.name == sceneName,
					_ => throw new ArgumentOutOfRangeException(nameof(selectType), selectType, null)
				};
				
				if (!needUnload) continue;
				if (!SceneUtils.ShowSaveSceneDialogAndUnload_EditorOnly(scene)) return;
			}
			
			if (isRequestedSceneLoaded) return;

			var mode = selectType switch {
				AdvancedDropdownSelectType.Item => OpenSceneMode.Single,
				AdvancedDropdownSelectType.ItemIcon => OpenSceneMode.Additive,
				_ => throw new ArgumentOutOfRangeException(nameof(selectType), selectType, null)
			};
			
			EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneAsset), mode);
		}
	}

}
