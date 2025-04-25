using UnityEngine;

namespace Steft.SimpleCarousel
{
    /// <summary>
    /// Base class for carousel cells that can display data and be positioned within a carousel view.
    /// </summary>
    /// <typeparam name="TData">The type of data to be displayed in the cell.</typeparam>
    public abstract class CarouselCell<TData> : MonoBehaviour, ICarouselCell<TData> where TData : class, ICarouselData
    {
        private float         m_OffsetFromCenter;
        private RectTransform m_RectTransform;
        private TData         m_Data;

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

        public int index { get; set; }

        public virtual TData data
        {
            get => m_Data;
            set
            {
                m_Data          = value;
                gameObject.name = $"[{index:000}] Element '{m_Data.name}'";
            }
        }

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
