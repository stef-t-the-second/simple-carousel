using Steft.SimpleCarousel;
using Steft.SimpleCarousel.Samples;
using UnityEngine;

namespace SimpleCarousel.Samples
{
    internal class ProduceCarouselView : CarouselView<ProduceData>
    {
#region In-Editor Testing

        [ContextMenu(nameof(InvokeCenterNext))]
        private void InvokeCenterNext() => CenterNext(true);

        [ContextMenu(nameof(InvokeCenterPrevious))]
        private void InvokeCenterPrevious() => CenterPrevious(true);

#endregion
    }
}
