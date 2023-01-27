using MisterGames.Blueprints.Validation;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Core {

    /// <summary>
    /// This editor is not for default Unity Inspector with BlueprintAsset.asset as serialized object.
    /// BlueprintAsset default inspector is made empty by overriding UnityEditor.Editor.OnInspectorGUI() method.
    /// </summary>
    [CustomEditor(typeof(BlueprintAsset))]
    public sealed class BlueprintAssetEditor : UnityEditor.Editor {

        /// <summary>
        /// Empty method override to make BlueprintAsset default inspector empty.
        /// </summary>
        public override void OnInspectorGUI() { }

        public void DoOnInspectorGUI() {
            if (target is not BlueprintAsset blueprint) return;
            if (serializedObject.targetObject == null) return;

            int editedNodeId = blueprint.editedNodeId;

            serializedObject.Update();

            var nodeProperty = serializedObject.FindProperty("editedNode");

            float labelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = EditorGUIUtility.fieldWidth;

            EditorGUIUtility.labelWidth = 140;
            EditorGUIUtility.fieldWidth = 240;

            EditorGUI.BeginChangeCheck();

            Debug.Log($"BlueprintAssetEditor.OnInspectorGUI: paint Node#{editedNodeId} {blueprint.editedNode}");

            bool enterChildren = true;
            while (nodeProperty.NextVisible(enterChildren)) {
                enterChildren = false;
                EditorGUILayout.PropertyField(nodeProperty, true);

                if (nodeProperty.GetValue() is BlueprintAsset blueprintAsset && GUILayout.Button("Edit")) {
                    BlueprintsEditorWindow.OpenAsset(blueprintAsset);
                }
            }

            if (EditorGUI.EndChangeCheck()) {
                Debug.Log($"BlueprintAssetEditor.OnInspectorGUI: validate Node#{editedNodeId} {blueprint.editedNode}");

                var node = blueprint.editedNode;
                node.OnValidate();
                if (node is IBlueprintAssetValidator validator) validator.ValidateBlueprint(blueprint, editedNodeId);
            }

            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUIUtility.fieldWidth = fieldWidth;

            serializedObject.ApplyModifiedProperties();
        }
    }

}
