using System.Collections.Generic;
using MisterGames.Common.Labels;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Logic.Rendering {

    public sealed class LampIntensityNoiseReferenceToLabeledSet : MonoBehaviour {

        [SerializeField] private LampIntensityNoise _lampIntensityNoise;
        [SerializeField] private LabelValue<HashSet<Object>> _lampsLabel;

        private void Awake() {
            if (!_lampsLabel.TryGetData(out var set) || set == null) return;

            var lamps = new List<LampBehaviour>(set.Count);

            foreach (var obj in set) {
                if (obj is LampBehaviour lamp) lamps.Add(lamp);
            }

            _lampIntensityNoise.SetLamps(lamps.ToArray());
        }
    }

}
