using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    private const int MapWidth = 1920;
    private const int MapHeight = 1000;

    static void Main(string[] args)
    {
        var startX = -1;
        var startY = -1;

        string[] inputs;
        int numSites = int.Parse(Console.ReadLine());

        var baseSites = new List<Site>();
        for (int i = 0; i < numSites; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int siteId = int.Parse(inputs[0]);
            int x = int.Parse(inputs[1]);
            int y = int.Parse(inputs[2]);
            int radius = int.Parse(inputs[3]);

            baseSites.Add(new Site()
            {
                Id = siteId,
                Position = new Vector2(x, y),
                Radius = radius
            });
        }

        var highestId = baseSites.Max(site => site.Id);
        var sites = new Site[highestId + 1]; // hack

        foreach (var site in baseSites)
        {
            sites[site.Id] = site;
        }

        var friendlyRoster = new Roster(Owner.Friendly);
        var enemyRoster = new Roster(Owner.Enemy);

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int gold = int.Parse(inputs[0]);
            int touchedSiteId = int.Parse(inputs[1]); // -1 if none

            Site touchedSite;
            if (touchedSiteId == -1)
            {
                touchedSite = null;
            }
            else
            {
                touchedSite = sites[touchedSiteId];
            }
            UpdateSites(inputs, numSites, sites);
            PopulateRosters(friendlyRoster, enemyRoster);

            var friendlyQueen = friendlyRoster.Queen;

            if (startX == -1 && startY == -1)
            {
                if (friendlyQueen.Position.X > MapWidth / 2)
                {
                    startX = MapWidth;
                }
                else
                {
                    startX = 0;
                }

                if (friendlyQueen.Position.Y > MapHeight / 2)
                {
                    startY = MapHeight;
                }
                else
                {
                    startY = 0;
                }
            }

            var enemyQueen = enemyRoster.Queen;
            var buildableSitesByDistance = OrderByDistance(sites, friendlyQueen)
                .Where(site => site.Type != StructureType.Tower && site.Owner != Owner.Enemy);

            Console.Error.WriteLine($"Closest site: {buildableSitesByDistance.FirstOrDefault().Id}");

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            // First line: A valid queen action
            // Second line: A set of training instructions

            var allFriendlyBarracks = sites.OfType<Barracks>()
                .Where(b => b.Owner == Owner.Friendly);
            var shouldBuildKnightBarracks = !allFriendlyBarracks.Any(b => b.CreepType == CreepType.Knight);
            var shouldBuildArcherBarracks = !allFriendlyBarracks.Any(b => b.CreepType == CreepType.Archer);

            string buildingToBuild;
            if (shouldBuildArcherBarracks)
            {
                buildingToBuild = "BARRACKS-ARCHER";
            }
            else if (shouldBuildKnightBarracks)
            {
                buildingToBuild = "BARRACKS-KNIGHT";
            }
            else
            {
                buildingToBuild = "TOWER";
            }

            var closestBuildableSite = buildableSitesByDistance.FirstOrDefault(site => site.Owner != Owner.Friendly);
            var trainableSites = allFriendlyBarracks
                .Where(b => b.TrainingCooldown == 0);
            var orderedTrainableSites = OrderByDistance(trainableSites, enemyQueen);

            var knightsToQueenByDistance = OrderByDistance(enemyRoster.Knights, friendlyQueen);
            var closestEnemyKnight = knightsToQueenByDistance.FirstOrDefault();

            if (closestBuildableSite is null)
            {
                Console.WriteLine($"MOVE {startX} {startY}");
            }
            else if (closestEnemyKnight != default && Vector2.Distance(closestEnemyKnight.Position, friendlyQueen.Position) < 500)
            {
                Console.WriteLine($"BUILD {closestBuildableSite.Id} TOWER");
            }
            else
            {
                Console.WriteLine($"BUILD {closestBuildableSite.Id} {buildingToBuild}");
            }

            var archerBarracks = orderedTrainableSites.Where(site => site.CreepType == CreepType.Archer);
            var knightBarracks = orderedTrainableSites.Where(site => site.CreepType == CreepType.Knight);
            if (enemyRoster.Knights.Count / 2 > friendlyRoster.Archers.Count && archerBarracks.Any())
            {
                Console.WriteLine($"TRAIN {archerBarracks.First().Id}");
            }
            else if (knightBarracks.Any())
            {
                Console.WriteLine($"TRAIN {knightBarracks.First().Id}");
            }
            else
            {
                Console.WriteLine("TRAIN");
            }
        }
    }

    private static string[] UpdateSites(string[] inputs, int numSites, Site[] sites)
    {
        for (int i = 0; i < numSites; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int siteId = int.Parse(inputs[0]);
            int ignore1 = int.Parse(inputs[1]); // used in future leagues
            int ignore2 = int.Parse(inputs[2]); // used in future leagues
            StructureType structureType = (StructureType)int.Parse(inputs[3]); // -1 = No structure, 2 = Barracks
            int owner = int.Parse(inputs[4]); // -1 = No structure, 0 = Friendly, 1 = Enemy
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
                    case StructureType.Barracks:
                        site = new Barracks(site);
                        break;
                    default:
                        break; // exception?
                }

                site.Type = structureType;
            }

            site.Owner = (Owner)owner;
            site.Param1 = param1;
            site.Param2 = param2;

            sites[siteId] = site;
        }

        return inputs;
    }

    private static void PopulateRosters(Roster friendlyRoster, Roster enemyRoster)
    {
        friendlyRoster.Clear();
        enemyRoster.Clear();

        int numUnits = int.Parse(Console.ReadLine());
        for (int i = 0; i < numUnits; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            int owner = int.Parse(inputs[2]);
            int unitType = int.Parse(inputs[3]); // -1 = QUEEN, 0 = KNIGHT, 1 = ARCHER
            int health = int.Parse(inputs[4]);

            var unit = new Unit()
            {
                Position = new Vector2(x, y),
                Owner = (Owner)owner,
                CreepType = (CreepType)unitType,
                Health = health
            };

            Roster roster;
            if (unit.Owner == Owner.Friendly)
            {
                roster = friendlyRoster;
            }
            else
            {
                roster = enemyRoster;
            }

            switch (unit.CreepType)
            {
                case CreepType.Queen:
                    roster.Queen = unit;
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

    private class Site : ILocatable
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

        public StructureType Type { get; set; }

        public Owner Owner { get; set; }

        public int Param1 { get; set; }

        public int Param2 { get; set; }
    }

    private class Barracks : Site
    {
        public Barracks(Site site) : base(site)
        {
        }

        public int TrainingCooldown => Param1;

        public CreepType CreepType => (CreepType)Param2;
    }

    private struct Unit : ILocatable
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

    private class Roster
    {
        public Roster(Owner owner)
        {
            this.Owner = owner;
        }

        public Owner Owner { get; }

        public Unit Queen { get; set; }

        public List<Unit> Knights { get; set; } = new List<Unit>();

        public List<Unit> Archers { get; set; } = new List<Unit>();

        internal void Clear()
        {
            Knights.Clear();
            Archers.Clear();
        }
    }

    private enum StructureType
    {
        None = -1,
        Goldmine = 0,
        Tower = 1,
        Barracks = 2
    }

    private enum Owner
    {
        None = -1,
        Friendly = 0,
        Enemy = 1
    }

    private enum CreepType
    {
        Queen = -1,
        Knight = 0,
        Archer = 1
    }

    private interface ILocatable
    {
        Vector2 Position { get; set; }
    }

    public static class Costs
    {
        public const int Knight = 80;

        public const int Archer = 100;
    }

    private static IOrderedEnumerable<T> OrderByDistance<T>(IEnumerable<T> locatables, ILocatable locatable)
        where T : ILocatable
    {
        return OrderByDistance(locatables, locatable.Position);
    }

    private static IOrderedEnumerable<T> OrderByDistance<T>(IEnumerable<T> locatables, Vector2 position)
        where T : ILocatable
    {
        return locatables.OrderBy(loc => Vector2.DistanceSquared(loc.Position, position));
    }
}