using System.Collections.Generic;

namespace MisterGames.Common.Editor.GoogleSheets {
    
    public interface IGoogleSheetParser {

        void Parse(IReadOnlyList<SheetTable> sheetTables);
        
    }
    
}