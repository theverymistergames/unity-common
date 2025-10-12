using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Data;
using MisterGames.Common.Strings;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MisterGames.Common.Localization {
    
    public sealed class LocalizationService : ILocalizationService, IDisposable {
        
        private const bool EnableLogs = true;
        private static readonly string LogPrefix = nameof(LocalizationService).FormatColorOnlyForEditor(Color.white);

        public event Action<Locale> OnLocaleChanged = delegate { };

        public Locale Locale { get => _locale; set => SetLocale(value); }
        
        private readonly Dictionary<int, float> _tableUsageTimeMap = new();
        private readonly Dictionary<int, ILocalizationTable> _tableMap = new();
        private readonly Dictionary<int, AsyncOperationHandle<LocalizationTableStorageBase>> _tableStorageHandlesMap = new();

        private CancellationTokenSource _cts;
        private LocalizationSettings _settings;
        private Locale _locale;

        public void Initialize(LocalizationSettings settings) {
            _settings = settings;
            
            // todo load locale from saved settings
            var defaultLocale = settings.GetLocaleOrFallback(CreateSystemLocale());
            SetLocale(defaultLocale);
            
            AsyncExt.RecreateCts(ref _cts);
            StartTableDisposalRoutine(_cts.Token).Forget();
        }

        public void Dispose() {
            AsyncExt.DisposeCts(ref _cts);

            foreach (var table in _tableMap.Values) {
                if (table is IDisposable disposable) disposable.Dispose();
            }
            
            foreach (var handle in _tableStorageHandlesMap.Values) {
                Addressables.Release(handle);
            }
            
            _tableUsageTimeMap.Clear();
            _tableMap.Clear();
            _tableStorageHandlesMap.Clear();
        }

        public string GetLocalizedString(LocalizationKey key) {
            return GetLocalizedString(key, _locale);
        }

        public T GetLocalizedAsset<T>(LocalizationKey<T> key) {
            return GetLocalizedAsset(key, _locale);
        }
        
        public string GetLocalizedString(LocalizationKey key, Locale locale) {
            var table = GetTable(key.table.ToGuid());
            if (table == null) return null;
            
            if (table.TryGetValue(key.hash, locale.Hash, out string value) && !string.IsNullOrEmpty(value) ||
                _settings.ReplaceNotLocalizedStringsWithDefaultLocale &&
                table.TryGetValue(key.hash, _settings.GetDefaultFallbackLocale().Hash, out value)) 
            {
                return string.IsNullOrEmpty(value) ? _settings.GetFallbackString() : value;
            }

            return null;
        }

        public T GetLocalizedAsset<T>(LocalizationKey<T> key, Locale locale) {
            var table = GetTable(key.table.ToGuid());
            if (table == null) return default;
            
            if (table.TryGetValue(key.hash, locale.Hash, out T value) ||
                _settings.ReplaceNotLocalizedAssetsWithDefaultLocale &&
                table.TryGetValue(key.hash, _settings.GetDefaultFallbackLocale().Hash, out value)) 
            {
                return value;
            }

            return default;
        }

        private void SetLocale(Locale locale) {
            if (locale == _locale) return;
            
            _locale = _settings.GetLocaleOrFallback(locale);
            LogInfo($"set language: {_locale}");
            
            OnLocaleChanged.Invoke(_locale);
        }

        private ILocalizationTable GetTable(Guid guid) {
            if (guid == Guid.Empty) return null;
            
            int hash = guid.GetHashCode();
            
            if (_tableMap.TryGetValue(hash, out var table)) {
                _tableUsageTimeMap[hash] = Time.realtimeSinceStartup;
                return table;
            }
            
            var handle = Addressables.LoadAssetAsync<LocalizationTableStorageBase>(guid.ToUnityEditorGUID());
            _tableStorageHandlesMap[hash] = handle;
            
            handle.WaitForCompletion();

            switch (handle.Status) {
                case AsyncOperationStatus.Succeeded:
                    var storage = handle.Result;
                    table = new LocalizationTable(storage);
            
                    _tableMap[hash] = table;
                    _tableUsageTimeMap[hash] = Time.realtimeSinceStartup;
            
                    return table;
                
                case AsyncOperationStatus.None:
                case AsyncOperationStatus.Failed:
                    _tableStorageHandlesMap.Remove(hash);
                    LogError($"table with guid {guid} is not found.");
                    return null;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async UniTask StartTableDisposalRoutine(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                float time = Time.realtimeSinceStartup;
                float disposeDelay = _settings.UnloadUnusedTablesDelay;
                
                var disposeBuffer = new NativeArray<int>(_tableMap.Count, Allocator.Temp);
                int bufferCount = 0;
                
                foreach ((int hash, float lastUsageTime) in _tableUsageTimeMap) {
                    if (time > lastUsageTime + disposeDelay) disposeBuffer[bufferCount++] = hash; 
                }

                for (int i = 0; i < bufferCount; i++) {
                    int hash = disposeBuffer[i];
                    
                    _tableUsageTimeMap.Remove(hash);
                    
                    if (_tableMap.Remove(hash, out var table) && table is IDisposable disposable) {
                        disposable.Dispose();
                    }

                    if (_tableStorageHandlesMap.Remove(hash, out var handle)) {
                        Addressables.Release(handle);
                    }
                }
                
                disposeBuffer.Dispose();
                
                await UniTask.Delay(
                        TimeSpan.FromSeconds(disposeDelay), 
                        DelayType.UnscaledDeltaTime, 
                        cancellationToken: cancellationToken
                    )
                    .SuppressCancellationThrow();
            }
        }
        
        private static Locale CreateSystemLocale() {
            var id = LocaleExtensions.SystemLanguageToLocaleId(Application.systemLanguage);
            return LocaleExtensions.TryGetLocaleById(id, out var locale) ? locale : default;
        }
        
        private static void LogInfo(string message) {
            if (EnableLogs) Debug.Log($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
        
        private static void LogWarning(string message) {
            if (EnableLogs) Debug.LogWarning($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
        
        private static void LogError(string message) {
            if (EnableLogs) Debug.LogError($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
    }
    
}