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

        [SerializeField] private float m_CurrentScrollPosition = 2f;

#region Unity Methods

#if UNITY_EDITOR

        private void OnValidate()
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
                    var currentChildren = transform.GetChildren();
                    m_CarouselCells = new SimpleCarouselCell[m_NumberDisplayedElements];

                    for (int i = 0; i < m_NumberDisplayedElements; i++)
                    {
                        // TODO is there any way to squash the "SendMessage" warning when instantiating a Prefab?
                        var prefabInstance =
                            (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(m_PrefabElement, transform);
                        var rectTransform = prefabInstance.transform as RectTransform;
                        rectTransform.ResetToMiddleCenter();

                        // prefabInstance.hideFlags       = HideFlags.NotEditable;
                        m_CarouselCells[i] = new SimpleCarouselCell(i, rectTransform);
                    }

                    // unfortunately destroying GameObjects in OnValidate is not straightforward
                    // https://discussions.unity.com/t/onvalidate-and-destroying-objects/544819
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        foreach (Transform child in currentChildren)
                        {
                            // sanity check to avoid destroying something we don't want destroyed
                            if (child != null && child.parent != transform)
                                continue;

                            DestroyImmediate(child.gameObject);
                        }
                    };
                }
            }
        }

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

        public void SetLayoutHorizontal()
        {
            if (transform.childCount == 0)
                return;

            float currentScrollPosition = 2;
            float relativeOverlap = 0.2f;
            float scale = 0.8f;
            float rotationStep = -20;
            float centerWidthWorld =
                ((RectTransform)m_PrefabElement.transform).rect.width * transform.lossyScale.x;
            float centerHeightWorld =
                ((RectTransform)m_PrefabElement.transform).rect.height * transform.lossyScale.y;
            float neighbourWidthWorld =
                ((RectTransform)m_PrefabElement.transform).rect.width * transform.lossyScale.x * scale;
            float neighbourHeightWorld =
                ((RectTransform)m_PrefabElement.transform).rect.height * transform.lossyScale.y * scale;

            int cellLayers = m_NumberDisplayedElements / 2 + 1;
            foreach (var cell in m_CarouselCells)
            {
                int cellIndex = cell.carouselIndex;
                int offsetFromCenter = Mathf.RoundToInt(cellIndex - currentScrollPosition);
                int offsetFromCenterAbs = Mathf.Abs(offsetFromCenter);
                float posX, posY, posZ, rotY;
                if (offsetFromCenter == 0)
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

                    cell.rectTransform.localScale = Vector3.one * 0.8f;
                    posX =
                    (
                        (centerWidthWorld + neighbourWidthWorld) / 2 +
                        neighbourWidthWorld                      * (offsetFromCenterAbs - 1)
                        - (neighbourWidthWorld * relativeOverlap *
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
                cell.rectTransform.SetSiblingIndex(cellLayers - offsetFromCenterAbs);
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
