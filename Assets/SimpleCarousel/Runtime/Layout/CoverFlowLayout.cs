using UnityEngine;

namespace Steft.SimpleCarousel.Layout
{
    /// <summary>
    ///
    /// </summary>
    /// https://en.wikipedia.org/wiki/Cover_Flow
    public class CoverFlowLayout : MonoBehaviour, ICarouselCellLayoutHandler<ICarouselCell>
    {
        [Range(0.05f, 0.4f)] [SerializeField]
        // instead of have an absolute overlap depending on a cells width,
        // we may implement an overlap depending on the orthogonal size of a cell on screen
        // to implement this idea, we must consider the rotation of a cell, which changes the occupied pixels on screen
        private float m_RelativeOverlap = 0.2f;

        [Range(1f, 2f)] [SerializeField] private float m_DepthScalePower = 1.5f;

        [Range(0.1f, 0.9f)] [SerializeField] private float m_NeighbourScale = 0.8f;

        [Range(0, 70)] [SerializeField] private float m_PanStep = 20; // tilt, roll, pan

        [Range(10, 200)] [SerializeField] private float m_DepthStep = 80;

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
                int leftOrRight = cell.offsetFromCenter > 0 ? 1 : -1;

                float scale = cell.offsetFromCenterAbs < 1
                    // immediate neighbours of center require transition of scale
                    ? Mathf.Lerp(1f, m_NeighbourScale, cell.offsetFromCenterAbs)
                    : m_NeighbourScale;

                cell.rectTransform.localScale = Vector3.one * scale;
                float cellWidth = cell.rectTransform.rect.width;
                // for example, results in more overlap the further out we are
                float cellOffsetFromCenterDepth = Mathf.Pow(cell.offsetFromCenterAbs, m_DepthScalePower);

                posX =
                (
                    // for example, for center and left neighbour:
                    // right edge of the neighbour will align with left edge of the center
                    cellWidth +

                    // in which layer (how far out) this cell is
                    cellWidth * (cell.offsetFromCenterAbs - 1)

                    // more overlap the further out we are
                    - (cellWidth * m_RelativeOverlap * Mathf.Pow(cell.offsetFromCenterAbs, m_DepthScalePower))
                ) * leftOrRight;

                posY = 0;
                // the further back the further out we are
                posZ = m_DepthStep * Mathf.Pow(cell.offsetFromCenterAbs, m_DepthScalePower);
                rotY = -m_PanStep  * cell.offsetFromCenter;
            }

            cell.rectTransform.localPosition = new Vector3(posX, posY, posZ);
            cell.rectTransform.localRotation = Quaternion.Euler(0, rotY, 0);
        }
    }
}
