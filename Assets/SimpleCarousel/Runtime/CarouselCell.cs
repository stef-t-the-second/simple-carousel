using UnityEngine;

namespace Steft.SimpleCarousel
{
    public class CarouselCell : MonoBehaviour
    {
        private float m_OffsetFromCenter;

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

        public RectTransform rectTransform => transform as RectTransform;
    }
}
