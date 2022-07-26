using MisterGames.Common.Data;
using UnityEditor;

namespace MisterGames.Common.Editor.Windows {
    
    public static class ScriptableObjectsStorageMenu {

        [MenuItem("MisterGames/Tools/Refresh Scriptable Objects Storage")]
        private static void RefreshStorage() {
            ScriptableObjectsStorage.Instance.Refresh();
        }
        
    }
}