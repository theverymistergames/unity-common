using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Strings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MisterGames.Common.Audio {

    public sealed class AudioBankService : IAudioBankService, IDisposable {

        private const bool EnableLogs = true;
        private static readonly string LogPrefix = nameof(AudioBankService).FormatColorOnlyForEditor(Color.white);

        private readonly Dictionary<string, BankEntry> _bankEntryMap = new();

        private sealed class BankEntry {
            public readonly HashSet<int> sources = new();
            public AsyncOperationHandle<AudioBank> handle;
            public AudioBank bank;
            public CancellationTokenSource cts;
        }

        public void Dispose() {
            foreach (var entry in _bankEntryMap.Values) {
                ReleaseBank(entry);
            }

            _bankEntryMap.Clear();
        }

        public void Bind(object source, AudioBankReference bank) {
            if (source == null) {
                LogError($"trying to bind audio bank [{bank?.AssetGUID}] with null source.");
                return;
            }

            if (bank == null || !bank.RuntimeKeyIsValid()) {
                LogError($"source [{source}] is trying to bind an invalid audio bank reference.");
                return;
            }

            string key = bank.AssetGUID;

            if (!_bankEntryMap.TryGetValue(key, out var entry)) {
                entry = new BankEntry();
                _bankEntryMap[key] = entry;
            }

            if (!entry.sources.Add(source.GetHashCode()) || entry.sources.Count > 1) return;

            LoadBank(key, bank.RuntimeKey, entry).Forget();
        }

        public void Unbind(object source, AudioBankReference bank) {
            if (source == null || bank == null || !bank.RuntimeKeyIsValid()) return;

            string key = bank.AssetGUID;

            if (!_bankEntryMap.TryGetValue(key, out var entry) ||
                !entry.sources.Remove(source.GetHashCode()) ||
                entry.sources.Count > 0)
            {
                return;
            }

            _bankEntryMap.Remove(key);
            ReleaseBank(entry);

            LogInfo($"audio bank [{entry.bank}] is unloaded, no sources left.");
        }

        private static async UniTask LoadBank(string key, object runtimeKey, BankEntry entry) {
            AsyncExt.RecreateCts(ref entry.cts);
            var cancellationToken = entry.cts.Token;

            AsyncOperationHandle<AudioBank> handle;

            try {
                handle = Addressables.LoadAssetAsync<AudioBank>(runtimeKey);
            }
            catch (Exception exception) {
                LogError($"can not start loading audio bank [{key}]: {exception.Message}.");
                return;
            }

            if (!handle.IsValid()) {
                LogError($"can not start loading audio bank [{key}]: operation handle is invalid.");
                return;
            }

            entry.handle = handle;

            try {
                await handle.ToUniTask(cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException) {
                // The last source has unbound the bank while it was loading, the handle is released already.
                return;
            }
            catch (Exception exception) {
                LogError($"loading audio bank [{key}] failed: {exception.Message}.");
                return;
            }

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null) {
                LogError($"audio bank [{key}] is not loaded: operation status is {handle.Status}.");
                return;
            }

            entry.bank = handle.Result;

            int count = entry.bank.PreloadAudioData();

            LogInfo($"audio bank [{entry.bank.name}] is loaded for {entry.sources.Count} sources, " +
                    $"started loading audio data of {count} clips.");
        }

        private static void ReleaseBank(BankEntry entry) {
            AsyncExt.DisposeCts(ref entry.cts);

            if (entry.bank != null) entry.bank.UnloadAudioData();
            entry.bank = null;

            if (entry.handle.IsValid()) Addressables.Release(entry.handle);
            entry.handle = default;
        }

        private static void LogInfo(string message) {
            if (EnableLogs) Debug.Log($"{LogPrefix}: f {Time.frameCount}, {message}");
        }

        private static void LogError(string message) {
            if (EnableLogs) Debug.LogError($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
    }

}
