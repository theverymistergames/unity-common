using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Dbg.Draw {

    public struct DbgCylinder {
        
        private Vector3 _from;
        private Vector3 _to;
        private float _radius;
        private Color _color;
        private float _opacity;
        private float _time;
        private float _step;
        private bool _occluded;
        
        public static DbgCylinder Create() {
            return new DbgCylinder {
                _radius = 1f,
                _step = 0.05f,
                _color = UnityEngine.Color.white,
                _opacity = 1f,
                _occluded = true
            };
        }
        
        public DbgCylinder From(Vector3 point) {
            _from = point;
            return this;
        }
         
        public DbgCylinder To(Vector3 point) {
            _to = point;
            return this;
        }
        
        public DbgCylinder Radius(float radius) {
            _radius = radius;
            return this;
        }

        public DbgCylinder Color(Color color) {
            _color = color;
            return this;
        }
        
        public DbgCylinder Opacity(float value) {
            _opacity = value;
            return this;
        }

        public DbgCylinder Step(float step) {
            _step = step;
            return this;
        }

        public DbgCylinder Occlusion(bool enabled) {
            _occluded = enabled;
            return this;
        }
            
        public DbgCylinder Time(float time) {
            _time = time;
            return this;
        }

        public void Draw() {
            var normal = (_to - _from).normalized;
            var rot = Quaternion.FromToRotation(Vector3.up, normal);
            var forward = Vector3.forward.Rotate(rot);
            var right = Vector3.right.Rotate(rot);

            var fr = forward * _radius;
            var rr = right * _radius;
            
            var from0 = _from + fr;
            var from1 = _from - fr;
            var from2 = _from + rr;
            var from3 = _from - rr;
            
            var to0 = _to + fr;
            var to1 = _to - fr;
            var to2 = _to + rr;
            var to3 = _to - rr;

            var orient = Quaternion.LookRotation(forward, normal);
            
            DbgLine.Create().From(from0).To(to0).Color(_color).Opacity(_opacity).Time(_time).Occlusion(_occluded).Draw();
            DbgLine.Create().From(from1).To(to1).Color(_color).Opacity(_opacity).Time(_time).Occlusion(_occluded).Draw();
            DbgLine.Create().From(from2).To(to2).Color(_color).Opacity(_opacity).Time(_time).Occlusion(_occluded).Draw();
            DbgLine.Create().From(from3).To(to3).Color(_color).Opacity(_opacity).Time(_time).Occlusion(_occluded).Draw();
            
            DbgCircle.Create()
                .Position(_from).Orientation(orient).Radius(_radius)
                .Occlusion(_occluded).Color(_color).Opacity(_opacity).Step(_step).Time(_time)
                .Draw();
            
            DbgCircle.Create()
                .Position(_to).Orientation(orient).Radius(_radius)
                .Occlusion(_occluded).Color(_color).Opacity(_opacity).Step(_step).Time(_time)
                .Draw();
        }

    }

}