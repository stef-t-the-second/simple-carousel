using UnityEngine;

namespace Steft.SimpleCarousel
{
    public static class RectTransformExtensions
    {
        // will reset local position
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
