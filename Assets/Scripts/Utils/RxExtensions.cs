using UniRx;

public static class RxExtensions
{
    public static IReadOnlyReactiveProperty<int> CountProperty<T>(this IReadOnlyReactiveCollection<T> collection)
        => collection.ObserveCountChanged().ToReactiveProperty(collection.Count);
}