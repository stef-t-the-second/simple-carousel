using System;
using UnityEngine;

namespace Steft.SimpleCarousel
{
    public struct SimpleCarouselCell
    {
        private float m_OffsetFromCenter;

        public SimpleCarouselCell(int carouselIndex, RectTransform rectTransform, float width, float height)
        {
            if (carouselIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(carouselIndex));

            if (rectTransform == null)
                throw new ArgumentNullException(nameof(rectTransform));

            this.carouselIndex  = carouselIndex;
            this.rectTransform  = rectTransform;
            this.width          = width;
            this.height         = height;
            m_OffsetFromCenter  = 0;
            offsetFromCenterAbs = 0;
        }

        public int carouselIndex { get; }

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

        public RectTransform rectTransform { get; }

        public float width { get; }

        public float height { get; }
    }
}
