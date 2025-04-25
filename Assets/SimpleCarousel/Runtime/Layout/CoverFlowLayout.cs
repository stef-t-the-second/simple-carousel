using UnityEngine;

namespace Steft.SimpleCarousel.Layout
{
    /// <summary>
    /// Handles the layout management for a carousel in a Cover Flow style (https://en.wikipedia.org/wiki/Cover_Flow).
    /// This class implements the <see cref="ICarouselCellLayoutHandler"/> interface to dynamically
    /// arrange carousel cells based on their offset from the center position.
    /// </summary>
    /// <remarks>
    /// The Cover Flow layout visually organizes items within a carousel by applying scaling, spacing,
    /// depth, and rotation effects. It achieves smooth transitions for focus change, making the center
    /// cell the focal point while progressively modifying the appearance of the neighboring cells.
    /// </remarks>
    /// <seealso cref="Steft.SimpleCarousel.ICarouselCell" />
    /// <seealso cref="Steft.SimpleCarousel.Layout.ICarouselCellLayoutHandler" />
    public class CoverFlowLayout : MonoBehaviour, ICarouselCellLayoutHandler
    {
        [Tooltip("Controls the overlap between adjacent cells. Higher values increase overlap.")]
        [Range(0.05f, 0.4f)]
        [SerializeField]
        private float m_RelativeOverlap = 0.2f;

        [Tooltip("Controls depth effect intensity for outer cells. Higher values create stronger depth effect.")]
        [Range(1f, 2f)]
        [SerializeField]
        private float m_DepthScalePower = 1.5f;

        [Tooltip("Scale factor for cells next to center. Lower values make adjacent cells smaller.")]
        [Range(0.1f, 0.9f)]
        [SerializeField]
        private float m_NeighbourScale = 0.8f;

        [Tooltip("Rotation angle for off-center cells. Higher values increase rotation effect.")]
        [Range(0, 70)]
        [SerializeField]
        private float m_PanStep = 20;

        [Tooltip("Z-axis offset for cells. Higher values push off-center cells further back.")]
        [Range(10, 200)]
        [SerializeField]
        private float m_DepthStep = 80;

        /// <summary>
        /// Updates the position, rotation, and scale of a carousel cell based on its offset from the center.
        /// </summary>
        /// <param name="cell">The carousel cell to update.</param>
        /// <remarks>
        /// For center cells (offset = 0):
        /// <ul>
        /// <li>Scale is set to 1</li>
        /// <li>Position is at origin (0,0,0)</li>
        /// <li>No rotation applied</li>
        /// </ul>
        ///
        /// For non-center cells:
        /// <ul>
        /// <li>Scale decreases based on distance from center</li>
        /// <li>Position calculated using cell width, overlap, and depth effects</li>
        /// <li>Rotation increases with distance from center</li>
        /// </ul>
        /// </remarks>
        public void UpdateLayout(ICarouselCell cell)
        {
            float posX, posY, posZ, rotY;
            if (Mathf.Approximately(cell.offsetFromCenter, 0))
            {
                cell.rectTransform.localScale = Vector3.one;

                posX = 0;
                posY = 0;
                posZ = 0;
                rotY = 0;
            }
            else
            {
                // Determine if cell is to the left (-1) or right (1) of center
                int leftOrRight = cell.offsetFromCenter > 0 ? 1 : -1;

                // Calculate scale based on distance from center
                // Cells beyond immediate neighbors use fixed scale
                float scale = cell.offsetFromCenterAbs < 1
                    ? Mathf.Lerp(1f, m_NeighbourScale, cell.offsetFromCenterAbs)
                    : m_NeighbourScale;

                cell.rectTransform.localScale = Vector3.one * scale;
                float cellWidth = cell.rectTransform.rect.width;

                // Calculate X position using three components:
                // 1. Base width spacing between cells
                // 2. Additional offset based on layer distance from center
                // 3. Progressive overlap reduction for outer cells
                posX =
                (
                    cellWidth +
                    cellWidth * (cell.offsetFromCenterAbs - 1) -
                    (cellWidth * m_RelativeOverlap * Mathf.Pow(cell.offsetFromCenterAbs, m_DepthScalePower))
                ) * leftOrRight;

                posY = 0;

                // Apply exponential depth offset based on distance from center
                posZ = m_DepthStep * Mathf.Pow(cell.offsetFromCenterAbs, m_DepthScalePower);

                // Rotate cells proportionally to their offset, negative for proper perspective
                rotY = -m_PanStep * cell.offsetFromCenter;
            }

            cell.rectTransform.localPosition = new Vector3(posX, posY, posZ);
            cell.rectTransform.localRotation = Quaternion.Euler(0, rotY, 0);
        }
    }
}
