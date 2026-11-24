using System;
using System.Collections.Generic;
using MisterGames.Common.Strings;
using UnityEngine;

namespace MisterGames.Common.Audio {

    public sealed class AudioBankService : IAudioBankService, IDisposable {

        private const bool EnableLogs = true;
        private static readonly string LogPrefix = nameof(AudioBankService).FormatColorOnlyForEditor(Color.white);

        private readonly Dictionary<AudioBank, HashSet<int>> _bankSourcesMap = new();

        public void Dispose() {
            foreach (var bank in _bankSourcesMap.Keys) {
                if (bank != null) bank.UnloadAudioData();
            }

            _bankSourcesMap.Clear();
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
            int count = bank.UnloadAudioData();

            LogInfo($"audio bank [{bank.name}] is unbound, no sources left, unloaded audio data of {count} clips.");
        }

        private static void LogInfo(string message) {
            if (EnableLogs) Debug.Log($"{LogPrefix}: f {Time.frameCount}, {message}");
        }

        private static void LogError(string message) {
            if (EnableLogs) Debug.LogError($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
    }

}
