using System;
using System.Collections.Generic;
using System.Globalization;
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

        public LocalizationSettings Settings { get; private set; }
        public Locale Locale { get => _locale; set => SetLocale(value); }
        
        private readonly Dictionary<Guid, float> _tableUsageTimeMap = new();
        private readonly Dictionary<Guid, ILocalizationTable> _tableMap = new();
        private readonly Dictionary<Guid, AsyncOperationHandle<LocalizationTableStorageBase>> _tableStorageHandlesMap = new();
        private readonly HashSet<ILocalizationFormatter> _formatters = new();
        
        private CancellationTokenSource _cts;
        private Locale _locale;

        public void Initialize(LocalizationSettings settings) {
            Settings = settings;
            
            if (_locale.IsNull()) SetLocale(GetDefaultLocale());
            
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

        public Locale GetDefaultLocale() {
            return Settings.GetLocaleOrFallback(GetSystemLocale());
        }

        public string GetId(LocalizationKey key) {
            return GetTable(key.table.ToGuid())?.TryGetKey(key.hash, out string id) ?? false
                ? id
                : null;
        }

        public string GetId<T>(LocalizationKey<T> key) {
            return GetTable(key.table.ToGuid())?.TryGetKey(key.hash, out string id) ?? false
                ? id
                : null;
        }

        public string GetLocalizedString(LocalizationKey key) {
            return GetLocalizedString(key, _locale);
        }

        public string GetLocalizedString(LocalizationKey key, Locale locale) {
            var table = GetTable(key.table.ToGuid());
            if (table == null) return null;

            if (table.TryGetValue(key.hash, locale.Hash, out string value) && 
                !string.IsNullOrEmpty(value)) 
            {
                FormatString(key, locale, ref value);
                return value;
            }
            
            if (Settings.ReplaceNotLocalizedStringsWithDefaultLocale &&
                table.TryGetValue(key.hash, Settings.GetDefaultFallbackLocale().Hash, out value)) 
            {
                if (string.IsNullOrEmpty(value)) return Settings.GetFallbackString();
                
                FormatString(key, Settings.GetDefaultFallbackLocale(), ref value);
                return value;
            }

            return null;
        }

        public Disposable<T> GetLocalizedAsset<T>(LocalizationKey<T> key) {
            return GetLocalizedAsset(key, _locale);
        }

        public Disposable<T> GetLocalizedAsset<T>(LocalizationKey<T> key, Locale locale) {
            var guid = key.table.ToGuid();
            var table = GetTable(guid);
            if (table == null) return default;
            
            if (table.TryGetDisposableValue<T>(key.hash, locale.Hash, out var value) ||
                Settings.ReplaceNotLocalizedAssetsWithDefaultLocale &&
                table.TryGetDisposableValue(key.hash, Settings.GetDefaultFallbackLocale().Hash, out value)) 
            {
                return value;
            }

            return default;
        }

        public void RegisterFormatter(ILocalizationFormatter formatter) {
            _formatters.Add(formatter);
        }
        
        public void UnregisterFormatter(ILocalizationFormatter formatter) {
            _formatters.Remove(formatter);
        }
        
        private void FormatString(LocalizationKey key, Locale locale, ref string value) {
            foreach (var formatter in _formatters) {
                formatter.Format(key, locale, ref value);
            }
        }

        private void SetLocale(Locale locale) {
            if (locale == _locale) return;
            
            _locale = Settings.GetLocaleOrFallback(locale);
            LogInfo($"set language: {_locale}");
            
            OnLocaleChanged.Invoke(_locale);
        }

        private ILocalizationTable GetTable(Guid guid) {
            if (guid == Guid.Empty) return null;
            
            if (_tableMap.TryGetValue(guid, out var table)) {
                _tableUsageTimeMap[guid] = Time.realtimeSinceStartup;
                return table;
            }
            
            var handle = Addressables.LoadAssetAsync<LocalizationTableStorageBase>(guid.ToUnityEditorGUID());
            _tableStorageHandlesMap[guid] = handle;
            
            handle.WaitForCompletion();

            switch (handle.Status) {
                case AsyncOperationStatus.Succeeded:
                    var storage = handle.Result;
                    table = new LocalizationTable(storage);
            
                    _tableMap[guid] = table;
                    _tableUsageTimeMap[guid] = Time.realtimeSinceStartup;
            
                    return table;
                
                case AsyncOperationStatus.None:
                case AsyncOperationStatus.Failed:
                    _tableStorageHandlesMap.Remove(guid);
                    LogError($"table with guid {guid} is not found.");
                    return null;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async UniTask StartTableDisposalRoutine(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                float time = Time.realtimeSinceStartup;
                float disposeDelay = Settings.UnloadUnusedTablesDelay;
                
                var disposeBuffer = new NativeArray<Guid>(_tableMap.Count, Allocator.Temp);
                int bufferCount = 0;
                
                foreach ((var guid, float lastUsageTime) in _tableUsageTimeMap) {
                    if (time < lastUsageTime + disposeDelay ||
                        _tableMap.TryGetValue(guid, out var table) && !table.CanUnload()) 
                    {
                        continue;
                    }
                    
                    disposeBuffer[bufferCount++] = guid; 
                }

                for (int i = 0; i < bufferCount; i++) {
                    var guid = disposeBuffer[i];
                    
                    _tableUsageTimeMap.Remove(guid);
                    
                    if (_tableMap.Remove(guid, out var table) && table is IDisposable disposable) {
                        disposable.Dispose();
                    }

                    if (_tableStorageHandlesMap.Remove(guid, out var handle)) {
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
        
        private static Locale GetSystemLocale() {
            var id = LocaleExtensions.SystemLanguageToLocaleId(Application.systemLanguage);
            if (LocaleExtensions.TryGetLocaleById(id, out var locale)) return locale;

            try
            {
                var culture = CultureInfo.CurrentUICulture;
                return LocaleExtensions.TryGetLocale(culture.TwoLetterISOLanguageName, out locale) 
                    ? locale 
                    : LocaleExtensions.DefaultLocale;
            }
            catch
            {
                return LocaleExtensions.DefaultLocale;
            }
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