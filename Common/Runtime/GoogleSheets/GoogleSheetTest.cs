using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Strings;
using UnityEngine;

namespace MisterGames.Common.GoogleSheets {
    
    public class GoogleSheetTest : MonoBehaviour {

        public string sheetId;
        public TextAsset credentials;
        
        [Button]
        private async void Download() {
            Debug.Log($"GoogleSheetTest.Download: f {Time.frameCount}, downloading sheet: {sheetId}");
            
            var importer = new GoogleSheetImporter(credentials.text);
            
            var titles = await importer.GetTitles(sheetId);
            
            switch (titles.resultType) {
                case GoogleSheetImporter.ResultType.Success:
                    Debug.Log($"GoogleSheetTest.Download: f {Time.frameCount}, successfully downloaded titles: {titles.data.AsString()}");
                    break;
                
                case GoogleSheetImporter.ResultType.Error:
                    Debug.LogWarning($"GoogleSheetTest.Download: f {Time.frameCount}, error downloading titles: {titles.message}");
                    return;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            for (int i = 0; i < titles.data.Count; i++) {
                string sheetName = titles.data[i];
                var sheet = await importer.DownloadSheet(sheetId, sheetName);

                switch (sheet.resultType) {
                    case GoogleSheetImporter.ResultType.Success:
                        Debug.Log($"GoogleSheetTest.Download: f {Time.frameCount}, successfully downloaded sheet {sheetName}:\n {sheet.data}");
                        break;
                    
                    case GoogleSheetImporter.ResultType.Error: 
                        Debug.Log($"GoogleSheetTest.Download: f {Time.frameCount}, error downloading sheet {sheetName}: {sheet.message}");
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
    
}