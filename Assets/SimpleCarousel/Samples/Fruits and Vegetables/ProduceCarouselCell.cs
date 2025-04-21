using UnityEngine;
using UnityEngine.UI;

namespace Steft.SimpleCarousel.Samples
{
    internal class ProduceCarouselCell : CarouselCell<ProduceData>
    {
        [SerializeField] private Image m_Image;
        [SerializeField] private Text  m_TextName;
        [SerializeField] private Text  m_TextDescription;

        private ProduceData m_Data;

        public override ProduceData data
        {
            get => m_Data;
            set
            {
                m_Data                 = value;
                m_Image.sprite         = Resources.Load<Sprite>(m_Data.imageName);
                m_TextName.text        = m_Data.name;
                m_TextDescription.text = m_Data.description;
            }
        }
    }
}
