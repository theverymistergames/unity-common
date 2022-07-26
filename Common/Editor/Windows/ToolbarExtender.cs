using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MisterGames.Common.Editor.Windows
{
	public static class ToolbarExtender {
		
		private const float space = 8;
		private const float buttonWidth = 32;
		private const float dropdownWidth = 80;
		private const float playPauseStopWidth = 140;
		
		private static int _toolCount;
		private static GUIStyle _commandStyle;

		private static Action _leftToolbarGUI = delegate {  };
		private static Action _rightToolbarGUI = delegate {  };

		private static Action OnToolbarGUI;
		private static Action OnToolbarGUILeft;
		private static Action OnToolbarGUIRight;
		
		private static readonly Type _toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
		private static ScriptableObject _currentToolbar;

		public static void OnLeftToolbarGUI(Action action) {
			_leftToolbarGUI = action;
		}
		
		public static void OnRightToolbarGUI(Action action) {
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

			var leftRect = new Rect(0, 0, screenWidth, Screen.height);
			
			leftRect.xMin += space; // Spacing left
			leftRect.xMin += buttonWidth * _toolCount; // Tool buttons
			leftRect.xMin += space; // Spacing between tools and pivot
			leftRect.xMin += 64 * 2; // Pivot buttons
			leftRect.xMax = playButtonsPosition;

			var rightRect = new Rect(0, 0, screenWidth, Screen.height) { xMin = playButtonsPosition };
			rightRect.xMin += _commandStyle.fixedWidth * 3; // Play buttons
			rightRect.xMax = screenWidth;
			rightRect.xMax -= space; // Spacing right
			rightRect.xMax -= dropdownWidth; // Layout
			rightRect.xMax -= space; // Spacing between layout and layers
			rightRect.xMax -= dropdownWidth; // Layers
			rightRect.xMax -= space; // Spacing between layers and account
			rightRect.xMax -= dropdownWidth; // Account
			rightRect.xMax -= space; // Spacing between account and cloud
			rightRect.xMax -= buttonWidth; // Cloud
			rightRect.xMax -= space; // Spacing between cloud and collab
			rightRect.xMax -= 78; // Colab

			leftRect.xMin += space;
			leftRect.xMax -= space;
			rightRect.xMin += space;
			rightRect.xMax -= space;

			leftRect.y = 0;
			leftRect.height = 22;
			rightRect.y = 0;
			rightRect.height = 22;

			if (leftRect.width > 0) {
				GUILayout.BeginArea(leftRect);
				GUILeft();
				GUILayout.EndArea();
			}

			if (rightRect.width > 0) {
				GUILayout.BeginArea(rightRect);
				GUIRight();
				GUILayout.EndArea();
			}
		}

		private static void GUILeft() {
			GUILayout.BeginHorizontal();
			_leftToolbarGUI.Invoke();
			GUILayout.EndHorizontal();
		}

		private static void GUIRight() {
			GUILayout.BeginHorizontal();
			_rightToolbarGUI.Invoke();
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