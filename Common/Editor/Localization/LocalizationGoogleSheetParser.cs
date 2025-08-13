using System.Collections.Generic;
using MisterGames.Common.Editor.GoogleSheets;
using UnityEngine;

namespace MisterGames.Common.Editor.Localization {

    [CreateAssetMenu(fileName = nameof(LocalizationGoogleSheetParser), menuName = "MisterGames/Localization/" + nameof(LocalizationGoogleSheetParser))]
    public sealed class LocalizationGoogleSheetParser : GoogleSheetParserBase {
        
        public override void Parse(IReadOnlyList<SheetTable> sheetTables) {
            foreach (var sheetTable in sheetTables) {
                Debug.Log($"{sheetTable}");
            }
        }
    }
    
}