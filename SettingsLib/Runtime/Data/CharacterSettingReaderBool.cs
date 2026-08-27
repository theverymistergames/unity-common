using System;
using MisterGames.Common.Labels;
using MisterGames.Common.Save;
using MisterGames.Common.Service;

namespace MisterGames.SettingsLib.Descs {
    
    [Serializable]
    public class CharacterSettingReaderBool : ISettingReaderBool {

        public LabelValue settingId;
        
        public void OnReadValue(bool value) {
            Services.Get<IGameplayRuntimeSettings>().Set(settingId.GetValue(), value);
        }
    }
    
}