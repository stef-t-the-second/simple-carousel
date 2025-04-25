using System;
using System.Collections.Generic;
using System.Linq;
using Steft.SimpleCarousel.Drag;
using Steft.SimpleCarousel.Layout;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Steft.SimpleCarousel
{
    /// <summary>
    ///     Base class for carousel views that can display and manage a collection of cells.
    /// </summary>
    public abstract class CarouselView : MonoBehaviour
    {
        /// <summary>
        ///     Rebuilds all cells in the carousel view.
        /// </summary>
        /// <param name="force">If true, forces a rebuild even if the number of cells matches the pool size.</param>
        internal abstract void RebuildCells(bool force = false);
    }

    /// <summary>
    ///     A carousel view component that displays and manages a collection of cells containing data of type TData.
    ///     Supports drag interactions and automatic cell recycling.
    /// </summary>
    /// <typeparam name="TData">The type of data to be displayed in the carousel cells.</typeparam>
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public class CarouselView<TData> : CarouselView, IBeginDragHandler, IEndDragHandler
        where TData : class, ICarouselData
    {
        // TODO:
        // 1. Clean up scene file by removing redundant serialized field entries
        // 2. Perform device testing for mobile compatibility
        // 3. Complete Unity Package setup and documentation

        [Tooltip("Event invoked when the center item changes.")] [SerializeField]
        private UnityEvent<TData> m_OnCenterChanged = new();

        [Space] [Min(3)] [Tooltip("Number of visible carousel cells (must be odd, minimum 3).")] [SerializeField]
        private int m_VisibleElements = 3;

        [Range(0.0001f, 20f)] [Tooltip("Duration of centering animation in seconds.")] [SerializeField]
        private float m_CenterSmoothTime = 0.2f;

        [Tooltip("Prefab used to create carousel cells.")] [SerializeField]
        private CarouselCell<TData> m_CellPrefab;

        [Tooltip("Collection of items to display in the carousel.")] [SerializeField]
        private List<TData> m_Data = new(32);

        private readonly LinkedList<CarouselCell<TData>> m_CellPool = new();

        // Potential improvement: Implement dependency injection to detect interface implementations
        // on GameObject during compilation rather than runtime for earlier error detection
        private IDeltaDragHandler          m_DeltaDragHandler;
        private ICarouselCellLayoutHandler m_CarouselCellLayoutHandler;

        private float m_CenterIndex;
        private float m_TargetCenterIndex;
        private float m_CenterSmoothVelocity;

        private float depthMinusMargin => depth - 0.5f;

        /// <summary>
        ///     Gets or sets the prefab used to instantiate carousel cells.
        /// </summary>
        internal CarouselCell<TData> cellPrefab
        {
            get => m_CellPrefab;
            set
            {
                bool changed = m_CellPrefab != value;

                m_CellPrefab = value;

                if (changed)
                    RebuildCells(true);
            }
        }

        /// <summary>
        ///     Gets the read-only list of data items in the carousel.
        /// </summary>
        internal IReadOnlyList<TData> data => m_Data;

        /// <summary>
        ///     Gets the depth of the carousel, representing how many cells can be displayed on each side of the center.
        /// </summary>
        internal int depth => (poolSize - 1) / 2;

        /// <summary>
        ///     Gets the total number of cells in the pool, including both visible and buffer cells.
        /// </summary>
        internal int poolSize => m_VisibleElements + 2;

        /// <summary>
        ///     Gets or sets the number of visible elements in the carousel.
        ///     Must be an odd number greater than or equal to 3.
        /// </summary>
        public int visibleElements
        {
            get => m_VisibleElements;
            set
            {
                // constraints:
                // - minimum is 3
                // - must be an odd number

                if (m_VisibleElements == value)
                    return;

                if (value < 3)
                {
                    m_VisibleElements = 3;
                    RebuildCells();
                    return;
                }

                if (value % 2 == 0)
                {
                    // minimum even number is 4
                    m_VisibleElements = value - 1;
                    RebuildCells();
                    return;
                }

                m_VisibleElements = value;
                RebuildCells();
            }
        }

        /// <summary>
        ///     Event invoked when the centered item changes in the carousel.
        /// </summary>
        public UnityEvent<TData> onCenterChanged => m_OnCenterChanged;

        private void InvokeOnCenterChangedWithCenterIndex()
        {
            if (m_Data.Count == 0)
                return;

            int index = GetCircularIndex(Mathf.RoundToInt(m_TargetCenterIndex), m_Data.Count);
            Debug.Log($"Center is '{m_Data[index].name}' at index '{index}'");
            onCenterChanged.Invoke(m_Data[index]);
        }

        /// <summary>
        ///     Retrieves the cell currently positioned at the center of the carousel.
        /// </summary>
        /// <returns>The center cell of the carousel view.</returns>
        private CarouselCell<TData> GetCenterCell()
        {
            var center = m_CellPool.First;
            for (int i = 0; i < depth; i++)
            {
                if (center == null)
                    throw new NullReferenceException($"{nameof(center)} at {i}");

                center = center.Next;
            }

            if (center == null || center.Value == null)
                throw new NullReferenceException($"{nameof(center)} at {m_CenterIndex}");

            return center.Value;
        }

        /// <summary>
        ///     Calculates the circular index for a given index and size, ensuring the result is
        ///     within the range [0, <paramref name="size" /> - 1].
        /// </summary>
        /// <param name="index">The index to wrap within the circular range.</param>
        /// <param name="size">The size of the range. Must be greater than zero.</param>
        /// <returns>The circular index, guaranteed to be in the range [0, <paramref name="size" /> - 1].</returns>
        private int GetCircularIndex(int index, int size)
        {
            // Double Modulo Operation:
            // 1) First modulo (index % size) handles wrapping but may be negative
            // 2) Adding size ensures result is positive
            // 3) Second modulo normalizes to range [0, size-1]
            return ((index % size) + size) % size;
        }

#region Unity Methods

        private void OnValidate()
        {
            // Re-assign the value to validate constraints through the property setter
            visibleElements = m_VisibleElements;

            if (m_CellPrefab != null)
            {
                var carouselCell = m_CellPrefab.GetComponent<ICarouselCell<TData>>();
                if (carouselCell == null)
                {
                    throw new NullReferenceException(
                        $"'{nameof(m_CellPrefab)}' is missing component that implements '{nameof(ICarouselCell<TData>)}'");
                }
            }
        }

        private void Awake()
        {
            // Hide the prefab if it's a scene GameObject rather than a prefab asset
            // (IsValid returns false for prefab references from project, true for scene objects)
            if (m_CellPrefab.gameObject.scene.IsValid())
                m_CellPrefab.gameObject.SetActive(false);

            RebuildCells();
        }

        private void OnEnable()
        {
            if (m_DeltaDragHandler == null)
            {
                m_DeltaDragHandler = GetComponent<IDeltaDragHandler>();
                if (m_DeltaDragHandler == null)
                    throw new NullReferenceException($"{nameof(m_DeltaDragHandler)} cannot be null");
            }

            if (m_CarouselCellLayoutHandler == null)
            {
                m_CarouselCellLayoutHandler = GetComponent<ICarouselCellLayoutHandler>();
                if (m_CarouselCellLayoutHandler == null)
                    throw new NullReferenceException($"{nameof(m_CarouselCellLayoutHandler)} cannot be null");
            }
        }

        public void Update()
        {
            UpdateCells();

            if (Mathf.Approximately(m_CenterIndex, m_TargetCenterIndex))
                return;

            m_CenterIndex = Mathf.SmoothDamp(
                m_CenterIndex, m_TargetCenterIndex,
                ref m_CenterSmoothVelocity, m_CenterSmoothTime,
                m_CenterSmoothTime * 4);

            if (Mathf.Abs(m_CenterSmoothVelocity)              < 0.01f &&
                Mathf.Abs(m_TargetCenterIndex - m_CenterIndex) < 0.01f)
            {
                m_TargetCenterIndex    = GetCircularIndex(Mathf.RoundToInt(m_TargetCenterIndex), m_Data.Count);
                m_CenterIndex          = m_TargetCenterIndex;
                m_CenterSmoothVelocity = 0f;
                InvokeOnCenterChangedWithCenterIndex();
            }
        }

#endregion

#region Drag Handlers

        public void OnBeginDrag(PointerEventData eventData) => m_CenterIndex = GetCenterCell().index;

        public void OnEndDrag(PointerEventData eventData)
        {
            var centerCell = GetCenterCell();
            m_TargetCenterIndex = centerCell.index;

            // Get current cell position to smoothly animate to the nearest integer index,
            // since carousel is still in motion and offset may be fractional
            m_CenterIndex = m_TargetCenterIndex - centerCell.offsetFromCenter;
        }

#endregion

        /// <summary>
        ///     Updates the properties and layout of all cells in the carousel view based on their position relative to the center.
        /// </summary>
        private void UpdateCells()
        {
            if (transform.childCount == 0)
            {
                RebuildCells();
                return;
            }

            var node = m_CellPool.First;

            while (node != null)
            {
                // Cache the next node reference before modifying the linked list to maintain iteration integrity
                var nodeNext = node.Next;
                node.Value.offsetFromCenter = node.Value.index - m_CenterIndex + m_DeltaDragHandler.totalDelta.x;

                // Detecting overflow: Cell is outside the allowed range
                if (Mathf.Round(node.Value.offsetFromCenterAbs) > depth)
                {
                    // Positive: Rightmost element needs to be moved to front
                    if (node.Value.offsetFromCenter > 0)
                    {
                        m_CellPool.Remove(node);
                        m_CellPool.AddFirst(node);
                        node.Value.index = node.Next!.Value.index - 1;
                    }

                    // Negative: Leftmost element needs to be moved to back
                    if (node.Value.offsetFromCenter < 0)
                    {
                        m_CellPool.Remove(node);
                        m_CellPool.AddLast(node);
                        node.Value.index = node.Previous!.Value.index + 1;
                    }
                }

                // Add visibility margin to smooth transition of elements entering/leaving view
                node.Value.gameObject.SetActive(
                    node.Value.offsetFromCenterAbs < depthMinusMargin
                );

                if (m_Data.Count > 0)
                {
                    int dataIndex = GetCircularIndex(node.Value.index, m_Data.Count);
                    if (node.Value.data != m_Data[dataIndex])
                    {
                        node.Value.data = m_Data[dataIndex];
                    }
                }

                m_CarouselCellLayoutHandler.UpdateLayout(node.Value);
                node = nodeNext;
            }

            // Set sibling indices to control render order:
            // - Center cell should be rendered last (frontmost)
            // - Cells further from center are rendered earlier (further back)
            // - Higher sibling index = rendered later = appears in front
            var cellsOrderedByOffset = m_CellPool
                .OrderBy(t => t.offsetFromCenterAbs)
                .ToArray();

            for (int i = 0; i < cellsOrderedByOffset.Length; i++)
            {
                cellsOrderedByOffset[i].rectTransform.SetSiblingIndex(cellsOrderedByOffset.Length - 1 - i);
            }
        }

        internal override void RebuildCells(bool force = false)
        {
            if (m_CellPrefab == null)
            {
                Debug.LogError($"Cannot build cells while '{nameof(m_CellPrefab)}' is null");
                return;
            }

            if (!force && transform.childCount == poolSize)
                return;

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            m_CellPool.Clear();
            for (int i = 0; i < poolSize; i++)
            {
                var prefabInstance = Instantiate(m_CellPrefab.gameObject, transform);
                prefabInstance.SetActive(true);

                var rectTransform = prefabInstance.transform as RectTransform;
                if (rectTransform == null)
                    throw new NullReferenceException($"[{i}] RectTransform is null");

                rectTransform.ResetToMiddleCenter();

                if (!Application.isPlaying)
                {
                    // Ensure hideFlags are properly set on all child objects since Unity Editor
                    // doesn't always propagate flags from root GameObject
                    foreach (var child in prefabInstance.GetComponentsInChildren<Transform>(true))
                    {
                        child.gameObject.hideFlags = HideFlags.NotEditable | HideFlags.DontSaveInEditor;
                    }
                }

                var cell = prefabInstance.GetComponent<CarouselCell<TData>>();
                cell.index          = i   - 1;
                prefabInstance.name = "[" + cell.index + "]";
                m_CellPool.AddLast(cell);

                if (m_Data.Count > 0)
                {
                    int dataIndex = GetCircularIndex(cell.index, m_Data.Count);
                    cell.data = m_Data[dataIndex];
                }
            }

            m_CenterIndex = m_TargetCenterIndex = GetCenterCell().index;
            OnEnable();
            UpdateCells();
            InvokeOnCenterChangedWithCenterIndex();
        }

#region Public Methods

        /// <summary>
        ///     Adds a single item to the carousel.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(TData item)
        {
            if (item == null)
            {
                Debug.LogError($"Passed '{nameof(item)}' is null");
                return;
            }

            m_Data.Add(item);
        }

        /// <summary>
        ///     Adds multiple items to the carousel.
        /// </summary>
        /// <param name="items">The collection of items to add.</param>
        public void Add(IReadOnlyList<TData> items)
        {
            if (items == null)
            {
                Debug.LogError($"Passed '{nameof(items)}' is null");
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null)
                {
                    Debug.LogWarning($"Skipping 'cell' at index '{i}', because it is null");
                    continue;
                }

                m_Data.Add(items[i]);
            }
        }

        /// <summary>
        ///     Inserts an item at the specified index in the carousel.
        /// </summary>
        /// <param name="index">The index at which to insert the item.</param>
        /// <param name="item">The item to insert.</param>
        public void Insert(int index, TData item)
        {
            if (index < 0 || index >= m_Data.Count)
            {
                Debug.LogError($"Passed '{nameof(index)}' {index} is out of range");
                return;
            }

            if (item == null)
            {
                Debug.LogError($"Passed '{nameof(item)}' is null");
                return;
            }

            m_Data.Insert(index, item);
        }

        /// <summary>
        ///     Inserts multiple items at the specified index in the carousel.
        /// </summary>
        /// <param name="index">The index at which to insert the items.</param>
        /// <param name="items">The collection of items to insert.</param>
        public void Insert(int index, IReadOnlyList<TData> items)
        {
            if (index < 0 || index >= m_Data.Count)
            {
                Debug.LogError($"Passed '{nameof(index)}' {index} is out of range");
                return;
            }

            if (items == null)
            {
                Debug.LogError($"Passed '{nameof(items)}' is null");
                return;
            }

            var dataNoNull = items.Where(d => d != null);
            m_Data.InsertRange(index, dataNoNull);
        }

        /// <summary>
        ///     Removes all items from the carousel.
        /// </summary>
        public void RemoveAll()
        {
            m_Data.Clear();
            RebuildCells(true);
        }

        /// <summary>
        ///     Removes a specific item from the carousel.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if the item was successfully removed; otherwise, false.</returns>
        public bool Remove(TData item)
        {
            if (item == null)
            {
                Debug.LogError($"Passed '{nameof(item)}' is null");
                return false;
            }

            return m_Data.Remove(item);
        }

        /// <summary>
        ///     Centers the carousel on the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item to center.</param>
        /// <param name="animated">If true, animates the centering movement.</param>
        public void Center(int index, bool animated)
        {
            if (m_Data.Count == 0)
            {
                Debug.LogWarning("No items in list");
                return;
            }

            index = GetCircularIndex(index, m_Data.Count);

            if (animated)
            {
                m_TargetCenterIndex = index;
                // OnCenterChanged will be invoked when upon the animation ending
            }
            else
            {
                m_CenterIndex = m_TargetCenterIndex = index;
                InvokeOnCenterChangedWithCenterIndex();
            }
        }

        /// <summary>
        ///     Centers the carousel on the specified item.
        /// </summary>
        /// <param name="item">The item to center.</param>
        /// <param name="animated">If true, animates the centering movement.</param>
        public void Center(TData item, bool animated)
        {
            if (item == null)
            {
                Debug.LogError($"Passed '{nameof(item)}' is null");
                return;
            }

            int index = m_Data.IndexOf(item);
            if (index > 0)
            {
                Center(index, animated);
            }
            else
            {
                Debug.LogWarning($"Passed '{nameof(item)}' not present in list");
            }
        }

        /// <summary>
        ///     Centers the carousel on the next item.
        /// </summary>
        /// <param name="animated">If true, animates the centering movement.</param>
        public void CenterNext(bool animated)
        {
            if (m_Data.Count == 0)
            {
                Debug.LogWarning("No items in list");
                return;
            }

            int index = GetCircularIndex(Mathf.RoundToInt(m_TargetCenterIndex), m_Data.Count);
            Center(index + 1, animated);
        }

        /// <summary>
        ///     Centers the carousel on the previous item.
        /// </summary>
        /// <param name="animated">If true, animates the centering movement.</param>
        public void CenterPrevious(bool animated)
        {
            if (m_Data.Count == 0)
            {
                Debug.LogWarning("No items in list");
                return;
            }

            int index = GetCircularIndex(Mathf.RoundToInt(m_TargetCenterIndex), m_Data.Count);
            Center(index - 1, animated);
        }

#endregion
    }
}
