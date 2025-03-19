using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Labels;
using MisterGames.Common.Localization;

namespace MisterGames.ActionLib.Localization {
    
    [Serializable]
    public sealed class LocalizationCondition : IActorCondition {

        [LabelFilter("LocalizationLib")]
        public LabelValue localization;
        
        public bool IsMatch(IActor context, float startTime) {
            return LocalizationService.Instance is { } service && 
                   service.LocalizationId == localization.GetValue();
        }
    }
    
}