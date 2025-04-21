namespace Steft.SimpleCarousel
{
    public interface ICarouselCell : IRectTransform
    {
        float offsetFromCenter { get; set; }

        float offsetFromCenterAbs { get; }
    }

    public interface ICarouselCell<out TData> : ICarouselCell where TData : class, ICarouselData
    {
        TData data { get; }
    }
}
