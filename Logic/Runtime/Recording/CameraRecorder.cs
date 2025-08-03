using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.Logic.Recording {
    
    public sealed class CameraRecorder : MonoBehaviour {

        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private InputActionKey _recordInput;
        [SerializeField] private InputActionKey _playInput;
        [SerializeField] private InputActionKey _clearInput;
        [SerializeField] private InputActionKey _resetCameraPositionInput;
        
        [Header("Recording")]
        [SerializeField] private bool _isRecording;
        [SerializeField] private List<Data> _dataArray;
        
        [Serializable]
        private struct Data {
            public Vector3 pos;
            public Quaternion rot;
            public float time;
        }

        private CancellationTokenSource _enableCts;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private bool _startedRecord;
        private bool _isPlaying;
        private float _time;
        private byte _playId;

        private void Awake() {
            _cameraTransform.GetLocalPositionAndRotation(out _initialPosition, out _initialRotation);
            
            _dataArray ??= new List<Data>(256);
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            _recordInput.OnPress += OnRecordPressed;
            _playInput.OnPress += OnPlayPressed;
            _clearInput.OnPress += OnClearPressed;
            _resetCameraPositionInput.OnPress += OnResetPositionPressed;
        }
        
        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            _recordInput.OnPress -= OnRecordPressed;
            _playInput.OnPress -= OnPlayPressed;
            _clearInput.OnPress -= OnClearPressed;
            _resetCameraPositionInput.OnPress -= OnResetPositionPressed;
        }

        private void OnRecordPressed() {
            ToggleRecord();
        }
        
        private void OnPlayPressed() {
            PlayRecording(_enableCts.Token).Forget();
        }

        private void OnClearPressed() {
            _playId++;

            bool wasPlaying = _isPlaying;
            
            _isRecording = false;
            _isPlaying = false;
            
            _dataArray ??= new List<Data>();
            _dataArray.Clear();

            OnResetPositionPressed();
            
            Debug.Log($"CameraRecorder.OnClearPressed: f {Time.frameCount}, recording cleared{(wasPlaying ? ", stop playing" : "")}");
        }

        private void OnResetPositionPressed() {
            _cameraTransform.SetLocalPositionAndRotation(_initialPosition, _initialRotation);
        }
        
        private void LateUpdate() {
            if (!_isRecording) return;

            float dt = Time.deltaTime;
            _time += dt;
            
            _cameraTransform.GetPositionAndRotation(out var pos, out var rot);
            
            _dataArray.Add(new Data { pos = pos, rot = rot, time = _time });
        }

        private async UniTask PlayRecording(CancellationToken cancellationToken) {
            if (_isRecording) return;

            byte id = ++_playId;
            
            if (_isPlaying) {
                _isPlaying = false;
                Debug.Log($"CameraRecorder.PlayRecording: f {Time.frameCount}, stop playing");
                return;
            }

            if (_dataArray.Count <= 0) return;
            
            _isPlaying = true;
            float time = 0f;
            int index = 0;
            int count = _dataArray.Count;
            
            Debug.Log($"CameraRecorder.PlayRecording: f {Time.frameCount}, start playing");
            
            while (!cancellationToken.IsCancellationRequested && id == _playId && index < count) {
                time += Time.deltaTime;

                while (index < count && _dataArray[index].time < time) {
                    index++;
                }

                if (index >= count) break;
                
                var d0 = _dataArray[index];
                var d1 = index + 1 < count ? _dataArray[index + 1] : d0;
                
                float t = d1.time - d0.time > 0f ? (time - d0.time) / (d1.time - d0.time) : 1f;
                
                var pos = Vector3.Lerp(d0.pos, d1.pos, t);
                var rot = Quaternion.Slerp(d0.rot, d1.rot, t);
                
                _cameraTransform.SetPositionAndRotation(pos, rot);
                
                await UniTask.Yield();
            }
            
            Debug.Log($"CameraRecorder.PlayRecording: f {Time.frameCount}, stop playing");
        }

        private void ToggleRecord() {
            if (_isRecording) {
                _isRecording = false;
                Debug.Log($"CameraRecorder.ToggleRecord: f {Time.frameCount}, stop recording");
                return;
            }

            if (_dataArray?.Count > 0) {
                Debug.Log($"CameraRecorder.ToggleRecord: f {Time.frameCount}, continue recording");
            }
            else {
                Debug.Log($"CameraRecorder.ToggleRecord: f {Time.frameCount}, start recording");
            }
            _isRecording = true;
        }
    }
    
}