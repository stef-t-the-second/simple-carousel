using UnityEngine.EventSystems;

namespace Steft.SimpleCarousel.Drag
{
    public interface IDeltaDragHandler : IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public float totalDelta { get; }

        public bool isDragging { get; }
    }
}
