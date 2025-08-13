using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Editor.GoogleSheets;
using MisterGames.Common.Localization;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Localization {

    [CreateAssetMenu(fileName = nameof(LocalizationGoogleSheetParser), menuName = "MisterGames/Localization/" + nameof(LocalizationGoogleSheetParser))]
    public sealed class LocalizationGoogleSheetParser : GoogleSheetParserBase {

        [Header("Storage Settings")]
        [SerializeField] private string _folderPath = "Localization";
        [SerializeField] private DivideFilesMode _divideFilesMode;
        
        [Header("Create ScriptableObject Settings")]
        [SerializeField] [Range(1, 100)] private int _retryCreateStorageAttempts = 5;
        [SerializeField] [Min(0f)] private float _retryCreateStorageDelay = 0.5f;

        private const string FileNamePrefix = "LocalizationTable";
        private static readonly string MainFileName = $"{FileNamePrefix}_main";
        
        private enum DivideFilesMode {
            OneFile,
            OneFileForEachSpreadsheet,
            OneFileForEachTable,
        }
        
        public override void Parse(IReadOnlyList<SheetTable> sheetTables) {
            var storages = new Dictionary<string, LocalizationTableStorage>();
            var localizationSettingsAssets = AssetDatabase
                .FindAssets($"a:assets t:{nameof(LocalizationSettings)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<LocalizationSettings>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();
            
            for (int i = 0; i < sheetTables.Count; i++) {
                var sheetTable = sheetTables[i];
                var storage = GetOrCreateStorage(sheetTable, storages);

                int columnCount = sheetTable.ColumnCount;
                int rowCount = sheetTable.RowCount;

                for (int r = 0; r < rowCount; r++) {
                    string row = sheetTable.GetRow(r);

                    for (int c = 0; c < columnCount; c++) {
                        string value = sheetTable.GetData(r, c);
                        string column = sheetTable.GetColumn(c);
                        
                        if (string.IsNullOrEmpty(value) || !TryGetLocale(column, localizationSettingsAssets, out var locale)) continue;
                        
                        storage.AddValue(row, value, locale);
                    }
                }
            }

            foreach (var storage in storages.Values) {
                EditorUtility.SetDirty(storage);
            }
        }

        private static bool TryGetLocale(string localeCode, IReadOnlyList<LocalizationSettings> settingsList, out Locale locale) {
            if (string.IsNullOrWhiteSpace(localeCode)) {
                locale = default;
                return false;
            }
            
            for (int i = 0; i < settingsList.Count; i++) {
                if (settingsList[i].TryGetLocale(localeCode, out locale)) return true;
            }
            
            int hash = Animator.StringToHash(LocaleExtensions.FormatLocaleCode(localeCode));
            
            if (LocaleExtensions.TryGetLocaleIdByHash(hash, out var id)) {
                return LocaleExtensions.TryGetLocaleById(id, out locale);
            }

            locale = default;
            return false;
        }

        private LocalizationTableStorage GetOrCreateStorage(SheetTable sheetTable, Dictionary<string, LocalizationTableStorage> storages) {
            return _divideFilesMode switch {
                DivideFilesMode.OneFile => GetOrCreateStorage(MainFileName, storages),
                DivideFilesMode.OneFileForEachSpreadsheet => GetOrCreateStorage($"{FileNamePrefix}_{sheetTable.SheetId}", storages),
                DivideFilesMode.OneFileForEachTable => GetOrCreateStorage($"{FileNamePrefix}_{sheetTable.Title}_{sheetTable.SheetId}", storages),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private LocalizationTableStorage GetOrCreateStorage(string fileName, Dictionary<string, LocalizationTableStorage> storages) {
            if (!storages.TryGetValue(fileName, out var storage)) {
                storage = CreateStorage(fileName);
                storage.ClearAll();
                storages[fileName] = storage;
            }
                
            return storage;
        }

        private LocalizationTableStorage CreateStorage(string fileName) {
            var instance = CreateInstance<LocalizationTableStorage>();
            instance.hideFlags = HideFlags.DontUnloadUnusedAsset;

            SaveAsset(instance, fileName).Forget();
            
            return instance;
        }

        private async UniTask SaveAsset(LocalizationTableStorage instance, string fileName) {
            string folderPath = $"Assets/{_folderPath}";
            
            if (!AssetDatabase.IsValidFolder(folderPath)) {
                string[] parts = folderPath.Split('/');
                for (int i = 1; i < parts.Length; i++) {
                    AssetDatabase.CreateFolder(Path.Combine(parts[..(i - 1)]), parts[i]);
                }
            }

            string path = GetAssetPath(fileName);
            
            try {
                AssetDatabase.CreateAsset(instance, path);
                AssetDatabase.SaveAssets();

                Debug.Log($"Created new {nameof(LocalizationTableStorage)} instance at [{path}]");
            }
            catch (Exception) {
                Debug.LogWarning($"Failed to create {nameof(LocalizationTableStorage)} instance at [{path}] at first attempt. " +
                                 $"Asset will be created after delay.");

                int attempts = 0;
                while (attempts++ < _retryCreateStorageAttempts) {
                    try {
                        AssetDatabase.CreateAsset(instance, path);
                        AssetDatabase.SaveAssets();

                        Debug.Log($"Created new {nameof(LocalizationTableStorage)} instance at [{path}]");
                        break;
                    }
                    catch (Exception) {
                        // ignored
                    }
                    
                    if (_retryCreateStorageDelay > 0f) {
                        await UniTask.Delay(TimeSpan.FromSeconds(_retryCreateStorageDelay));
                    }
                    else {
                        await UniTask.Yield();
                    }
                }
            }
        }
        
        private string GetAssetPath(string fileName) {
            return $"Assets/{_folderPath}/{fileName}.asset";
        }
    }
    
}