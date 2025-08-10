using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;

namespace MisterGames.ActionLib.Localization {
    
    [Serializable]
    public sealed class LocalizationCondition : IActorCondition {

        public Locale locale;
        
        public bool IsMatch(IActor context, float startTime) {
            return Services.Get<ILocalizationService>() is { } service && service.Locale == locale;
        }
    }
    
}