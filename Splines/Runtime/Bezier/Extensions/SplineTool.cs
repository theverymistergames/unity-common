using System;
using System.Collections.Generic;
using MisterGames.Bezier.Objects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Bezier.Extensions {

    [ExecuteInEditMode]
    [RequireComponent(typeof(SplineCreator))]
    public class SplineTool : MonoBehaviour {
        
        [Header("Length")]
        [SerializeField] [Min(0f)] private float _segmentLength = 1f;
        [SerializeField] private bool _useRandomLength = false;
        
        [Header("Vertical")]
        [SerializeField] private float _verticalOffset = 0f;
        [SerializeField] private bool _useRandomOffset = false;
        
        [Header("Horizontal")]
        [SerializeField] [Range(-90f, 90f)] private float _horizontalTurnAngle = 0f;
        [SerializeField] private bool _useRandomAngle = false;
        
        [Header("Curvature")]
        [SerializeField] [Range(0.001f, 1f)] private float _curvature = 0.5f;
        [SerializeField] private bool _useRandomCurvature = false;
        
        [Header("Limits")]
        [SerializeField] [Min(0f)] private float _minSegmentLength = 1f;
        [SerializeField] [Min(0f)] private float _maxSegmentLength = 1f;
        
        [SerializeField] [Min(0f)] private float _maxVerticalOffset = 1f;
        [SerializeField] [Range(0f, 90f)] private float _maxHorizontalTurnAngle = 90f;
        
        [SerializeField] [Range(0.001f, 1f)] private float _maxCurvature = 1f;
        [SerializeField] [Range(0.001f, 1f)] private float _minCurvature = 0.001f;
        
        [SerializeField] [HideInInspector] 
        private List<SegmentInfo> _currentSpline = new List<SegmentInfo>();
        
        private readonly Vector3 _initialDirection = Vector3.right;
        private SplineCreator _splineCreator;

        private void OnValidate() {
            ValidateParameters();
            RecalculateSpline();
            RedrawSpline();
        }

        private void Awake() {
            _splineCreator = GetComponent<SplineCreator>();
            Clear();
            ResetSplineCreatorSettings();
        }

        public void GenerateNext() {
            var settings = BuildSettings();
            var segment = CreateSegment(settings);
            _currentSpline.Add(segment);
            RedrawSpline();
        }

        public void Clear() {
            _currentSpline.Clear();
            RedrawSpline();
        }
        
        public void RemoveLast() {
            var segmentCount = _currentSpline.Count;
            if (segmentCount == 0) return;
            _currentSpline.RemoveAt(segmentCount - 1);
            MatchCurrentParametersWithLastSegment();
            RedrawSpline();
        }

        
        // ---------------------------- Generation ----------------------------

        private SegmentSettings BuildSettings() {
            var start = GetStartPoint();
            var startDir = GetStartDir();
            var distance = GetSegmentDistance();

            var horizontalTurnAngle = GetHorizontalTurnAngle();
            var verticalOffset = GetVerticalOffset();
            var curvature = GetCurvature();
            
            return new SegmentSettings {
                start = start,
                startDir = startDir,
                distance = distance,
                horizontalTurnAngle = horizontalTurnAngle,
                verticalOffset = verticalOffset,
                curvature = curvature
            };
        }
        
        private static SegmentInfo CreateSegment(SegmentSettings settings) {
            var start = settings.start;
            var dir = settings.startDir.normalized;
            
            var rotation = Quaternion.Euler(0f, settings.horizontalTurnAngle, 0f);
            var end = start + rotation * (dir * settings.distance) + Vector3.up * settings.verticalOffset;
    
            var controlLength = settings.distance * settings.curvature;
            var controlEndDir = (end - start).normalized;
            controlEndDir.y = 0f;
    
            var controlStart = start + dir * controlLength;
            var controlEnd = end + rotation * (controlEndDir * -controlLength);
        
            return new SegmentInfo {
                start = settings.start,
                controlStart = controlStart,
                controlEnd = controlEnd,
                end = end,
                distance = settings.distance,
                horizontalTurnAngle = settings.horizontalTurnAngle,
                verticalOffset = settings.verticalOffset,
                curvature = settings.curvature
            };
        }


        // ---------------------------- SplineCreator operations ----------------------------

        private void RedrawSpline() {
            _splineCreator.EditorData?.ResetBezierPath(transform.position);
            
            var segmentCount = _currentSpline.Count;
            if (segmentCount == 0) return;
            
            var bezier = _splineCreator.bezierPath;
            var current = _currentSpline[0];

            bezier.MovePoint(0, current.start, true);
            bezier.MovePoint(1, current.controlStart, true);
            bezier.MovePoint(3, current.end, true);
            bezier.MovePoint(2, current.controlEnd, true);

            for (var i = 1; i < segmentCount; i++) {
                current = _currentSpline[i];
                
                bezier.AddSegmentToEnd(current.end);
                
                var endIndex = bezier.NumPoints - 1;
                var controlStartIndex = endIndex - 2;
                var controlEndIndex = endIndex - 1;
                
                bezier.MovePoint(controlStartIndex, current.controlStart, true);
                bezier.MovePoint(controlEndIndex, current.controlEnd, true);
            }

            bezier.ResetNormalAngles();
        }

        private void ResetSplineCreatorSettings() {
            var bezier = _splineCreator.bezierPath;
            bezier.Space = PathSpace.Xyz;
            bezier.ControlPointMode = BezierPath.ControlMode.Aligned;
            
            var data = _splineCreator.EditorData;
            data.vertexPathMinVertexSpacing = 0f;
            data.vertexPathMaxAngleError = 0f;
        }
        

        // ---------------------------- Validation ----------------------------
        
        private void RecalculateSpline() {
            var segmentCount = _currentSpline.Count;
            if (segmentCount == 0) return;

            var spline = new SegmentInfo[segmentCount];

            for (var i = segmentCount - 1; i >= 0; i--) {
                var segmentInfo = _currentSpline[i];
                var start = segmentInfo.start;
                var dir = segmentInfo.GetStartDirection();

                var isLast = i == segmentCount - 1;
                var newSettings = GetValidatedSettings(start, dir, segmentInfo, isLast);
                var newSegment = CreateSegment(newSettings);
                
                spline[i] = newSegment;
            }
            
            _currentSpline.Clear();
            _currentSpline.AddRange(spline);
        }

        private void MatchCurrentParametersWithLastSegment() {
            var segmentCount = _currentSpline.Count;
            if (segmentCount == 0) return;
            
            var segmentInfo = _currentSpline[segmentCount - 1];
            _segmentLength = segmentInfo.distance;
            _verticalOffset = segmentInfo.verticalOffset;
            _horizontalTurnAngle = segmentInfo.horizontalTurnAngle;
            _curvature = segmentInfo.curvature;
        }
        
        private void ValidateParameters() {
            if (_maxSegmentLength < _minSegmentLength) _maxSegmentLength = _minSegmentLength;
            if (_maxCurvature < _minCurvature) _maxCurvature = _minCurvature;

            if (_segmentLength < _minSegmentLength) _segmentLength = _minSegmentLength;
            if (_segmentLength > _maxSegmentLength) _segmentLength = _maxSegmentLength;
            
            if (_verticalOffset < -_maxVerticalOffset) _verticalOffset = -_maxVerticalOffset;
            if (_verticalOffset > _maxVerticalOffset) _verticalOffset = _maxVerticalOffset;
            
            if (_horizontalTurnAngle < -_maxHorizontalTurnAngle) _horizontalTurnAngle = -_maxHorizontalTurnAngle;
            if (_horizontalTurnAngle > _maxHorizontalTurnAngle) _horizontalTurnAngle = _maxHorizontalTurnAngle;
            
            if (_curvature < _minCurvature) _curvature = _minCurvature;
            if (_curvature > _maxCurvature) _curvature = _maxCurvature;
        }
        
        private SegmentSettings GetValidatedSettings(
            Vector3 start,
            Vector3 startDir,
            SegmentInfo info,
            bool useParameters
        ) {
            var distance = ValidateDistance(useParameters ? _segmentLength : info.distance);
            var horizontalTurnAngle = ValidateHorizontalTurnAngle(
                useParameters ? _horizontalTurnAngle : info.horizontalTurnAngle
            );
            var verticalOffset = ValidateVerticalOffset(useParameters ? _verticalOffset : info.verticalOffset);
            var curvature = ValidateCurvature(useParameters ? _curvature : info.curvature); 
            
            return new SegmentSettings {
                start = start,
                startDir = startDir,
                distance = distance,
                horizontalTurnAngle = horizontalTurnAngle,
                verticalOffset = verticalOffset,
                curvature = curvature
            };
        }

        private float ValidateDistance(float distance) {
            if (distance > _maxSegmentLength) return _maxSegmentLength;
            if (distance < _minSegmentLength) return _minSegmentLength;
            return distance;
        }

        private float ValidateHorizontalTurnAngle(float angle) {
            return Mathf.Clamp(angle, -_maxHorizontalTurnAngle, _maxHorizontalTurnAngle);
        }

        private float ValidateVerticalOffset(float verticalOffset) {
            return Mathf.Clamp(verticalOffset, -_maxVerticalOffset, _maxVerticalOffset);
        }
        
        private float ValidateCurvature(float curvature) {
            if (curvature > _maxCurvature) return _maxCurvature;
            if (curvature < _minCurvature) return _minCurvature;
            return curvature;
        }
        
        
        // ---------------------------- Utility functions ----------------------------

        private Vector3 GetStartPoint() {
            var segmentCount = _currentSpline.Count;
            if (segmentCount == 0) return transform.position;
            var lastSegmentInfo = _currentSpline[segmentCount - 1];
            return lastSegmentInfo.end;
        }
        
        private Vector3 GetStartDir() {
            var segmentCount = _currentSpline.Count;
            if (segmentCount == 0) return _initialDirection;
            var lastSegmentInfo = _currentSpline[segmentCount - 1];
            return lastSegmentInfo.GetEndDirection();
        }
        
        private float GetSegmentDistance() {
            return _useRandomLength 
                ? Random.Range(_minSegmentLength, _maxSegmentLength) 
                : _segmentLength;
        }

        private float GetHorizontalTurnAngle() {
            return _useRandomAngle 
                ? Random.Range(-_maxHorizontalTurnAngle, _maxHorizontalTurnAngle) 
                : _horizontalTurnAngle;
        }
        
        private float GetVerticalOffset() {
            return _useRandomOffset
                ? Random.Range(-_maxVerticalOffset, _maxVerticalOffset)
                : _verticalOffset;
        }

        private float GetCurvature() {
            return _useRandomCurvature 
                ? Random.Range(_minCurvature, _maxCurvature) 
                : _curvature;
        }

        
        // ---------------------------- Data ----------------------------
        
        [Serializable]
        private struct SegmentInfo {
            
            public Vector3 start;
            public Vector3 controlStart;
            public Vector3 controlEnd;
            public Vector3 end;
            
            public float distance;
            public float horizontalTurnAngle;
            public float verticalOffset;
            public float curvature;

            public Vector3 GetStartDirection() {
                return controlStart - start;
            }
            
            public Vector3 GetEndDirection() {
                return end - controlEnd;
            }
            
        }
        
        [Serializable]
        private struct SegmentSettings {
            public Vector3 start;
            public Vector3 startDir;
            public float distance;
            public float horizontalTurnAngle;
            public float verticalOffset;
            public float curvature;        
        }
        
    }

}