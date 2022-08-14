using System.Numerics;

public struct Unit : ILocatable
{
    public Vector2 Position { get; set; }

    public Owner Owner { get; set; }

    public CreepType CreepType { get; set; }

    public int Health { get; set; }

    public static bool operator ==(Unit a, Unit b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Unit a, Unit b)
    {
        return !a.Equals(b);
    }
}