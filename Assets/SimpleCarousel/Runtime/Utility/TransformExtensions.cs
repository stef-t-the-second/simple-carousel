using System;
using UnityEngine;

namespace Steft.SimpleCarousel
{
    /// <summary>
    ///     Provides extension methods for <see cref="Transform" />.
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        ///     Gets an array of all immediate children Transforms of the specified Transform.
        /// </summary>
        /// <param name="transform">The Transform component to get children from.</param>
        /// <returns>
        ///     An array of Transform components representing immediate children. Returns an empty array if there are no
        ///     children.
        /// </returns>
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
