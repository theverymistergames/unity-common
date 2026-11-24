using System;
using System.Collections.Generic;
using MisterGames.Common.Strings;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Common.Audio {

    public sealed class AudioBankService : IAudioBankService, IUpdate, IDisposable {

        private const bool EnableLogs = true;
        private static readonly string LogPrefix = nameof(AudioBankService).FormatColorOnlyForEditor(Color.white);

        private readonly AudioBanksSettings _settings;
        private readonly Dictionary<AudioBank, HashSet<int>> _bankSourcesMap = new();
        private readonly Dictionary<AudioBank, float> _pendingUnloadMap = new();
        private readonly List<AudioBank> _unloadBuffer = new();

        public AudioBankService(AudioBanksSettings settings) {
            _settings = settings;
        }

        public void Dispose() {
            PlayerLoopStage.Update.Unsubscribe(this);

            foreach (var bank in _bankSourcesMap.Keys) {
                if (bank != null) bank.UnloadAudioData();
            }

            foreach (var bank in _pendingUnloadMap.Keys) {
                if (bank != null) bank.UnloadAudioData();
            }

            _bankSourcesMap.Clear();
            _pendingUnloadMap.Clear();
        }

        public void Bind(object source, AudioBank bank) {
            if (source == null) {
                LogError($"trying to bind audio bank [{bank}] with null source.");
                return;
            }

            if (bank == null) {
                LogError($"source [{source}] is trying to bind a null audio bank.");
                return;
            }

            if (!_bankSourcesMap.TryGetValue(bank, out var sources)) {
                sources = new HashSet<int>();
                _bankSourcesMap[bank] = sources;
            }

            if (!sources.Add(source.GetHashCode()) || sources.Count > 1) return;

            if (_pendingUnloadMap.Remove(bank)) {
                if (_pendingUnloadMap.Count == 0) PlayerLoopStage.Update.Unsubscribe(this);

                LogInfo($"audio bank [{bank.name}] is bound again, pending unload is cancelled.");
                return;
            }

            int count = bank.PreloadAudioData();

            LogInfo($"audio bank [{bank.name}] is bound, started loading audio data of {count} clips.");
        }

        public void Unbind(object source, AudioBank bank) {
            if (source == null || bank == null) return;

            if (!_bankSourcesMap.TryGetValue(bank, out var sources) ||
                !sources.Remove(source.GetHashCode()) ||
                sources.Count > 0)
            {
                return;
            }

            _bankSourcesMap.Remove(bank);

            float delay = GetUnloadDelay();

            if (delay <= 0f) {
                UnloadBank(bank);
                return;
            }

            _pendingUnloadMap[bank] = Time.unscaledTime + delay;
            PlayerLoopStage.Update.Subscribe(this);

            LogInfo($"audio bank [{bank.name}] is unbound, no sources left, unload in {delay:0.##} s.");
        }

        public void OnUpdate(float dt) {
            float time = Time.unscaledTime;

            foreach (var (bank, unloadTime) in _pendingUnloadMap) {
                if (bank == null || time >= unloadTime) _unloadBuffer.Add(bank);
            }

            for (int i = 0; i < _unloadBuffer.Count; i++) {
                var bank = _unloadBuffer[i];
                _pendingUnloadMap.Remove(bank);

                if (bank != null) UnloadBank(bank);
            }

            _unloadBuffer.Clear();

            if (_pendingUnloadMap.Count == 0) PlayerLoopStage.Update.Unsubscribe(this);
        }

        private float GetUnloadDelay() {
            return _settings != null ? _settings.unloadDelay : 0f;
        }

        private static void UnloadBank(AudioBank bank) {
            int count = bank.UnloadAudioData();

            LogInfo($"audio bank [{bank.name}] is unloaded, unloaded audio data of {count} clips.");
        }

        private static void LogInfo(string message) {
            if (EnableLogs) Debug.Log($"{LogPrefix}: f {Time.frameCount}, {message}");
        }

        private static void LogError(string message) {
            if (EnableLogs) Debug.LogError($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
    }

}
