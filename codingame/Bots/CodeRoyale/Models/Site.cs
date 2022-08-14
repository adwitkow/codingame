using System.Numerics;

public class Site : ILocatable
{
    public Site()
    {

    }

    public Site(Site site)
    {
        this.Id = site.Id;
        this.Position = site.Position;
        this.Radius = site.Radius;
        this.Type = site.Type;
        this.Owner = site.Owner;
        this.Param1 = site.Param1;
        this.Param2 = site.Param2;
    }

    public int Id { get; set; }

    public Vector2 Position { get; set; }

    public int Radius { get; set; }

    public int Gold { get; set; }

    public int MaxMineSize { get; set; }

    public StructureType Type { get; set; }

    public Owner Owner { get; set; }

    public int Param1 { protected get; set; }

    public int Param2 { protected get; set; }
}