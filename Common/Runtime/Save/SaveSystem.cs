using System;
using System.Collections.Generic;
using System.IO;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Common.Save.Tables;
using UnityEngine;

namespace MisterGames.Common.Save {
    
    [DefaultExecutionOrder(-10000)]
    public sealed class SaveSystem : MonoBehaviour, ISaveSystem {

        [EmbeddedInspector]
        [SerializeField] private SaveSystemSettings _saveSystemSettings;
        
        public static ISaveSystem Instance { get; private set; }
        
        private readonly HashSet<ISaveable> _saveableSet = new();
        private readonly ISaveTableFactory _tables = new SaveTableFactory();
        
        private readonly List<SaveMeta> _saveMetas = new();
        private readonly SaveFileDto _saveFileDto = new SaveFileDto { tables = new List<ISaveTable>() };
        private string _activeSaveId;

        private void Awake() {
            Instance = this;
        }

        private void Start() {
            _tables.Prewarm();
        }

        private void OnDestroy() {
            Cleanup();
        }
        
        private void Cleanup() {
            _saveableSet.Clear();
            _tables.Clear();
            _saveFileDto.tables.Clear();
            _saveMetas.Clear();
        }

        public void Register(ISaveable saveable) {
            _saveableSet.Add(saveable);
            saveable.OnLoadData(this);
        }

        public void Unregister(ISaveable saveable) {
            _saveableSet.Remove(saveable);
        }

        public T Get<T>(string id, int index) {
            return TryGet<T>(id, index, out var data) ? data : default;
        }

        public bool TryGet<T>(string id, int index, out T data) {
            data = default;
            return _tables.Get<T>()
                ?.TryGetData(NumberExtensions.TwoIntsAsLong(Animator.StringToHash(id), index), out data) ?? false;
        }

        public void Set<T>(string id, int index, T data) {
            var repo = _tables.GetOrCreate<T>();
            long key = NumberExtensions.TwoIntsAsLong(Animator.StringToHash(id), index);
            
            repo?.PrepareRecord(id, index);
            repo?.SetData(key, data);
        }

        public SaveBuilder Pop<T>(string id, T def, out T data) {
            return new SaveBuilder(this, id).Pop(def, out data);
        }

        public SaveBuilder Pop<T>(string id, out T data) {
            return new SaveBuilder(this, id).Pop(out data);
        }

        public SaveBuilder Push<T>(string id, T data) {
            return new SaveBuilder(this, id).Push(data);
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
            saveId ??= _activeSaveId ?? _saveSystemSettings.fileName;
            _activeSaveId = saveId;
            
            NotifySaveAll();
            
            _saveFileDto.tables.Clear();
            _saveFileDto.tables.AddRange(_tables.Tables);
            
            Directory.CreateDirectory(_saveSystemSettings.GetFolderPath());
            
            using var fs = new FileStream(_saveSystemSettings.GetFilePath(saveId), FileMode.Create);
            using var sw = new StreamWriter(fs);

            try {
                sw.Write(JsonUtility.ToJson(_saveFileDto, prettyPrint: true));
            }
            catch (IOException e) {
                Console.WriteLine(e);
                throw;
            }
            finally {
                sw.Close();
                fs.Close();
            }
        }

        public bool TryLoad(string saveId = null) {
            saveId ??= _activeSaveId ?? _saveSystemSettings.fileName;
            
            _tables.Clear();
            _saveFileDto.tables.Clear();

            string path = _saveSystemSettings.GetFilePath(saveId);

            if (!File.Exists(path)) return false;
            
            using var fs = new FileStream(path, FileMode.Open);
            using var sr = new StreamReader(fs);

            try {
                var tables = JsonUtility.FromJson<SaveFileDto>(sr.ReadToEnd()).tables;
                
                for (int i = 0; i < tables.Count; i++) {
                    var table = tables[i];
                    _tables.Set(table.GetElementType(), table);
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

            _activeSaveId = saveId;
            NotifyLoadAll();
            
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
        
        
    }
    
}