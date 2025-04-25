using Steft.SimpleCarousel.Drag;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Steft.SimpleCarousel
{
    internal class MockDragHandler : MonoBehaviour, IDeltaDragHandler
    {
        public void OnBeginDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData) { }

        public void OnDrag(PointerEventData eventData) { }

        public Vector2 totalDelta { get; set; }

        public bool isDragging { get; set; }
    }
}
