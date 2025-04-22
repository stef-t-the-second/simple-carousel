using System;
using System.Collections.Generic;
using System.Linq;
using Steft.SimpleCarousel.Drag;
using Steft.SimpleCarousel.Layout;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Steft.SimpleCarousel
{
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public class CarouselView<TData> : MonoBehaviour, IBeginDragHandler, IEndDragHandler
        where TData : class, ICarouselData
    {
        // TODO cleanup .scene file and remove redundant sfield entries
        // TODO add tooltip attributes to all sfields

        [Min(3), SerializeField] private int m_VisibleElements = 3;

        [SerializeField] private CarouselCell<TData> m_CellPrefab;
        [SerializeField] private TData[]             m_Data = Array.Empty<TData>();

        private readonly LinkedList<CarouselCell<TData>> m_CarouselCells = new();

        // note we could implement some dependency injection here,
        // that for example looks for the presence of respective interface implementations on the local GameObject
        // this has the benefit of throwing these setup related errors already when compiling instead of throwing during runtime
        private ISteppedSmoothDragHandler                 m_SteppedDragHandler;
        private ICarouselCellLayoutHandler<ICarouselCell> m_CarouselCellLayoutHandler;

        private int m_MovingCircularDataIndex;

        private int mappedCircularDataIndex
        {
            get
            {
                // Double Modulo Trick: ((index % size) + size) % size
                // 1. index % size: Can be negative if index is negative.
                // 2. + size: Ensures the value is >= 0 before the final modulo.
                // 3. % size: Brings the value back into the [0, size - 1] range.
                int mappedIndex = ((m_MovingCircularDataIndex % data.Length) + data.Length) % data.Length;
                return mappedIndex;
            }
        }

        public TData[] data
        {
            get => m_Data;
            set => m_Data = value ?? Array.Empty<TData>();
        }

        public int displayedElements
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

#region Unity Methods

        private void OnValidate()
        {
            // re-assigning will check all constraints defined through the property
            displayedElements = m_VisibleElements;
        }

        private void Awake()
        {
            m_SteppedDragHandler = GetComponent<ISteppedSmoothDragHandler>();
            m_SteppedDragHandler.Init(depth, poolSize - 1);
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

                    UpdateCenterStartIndex();
                }
            }
        }

        public void Update()
        {
            UpdateCells(m_SteppedDragHandler.currentScrollIndex);
        }

#if UNITY_EDITOR
        // private void OnValidate()
        // {
        //     // TODO this will be refactored as soon as we implement pooling
        //     if (m_PrefabElement != null)
        //     {
        //         Debug.Log("OnValidate");
        //
        //         // is it a GameObject reference of an instance in the scene?
        //         // is it a Prefab reference?
        //         if (m_PrefabElement.scene.IsValid())
        //         {
        //             Debug.LogWarning($"Prefab reference required, but was GameObject instance: {m_PrefabElement.name}");
        //             return;
        //         }
        //
        //         if (transform.childCount   != m_NumberDisplayedElements ||
        //             m_CarouselCells.Length != m_NumberDisplayedElements)
        //         {
        //             var currentChildren = transform.GetChildren();
        //             m_CarouselCells = new SimpleCarouselCell[m_NumberDisplayedElements];
        //
        //             for (int i = 0; i < m_NumberDisplayedElements; i++)
        //             {
        //                 // TODO is there any way to squash the "SendMessage" warning when instantiating a Prefab?
        //                 var prefabInstance =
        //                     (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(m_PrefabElement, transform);
        //                 var rectTransform = prefabInstance.transform as RectTransform;
        //                 rectTransform.ResetToMiddleCenter();
        //                 rectTransform.gameObject.name += " (" + i + ")";
        //
        //                 // prefabInstance.hideFlags       = HideFlags.NotEditable;
        //                 // prefabInstance.hideFlags = HideFlags.DontSave;
        //                 m_CarouselCells[i] = new SimpleCarouselCell(i, rectTransform);
        //             }
        //
        //             // unfortunately destroying GameObjects in OnValidate is not straightforward
        //             // https://discussions.unity.com/t/onvalidate-and-destroying-objects/544819
        //             UnityEditor.EditorApplication.delayCall += () =>
        //             {
        //                 foreach (Transform child in currentChildren)
        //                 {
        //                     // sanity check to avoid destroying something we don't want destroyed
        //                     if (child != null && child.parent != transform)
        //                         continue;
        //
        //                     DestroyImmediate(child.gameObject);
        //                 }
        //             };
        //         }
        //     }
        //
        //     m_CurrentScrollPosition = m_NewTargetScrollPosition;
        //
        //     // TODO remove from OnValidate later one
        //     UpdateLayout();
        // }

#endif

