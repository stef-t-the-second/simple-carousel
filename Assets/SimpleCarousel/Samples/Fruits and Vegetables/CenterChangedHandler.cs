using UnityEngine;

namespace Steft.SimpleCarousel.Samples
{
    internal class CenterChangedHandler : MonoBehaviour
    {
        public void Handle(ProduceData data)
        {
            Debug.Log($"Center is '{data.name}'");
        }
    }
}
