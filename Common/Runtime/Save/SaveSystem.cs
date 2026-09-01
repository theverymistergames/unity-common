using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Files;
using MisterGames.Common.Lists;
using MisterGames.Common.Save.Storages;
using MisterGames.Common.Save.Tables;
using MisterGames.Common.Strings;
using UnityEngine;

namespace MisterGames.Common.Save {
    
    public sealed class SaveSystem : ISaveSystem, IDisposable {
        
        public static readonly ISaveSystem Main = new SaveSystem();

        private readonly Dictionary<string, SaveStorage<SaveKey>> _saveStorageMap = new();
        private readonly HashSet<ISaveable> _saveableSet = new();

        private readonly Dictionary<string, UniTask> _fileOperations = new();
        private readonly Dictionary<string, int> _fileOperationIds = new();
        private readonly Dictionary<string, int> _fileDeleteVersions = new();
        private readonly Dictionary<string, object> _fileLocks = new();

        private SaveSystemSettings _saveSystemSettings;
        private bool _disposed;

        public void Initialize(SaveSystemSettings saveSystemSettings) {
            _disposed = false;
            _saveSystemSettings = saveSystemSettings;
        }

        public void Dispose() {
            if (_disposed) return;

            foreach (var storage in _saveStorageMap.Values) {
                storage.Clear();
            }

            _saveableSet.Clear();
            _saveStorageMap.Clear();

            WaitForStartedFileOperations();

            _fileOperations.Clear();
            _fileOperationIds.Clear();
            _fileDeleteVersions.Clear();
            _fileLocks.Clear();

            _disposed = true;
        }

        private void WaitForStartedFileOperations() {
            foreach (object fileLock in _fileLocks.Values) {
                lock (fileLock) { }
            }
        }

        private object GetFileLock(string storageId) {
            if (!_fileLocks.TryGetValue(storageId, out object fileLock)) {
                fileLock = new object();
                _fileLocks[storageId] = fileLock;
            }

            return fileLock;
        }

        private UniTask EnqueueFileOperation(string storageId, Func<UniTask> operation) {
            int id = _fileOperationIds.GetValueOrDefault(storageId) + 1;
            _fileOperationIds[storageId] = id;

            var task = ProcessFileOperationAsync(storageId, id, _fileOperations.GetValueOrDefault(storageId), operation)
                .Preserve(); 
            
            if (task.Status == UniTaskStatus.Pending) _fileOperations[storageId] = task;

            return task;
        }

        private async UniTask ProcessFileOperationAsync(string storageId, int id, UniTask previous, Func<UniTask> operation) {
            try {
                await previous;
            }
            catch (Exception) {
                // Result of the previous operation is observed by the code that requested it.
            }

            try {
                await operation.Invoke();
            }
            finally {
                if (_fileOperationIds.GetValueOrDefault(storageId) == id) {
                    _fileOperations.Remove(storageId);
                    _fileOperationIds.Remove(storageId);
                }
            }
        }
        
        public void Register(ISaveable saveable, bool notifyLoad = true) {
            _saveableSet.Add(saveable);
            if (notifyLoad) saveable.OnLoadData(this);
        }

        public void Unregister(ISaveable saveable) {
            _saveableSet.Remove(saveable);
        }

        public T Get<T>(string storageId, string dataId, int index) {
            return TryGet<T>(storageId, dataId, index, out var data) ? data : default;
        }

        public bool TryGet<T>(string storageId, string dataId, int index, out T data) {
            data = default;
            
            return GetStorage(storageId)
                ?.GetTable<T>()
                ?.TryGetData(new SaveKey(dataId, index), out data) ?? false;
        }

        public bool Set<T>(string storageId, string dataId, int index, T data) {
            return GetOrCreateStorage(storageId)?.GetOrCreateTable<T>()?.SetData(new SaveKey(dataId, index), data) ?? false;
        }

        public bool Remove<T>(string storageId, string dataId, int index) {
            return GetStorage(storageId)?.GetTable<T>()?.RemoveData(new SaveKey(dataId, index)) ?? false;
        }
        
