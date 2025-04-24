using UnityEngine;
using UnityEngine.UI;

namespace Steft.SimpleCarousel.Samples
{
    internal class ProduceCarouselCell : CarouselCell<ProduceData>
    {
        [SerializeField] private Image m_Image;
        [SerializeField] private Text  m_TextName;
        [SerializeField] private Text  m_TextDescription;

        public override ProduceData data
        {
            get => base.data;
            set
            {
                base.data              = value;
                m_Image.sprite         = Resources.Load<Sprite>(value.imageName);
                m_TextName.text        = value.name;
                m_TextDescription.text = value.description;
            }
        }
    }
}
