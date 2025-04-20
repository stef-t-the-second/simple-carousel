using UnityEngine;

namespace Steft.SimpleCarousel.Layout
{
    /// <summary>
    ///
    /// </summary>
    /// https://en.wikipedia.org/wiki/Cover_Flow
    public class CoverFlowLayout : MonoBehaviour, ICarouselCellLayoutHandler<SimpleCarouselCell>
    {
        [Range(0.05f, 0.4f)] [SerializeField]
        // instead of have an absolute overlap depending on a cells width,
        // we may implement an overlap depending on the orthogonal size of a cell on screen
        // to implement this idea, we must consider the rotation of a cell, which changes the occupied pixels on screen
        private float m_RelativeOverlap = 0.2f;

        [Range(0.1f, 0.9f)] [SerializeField] private float m_NeighbourScale = 0.8f;

        [Range(0, 70)] [SerializeField] private float m_PanStep = 20; // tilt, roll, pan

        [Range(10, 200)] [SerializeField] private float m_DepthStep = 80;

        public void UpdateLayout(SimpleCarouselCell cell)
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
                float currentNeighbourWidthWorld = cell.rectTransform.rect.width;

                posX =
                (
                    // for example for center and left neighbour:
                    // right edge of the neighbour will align with left edge of the center
                    (cell.width + currentNeighbourWidthWorld) / 2 +

                    // in which layer (how far out) this cell is
                    currentNeighbourWidthWorld * (cell.offsetFromCenterAbs - 1)

                    //
                    - (currentNeighbourWidthWorld * m_RelativeOverlap *
                       // more overlap the further out we are
                       cell.offsetFromCenterAbs * cell.offsetFromCenterAbs)
                ) * leftOrRight;

                posY = 0;
                posZ = m_DepthStep *
                       // further back the further out we are
                       cell.offsetFromCenterAbs * cell.offsetFromCenterAbs;
                rotY = -m_PanStep * cell.offsetFromCenter;
            }

            cell.rectTransform.localPosition = new Vector3(posX, posY, posZ);
            cell.rectTransform.localRotation = Quaternion.Euler(0, rotY, 0);
        }
    }
}
