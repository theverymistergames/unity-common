using UnityEditor;
using UnityEditor.Toolbars;

namespace MisterGames.Common.Editor.Menu {
    
    internal static class RecompileMenu {
        
        [MainToolbarElement("MisterGames/Recompile", defaultDockPosition = MainToolbarDockPosition.Left)]
        public static MainToolbarElement Recompile() {
            var content = new MainToolbarContent("Recompile \ud83d\udee0\ufe0f");
            return new MainToolbarButton(content, EditorUtility.RequestScriptReload);
        }
    }
    
}