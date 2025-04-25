using UnityEngine;
using UnityEngine.EventSystems;

namespace Steft.SimpleCarousel.Drag
{
    /// <summary>
    ///     The NormalizedDeltaDrag class implements the <see cref="IDeltaDragHandler" /> interface
    ///     to handle drag events in a way that normalizes delta values based on
    ///     the size of the associated <see cref="RectTransform" />.
    /// </summary>
    /// <remarks>
    ///     This script is intended to be used in Unity for components requiring
    ///     normalized drag input handling. The normalized delta is calculated
    ///     relative to the <see cref="RectTransform" />'s dimensions.
    /// </remarks>
    public class NormalizedDeltaDrag : MonoBehaviour, IDeltaDragHandler
    {
        [Tooltip("Adjusts the drag sensitivity. Higher values increase scrolling speed.")]
        [Range(0.1f, 100f)]
        [SerializeField]
        private float m_ScrollSensitivity = 10f;

        /// <summary>
        ///     Stores the last known local cursor position in the coordinate space of the associated <see cref="RectTransform" />.
        /// </summary>
        private Vector2 m_LastLocalCursor = Vector2.zero;

        /// <summary>
        ///     The RectTransform used to calculate normalized drag deltas.
        /// </summary>
        private RectTransform m_RectTransform;

        public Vector2 totalDelta { get; private set; }

        public bool isDragging { get; private set; }

        private void Awake() => m_RectTransform = transform as RectTransform;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isDragging) return;
            isDragging        = true;
            m_LastLocalCursor = Vector2.zero;
            totalDelta        = Vector2.zero;

            // We need to initialize LastLocalCursor
            // to avoid big deltas during the first OnDrag call
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                m_RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out m_LastLocalCursor);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    m_RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var localCursor))
            {
                return;
            }

            var normalizedDelta = new Vector2(
                (localCursor.x - m_LastLocalCursor.x) / ((RectTransform)transform).rect.width,
                (localCursor.y - m_LastLocalCursor.y) / ((RectTransform)transform).rect.height
            );

            var smoothedDelta = normalizedDelta * m_ScrollSensitivity;
            totalDelta += smoothedDelta;

            m_LastLocalCursor = localCursor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging        = false;
            m_LastLocalCursor = Vector2.zero;
            totalDelta        = Vector2.zero;
        }
    }
}
