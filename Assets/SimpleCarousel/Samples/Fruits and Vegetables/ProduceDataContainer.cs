using UnityEngine;

namespace Steft.SimpleCarousel.Samples
{
    [CreateAssetMenu(menuName = "Simple Carousel/Produce Data Container")]
    internal class ProduceDataContainer : ScriptableObject
    {
        [SerializeField] private ProduceData[] m_Data;
    }
}
