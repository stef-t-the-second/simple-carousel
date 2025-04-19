using UnityEngine;
using UnityEngine.EventSystems;

namespace Steft.SimpleCarousel.Drag
{
    // TODO this needs to be refactored into a sort of "ISteppedSmoothDragHandler"
    //      parts of "SimpleCarouselView.Update" needs to migrated to "NormalizedDrag.Update"
    public interface ISmoothDragProvider : IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public bool isDragging { get; }

        public Vector2 smoothedDelta { get; }
    }
}
