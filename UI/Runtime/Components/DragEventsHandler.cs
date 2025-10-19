using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MisterGames.UI.Components {
    
    public sealed class DragEventsHandler : 
        MonoBehaviour, 
        IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public event Action<PointerEventData> OnBeginDrag = delegate { };
        public event Action<PointerEventData> OnEndDrag = delegate { };
        public event Action<PointerEventData> OnDrag = delegate { };

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
            OnBeginDrag.Invoke(eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            OnEndDrag.Invoke(eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData) {
            OnDrag.Invoke(eventData);
        }
    }
    
}