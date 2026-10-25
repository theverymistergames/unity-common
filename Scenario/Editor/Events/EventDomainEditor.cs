using System.Collections.Generic;
using System.IO;
using MisterGames.Scenario.Events;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenario.Editor.Events {

    [CustomEditor(typeof(EventDomain))]
    public sealed class EventDomainEditor : UnityEditor.Editor {

        private const string ScriptPropertyPath = "m_Script";
        private const string WorkingModePropertyPath = "_workingMode";
        private const string ExpectEventDomainRunnerMode = "ExpectEventDomainRunner";

        private const string SearchingText = "Searching usages...";
        private const string NoUsagesText = "No usages yet. Setup EventDomainRunner on scene or prefab to use this event domain.";
        private const string UntitledSceneText = "Untitled scene";
        private const string PrefabInstanceText = "instance of";
        private const float UsageIndent = 16f;

        private static GUIStyle _linkStyle;
        private static GUIStyle _pathLinkStyle;

        private readonly List<EventDomainUsagesCache.UsageGroup> _usageGroups = new();
        private int _usagesVersion = -1;

        private void OnEnable() {
            EventDomainUsagesCache.Changed += Repaint;
        }

        private void OnDisable() {
            EventDomainUsagesCache.Changed -= Repaint;
        }

        public override void OnInspectorGUI() {
            serializedObject.UpdateIfRequiredOrScript();

            var property = serializedObject.GetIterator();

            for (bool enterChildren = true; property.NextVisible(enterChildren); enterChildren = false) {
                using (new EditorGUI.DisabledScope(property.propertyPath == ScriptPropertyPath)) {
                    EditorGUILayout.PropertyField(property, includeChildren: true);
                }

                if (property.propertyPath == WorkingModePropertyPath &&
                    !serializedObject.isEditingMultipleObjects &&
                    IsExpectingEventDomainRunner(property)
                ) {
                    DrawUsages();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static bool IsExpectingEventDomainRunner(SerializedProperty property) {
            int index = property.enumValueIndex;
            string[] names = property.enumNames;

            return index >= 0 && index < names.Length && names[index] == ExpectEventDomainRunnerMode;
        }

        private void DrawUsages() {
            var eventDomain = target as EventDomain;

            if (_usagesVersion != EventDomainUsagesCache.Version) {
                EventDomainUsagesCache.GetUsages(eventDomain, _usageGroups);
                _usagesVersion = EventDomainUsagesCache.Version;
            }

            InitStyles();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            for (int i = 0; i < _usageGroups.Count; i++) {
                DrawUsageGroup(eventDomain, _usageGroups[i]);
            }

            if (EventDomainUsagesCache.IsSearching) {
                EditorGUILayout.LabelField(SearchingText, EditorStyles.miniLabel);
            }
            else if (_usageGroups.Count == 0) {
                EditorGUILayout.LabelField(NoUsagesText, EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawUsageGroup(EventDomain eventDomain, EventDomainUsagesCache.UsageGroup group) {
            var assetContent = GetAssetContent(group.assetPath);

            if (group.usages.Count == 1) {
                EditorGUILayout.BeginHorizontal();

                DrawAssetLink(group.assetPath, assetContent);
                DrawUsageLink(eventDomain, group, group.usages[0]);

                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            DrawAssetLink(group.assetPath, assetContent);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < group.usages.Count; i++) {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Space(UsageIndent);
                DrawUsageLink(eventDomain, group, group.usages[i]);

                EditorGUILayout.EndHorizontal();
            }
        }

        private static void DrawAssetLink(string assetPath, GUIContent content) {
            var oldIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));

            var rect = GUILayoutUtility.GetRect(content, _linkStyle, GUILayout.ExpandWidth(false));
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            if (GUI.Button(rect, content, _linkStyle)) EventDomainUsagesCache.PingAsset(assetPath);

            EditorGUIUtility.SetIconSize(oldIconSize);
        }

        private static void DrawUsageLink(
            EventDomain eventDomain,
            EventDomainUsagesCache.UsageGroup group,
            EventDomainUsagesCache.Usage usage
        ) {
            var content = new GUIContent(usage.Path, usage.Path);

            var rect = GUILayoutUtility.GetRect(content, _pathLinkStyle, GUILayout.MinWidth(0f), GUILayout.ExpandWidth(true));
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            if (GUI.Button(rect, content, _pathLinkStyle)) {
                EditorApplication.delayCall += () => EventDomainUsagesCache.SelectUsage(eventDomain, group, usage);
            }

            if (!usage.IsPrefabInstance) return;

            GUILayout.Label(PrefabInstanceText, EditorStyles.miniLabel, GUILayout.ExpandWidth(false));
            DrawAssetLink(usage.PrefabPath, GetAssetContent(usage.PrefabPath));
        }

        private static GUIContent GetAssetContent(string assetPath) {
            if (string.IsNullOrEmpty(assetPath)) return new GUIContent(UntitledSceneText);

            return new GUIContent(Path.GetFileName(assetPath), AssetDatabase.GetCachedIcon(assetPath), assetPath);
        }

        private static void InitStyles() {
            _linkStyle ??= new GUIStyle(EditorStyles.linkLabel) { imagePosition = ImagePosition.ImageLeft };
            _pathLinkStyle ??= new GUIStyle(EditorStyles.linkLabel) { clipping = TextClipping.Clip };
        }
    }

}
