using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Tick.Core;
using MisterGames.TweenLib.MotionCapture;
using UnityEditor;
using UnityEngine;

namespace MisterGames.TweenLib.Editor.MotionCapture {
    
    [CustomEditor(typeof(MotionCaptureClip))]
    public class MotionCaptureClipEditor : UnityEditor.Editor {

        private CancellationTokenSource _cts;
        
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (target is not MotionCaptureClip clip) return;
            
            GUILayout.Space(10f);
            GUILayout.Label("Preview on camera", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Play")) {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = new CancellationTokenSource();
                
                Play(clip, _cts.Token).Forget();
            }

            if (GUILayout.Button("Stop")) {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
            }
            
            GUILayout.EndHorizontal();
        }

        private static async UniTaskVoid Play(MotionCaptureClip clip, CancellationToken cancellationToken) {
            var camera = FindObjectOfType<Camera>();
            if (camera == null) return;

            var t = camera.transform;
            float duration = clip.Duration;
            float progress = 0f;
            var timeSource = TimeSources.Get(PlayerLoopStage.Update);

            var initialPosition = t.localPosition;
            var initialRotation = t.localRotation;
            
            while (!cancellationToken.IsCancellationRequested) {
                progress = duration > 0f ? Mathf.Clamp01(progress + timeSource.DeltaTime / duration) : 1f;

                t.localPosition = initialPosition + clip.EvaluatePosition(progress);
                t.localRotation = initialRotation * clip.EvaluateRotation(progress);

                if (progress >= 1f) break;
                
                await UniTask.Yield();
            }
        }
    }
    
}