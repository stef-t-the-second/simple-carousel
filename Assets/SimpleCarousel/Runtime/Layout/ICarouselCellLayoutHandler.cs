namespace Steft.SimpleCarousel.Layout
{
    public interface ICarouselCellLayoutHandler<in TCell> where TCell : ICarouselCell
    {
        void UpdateLayout(TCell cell);
    }
}
