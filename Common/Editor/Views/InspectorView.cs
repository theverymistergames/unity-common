using System;
using UnityEngine.UIElements;

namespace MisterGames.Common.Editor.Views {

    public class InspectorView : VisualElement {

        private Action _onClear;

        public InspectorView() { }

        public void Inject(Action onInspectorGUI, Action onClear = null) {
            ClearInspector();
            _onClear = onClear;
            Add(new IMGUIContainer(onInspectorGUI));
        }

        public void ClearInspector() {
            Clear();
            _onClear?.Invoke();
        }

        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }
    }

}
