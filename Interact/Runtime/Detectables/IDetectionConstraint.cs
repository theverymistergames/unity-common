namespace MisterGames.Interact.Detectables {

    public interface IDetectionConstraint {
        bool IsAllowedDetection(IDetector detector, IDetectable detectable);
    }

}
