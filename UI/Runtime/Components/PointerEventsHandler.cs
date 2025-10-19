using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MisterGames.UI.Components {
    
    public sealed class PointerEventsHandler : 
        MonoBehaviour, 
        IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler,
        IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
    {
        public event Action<PointerEventData> OnPointerEnter = delegate { };
        public event Action<PointerEventData> OnPointerExit = delegate { };
        public event Action<PointerEventData> OnPointerMove = delegate { };

        public event Action<PointerEventData> OnPointerDown = delegate { };
        public event Action<PointerEventData> OnPointerUp = delegate { };
        public event Action<PointerEventData> OnPointerClick = delegate { };

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
            OnPointerEnter.Invoke(eventData);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
            OnPointerExit.Invoke(eventData);
        }

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData) {
            OnPointerMove.Invoke(eventData);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
            OnPointerUp.Invoke(eventData);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            OnPointerDown.Invoke(eventData);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
            OnPointerClick.Invoke(eventData);
        }
    }
    
}