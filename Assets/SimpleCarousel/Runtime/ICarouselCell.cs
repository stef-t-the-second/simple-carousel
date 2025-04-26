using UnityEngine.Events;

namespace Steft.SimpleCarousel
{
    /// <summary>
    ///     Represents a cell in a carousel view that can be positioned and identified.
    /// </summary>
    public interface ICarouselCell : IRectTransform
    {
        /// <summary>
        ///     Gets or sets the offset of the cell from the center position of the carousel.
        ///     Negative values indicate positions to the left, positive values to the right.
        /// </summary>
        float offsetFromCenter { get; }

        /// <summary>
        ///     Gets the absolute value of the offset from center, representing the distance
        ///     regardless of direction.
        /// </summary>
        float offsetFromCenterAbs { get; }

        /// <summary>
        ///     Gets or sets the index position of the cell within the carousel's data collection.
        /// </summary>
        int index { get; }
    }

    /// <summary>
    ///     Generic interface for carousel cells that contain typed data.
    /// </summary>
    /// <typeparam name="TData">The type of data contained in the cell.</typeparam>
    public interface ICarouselCell<out TData> : ICarouselCell where TData : class, ICarouselData
    {
        /// <summary>
        ///     The UnityEvent that is triggered when the cell is clicked.
        /// </summary>
        UnityEvent<ICarouselCell<ICarouselData>> onClicked { get; }

        /// <summary>
        ///     Gets the data associated with this carousel cell.
        /// </summary>
        TData data { get; }
    }
}
