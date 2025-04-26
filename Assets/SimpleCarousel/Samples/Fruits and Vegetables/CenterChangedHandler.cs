using UnityEngine;

namespace Steft.SimpleCarousel.Samples
{
    internal class CenterChangedHandler : MonoBehaviour
    {
        public void Handle(ICarouselCell<ICarouselData> cell)
        {
            Debug.Log($"Center is '{cell.data.name}'");
        }
    }
}
