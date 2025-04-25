using UnityEngine;
using UnityEngine.EventSystems;

namespace Steft.SimpleCarousel.Drag
{
    /// <summary>
    /// Represents an interface for handling delta-based drag events.
    /// </summary>
    /// <remarks>
    /// This interface extends the drag lifecycle interfaces <see cref="IBeginDragHandler"/>,
    /// <see cref="IEndDragHandler"/>, and <see cref="IDragHandler"/>
    /// to provide additional properties for managing drag-related state and data.
    /// </remarks>
    public interface IDeltaDragHandler : IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        /// <summary>
        /// Gets the total accumulated delta of the drag input.
        /// </summary>
        /// <remarks>
        /// The value represents the overall change in position (as a 2D vector) observed during a drag operation.
        /// It is typically used to calculate the offset or displacement relative to the initial position.
        /// </remarks>
        public Vector2 totalDelta { get; }

        /// <summary>
        /// Indicates whether a drag operation is currently in progress.
        /// </summary>
        /// <remarks>
        /// This property is used to determine the active state of a drag action.
        /// It is typically set to true when a drag starts and resets to false upon drag completion or cancellation.
        /// </remarks>
        public bool isDragging { get; }
    }
}
