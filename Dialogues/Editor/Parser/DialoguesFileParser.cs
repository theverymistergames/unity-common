using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.GoogleSheets;
using MisterGames.Common.Files;
using MisterGames.Common.Localization;
using MisterGames.Common.Strings;
using MisterGames.Dialogues.Storage;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Dialogues.Editor.Parser {

    [CreateAssetMenu(fileName = nameof(DialoguesFileParser), menuName = "MisterGames/Dialogues/" + nameof(DialoguesFileParser))]
    public sealed class DialoguesFileParser : SheetParserBase {

        [Header("Search Dialogue Files")]
        [SerializeField] private string[] _searchDialoguesFilesInFolders = { "Dialogues" };
        [SerializeField] private SearchMode _searchMode;
        
        [Header("Storage Settings")]
        [SerializeField] private string _dialogueFilenamePrefix = "Dlg";
        [SerializeField] private string _locTableFilenamePrefix = "LocTable";
        [SerializeField] private string _localizationTablesFolderPath = "Localization/Tables";
        [SerializeField] private string _dialoguesFolderPath = "Dialogues/Storages";
        [SerializeField] private DivideFilesMode _divideLocalizationTablesMode = DivideFilesMode.OneFilePerDialogue;
        
        [Header("Create ScriptableObject Settings")]
        [SerializeField] [Range(1, 100)] private int _retryCreateStorageAttempts = 5;
        [SerializeField] [Min(0f)] private float _retryCreateStorageDelay = 0.5f;

        private enum SearchMode {
            AllFiles,
            FilesUpdatedLastMonth,
            FilesUpdatedLastWeek,
            FilesUpdatedLastDay,
            FilesUpdatedLastHour,
        }

        private enum DivideFilesMode {
            OneFile,
            OneFilePerDialogue,
        }
        
        private const string LocTableFilenamePrefixDefault = "LocTable_";
        private const int BufferSize = 4096;
        
        private CancellationTokenSource _cts;
        
        public override async UniTask DownloadAndParse(CancellationToken cancellationToken) {
            if (Application.isPlaying) {
                Debug.LogWarning($"Downloading and parsing dialogs is not allowed in playmode.");
                return;
            }
            
            AsyncExt.RecreateCts(ref _cts);
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token).Token;
            
            var localizationTableStorages = new Dictionary<string, LocalizationTableStorage>();
            var dialogueStorages = new Dictionary<string, DialogueTableStorage>();
            
            var localizationSettingsAssets = AssetDatabase
                .FindAssets($"a:assets t:{nameof(LocalizationSettings)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<LocalizationSettings>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();

            var dialogueFiles = GetAllDialoguesFilePaths(_searchMode, _searchDialoguesFilesInFolders);
            var tasks = new UniTask<int>[dialogueFiles.Count];
            
            LogInfo($"starting to parse {dialogueFiles.Count} dialogue files from folders {_searchDialoguesFilesInFolders.AsString()}...");
            
            for (int i = 0; i < dialogueFiles.Count; i++) {
                tasks[i] = ParseAndWriteDialoguesFromFile(
                    dialogueFiles[i],
                    localizationTableStorages,
                    dialogueStorages,
                    localizationSettingsAssets,
                    cancellationToken
                );
            }

            int[] result = await UniTask.WhenAll(tasks);
            if (cancellationToken.IsCancellationRequested) return;

            foreach (var storage in localizationTableStorages.Values) {
                EditorUtility.SetDirty(storage);
            }
            
            foreach (var storage in dialogueStorages.Values) {
                EditorUtility.SetDirty(storage);
            }

            LogInfo($"parsed total {result.Sum()} dialogs in {dialogueFiles.Count} files.");
        }

        protected override void Cancel() {
            AsyncExt.DisposeCts(ref _cts);
        }

        private static IReadOnlyList<string> GetAllDialoguesFilePaths(SearchMode searchMode, string[] folders) {
            var paths = new List<string>();
            var now = DateTime.Now;
            
            for (int i = 0; i < folders.Length; i++) {
                string folder = folders[i];
                string folderPath = $"Assets/{folder}";
                string[] files = Directory.GetFiles(folderPath, "*.json", SearchOption.AllDirectories);

                for (int j = 0; j < files.Length; j++) {
                    string filePath = files[j];

                    var timeSinceLastWrite = now - File.GetLastWriteTimeUtc(filePath);

                    bool canAdd = searchMode switch {
                        SearchMode.AllFiles => true,
                        SearchMode.FilesUpdatedLastMonth => timeSinceLastWrite.TotalDays <= 31,
                        SearchMode.FilesUpdatedLastWeek => timeSinceLastWrite.TotalDays <= 7,
                        SearchMode.FilesUpdatedLastDay => timeSinceLastWrite.TotalHours <= 24,
                        SearchMode.FilesUpdatedLastHour => timeSinceLastWrite.TotalMinutes <= 60,
                        _ => throw new ArgumentOutOfRangeException(nameof(searchMode), searchMode, null)
                    };
                    
                    if (canAdd) paths.Add(filePath);
                }
            }
            
            return paths;
        }

        private async UniTask<int> ParseAndWriteDialoguesFromFile(
            string filePath,
            Dictionary<string, LocalizationTableStorage> localizationTableStorages,
            Dictionary<string, DialogueTableStorage> dialogueStorages,
            LocalizationSettings[] localizationSettingsArray,
            CancellationToken cancellationToken) 
        {
            var result = await JsonExtensions.ReadJsonFromFile<DialogueFileDto>(filePath, BufferSize);

            if (cancellationToken.IsCancellationRequested) {
                return 0;
            }
            
            if (result.status == JsonExtensions.Status.Error) {
                LogInfo($"Error while parsing file {filePath}: {result.message}");
                return 0;
            }
            
            var fileDto = result.value;

            string locTableStorageFolderPath = $"Assets/{_localizationTablesFolderPath}";
            string dialogueStorageFolderPath = $"Assets/{_dialoguesFolderPath}";
            
            var localizationTableStorage = GetOrCreateLocalizationTableStorage(
                locTableStorageFolderPath,
                fileDto.id,
                localizationTableStorages
            );
                
            var dialogueStorage = GetOrCreateDialogueStorage(
                dialogueStorageFolderPath,
                fileDto.id,
                dialogueStorages
            );

            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(localizationTableStorage));

            if (!DialogueDtoExtensions.ParseAndWrite(fileDto, new Guid(guid), localizationTableStorage, dialogueStorage, localizationSettingsArray)) {
                LogError($"Error while parsing [{filePath}]: dialogue id is not set properly.");
                return 0;
            }
            
            LogInfo($"File {filePath}: parsed dialog {fileDto.id}.");
            return 1;
        }

        private LocalizationTableStorage GetOrCreateLocalizationTableStorage(
            string folderPath,
            string dialogueId,
            Dictionary<string, LocalizationTableStorage> storages) 
        {
            string filename = _divideLocalizationTablesMode switch {
                DivideFilesMode.OneFile => GetSingleFileName(_locTableFilenamePrefix, LocTableFilenamePrefixDefault),
                DivideFilesMode.OneFilePerDialogue => GetSingleDialogueFilename(dialogueId, _locTableFilenamePrefix),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            var storage = GetOrCreateScriptableObject(folderPath, filename, storages, out bool wasAlreadyAdded);
            
            if (!wasAlreadyAdded) storage.ClearAll();
            
            return storage;
        }

        private DialogueTableStorage GetOrCreateDialogueStorage(
            string folderPath,
            string dialogueId,
            Dictionary<string, DialogueTableStorage> storages) 
        {
            string filename = GetSingleDialogueFilename(dialogueId, _dialogueFilenamePrefix);
            var storage = GetOrCreateScriptableObject(folderPath, filename, storages, out bool wasAlreadyAdded);
            
            if (!wasAlreadyAdded) storage.ClearAll();
            
            return storage;
        }
        
        private T GetOrCreateScriptableObject<T>(
            string folderPath,
            string fileName,
            Dictionary<string, T> storages, 
            out bool wasAlreadyAdded) 
            where T : ScriptableObject
        {
            if (storages.TryGetValue(fileName, out var storage)) {
                wasAlreadyAdded = true;
                return storage;
            }
            
            storage = AssetDatabase.LoadAssetAtPath<T>(Path.Combine(folderPath, $"{fileName}.asset"));
            
            if (storage == null) {
                storage = CreateInstance<T>();
                storage.hideFlags = HideFlags.DontUnloadUnusedAsset;

                SaveAsset(storage, folderPath, fileName).Forget();
            }
            
            storages[Path.Combine(folderPath, fileName)] = storage;

            wasAlreadyAdded = false;
            return storage;
        }
        
        private static string GetSingleFileName(string prefix, string defaultPrefix = null) {
            return string.IsNullOrWhiteSpace(prefix) ? $"{defaultPrefix}main" : $"{prefix}_main";
        }
        
        private static string GetSingleDialogueFilename(string dialogueId, string prefix) {
            return string.IsNullOrWhiteSpace(prefix) ? $"{dialogueId}" : $"{prefix}_{dialogueId}";
        }

        private async UniTask SaveAsset(Object instance, string folderPath, string fileName) {
            folderPath = Path.Combine(folderPath);
            
            if (!AssetDatabase.IsValidFolder(folderPath)) {
                string[] parts = folderPath.Split('/', '\\');
                for (int i = 1; i < parts.Length; i++) {
                    AssetDatabase.CreateFolder(Path.Combine(parts[..i]), parts[i]);
                }
            }
            
            string path = Path.Combine(folderPath, $"{fileName}.asset");
            
            try {
                AssetDatabase.CreateAsset(instance, path);

                LogInfo($"created new {nameof(LocalizationTableStorage)} instance at [{path}].");
            }
            catch (Exception) {
                LogWarning($"failed to create {nameof(LocalizationTableStorage)} instance at [{path}] at first attempt. " +
                           $"Asset will be created after delay.");

                int attempts = 0;
                while (attempts++ < _retryCreateStorageAttempts) {
                    try {
                        AssetDatabase.CreateAsset(instance, path);

                        LogInfo($"created new {nameof(LocalizationTableStorage)} instance at [{path}].");
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

        private static void LogInfo(string message) {
            Debug.Log($"{nameof(DialoguesFileParser).FormatColorOnlyForEditor(Color.white)}: {message}");
        }
        
        private static void LogWarning(string message) {
            Debug.LogWarning($"{nameof(DialoguesFileParser).FormatColorOnlyForEditor(Color.white)}: {message}");
        }
        
        private static void LogError(string message) {
            Debug.LogError($"{nameof(DialoguesFileParser).FormatColorOnlyForEditor(Color.white)}: {message}");
        }
    }
    
}