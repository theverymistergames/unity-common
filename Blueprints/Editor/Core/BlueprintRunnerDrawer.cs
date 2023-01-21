﻿using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Core {

    [CustomEditor(typeof(BlueprintRunner), true)]
    public class BlueprintRunnerDrawer : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (target is not BlueprintRunner runner) return;

            var blueprintAsset = runner.BlueprintAsset;
            if (blueprintAsset == null) return;

            GUILayout.Space(10);
            if (GUILayout.Button("Edit")) {
                BlueprintsEditorWindow.GetWindow().PopulateFromAsset(blueprintAsset);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Fetch blackboard properties")) {
                runner.FetchBlackboardGameObjectProperties();
            }
        }
    }

}
