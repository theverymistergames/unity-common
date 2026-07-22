using System.Collections.Generic;

namespace MisterGames.Common.Localization {
    
    public interface ILocalizedStringProvider {
        void GetValues(List<LocalizationKey> buffer);
    }
    
}