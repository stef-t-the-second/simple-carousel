using UnityEngine.EventSystems;

namespace Steft.SimpleCarousel.Drag
{
    public interface IDeltaDragHandler : IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public float delta { get; }

        public bool isDragging { get; }
    }
}
