using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MisterGames.Dbg.Draw {

    [System.Obsolete("Use MisterGames.Common.DebugExt.Draw...() methods")]
    public struct DbgLineArray {

        private Vector3[] _points;
        private Color _color;
        private float _opacity;
        private float _time;
        private bool _occluded;
        private bool _looped;

        public static DbgLineArray Create() {
            return new DbgLineArray {
                _color = UnityEngine.Color.white,
                _opacity = 1f,
                _occluded = true
            };
        }
        
        public DbgLineArray Points(IEnumerable<Vector3> points) {
            _points = points.ToArray();
            return this;
        }

        public DbgLineArray Color(Color color) {
            _color = color;
            return this;
        }
        
        public DbgLineArray Opacity(float value) {
            _opacity = value;
            return this;
        }

        public DbgLineArray Loop(bool enabled) {
            _looped = enabled;
            return this;
        }

        public DbgLineArray Occlusion(bool enabled) {
            _occluded = enabled;
            return this;
        }
            
        public DbgLineArray Time(float time) {
            _time = time;
            return this;
        }

        public void Draw() {
            for (var i = 0; i < _points.Length - 1; i++) {
                var curr = _points[i];
                var next = _points[i + 1];
                    
                DbgLine.Create().From(curr).To(next).Color(_color).Opacity(_opacity).Time(_time).Occlusion(_occluded).Draw();
            }

            if (_looped && _points.Length > 1) {
                var start = _points.First();
                var end = _points.Last();
                DbgLine.Create().From(end).To(start).Color(_color).Opacity(_opacity).Time(_time).Occlusion(_occluded).Draw();
            }
        }

    }

}