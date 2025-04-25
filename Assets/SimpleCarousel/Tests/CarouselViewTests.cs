using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Steft.SimpleCarousel
{
    internal class CarouselViewTests
    {
        private MockCarouselView      m_SUT; // system under test
        private MockCarouselCell      m_Cell;
        private MockDragHandler       m_DragHandler;
        private MockCellLayoutHandler m_LayoutHandler;

        private static object[] mockData =>
            new object[]
            {
                new MockData { name = Path.GetRandomFileName() }
            };

        private static object[] mockDataArrays =>
            new object[]
            {
                new[]
                {
                    new MockData { name = "1" },
                    new MockData { name = "2" },
                    new MockData { name = "3" },
                    new MockData { name = "4" },
                    new MockData { name = "5" },
                }
            };

        [SetUp]
        public void SetUp()
        {
            var gameObject = new GameObject(nameof(CarouselViewTests));
            gameObject.SetActive(false);
            m_DragHandler   = gameObject.AddComponent<MockDragHandler>();
            m_LayoutHandler = gameObject.AddComponent<MockCellLayoutHandler>();
            m_SUT           = gameObject.AddComponent<MockCarouselView>();

            var cellPrefab = new GameObject(nameof(MockCarouselCell));
            cellPrefab.gameObject.SetActive(false);
            cellPrefab.AddComponent<RectTransform>();
            m_Cell           = cellPrefab.AddComponent<MockCarouselCell>();
            m_SUT.cellPrefab = m_Cell;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(m_SUT.gameObject);
        }

#region Unity Methods

        [Test]
        public void Awake_HasChildren()
        {
            Assert.That(m_SUT.cellPrefab,           Is.Not.Null);
            Assert.That(m_SUT.visibleElements,      Is.GreaterThan(0));
            Assert.That(m_SUT.transform.childCount, Is.Zero);

            m_SUT.gameObject.SetActive(true);

            Assert.That(m_SUT.transform.childCount, Is.GreaterThan(0));
            Assert.That(m_SUT.transform.childCount, Is.EqualTo(m_SUT.poolSize));
        }

#endregion

#region Visible Elements

        [Test]
        [TestCase(0, 3)]
        [TestCase(4, 3)]
        [TestCase(8, 7)]
        [TestCase(3, 3)]
        [TestCase(5, 5)]
        public void VisibleElements_VerifyConstraints(int value, int expected)
        {
            m_SUT.visibleElements = value;
            Assert.That(m_SUT.visibleElements,      Is.EqualTo(expected));
            Assert.That(m_SUT.transform.childCount, Is.EqualTo(expected));
        }

#endregion

#region Add

        [Test]
        [TestCaseSource(nameof(mockDataArrays))]
        public void Data_AddRange(MockData[] data)
        {
            Assert.That(m_SUT.data, Is.Not.Null);
            Assert.That(m_SUT.data, Is.Empty);

            m_SUT.Add(data);

            Assert.That(m_SUT.data.Count, Is.EqualTo(data.Length));
        }

        [Test]
        [TestCaseSource(nameof(mockData))]
        public void Data_Add(MockData data)
        {
            Assert.That(m_SUT.data, Is.Not.Null);
            Assert.That(m_SUT.data, Is.Empty);

            m_SUT.Add(data);

            Assert.That(m_SUT.data, Is.Not.Empty);
        }

#endregion

#region Remove

        [Test]
        public void Data_RemoveAll()
        {
            Assert.That(m_SUT.data, Is.Empty);
            var data = mockDataArrays[0] as MockData[];
            Assert.That(data,        Is.Not.Null);
            Assert.That(data.Length, Is.EqualTo(5));
            m_SUT.Add(data);
            Assert.That(m_SUT.data.Count, Is.EqualTo(data.Length));

            m_SUT.RemoveAll();

            Assert.That(m_SUT.data,       Is.Not.Null);
            Assert.That(m_SUT.data.Count, Is.Zero);
        }

#endregion

#region OnCenterChanged

        [Test]
        public void OnCenterChanged_Invoke()
        {
            Assert.That(m_SUT.onCenterChanged, Is.Not.Null);

            bool called = false;
            m_SUT.onCenterChanged.AddListener(_ =>
            {
                called = true;
            });

            m_SUT.onCenterChanged.Invoke(new MockData { name = nameof(OnCenterChanged_Invoke) });
            Task.Delay(50);

            Assert.That(called, Is.True);
        }

        [Test]
        public void OnCenterChanged_Center()
        {
            Assert.That(m_SUT.onCenterChanged, Is.Not.Null);
            Assert.That(m_SUT.data,            Is.Empty);

            var data = mockDataArrays[0] as MockData[];
            Assert.That(data.Length, Is.EqualTo(5));
            m_SUT.Add(data);

            MockData dataReceived = null;
            m_SUT.onCenterChanged.AddListener(d => dataReceived = d);

            m_SUT.Center(2, false);
            Task.Delay(50);

            Assert.That(dataReceived.name, Is.EqualTo(data[2].name));
        }

        [Test]
        public void OnCenterChanged_RebuildCells()
        {
            Assert.That(m_SUT.onCenterChanged, Is.Not.Null);
            Assert.That(m_SUT.data,            Is.Empty);

            var data = mockDataArrays[0] as MockData[];
            Assert.That(data,        Is.Not.Null);
            Assert.That(data.Length, Is.EqualTo(5));
            m_SUT.Add(data);
            m_SUT.visibleElements = data.Length;

            MockData dataReceived = null;
            m_SUT.onCenterChanged.AddListener(d =>
            {
                Debug.Log("Received: " + d.name);
                dataReceived = d;
            });

            m_SUT.RebuildCells(true);
            Task.Delay(50);

            Assert.That(dataReceived,      Is.Not.Null);
            Assert.That(dataReceived.name, Is.EqualTo(data[2].name));
        }

#endregion
    }
}
