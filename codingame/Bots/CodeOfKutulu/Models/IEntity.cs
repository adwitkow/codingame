using System.Numerics;

public abstract class EntityBase
{
    public int Id { get; set; }

    public EntityType EntityType { get; set; }

    public Vector2 Position { get; set; }

    public int Param0 { get; set; }

    public int Param1 { get; set; }

    public int Param2 { get; set; }
}