using UnityEngine;
using UnityEngine.EventSystems;

namespace Steft.SimpleCarousel.Drag
{
    public interface ISmoothDragProvider : IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public bool isDragging { get; }

        public Vector2 smoothedDelta { get; }
    }
}
