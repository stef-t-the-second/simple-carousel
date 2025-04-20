namespace Steft.SimpleCarousel.Layout
{
    public interface ICarouselCellLayoutHandler<in TCell>
    {
        void UpdateLayout(TCell cell);
    }
}
