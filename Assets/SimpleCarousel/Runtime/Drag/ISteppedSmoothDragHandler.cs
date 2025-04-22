using UnityEngine;
using UnityEngine.EventSystems;

namespace Steft.SimpleCarousel.Drag
{
    public interface ISteppedSmoothDragHandler : IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        void Init(float startScrollIndex, float maximumScrollIndex);

        public float targetScrollIndex { get; }

        public int rounds { get; }

        public float startScrollIndex { get; }

        public float traveledDelta { get; }

        public float traveledScrollIndizes { get; }

        public float currentScrollIndex { get; }

        public bool isDragging { get; }
    }
}
