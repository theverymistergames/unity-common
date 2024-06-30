using UnityEngine;

namespace MisterGames.Interact.Cursors {

    [CreateAssetMenu(fileName = nameof(CursorIcon), menuName = "MisterGames/Interactives/" + nameof(CursorIcon))]
    public class CursorIcon : ScriptableObject {

        public Sprite sprite;
        [ColorUsage(showAlpha: true)]
        public Color tint = Color.white;
        public Vector2 size = new Vector2(10f, 10f);
    }

}
