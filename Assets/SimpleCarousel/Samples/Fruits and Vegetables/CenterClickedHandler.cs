using UnityEngine;

namespace Steft.SimpleCarousel.Samples
{
    public class CenterClickedHandler : MonoBehaviour
    {
        public void Handle(ICarouselCell<ICarouselData> cell)
        {
            Debug.Log("Center clicked");
        }
    }
}
