﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.TweenLib.MotionCapture {

    [CreateAssetMenu(fileName = nameof(MotionCaptureClip), menuName = "MisterGames/MotionCapture/" + nameof(MotionCaptureClip))]
    public sealed class MotionCaptureClip : ScriptableObject {

        [Header("Input Data")]
        [SerializeField] private TextAsset _rotationData;
        [SerializeField] private TextAsset _accelerationData;
        
        [Header("Import Settings")]
        [SerializeField] private RotationSource _rotationSource;
        [SerializeField] private float _rotationAmplitude = 1f;
        [SerializeField] private float _velocityAmplitude = 1f;
        [SerializeField] [Min(0f)] private float _rotationSmoothing = 20f;
        [SerializeField] [Min(0f)] private float _positionSmoothing = 20f;
        [SerializeField] [Min(0f)] private float _velocitySmoothing = 20f;
        [SerializeField] [Min(0.001f)] private float _resolutionDeltaTime = 0.05f;
        [SerializeField] [MinMaxSlider(0f, 1f, show: true)] private Vector2 _crop = new Vector2(0f, 1f);

        [Header("Output Data")]
        [SerializeField] [ReadOnly] private float _duration;
        [SerializeField] [ReadOnly] private List<KeyFrame> _keyFrames;
        
        [Serializable]
        private struct KeyFrame {
            public Vector3 position;
            public Quaternion rotation;
        }
        
        private enum RotationSource {
            Gyroscope,
            OrientationQuaternion,
            OrientationRollPitchYaw,
        }

        public float Duration => _duration;

        public Vector3 EvaluatePosition(float t) {
            int count = _keyFrames.Count;
            int i = Mathf.FloorToInt(t * count);

            if (i < 0 || i > count - 1) return Vector3.zero;

            return i + 1 <= count - 1 
                ? Vector3.Lerp(_keyFrames[i].position, _keyFrames[i + 1].position, (t - (float) i / count) * count) 
                : _keyFrames[i].position;
        }
        
        public Quaternion EvaluateRotation(float t) {
            int count = _keyFrames.Count;
            int i = Mathf.FloorToInt(t * count);
            
            if (i < 0 || i > count - 1) return Quaternion.identity;

            return i + 1 <= count - 1 
                ? Quaternion.Slerp(_keyFrames[i].rotation, _keyFrames[i + 1].rotation, (t - (float) i / count) * count) 
                : _keyFrames[i].rotation;
        }

        private void OnValidate() {
            _keyFrames.Clear();

            using var rotationReader = _rotationData != null 
                ? new StreamReader(UnityEditor.AssetDatabase.GetAssetPath(_rotationData))
                : null;
            
            using var accelerationReader = _accelerationData != null
                ? new StreamReader(UnityEditor.AssetDatabase.GetAssetPath(_accelerationData))
                : null;

            rotationReader?.ReadLine();
            accelerationReader?.ReadLine();
            
            float timer = 0f;
            float prevTimeRot = -1f;
            float prevTimeAcc = -1f;
            
            var prevAcc = Vector3.zero;
            var prevEulers = Vector3.zero;
            
            var totalPosition = Vector3.zero;
            var totalRotation = Quaternion.identity;
            var totalVelocity = Vector3.zero;

            var smoothedPosition = Vector3.zero;
            var smoothedRotation = Quaternion.identity;
            var smoothedVelocity = Vector3.zero;
            
            while (true) {
                float dt = _resolutionDeltaTime;
                
                bool hasNextRot = false;
                bool hasNextAcc = false;
                
                var rot = prevEulers;
                var acc = prevAcc;

                while (rotationReader?.ReadLine() is { } rotationLine && !rotationReader.EndOfStream) {
                    ReadOnlySpan<string> line = rotationLine.Split(',');
                    
                    float t = ReadTime(line);
                    var eulers = ReadRotation(line, _rotationSource);
                    
                    if (t < timer) {
                        prevTimeRot = t;
                        prevEulers = eulers;
                        continue;
                    }
                    
                    hasNextRot = true;
                    
                    if (prevTimeRot > 0f) {
                        rot = Vector3.Lerp(prevEulers, eulers, (timer - prevTimeRot) / (t - prevTimeRot));
                        prevEulers = eulers;
                        prevTimeRot = t;
                    }
                    else {
                        rot = eulers;
                        prevTimeRot = t;
                        prevEulers = eulers;
                    }
                    
                    break;
                }

                while (accelerationReader?.ReadLine() is { } accelerationLine && !accelerationReader.EndOfStream) {
                    ReadOnlySpan<string> line = accelerationLine.Split(',');
                    
                    float t = ReadTime(line);
                    var acceleration = ReadAcceleration(line);
                    
                    if (t < timer) {
                        prevTimeAcc = t;
                        prevAcc = acceleration;
                        continue;
                    }
                    
                    hasNextAcc = true;
                    
                    if (prevTimeRot > 0f) {
                        acc = Vector3.Lerp(prevAcc, acceleration, (timer - prevTimeAcc) / (t - prevTimeAcc));
                        prevTimeAcc = t;
                        prevAcc = acceleration;
                    }
                    else {
                        acc = acceleration;
                        prevTimeAcc = t;
                        prevAcc = acceleration;
                    }
                    
                    break;
                }
                
                switch (_rotationSource) {
                    case RotationSource.Gyroscope:
                        if (hasNextRot) totalRotation *= Quaternion.Euler(Mathf.Rad2Deg * _rotationAmplitude * rot * dt);
                        break;
                    
                    case RotationSource.OrientationRollPitchYaw:
                    case RotationSource.OrientationQuaternion:
                        totalRotation = Quaternion.Euler(rot);
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                totalVelocity = hasNextAcc ? totalVelocity + acc * (dt * _velocityAmplitude) : Vector3.zero;
                smoothedVelocity = Vector3.Lerp(smoothedVelocity, totalVelocity, _velocitySmoothing * dt);
                totalPosition += totalVelocity * dt;
                
                smoothedRotation = Quaternion.Slerp(smoothedRotation, totalRotation, _rotationSmoothing * dt);
                smoothedPosition = Vector3.Lerp(smoothedPosition, totalPosition, _positionSmoothing * dt);
                
                _keyFrames.Add(new KeyFrame { position = totalPosition, rotation = smoothedRotation });
                
                if (!hasNextAcc && !hasNextRot) break;

                timer += dt;
            }

            _duration = Mathf.Max(0f,(_crop.y - _crop.x) * timer);
            
            int count = _keyFrames.Count;
            int lowerIndex = Mathf.FloorToInt(_crop.x * count);
            int upperIndex = Mathf.FloorToInt(_crop.y * count);
            
            if (_crop.y < 1f && ++upperIndex >= 0 && upperIndex <= count - 1) {
                _keyFrames.RemoveRange(upperIndex, count - 1 - upperIndex);
            }

            if (_crop.x > 0f && --lowerIndex >= 0 && lowerIndex <= _keyFrames.Count - 1) {
                _keyFrames.RemoveRange(0, lowerIndex + 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ReadTime(ReadOnlySpan<string> line) {
            return float.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 ReadAcceleration(ReadOnlySpan<string> line) {
            return new Vector3(
                float.Parse(line[4], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(line[3], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(line[2], CultureInfo.InvariantCulture.NumberFormat)
            );
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 ReadRotation(ReadOnlySpan<string> line, RotationSource source) {
            switch (source) {
                case RotationSource.Gyroscope:
                    return new Vector3(
                        float.Parse(line[4], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(line[3], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(line[2], CultureInfo.InvariantCulture.NumberFormat)
                    );

                case RotationSource.OrientationQuaternion:
                    return new Quaternion(
                        float.Parse(line[4], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(line[3], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(line[2], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(line[5], CultureInfo.InvariantCulture.NumberFormat)
                    ).eulerAngles;

                case RotationSource.OrientationRollPitchYaw:
                    return new Vector3(
                        float.Parse(line[6], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(line[8], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(line[7], CultureInfo.InvariantCulture.NumberFormat)
                    );

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
}