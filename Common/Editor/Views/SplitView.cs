using UnityEngine.UIElements;

namespace MisterGames.Common.Editor.Views {

    public class SplitView : TwoPaneSplitView {

        public SplitView() { }
        
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }
        
    }

}