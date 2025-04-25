using System;
using UnityEngine;

namespace Steft.SimpleCarousel.Samples
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformToScreen : MonoBehaviour
    {
        private RectTransform m_RectTransform;

        private void Awake()
        {
            m_RectTransform = transform as RectTransform;
            if (m_RectTransform == null)
                throw new NullReferenceException();

            m_RectTransform.ResetToMiddleCenter();
            m_RectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        }

#if UNITY_EDITOR
        private void Update() => m_RectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
#endif
    }
}
