using System;
using System.Collections.Generic;
using System.Linq;
using Steft.SimpleCarousel.Drag;
using Steft.SimpleCarousel.Layout;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Steft.SimpleCarousel
{
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public class CarouselView<TData> : MonoBehaviour, IBeginDragHandler, IEndDragHandler
        where TData : class, ICarouselData
    {
        // TODO
        //  - cleanup .scene file and remove redundant sfield entries
        //  - add tooltip attributes to all sfields
        //  - "OnValidate"
        //  - refactor "Awake"
        //  - stress test scrolling / swiping to avoid it malfunctioning
        //  - test on device
        //  - add "Tests"
        //  - add "developer API"
        //  - decide if we want SmoothDamp towards center after drag ends
        //  - "CustomEditor" for CarouselView if possible: warmup/preload all prefab instances (AssemblyInfo InternalsVisibleTo?)

        [Min(3), SerializeField]               private int   m_VisibleElements  = 3;
        [Range(0.0001f, 20f)] [SerializeField] private float m_CenterSmoothTime = 0.2f;

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

        private float depthMinusMargin => depth - 0.5f;

        private int depth => (poolSize - 1) / 2;

        private int poolSize => m_VisibleElements + 2;

        private float GetCenterIndex()
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

            return GetCircularIndex(center.Value.index, m_Data.Length);
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
            m_DeltaDragHandler          = GetComponent<IDeltaDragHandler>();
            m_CarouselCellLayoutHandler = GetComponent<ICarouselCellLayoutHandler<ICarouselCell>>();

            // TODO this will be refactored as soon as we implement pooling
            if (m_CellPrefab != null)
            {
                Debug.Log("Awake");

                if (transform.childCount != poolSize)
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
                        // rectTransform.gameObject.name = $"[{i:000}] Element '{m_Data[i].name}'";

                        // prefabInstance.hideFlags       = HideFlags.NotEditable;
                        // prefabInstance.hideFlags = HideFlags.DontSave;
                        var carouselCell = prefabInstance.GetComponent<CarouselCell<TData>>();
                        carouselCell.index  = i   - 1;
                        prefabInstance.name = "[" + carouselCell.index + "]";
                        m_CarouselCells.AddLast(carouselCell);
                        // m_CarouselCells[i]       = prefabInstance.GetComponent<CarouselCell<TData>>();
                        // m_CarouselCells[i].data  = m_Data[i];
                        // m_CarouselCells[i].index = i;
                    }

                    m_CenterIndex = m_TargetCenterIndex = GetCenterIndex();
                }
            }
        }

        public void Update()
        {
            UpdateCells();

            // if (Mathf.Approximately(m_CenterIndex, m_TargetCenterIndex))
            //     return;
            //
            // m_CenterIndex = Mathf.SmoothDamp(
            //     m_CenterIndex, m_TargetCenterIndex, ref m_CenterSmoothVelocity, m_CenterSmoothTime, 10);
            //
            // if (Mathf.Abs(m_CenterSmoothVelocity)              < 0.01f &&
            //     Mathf.Abs(m_TargetCenterIndex - m_CenterIndex) < 0.01f)
            // {
            //     m_CenterIndex          = m_TargetCenterIndex;
            //     m_CenterSmoothVelocity = 0f;
            // }
        }

#endregion

        public void OnBeginDrag(PointerEventData eventData) => m_CenterIndex = GetCenterIndex();

        public void OnEndDrag(PointerEventData eventData) =>
            m_CenterIndex = m_TargetCenterIndex = GetCenterIndex();
        // m_TargetCenterIndex = GetCenterIndex();

        private void UpdateCells()
        {
            if (transform.childCount == 0)
                return;

            var node = m_CarouselCells.First;
            var first = m_CarouselCells.First;
            var last = m_CarouselCells.Last;

            // there is a symmetrical buffer of 1 element to the left/right
            // hence we can at most "overflow" 1 element at a time
            bool overflowedHandled = false;

            while (node != null)
            {
                var nodeNext = node.Next; // caching next node before any changes to the linked list
                node.Value.offsetFromCenter = node.Value.index - m_CenterIndex + m_DeltaDragHandler.delta;

                // detecting overflow: cell is outside the allowed range
                if (!overflowedHandled && Mathf.Round(node.Value.offsetFromCenterAbs) > depth)
                {
                    // positive: rightmost element needs to be moved to front
                    if (node.Value.offsetFromCenter > 0)
                    {
                        m_CarouselCells.Remove(node);
                        m_CarouselCells.AddFirst(node);
                        node.Value.index = first.Value.index - 1;
                    }

                    // negative: leftmost element needs to be moved to back
                    if (node.Value.offsetFromCenter < 0)
                    {
                        m_CarouselCells.Remove(node);
                        m_CarouselCells.AddLast(node);
                        node.Value.index = last.Value.index + 1;
                    }

                    overflowedHandled = true;
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
                    node.Value.gameObject.name =
                        $"[{node.Value.index:000} <> {dataIndex:000}] Element '{m_Data[dataIndex].name}'";
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
    }
}
