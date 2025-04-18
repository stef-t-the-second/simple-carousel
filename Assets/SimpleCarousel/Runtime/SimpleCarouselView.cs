using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Steft.SimpleCarousel
{
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public class SimpleCarouselView : MonoBehaviour,
        IBeginDragHandler, IEndDragHandler, IDragHandler, ILayoutGroup
    {
        // TODO add tooltip attributes to all sfields

        // TODO do we need this to be an odd number? OnValidate: If even; +1?
        [Min(1), SerializeField] private int m_NumberDisplayedElements = 3;

        [Range(0.1f, 0.4f)] private float m_Overlap = 0.2f; // relative

        [SerializeField] private GameObject m_PrefabElement;

        private SimpleCarouselCell[] m_CarouselCells = Array.Empty<SimpleCarouselCell>();

        [Range(0, 5)] [SerializeField] private float m_NewTargetScrollPosition = 2f;

#region Unity Methods

#if UNITY_EDITOR

        private void Awake()
        {
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

                if (transform.childCount   != m_NumberDisplayedElements ||
                    m_CarouselCells.Length != m_NumberDisplayedElements)
                {
                    foreach (Transform child in transform)
                    {
                        Destroy(child.gameObject);
                    }

                    m_CarouselCells = new SimpleCarouselCell[m_NumberDisplayedElements];

                    for (int i = 0; i < m_NumberDisplayedElements; i++)
                    {
                        // TODO is there any way to squash the "SendMessage" warning when instantiating a Prefab?
                        var prefabInstance =
                            (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(m_PrefabElement, transform);
                        var rectTransform = prefabInstance.transform as RectTransform;
                        rectTransform.ResetToMiddleCenter();
                        rectTransform.gameObject.name += " (" + i + ")";

                        // prefabInstance.hideFlags       = HideFlags.NotEditable;
                        // prefabInstance.hideFlags = HideFlags.DontSave;
                        m_CarouselCells[i] = new SimpleCarouselCell(i, rectTransform);
                    }
                }
            }

            m_CurrentScrollPosition = m_NewTargetScrollPosition;

            // TODO remove from OnValidate later one
            UpdateLayout();
        }

        public void Update()
        {
            if (!Mathf.Approximately(m_TargetScrollPosition, m_NewTargetScrollPosition))
            {
                m_ScrollVelocity       = 0.0001f;
                m_TargetScrollPosition = m_NewTargetScrollPosition;
            }

            m_CurrentScrollPosition = Mathf.SmoothDamp(
                m_CurrentScrollPosition, m_TargetScrollPosition, ref m_ScrollVelocity, m_ScrollSmoothTime);

            if (!m_IsDragging && Mathf.Abs(m_ScrollVelocity)                < 0.01f &&
                Mathf.Abs(m_TargetScrollPosition - m_CurrentScrollPosition) < 0.01f)
            {
                m_CurrentScrollPosition = m_TargetScrollPosition;
                m_ScrollVelocity        = 0f;
            }

            UpdateLayout();
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

#region Drag Handlers

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("OnBeginDrag");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("OnEndDrag");
        }

        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("OnDrag");
        }

#endregion

#region Layout Group

        private void UpdateLayout()
        {
            // TODO double check that "lossyScale" is used correctly in the following

            if (transform.childCount == 0)
                return;

            float currentScrollPosition = Mathf.Clamp(m_CurrentScrollPosition, 0, m_NumberDisplayedElements - 1);
            // Debug.Log($"{nameof(currentScrollPosition)} {currentScrollPosition}");

            // instead of have an absolute overlap depending on a cells width,
            // we may implement an overlap depending on the orthogonal size of a cell on screen
            // to implement this idea, we must consider the rotation of a cell, which changes the occupied pixels on screen
            float relativeOverlap = 0.2f;
            float neighbourScale = 0.8f;
            float rotationStep = 20;
            float positionStep = 80;
            float centerWidthWorld =
                ((RectTransform)m_PrefabElement.transform).rect.width * transform.lossyScale.x;
            float centerHeightWorld =
                ((RectTransform)m_PrefabElement.transform).rect.height * transform.lossyScale.y;
            float neighbourWidthWorld =
                ((RectTransform)m_PrefabElement.transform).rect.width * transform.lossyScale.x * neighbourScale;
            float neighbourHeightWorld =
                ((RectTransform)m_PrefabElement.transform).rect.height * transform.lossyScale.y * neighbourScale;

            float cellLayers = Mathf.Max(currentScrollPosition, m_CarouselCells.Length - currentScrollPosition);
            foreach (var cell in m_CarouselCells)
            {
                int cellIndex = cell.carouselIndex;
                float offsetFromCenter = cellIndex - currentScrollPosition;
                float offsetFromCenterAbs = Mathf.Abs(offsetFromCenter);
                float posX, posY, posZ, rotY;
                if (Mathf.Approximately(offsetFromCenter, 0))
                {
                    cell.rectTransform.localScale = Vector3.one;

                    posX = 0;
                    posY = 0;
                    posZ = 0;
                    rotY = 0;
                }
                else
                {
                    int leftOrRight = offsetFromCenter > 0 ? 1 : -1;

                    float scale = offsetFromCenterAbs < 1
                        // immediate neighbours of center require transition of scale
                        ? Mathf.Lerp(1f, neighbourScale, offsetFromCenterAbs)
                        : neighbourScale;

                    cell.rectTransform.localScale = Vector3.one * scale;
                    float currentNeighbourWidthWorld =
                        cell.rectTransform.rect.width * cell.rectTransform.lossyScale.x;

                    posX =
                    (
                        // for example for center and left neighbour:
                        // right edge of the neighbour will align with left edge of the center
                        (centerWidthWorld + currentNeighbourWidthWorld) / 2 +

                        // in which layer (how far out) this cell is
                        currentNeighbourWidthWorld * (offsetFromCenterAbs - 1)

                        //
                        - (currentNeighbourWidthWorld * relativeOverlap *
                           // more overlap the further out we are
                           offsetFromCenterAbs * offsetFromCenterAbs)
                    ) * leftOrRight;

                    posY = 0;
                    posZ = positionStep *
                           // further back the further out we are
                           offsetFromCenterAbs * offsetFromCenterAbs;
                    rotY = -rotationStep * offsetFromCenter;
                }

                cell.rectTransform.localPosition = new Vector3(posX, posY, posZ);
                cell.rectTransform.localRotation = Quaternion.Euler(0, rotY, 0);

                // sibling order determines render order
                // center is considered the first layer; its neighbours the second layer, etc.
                // last sibling is rendered last; hence the last sibling is ultimately in front
                // TODO fix setting of sibling index
                int siblingIndex = Mathf.RoundToInt(cellLayers - offsetFromCenterAbs) - 1;
                cell.rectTransform.SetSiblingIndex(siblingIndex);

                if (offsetFromCenterAbs < 1)
                    Debug.Log($"{cellIndex}: {cellLayers}, {siblingIndex}");
            }
        }

        public void SetLayoutHorizontal()
        {
            UpdateLayout();
        }

        public void SetLayoutVertical()
        {
            // note that SetLayoutVertical is called AFTER SetLayoutHorizontal by the auto layout system
            Debug.Log("SetLayoutVertical");
        }

#endregion
    }
}
