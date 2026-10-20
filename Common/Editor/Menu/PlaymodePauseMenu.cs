using UnityEditor;

namespace MisterGames.Common.Editor.Menu {

    public static class PlaymodePauseMenu {

        [MenuItem("MisterGames/Playmode/Pause _F3")]
        private static void TogglePause() {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }

        [MenuItem("MisterGames/Playmode/Step _F4")]
        private static void Step() {
            EditorApplication.Step();
        }
    }

}
