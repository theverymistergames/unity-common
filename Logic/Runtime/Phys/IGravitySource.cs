using UnityEngine;

namespace MisterGames.Logic.Phys {

    public interface IGravitySource {
        Vector3 GetGravity(Vector3 position, out float weight);
    }
    
}