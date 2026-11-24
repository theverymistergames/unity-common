namespace MisterGames.Common.Audio {
    
    public interface IAudioBankService {
        void Bind(object source, AudioBank bank);
        void Unbind(object source, AudioBank bank);
    }
    
}