using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Dbg.Draw {

    public struct DbgCircle {
        
        private Vector3 _position;
        private Quaternion _orientation;
        private float _radius;
        private float _angle;
        private float _step;
        private Color _color;
        private float _opacity;
        private float _time;
        private bool _occluded;
        
        public static DbgCircle Create() {
            return new DbgCircle {
                _radius = 1f,
                _angle = 360f,
                _step = 0.05f,
                _color = UnityEngine.Color.white,
                _opacity = 1f,
                _occluded = true,
                _orientation = Quaternion.identity
            };
        }
        
        public DbgCircle Position(Vector3 position) {
            _position = position;
            return this;
        }
        
        public DbgCircle Orientation(Quaternion orientation) {
            _orientation = orientation;
            return this;
        }
            
        public DbgCircle Radius(float radius) {
            _radius = radius;
            return this;
        }
        
        public DbgCircle Angle(float angle) {
            _angle = Mathf.Clamp(angle, 0f, 360f);
            return this;
        }
        
        public DbgCircle Step(float step) {
            _step = step;
            return this;
        }

        public DbgCircle Color(Color color) {
            _color = color;
            return this;
        }

        public DbgCircle Opacity(float value) {
            _opacity = value;
            return this;
        }

        public DbgCircle Occlusion(bool enabled) {
            _occluded = enabled;
            return this;
        }
            
        public DbgCircle Time(float time) {
            _time = time;
            return this;
        }

        public void Draw() {
            var normal = _orientation * Vector3.up;
            var start = _orientation * Vector3.forward * _radius;
            var inc = _step * 360f;
            var count = Mathf.CeilToInt(_angle / inc) + 1;
            
            var points = new Vector3[count];
            for (var i = 0; i < count; i++) {
                var angle = Mathf.Clamp(i * inc, 0, _angle);
                var rot = Quaternion.AngleAxis(angle, normal);
                points[i] = rot * start + _position;
            }
            
            DbgLineArray.Create()
                .Points(points).Loop(_angle >= 360f)
                .Color(_color).Opacity(_opacity).Occlusion(_occluded).Time(_time)
                .Draw();
        }

    }

}