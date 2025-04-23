using UnityEngine;
using UnityEngine.EventSystems;

namespace Steft.SimpleCarousel.Drag
{
    public class NormalizedDeltaDrag : MonoBehaviour, IDeltaDragHandler
    {
        [Range(0.1f, 100f)] [SerializeField] private float m_ScrollSensitivity = 10f;

        private Vector2       m_LastLocalCursor = Vector2.zero;
        private RectTransform m_RectTransform;

        public float delta      { get; private set; }
        public bool  isDragging { get; private set; }

        private void Awake()
        {
            m_RectTransform = transform as RectTransform;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isDragging) return;
            isDragging        = true;
            m_LastLocalCursor = Vector2.zero;
            delta             = 0f;

            // we need to initialize LastLocalCursor
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
            delta += smoothedDelta.x;

            m_LastLocalCursor = localCursor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging        = false;
            m_LastLocalCursor = Vector2.zero;
            delta             = 0f;
        }
    }
}
