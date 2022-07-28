using UnityEngine;

namespace MisterGames.Interact.Cursors {

    [CreateAssetMenu(fileName = nameof(CursorIcon), menuName = "MisterGames/Interact/" + nameof(CursorIcon))]
    public class CursorIcon : ScriptableObject {

        public Sprite sprite;

    }
}
