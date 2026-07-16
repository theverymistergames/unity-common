namespace MisterGames.Common.Audio {
    
    public interface IReverbSettings {
        float RoomHf { get; }
        float RoomLf { get; }
        float DecayTime { get; }
        float DecayHfRatio { get; }
        float ReflectionsLevel { get; }
        float ReflectionsDelay { get; }
        float ReverbLevel { get; }
        float ReverbDelay { get; }
        float HfReference { get; }
        float LfReference { get; }
        float Diffusion { get; }
        float Density { get; }
    }
    
}