using System;
using System.Collections.Generic;
using MisterGames.Input.Global;
using MisterGames.Tick.Core;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Input.Core {

    [Serializable]
    public sealed class InputUpdater : IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.PreUpdate;
        [SerializeField] private InputChannel _inputChannel;

        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);

        public void Awake() {
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
            _inputChannel.DoUpdate(dt);
        }

#if UNITY_EDITOR
        private static InputUpdater _editorInputUpdater;

        public static void CheckEditorInputUpdaterIsCreated() {
            if (Application.isPlaying) return;
            if (_editorInputUpdater != null) return;

            var inputChannels = new List<InputChannel>();

            string[] guids = AssetDatabase.FindAssets($"a:assets t:{nameof(InputChannel)}");
            for (int i = 0; i < guids.Length; i++) {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (string.IsNullOrEmpty(path)) continue;

                var inputChannel = AssetDatabase.LoadAssetAtPath<InputChannel>(path);
                if (inputChannel == null) continue;

                inputChannels.Add(inputChannel);
            }

            if (inputChannels.Count > 0) {
                _editorInputUpdater = new InputUpdater();
                _editorInputUpdater._timeSourceStage = PlayerLoopStage.PreUpdate;

                _editorInputUpdater._inputChannel = inputChannels[0];
                _editorInputUpdater.Awake();
                _editorInputUpdater.OnEnable();
            }
        }
#endif
    }

}
