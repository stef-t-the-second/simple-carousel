using UnityEngine;
using UnityEngine.EventSystems;

namespace Steft.SimpleCarousel.Drag
{
    public class NormalizedDeltaDrag : MonoBehaviour, ISteppedSmoothDragHandler
    {
        private const float k_MinMaximumScrollIndex = 2f;

        [Range(0.1f,    100f)] [SerializeField] private float m_ScrollSensitivity = 10f;
        [Range(0.0001f, 20f)] [SerializeField]  private float m_ScrollSmoothTime  = 0.2f;

        private Vector2       m_LastLocalCursor = Vector2.zero;
        private RectTransform m_RectTransform;
        private float         m_ScrollVelocity;
        private float         m_MaximumScrollIndex = 2f;

        public float maximumScrollIndex
        {
            get => m_MaximumScrollIndex;
            set
            {
                if (value < k_MinMaximumScrollIndex)
                {
                    Debug.LogWarning(
                        $"'{value}' too small for '{nameof(maximumScrollIndex)}'. " +
                        $"Using fallback '{k_MinMaximumScrollIndex}'.");

                    m_MaximumScrollIndex = k_MinMaximumScrollIndex;
                }
                else
                {
                    m_MaximumScrollIndex = value;
                }
            }
        }

        public float targetScrollIndex { get; private set; }

        public float currentScrollIndex { get; private set; }

        public bool isDragging { get; private set; }

        private void Awake()
        {
            m_RectTransform = transform as RectTransform;
        }

        private void Update()
        {
            currentScrollIndex = Mathf.SmoothDamp(
                currentScrollIndex, targetScrollIndex, ref m_ScrollVelocity, m_ScrollSmoothTime, 10);

            // Debug.Log(
            //     $"{nameof(targetScrollIndex)}: {targetScrollIndex:F2}, " +
            //     $"{nameof(currentScrollIndex)}: {currentScrollIndex:F2}"
            // );

            if (!isDragging                                               &&
                Mathf.Abs(m_ScrollVelocity)                       < 0.01f &&
                Mathf.Abs(targetScrollIndex - currentScrollIndex) < 0.01f)
            {
                currentScrollIndex = targetScrollIndex;
                m_ScrollVelocity   = 0f;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isDragging) return;
            isDragging        = true;
            targetScrollIndex = currentScrollIndex;
            m_LastLocalCursor = Vector2.zero;
            m_ScrollVelocity  = 0f;
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

            var smoothedDelta = -normalizedDelta * m_ScrollSensitivity;
            targetScrollIndex += smoothedDelta.x;
            m_LastLocalCursor =  localCursor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging        = false;
            m_LastLocalCursor = Vector2.zero;
            targetScrollIndex = Mathf.Round(targetScrollIndex);
            targetScrollIndex = Mathf.Clamp(targetScrollIndex, 0f, m_MaximumScrollIndex);
        }
    }
}
