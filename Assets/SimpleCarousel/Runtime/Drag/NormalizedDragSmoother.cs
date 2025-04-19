using UnityEngine;
using UnityEngine.EventSystems;

namespace Steft.SimpleCarousel.Drag
{
    public class NormalizedDragSmoother : MonoBehaviour, ISmoothDragProvider
    {
        [Range(0.1f, 100f)] [SerializeField] private float m_ScrollSensitivity = 10f;

        private Vector2       m_LastLocalCursor = Vector2.zero;
        private RectTransform m_RectTransform;

        public bool isDragging { get; private set; }

        public Vector2 smoothedDelta { get; private set; }

        private void Awake()
        {
            m_RectTransform = transform as RectTransform;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isDragging) return;
            isDragging        = true;
            smoothedDelta     = Vector2.zero;
            m_LastLocalCursor = Vector2.zero;
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

            smoothedDelta     = -normalizedDelta * m_ScrollSensitivity;
            m_LastLocalCursor = localCursor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging    = false;
            smoothedDelta = Vector2.zero;
        }
    }
}
