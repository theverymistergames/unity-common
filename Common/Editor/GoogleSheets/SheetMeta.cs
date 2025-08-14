using System.Collections.Generic;

namespace MisterGames.Common.Editor.GoogleSheets {
    
    public readonly struct SheetMeta {

        public readonly string sheetTitle;
        public readonly IReadOnlyList<string> tables;
        
        public SheetMeta(string sheetTitle, IReadOnlyList<string> tables) {
            this.sheetTitle = sheetTitle;
            this.tables = tables;
        }
    }
    
}