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

        public string GetFolderPath() {
            return Path.Combine(Application.persistentDataPath, folder);
        }
        
        public string GetFilePath(string fileId) {
            return Path.Combine(GetFolderPath(), $"{fileName}_{fileId}.{fileFormat}");
        }

        public string GetFileId(string fileName) {
            fileName = fileName.Substring(this.fileName.Length, fileName.Length - this.fileName.Length);
            if (fileName[0] == '_') fileName = fileName.Remove(0, 1);
            return fileName;
        }
    }
    
}