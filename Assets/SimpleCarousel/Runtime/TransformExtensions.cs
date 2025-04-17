using System;
using UnityEngine;

namespace Steft.SimpleCarousel
{
    public static class TransformExtensions
    {
        public static Transform[] GetChildren(this Transform transform)
        {
            if (transform.childCount == 0)
                return Array.Empty<Transform>();

            var children = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                children[i] = transform.GetChild(i);
            }

            return children;
        }
    }
}