        public IReadOnlyList<StorageData> GetStorageFiles() {
            string path = _saveSystemSettings.GetFolderPath();
            if (!Directory.Exists(path)) return Array.Empty<StorageData>();
            
            string[] files = Directory.GetFiles(path);
            string fileNameTemplate = _saveSystemSettings.fileName;
            string fileFormat = _saveSystemSettings.fileFormat;
            
            var saves = new List<StorageData>();
            
            for (int i = 0; i < files.Length; i++) {
                string file = files[i];
                string fileName = Path.GetFileNameWithoutExtension(file);

                if (!fileName.Contains(fileNameTemplate) || !Path.GetExtension(file).Contains(fileFormat)) continue;
                
                saves.Add(new StorageData(_saveSystemSettings.GetFileId(fileName), File.GetLastWriteTime(file)));
            }

            return saves;
        }

        public ISaveStorage<SaveKey> GetStorage(string storageId) {
            return _saveStorageMap.GetValueOrDefault(storageId);
        }

        private ISaveStorage<SaveKey> GetOrCreateStorage(string storageId) {
            if (_saveStorageMap.TryGetValue(storageId, out var storage)) return storage;
            
            storage = new SaveStorage<SaveKey>();
            _saveStorageMap[storageId] = storage;
            
            return storage;
        }

        public UniTask SaveIntoFile(string storageId) {
            return _saveStorageMap.TryGetValue(storageId, out var storage) 
                ? SaveStorageAsync(storageId, storage) 
                : default;
        }

        public UniTask SaveIntoFile(string storageId, ISaveStorage source) {
            return source == null ? default : SaveStorageAsync(storageId, source);
        }

        public UniTask LoadFromFile(string storageId) {
            var storage = GetOrCreateStorage(storageId);
            storage.Clear();
            
            return LoadStorageAsync(storageId, storage);
        }

        public UniTask LoadFromFile(string storageId, ISaveStorage dest) {
            return dest == null ? default : LoadStorageAsync(storageId, dest);
        }
        
        public void SaveAllFiles() {
            SaveAllStoragesAsync().Forget();
        }

        public void LoadAllFiles() {
            LoadAllStoragesAsync().Forget();
        }

        private async UniTask SaveAllStoragesAsync() {
            int count = _saveStorageMap.Count;
            var tasks = ArrayPool<UniTask>.Shared.Rent(count);
            tasks.ResetArrayElements();

            try {
                int index = 0;
                foreach ((string storageId, var storage) in _saveStorageMap) {
                    tasks[index++] = SaveStorageAsync(storageId, storage);
                }

                await UniTask.WhenAll(tasks);
            }
            finally {
                tasks.ResetArrayElements();
                ArrayPool<UniTask>.Shared.Return(tasks);
            }
        }
        
        private UniTask SaveStorageAsync(string storageId, ISaveStorage source) {
            NotifySaveAll();

            Directory.CreateDirectory(_saveSystemSettings.GetFolderPath());

            string json = JsonExtensions.SerializeJson(new SaveStorageDto(source.Tables));
            string filePath = _saveSystemSettings.GetFilePath(storageId);
            int bufferSize = _saveSystemSettings.bufferSize;
            int deleteVersion = _fileDeleteVersions.GetValueOrDefault(storageId);
            object fileLock = GetFileLock(storageId);

            return EnqueueFileOperation(storageId, () => WriteStorageAsync(storageId, deleteVersion, json, filePath, bufferSize, fileLock));
        }

        private async UniTask WriteStorageAsync(
            string storageId,
            int deleteVersion,
            string json,
            string filePath,
            int bufferSize,
            object fileLock)
        {
            if (_fileDeleteVersions.GetValueOrDefault(storageId) != deleteVersion) return;

            var result = await SaveFileAsync(json, filePath, bufferSize, fileLock);

            switch (result.status) {
                case JsonExtensions.Status.Success:
                    break;
                
                case JsonExtensions.Status.Error:
                    LogError($"could not save storage [{storageId}]: {result.message}");
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            NotifyAfterSaveAll();
        }
        
