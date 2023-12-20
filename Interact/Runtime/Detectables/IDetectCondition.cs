namespace MisterGames.Interact.Detectables {

    public interface IDetectCondition {
        bool IsMatch(IDetector detector, IDetectable detectable);
    }

}
