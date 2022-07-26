using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Dbg.Draw {

    public struct DbgLine {

        private Vector3 _from;
        private Vector3 _to;
        private Color _color;
        private float _opacity;
        private float _time;
        private float _arrowSize;
        private bool _occluded;
        
        public static DbgLine Create() {
            return new DbgLine {
                _color = UnityEngine.Color.white,
                _opacity = 1f,
                _occluded = true
            };
        }
        
        public DbgLine From(Vector3 point) {
            _from = point;
            return this;
        }
            
        public DbgLine To(Vector3 point) {
            _to = point;
            return this;
        }
        
        public DbgLine Arrow(float size) {
            _arrowSize = size;
            return this;
        }

        public DbgLine Color(Color color) {
            _color = color;
            return this;
        }
        
        public DbgLine Opacity(float value) {
            _opacity = value;
            return this;
        }

        public DbgLine Occlusion(bool enabled) {
            _occluded = enabled;
            return this;
        }
            
        public DbgLine Time(float time) {
            _time = time;
            return this;
        }

        public void Draw() {
            _color.a = _opacity;
            Debug.DrawLine(_from, _to, _color, _time, _occluded);
            
            var arrowOffset = (_from - _to).normalized * _arrowSize;
            var arrowDir = new Vector3(0, 1, 0.6f);
            var arrowRot = Quaternion.FromToRotation(Vector3.up, arrowDir);
            
            var arrowLeft = _to + arrowOffset.Rotate(arrowRot);
            var arrowRight = _to + arrowOffset.Rotate(arrowRot.Inverted());
            
            Debug.DrawLine(_to, arrowLeft, _color, _time, _occluded);
            Debug.DrawLine(_to, arrowRight, _color, _time, _occluded);
        }

    }

}