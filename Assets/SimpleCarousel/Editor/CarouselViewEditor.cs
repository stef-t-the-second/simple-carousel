using UnityEditor;
using UnityEngine;

namespace Steft.SimpleCarousel
{
    /// <summary>
    ///     Custom editor for the <see cref="CarouselView{TData}" /> component.
    ///     Includes automatic cell rebuilding when properties change in the editor and
    ///     a manual rebuild button.
    /// </summary>
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

        /// <summary>
        ///     Destroys all immediate child GameObjects of the given transform.
        /// </summary>
        /// <param name="transform">The parent transform whose children will be destroyed.</param>
        private void DestroyChildren(Transform transform)
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        public override void OnInspectorGUI()
        {
            if (!Application.isPlaying)
            {
                if (GUILayout.Button("Preview View"))
                {
                    DestroyChildren(carouselView.transform);
                    carouselView.RebuildView(true);
                }

                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            }

            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            bool changed = EditorGUI.EndChangeCheck();

            if (!Application.isPlaying && changed)
            {
                DestroyChildren(carouselView.transform);
                carouselView.RebuildView(true);
            }
        }
    }
}
