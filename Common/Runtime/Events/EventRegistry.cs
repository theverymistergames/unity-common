using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Events {

    [CreateAssetMenu(fileName = nameof(EventRegistry), menuName = "MisterGames/Events" + nameof(EventRegistry))]
    public class EventRegistry : ScriptableObject {

        [SerializeField] private Event[] _events;

        public IReadOnlyList<Event> Events => _events;

    }

}
