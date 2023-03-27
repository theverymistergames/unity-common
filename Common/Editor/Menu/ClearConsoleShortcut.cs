using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace MisterGames.Common.Editor.Menu {

    public static class ClearConsoleShortcut {

        [Shortcut("Clear Console", KeyCode.Q, ShortcutModifiers.Alt)]
        public static void ClearConsole() {
            Assembly
                .GetAssembly(typeof(SceneView))
                .GetType("UnityEditor.LogEntries")
                .GetMethod("Clear")!
                .Invoke(new object(), null);
        }
    }

}
