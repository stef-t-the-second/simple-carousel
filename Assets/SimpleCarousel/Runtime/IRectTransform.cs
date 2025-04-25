using UnityEngine;

namespace Steft.SimpleCarousel
{
    /// <summary>
    ///     Interface for components that provide access to a <see cref="RectTransform" />.
    /// </summary>
    public interface IRectTransform
    {
        /// <summary>
        ///     Gets the <see cref="RectTransform" /> component associated with this object.
        /// </summary>
        RectTransform rectTransform { get; }
    }
}
