using UnityEngine;

namespace MisterGames.Character.Configs {

    //[CreateAssetMenu(fileName = nameof(ViewSettings), menuName = "MisterGames/Character/" + nameof(ViewSettings))]
    public class ViewSettings : ScriptableObject {

        [Min(0.001f)] public float sensitivityHorizontal;
        [Min(0.001f)] public float sensitivityVertical;
        [Min(0.001f)] public float viewSmoothFactor;

    }

}
