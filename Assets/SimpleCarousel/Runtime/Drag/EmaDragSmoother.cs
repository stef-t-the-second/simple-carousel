using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Steft.SimpleCarousel.Drag
{
    /// <summary>
    ///
    /// </summary>
    /// https://en.wikipedia.org/wiki/Moving_average#Exponential_moving_average
    public class EmaDragSmoother : MonoBehaviour, ISmoothDragProvider
    {
        [Range(0, 1)] [SerializeField] private float m_SmoothingFactor  = 0.2f;
        [Min(0.1f)] [SerializeField]   private float m_TimeDecayFactors = 4.2f;

        public bool isDragging { get; private set; }

        public Vector2 smoothedDelta { get; private set; }

#region Unity Methods

        private void Update()
        {
            if (!isDragging) return;
            float decay = m_TimeDecayFactors * Time.deltaTime;
            smoothedDelta = Vector2.Lerp(smoothedDelta, Vector2.zero, decay);
        }

#endregion

#region Drag Handlers

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging    = true;
            smoothedDelta = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData) => OnDrag(eventData.delta);

        private void OnDrag(Vector2 delta)
        {
            if (!isDragging) return;
            // Debug.Log($"{nameof(delta)}: {delta}");
            smoothedDelta = Vector2.Lerp(smoothedDelta, delta, m_SmoothingFactor);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging    = false;
            smoothedDelta = Vector2.zero;
        }

#endregion
    }
}
