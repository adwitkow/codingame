public class Barracks : Site
{
    public Barracks(Site site) : base(site)
    {
    }

    public int TrainingCooldown => Param1;

    public CreepType CreepType => (CreepType)Param2;
}