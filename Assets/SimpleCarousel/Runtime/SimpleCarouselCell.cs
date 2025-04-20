using System;
using UnityEngine;

namespace Steft.SimpleCarousel
{
    public struct SimpleCarouselCell
    {
        private float m_OffsetFromCenter;

        public SimpleCarouselCell(RectTransform rectTransform, float width, float height)
        {
            if (rectTransform == null)
                throw new ArgumentNullException(nameof(rectTransform));

            this.rectTransform  = rectTransform;
            gameObject          = rectTransform.gameObject;
            this.width          = width;
            this.height         = height;
            m_OffsetFromCenter  = 0;
            offsetFromCenterAbs = 0;
        }

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

        public GameObject gameObject { get; }

        public float width { get; }

        public float height { get; }
    }
}
