public class GameState
{
    public int Gold { get; set; }

    public Dictionary<int, Site> AllSites { get; set; } = new Dictionary<int, Site>();

    public IEnumerable<Site> FriendlySites => AllSites
        .Where(pair => pair.Value.Owner == Owner.Friendly)
        .Select(pair => pair.Value);

    public IEnumerable<Site> EnemySites => AllSites
        .Where(pair => pair.Value.Owner == Owner.Enemy)
        .Select(pair => pair.Value);

    public IEnumerable<Site> NeutralSites => AllSites
        .Where(pair => pair.Value.Owner == Owner.None)
        .Select(pair => pair.Value);

    public Roster FriendlyUnits { get; set; } = new Roster(Owner.Friendly);

    public Roster EnemyUnits { get; set; } = new Roster(Owner.Enemy);

    public Unit FriendlyQueen { get; set; }

    public Unit EnemyQueen { get; set; }

    public void AddUnit(Unit unit)
    {
        var roster = unit.Owner == Owner.Friendly ? FriendlyUnits : EnemyUnits;

        switch (unit.CreepType)
        {
            case CreepType.Queen:
                if (unit.Owner == Owner.Friendly)
                {
                    FriendlyQueen = unit;
                }
                else
                {
                    EnemyQueen = unit;
                }
                break;
            case CreepType.Knight:
                roster.Knights.Add(unit);
                break;
            case CreepType.Archer:
                roster.Archers.Add(unit);
                break;
        }
    }
}