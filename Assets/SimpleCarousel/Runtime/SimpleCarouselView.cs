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

        private RectTransform[] m_DisplayedElementInstances;

        public IReadOnlyList<RectTransform> prefabElementInstances => m_DisplayedElementInstances;

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

                if (transform.childCount               != m_NumberDisplayedElements ||
                    m_DisplayedElementInstances.Length != m_NumberDisplayedElements)
                {
                    var currentChildren = transform.GetChildren();
                    m_DisplayedElementInstances = new RectTransform[m_NumberDisplayedElements];

                    for (int i = 0; i < m_NumberDisplayedElements; i++)
                    {
                        // TODO is there any way to squash the "SendMessage" warning when instantiating a Prefab?
                        var prefabInstance =
                            (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(m_PrefabElement, transform);
                        var rectTransform = prefabInstance.transform as RectTransform;
                        rectTransform.ResetToMiddleCenter();

                        // prefabInstance.hideFlags       = HideFlags.NotEditable;
                        m_DisplayedElementInstances[i] = rectTransform;
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

            for (int i = 0; i < m_DisplayedElementInstances.Length; i++)
            {
                var rectTransform = m_DisplayedElementInstances[i].transform as RectTransform;
                // sanity check that should under no circumstances trigger,
                // because we are dealing with UI elements that all come with a RectTransform
                if (rectTransform == null)
                {
                    Debug.LogError("Displayed GameObject has no RectTransform attached.");
                    return;
                }

                int offsetFromCenter = Mathf.RoundToInt(i - currentScrollPosition);

                float posX, posY, rotY;
                if (offsetFromCenter == 0)
                {
                    rectTransform.localScale = Vector3.one;

                    posX = 0;
                    posY = 0;
                    rotY = 0;
                }
                else
                {
                    int leftOrRight = offsetFromCenter > 0 ? 1 : -1;
                    int offsetFromCenterAbs = Mathf.Abs(offsetFromCenter);

                    rectTransform.localScale = Vector3.one * 0.8f;
                    posX =
                    (
                        (centerWidthWorld + neighbourWidthWorld) / 2 +
                        neighbourWidthWorld                      * (offsetFromCenterAbs - 1)
                        - (neighbourWidthWorld * relativeOverlap *
                           //more overlap the further out we are
                           offsetFromCenterAbs * offsetFromCenterAbs)
                    ) * leftOrRight;

                    posY = 0;
                    rotY = rotationStep * offsetFromCenter;
                }

                rectTransform.anchoredPosition = new Vector2(posX, posY);
                rectTransform.localRotation    = Quaternion.Euler(0, rotY, 0);
            }
        }

        public void SetLayoutVertical()
        {
            // note that SetLayoutVertical is called AFTER SetLayoutHorizontal by the auto layout system
            Debug.Log("SetLayoutVertical");
        }

#endregion
    }
}
