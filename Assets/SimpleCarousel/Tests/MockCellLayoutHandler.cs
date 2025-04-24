using System;
using Steft.SimpleCarousel.Layout;
using UnityEngine;

namespace Steft.SimpleCarousel
{
    internal class MockCellLayoutHandler : MonoBehaviour, ICarouselCellLayoutHandler
    {
        public Action<ICarouselCell> updateLayoutAction { get; set; } = null;

        public void UpdateLayout(ICarouselCell cell) => updateLayoutAction?.Invoke(cell);
    }
}
