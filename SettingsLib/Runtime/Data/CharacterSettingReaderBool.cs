using System;
using MisterGames.Character.Core;
using MisterGames.Common.Labels;
using MisterGames.Common.Service;

namespace MisterGames.SettingsLib.Descs {
    
    [Serializable]
    public class CharacterSettingReaderBool : ISettingReaderBool {

        public LabelValue settingId;
        
        public void OnReadValue(bool value) {
            Services.Get<ICharacterSettings>().Set(settingId.GetValue(), value);
        }
    }
    
}