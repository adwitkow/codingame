public class Tower : Site
{
    public Tower(Site site) : base(site)
    {
    }

    public int Health => Param1;

    public int AttackRadius => Param2;
}