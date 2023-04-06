using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Episode.EP1.IceCreamGame
{
    public class IceCreamClicker : MonoBehaviour, IPointerDownHandler
    {
        [NonSerialized]
        public UnityAction onPointerEnter;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!enabled || onPointerEnter == null)
            {
                return;
            }
            onPointerEnter();
        }
    }
}