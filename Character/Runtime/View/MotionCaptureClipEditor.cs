#if UNITY_EDITOR

using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.View;
using MisterGames.Common;
using MisterGames.Common.Async;
using MisterGames.Tick.Core;
using UnityEditor;
using UnityEngine;

namespace MisterGames.TweenLib.Editor.MotionCapture {
    
    [CustomEditor(typeof(MotionCaptureClip))]
    public class MotionCaptureClipEditor : UnityEditor.Editor {

        private CancellationTokenSource _cts;
        private Vector3 _initialPos;
        private Quaternion _initialRot;
        private float _previewProgress;
        private byte _playId;

        private void OnDisable() {
            _previewProgress = 0f;
            AsyncExt.DisposeCts(ref _cts);
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (target is not MotionCaptureClip clip) return;

            var camera = GetCamera();
            if (camera == null) {
                Debug.LogWarning($"{nameof(MotionCaptureClipEditor)}: cannot find Camera on current scene");
                return;
            }

            var cameraTransform = camera.transform;
            
            GUILayout.Space(10f);
            GUILayout.Label("Preview on camera", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            if (_cts is {IsCancellationRequested: false}) {
                if (GUILayout.Button("Pause")) {
                    AsyncExt.DisposeCts(ref _cts);
                }
            }
            else {
                if (GUILayout.Button("Play")) {
                    AsyncExt.RecreateCts(ref _cts);
                    Play(cameraTransform, clip, _cts.Token).Forget();
                }
            }
            
            if (GUILayout.Button("Stop")) {
                _previewProgress = 0f;
                AsyncExt.DisposeCts(ref _cts);
            }
            
            if (GUILayout.Button("Reset Camera Transform")) {
                cameraTransform.localPosition = Vector3.zero;
                cameraTransform.localRotation = Quaternion.identity;
            }
            
            GUILayout.EndHorizontal();

            _previewProgress = EditorGUILayout.Slider("Progress", _previewProgress, 0f, 1f);
        }

        private static Camera GetCamera() {
            var cameras = FindObjectsOfType<Camera>();
            if (cameras.Length == 1) return cameras[0];

            for (int i = 0; i < cameras.Length; i++) {
                if (cameras[i].gameObject.CompareTag("MainCamera")) return cameras[i];
            }

            return null;
        }
        
        private async UniTaskVoid Play(Transform t, MotionCaptureClip clip, CancellationToken token) {
            byte id = ++_playId;
            float duration = clip.Duration;
            var timeSource = PlayerLoopStage.Update.Get();

            if (_previewProgress <= 0f) t.GetLocalPositionAndRotation(out _initialPos, out _initialRot);
            
            var prev = t.position;
            
            while (!token.IsCancellationRequested && id == _playId) {
                _previewProgress = duration > 0f ? Mathf.Clamp01(_previewProgress + timeSource.DeltaTime / duration) : 1f;

                t.SetLocalPositionAndRotation(
                    _initialPos + clip.EvaluatePosition(_previewProgress), 
                    _initialRot * clip.EvaluateRotation(_previewProgress)
                );

                var p = t.position;
                DebugExt.DrawLine(p, prev, Color.green, duration: 15f);
                DebugExt.DrawSphere(p, 0.001f, Color.yellow, duration: 15f);
                prev = p;
                
                Repaint();

                if (_previewProgress >= 1f) {
                    AsyncExt.DisposeCts(ref _cts);
                    break;
                }
                
                await UniTask.Yield();
            }
        }
    }
    
}

#endif