using UnityEngine;

namespace MisterGames.Dbg.Draw {

    [System.Obsolete("Use MisterGames.Common.DebugExt.Draw...() methods")]
    public struct DbgSphere {
        
        private Vector3 _position;
        private float _radius;
        private Color _color;
        private float _opacity;
        private float _time;
        private float _step;
        private bool _occluded;
        
        public static DbgSphere Create() {
            return new DbgSphere {
                _color = UnityEngine.Color.white,
                _opacity = 1f,
                _occluded = true,
                _radius = 1f,
                _step = 0.05f
            };
        }
        
        public DbgSphere Position(Vector3 position) {
            _position = position;
            return this;
        }
            
        public DbgSphere Radius(float radius) {
            _radius = radius;
            return this;
        }

        public DbgSphere Color(Color color) {
            _color = color;
            return this;
        }
        
        public DbgSphere Opacity(float value) {
            _opacity = value;
            return this;
        }

        public DbgSphere Step(float step) {
            _step = step;
            return this;
        }

        public DbgSphere Occlusion(bool enabled) {
            _occluded = enabled;
            return this;
        }
            
        public DbgSphere Time(float time) {
            _time = time;
            return this;
        }

        public void Draw() {
            var or0 = Quaternion.LookRotation(Vector3.up, Vector3.forward);
            var or1 = Quaternion.LookRotation(Vector3.up, Vector3.right);
            
            DbgCircle.Create()
                .Position(_position).Radius(_radius)
                .Occlusion(_occluded).Color(_color).Opacity(_opacity)
                .Step(_step).Time(_time)
                .Draw();
            
            DbgCircle.Create()
                .Position(_position).Orientation(or0).Radius(_radius)
                .Occlusion(_occluded).Color(_color).Opacity(_opacity).Step(_step).Time(_time)
                .Draw();
            
            DbgCircle.Create()
                .Position(_position).Orientation(or1).Radius(_radius)
                .Occlusion(_occluded).Color(_color).Opacity(_opacity).Step(_step).Time(_time)
                .Draw();
        }

    }

}