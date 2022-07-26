using UnityEngine;

namespace MisterGames.Dbg.Draw {

    public struct DbgCapsule {

        private Vector3 _from;
        private Vector3 _to;
        private float _radius;
        private Color _color;
        private float _opacity;
        private float _time;
        private float _step;
        private bool _occluded;

        public static DbgCapsule Create() {
            return new DbgCapsule {
                _color = UnityEngine.Color.white,
                _opacity = 1f,
                _step = 0.05f,
                _occluded = true
            };
        }
        
        public DbgCapsule From(Vector3 point) {
            _from = point;
            return this;
        }
         
        public DbgCapsule To(Vector3 point) {
            _to = point;
            return this;
        }
        
        public DbgCapsule Radius(float radius) {
            _radius = radius;
            return this;
        }

        public DbgCapsule Color(Color color) {
            _color = color;
            return this;
        }
        
        public DbgCapsule Opacity(float value) {
            _opacity = value;
            return this;
        }
        
        public DbgCapsule Step(float step) {
            _step = step;
            return this;
        }

        public DbgCapsule Occlusion(bool enabled) {
            _occluded = enabled;
            return this;
        }
            
        public DbgCapsule Time(float time) {
            _time = time;
            return this;
        }

        public void Draw() {
            var normal = (_to - _from).normalized;
            var rot = Quaternion.FromToRotation(Vector3.up, normal);
            var forward = rot * Vector3.forward;
            var right = rot * Vector3.right;
            
            var or0 = Quaternion.LookRotation(forward, right);
            var or1 = Quaternion.LookRotation(-right, forward);
            var or2 = Quaternion.LookRotation(forward, -right);
            var or3 = Quaternion.LookRotation(-right, -forward);
            
            DbgCylinder.Create()
                .From(_from).To(_to)
                .Color(_color).Opacity(_opacity).Occlusion(_occluded).Time(_time).Step(_step).Radius(_radius)
                .Draw();
            
            DbgCircle.Create()
                .Position(_from).Orientation(or0).Radius(_radius).Angle(180f)
                .Occlusion(_occluded).Color(_color).Opacity(_opacity).Step(_step).Time(_time)
                .Draw();
            
            DbgCircle.Create()
                .Position(_from).Orientation(or1).Radius(_radius).Angle(180f)
                .Occlusion(_occluded).Color(_color).Opacity(_opacity).Step(_step).Time(_time)
                .Draw();
            
            DbgCircle.Create()
                .Position(_to).Orientation(or2).Radius(_radius).Angle(180f)
                .Occlusion(_occluded).Color(_color).Opacity(_opacity).Step(_step).Time(_time)
                .Draw();
            
            DbgCircle.Create()
                .Position(_to).Orientation(or3).Radius(_radius).Angle(180f)
                .Occlusion(_occluded).Color(_color).Opacity(_opacity).Step(_step).Time(_time)
                .Draw();
        }

    }

}