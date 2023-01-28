using System;
using UnityEngine.UIElements;

namespace MisterGames.Common.Editor.Views {

    public class InspectorView : VisualElement {

        public InspectorView() { }

        public void Inject(Action onInspectorGUI) {
            Clear();
            Add(new IMGUIContainer(onInspectorGUI));
        }

        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }
    }

}
