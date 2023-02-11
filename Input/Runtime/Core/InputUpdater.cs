using System;
using System.Collections.Generic;
using MisterGames.Input.Global;
using MisterGames.Tick.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Core {

    [Serializable]
    public sealed class InputUpdater : IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.PreUpdate;
        [SerializeField] private InputChannel _inputChannel;

        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);

        public void Awake() {
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
            GlobalInput.Init();
            _inputChannel.Init();
        }

        public void OnDestroy() {
            GlobalInput.Terminate();
            _inputChannel.Terminate();
        }

        public void OnEnable() {
            GlobalInput.Enable();
            _inputChannel.Activate();
            _timeSource.Subscribe(this);
        }

        public void OnDisable() {
            GlobalInput.Disable();
            _inputChannel.Deactivate();
            _timeSource.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            InputSystem.Update();
            _inputChannel.DoUpdate(dt);
        }

#if UNITY_EDITOR
        private static InputUpdater _editorInputUpdater;
        private static InputChannel _editorInputChannel;
        private static readonly HashSet<int> _sources = new HashSet<int>();

        public static bool CheckEditorInputUpdaterIsStarted(object source, out InputChannel inputChannel) {
            inputChannel = null;

            if (Application.isPlaying) {
                Debug.LogWarning($"Cannot start editor InputUpdater while application is playing");
                return false;
            }

            _sources.Add(source.GetHashCode());

            if (_editorInputUpdater != null) {
                inputChannel = _editorInputChannel;
                return true;
            }

            _editorInputChannel = ScriptableObject.CreateInstance<InputChannel>();
            inputChannel = _editorInputChannel;

            _editorInputUpdater = new InputUpdater();
            _editorInputUpdater._timeSourceStage = PlayerLoopStage.PreUpdate;
            _editorInputUpdater._inputChannel = _editorInputChannel;

            InputSystem.settings.SetInternalFeatureFlag("RUN_PLAYER_UPDATES_IN_EDIT_MODE", true);

            _editorInputUpdater.Awake();
            _editorInputUpdater.OnEnable();

            return true;
        }

        public static void CheckEditorInputUpdaterIsStopped(object source) {
            if (Application.isPlaying) {
                Debug.LogWarning($"Cannot stop editor InputUpdater while application is playing");
                return;
            }

            _sources.Remove(source.GetHashCode());
            if (_sources.Count > 0 || _editorInputUpdater == null) return;

            _editorInputUpdater.OnDisable();
            _editorInputUpdater.OnDestroy();
            _editorInputUpdater = null;
            _editorInputChannel = null;
        }
#endif
    }

}
