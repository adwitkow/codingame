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
partial class Game
{
    private const int MapWidth = 1920;
    private const int MapHeight = 1000;
    private static Vector2 MapMiddle = new Vector2(MapWidth / 2, MapHeight / 2);

    static void Main(string[] args)
    {
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

            UpdateSites(inputs, numSites, sites);
            PopulateRosters(friendlyRoster, enemyRoster);

            var friendlyQueen = friendlyRoster.Queen;

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
            var closestBuildableSite = buildableSitesByDistance.FirstOrDefault(site => site.Owner != Owner.Friendly || (site is Mine mine && mine.IncomeRate < mine.MaxMineSize));

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
                buildingToBuild = "MINE";
            }

            var trainableSites = allFriendlyBarracks
                .Where(b => b.TrainingCooldown == 0);
            var orderedTrainableSites = OrderByDistance(trainableSites, enemyQueen);

            var knightsToQueenByDistance = OrderByDistance(enemyRoster.Knights, friendlyQueen);
            var closestEnemyKnight = knightsToQueenByDistance.FirstOrDefault();

            if (closestBuildableSite is null)
            {
                // move to the closest tower
                Console.WriteLine($"MOVE 0 0");
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