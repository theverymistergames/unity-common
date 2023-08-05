using MisterGames.Character.Core;

namespace MisterGames.Character.Breath {

    public delegate void BreathCallback(float duration, float amplitude);

    public interface ICharacterBreathPipeline : ICharacterPipeline {
        event BreathCallback OnInhale;
        event BreathCallback OnExhale;

        float Period { get; set; }
        float Amplitude { get; set; }
    }

}
