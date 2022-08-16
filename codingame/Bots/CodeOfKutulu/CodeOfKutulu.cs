using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

public class CodeOfKutulu
{
    static void Main(string[] args)
    {
        string[] inputs;
        int width = int.Parse(Console.ReadLine());
        int height = int.Parse(Console.ReadLine());

        var spawnPoints = new List<Vector2>();
        var map = new MapTile[width, height];

        for (int y = 0; y < height; y++)
        {
            string line = Console.ReadLine();
            for (int x = 0; x < width; x++)
            {
                var chars = line.ToCharArray();
                var c = chars[x];

                var tile = c switch
                {
                    '#' => MapTile.Wall,
                    'w' => MapTile.WandererSpawn,
                    '.' => MapTile.Empty
                };

                map[x, y] = tile;

                if (tile == MapTile.WandererSpawn)
                {
                    spawnPoints.Add(new Vector2(x, y));
                }
            }
            Console.Error.WriteLine(line);
        }

        inputs = Console.ReadLine().Split(' ');
        int sanityLossLonely = int.Parse(inputs[0]); // how much sanity you lose every turn when alone, always 3 until wood 1
        int sanityLossGroup = int.Parse(inputs[1]); // how much sanity you lose every turn when near another player, always 1 until wood 1
        int wandererSpawnTime = int.Parse(inputs[2]); // how many turns the wanderer take to spawn, always 3 until wood 1
        int wandererLifeTime = int.Parse(inputs[3]); // how many turns the wanderer is on map after spawning, always 40 until wood 1

        var entities = new List<EntityBase>();

        // game loop
        while (true)
        {
            entities.Clear();

            int entityCount = int.Parse(Console.ReadLine()); // the first given entity corresponds to your explorer
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');

                string inputEntityType = inputs[0];
                int id = int.Parse(inputs[1]);
                int x = int.Parse(inputs[2]);
                int y = int.Parse(inputs[3]);
                int param0 = int.Parse(inputs[4]);
                int param1 = int.Parse(inputs[5]);
                int param2 = int.Parse(inputs[6]);

                EntityBase entity;
                if (inputEntityType.Equals("EXPLORER"))
                {
                    entity = new Explorer()
                    {
                        EntityType = EntityType.Explorer
                    };
                }
                else
                {
                    entity = new Wanderer()
                    {
                        EntityType = EntityType.Wanderer
                    };
                }

                entity.Id = id;
                entity.Position = new Vector2(x, y);
                entity.Param0 = param0;
                entity.Param1 = param1;
                entity.Param2 = param2;

                entities.Add(entity);
            }

            var allExplorers = entities.OfType<Explorer>();
            var myExplorer = allExplorers.First();
            var otherExplorers = allExplorers.Skip(1);

            var spawnMap = new Dictionary<Explorer, float>();
            foreach (var explorer in allExplorers)
            {
                var distanceToClosestSpawn = spawnPoints.Min(point => Vector2.DistanceSquared(point, explorer.Position));
                spawnMap[explorer] = distanceToClosestSpawn;
            }

            var firstOtherExplorer = otherExplorers.FirstOrDefault();

            Vector2 targetCandidate;
            if (firstOtherExplorer != null)
            {
                targetCandidate = firstOtherExplorer.Position;
            }
            else
            {
                targetCandidate = myExplorer.Position;
            }

            if (spawnMap[myExplorer] == spawnMap.Min(pair => pair.Value))
            {
                var currentClosestSpawnPosition = spawnPoints.OrderBy(point => Vector2.DistanceSquared(point, myExplorer.Position)).First();
                targetCandidate = MoveAwayFromPoint(currentClosestSpawnPosition, map, myExplorer, spawnMap[myExplorer]);
            }

            var wanderers = entities.OfType<Wanderer>();
            var targettingWanderer = wanderers.FirstOrDefault(w => w.HasTarget && w.TargetId == myExplorer.Id);
            if (targettingWanderer != null)
            {
                targetCandidate = MoveAwayFromPoint(targettingWanderer.Position, map, myExplorer, Vector2.DistanceSquared(targettingWanderer.Position, myExplorer.Position));
            }

            Console.Error.WriteLine($"Target tile: [{(int)targetCandidate.X}, {(int)targetCandidate.Y}] ({map[(int)targetCandidate.X, (int)targetCandidate.Y]})");

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            var targetPosition = targetCandidate;

            Console.WriteLine($"MOVE {targetPosition.X} {targetPosition.Y}"); // MOVE <x> <y> | WAIT

        }
    }

    private static Vector2 MoveAwayFromPoint(Vector2 point, MapTile[,] map, Explorer myExplorer, float initialDistanceSquared)
    {
        Vector2 targetCandidate;
        
        var targets = new[]
        {
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(-1, 0),
            new Vector2(0, -1),
        };

        var bestDistance = initialDistanceSquared;
        var bestVector = Vector2.Zero;

        foreach (var target in targets)
        {
            targetCandidate = Vector2.Add(myExplorer.Position, target);

            Console.Error.WriteLine($"Trying out vector {targetCandidate}");

            var targetMapTile = map[(int)targetCandidate.X, (int)targetCandidate.Y];
            var targetDistance = Vector2.DistanceSquared(targetCandidate, point);

            if (targetMapTile != MapTile.Wall && targetDistance > bestDistance)
            {
                bestDistance = targetDistance;
                bestVector = target;

                Console.Error.WriteLine($"{targetCandidate} is better than anything before it");
            }
            else
            {
                Console.Error.WriteLine($"{targetCandidate} failed");
            }
        }

        Console.Error.WriteLine($"{bestVector} has won");

        return Vector2.Add(myExplorer.Position, bestVector);
    }
}