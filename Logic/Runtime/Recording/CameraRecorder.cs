using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Lists;
using MisterGames.Input.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Logic.Recording {
    
    public sealed class CameraRecorder : MonoBehaviour, IActorComponent {

        [TextArea] [SerializeField] [ReadOnly] private string _comment = 
            "Для вкл/выкл записи нажать O, плей/стоп - P, очистить - [, сбросить позицию камеры после проигрывания - ]";
        
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private InputActionRef _recordInput;
        [SerializeField] private InputActionRef _playInput;
        [SerializeField] private InputActionRef _clearInput;
        [SerializeField] private InputActionRef _resetCameraPositionInput;
        
        [Header("Recording")]
        [SerializeField] private bool _isRecording;
        [SerializeField] private List<Data> _dataArray;
        [SerializeField] private List<Data> _reserveDataArray;

        [Header("Playing")]
        [SerializeField] [Min(0f)] private float _playDelay = 3f;
        [SubclassSelector]
        [SerializeReference] private IActorAction _onPlay;
        [SubclassSelector]
        [SerializeReference] private IActorAction _onStop;

        [Header("Saving")]
        [SerializeField] private string _loadName;
            
        [Serializable]
        public struct Data {
            public Vector3 pos;
            public Quaternion rot;
            public float time;
        }

        private CancellationTokenSource _enableCts;
        private IActor _actor;
        
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private bool _startedRecord;
        private bool _isPlaying;
        private float _time;
        private byte _playId;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void Awake() {
            _cameraTransform.GetLocalPositionAndRotation(out _initialPosition, out _initialRotation);
            
            _dataArray ??= new List<Data>(256);
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            _recordInput.Get().performed += OnRecordPressed;
            _playInput.Get().performed += OnPlayPressed;
            _clearInput.Get().performed += OnClearPressed;
            _resetCameraPositionInput.Get().performed += OnResetPositionPressed;
        }
        
        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            _recordInput.Get().performed -= OnRecordPressed;
            _playInput.Get().performed -= OnPlayPressed;
            _clearInput.Get().performed -= OnClearPressed;
            _resetCameraPositionInput.Get().performed -= OnResetPositionPressed;
        }

        private void OnRecordPressed(InputAction.CallbackContext callbackContext) {
            ToggleRecord();
        }
        
        private void OnPlayPressed(InputAction.CallbackContext callbackContext) {
            PlayRecording(_enableCts.Token).Forget();
        }

        private void OnClearPressed(InputAction.CallbackContext callbackContext) {
            Clear();
        }

        private void OnResetPositionPressed(InputAction.CallbackContext callbackContext) {
            ResetCameraPosition();
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
                _onStop?.Apply(_actor, destroyCancellationToken).Forget();
                return;
            }

            if (_dataArray.Count <= 0) return;
            
            _isPlaying = true;
            float time = 0f;
            int index = 0;
            int count = _dataArray.Count;
            
            Debug.Log($"CameraRecorder.PlayRecording: f {Time.frameCount}, will start playing in {_playDelay} sec");

            _onPlay?.Apply(_actor, destroyCancellationToken).Forget();
            
            var data0 = _dataArray[0];
            _cameraTransform.SetPositionAndRotation(data0.pos, data0.rot);

            await UniTask.Delay(TimeSpan.FromSeconds(_playDelay), cancellationToken: cancellationToken)
                .SuppressCancellationThrow();
            
            if (cancellationToken.IsCancellationRequested || id != _playId) return;
            
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
            
            if (cancellationToken.IsCancellationRequested || id != _playId) return;
            
            _onStop?.Apply(_actor, destroyCancellationToken).Forget();
            
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
        
        [Button]
        private void Clear() {
            _playId++;

            bool wasPlaying = _isPlaying;
            
            _isRecording = false;
            _isPlaying = false;
            
            _dataArray ??= new List<Data>();
            _dataArray.Clear();

            ResetCameraPosition();

            if (wasPlaying) {
                _onStop?.Apply(_actor, destroyCancellationToken).Forget();
            }
            
            Debug.Log($"CameraRecorder.OnClearPressed: f {Time.frameCount}, recording cleared{(wasPlaying ? ", stop playing" : "")}");
        }
        
        [Button]
        private void ResetCameraPosition() {
            _cameraTransform.SetLocalPositionAndRotation(_initialPosition, _initialRotation);
        }

#if UNITY_EDITOR
        [Button]
        private void Save() {
            if (string.IsNullOrWhiteSpace(_loadName)) {
                Debug.LogWarning($"CameraRecorder.Save: Save Name is invalid.");
                return;
            }
            
            if (_isRecording) {
                Debug.LogWarning($"CameraRecorder.Save: cannot save while recording.");
                return;
            }
            
            int index = CameraRecorderStorage.Instance.recordings.TryFindIndex(_loadName, (e, s) => e.saveName == s);
            var entry = new CameraRecorderStorage.Entry { saveName = _loadName, data = new List<Data>(_dataArray) };
            
            if (index < 0) {
                Undo.RecordObject(CameraRecorderStorage.Instance, "CameraRecorder_Save");
                CameraRecorderStorage.Instance.recordings.Add(entry);
                EditorUtility.SetDirty(CameraRecorderStorage.Instance);
                
                Debug.Log($"CameraRecorder.Save: added new recording, name <color=yellow>{_loadName}</color>, frames {entry.data?.Count ?? 0}");
                return;
            }

            var backup = entry;
            backup.saveName = $"{entry.saveName}_backup_{DateTime.Now.ToString(CultureInfo.InvariantCulture)}";
            
            Undo.RecordObject(CameraRecorderStorage.Instance, "CameraRecorder_Save");
            CameraRecorderStorage.Instance.recordings[index] = entry;
            CameraRecorderStorage.Instance.backup.Add(backup);
            EditorUtility.SetDirty(CameraRecorderStorage.Instance);
            
            Debug.Log($"CameraRecorder.Save: saved recording at index {index}, name <color=yellow>{_loadName}</color>, frames {entry.data?.Count ?? 0}. " +
                      $"There was an old entry at this index, it is now moved into the backup entry: " +
                      $"<color=yellow>{backup.saveName}</color>.");
        }

        [Button]
        private void Load() {
            if (string.IsNullOrWhiteSpace(_loadName)) {
                Debug.LogWarning($"CameraRecorder.Load: Save Name is invalid.");
                return;
            }
            
            if (_isRecording) {
                Debug.LogWarning($"CameraRecorder.Load: cannot load while recording.");
                return;
            }
            
            int index = CameraRecorderStorage.Instance.recordings.TryFindIndex(_loadName, (e, s) => e.saveName == s);
            if (index < 0) {
                Debug.Log($"CameraRecorder.Save: recording with name <color=yellow>{_loadName}</color> is not found");
                return;
            }

            Undo.RecordObject(this, "CameraRecorder_Load");
            _dataArray = new List<Data>(CameraRecorderStorage.Instance.recordings[index].data);
            EditorUtility.SetDirty(this);

            Debug.Log($"CameraRecorder.Save: loaded recording at index {index}, name <color=yellow>{_loadName}</color>, frames {_dataArray?.Count ?? 0}");
            
            if (Application.isPlaying && _isPlaying) PlayRecording(_enableCts.Token).Forget();
        }
#endif
    }
    
}