using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Steft.SimpleCarousel
{
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public class SimpleCarouselView : MonoBehaviour,
        IBeginDragHandler, IEndDragHandler, IDragHandler,
        ILayoutGroup
    {
        [SerializeField] private GameObject[] m_PrefabElements;

        private GameObject[] m_PrefabElementInstances;

        public IReadOnlyList<GameObject> prefabElements => m_PrefabElements;

        public IReadOnlyList<GameObject> prefabElementInstances => m_PrefabElementInstances;

#region Unity Methods

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (m_PrefabElements != null)
            {
                Debug.Log("OnValidate");

                // capturing current children before we add new ones
                var currentChildren = new List<Transform>(m_PrefabElementInstances.Length);
                foreach (Transform child in transform)
                {
                    currentChildren.Add(child);
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

                m_PrefabElementInstances = new GameObject[m_PrefabElements.Length];
                for (int i = 0; i < m_PrefabElements.Length; i++)
                {
                    var element = m_PrefabElements[i];
                    if (element == null)
                        continue;

                    // is it a GameObject reference of an instance in the scene?
                    // is it a Prefab reference?
                    if (element.scene.IsValid())
                    {
                        Debug.LogWarning(
                            $"Skipping. Prefab reference required, but was GameObject instance: {element.name}");
                        continue;
                    }

                    // TODO is there any way to squash the "SendMessage" warning when instantiating a Prefab?
                    var prefabInstance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(element, transform);
                    prefabInstance.hideFlags    = HideFlags.NotEditable;
                    m_PrefabElementInstances[i] = prefabInstance;
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
            // note that SetLayoutHorizontal is called BEFORE SetLayoutVertical by the auto layout system
            Debug.Log("SetLayoutHorizontal");
        }

        public void SetLayoutVertical()
        {
            // note that SetLayoutVertical is called AFTER SetLayoutHorizontal by the auto layout system
            Debug.Log("SetLayoutVertical");
        }

#endregion
    }
}
