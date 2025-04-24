using NUnit.Framework;
using UnityEngine;

namespace Steft.SimpleCarousel
{
    internal class CarouselViewTests
    {
        private MockCarouselView      m_SUT; // system under test
        private MockDragHandler       m_DragHandler;
        private MockCellLayoutHandler m_LayoutHandler;

        [SetUp]
        public void SetUp()
        {
            var gameObject = new GameObject(nameof(CarouselViewTests));
            m_DragHandler   = gameObject.AddComponent<MockDragHandler>();
            m_LayoutHandler = gameObject.AddComponent<MockCellLayoutHandler>();
            m_SUT           = gameObject.AddComponent<MockCarouselView>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(m_SUT.gameObject);
        }
    }
}
