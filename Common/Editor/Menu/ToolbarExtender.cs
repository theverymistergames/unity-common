using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MisterGames.Common.Editor.Menu {

	public static class ToolbarExtender {
		
		private const float space = 8;
		private const float buttonWidth = 32;
		private const float dropdownWidth = 80;
		private const float playPauseStopWidth = 140;
		
		private static int _toolCount;
		private static GUIStyle _commandStyle;

		private static Action<Rect> _leftToolbarGUI = delegate {  };
		private static Action<Rect> _rightToolbarGUI = delegate {  };

		private static Action OnToolbarGUI;
		private static Action OnToolbarGUILeft;
		private static Action OnToolbarGUIRight;

		private static Rect _leftRect;
		private static Rect _rightRect;

		private static readonly Type _toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
		private static ScriptableObject _currentToolbar;

		public static void OnLeftToolbarGUI(Action<Rect> action) {
			_leftToolbarGUI = action;
		}
		
		public static void OnRightToolbarGUI(Action<Rect> action) {
			_rightToolbarGUI = action;
		}
		
		[InitializeOnLoadMethod]
		private static void Initialize() {
			var toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
			var toolIcons = toolbarType
				.GetField("k_ToolCount", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			
			_toolCount = toolIcons != null ? (int) toolIcons.GetValue(null) : 8;
	
			// ReSharper disable once DelegateSubtraction
			EditorApplication.update -= OnUpdate;
			EditorApplication.update += OnUpdate;
	
			OnToolbarGUI = OnGUI;
			OnToolbarGUILeft = GUILeft;
			OnToolbarGUIRight = GUIRight;
		}

		private static void OnGUI() {
			if (_commandStyle == null) _commandStyle = new GUIStyle("CommandLeft");

			float screenWidth = EditorGUIUtility.currentViewWidth;
			float playButtonsPosition = Mathf.RoundToInt ((screenWidth - playPauseStopWidth) / 2);

			_leftRect = new Rect(0, 0, screenWidth, Screen.height);

			_leftRect.xMin += space; // Spacing left
			_leftRect.xMin += buttonWidth * _toolCount; // Tool buttons
			_leftRect.xMin += space; // Spacing between tools and pivot
			_leftRect.xMin += 64 * 2; // Pivot buttons
			_leftRect.xMax = playButtonsPosition;

			_rightRect = new Rect(0, 0, screenWidth, Screen.height) { xMin = playButtonsPosition };
			_rightRect.xMin += _commandStyle.fixedWidth * 3; // Play buttons
			_rightRect.xMax = screenWidth;
			_rightRect.xMax -= space; // Spacing right
			_rightRect.xMax -= dropdownWidth; // Layout
			_rightRect.xMax -= space; // Spacing between layout and layers
			_rightRect.xMax -= dropdownWidth; // Layers
			_rightRect.xMax -= space; // Spacing between layers and account
			_rightRect.xMax -= dropdownWidth; // Account
			_rightRect.xMax -= space; // Spacing between account and cloud
			_rightRect.xMax -= buttonWidth; // Cloud
			_rightRect.xMax -= space; // Spacing between cloud and collab
			_rightRect.xMax -= 78; // Colab

			_leftRect.xMin += space;
			_leftRect.xMax -= space;
			_rightRect.xMin += space;
			_rightRect.xMax -= space;

			_leftRect.y = 0;
			_leftRect.height = 22;
			_rightRect.y = 0;
			_rightRect.height = 22;

			if (_leftRect.width > 0) {
				GUILayout.BeginArea(_leftRect);
				GUILeft();
				GUILayout.EndArea();
			}

			if (_rightRect.width > 0) {
				GUILayout.BeginArea(_rightRect);
				GUIRight();
				GUILayout.EndArea();
			}
		}

		private static void GUILeft() {
			GUILayout.BeginHorizontal();
			_leftToolbarGUI.Invoke(_leftRect);
			GUILayout.EndHorizontal();
		}

		private static void GUIRight() {
			GUILayout.BeginHorizontal();
			_rightToolbarGUI.Invoke(_rightRect);
			GUILayout.EndHorizontal();
		}
		
		private static void OnUpdate() {
			if (_currentToolbar != null) return;
			
			var toolbars = Resources.FindObjectsOfTypeAll(_toolbarType);
			_currentToolbar = toolbars.Length > 0 ? (ScriptableObject) toolbars[0] : null;
			if (_currentToolbar == null) return;
			
			var root = _currentToolbar
				.GetType()
				.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
			
			if (root == null) return;
			
			var rawRoot = root.GetValue(_currentToolbar);
			var mRoot = rawRoot as VisualElement;
			
			RegisterCallback(mRoot.Q("ToolbarZoneLeftAlign"), OnToolbarGUILeft);
			RegisterCallback(mRoot.Q("ToolbarZoneRightAlign"), OnToolbarGUIRight);
		}

		private static void RegisterCallback(VisualElement root, Action cb) {
			var parent = new VisualElement {
				style = {
					flexGrow = 1,
					flexDirection = FlexDirection.Row,
				}
			};
			
			var container = new IMGUIContainer();
			container.onGUIHandler += () => cb?.Invoke(); 
			parent.Add(container);
			root.Add(parent);
		}
	}
}
