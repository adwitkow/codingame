using System.Numerics;

public static class Extensions
{
    public static IOrderedEnumerable<T> OrderByDistance<T>(this IEnumerable<T> locatables, ILocatable locatable)
        where T : ILocatable
    {
        return OrderByDistance(locatables, locatable.Position);
    }

    public static IOrderedEnumerable<T> OrderByDistance<T>(this IEnumerable<T> locatables, Vector2 position)
        where T : ILocatable
    {
        return locatables.OrderBy(loc => Vector2.DistanceSquared(loc.Position, position));
    }
}