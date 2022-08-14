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
public class Game
{
    private const int MapWidth = 1920;
    private const int MapHeight = 1000;
    private static Vector2 MapMiddle = new Vector2(MapWidth / 2, MapHeight / 2);

    static void Main(string[] args)
    {
        var parser = new InputParser();
        var gameState = parser.ParseInitialization();

        // game loop
        while (true)
        {
            gameState = parser.ParseUpdate(gameState);

            var friendlyQueen = gameState.FriendlyQueen;
            var enemyQueen = gameState.EnemyQueen;

            var buildableSitesByDistance = gameState.AllSites.Values.OrderByDistance(friendlyQueen)
                .Where(site => !(site.Type == StructureType.Tower && site.Owner == Owner.Enemy));

            Console.Error.WriteLine("Included towers: " + buildableSitesByDistance.OfType<Tower>().Count());

            Console.Error.WriteLine($"Closest site: {buildableSitesByDistance.FirstOrDefault().Id}");

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            // First line: A valid queen action
            // Second line: A set of training instructions

            var allFriendlyBarracks = gameState.AllSites.Values.OfType<Barracks>()
                .Where(b => b.Owner == Owner.Friendly);
            var shouldBuildKnightBarracks = !allFriendlyBarracks.Any(b => b.CreepType == CreepType.Knight);
            var shouldBuildArcherBarracks = !allFriendlyBarracks.Any(b => b.CreepType == CreepType.Archer);
            var closestBuildableSite = buildableSitesByDistance.FirstOrDefault(site => site.Owner != Owner.Friendly || (site is Mine mine && mine.IncomeRate < mine.MaxMineSize && mine.Gold > 0) || (site is Tower tower && tower.AttackRadius < 350));

            string buildingToBuild;
            if (shouldBuildKnightBarracks)
            {
                buildingToBuild = "BARRACKS-KNIGHT";
            }
            else
            {
                buildingToBuild = "MINE";
            }

            var trainableSites = allFriendlyBarracks
                .Where(b => b.TrainingCooldown == 0);
            var orderedTrainableSites = trainableSites.OrderByDistance(enemyQueen);

            var knightsToQueenByDistance = gameState.EnemyUnits.Knights.OrderByDistance(friendlyQueen);
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
                Console.Error.WriteLine($"Building mine on site {closestBuildableSite.Id} (max level: {closestBuildableSite.MaxMineSize})");
                Console.WriteLine($"BUILD {closestBuildableSite.Id} {buildingToBuild}");
            }

            var archerBarracks = orderedTrainableSites.Where(site => site.CreepType == CreepType.Archer);
            var knightBarracks = orderedTrainableSites.Where(site => site.CreepType == CreepType.Knight);
            if (gameState.EnemyUnits.Knights.Count / 2 > gameState.FriendlyUnits.Archers.Count && archerBarracks.Any())
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
}