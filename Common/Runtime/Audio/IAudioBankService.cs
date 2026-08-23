namespace MisterGames.Common.Audio {
    
    public interface IAudioBankService {
        void Bind(object source, AudioBankReference bank);
        void Unbind(object source, AudioBankReference bank);
    }
    
}