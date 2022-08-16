public class Wanderer : EntityBase
{
    public int TimeTillSpawn => Param0;

    public WandererState State => (WandererState)Param1;

    public int TargetId => Param2;

    public bool HasTarget => Param2 != -1;
}