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
        internal abstract void RebuildCells();
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

        [SerializeField] private UnityEvent<TData> m_OnCenterChanged;

        [Space, Min(3), SerializeField]       private int   m_VisibleElements  = 3;
        [Range(0.0001f, 20f), SerializeField] private float m_CenterSmoothTime = 0.2f;

        [SerializeField] private CarouselCell<TData> m_CellPrefab;
        [SerializeField] private TData[]             m_Data = Array.Empty<TData>();

        private readonly LinkedList<CarouselCell<TData>> m_CarouselCells = new();

        // note we could implement some dependency injection here,
        // that for example looks for the presence of respective interface implementations on the local GameObject
        // this has the benefit of throwing these setup related errors already when compiling instead of throwing during runtime
        private IDeltaDragHandler                         m_DeltaDragHandler;
        private ICarouselCellLayoutHandler<ICarouselCell> m_CarouselCellLayoutHandler;

        private float m_CenterIndex;
        private float m_TargetCenterIndex;
        private float m_CenterSmoothVelocity;

        private float depthMinusMargin => depth - 0.5f;

        private int depth => (poolSize - 1) / 2;

        private int poolSize => m_VisibleElements + 2;

        public int visibleElements
        {
            get => m_VisibleElements;
            set
            {
                // constraints:
                // - minimum is 3
                // - must be an odd number

                if (value < 3)
                {
                    m_VisibleElements = 3;
                    return;
                }

                if (value % 2 == 0)
                {
                    // minimum even number is 4
                    m_VisibleElements = value - 1;
                    return;
                }

                m_VisibleElements = value;
            }
        }

        public UnityEvent<TData> onCenterChanged => m_OnCenterChanged;

        private CarouselCell<TData> GetCenterCell()
        {
            var center = m_CarouselCells.First;
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
        }

        private void Awake()
        {
            // if the prefab is some GameObject that is part of the scene, we don't want it to be visible
            // IsValid will return false, if a GameObject from the Hierarchy is referenced
            if (m_CellPrefab.gameObject.scene.IsValid())
                m_CellPrefab.gameObject.SetActive(false);

            if (m_CellPrefab         == null) return;
            if (m_Data.Length        == 0) return;
            if (transform.childCount == poolSize) return;
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
                m_CarouselCellLayoutHandler = GetComponent<ICarouselCellLayoutHandler<ICarouselCell>>();
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
                return;

            var node = m_CarouselCells.First;

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
                        m_CarouselCells.Remove(node);
                        m_CarouselCells.AddFirst(node);
                        node.Value.index = node.Next!.Value.index - 1;
                    }

                    // negative: leftmost element needs to be moved to back
                    if (node.Value.offsetFromCenter < 0)
                    {
                        m_CarouselCells.Remove(node);
                        m_CarouselCells.AddLast(node);
                        node.Value.index = node.Previous!.Value.index + 1;
                    }
                }

                // elements outside the allowed range are invisible
                // "MinusMargin" makes sure elements are visible longer than necessary,
                // resulting in a nicer UX
                node.Value.gameObject.SetActive(
                    node.Value.offsetFromCenterAbs < depthMinusMargin
                );

                int dataIndex = GetCircularIndex(node.Value.index, m_Data.Length);
                if (node.Value.data != m_Data[dataIndex])
                {
                    node.Value.data = m_Data[dataIndex];
                }

                m_CarouselCellLayoutHandler.UpdateLayout(node.Value);
                node = nodeNext;
            }

            // sibling order determines render order
            // center is considered the first layer; its neighbours the second layer, etc.
            // last sibling is rendered last; hence the last sibling is ultimately in front
            var cellsOrderedByOffset = m_CarouselCells
                .OrderBy(t => t.offsetFromCenterAbs)
                .ToArray();

            for (int i = 0; i < cellsOrderedByOffset.Length; i++)
            {
                cellsOrderedByOffset[i].rectTransform.SetSiblingIndex(cellsOrderedByOffset.Length - 1 - i);
            }
        }

        internal override void RebuildCells()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            m_CarouselCells.Clear();
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

                var carouselCell = prefabInstance.GetComponent<CarouselCell<TData>>();
                carouselCell.index  = i   - 1;
                prefabInstance.name = "[" + carouselCell.index + "]";
                m_CarouselCells.AddLast(carouselCell);

                if (m_Data.Length > 0)
                {
                    int dataIndex = GetCircularIndex(carouselCell.index, m_Data.Length);
                    carouselCell.data = m_Data[dataIndex];
                }
            }

            m_CenterIndex = m_TargetCenterIndex = GetCenterCell().index;
            OnEnable();
            UpdateCells();
        }

        public void AddLast(CarouselCell<TData> cell)
        {
            if (cell == null)
            {
                Debug.Log($"Passed '{nameof(cell)}' is null");
                return;
            }

            m_CarouselCells.AddLast(cell);
        }

        public void AddLast(IReadOnlyList<CarouselCell<TData>> cells)
        {
            if (cells == null)
            {
                Debug.Log($"Passed '{nameof(cells)}' is null");
                return;
            }

            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i] == null)
                {
                    Debug.LogWarning($"Skipping 'cell' at index '{i}', because it is null");
                    continue;
                }

                m_CarouselCells.AddLast(cells[i]);
            }
        }

        public void AddAfter(CarouselCell<TData> cellAfter, CarouselCell<TData> cellNew)
        {
            if (cellAfter == null || cellNew == null)
            {
                Debug.Log($"Either '{nameof(cellAfter)}' or '{nameof(cellNew)}' is null");
                return;
            }

            if (m_CarouselCells.Find(cellAfter) is { } node)
            {
                m_CarouselCells.AddAfter(node, cellNew);
            }
            else
            {
                Debug.LogWarning($"Cannot add '{cellNew.name}', because '{cellAfter.name}' not found");
            }
        }

        public void AddAfter(CarouselCell<TData> cellAfter, IReadOnlyList<CarouselCell<TData>> cells)
        {
            if (cellAfter == null || cells == null)
            {
                Debug.Log($"Either '{nameof(cellAfter)}' or '{nameof(cells)}' is null");
                return;
            }

            if (m_CarouselCells.Find(cellAfter) is { } node)
            {
                for (int i = cells.Count; i >= 0; i--)
                {
                    m_CarouselCells.AddAfter(node, cells[i]);
                }
            }
            else
            {
                Debug.LogWarning($"Cannot add '{nameof(cells)}', because '{cellAfter.name}' not found");
            }
        }

        public void RemoveAll()
        {
            foreach (var cell in m_CarouselCells)
            {
                Destroy(cell.gameObject);
            }

            m_CarouselCells.Clear();
        }

        public bool Remove(CarouselCell<TData> cell)
        {
            if (cell == null)
            {
                Debug.Log($"Passed '{nameof(cell)}' is null");
                return false;
            }

            return m_CarouselCells.Remove(cell);
        }
    }
}
