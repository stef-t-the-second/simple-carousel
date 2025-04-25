using System;
using System.Collections;
using UnityEngine;

namespace Steft.SimpleCarousel.Animator
{
    /// <summary>
    ///     Interface for animating <see cref="RectTransform"/> components.
    /// </summary>
    public interface IRectTransformAnimator
    {
        /// <summary>
        ///     Animates the given <see cref="RectTransform"/> component.
        /// </summary>
        /// <param name="rectTransform">The <see cref="RectTransform"/> to animate.</param>
        /// <param name="onFinished">Optional callback to invoke when the animation is complete.</param>
        IEnumerator Animate(RectTransform rectTransform, Action onFinished = null);
    }
}
