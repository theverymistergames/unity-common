using System.IO;
using UnityEngine;

namespace MisterGames.Common.Save {

    [CreateAssetMenu(fileName = nameof(SaveSystemSettings), menuName = "MisterGames/Save/" + nameof(SaveSystemSettings))]
    public sealed class SaveSystemSettings : ScriptableObject {

        [Header("Save Settings")]
        [Min(0)] public int bufferSize = 4096;
        
        [Header("Path")]
        public string folder;
        public string fileName;
        public string fileFormat;

        private const string ProfilePrefix = "profile";
        private const string PersistentPrefix = "persistent";
        
        public string GetFolderPath() {
            return Path.Combine(Application.persistentDataPath, folder);
        }
        
        public string GetFilePath(string saveId = null) {
            string f = string.IsNullOrWhiteSpace(saveId) 
                ? $"{ProfilePrefix}_{fileName}.{fileFormat}" 
                : $"{ProfilePrefix}_{fileName}_{saveId}.{fileFormat}";
            
            return Path.Combine(GetFolderPath(), f);
        }
        
        public string GetFilePathPersistent() {
            string f = $"{PersistentPrefix}_{fileName}.{fileFormat}";
            
            return Path.Combine(GetFolderPath(), f);
        }

        public string GetSaveId(string fileName) {
            fileName = fileName.Substring(this.fileName.Length, fileName.Length - this.fileName.Length);
            if (fileName[0] == '_') fileName = fileName.Remove(0, 1);
            return fileName;
        }
    }
    
}