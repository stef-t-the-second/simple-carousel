using UnityEngine;
using UnityEngine.EventSystems;

namespace Steft.SimpleCarousel.Drag
{
    // TODO this needs to be refactored into a sort of "ISteppedSmoothDragHandler"
    //      parts of "SimpleCarouselView.Update" needs to migrated to "NormalizedDrag.Update"
    public interface ISteppedSmoothDragHandler : IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        void Init(float startScrollIndex, float maximumScrollIndex);

        public float targetScrollIndex { get; }

        public float currentScrollIndex { get; }

        public bool isDragging { get; }
    }
}
