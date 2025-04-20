using System;
using System.Linq;
using Steft.SimpleCarousel.Drag;
using Steft.SimpleCarousel.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace Steft.SimpleCarousel
{
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public class SimpleCarouselView : MonoBehaviour, ILayoutGroup
    {
        // TODO add tooltip attributes to all sfields

        [SerializeField] private GameObject m_PrefabElement;

        [Min(3), SerializeField] private int m_DisplayedElements = 3;

        [SerializeField] private int m_StartScrollPosition = 3; // index = position - 1

        private SimpleCarouselCell[] m_CarouselCells = Array.Empty<SimpleCarouselCell>();

        private ISteppedSmoothDragHandler                      m_SteppedDragHandler;
        private ICarouselCellLayoutHandler<SimpleCarouselCell> m_CarouselCellLayoutHandler;

        public int displayedElements
        {
            get => m_DisplayedElements;
            set
            {
                // constraints:
                // - minimum is 3
                // - must be an odd number

                if (value < 3)
                {
                    m_DisplayedElements = 3;
                    return;
                }

                if (value % 2 == 0)
                {
                    // minimum even number is 4
                    m_DisplayedElements = value - 1;
                    return;
                }

                m_DisplayedElements = value;
            }
        }

        private int depth => (poolSize - 1) / 2;

        private int poolSize => m_DisplayedElements + 2;

#region Unity Methods

#if UNITY_EDITOR

        private void Awake()
        {
            m_SteppedDragHandler = GetComponent<ISteppedSmoothDragHandler>();
            Debug.Log(m_SteppedDragHandler);
            m_SteppedDragHandler.Init(m_StartScrollPosition - 1, poolSize - 1);

            m_CarouselCellLayoutHandler = GetComponent<ICarouselCellLayoutHandler<SimpleCarouselCell>>();
            Debug.Log(m_CarouselCellLayoutHandler);

            Application.targetFrameRate = 10;

            // TODO this will be refactored as soon as we implement pooling
            if (m_PrefabElement != null)
            {
                Debug.Log("OnValidate");

                // is it a GameObject reference of an instance in the scene?
                // is it a Prefab reference?
                if (m_PrefabElement.scene.IsValid())
                {
                    Debug.LogWarning($"Prefab reference required, but was GameObject instance: {m_PrefabElement.name}");
                    return;
                }

                if (transform.childCount   != poolSize ||
                    m_CarouselCells.Length != poolSize)
                {
                    foreach (Transform child in transform)
                    {
                        Destroy(child.gameObject);
                    }

                    m_CarouselCells = new SimpleCarouselCell[poolSize];

                    var prefabRectTransform = m_PrefabElement.transform as RectTransform;

                    for (int i = 0; i < poolSize; i++)
                    {
                        // TODO is there any way to squash the "SendMessage" warning when instantiating a Prefab?
                        var prefabInstance =
                            (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(m_PrefabElement, transform);
                        var rectTransform = prefabInstance.transform as RectTransform;
                        rectTransform.ResetToMiddleCenter();
                        rectTransform.gameObject.name += " (" + i + ")";

                        // prefabInstance.hideFlags       = HideFlags.NotEditable;
                        // prefabInstance.hideFlags = HideFlags.DontSave;
                        m_CarouselCells[i] = new SimpleCarouselCell(
                            i, rectTransform,
                            prefabRectTransform.rect.width, prefabRectTransform.rect.height);
                    }
                }
            }

            // TODO remove from OnValidate later one
            UpdateCells(m_StartScrollPosition);
        }

        public void Update()
        {
            UpdateCells(m_SteppedDragHandler.currentScrollIndex);
        }

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

#region Layout Group

        private void UpdateCells(float currentScrollIndex)
        {
            if (transform.childCount == 0)
                return;

            for (int i = 0; i < m_CarouselCells.Length; i++)
            {
                m_CarouselCells[i].offsetFromCenter = m_CarouselCells[i].carouselIndex - currentScrollIndex;

                // detecting overflow: cell is outside the visible range
                if (m_CarouselCells[i].offsetFromCenterAbs > depth)
                {
                    // shift overflowed cell to left or right?
                    if (m_CarouselCells[i].offsetFromCenter > 0)
                    {
                        m_CarouselCells[i].offsetFromCenter =
                            m_CarouselCells[i].carouselIndex - m_CarouselCells.Length - currentScrollIndex;
                    }
                    else
                    {
                        m_CarouselCells[i].offsetFromCenter =
                            m_CarouselCells[i].carouselIndex + m_CarouselCells.Length - currentScrollIndex;
                    }
                }

                m_CarouselCellLayoutHandler.UpdateLayout(m_CarouselCells[i]);
            }

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

        public void SetLayoutHorizontal()
        {
            // note that SetLayoutHorizontal is called BEFORE SetLayoutVertical by the auto layout system

            if (Application.isPlaying)
                UpdateCells(m_SteppedDragHandler.currentScrollIndex);
            else
                UpdateCells(m_StartScrollPosition);
        }

        public void SetLayoutVertical()
        {
            // note that SetLayoutVertical is called AFTER SetLayoutHorizontal by the auto layout system
        }

#endregion
    }
}
