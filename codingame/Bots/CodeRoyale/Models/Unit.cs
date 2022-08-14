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

    public override bool Equals(object? obj)
    {
        return obj is Unit unit && Equals(unit);
    }

    public bool Equals(Unit unit)
    {
        return this.Position.Equals(unit.Position)
            && this.Owner.Equals(unit.Owner)
            && this.CreepType.Equals(unit.CreepType)
            && this.Health.Equals(unit.Health);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Position, this.Owner, this.CreepType, this.Health);
    }
}