        private async UniTask LoadAllStoragesAsync() {
            var storageFiles = GetStorageFiles();
            int count = storageFiles.Count;
            var tasks = ArrayPool<UniTask>.Shared.Rent(count);
            tasks.ResetArrayElements();

            try {
                for (int i = 0; i < count; i++) {
                    string storageId = storageFiles[i].storageId;

                    var storage = GetOrCreateStorage(storageId);
                    storage.Clear();

                    tasks[i] = LoadStorageAsync(storageId, storage);
                }

                await UniTask.WhenAll(tasks);
            }
            finally {
                tasks.ResetArrayElements();
                ArrayPool<UniTask>.Shared.Return(tasks);
            }
        }

        private UniTask LoadStorageAsync(string storageId, ISaveStorage storage) {
            string filePath = _saveSystemSettings.GetFilePath(storageId);
            int bufferSize = _saveSystemSettings.bufferSize;
            object fileLock = GetFileLock(storageId);

            return EnqueueFileOperation(storageId, () => ReadStorageAsync(storageId, storage, filePath, bufferSize, fileLock));
        }

        private async UniTask ReadStorageAsync(string storageId, ISaveStorage storage, string filePath, int bufferSize, object fileLock) {
            var result = await LoadFileAsync(filePath, bufferSize, fileLock);

            switch (result.status) {
                case JsonExtensions.Status.Success:
                    if (result.value?.Tables is { } tables) {
                        foreach (var (type, table) in tables) {
                            storage.SetTable(type, table);
                        }
                    }
                    
                    NotifyLoadAll();
                    NotifyAfterLoadAll();
                    
                    break;
                
                case JsonExtensions.Status.Error:
                    LogWarning($"could not load storage [{storageId}]: {result.message}");
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private static UniTask<JsonExtensions.Result> SaveFileAsync(string json, string filePath, int bufferSize, object fileLock) {
            return JsonExtensions.WriteJsonIntoFile(json, filePath, bufferSize, fileLock);
        }

        private static UniTask<JsonExtensions.Result<SaveStorageDto>> LoadFileAsync(string filePath, int bufferSize, object fileLock) {
            return JsonExtensions.ReadJsonFromFile<SaveStorageDto>(filePath, bufferSize, fileLock);
        }
        
        public void DeleteFile(string storageId) {
            GetStorage(storageId)?.Clear();
            DeleteFileInternal(storageId);
        }

        public void DeleteAllFiles() {
            var storageFiles = GetStorageFiles();

            for (int i = 0; i < storageFiles.Count; i++) {
                DeleteFileInternal(storageFiles[i].storageId);
            }
        }

        private void DeleteFileInternal(string storageId) {
            _fileDeleteVersions[storageId] = _fileDeleteVersions.GetValueOrDefault(storageId) + 1;

            var result = JsonExtensions.DeleteFile(_saveSystemSettings.GetFilePath(storageId), GetFileLock(storageId));

            if (result.status == JsonExtensions.Status.Error) {
                LogError($"could not delete storage [{storageId}]: {result.message}");
            }
        }
        
        private void NotifyLoadAll() {
            foreach (var saveable in _saveableSet) {
                saveable.OnLoadData(this);
            }
        }

        private void NotifySaveAll() {
            foreach (var saveable in _saveableSet) {
                saveable.OnSaveData(this);
            }
        }

        private void NotifyAfterLoadAll() {
            foreach (var saveable in _saveableSet) {
                saveable.OnAfterLoadData(this);
            }
        }
        
        private void NotifyAfterSaveAll() {
            foreach (var saveable in _saveableSet) {
                saveable.OnAfterSaveData(this);
            }
        }
        
        [HideInCallstack]
        private static void Log(string message) {
            Debug.Log($"{nameof(SaveSystem).FormatColorOnlyForEditor(Color.white)}: f {Time.frameCount}, {message}");
        }
        
        [HideInCallstack]
        private static void LogWarning(string message) {
            Debug.LogWarning($"{nameof(SaveSystem).FormatColorOnlyForEditor(Color.white)}: f {Time.frameCount}, {message}");
        }
        
        [HideInCallstack]
        private static void LogError(string message) {
            Debug.LogError($"{nameof(SaveSystem).FormatColorOnlyForEditor(Color.white)}: f {Time.frameCount}, {message}");
        }
    }
    
}