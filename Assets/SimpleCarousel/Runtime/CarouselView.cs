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
    public abstract class CarouselView : MonoBehaviour
    {
        internal abstract void RebuildCells(bool force = false);
    }

    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public class CarouselView<TData> : CarouselView, IBeginDragHandler, IEndDragHandler
        where TData : class, ICarouselData
    {
        // TODO
        //  - element edge closer to the screen does not appear longer than element edge further away from the screen
        //      how should we proceed? change everything to 3d?
        //  - "OnValidate"
        //  - refactor "Awake"
        //  - stress test scrolling / swiping to avoid it malfunctioning
        //  - add "developer API"
        //  - "CustomEditor" for CarouselView if possible: warmup/preload all prefab instances (AssemblyInfo InternalsVisibleTo?)
        //  - cleanup .scene file and remove redundant sfield entries
        //  - add tooltip attributes to all sfields
        //  - add "summaries"
        //  - test on device
        //  - add "Tests"
        //  - decide if we want SmoothDamp towards center after drag ends

        [SerializeField] private UnityEvent<TData> m_OnCenterChanged = new();

        [Space, Min(3), SerializeField]       private int   m_VisibleElements  = 3;
        [Range(0.0001f, 20f), SerializeField] private float m_CenterSmoothTime = 0.2f;

        [SerializeField] private CarouselCell<TData> m_CellPrefab;
        [SerializeField] private List<TData>         m_Data = new(32);

        private readonly LinkedList<CarouselCell<TData>> m_CellPool = new();

        // note we could implement some dependency injection here,
        // that for example looks for the presence of respective interface implementations on the local GameObject
        // this has the benefit of throwing these setup related errors already when compiling instead of throwing during runtime
        private IDeltaDragHandler          m_DeltaDragHandler;
        private ICarouselCellLayoutHandler m_CarouselCellLayoutHandler;

        private float m_CenterIndex;
        private float m_TargetCenterIndex;
        private float m_CenterSmoothVelocity;

        private float depthMinusMargin => depth - 0.5f;

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

        internal IReadOnlyList<TData> data => m_Data;

        internal int depth => (poolSize - 1) / 2;

        internal int poolSize => m_VisibleElements + 2;

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

        public UnityEvent<TData> onCenterChanged => m_OnCenterChanged;

        private void InvokeOnCenterChangedWithCenterIndex()
        {
            if (m_Data.Count == 0)
                return;

            int index = GetCircularIndex(Mathf.RoundToInt(m_TargetCenterIndex), m_Data.Count);
            Debug.Log($"Center is '{m_Data[index].name}' at index '{index}'");
            onCenterChanged.Invoke(m_Data[index]);
        }

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

        private int GetCircularIndex(int index, int size)
        {
            // Double Modulo Trick: ((index % size) + size) % size
            // 1. index % size: can be negative if index is negative
            // 2. + size: ensures the value is >= 0 before the final modulo
            // 3. % size: brings the value back into the [0, size - 1] range
            return ((index % size) + size) % size;
        }

#region Unity Methods

        private void OnValidate()
        {
            // re-assigning will check all constraints defined through the property
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
            // if the prefab is some GameObject that is part of the scene, we don't want it to be visible
            // IsValid will return false, if a GameObject from the Hierarchy is referenced
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
                m_CenterIndex, m_TargetCenterIndex, ref m_CenterSmoothVelocity, m_CenterSmoothTime, 10);

            if (Mathf.Abs(m_CenterSmoothVelocity)              < 0.01f &&
                Mathf.Abs(m_TargetCenterIndex - m_CenterIndex) < 0.01f)
            {
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

            // we are still in the process of moving the carousel,
            // so the offsetFromCenter will not be an integer value yet
            m_CenterIndex = m_TargetCenterIndex - centerCell.offsetFromCenter;
        }

#endregion

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
                var nodeNext = node.Next; // caching next node before any changes to the linked list
                node.Value.offsetFromCenter = node.Value.index - m_CenterIndex + m_DeltaDragHandler.totalDelta;

                // detecting overflow: cell is outside the allowed range
                if (Mathf.Round(node.Value.offsetFromCenterAbs) > depth)
                {
                    // positive: rightmost element needs to be moved to front
                    if (node.Value.offsetFromCenter > 0)
                    {
                        m_CellPool.Remove(node);
                        m_CellPool.AddFirst(node);
                        node.Value.index = node.Next!.Value.index - 1;
                    }

                    // negative: leftmost element needs to be moved to back
                    if (node.Value.offsetFromCenter < 0)
                    {
                        m_CellPool.Remove(node);
                        m_CellPool.AddLast(node);
                        node.Value.index = node.Previous!.Value.index + 1;
                    }
                }

                // elements outside the allowed range are invisible
                // "MinusMargin" makes sure elements are visible longer than necessary,
                // resulting in a nicer UX
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

            // sibling order determines render order
            // center is considered the first layer; its neighbours the second layer, etc.
            // last sibling is rendered last; hence the last sibling is ultimately in front
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

                // this method might be called from the CarouselViewEditor
                if (!Application.isPlaying)
                {
                    // sometimes the Editor does not set recursively set hideFlags when applied to root
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

        public void Add(TData item)
        {
            if (item == null)
            {
                Debug.LogError($"Passed '{nameof(item)}' is null");
                return;
            }

            m_Data.Add(item);
        }

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

        public void RemoveAll()
        {
            m_Data.Clear();
            RebuildCells(true);
        }

        public bool Remove(TData item)
        {
            if (item == null)
            {
                Debug.LogError($"Passed '{nameof(item)}' is null");
                return false;
            }

            return m_Data.Remove(item);
        }

        public void Center(int index, bool animated)
        {
            if (index < 0 || index >= m_Data.Count)
            {
                Debug.LogError($"Passed '{nameof(index)}' {index} is out of range");
                return;
            }

            if (animated)
            {
                m_TargetCenterIndex = index;
            }
            else
            {
                m_CenterIndex = m_TargetCenterIndex = index;
            }

            InvokeOnCenterChangedWithCenterIndex();
        }

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

#endregion
    }
}
