using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Files;
using MisterGames.Common.Lists;
using MisterGames.Common.Maths;
using MisterGames.Common.Save.Tables;
using MisterGames.Common.Strings;
using UnityEngine;

namespace MisterGames.Common.Save {
    
    public sealed class SaveSystem : ISaveSystem, IDisposable {
        
        public static readonly ISaveSystem Main = new SaveSystem();

        private readonly Dictionary<string, ISaveStorage> _saveStorageMap = new();
        private readonly HashSet<ISaveable> _saveableSet = new();
        
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
            
            _disposed = true;
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
                ?.Get<T>()
                ?.TryGetData(NumberExtensions.TwoIntsAsLong(Animator.StringToHash(dataId), index), out data) ?? false;
        }

        public void Set<T>(string storageId, string dataId, int index, T data) {
            long key = NumberExtensions.TwoIntsAsLong(Animator.StringToHash(dataId), index);
            GetOrCreateStorage(storageId)?.GetOrCreateTable<T>()?.SetData(key, data);
        }

        public SaveBuilder Pop<T>(string storageId, string dataId, T def, out T data) {
            return new SaveBuilder(this, storageId, dataId).Pop(def, out data);
        }

        public SaveBuilder Pop<T>(string storageId, string dataId, out T data) {
            return new SaveBuilder(this, storageId, dataId).Pop(out data);
        }

        public SaveBuilder Push<T>(string storageId, string dataId, T data) {
            return new SaveBuilder(this, storageId, dataId).Push(data);
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

        private ISaveStorage GetStorage(string storageId) {
            return _saveStorageMap.GetValueOrDefault(storageId);
        }

        private ISaveStorage GetOrCreateStorage(string storageId) {
            if (_saveStorageMap.TryGetValue(storageId, out var storage)) return storage;
            
            storage = new SaveStorage();
            storage.PrewarmTables();
            
            _saveStorageMap[storageId] = storage;
            
            return storage;
        }

        public void SaveIntoFile(string storageId) {
            SaveStorageAsync(storageId).Forget();
        }

        public void SaveAllFiles() {
            SaveAllStoragesAsync().Forget();
        }

        public void LoadFromFile(string storageId) {
            LoadStorageAsync(storageId).Forget();
        }

        public void LoadAllFiles() {
            LoadAllStoragesAsync().Forget();
        }

        private async UniTask SaveAllStoragesAsync() {
            int count = _saveStorageMap.Count;
            var tasks = ArrayPool<UniTask>.Shared.Rent(count);
            tasks.ResetArrayElements();
            
            foreach (string storageId in _saveStorageMap.Keys) {
                tasks[count++] = SaveStorageAsync(storageId);
            }
            
            await UniTask.WhenAll(tasks);
            
            ArrayPool<UniTask>.Shared.Return(tasks);
        }
        
        private async UniTask SaveStorageAsync(string storageId) {
            if (!_saveStorageMap.TryGetValue(storageId, out var storage)) return;
            
            NotifySaveAll();
            
            Directory.CreateDirectory(_saveSystemSettings.GetFolderPath());

            var result = await SaveFileAsync(
                new SaveFileDto { tables = new List<ISaveTable>(storage.Tables) },
                _saveSystemSettings.GetFilePath(storageId),
                _saveSystemSettings.bufferSize
            );

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

            for (int i = 0; i < storageFiles.Count; i++) {
                tasks[count++] = LoadStorageAsync(storageFiles[i].storageId);
            }

            await UniTask.WhenAll(tasks);
            
            ArrayPool<UniTask>.Shared.Return(tasks);
        }

        private async UniTask LoadStorageAsync(string storageId) {
            var storage = GetOrCreateStorage(storageId);
            storage.Clear();
            
            var result = await LoadFileAsync(
                _saveSystemSettings.GetFilePath(storageId),
                _saveSystemSettings.bufferSize
            );

            switch (result.status) {
                case JsonExtensions.Status.Success:
                    var tables = (IReadOnlyList<ISaveTable>) result.value.tables ?? Array.Empty<ISaveTable>();
                    for (int i = 0; i < tables.Count; i++) {
                        var table = tables[i];
                        storage.Set(table.GetElementType(), table);
                    }
                    
                    NotifyLoadAll();
                    NotifyAfterLoadAll();
                    
                    break;
                
                case JsonExtensions.Status.Error:
                    LogError($"could not load storage [{storageId}]: {result.message}");
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private static UniTask<JsonExtensions.Result> SaveFileAsync(SaveFileDto saveFileDto, string filePath, int bufferSize) {
            return JsonExtensions.WriteJsonIntoFile(saveFileDto, filePath, bufferSize);
        }
        
        private static UniTask<JsonExtensions.Result<SaveFileDto>> LoadFileAsync(string filePath, int bufferSize) {
            return JsonExtensions.ReadJsonFromFile<SaveFileDto>(filePath, bufferSize);
        }
        
        public void DeleteFile(string storageId) {
            File.Delete(_saveSystemSettings.GetFilePath(storageId));
        }

        public void DeleteAllFiles() {
            var storageFiles = GetStorageFiles();

            for (int i = 0; i < storageFiles.Count; i++) {
                var storageFile = storageFiles[i];
                string filePath = _saveSystemSettings.GetFilePath(storageFile.storageId);
                File.Delete(filePath);
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