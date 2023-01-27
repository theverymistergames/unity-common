using MisterGames.Blueprints.Validation;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Core {

    [CustomEditor(typeof(BlueprintAsset))]
    public sealed class BlueprintAssetEditor : UnityEditor.Editor {

        private bool _isAllowedToShowEditedNode;

        public void AllowShowEditedBlueprintNode() {
            _isAllowedToShowEditedNode = true;
        }

        public override void OnInspectorGUI() {
            if (!_isAllowedToShowEditedNode) return;
            if (target is not BlueprintAsset blueprint) return;
            if (serializedObject.targetObject == null) return;

            serializedObject.Update();

            var nodeProperty = serializedObject.FindProperty("editedNode");

            float labelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = EditorGUIUtility.fieldWidth;

            EditorGUIUtility.labelWidth = 140;
            EditorGUIUtility.fieldWidth = 240;

            EditorGUI.BeginChangeCheck();

            var endProperty = nodeProperty.GetEndProperty();
            bool enterChildren = true;
            while (nodeProperty.NextVisible(enterChildren) && !SerializedProperty.EqualContents(nodeProperty, endProperty)) {
                enterChildren = false;
                EditorGUILayout.PropertyField(nodeProperty, true);

                if (nodeProperty.GetValue() is BlueprintAsset blueprintAsset && GUILayout.Button("Edit")) {
                    BlueprintsEditorWindow.OpenAsset(blueprintAsset);
                }
            }

            if (EditorGUI.EndChangeCheck()) {
                var node = blueprint.editedNode;
                node.OnValidate();
                if (node is IBlueprintAssetValidator validator) validator.ValidateBlueprint(blueprint, blueprint.editedNodeId);
            }

            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUIUtility.fieldWidth = fieldWidth;

            serializedObject.ApplyModifiedProperties();
        }
    }

}
