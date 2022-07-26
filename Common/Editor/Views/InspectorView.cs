using UnityEngine;
using UnityEngine.UIElements;

namespace MisterGames.Common.Editor.Views {

    public class InspectorView : VisualElement {

        private UnityEditor.Editor _editor;
        
        public InspectorView() { }

        public void UpdateSelection(Object obj) {
            ClearInspector();

            _editor = UnityEditor.Editor.CreateEditor(obj);
            var container = new IMGUIContainer(_editor.OnInspectorGUI);
            Add(container);
        }

        public void ClearInspector() {
            Clear();
            Object.DestroyImmediate(_editor);
        }

        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }
        
    }

}