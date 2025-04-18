using System;
using UnityEngine;

namespace Steft.SimpleCarousel
{
    public readonly struct SimpleCarouselCell
    {
        public SimpleCarouselCell(int carouselIndex, RectTransform rectTransform)
        {
            if (carouselIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(carouselIndex));

            if (rectTransform == null)
                throw new ArgumentNullException(nameof(rectTransform));

            this.carouselIndex = carouselIndex;
            this.rectTransform = rectTransform;
        }

        public int carouselIndex { get; }

        public RectTransform rectTransform { get; }
    }
}
