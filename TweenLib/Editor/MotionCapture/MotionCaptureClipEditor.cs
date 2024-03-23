using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common;
using MisterGames.Tick.Core;
using MisterGames.TweenLib.MotionCapture;
using UnityEditor;
using UnityEngine;

namespace MisterGames.TweenLib.Editor.MotionCapture {
    
    [CustomEditor(typeof(MotionCaptureClip))]
    public class MotionCaptureClipEditor : UnityEditor.Editor {

        private CancellationTokenSource _cts;
        private float _previewProgress;

        private void OnDisable() {
            _previewProgress = 0f;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (target is not MotionCaptureClip clip) return;

            var camera = FindObjectOfType<Camera>();
            if (camera == null) return;

            var cameraTransform = camera.transform;
            
            GUILayout.Space(10f);
            GUILayout.Label("Preview on camera", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Play")) {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = new CancellationTokenSource();
                
                Play(cameraTransform, clip, _cts.Token).Forget();
            }

            if (GUILayout.Button("Stop")) {
                if (_previewProgress > 0f) {
                    cameraTransform.localPosition = Vector3.zero;
                    cameraTransform.localRotation = Quaternion.identity;
                }
                
                _previewProgress = 0f;
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
            }
            
            if (GUILayout.Button("Reset Camera Transform")) {
                cameraTransform.localPosition = Vector3.zero;
                cameraTransform.localRotation = Quaternion.identity;
            }
            
            GUILayout.EndHorizontal();

            GUI.enabled = false;
            EditorGUILayout.Slider("Progress", _previewProgress, 0f, 1f);
            GUI.enabled = true;
        }

        private async UniTaskVoid Play(UnityEngine.Transform t, MotionCaptureClip clip, CancellationToken cancellationToken) {
            float duration = clip.Duration;
            var timeSource = TimeSources.Get(PlayerLoopStage.Update);

            var initialPosition = t.localPosition;
            var initialRotation = t.localRotation;
            _previewProgress = 0f;

            var prev = t.position;
            
            while (!cancellationToken.IsCancellationRequested) {
                _previewProgress = duration > 0f ? Mathf.Clamp01(_previewProgress + timeSource.DeltaTime / duration) : 1f;

                t.localPosition = initialPosition + clip.EvaluatePosition(_previewProgress);
                t.localRotation = initialRotation * clip.EvaluateRotation(_previewProgress);

                DebugExt.DrawLine(t.position, prev, Color.green, duration: 15f);
                DebugExt.DrawSphere(t.position, 0.001f, Color.yellow, duration: 15f);
                prev = t.position;
                
                if (_previewProgress >= 1f) break;
                
                Repaint();
                await UniTask.Yield();
            }
        }
    }
    
}