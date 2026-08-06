using MisterGames.Common.Stats;

namespace MisterGames.Common.Audio {
    
    public interface IAudioMixerService {
        void SetModifier(object source, string parameter, ValueModifier modifier);
        void RemoveModifier(object source, string parameter);
        float GetFloat(string parameter);
    }
}