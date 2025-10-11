using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Files;
using MisterGames.Common.Lists;
using MisterGames.Common.Maths;
using MisterGames.Common.Save.Tables;
using UnityEngine;

namespace MisterGames.Common.Save {
    
    public sealed class SaveSystem : ISaveSystem, IDisposable {
        
        public static readonly ISaveSystem Main = new SaveSystem();
        
        private readonly HashSet<ISaveable> _saveableSet = new();
        private readonly ISaveTableFactory _tablesCurrentSave = new SaveTableFactory();
        private readonly ISaveTableFactory _tablesPersistent = new SaveTableFactory();
        
        private readonly List<SaveMeta> _saveMetas = new();
        
        private SaveSystemSettings _saveSystemSettings;
        private string _activeSaveId;
        private bool _disposed;

        public void Initialize(SaveSystemSettings saveSystemSettings) {
            _disposed = false;
            _saveSystemSettings = saveSystemSettings;
            
            _tablesCurrentSave.Prewarm();
            _tablesPersistent.Prewarm();
        }

        public void Dispose() {
            if (_disposed) return;
            
            _saveableSet.Clear();
            _tablesCurrentSave.Clear();
            _tablesPersistent.Clear();
            _saveMetas.Clear();
            
            _disposed = true;
        }
        
        public void Register(ISaveable saveable, bool notifyLoad = true) {
            _saveableSet.Add(saveable);
            if (notifyLoad) saveable.OnLoadData(this);
        }

        public void Unregister(ISaveable saveable) {
            _saveableSet.Remove(saveable);
        }

        public T Get<T>(SaveStorage storage, string id, int index) {
            return TryGet<T>(storage, id, index, out var data) ? data : default;
        }

        public bool TryGet<T>(SaveStorage storage, string id, int index, out T data) {
            data = default;
            
            return GetStorage(storage)
                .Get<T>()
                ?.TryGetData(NumberExtensions.TwoIntsAsLong(Animator.StringToHash(id), index), out data) ?? false;
        }

        public void Set<T>(SaveStorage storage, string id, int index, T data) {
            var table = GetStorage(storage).GetOrCreate<T>();
            long key = NumberExtensions.TwoIntsAsLong(Animator.StringToHash(id), index);
            
            table?.PrepareRecord(id, index);
            table?.SetData(key, data);
        }

        public SaveBuilder Pop<T>(SaveStorage storage, string id, T def, out T data) {
            return new SaveBuilder(this, storage, id).Pop(def, out data);
        }

        public SaveBuilder Pop<T>(SaveStorage storage, string id, out T data) {
            return new SaveBuilder(this, storage, id).Pop(out data);
        }

        public SaveBuilder Push<T>(SaveStorage storage, string id, T data) {
            return new SaveBuilder(this, storage, id).Push(data);
        }

        public string GetActiveSave() {
            return _activeSaveId;
        }

        public string GetLastWrittenSave() {
            string dir = _saveSystemSettings.GetFolderPath();
            if (!Directory.Exists(dir)) return null;
            
            string[] files = Directory.GetFiles(dir);
            string fileNameTemplate = _saveSystemSettings.fileName;
            string fileFormat = _saveSystemSettings.fileFormat;
            string result = null;
            var lastTime = DateTime.MinValue;
            
            for (int i = 0; i < files.Length; i++) {
                string file = files[i];
                string fileName = Path.GetFileNameWithoutExtension(file);
                
                if (!fileName.Contains(fileNameTemplate) || !Path.GetExtension(file).Contains(fileFormat)) continue;

                var writeTime = File.GetLastWriteTime(file);
                if (writeTime <= lastTime) continue;

                result = _saveSystemSettings.GetSaveId(fileName);
                lastTime = writeTime;
            }

            return result;
        }

        public IReadOnlyList<SaveMeta> GetSaves() {
            string path = _saveSystemSettings.GetFolderPath();
            if (!Directory.Exists(path)) return Array.Empty<SaveMeta>();
            
            _saveMetas.Clear();
            
            string[] files = Directory.GetFiles(path);
            string fileNameTemplate = _saveSystemSettings.fileName;
            string fileFormat = _saveSystemSettings.fileFormat;
            
            for (int i = 0; i < files.Length; i++) {
                string file = files[i];
                string fileName = Path.GetFileNameWithoutExtension(file);

                if (!fileName.Contains(fileNameTemplate) || !Path.GetExtension(file).Contains(fileFormat)) continue;
                
                _saveMetas.Add(new SaveMeta(_saveSystemSettings.GetSaveId(fileName), File.GetLastWriteTime(file)));
            }

            return _saveMetas;
        }

