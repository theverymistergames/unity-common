using MisterGames.Blueprints.Core;
using MisterGames.BlueprintLib;
using MisterGames.Fsm.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Drawers {

    [CustomEditor(typeof(BlueprintNodeSubgraph), true)]
    public class BlueprintNodeSubgraphDrawer : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            if (serializedObject.targetObject == null) return;
            
            serializedObject.Update();
            
            float labelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = EditorGUIUtility.fieldWidth;
            
            EditorGUIUtility.labelWidth = 110;
            EditorGUIUtility.fieldWidth = 160;
            
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
            
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUIUtility.fieldWidth = fieldWidth;

            if (!(target is IBlueprintHost host)) return;

            var asset = host.Source;
            if (asset == null) return;
            
            if (GUILayout.Button("Open")) {
                if (Application.isPlaying && host.Instance != null) {
                    BlueprintsEditorWindow.OpenWindow().PopulateFromHost(target);
                }
                else {
                    BlueprintsEditorWindow.OpenWindow().PopulateFromAsset(asset);    
                }
            }
        }
        
    }
}