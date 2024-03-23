using UnityEngine;

namespace MisterGames.Dbg.Draw {

    [System.Obsolete("Use MisterGames.Common.DebugExt.Draw...() methods")]
    public struct DbgRay {

        private Vector3 _from;
        private Vector3 _dir;
        private Color _color;
        private float _opacity;
        private float _time;
        private bool _occluded;
        private float _arrowSize;

        public static DbgRay Create() {
            return new DbgRay {
                _color = UnityEngine.Color.white,
                _opacity = 1f,
                _occluded = true
            };
        }
        
        public DbgRay From(Vector3 point) {
            _from = point;
            return this;
        }
            
        public DbgRay Dir(Vector3 dir) {
            _dir = dir;
            return this;
        }

        public DbgRay Arrow(float size) {
            _arrowSize = size;
            return this;
        }
        
        public DbgRay Color(Color color) {
            _color = color;
            return this;
        }

        public DbgRay Opacity(float value) {
            _opacity = value;
            return this;
        }

        public DbgRay Occlusion(bool enabled) {
            _occluded = enabled;
            return this;
        }
            
        public DbgRay Time(float time) {
            _time = time;
            return this;
        }

        public void Draw() {
            DbgLine.Create()
                .From(_from).To(_from + _dir)
                .Color(_color).Opacity(_opacity).Arrow(_arrowSize)
                .Time(_time).Occlusion(_occluded)
                .Draw();
        }

    }

}