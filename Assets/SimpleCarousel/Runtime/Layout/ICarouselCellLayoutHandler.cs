namespace Steft.SimpleCarousel.Layout
{
    /// <summary>
    /// Defines an interface for handling the layout of carousel cells within a carousel view.
    /// Implementations of this interface are responsible for positioning and arranging cells.
    /// </summary>
    public interface ICarouselCellLayoutHandler
    {
        /// <summary>
        /// Updates the layout of a specific carousel cell.
        /// </summary>
        /// <param name="cell">The carousel cell to update the layout for.</param>
        void UpdateLayout(ICarouselCell cell);
    }
}
