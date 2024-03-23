using UnityEngine;

namespace MisterGames.Dbg.Draw {

    public struct DbgText {

        private Vector3 _position;
        private Color _color;
        private string _text;
        private int _fontSize;

        public static DbgText Create() {
            return new DbgText {
                _position = Vector3.zero,
                _color = UnityEngine.Color.white,
                _text = "",
                _fontSize = 14
            };
        }

        public DbgText Text(string text) {
            _text = text;
            return this;
        }
        
        public DbgText Color(Color color) {
            _color = color;
            return this;
        }

        public DbgText Position(Vector3 point) {
            _position = point;
            return this;
        }
        
        public void Draw() {
            var style = new GUIStyle {
                fontSize = _fontSize, 
                normal = { textColor = _color }
            };
#if UNITY_EDITOR
            UnityEditor.Handles.Label(_position, _text, style);
#endif
        }
        
    }

}