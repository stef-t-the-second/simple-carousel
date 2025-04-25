namespace Steft.SimpleCarousel
{
    /// <summary>
    ///     Defines the basic data contract for items that can be displayed in a carousel.
    /// </summary>
    public interface ICarouselData
    {
        /// <summary>
        ///     Gets the display name of the carousel item.
        ///     This name is used for identification and display purposes within the carousel.
        /// </summary>
        string name { get; }
    }
}
