using System;
using UnityEngine;

namespace Steft.SimpleCarousel
{
    public abstract class CarouselCell<TData> : MonoBehaviour, ICarouselCell<TData>
        where TData : class
    {
        private float         m_OffsetFromCenter;
        private RectTransform m_RectTransform;

        public float offsetFromCenter
        {
            get => m_OffsetFromCenter;
            set
            {
                m_OffsetFromCenter  = value;
                offsetFromCenterAbs = Mathf.Abs(m_OffsetFromCenter);
            }
        }

        public float offsetFromCenterAbs { get; private set; }

        public abstract TData data { get; set; }

        public RectTransform rectTransform
        {
            get
            {
                if (m_RectTransform == null)
                    m_RectTransform = transform as RectTransform;

                return m_RectTransform;
            }
        }
    }
}
