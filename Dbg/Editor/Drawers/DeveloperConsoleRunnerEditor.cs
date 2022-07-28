using System;
using System.Linq;
using MisterGames.Dbg.Console.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Dbg.Editor.Drawers {
    
    [CustomEditor(typeof(DeveloperConsoleRunner))]
    public class DeveloperConsoleRunnerEditor : UnityEditor.Editor {
        
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            if (target is not DeveloperConsoleRunner console) return;

            if (GUILayout.Button("Refresh commands")) {
                console.SetConsoleCommands(GetConsoleCommands());
                EditorUtility.SetDirty(console);
            }
        }

        private static IConsoleCommand[] GetConsoleCommands() {
            return TypeCache
                .GetTypesDerivedFrom<IConsoleCommand>()
                .Where(t =>
                    (t.IsPublic || t.IsNestedPublic) &&
                    !t.IsAbstract &&
                    !t.IsGenericType &&
                    !typeof(Object).IsAssignableFrom(t) &&
                    Attribute.IsDefined(t, typeof(SerializableAttribute))
                )
                .Select(Activator.CreateInstance)
                .Cast<IConsoleCommand>()
                .ToArray();
        }
    }
    
}
