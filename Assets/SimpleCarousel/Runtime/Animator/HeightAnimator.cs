using System;
using System.Collections;
using UnityEngine;

namespace Steft.SimpleCarousel.Animator
{
    public class HeightAnimator : MonoBehaviour, IRectTransformAnimator
    {
        [Tooltip("Scale factor for the maximum height during animation (1 = no change, 2 = double height).")]
        [Range(1, 2)]
        [SerializeField]
        private float m_TargetHeightScale = 1.2f;

        [Tooltip("Duration of the complete animation cycle in seconds.")] [Range(0, 4)] [SerializeField]
        private float m_Duration = 0.5f;

        /// <summary>
        /// Animates the height of a <see cref="RectTransform"/> up to a scaled size and back
        /// to its original size over a specified duration with ease-in/ease-out.
        /// </summary>
        public IEnumerator Animate(RectTransform rectTransform, Action onFinished = null)
        {
            var startSizeDelta = rectTransform.sizeDelta;
            float originalHeight = startSizeDelta.y;
            float originalWidth = startSizeDelta.x;
            float targetScaledHeight = originalHeight * m_TargetHeightScale; // Calculate the peak height

            // If duration is too short, just stay at original size
            if (m_Duration <= 0f)
            {
                rectTransform.sizeDelta = new Vector2(originalWidth, originalHeight);
                yield break;
            }

            float elapsedTime = 0f;
            float halfDuration = m_Duration / 2.0f;

            // Animation Loop
            while (elapsedTime < m_Duration)
            {
                float currentHeight;
                float phaseProgress;
                float easedPhaseProgress;

                if (elapsedTime < halfDuration) // Phase 1: Scaling Up
                {
                    // Calculate progress within the first half (0 to 1)
                    phaseProgress      = elapsedTime / halfDuration;
                    easedPhaseProgress = Mathf.SmoothStep(0f, 1f, phaseProgress);
                    // Interpolate from original height to target scaled height
                    currentHeight = Mathf.Lerp(originalHeight, targetScaledHeight, easedPhaseProgress);
                }
                else // Phase 2: Scaling Down
                {
                    // Calculate progress within the second half (0 to 1)
                    phaseProgress      = (elapsedTime - halfDuration) / halfDuration;
                    easedPhaseProgress = Mathf.SmoothStep(0f, 1f, phaseProgress);
                    // Interpolate from target scaled height back to original height
                    currentHeight = Mathf.Lerp(targetScaledHeight, originalHeight, easedPhaseProgress);
                }

                // Apply the calculated height, keeping the original width
                rectTransform.sizeDelta = new Vector2(originalWidth, currentHeight);

                // Wait for the next frame
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure the final height is set precisely back to the original at the end
            rectTransform.sizeDelta = new Vector2(originalWidth, originalHeight);
            onFinished?.Invoke();
        }
    }
}
