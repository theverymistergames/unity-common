using System.IO;
using UnityEngine;

namespace MisterGames.Common.Save {

    [CreateAssetMenu(fileName = nameof(SaveSystemSettings), menuName = "MisterGames/Save/" + nameof(SaveSystemSettings))]
    public sealed class SaveSystemSettings : ScriptableObject {

        public string folder;
        public string fileName;
        public string fileFormat;

        public string GetFolderPath() {
            return Path.Combine(Application.persistentDataPath, folder);
        }
        
        public string GetFilePath(string saveId = null) {
            string f = string.IsNullOrEmpty(saveId) ? $"{fileName}.{fileFormat}" : $"{fileName}_{saveId}.{fileFormat}";
            return Path.Combine(GetFolderPath(), f);
        }

        public string GetSaveId(string fileName) {
            fileName = fileName.Substring(this.fileName.Length, fileName.Length - this.fileName.Length);
            if (fileName[0] == '_') fileName = fileName.Remove(0, 1);
            return fileName;
        }
    }
    
}