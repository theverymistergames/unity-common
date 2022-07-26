using System.Collections.Generic;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Routines {

    public interface IUpdate {
        void OnUpdate(float dt);
    }
    
    public interface ILateUpdate {
        void OnLateUpdate(float dt);
    }
    
    public interface IFixedUpdate {
        void OnFixedUpdate(float dt);
    }

    [CreateAssetMenu(fileName = nameof(TimeDomain), menuName = "MisterGames/" + nameof(TimeDomain))]
    public sealed class TimeDomain : ScriptableObject {

        [Header("Time Scale")]
        [SerializeField] [Range(0f, 2f)] private float _timeScale = 1f;
        [SerializeField] [Range(0f, 2f)] private float _fixedTimeScale = 1f;
        
        [Header("Current Frame Info")]
        [ReadOnly] [SerializeField] private float _deltaTime;
        [ReadOnly] [SerializeField] private float _fixedDeltaTime;

        [HideInInspector] [SerializeField] private bool _isPaused = false;

        public float DeltaTime => _deltaTime;
        public float FixedDeltaTime => _fixedDeltaTime;

        public float TimeScale {
            get => _timeScale;
            set {
                _timeScale = value;
                UpdateDeltaTime();
            }
        }

        public float FixedTimeScale {
            get => _fixedTimeScale;
            set {
                _fixedTimeScale = value;
                UpdateFixedDeltaTime();
            }
        }

        public bool IsPaused {
            get => _isPaused;
            set {
                _isPaused = value;
                UpdateDeltaTime();
                UpdateFixedDeltaTime();
            }
        }

        public bool IsActive { get; private set; }
        public bool IsStarted { get; private set; }

        private readonly List<IUpdate> _updateList = new List<IUpdate>();
        private readonly List<IUpdate> _updateListToAdd = new List<IUpdate>();
        private readonly List<IUpdate> _updateListToRemove = new List<IUpdate>();
        
        private readonly List<ILateUpdate> _lateUpdateList = new List<ILateUpdate>();
        private readonly List<ILateUpdate> _lateUpdateListToAdd = new List<ILateUpdate>();
        private readonly List<ILateUpdate> _lateUpdateListToRemove = new List<ILateUpdate>();
        
        private readonly List<IFixedUpdate> _fixedUpdateList = new List<IFixedUpdate>();
        private readonly List<IFixedUpdate> _fixedUpdateListToAdd = new List<IFixedUpdate>();
        private readonly List<IFixedUpdate> _fixedUpdateListToRemove = new List<IFixedUpdate>();

        private bool IsUpdating => !IsPaused && IsStarted && IsActive;

        public void SubscribeUpdate(IUpdate update) {
            if (!IsUpdating) {
                if (!_updateList.Contains(update)) _updateList.Add(update);
                return;
            }
            
            if (_updateListToAdd.Contains(update)) return;
            
            if (_updateListToRemove.Contains(update)) {
                _updateListToRemove.Remove(update);
                return;
            }
            
            _updateListToAdd.Add(update);
        }
        
        public void UnsubscribeUpdate(IUpdate update) {
            if (!IsUpdating) {
                if (_updateList.Contains(update)) _updateList.Remove(update);
                return;
            }
            
            if (_updateListToRemove.Contains(update)) return;
            
            if (_updateListToAdd.Contains(update)) {
                _updateListToAdd.Remove(update);
                return;
            }
            
            _updateListToRemove.Add(update);
        }
        
        public void SubscribeLateUpdate(ILateUpdate update) {
            if (!IsUpdating) {
                if (!_lateUpdateList.Contains(update)) _lateUpdateList.Add(update);
                return;
            }
            
            if (_lateUpdateListToAdd.Contains(update)) return;
            
            if (_lateUpdateListToRemove.Contains(update)) {
                _lateUpdateListToRemove.Remove(update);
                return;
            }
            
            _lateUpdateListToAdd.Add(update);
        }
        
        public void UnsubscribeLateUpdate(ILateUpdate update) {
            if (!IsUpdating) {
                if (_lateUpdateList.Contains(update)) _lateUpdateList.Remove(update);
                return;
            }
            
            if (_lateUpdateListToRemove.Contains(update)) return;
            
            if (_lateUpdateListToAdd.Contains(update)) {
                _lateUpdateListToAdd.Remove(update);
                return;
            }
            
            _lateUpdateListToRemove.Add(update);
        }
        
        public void SubscribeFixedUpdate(IFixedUpdate update) {
            if (!IsUpdating) {
                if (!_fixedUpdateList.Contains(update)) _fixedUpdateList.Add(update);
                return;
            }
            
            if (_fixedUpdateListToAdd.Contains(update)) return;
            
            if (_fixedUpdateListToRemove.Contains(update)) {
                _fixedUpdateListToRemove.Remove(update);
                return;
            }
            
            _fixedUpdateListToAdd.Add(update);
        }
        
        public void UnsubscribeFixedUpdate(IFixedUpdate update) {
            if (!IsUpdating) {
                if (_fixedUpdateList.Contains(update)) _fixedUpdateList.Remove(update);
                return;
            }
            
            if (_fixedUpdateListToRemove.Contains(update)) return;
            
            if (_fixedUpdateListToAdd.Contains(update)) {
                _fixedUpdateListToAdd.Remove(update);
                return;
            }
            
            _fixedUpdateListToRemove.Add(update);
        }

        internal void Start() {
            IsStarted = true;
            UpdateDeltaTime();
            UpdateFixedDeltaTime();
        }
        
        internal void Activate() {
            IsActive = true;
            UpdateDeltaTime();
            UpdateFixedDeltaTime();
        }
        
        internal void Deactivate() {
            IsActive = false;
            UpdateDeltaTime();
            UpdateFixedDeltaTime();
        }
        
        internal void Terminate() {
            IsStarted = false;
            IsActive = false;

            UpdateDeltaTime();
            UpdateFixedDeltaTime();
            
            _updateList.Clear();
            _updateListToAdd.Clear();
            _updateListToRemove.Clear();
            
            _lateUpdateList.Clear();
            _lateUpdateListToAdd.Clear();
            _lateUpdateListToRemove.Clear();
            
            _fixedUpdateList.Clear();
            _fixedUpdateListToAdd.Clear();
            _fixedUpdateListToRemove.Clear();
        }
        
        internal void DoUpdate() {
            UpdateDeltaTime();
            if (!IsUpdating) return;

            for (int i = 0; i < _updateListToAdd.Count; i++) {
                _updateList.Add(_updateListToAdd[i]);
            }
            
            for (int i = 0; i < _updateListToRemove.Count; i++) {
                _updateList.Remove(_updateListToRemove[i]);
            }
            
            _updateListToAdd.Clear();
            _updateListToRemove.Clear();
            
            for (int i = 0; i < _updateList.Count; i++) {
                _updateList[i].OnUpdate(DeltaTime);
            }
        }

        internal void LateUpdate() {
            if (!IsUpdating) return;
            
            for (int i = 0; i < _lateUpdateListToAdd.Count; i++) {
                _lateUpdateList.Add(_lateUpdateListToAdd[i]);
            }
            
            for (int i = 0; i < _lateUpdateListToRemove.Count; i++) {
                _lateUpdateList.Remove(_lateUpdateListToRemove[i]);
            }
            
            _lateUpdateListToAdd.Clear();
            _lateUpdateListToRemove.Clear();
            
            for (int i = 0; i < _lateUpdateList.Count; i++) {
                _lateUpdateList[i].OnLateUpdate(DeltaTime);
            }
        }
        
        internal void FixedUpdate() {
            UpdateFixedDeltaTime();
            if (!IsUpdating) return;

            for (int i = 0; i < _fixedUpdateListToAdd.Count; i++) {
                _fixedUpdateList.Add(_fixedUpdateListToAdd[i]);
            }
            
            for (int i = 0; i < _fixedUpdateListToRemove.Count; i++) {
                _fixedUpdateList.Remove(_fixedUpdateListToRemove[i]);
            }
            
            _fixedUpdateListToAdd.Clear();
            _fixedUpdateListToRemove.Clear();
            
            for (int i = 0; i < _fixedUpdateList.Count; i++) {
                _fixedUpdateList[i].OnFixedUpdate(DeltaTime);
            }
        }

        private void UpdateDeltaTime() {
            _deltaTime = IsUpdating ? Time.unscaledDeltaTime * _timeScale : 0f;
        }

        private void UpdateFixedDeltaTime() {
            _fixedDeltaTime = IsUpdating ? Time.fixedUnscaledDeltaTime * _fixedTimeScale : 0f;
        }
    }

}