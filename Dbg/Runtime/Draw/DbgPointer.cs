using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Dbg.Draw {

    public struct DbgPointer {

        private Vector3 _position;
        private Quaternion _orientation;
        private Color _color;
        private float _opacity;
        private float _time;
        private bool _occluded;
        private float _size;

        public static DbgPointer Create() {
            return new DbgPointer {
                _color = UnityEngine.Color.white,
                _opacity = 1f,
                _occluded = true,
                _orientation = Quaternion.identity
            };
        }
        
        public DbgPointer Position(Vector3 point) {
            _position = point;
            return this;
        }
        
        public DbgPointer Orientation(Quaternion orientation) {
            _orientation = orientation;
            return this;
        }
        
        public DbgPointer Size(float size) {
            _size = size;
            return this;
        }
        
        public DbgPointer Color(Color color) {
            _color = color;
            return this;
        }

        public DbgPointer Opacity(float value) {
            _opacity = value;
            return this;
        }

        public DbgPointer Occlusion(bool enabled) {
            _occluded = enabled;
            return this;
        }
            
        public DbgPointer Time(float time) {
            _time = time;
            return this;
        }

        public void Draw() {
            var dirF = new Vector3(0, 1, 0.5f);
            var dirR = new Vector3(0.5f, 1, 0f);
            
            var rotF = Quaternion.FromToRotation(Vector3.up, dirF);
            var rotR = Quaternion.FromToRotation(Vector3.up, dirR);
            
            var offset = Vector3.up * _size;
            
            var points = new[] {
                _position,
                _position + offset.Rotate(rotF * _orientation),
                _position + offset.Rotate(rotF.Inverted() * _orientation),
                _position,
                _position + offset.Rotate(rotR * _orientation),
                _position + offset.Rotate(rotR.Inverted() * _orientation)
            };
            
            DbgLineArray.Create().Points(points).Loop(true).Color(_color).Opacity(_opacity).Occlusion(_occluded).Time(_time).Draw();
        }

    }

}