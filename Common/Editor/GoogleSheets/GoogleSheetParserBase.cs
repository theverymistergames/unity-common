using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.GoogleSheets {
    
    public abstract class GoogleSheetParserBase : ScriptableObject, IGoogleSheetParser {
        
        [SerializeField] private GoogleSheetImporter _googleSheetImporter;
        [SerializeField] private string[] _sheetIds;
        
        [Button]
        public void DownloadAndParse() {
            _googleSheetImporter.DownloadAndParse(_sheetIds, this).Forget();
        }

        public abstract void Parse(IReadOnlyList<SheetTable> sheetTables);

        private void Reset() {
            _googleSheetImporter = AssetDatabase
                .FindAssets($"a:assets t:{nameof(GoogleSheetImporter)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<GoogleSheetImporter>(AssetDatabase.GUIDToAssetPath(guid)))
                .FirstOrDefault();
        }
    }
    
}