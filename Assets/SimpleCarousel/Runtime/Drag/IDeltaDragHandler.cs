using UnityEngine;
using UnityEngine.EventSystems;

namespace Steft.SimpleCarousel.Drag
{
    public interface IDeltaDragHandler : IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public Vector2 totalDelta { get; }

        public bool isDragging { get; }
    }
}
