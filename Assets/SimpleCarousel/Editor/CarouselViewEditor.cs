using UnityEditor;
using UnityEngine;

namespace Steft.SimpleCarousel
{
    [CustomEditor(typeof(CarouselView<>), true)]
    internal class CarouselViewEditor : Editor
    {
        private CarouselView m_CarouselView;

        private CarouselView carouselView
        {
            get
            {
                if (m_CarouselView == null)
                    m_CarouselView = target as CarouselView;

                return m_CarouselView;
            }
        }

        private void DestroyChildren(Transform transform)
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            bool changed = EditorGUI.EndChangeCheck();

            if (Application.isPlaying)
                return;

            if (carouselView.transform.childCount == 0)
            {
                carouselView.RebuildCells();
                return;
            }

            if (changed)
            {
                serializedObject.ApplyModifiedProperties();
                DestroyChildren(carouselView.transform);
                carouselView.RebuildCells();
                return;
            }

            if (GUILayout.Button("Rebuild Cells"))
            {
                DestroyChildren(carouselView.transform);
                carouselView.RebuildCells();
            }
        }
    }
}
