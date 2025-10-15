using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MisterGames.UI.Components {
    
    public sealed class ScrollEventsHandler : MonoBehaviour, IScrollHandler {
        
        public event Action<PointerEventData> OnScroll = delegate { };

        void IScrollHandler.OnScroll(PointerEventData eventData) {
            OnScroll.Invoke(eventData);
        }
    }
    
}