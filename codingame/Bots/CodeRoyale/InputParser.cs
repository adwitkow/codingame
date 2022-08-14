using System.Numerics;

public class InputParser
{
    public GameState ParseUpdate(GameState gameState)
    {
        var inputs = Console.ReadLine()!.Split(' ');
        int gold = int.Parse(inputs[0]);
        int touchedSiteId = int.Parse(inputs[1]); // -1 if none

        gameState.Gold = gold;

        UpdateSites(gameState);
        PopulateRosters(gameState);

        return gameState;
    }

    public GameState ParseInitialization()
    {
        var gameState = new GameState();
        int numSites = int.Parse(Console.ReadLine()!);

        for (int i = 0; i < numSites; i++)
        {
            var inputs = Console.ReadLine()!.Split(' ');
            int siteId = int.Parse(inputs[0]);
            int x = int.Parse(inputs[1]);
            int y = int.Parse(inputs[2]);
            int radius = int.Parse(inputs[3]);

            gameState.AllSites[siteId] = new Site()
            {
                Id = siteId,
                Position = new Vector2(x, y),
                Radius = radius
            };
        }

        return gameState;
    }

    private void UpdateSites(GameState gameState)
    {
        var sites = gameState.AllSites;
        for (int i = 0; i < sites.Count; i++)
        {
            var inputs = Console.ReadLine()!.Split(' ');
            int siteId = int.Parse(inputs[0]);
            int gold = int.Parse(inputs[1]);
            int maxMineSize = int.Parse(inputs[2]);
            StructureType structureType = (StructureType)int.Parse(inputs[3]);
            int owner = int.Parse(inputs[4]);
            int param1 = int.Parse(inputs[5]);
            int param2 = int.Parse(inputs[6]);

            var site = sites[siteId];
            if (site.Type != structureType)
            {
                Console.Error.WriteLine($"Site {site.Id} has different type, updating to {structureType}");
                switch (structureType)
                {
                    case StructureType.None:
                        site = new Site(site);
                        break;
                    case StructureType.Goldmine:
                        site = new Mine(site);
                        break;
                    case StructureType.Tower:
                        site = new Tower(site);
                        break;
                    case StructureType.Barracks:
                        site = new Barracks(site);
                        break;
                    default:
                        break; // exception?
                }

                site.Type = structureType;
            }

            site.Gold = gold;
            site.MaxMineSize = maxMineSize;
            site.Owner = (Owner)owner;
            site.Param1 = param1;
            site.Param2 = param2;

            gameState.AllSites[siteId] = site;
        }
    }

    private void PopulateRosters(GameState gameState)
    {
        gameState.FriendlyUnits.Clear();
        gameState.EnemyUnits.Clear();

        int numUnits = int.Parse(Console.ReadLine()!);
        for (int i = 0; i < numUnits; i++)
        {
            var inputs = Console.ReadLine()!.Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            int owner = int.Parse(inputs[2]);
            int unitType = int.Parse(inputs[3]);
            int health = int.Parse(inputs[4]);

            var unit = new Unit()
            {
                Position = new Vector2(x, y),
                Owner = (Owner)owner,
                CreepType = (CreepType)unitType,
                Health = health
            };

            gameState.AddUnit(unit);
        }
    }
}