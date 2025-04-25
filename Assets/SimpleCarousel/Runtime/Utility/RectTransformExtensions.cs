using UnityEngine;

namespace Steft.SimpleCarousel
{
    /// <summary>
    ///     Provides extension methods for <see cref="RectTransform" />.
    /// </summary>
    public static class RectTransformExtensions
    {
        /// <summary>
        ///     Resets a RectTransform to the middle center position with default anchor and pivot settings.
        /// </summary>
        public static void ResetToMiddleCenter(this RectTransform rectTransform)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.anchorMin        = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax        = new Vector2(0.5f, 0.5f);
            rectTransform.pivot            = new Vector2(0.5f, 0.5f);
            rectTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}
