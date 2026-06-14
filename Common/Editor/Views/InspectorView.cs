using System;
using UnityEngine.UIElements;

namespace MisterGames.Common.Editor.Views {

    [UxmlElement]
    public partial class InspectorView : VisualElement {

        public void Inject(Action onInspectorGUI) {
            Clear();
            Add(new IMGUIContainer(onInspectorGUI));
        }
    }

}