#endregion

        private int m_CenterStartIndex;

        private void UpdateCenterStartIndex()
        {
            var center = m_CarouselCells.First;
            for (int i = 0; i < depth; i++)
            {
                center = center.Next;
            }

            m_CenterStartIndex = center.Value.index;
            Debug.Log($"{nameof(m_CenterStartIndex)}: {m_CenterStartIndex}");
        }

        public void OnBeginDrag(PointerEventData eventData) => UpdateCenterStartIndex();

        public void OnEndDrag(PointerEventData eventData) => UpdateCenterStartIndex();

        private void UpdateCells(float currentScrollIndex)
        {
            if (transform.childCount == 0)
                return;

            // Debug.Log($"{m_SteppedDragHandler.traveledDelta:F2}");
            var node = m_CarouselCells.First;
            var first = m_CarouselCells.First;
            var last = m_CarouselCells.Last;

            while (node != null)
            {
                var nextNode = node.Next;
                node.Value.offsetFromCenter =
                    node.Value.index - m_CenterStartIndex - m_SteppedDragHandler.traveledDelta;

                // detecting overflow: cell is outside the allowed range
                if (Mathf.Round(node.Value.offsetFromCenterAbs) > depth)
                {
                    // shift overflowed cell to left (negative) or right (positive)?
                    if (node.Value.offsetFromCenter > 0)
                    {
                        Debug.Log($"Shifting {node.Value.index} to front");
                        // // m_CarouselCells[i].offsetFromCenter = i - m_CarouselCells.Length - currentScrollIndex;
                        m_CarouselCells.Remove(node);
                        m_CarouselCells.AddFirst(node);
                        node.Value.index = first.Value.index - 1;
                    }
                    else
                    {
                        Debug.Log($"Shifting {node.Value.index} to back");
                        // // m_CarouselCells[i].offsetFromCenter = i + m_CarouselCells.Length - currentScrollIndex;
                        m_CarouselCells.Remove(node);
                        m_CarouselCells.AddLast(node);
                        node.Value.index = last.Value.index + 1;
                    }

                    // m_CarouselCells[i].index = Mathf.RoundToInt(m_CarouselCells[i].offsetFromCenter + depth);
                }

                node.Value.gameObject.SetActive(
                    node.Value.offsetFromCenterAbs < depthMinusMargin
                );

                int mappedDataIndex =
                (
                    (
                        (
                            node.Value.index)
                        % data.Length
                    )
                    + data.Length
                ) % data.Length;

                // if (m_CarouselCells[i].data != m_Data[mappedDataIndex])
                {
                    node.Value.data = m_Data[mappedDataIndex];
                    node.Value.gameObject.name =
                        $"[{node.Value.index:000} <> {mappedDataIndex:000}] Element '{m_Data[mappedDataIndex].name}'";
                }

                // Debug.Log($"{i} = {mappedDataIndex}; {m_SteppedDragHandler.traveledScrollIndizes}");

                m_CarouselCellLayoutHandler.UpdateLayout(node.Value);
                node = nextNode;
            }
            // for (int i = 0; i < m_CarouselCells.Length; i++)
            // {
            //     m_CarouselCells[i].offsetFromCenter = i - currentScrollIndex;
            //
            //     // detecting overflow: cell is outside the allowed range
            //     if (Mathf.RoundToInt(m_CarouselCells[i].offsetFromCenterAbs) > depth)
            //     {
            //         // shift overflowed cell to left (negative) or right (positive)?
            //         if (m_CarouselCells[i].offsetFromCenter > 0)
            //         {
            //             m_CarouselCells[i].offsetFromCenter = i - m_CarouselCells.Length - currentScrollIndex;
            //         }
            //         else
            //         {
            //             m_CarouselCells[i].offsetFromCenter = i + m_CarouselCells.Length - currentScrollIndex;
            //         }
            //
            //         // m_CarouselCells[i].index = Mathf.RoundToInt(m_CarouselCells[i].offsetFromCenter + depth);
            //     }
            //
            //     m_CarouselCells[i].gameObject.SetActive(
            //         m_CarouselCells[i].offsetFromCenterAbs < depthMinusMargin
            //     );
            //
            //     int mappedDataIndex =
            //     (
            //         (
            //             (
            //                 // Mathf.RoundToInt(m_SteppedDragHandler.traveledScrollIndizes)
            //                 -1 + m_CarouselCells[i].index)
            //             % data.Length
            //         )
            //         + data.Length
            //     ) % data.Length;
            //
            //     // if (m_CarouselCells[i].data != m_Data[mappedDataIndex])
            //     {
            //         m_CarouselCells[i].data = m_Data[mappedDataIndex];
            //         m_CarouselCells[i].gameObject.name =
            //             $"[{m_CarouselCells[i].index:000} <> {mappedDataIndex:000}] Element '{m_Data[mappedDataIndex].name}'";
            //     }
            //
            //     // Debug.Log($"{i} = {mappedDataIndex}; {m_SteppedDragHandler.traveledScrollIndizes}");
            //
            //     m_CarouselCellLayoutHandler.UpdateLayout(m_CarouselCells[i]);
            // }

            // sibling order determines render order
            // center is considered the first layer; its neighbours the second layer, etc.
            // last sibling is rendered last; hence the last sibling is ultimately in front
            // TODO maybe we can find a better way to solve this
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