        public void Save(string saveId = null) {
            SaveAllAsync(saveId).Forget();
        }
        
        private async UniTask SaveAllAsync(string saveId) {
            saveId ??= _activeSaveId ?? _saveSystemSettings.fileName;
            _activeSaveId = saveId;
            
            NotifySaveAll();
            
            Directory.CreateDirectory(_saveSystemSettings.GetFolderPath());

            var tasks = ArrayPool<UniTask>.Shared.Rent(2);

            tasks[0] = SaveStorageAsync(_tablesCurrentSave, _saveSystemSettings.GetFilePath(saveId), _saveSystemSettings.bufferSize);
            tasks[1] = SaveStorageAsync(_tablesPersistent, _saveSystemSettings.GetFilePathPersistent(), _saveSystemSettings.bufferSize);
            
            await UniTask.WhenAll(tasks);
            
            tasks.ResetArrayElements();
            ArrayPool<UniTask>.Shared.Return(tasks);
            
            NotifyAfterSaveAll();
        }

        private static UniTask SaveStorageAsync(ISaveTableFactory storage, string filePath, int bufferSize) {
            var saveFileDto = new SaveFileDto { tables = new List<ISaveTable>(storage.Tables) };
            return JsonExtensions.WriteJsonIntoFile(saveFileDto, filePath, bufferSize);
        }

        public bool TryLoad(string saveId = null) {
            saveId ??= _activeSaveId ?? _saveSystemSettings.fileName;
            
            _tablesCurrentSave.Clear();

            bool loadedSave = TryLoad(_tablesCurrentSave, _saveSystemSettings.GetFilePath(saveId));
            bool loadedPersistent = TryLoad(_tablesPersistent, _saveSystemSettings.GetFilePathPersistent());

            if (loadedSave) _activeSaveId = saveId;
            
            NotifyLoadAll();
            NotifyAfterLoadAll();
            
            return loadedSave;
        }

        private static bool TryLoad(ISaveTableFactory storage, string filePath) {
            storage.Clear();

            if (!File.Exists(filePath)) return false;
            
            using var fs = new FileStream(filePath, FileMode.Open);
            using var sr = new StreamReader(fs);

            try {
                var tables = JsonUtility.FromJson<SaveFileDto>(sr.ReadToEnd()).tables;
                
                for (int i = 0; i < tables.Count; i++) {
                    var table = tables[i];
                    storage.Set(table.GetElementType(), table);
                }
            }
            catch (IOException e) {
                Console.WriteLine(e);
                return false;
            }
            finally {
                sr.Close();
                fs.Close();
            }
            
            return true;
        }
        
        public void DeleteSave(string saveId) {
            string path = _saveSystemSettings.GetFolderPath();
            if (!Directory.Exists(path)) return;
            
            _saveMetas.Clear();
            
            string[] files = Directory.GetFiles(path);
            string fileNameTemplate = _saveSystemSettings.fileName;
            string fileFormat = _saveSystemSettings.fileFormat;
            
            for (int i = 0; i < files.Length; i++) {
                string file = files[i];
                string fileName = Path.GetFileNameWithoutExtension(file);
                
                if (!fileName.Contains(fileNameTemplate) ||
                    !fileName.Contains(saveId) ||
                    !Path.GetExtension(file).Contains(fileFormat)
                ) {
                    continue;
                }
                
                File.Delete(file);
            }
        }

        public void DeleteAllSaves() {
            string path = _saveSystemSettings.GetFolderPath();
            if (!Directory.Exists(path)) return;
            
            _saveMetas.Clear();
            
            string[] files = Directory.GetFiles(path);
            string fileNameTemplate = _saveSystemSettings.fileName;
            string fileFormat = _saveSystemSettings.fileFormat;
            
            for (int i = 0; i < files.Length; i++) {
                string file = files[i];

                if (!Path.GetFileNameWithoutExtension(file).Contains(fileNameTemplate) ||
                    !Path.GetExtension(file).Contains(fileFormat)
                ) {
                    continue;
                }
                
                File.Delete(file);
            }
        }

        private ISaveTableFactory GetStorage(SaveStorage storage) {
            return storage switch {
                SaveStorage.CurrentSave => _tablesCurrentSave,
                SaveStorage.Persistent => _tablesPersistent,
                _ => throw new ArgumentOutOfRangeException(nameof(storage), storage, null)
            };
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
    }
    
}