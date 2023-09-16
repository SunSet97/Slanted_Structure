using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Episode.EP1.IceCreamGame
{
    public class IceCreamClicker : MonoBehaviour, IPointerDownHandler
    {
        private Action onPointerEnter;

        public void Init(Action onPointerEnter)
        {
            this.onPointerEnter = onPointerEnter;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!enabled)
            {
                return;
            }

            onPointerEnter?.Invoke();
        }
    }
}