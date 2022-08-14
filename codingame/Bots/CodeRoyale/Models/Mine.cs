public class Mine : Site
{
    public Mine(Site site) : base(site)
    {
    }

    public int IncomeRate => Param1;
}